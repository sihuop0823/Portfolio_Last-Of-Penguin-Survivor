using GlobalAudio;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingPopup : Popup<SettingPopup>
{
    public override PopupType PopupType => PopupType.SettingPopup;

    [Header("Audio")]
    public Slider bgm_slider;
    public Slider sfx_slider;
    [SerializeField] private AudioClip clickClip;

    [Header("Setting Popup")]
    public GameObject settingpopup;
    public GameObject blind_img;

    [Header("Setting Button")]
    [SerializeField] private Button setting_exit_btn;
    [SerializeField] private Button setting_lobby_btn;
    [SerializeField] private Button setting_close_btn;

    [Header("Check Exit Popup")]
    public GameObject CheckExitPopupObject;

    [Header("Camera")]
    [SerializeField] private GameObject CameraGroup;
    [SerializeField] Slider camera_sensitivity_slider;
    [Range(0.1f, 1.0f)] public float viewSensitivity = 0.5f;
    [SerializeField] private float sensitivityCorrection = 1.0f;

   

    public static SettingPopup Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Hide();

        // ̴ ʱⰪ 
        if (bgm_slider != null)
            bgm_slider.value = AudioManager.Instance != null ? AudioManager.Instance.BGMVolume : 1f;

        if (sfx_slider != null)
            sfx_slider.value = AudioManager.Instance != null ? AudioManager.Instance.SFXVolume : 1f;

        // ̺Ʈ 
        if (bgm_slider != null)
            bgm_slider.onValueChanged.AddListener(SetBGMVolume);

        if (sfx_slider != null)
            sfx_slider.onValueChanged.AddListener(SetSFXVolume);

        //setting btn

        if (setting_lobby_btn != null)
            setting_lobby_btn.onClick.AddListener(GoLobby);

        if (setting_exit_btn != null)
            setting_exit_btn.onClick.AddListener(GameExit);
        if (setting_close_btn != null)
            setting_close_btn.onClick.AddListener(OnCloseButtonClicked);

        if (camera_sensitivity_slider != null)
        {
            camera_sensitivity_slider.minValue = 0.1f;
            camera_sensitivity_slider.maxValue = 1.0f;
            camera_sensitivity_slider.value = 0.5f;
            camera_sensitivity_slider.onValueChanged.AddListener(SetCameraSensitivity);
        }

        CameraGroup.SetActive(false);

        
    }

    protected override void OnShow(PopupArgument args)
    {
        if (settingpopup != null)
        {
            Scene currentScene = SceneManager.GetActiveScene();

            if (currentScene.name == SceneName.Game || currentScene.name == SceneName.MultiGame)
            {
                GameManager.Instance?.characterController?.ChangeCharacterState(CharacterState.UsingUI);
            }

            if (currentScene.name == SceneName.Lobby)
            {
                if (setting_lobby_btn != null) setting_lobby_btn.interactable = false;
                CameraGroup.SetActive(false);
            }
            else
            {
                if (setting_lobby_btn != null) setting_lobby_btn.interactable = true;
                CameraGroup.SetActive(true);
            }

            settingpopup.SetActive(true);
            blind_img.SetActive(true);
        }
    }

    protected override void OnHide()
    {
        if (settingpopup != null)
        {
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.name == SceneName.Game || currentScene.name == SceneName.MultiGame)
            {
                GameManager.Instance?.characterController.ChangeCharacterState(CharacterState.Alive);
            }
            settingpopup.SetActive(false);
            blind_img.SetActive(false);
            ApplySceneUIState();

            CheckExitPopup.Instance.isLobby = false;
            CheckExitPopup.Instance.isExit = false;
        }
    }

    public void OnCloseButtonClicked()
    {
        AudioManager.Instance.PlaySFX(clickClip);
        WorldInputManager.Instance.gameInputType = WorldInputManager.GameInputType.None;
        Hide();
    }

    private void SetCameraSensitivity(float value)
    {
        var controller = GameManager.Instance?.characterController;
        if (controller != null)
        {
            float finalSens = value * sensitivityCorrection;

            if (controller.firstPersonController != null)
                controller.firstPersonController.mouseSensitivity = finalSens;

            if (controller.thirdPersonController != null)
                controller.thirdPersonController.mouseSensitivity = finalSens;
        }
    }

    private void SetBGMVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBGMVolume(value);
    }

    private void SetSFXVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    private void ApplySceneUIState()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case SceneName.Lobby:
                settingpopup.SetActive(false);
                blind_img.SetActive(false);
                break;
            case SceneName.Game:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                settingpopup.SetActive(false);
                blind_img.SetActive(false);
                break;
            case SceneName.MultiGame:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                settingpopup.SetActive(false);
                blind_img.SetActive(false);
                break;
            default:
                break;
        }
    }

    public void GoLobby()
    {
        WorldInputManager.Instance.gameInputType = WorldInputManager.GameInputType.isOpenOptionCheckPopup;
        AudioManager.Instance.PlaySFX(clickClip);
        CheckExitPopup.Instance.isLobby = true;
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == SceneName.Lobby)
        {
            OnCloseButtonClicked();
        }
        else
        {
            GuideText();
        }
    }

    private void GameExit()
    {
        WorldInputManager.Instance.gameInputType = WorldInputManager.GameInputType.isOpenOptionCheckPopup;
        AudioManager.Instance.PlaySFX(clickClip);
        CheckExitPopup.Instance.isExit = true;
        GuideText();
    }

    public void GuideText()
    {
        CheckExitPopup.Instance.Show(null);

        CheckExitPopup.Instance.guide_txt.text = CheckExitPopup.Instance.isLobby ? "로비로 돌아가시겠습니까?" : "정말로 게임을 종료하시겠습니까?";
    }

    public bool IsSettingOpen()
    {
        return settingpopup != null && settingpopup.activeSelf;
    }
}