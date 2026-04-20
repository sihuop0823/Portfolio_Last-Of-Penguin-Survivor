using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Lop.Survivor;

public class ChatPopup : Popup<ChatPopup>
{
    public override PopupType PopupType => PopupType.ChatPopup;
    public static ChatPopup Instance;

    [Header("UI Components")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button chat_enter_btn;
    [SerializeField] private Button chat_exit_btn;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private GameObject chatTextPrefab;
    [SerializeField] private ScrollRect chatScrollRect;

    [Header("Message Setting")]
    [SerializeField] private int maxMessages = 50;
    [SerializeField] private int maxCharacterLimit = 80;

    private bool wasFocused = false;
    public Action<bool> isNewMessage;

    private float lastSendTime = 0f;
    private readonly float sendCooldown = 0.15f;

    public bool IsChatOpen => gameObject.activeSelf;

    private void Awake()
    {
        if (null == Instance)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        if (chat_enter_btn != null)
            chat_enter_btn.onClick.AddListener(SendChatMessage);

        if (inputField != null)
        {
            inputField.characterLimit = maxCharacterLimit;
            inputField.onSubmit.AddListener((val) => SendChatMessage());
        }

        if (chat_exit_btn != null)
            chat_exit_btn.onClick.AddListener(() => gameObject.SetActive(false));
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (inputField != null)
        {
            inputField.text = "";
            inputField.Select();
            inputField.ActivateInputField();
        }

        if (WorldInputManager.Instance != null) WorldInputManager.Instance.SetChatState(true);

        if (GameManager.Instance?.characterController != null)
        {
            GameManager.Instance.characterController.ChangeCharacterState(CharacterState.UsingUI);
        }

        isNewMessage?.Invoke(false);

        if (contentRect != null)
        {
            foreach (Transform child in contentRect)
            {
                ChatText ct = child.GetComponent<ChatText>();
                if (ct != null) ct.ChatTextSizeUpdate();
            }

            // 레이아웃 갱신 강제 명령
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

            StartCoroutine(ScrollToBottom());
        }
    }

    private void OnDisable()
    {
        if (inputField != null) inputField.DeactivateInputField();

        // 한글 모음이나 자음만 남는거 방어
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

        wasFocused = false;

        if (PopupBase.ActivePopupCount == 0 && PanelBase.ActivePanelCount == 0)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (WorldInputManager.Instance != null)
        {
            WorldInputManager.Instance.SetChatState(false);
        }

        if (GameManager.Instance?.characterController != null)
        {
            GameManager.Instance.characterController.ChangeCharacterState(CharacterState.Alive);
        }
    }

    protected override void OnShow(PopupArgument args) { }
    protected override void OnHide() { }

    private void SendChatMessage()
    {
        if (Time.unscaledTime - lastSendTime < sendCooldown) return;
        lastSendTime = Time.unscaledTime;

        if (inputField == null) return;

        string currentText = inputField.text;
        if (Input.compositionString.Length > 0)
        {
            currentText += Input.compositionString;
        }

        string finalMessage = currentText.Replace("\n", "").Replace("\r", "");

        if (string.IsNullOrWhiteSpace(finalMessage))
        {
            gameObject.SetActive(false);
            return;
        }

        if (LOPNetworkManager.Instance != null)
        {
            LOPNetworkManager.Instance.SendChatMessage(finalMessage);
        }

        string myName = LOPNetworkManager.Instance != null ? LOPNetworkManager.Instance.playerName : "Me";

        // 내가 보낸 메시지 true
        AddMessageToContent(myName, finalMessage, true);

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(ClearInputNextFrame());
        }
    }

    private IEnumerator ClearInputNextFrame()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        inputField.DeactivateInputField();
        inputField.text = "";

        yield return null;
        yield return null;

        inputField.text = "";
        inputField.ActivateInputField();
        inputField.Select();
    }

    public void ReceiveChatMessage(string nickname, string content)
    {
        AddMessageToContent(nickname, content, false);
    }

    private void AddMessageToContent(string nickname, string content, bool isMyMessage)
    {
        if (chatTextPrefab == null) return;
        GameObject instGo = Instantiate(chatTextPrefab, contentRect);

        ChatText chatText = instGo.GetComponent<ChatText>();
        if (chatText != null) chatText.SetContent(nickname, content);

        if (contentRect.childCount > maxMessages)
            Destroy(contentRect.GetChild(0).gameObject);

        if (gameObject.activeInHierarchy)
        {
            // 내 메시지 & 현재 스크롤이 바닥 부근일 때만 갱신
            if (isMyMessage || chatScrollRect.verticalNormalizedPosition <= 0.05f)
            {
                StartCoroutine(ScrollToBottom());
            }
        }

        if (!gameObject.activeSelf)
        {
            isNewMessage?.Invoke(true);
        }
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();

        if (chatScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }
}