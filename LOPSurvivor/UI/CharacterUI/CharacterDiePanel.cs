using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class CharacterDiePanel : Panel<CharacterDiePanel>
{
    public override PanelType PanelType => PanelType.CharacterDiePanel;

    [Header("Panel")]
    [SerializeField] private GameObject characterDie_img;
    [SerializeField] private TMP_Text dieReason_txt;
    [SerializeField] private Button respawn_btn;
    [SerializeField] private Button lobby_btn;

    public class Args : PanelArgument
    {

    }

    private void Awake()
    {
        
    }

    private void Start()
    {
        if(respawn_btn != null)
            respawn_btn.onClick.AddListener(RespawnCharacter);

        if (lobby_btn != null)
            lobby_btn.onClick.AddListener(GoLobby);

    }

    public override void OnShow(PanelArgument panelArguments)
    {

    }

    public override void OnHide()
    {
    }

    private void RespawnCharacter()
    {
        PanelManager.Instance.HideAll();

        if (CharacterController.Instance != null)
        { 
            CharacterController.Instance.RespawnProcess();
        }
        else
        {
            Debug.LogError("캐릭터가 없는 매우 행복한 상황");
        }
    }

    private void GoLobby()
    {
        DisconnectAndGoLobby();

        SceneManager.LoadScene(SceneName.Lobby);
    }

    IEnumerator DisconnectAndGoLobby()
    {
        if (LOPNetworkManager.Instance.isConnected)
        {
            LOPNetworkManager.Instance.Disconnect();

            while (LOPNetworkManager.Instance.isConnected)
            {
                yield return null; 
            }
        }

        if (WorldInputManager.Instance != null)
        {
            WorldInputManager.Instance.gameInputType = WorldInputManager.GameInputType.None;
        }

        SceneManager.LoadScene(SceneName.Lobby);
    }
}
