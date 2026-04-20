using GameSync;
using Google.Protobuf;
using Island;
using Lop.Survivor;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

//test
public class LOPNetworkManager : MonoBehaviour
{
    public static LOPNetworkManager Instance;

    [Header("Server Settings")]
    public string serverIp;
    public int serverPort;

    [Header("Prefabs")]
    public GameObject playerPrefab;

    [Header("Dependencies")]
    [SerializeField] private NetworkPrefabRegister prefabRegister;

    [Header("Spawn Settings")]
    public Transform[] spawnPoints;

    [Header("Chat Settings")]
    public ChatHandler chatHandler;

    private string playerId;
    public string playerName;
    public bool IsWorldSpawner;

    public bool isConnected = false;

    private LopRpcSystem rpcSystem;

    private UdpClient udp;
    private IPEndPoint serverEp;
    private Thread recvThread;

    private ConcurrentQueue<byte[]> receivedMessageQueue = new ConcurrentQueue<byte[]>();
    private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

    private Dictionary<int, GameObject> networkObjects = new Dictionary<int, GameObject>();

    private Dictionary<int, Action<GameObject>> pendingSpawnCallbacks = new Dictionary<int, Action<GameObject>>();

    public Action<bool> OnConnecting;
    public Action OnConnectionFailed;

    private int nextCallbackKey = 1;

    [SerializeField] private float joinTimeoutSeconds = 5f;

    private bool isConnecting = false;
    private bool isWorldSceneLoaded = false;
    private bool isLoadingWorldScene = false;
    private bool hasReceivedLocalJoin = false;

    private Coroutine joinTimeoutCoroutine;

    private readonly Queue<GameMessage> pendingMessagesBeforeSceneLoad = new Queue<GameMessage>();
    private GameMessage pendingLocalJoinMessage;
    private bool hasPendingLocalJoinMessage = false;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(spawnPoints[0]);
        }
        else
        {
            NetworkDestroy(gameObject);
            return;
        }

        rpcSystem = new LopRpcSystem();

        rpcSystem.SetObjectResolver(GetNetworkObject);
        rpcSystem.RegisterRpcComponent(this);

        // NetworkPrefabRegister 초기화 및 유효성 검사
        if (prefabRegister == null)
        {
            Debug.LogError("[LOPNetworkManager] NetworkPrefabRegister is NULL! Assign the Register component.");
        }
        else
        {
            prefabRegister.Initialize();
        }

        Application.runInBackground = true;
    }

    public GameObject GetNetworkObject(int networkId)
    {
        // players 딕셔너리도 필요하다면 여기서 통합하여 검색해야 합니다. (현재는 networkObjects만 검색)
        networkObjects.TryGetValue(networkId, out var obj);
        return obj;
    }

    private void Start()
    {
        //Connect();

        if (chatHandler == null)
            chatHandler = FindFirstObjectByType<ChatHandler>();
        else { }
    }

    private void Update()
    {
        while (receivedMessageQueue.TryDequeue(out var data))
        {
            OnDataReceived(data);
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("[LOPNetworkManager] Application is quitting. Sending Leave message...");
        Disconnect(); 
    }

    // -------------------- 서버 연결 --------------------
    public void Connect(string ipInput, string port, string name)
    {
        if (!IPAddress.TryParse(ipInput, out IPAddress parsedIp))
        {
            Debug.LogWarning("잘못된 IP 입력 값");
            OnConnectionFailed?.Invoke();
            return;
        }

        if (!int.TryParse(port, out int parsedPort) || parsedPort <= 0 || parsedPort > 65535)
        {
            Debug.LogWarning("존재하지 않는 포트 번호");
            OnConnectionFailed?.Invoke();
            return;
        }

        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("닉넴 없음");
            OnConnectionFailed?.Invoke();
            return;
        }

        if (udp != null)
        {
            Disconnect();
        }

        try
        {
            ResetConnectionStateForNewAttempt();

            serverEp = new IPEndPoint(parsedIp, parsedPort);
            udp = new UdpClient();
            udp.Connect(serverEp);

            recvThread = new Thread(ReceiveLoop) { IsBackground = true };
            recvThread.Start();

            isConnected = true;
            isConnecting = true;
            playerName = name;
            playerId = string.Empty;

            OnConnecting?.Invoke(true);

            SendJoinRequest();
            StartJoinTimeout();

            Debug.Log("[LOPNetworkManager] Join 요청 전송. 서버 응답 대기 시작.");
        }
        catch (SocketException se)
        {
            Debug.LogError($"[소켓 연결 실패 : {se.SocketErrorCode}] {se.Message}");
            FailConnection();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[연결 오류] {ex}");
            FailConnection();
        }
    }

    public void Disconnect()
    {
        InternalDisconnect(true);
    }

    private void ReceiveLoop()
    {
        try
        {
            while (udp != null && isConnected)
            {
                try
                {
                    var receiveBytes = udp.Receive(ref serverEp);
                    receivedMessageQueue.Enqueue(receiveBytes);
                }
                catch (SocketException e)
                {
                    if (!isConnected || udp == null)
                        break;

                    if (e.SocketErrorCode != SocketError.Interrupted &&
                        e.SocketErrorCode != SocketError.NotSocket &&
                        e.SocketErrorCode != SocketError.OperationAborted)
                    {
                        Debug.LogWarning($"[LOPNetworkManager] SocketException: {e.Message}");
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LOPNetworkManager] ReceiveLoop 종료: {e}");
        }
    }


    // -------------------- 메시지 처리 --------------------
    private void OnDataReceived(byte[] data)
    {
        try
        {
            var msg = GameMessage.Parser.ParseFrom(data);

            if (TryHandleConnectionBootstrap(msg)) // 추가됨
                return;

            if (ShouldBufferMessageUntilSceneLoaded()) // 추가됨
            {
                pendingMessagesBeforeSceneLoad.Enqueue(msg);
                return;
            }

            DispatchMessage(msg); // 추가됨
        }
        catch (Google.Protobuf.InvalidProtocolBufferException ex)
        {
            Debug.LogError($"[Protobuf 파싱 오류] 원본 데이터 크기: {data.Length}. 서버/클라이언트 gamesync.proto 일치 여부를 확인하세요. 오류 메시지: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[메시지 처리 오류] 수신 패킷 처리 중 예외 발생.\n원본 오류(ToString): {ex}");
        }
    }

    private void SendJoinRequest()
    {
        var joinMsg = new GameMessage
        {
            Join = new Join
            {
                Nickname = playerName
            }
        };

        SendRaw(joinMsg);
    }

    private void StartJoinTimeout()
    {
        StopJoinTimeout();

        joinTimeoutCoroutine = StartCoroutine(JoinTimeoutRoutine());
    }

    private void StopJoinTimeout()
    {
        if (joinTimeoutCoroutine != null)
        {
            StopCoroutine(joinTimeoutCoroutine);
            joinTimeoutCoroutine = null;
        }
    }

    private IEnumerator JoinTimeoutRoutine()
    {
        yield return new WaitForSeconds(joinTimeoutSeconds);

        if (!hasReceivedLocalJoin)
        {
            Debug.LogWarning("[LOPNetworkManager] Join 응답 시간 초과");
            FailConnection();
        }
    }

    private bool TryHandleConnectionBootstrap(GameMessage msg)
    {
        if (!isConnecting)
            return false;

        if (msg.PayloadCase != GameMessage.PayloadOneofCase.Join)
            return false;

        bool looksLikeLocalJoin = !hasReceivedLocalJoin &&
                                  string.IsNullOrEmpty(playerId) &&
                                  msg.Join != null &&
                                  msg.Join.Nickname == playerName;

        if (!looksLikeLocalJoin)
        {
            pendingMessagesBeforeSceneLoad.Enqueue(msg);
            return true;
        }

        hasReceivedLocalJoin = true;
        hasPendingLocalJoinMessage = true;
        pendingLocalJoinMessage = msg;

        StopJoinTimeout();
        BeginEnterWorldScene();

        return true;
    }

    private void BeginEnterWorldScene()
    {
        if (isLoadingWorldScene || isWorldSceneLoaded)
            return;

        isLoadingWorldScene = true;
        SceneManager.sceneLoaded += OnMultiTestSceneLoaded;
        SceneManager.LoadScene("MultiTest");

        Debug.Log("[LOPNetworkManager] 서버 Join 응답 확인. MultiTest 씬 로드 시작.");
    }

    private bool ShouldBufferMessageUntilSceneLoaded()
    {
        return !isWorldSceneLoaded;
    }

    private void FlushBufferedMessagesAfterSceneLoad()
    {
        if (hasPendingLocalJoinMessage)
        {
            DispatchMessage(pendingLocalJoinMessage);
            pendingLocalJoinMessage = null;
            hasPendingLocalJoinMessage = false;
        }

        while (pendingMessagesBeforeSceneLoad.Count > 0)
        {
            DispatchMessage(pendingMessagesBeforeSceneLoad.Dequeue());
        }
    }

    private void DispatchMessage(GameMessage msg)
    {
        switch (msg.PayloadCase)
        {
            case GameMessage.PayloadOneofCase.Join:
                HandleJoinMessage(msg);
                break;
            case GameMessage.PayloadOneofCase.Leave:
                HandleLeaveMessage(msg);
                break;
            case GameMessage.PayloadOneofCase.Position:
                HandlePositionMessage(msg);
                break;
            case GameMessage.PayloadOneofCase.AnimState:
                HandleAnimationStateMessage(msg);
                break;
            case GameMessage.PayloadOneofCase.AnimTrigger:
                HandleAnimationTriggerMessage(msg);
                break;
            case GameMessage.PayloadOneofCase.ChatMessage:
                HandleChatMessage(msg);
                break;
            case GameMessage.PayloadOneofCase.NetworkInstantiate:
                HandleInstantiateMessage(msg.NetworkInstantiate);
                break;
            case GameMessage.PayloadOneofCase.NetworkObjectList:
                HandleNetworkObjectList(msg.NetworkObjectList);
                break;
            case GameMessage.PayloadOneofCase.MapBlockUpdate:
                HandleMapBlockUpdate(msg);
                break;
            case GameMessage.PayloadOneofCase.ObjectDestroyMessage:
                HandleObjectDestroy(msg.ObjectDestroyMessage);
                break;
            case GameMessage.PayloadOneofCase.BlockDestroyFlag:
                HandleBlockDestroyFlag(msg.BlockDestroyFlag);
                break;
            case GameMessage.PayloadOneofCase.TickSync:
                HandleTickSync(msg.TickSync);
                break;
            case GameMessage.PayloadOneofCase.AssignWorldSpawner:
                HandleAssignWorldSpawner();
                break;
            case GameMessage.PayloadOneofCase.TransferOwnership:
                HandleTransferOwnership(msg.TransferOwnership);
                break;
        }

        if (msg.PayloadCase == GameMessage.PayloadOneofCase.Rpc)
        {
            rpcSystem.OnReceive(msg);
        }
    }

    private void FailConnection()
    {
        Debug.LogWarning("[LOPNetworkManager] 서버 연결 실패 처리 시작");

        ServerConnectFailPopup.Instance.Show(new ServerConnectFailPopup.Args());

        InternalDisconnect(false);

        OnConnecting?.Invoke(false);
        OnConnectionFailed?.Invoke();
    }

    private void InternalDisconnect(bool sendLeave)
    {
        StopJoinTimeout();

        SceneManager.sceneLoaded -= OnMultiTestSceneLoaded;

        if (sendLeave)
        {
            SendLeave();
        }

        isConnected = false;
        isConnecting = false;
        isWorldSceneLoaded = false;
        isLoadingWorldScene = false;
        hasReceivedLocalJoin = false;

        if (udp != null)
        {
            try
            {
                udp.Close();
            }
            catch { }

            udp = null;
        }

        if (recvThread != null && recvThread.IsAlive)
        {
            try
            {
                recvThread.Join(100);
            }
            catch { }

            recvThread = null;
        }

        receivedMessageQueue = new ConcurrentQueue<byte[]>();
        pendingMessagesBeforeSceneLoad.Clear();
        pendingLocalJoinMessage = null;
        hasPendingLocalJoinMessage = false;

        playerId = string.Empty;
        IsWorldSpawner = false;

        Debug.Log("[LOPNetworkManager] 연결 상태 정리 완료");
    }

    private void ResetConnectionStateForNewAttempt()
    {
        StopJoinTimeout();

        SceneManager.sceneLoaded -= OnMultiTestSceneLoaded;

        isConnected = false;
        isConnecting = false;
        isWorldSceneLoaded = false;
        isLoadingWorldScene = false;
        hasReceivedLocalJoin = false;

        receivedMessageQueue = new ConcurrentQueue<byte[]>();
        pendingMessagesBeforeSceneLoad.Clear();
        pendingLocalJoinMessage = null;
        hasPendingLocalJoinMessage = false;

        playerId = string.Empty;
        IsWorldSpawner = false;
    }

    // -------------------- Join --------------------
    private void HandleJoinMessage(GameMessage msg)
    {
        string newPlayerId = msg.PlayerId;
        GameSync.Join join = msg.Join;
        int newNetworkId = join.NetworkId;
        string newNickname = join.Nickname;

        if (players.ContainsKey(newPlayerId) && players[newPlayerId] != null)
            return;

        bool isLocalPlayer = string.IsNullOrEmpty(playerId);

        if (isLocalPlayer)
        {
            playerId = newPlayerId;
            playerName = newNickname;

            if (join.HasIsWorldSpawner && join.IsWorldSpawner)
            {
                IsWorldSpawner = true;
                Debug.Log("[Client] I am the World Spawner!");
            }
            else
            {
                IsWorldSpawner = false;
            }
        }

        SpawnPlayer(newPlayerId, newNetworkId, isLocalPlayer, newNickname);

        if (isLocalPlayer)
        {
            Debug.Log($"[Client] Local Player Joined: {newPlayerId}. IsWorldSpawner: {IsWorldSpawner}");

            if (IsWorldSpawner)
            {
                if (TreeManager.Instance != null)
                {
                    TreeManager.Instance.SpawnTree();
                }
                else
                {
                    Debug.LogError("TreeManager.Instance가 NULL입니다. 씬에 TreeManager가 있는지 확인하세요.");
                }
            }
        }
        else
        {
            Debug.Log($"[Client] Remote Player Joined: {newPlayerId}");
        }
    }

    private void OnMultiTestSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MultiTest")
            return;

        SceneManager.sceneLoaded -= OnMultiTestSceneLoaded;

        isWorldSceneLoaded = true;
        isLoadingWorldScene = false;
        isConnecting = false;

        OnConnecting?.Invoke(false);

        Debug.Log("[LOPNetworkManager] MultiTest 씬 로드 완료. 대기 중이던 네트워크 메시지를 처리합니다.");

        FlushBufferedMessagesAfterSceneLoad(); // 추가됨
    }

    // -------------------- Leave --------------------
    private void HandleLeaveMessage(GameMessage msg)
    {
        string leaveId = msg.PlayerId;
        if (players.TryGetValue(leaveId, out var obj) && obj != null)
        {
            Destroy(obj);
            players.Remove(leaveId);
            Debug.Log($"[LOPNetworkManager] Player {leaveId} left.");
        }
    }

    // -------------------- TickSync --------------------
    private void HandleTickSync(TickSync tickSync)
    {
        if (Lop.Survivor.TickManager.Instance != null)
        {
            //Debug.Log($"[TickSync] Received ElapsedTicks: {tickSync.ElapsedTicks}");
            TickManager.Instance.ServerOnTick(tickSync.ElapsedTicks);
        }
    }

    // -------------------- Position --------------------
    private void HandlePositionMessage(GameMessage msg)
    {
        if (!networkObjects.TryGetValue(msg.Position.NetworkId, out var obj) || obj == null) return;

        var identity = obj.GetComponent<NetworkIdentity>();
        if (identity != null && identity.IsOwner) return;

        var sync = obj.GetComponent<NetworkTransform>();
        if (sync != null)
        {
            Vector3 pos = new Vector3(msg.Position.Position.X, msg.Position.Position.Y, msg.Position.Position.Z);
            Quaternion rot = new Quaternion(msg.Position.Rotation.Qx, msg.Position.Rotation.Qy, msg.Position.Rotation.Qz, msg.Position.Rotation.Qw);

            // 시퀀스 번호와 함께 전달
            sync.ApplyState(pos, rot, msg.Position.Sequence);
        }
    }

    // -------------------- AnimState --------------------
    private void HandleAnimationStateMessage(GameMessage msg)
    {
        if (!networkObjects.TryGetValue(msg.AnimState.NetworkId, out var obj) || obj == null) return;
        var identity = obj.GetComponent<NetworkIdentity>();
        if (identity != null && identity.IsOwner) return;

        var sync = obj.GetComponent<NetworkAnimator>();
        if (sync != null)
        {
            foreach (var state in msg.AnimState.States)
                sync.ApplyState(state.Key, state.Value);
        }
    }

    // -------------------- AnimTrigger --------------------
    private void HandleAnimationTriggerMessage(GameMessage msg)
    {
        if (!networkObjects.TryGetValue(msg.AnimTrigger.NetworkId, out var obj) || obj == null) return;
        var identity = obj.GetComponent<NetworkIdentity>();
        if (identity != null && identity.IsOwner) return;

        var sync = obj.GetComponent<NetworkAnimator>();
        if (sync != null)
        {
            foreach (var trigger in msg.AnimTrigger.Triggers)
                sync.ApplyTrigger(trigger);
        }
    }

    // -------------------- Spawn Player --------------------
    private void SpawnPlayer(string id, int networkId, bool isLocal, string name)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[LOPNetworkManager] playerPrefab is NULL! Inspector에서 연결했는지 확인하세요.");
            return;
        }

        Transform spawnPoint = GetSpawnPoint(players.Count);
        var newPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        newPlayer.name = $"Player_{id}";

        var networkIdentity = newPlayer.GetComponent<NetworkIdentity>();
        if (networkIdentity != null)
        {
            networkIdentity.SetIdentity(networkId: networkId, isOwner: isLocal);

            foreach (var component in newPlayer.GetComponents<Component>())
            {
                rpcSystem.RegisterRpcComponent(component);
            }
        }
        else
        {
            Debug.LogWarning($"Player Prefab is missing NetworkIdentity component: {newPlayer.name}");
        }

        if (isLocal)
        {
            var controller = newPlayer.GetComponent<CharacterController>();

            if (controller != null)
            {
                GameManager.Instance.SetCharacterController(controller);
            }
            else
            {
                if (controller == null) Debug.LogError("CharacterController를 찾을 수 없습니다! 로컬 플레이어 프리팹 확인 필요.");
                //if (chatHandler == null) Debug.LogError("ChatHandler가 LOPNetworkManager에 연결되지 않았습니다! 인스펙터 확인 필요.");
            }
        }

        var nickSystem = newPlayer.GetComponentInChildren<NicknameSystem>();
        if (nickSystem != null)
        {
            nickSystem.SetNickname(name);
        }
        else
        {
            Debug.LogWarning($"[Nickname] {newPlayer.name}에 NicknameSystem 컴포넌트가 없습니다.");
        }

        players[id] = newPlayer;
        networkObjects[networkId] = newPlayer;
        Debug.Log($"[LOPNetworkManager] Player spawned: {id} at {spawnPoint.position}");
    }

    private Transform GetSpawnPoint(int index)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
            return spawnPoints[index % spawnPoints.Length];

        Debug.LogWarning("[LOPNetworkManager] SpawnPoints not set, using Vector3.zero");
        GameObject temp = new GameObject("TempSpawnPoint");
        temp.transform.position = Vector3.zero;
        return temp.transform;
    }

    // -------------------- ChatMessage --------------------

    private void HandleChatMessage(GameMessage msg)
    {
        string senderId = msg.PlayerId; // UID
        string content = msg.ChatMessage.Message;
        string senderName = senderId; // 기본 UID

        // players 딕셔너리에서 이 UID를 가진 캐릭터 오브젝트 찾기
        if (players.TryGetValue(senderId, out GameObject playerObj))
        {
            if (playerObj != null)
            {
                // 그 캐릭터에 붙어있는 NicknameSystem에서 진짜 닉네임 가져오기
                var nickSystem = playerObj.GetComponentInChildren<NicknameSystem>();
                if (nickSystem != null)
                {
                    senderName = nickSystem.Nickname;
                }
            }
        }

        if (ChatPopup.Instance != null)
        {
            ChatPopup.Instance.ReceiveChatMessage(senderName, content);
        }
    }

    // -------------------- InstantiateMessage --------------------

    private void HandleInstantiateMessage(GameSync.NetworkInstantiate inst)
    {
        if (networkObjects.ContainsKey(inst.NetworkId)) return;

        // 1. 프리팹 식별 (Register를 통해 가져옴)
        if (!prefabRegister.TryGetPrefab(inst.PrefabHash, out var prefab))
        {
            Debug.LogError($"Prefab not found for hash: {inst.PrefabHash}");
            return;
        }

        // 2. 위치 및 회전 설정
        Vector3 position = new Vector3(inst.Position.X, inst.Position.Y, inst.Position.Z);
        Quaternion rotation = new Quaternion(inst.Rotation.Qx, inst.Rotation.Qy, inst.Rotation.Qz, inst.Rotation.Qw);

        // 3. 오브젝트 생성 및 등록
        SpawnNetworkObject(inst, prefab, position, rotation);

        Debug.Log($"[Client] Spawning Network Object: ID={inst.NetworkId}, Owner={inst.OwnerPlayerId}");
    }

    private void SpawnNetworkObject(GameSync.NetworkInstantiate inst, GameObject prefab, Vector3 position, Quaternion rotation, bool spawnByPlayer = false)
    {
        GameObject newObject = Instantiate(prefab, position, rotation);
        newObject.name = $"{prefab.name}NetID{inst.NetworkId}";

        var netId = newObject.GetComponent<NetworkIdentity>();
        bool isOwner = inst.OwnerPlayerId == playerId;

        if (netId != null)
        {
            netId.SetIdentity(inst.NetworkId, isOwner);

            // 새로 생성된 오브젝트의 모든 RPC 컴포넌트 등록
            foreach (var component in newObject.GetComponents<Component>())
            {
                rpcSystem.RegisterRpcComponent(component);
            }
        }

        if (!string.IsNullOrEmpty(inst.DropItemId))
        {
            if (newObject.TryGetComponent<DropItemManage>(out var dropItemManage))
            {
                ItemDatabase itemData = ItemGenerator.Instance.GetItemData(inst.DropItemId);

                if (itemData != null)
                {
                    InventoryItem itemToSet = new InventoryItem(
                        itemData,
                        inst.DropItemCount
                    );

                    dropItemManage.item = itemToSet;
                    dropItemManage.DropItemUIInit();
                    StartCoroutine(dropItemManage.EnablePickUp());
                }
                else
                {
                    Debug.LogError($"[Network] Failed to find ItemData for ID: {inst.DropItemId}");
                }
            }
        }
        networkObjects[inst.NetworkId] = newObject;

        if (isOwner && inst.PredictionKey > 0)
        {
            if (pendingSpawnCallbacks.TryGetValue(inst.PredictionKey, out var callback))
            {
                callback?.Invoke(newObject);

                pendingSpawnCallbacks.Remove(inst.PredictionKey);
            }
        }
    }


    // -------------------- NetworkObjectList --------------------

    private void HandleNetworkObjectList(GameSync.NetworkObjectList list)
    {
        Debug.Log($"[Client] Received existing object list with {list.Objects.Count} objects.");

        foreach (var inst in list.Objects)
        {
            // NetworkInstantiate 메시지 처리 함수를 재사용합니다.
            HandleInstantiateMessage(inst);
        }
    }

    // -------------------- MapBlockUpdate --------------------
    private void HandleMapBlockUpdate(GameMessage msg)
    {
        var update = msg.MapBlockUpdate;

        if (update == null || MapSettingManager.Instance == null) return;

        Vector3 pos = new Vector3(update.X, update.Y, update.Z);

        var map = MapSettingManager.Instance.Map;

        var groundChunk = map.GetChunkFromPosition(pos, Island.ChunkType.Ground);
        var waterChunk = map.GetChunkFromPosition(pos, Island.ChunkType.Water);

        if (groundChunk == null || waterChunk == null) return;

        int localX = Mathf.FloorToInt(pos.x) - Mathf.FloorToInt(groundChunk.Position.x);
        int localY = Mathf.FloorToInt(pos.y);
        int localZ = Mathf.FloorToInt(pos.z) - Mathf.FloorToInt(groundChunk.Position.z);

        BlockData newBlock = map.FindBlockType(update.NewBlockId);

        if (newBlock == null) return;

        newBlock.id = update.NewBlockId;

        groundChunk.chunkData.chunkBlocks[localX, localY, localZ] =

        MapSettingManager.Instance.Map.FindBlockType(update.NewBlockId);

        BlockData blockDataInstance = new BlockData(newBlock);

        if (update.NewLevel != -1)
        {
            blockDataInstance.level = update.NewLevel;
            groundChunk.chunkData.chunkBlocks[localX, localY, localZ] = newBlock;
            groundChunk.chunkData.chunkBlocks[localX, localY, localZ].level = update.NewLevel;
        }



        if (update.NewBlockId == BlockConstants.Water)
        {
            waterChunk.chunkData.chunkBlocks[localX, localY, localZ] = blockDataInstance;
            groundChunk.chunkData.chunkBlocks[localX, localY, localZ] = map.FindBlockType(BlockConstants.Air);
        }

        else if (update.NewBlockId == BlockConstants.Air)
        {
            waterChunk.chunkData.chunkBlocks[localX, localY, localZ] = map.FindBlockType(BlockConstants.Air);
            groundChunk.chunkData.chunkBlocks[localX, localY, localZ] = map.FindBlockType(BlockConstants.Air);
        }

        else
        {
            groundChunk.chunkData.chunkBlocks[localX, localY, localZ] = blockDataInstance;
            waterChunk.chunkData.chunkBlocks[localX, localY, localZ] = map.FindBlockType(BlockConstants.Air);
        }

#if UNITY_EDITOR
        Debug.Log($"[MapSync] Block at {pos} updated to {update.NewBlockId}. Local data synchronized.");
#endif
        groundChunk.UpdateChunk();

        if (waterChunk != groundChunk)
        {
            waterChunk.UpdateChunk();
        }

        map.UpdateChunk(pos);
    }


    // -------------------- ObjectDestroy --------------------

    private void HandleObjectDestroy(ObjectDestroyMessage destroyMsg)
    {
        int networkId = destroyMsg.NetworkId;

        if (networkObjects.TryGetValue(networkId, out var obj))
        {
            if (obj == null)
            {
                Debug.LogWarning($"[Client Warning] ID={networkId}는 dict에 있지만 이미 유니티에서 파괴되었거나 유효하지 않습니다. 제거합니다.");
            }
            else
            {
                Destroy(obj);
                Debug.Log($"[Client] Successfully requested Destroy for Network Object: ID={networkId}, Name={obj.name}");
            }

            networkObjects.Remove(networkId);
        }
        else
        {
            Debug.LogWarning($"[Client] Received destroy message for an unknown object: ID={networkId}");
        }
    }

    private void HandleBlockDestroyFlag(BlockDestroyFlag destroyFlag)
    {
        // 수신된 int32 좌표를 Vector3로 변환하여 맵 함수에 전달
        var pos = new Vector3(destroyFlag.X, destroyFlag.Y, destroyFlag.Z);

        if (MapSettingManager.Instance != null && MapSettingManager.Instance.Map != null)
        {
            var mapInstance = MapSettingManager.Instance.Map;

            try
            {
                // GetBlockInChunk는 Vector3를 받습니다.
                var blockData = mapInstance.GetBlockInChunk(pos, ChunkType.Ground);

                if (blockData != null)
                {
                    blockData.isDestroy = destroyFlag.IsDestroyed;
                    mapInstance.UpdateChunk(pos);
                    Debug.Log($"[Client] Block {(destroyFlag.IsDestroyed ? "DESTROYED" : "RESTORED")} at {pos}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Client Error] Failed to handle BlockDestroyFlag at {pos}: {ex.Message}");
            }
        }
    }

    // -------------------- AssignWorldSpawner --------------------
    private void HandleAssignWorldSpawner()
    {
        // 이전에 스포너가 아니었을 경우에만 로그를 남기고 상태를 변경합니다.
        if (!IsWorldSpawner)
        {
            IsWorldSpawner = true;
            Debug.LogWarning("[Client] Received role assignment: I am NOW the World Spawner!");
        }
    }

    // -------------------- TransferOwnership --------------------
    private void HandleTransferOwnership(TransferOwnership transfer)
    {
        int networkId = transfer.NetworkId;
        string newOwnerId = transfer.NewOwnerPlayerId;

        if (!networkObjects.TryGetValue(networkId, out var obj))
        {
            Debug.LogWarning($"[Transfer] 소유권 이전 대상(ID: {networkId})을 찾을 수 없습니다.");
            return;
        }

        var identity = obj.GetComponent<NetworkIdentity>();
        if (identity == null) return;

        bool amINewOwner = (newOwnerId == this.playerId);

        Debug.LogWarning($"[Transfer] 객체 {networkId} 소유권 이전됨. 새 주인: {newOwnerId}. (내가 주인: {amINewOwner})");

        // NetworkIdentity의 IsOwner 상태 갱신
        identity.SetIdentity(networkId, amINewOwner);

        // MonsterInitializer를 찾아 컴포넌트 재설정
        var monsterInitializer = obj.GetComponent<MonsterInitializer>();
        if (monsterInitializer != null)
        {
            // 수정: 소유권 이전에 의한 호출임을 알림 (isMigration = true)
            monsterInitializer.ReInitialize(amINewOwner, true);
        }
    }

    // -------------------- Send --------------------
    private void SendRaw(GameMessage msg)
    {
        try
        {
            if (udp != null)
            {
                byte[] data = msg.ToByteArray();
                udp.Send(data, data.Length);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LOPNetworkManager] 메시지 전송 오류: {e.Message}");
        }
    }

    public void SendLeave()
    {
        // playerId가 없거나 이미 연결이 끊긴 상태면 전송하지 않습니다.
        if (string.IsNullOrEmpty(playerId) || !isConnected) return;

        var msg = new GameMessage
        {
            PlayerId = playerId,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Leave = new GameSync.Leave()
        };
        SendRaw(msg);
        Debug.Log("[LOPNetworkManager] Sending Leave message.");
    }

    public void SendPosition(int networkId, Vector3 pos, Quaternion rot, int sequence)
    {
        var protoRot = new ProtoRot { Qx = rot.x, Qy = rot.y, Qz = rot.z, Qw = rot.w };
        var protoPos = new ProtoPos { X = pos.x, Y = pos.y, Z = pos.z };

        var msg = new GameMessage
        {
            PlayerId = playerId,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Position = new PositionSync // 새 Position 메시지 (PositionSync 아님)
            {
                NetworkId = networkId,
                Position = protoPos,
                Rotation = protoRot,
                Sequence = sequence // 시퀀스 할당
            }
        };
        SendRaw(msg);
    }

    public void SendAnimationState(int networkId, int paramHash, bool value)
    {
        var msg = new GameMessage
        {
            PlayerId = playerId,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            AnimState = new GameSync.AnimationState { NetworkId = networkId } // 새 메시지
        };
        msg.AnimState.States.Add(paramHash, value);
        SendRaw(msg);
    }

    public void SendAnimationTrigger(int networkId, int paramHash)
    {
        var msg = new GameMessage
        {
            PlayerId = playerId,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            AnimTrigger = new AnimationTrigger { NetworkId = networkId } // 새 메시지
        };
        msg.AnimTrigger.Triggers.Add(paramHash);
        SendRaw(msg);
    }

    public void RPC(object component, string methodName, params object[] args)
    {
        var msg = rpcSystem.CreateRpcMessage(component, methodName, args);
        msg.PlayerId = playerId;
        SendRaw(msg);
    }

    public void NetworkInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, string dropItemId = "", int dropItemCount = 1)
    {
        int prefabHash = prefabRegister.GetPrefabHash(prefab);

        var protoPos = new ProtoPos { X = position.x, Y = position.y, Z = position.z };
        var protoRot = new ProtoRot { Qx = rotation.x, Qy = rotation.y, Qz = rotation.z, Qw = rotation.w };

        var msg = new GameMessage
        {
            PlayerId = playerId,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            NetworkInstantiate = new GameSync.NetworkInstantiate
            {
                PrefabHash = prefabHash,
                Position = protoPos,
                Rotation = protoRot,
                DropItemId = dropItemId,
                DropItemCount = dropItemCount
            }
        };
        SendRaw(msg);
    }

    public void NetworkInstantiateWithCallback(GameObject prefab, Vector3 position, Quaternion rotation, Action<GameObject> onSpawnedCallback, string dropItemId = "", int dropItemCount = 1)
    {
        int prefabHash = prefabRegister.GetPrefabHash(prefab);
        if (prefabHash == -1)
        {
            Debug.LogError($"[NetworkInstantiate] Prefab '{prefab.name}' is not registered.");
            onSpawnedCallback?.Invoke(null);
            return;
        }

        int callbackKey = nextCallbackKey++;

        if (onSpawnedCallback != null)
        {
            pendingSpawnCallbacks[callbackKey] = onSpawnedCallback;
        }

        var protoPos = new ProtoPos { X = position.x, Y = position.y, Z = position.z };
        var protoRot = new ProtoRot { Qx = rotation.x, Qy = rotation.y, Qz = rotation.z, Qw = rotation.w };

        var msg = new GameMessage
        {
            PlayerId = playerId,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            NetworkInstantiate = new GameSync.NetworkInstantiate
            {
                PrefabHash = prefabHash,
                Position = protoPos,
                Rotation = protoRot,
                DropItemId = dropItemId,
                DropItemCount = dropItemCount,
                PredictionKey = callbackKey
            }
        };

        SendRaw(msg);
    }

    public void SendChatMessage(string content)
    {
        var msg = new GameMessage
        {
            PlayerId = playerId,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ChatMessage = new ChatMessage
            {
                Message = content
            }
        };
        SendRaw(msg);
    }

    public void SendBlockUpdate(Vector3Int position, string newBlockId, int level = -1)
    {
        var msg = new GameMessage
        {
            PlayerId = playerId,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            MapBlockUpdate = new MapBlockUpdate
            {
                X = position.x,
                Y = position.y,
                Z = position.z,
                NewBlockId = newBlockId,
                NewLevel = level
            }
        };
        SendRaw(msg);
    }

    public void SendObjectDestroy(int networkId)
    {
        var msg = new GameMessage
        {
            PlayerId = playerId, // 로컬 플레이어 ID를 포함
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ObjectDestroyMessage = new ObjectDestroyMessage
            {
                NetworkId = networkId
            }
        };
        SendRaw(msg);
    }

    public void NetworkDestroy(GameObject obj)
    {
        if (obj.TryGetComponent<NetworkIdentity>(out var identity))
        {
            SendObjectDestroy(identity.NetworkId);
        }
        else
        {
            Destroy(obj); // 네트워크 객체가 아니면 일반 파괴
        }
    }

    public void SendBlockDestroyFlag(Vector3Int position, bool isDestroyed)
    {
        var msg = new GameMessage
        {
            PlayerId = playerId,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            BlockDestroyFlag = new BlockDestroyFlag
            {
                // Vector3Int의 정수 값을 직접 할당
                X = position.x,
                Y = position.y,
                Z = position.z,
                IsDestroyed = isDestroyed
            }
        };
        SendRaw(msg);
    }

    // -------------------- Utility --------------------
    public void NetworkSetActive(GameObject obj, bool isActive)
    {
        if (isConnected) RPC(this, nameof(NetworkSetActiveRPC), obj, isActive);
        else Debug.LogError("[LOPNetworkManager] 서버와 연결되지 않았습니다 SetActive RPC가 작동하지 않습니다");
    }

    [LopRPC]
    private void NetworkSetActiveRPC(GameObject obj, bool isActive)
    {
        obj.SetActive(isActive);
    }
}
