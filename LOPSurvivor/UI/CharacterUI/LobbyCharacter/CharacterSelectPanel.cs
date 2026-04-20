// System
using GlobalAudio;
using System;
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;
using UnityEngine.UI;

// Project
// Alias

public class CharacterSelectPanel : Panel<CharacterSelectPanel>
{
    /// <summary>
    /// Panel의 Type을 정의
    /// </summary>
    public override PanelType PanelType => PanelType.CharacterSelectPanel;

    /// <summary>
    /// Panel 구성에 필요한 변수들 선언
    /// </summary>
    public class Args : PanelArgument
    {
        
    }

    [Header("CharacterSelectPanel")]
    //[SerializeField] public Button btn_back = null;
    [SerializeField] private Button btn_survivor_start;
    [SerializeField] private Button btn_detail_back = null;
    [SerializeField] private CharacterLobbySelectUI lobbyUI;

    [SerializeField] private AudioClip clickClip;

    /// <summary>
    /// Panel이 생성될 때 처리해야 하는 작업들을 처리
    /// ex) 서버에 특정 Api를 요청해 데이터 받아오기
    /// ex) 코루틴 시작
    /// ex) 애니메이션 시작
    /// </summary>
    /// <param name="panelArguments">PanelClass.Args를 전달</param>
    public override void OnShow(PanelArgument panelArguments)
    {
        Debug.Log($"CharacterSelectPanel.OnShow()");

        if (panelArguments is not Args args)
        {
            Debug.LogError($"Cannot cast panelArguments to {nameof(CharacterSelectPanel)}'s Args.");
            return;
        }

        //AddListeners();
        btn_detail_back.onClick.RemoveAllListeners();
        btn_detail_back.onClick.AddListener(OnBackButtonClicked);
    }

    /// <summary>
    /// Panel이 닫힐 때 처리해줘야 하는 작업을 처리
    /// ex) 네트워크 연결 중이라면 취소하기
    /// ex) 진행 중인 코루틴 등 취소하기
    /// </summary>
    public override void OnHide()
    {
    }

    /*private void AddListeners()
    {
        btn_back.onClick.AddListener(OnBackButtonClicked);
    }*/


    public override void OnBackButtonClicked()
    {
        AudioManager.Instance.PlaySFX(clickClip);
        base.OnBackButtonClicked();
        Debug.Log($"CharacterSelectPanel.OnBackButtonClicked()");
    }
}
