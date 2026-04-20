// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelManager : Singletone<PanelManager>
{ 
    // BatteryPanel info wrapper class
    [Serializable]
    public class PanelData
    {
        public PanelType type = default;
        public PanelArgument argument = default;
    }

    // root canvas to attatch BatteryPanel
    [SerializeField] private RectTransform rootCanvas = null;

    // stack to save BatteryPanel open history
    private Stack<PanelData> panelStack = new Stack<PanelData>();

    // list to save BatteryPanel prefab info
    [SerializeField] private List<PanelBase> panelBases = new List<PanelBase>();

    // current BatteryPanel information
    [SerializeField] private PanelBase currentPanelBase = null;
    [SerializeField] private PanelData currentPanelData = null;

    private bool isUILocked = false; // UI 락거는 변수
    public bool IsUILocked => isUILocked;
    public void Show(PanelType type, PanelArgument argument)
    {
        if (isUILocked)
        {
            Debug.LogWarning("UI is Lock");
            return;
        }

        Scene currentScene = SceneManager.GetActiveScene();

        if (currentPanelBase)
        {
            panelStack.Push(currentPanelData);
            currentPanelBase.OnHide();
            Destroy(currentPanelBase.gameObject);
        }

        // �� �г� ����
        PanelBase panelBasePrefab = panelBases.Find((panelBase) => panelBase.PanelType == type);
        PanelBase instantiatedPanelBase = Instantiate(panelBasePrefab, rootCanvas);

        instantiatedPanelBase.OnShow(argument);

        if (currentScene.name == SceneName.Game || currentScene.name == SceneName.MultiGame)
        {
            if (CharacterController.Instance != null)
            {
                GameManager.Instance?.characterController.ChangeCharacterState(CharacterState.UsingUI);
            }
        }

        currentPanelBase = instantiatedPanelBase;
        currentPanelData = new PanelData()
        {
            type = type,
            argument = argument
        };
    }


    /// <summary>
    /// Hide panel and automatically open previous panel if exists.
    /// </summary>
    public void Hide()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (!currentPanelBase)
            return;

        currentPanelBase.OnHide();

        Destroy(currentPanelBase.gameObject);

        currentPanelBase = null;
        currentPanelData = null;

        if (panelStack.Count > 0)
        {
            PanelData previousPanelData = panelStack.Pop();
            Show(previousPanelData.type, previousPanelData.argument);
        }

        if (panelStack.Count <= 0)
        {
            if (currentScene.name == SceneName.Game || currentScene.name == SceneName.MultiGame)
            {
                GameManager.Instance?.characterController.ChangeCharacterState(CharacterState.Alive);
            }
            WorldUIManager.Instance?.Hide();
        }
    }

    public void HideAll()
    {
        if (!currentPanelBase)
            return;

        PanelBase.IsHidingAll = true;

        try
        {
            currentPanelBase.OnHide();
            Destroy(currentPanelBase.gameObject);

            panelStack.Clear();
            currentPanelBase = null;
            currentPanelData = null;

            // 0으로 리셋
            PanelBase.ActivePanelCount = 0;

            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.characterController != null)
                {
                    GameManager.Instance.characterController.ChangeCharacterState(CharacterState.Alive);
                }
            }

            if (PopupBase.ActivePopupCount == 0)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        finally
        {
            // 플래그를 바로 끄지 않고, OnDisable이 실행된 후에 끄도록 코루틴 예약 처리
            StartCoroutine(ResetHideAllFlag());
        }
    }

    public void SetUILock(bool isLocked)
    {
        isUILocked = isLocked;
    }

    /// <summary>
    /// OnDisable이 실행되고 난 "다음 프레임"에 플래그를 리셋하는 함수. Panel.cs의 OnDisable과 생명 주기 꼬여서 일부러 이렇게 처리함
    /// </summary>
    private IEnumerator ResetHideAllFlag()
    {

        yield return null;
        PanelBase.IsHidingAll = false;
    }

    public void Hide(int count)
    {

        if (count >= panelStack.Count)
        {
            Debug.LogError($"Your hide count is bigger than panel stack count. The excess part will be ignored.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Hide();
        }
    }

    public void HideAndOpen(int count, PanelType type, PanelArgument argument)
    {

    }

    public void HideAllAndOpen()
    {

    }
}