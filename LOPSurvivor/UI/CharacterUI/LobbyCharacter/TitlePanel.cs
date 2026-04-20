
// System
using GlobalAudio;
using System;
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Project
// Alias

public class TitlePanel : Panel<TitlePanel>
{
    /// <summary>
    /// Panel�� Type�� ����
    /// </summary>
    public override PanelType PanelType => PanelType.TitlePanel;

    /// <summary>
    /// Panel ������ �ʿ��� ������ ����
    /// </summary>
    public class Args : PanelArgument
    {
        // �� Panel�� ���� �ʿ� ����
    }

    /// <summary>
    /// ��ư(btn), �̹���(img), �ؽ�Ʈ(txt) �� UI���� uitype_uiname �̷������� �ۼ�.
    /// ������ Panel Ŭ�������� UI�� ����ϴ� Ŭ�������̱� ������ UI�� �������� ������ ã�� ����.
    /// ���� ��� ���� � ��ư�� �����Ϸ��� �ϸ�, �� ��ư�� ������ �����ϱ� ���� btn_�� ġ�� ����Ʈ�� ������ �ϰ� ���ؼ�.
    /// </summary>
    [SerializeField] private Button btn_gameStart = null;
    [SerializeField] private Button btn_options = null;
    [SerializeField] private Button btn_gameQuit = null;

    [SerializeField] private AudioClip clickClip;

    /// <summary>
    /// Panel�� ������ �� ó���ؾ� �ϴ� �۾����� ó��
    /// ex) ������ Ư�� Api�� ��û�� ������ �޾ƿ���
    /// ex) �ڷ�ƾ ����
    /// ex) �ִϸ��̼� ����
    /// </summary>
    /// <param name="panelArguments">PanelClass.Args�� ����</param>
    /// 

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void OnShow(PanelArgument panelArguments)
    {
        Debug.Log($"TitlePanel.OnShow()");
        if (panelArguments is not Args args)
        {
            Debug.LogError($"Cannot cast panelArguments to {nameof(TitlePanel)}'s Args.");
            return;
        }

        AddListeners();
    }

    /// <summary>
    /// Panel�� ���� �� ó������� �ϴ� �۾��� ó��
    /// ex) ��Ʈ��ũ ���� ���̶�� ����ϱ�
    /// ex) ���� ���� �ڷ�ƾ �� ����ϱ�
    /// </summary>
    public override void OnHide()
    {
        CheckExitPopup.Instance.isExit = false;
        CheckExitPopup.Instance.isLobby = false;

    }

    private void AddListeners()
    {
        btn_gameStart.onClick.AddListener(OnGameStartButtonClicked);
        btn_options.onClick.AddListener(OnOptionsButtonClicked);
        btn_gameQuit.onClick.AddListener(OnGameQuitButtonClicked);
    }

    private void OnGameStartButtonClicked()
    {
        AudioManager.Instance.PlaySFX(clickClip);
        Debug.Log("[TitlePanel] OnGameStartButtonClicked");
        
        PanelManager.Instance.Show(PanelType.ServerSelectPanel, new ServerSelectPanel.Args()
        {
            
        });

        
        //�߰��Ұ� ���ٸ� �Ʒ�ó�� �ص� ��.
        // example
        //PanelManager.Instance.Show(PanelType.PlayModeSelectPanel, new PlayModeSelectPanel.Args());
        // or
        //PanelManager.Instance.Show(PanelType.PlayModeSelectPanel, new PlayModeSelectPanel.Args() { });
    }

    private void OnOptionsButtonClicked()
    {
        bool nextState = !SettingPopup.Instance.settingpopup.activeSelf;

        SettingPopup.Instance.settingpopup.SetActive(nextState);
        SettingPopup.Instance.blind_img.SetActive(nextState);
        AudioManager.Instance.PlaySFX(clickClip);
        // TODO : Options �г��� ������ ���°� ����
    }

    private void OnGameQuitButtonClicked()
    {
        WorldInputManager.Instance.gameInputType = WorldInputManager.GameInputType.isOpenOptionCheckPopup;
        AudioManager.Instance.PlaySFX(clickClip);
        CheckExitPopup.Instance.isExit = true;
        GuideText();
    }

    public void GuideText()
    {
        CheckExitPopup.Instance.Show(null);
        Scene currentScene = SceneManager.GetActiveScene();
        Debug.Log(currentScene.name);

        if (currentScene.name != SceneName.Lobby)
        {
            CheckExitPopup.Instance.guide_txt.text = CheckExitPopup.Instance.isLobby ? "로비로 돌아가시겠습니까?" : "정말로 게임을 종료하시겠습니까?";
        }
        else
        {
            CheckExitPopup.Instance.guide_txt.text = "정말로 게임을 종료하시겠습니까?";
        }
    }
}

