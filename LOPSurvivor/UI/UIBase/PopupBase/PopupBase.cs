// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Project
// Alias

public abstract class PopupBase : MonoBehaviour
{
    private Action OnPopupHide = null;

    public static int ActivePopupCount = 0; 
    //팝업이 몇 개 열러 있는지 모르는 상태에서 팝업을 닫게 될 경우 커서 없어지는 문제를 해결하기 위해 추가
    // 또한 Panel과 동시에 열리는 걸 막기 위해서 public 사용

    public abstract PopupType PopupType { get; }

    protected abstract void OnShow(PopupArgument popupArguments);

    protected abstract void OnHide();

    public void SetPopupHideCallback(Action callback)
    {
        this.OnPopupHide = callback;
    }

    public void Show(PopupArgument args)
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == SceneName.Game || currentScene.name == SceneName.MultiGame)
        {
            if (PanelBase.ActivePanelCount > 0)
            {
                Debug.LogWarning("현재 패널이 열려 있어서 팝업을 열 수 없습니다.");
                return;
            }
        }

        if (PanelManager.Instance.IsUILocked)
        {
            Debug.LogWarning("UI is Lock => Cannot open popup. [PopupBase]");
            return;
        }

        ActivePopupCount++;

        OnShow(args);

        // 커서 로직을 PopupBase.Show()로 이동
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Hide()
    {
        OnHide();

        if (ActivePopupCount > 0)
            ActivePopupCount--;

        Scene currentScene = SceneManager.GetActiveScene();
        if (ActivePopupCount == 0 && PanelBase.ActivePanelCount == 0)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (currentScene.name != SceneName.Lobby)
        {
            if (ActivePopupCount == 0 && PanelBase.ActivePanelCount == 0)
            {
                Debug.LogWarning("커서 락");
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        OnPopupHide?.Invoke();
    }
}