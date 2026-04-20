using GlobalAudio;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static WorldInputManager;

public class CheckExitPopup : Popup<CheckExitPopup>
{
    public override PopupType PopupType => PopupType.CheckExitPopup;

    public TMP_Text guide_txt;
    [SerializeField] private Button check_yes_btn;
    [SerializeField] private Button check_no_btn;

    [SerializeField] private string lobbyScene;
    [SerializeField] private GameObject checkExitpopup;

    [SerializeField] private AudioClip clickClip;

    public static CheckExitPopup Instance;

    private bool isFisrtSound = false; // 맨 처음에 UI 클릭 사운드 나고 시작하는 거 비활성화

    //Lobby & Exit
    public bool isLobby;
    public bool isExit;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private void Start()
    {
        Hide();
        //isFisrtSound = true;
        isLobby = true;
        isExit = false;
    }

    protected override void OnShow(PopupArgument args)
    {
        checkExitpopup.SetActive(true);
    }

    protected override void OnHide()
    {
        isExit = false;
        isLobby = false;
        checkExitpopup.SetActive(false);
    }

    public void OnCloseButtonClicked()
    {
        AudioManager.Instance.PlaySFX(clickClip);
        WorldInputManager.Instance.gameInputType = GameInputType.isOpenOption;
        Hide();
    }

    /// <summary>
    /// Yes 버튼에 들어가는 함수를 조정해주는 친구입니다. isExit와 isLobby에 상태에 따라서 버튼을 인식해서 작동되는 함수가 다르게 조정해줍니다.
    /// </summary>
    public void TuneYesBtn()
    {
        AudioManager.Instance.PlaySFX(clickClip);
        if (SettingPopup.Instance != null && isLobby)
        {
            //guide_txt.text = "로비로 돌아가시겠습니까?";
            SettingLobby();
            Invoke(nameof(Hide), 0.3f);
        }

        if (SettingPopup.Instance != null && isExit)
        {
            //guide_txt.text = "정말로 게임을 종료하시겠습니까?";
            SettingExit();
            Invoke(nameof(Hide), 0.3f);
        }
    }

    private void SettingExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
    }

    private void SettingLobby()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (SettingPopup.Instance != null)
        {
            isLobby = false;
            isExit = false;
            SettingPopup.Instance.Hide();
            SettingPopup.Instance.blind_img.SetActive(false);
        }

        SettingPopup.Instance.Hide();

        if(LOPNetworkManager.Instance.isConnected)
        {
            LOPNetworkManager.Instance.Disconnect();
        }

        SceneManager.LoadScene(lobbyScene);
    }

}
