// System
using System;

// Unity
using UnityEngine;

public abstract class Panel<T> : PanelBase
{
    /// <summary>
    /// Unity callback method Awake.
    /// Set gameObject false.
    /// </summary>
    private void Awake()
    {
        // TODO : ��Ʈ��ũ ������ �������� �Ǹ� �̰� �ּ� �����ؾ� �� ���� ����.
        // ex)
        // 1. PanelManager.Instance.Show()ȣ���ϸ� �� �г��� OnShow()���� �ε� �г�(�Ǵ� �˾�) ����.
        // 2. ��Ʈ��ũ ����� �Ϸ�Ǳ� ������ scale zero.
        // 3. ��Ʈ��ũ ��� �Ϸ�Ǹ� scale (1, 1, 1)�� ����
        //transform.localScale = Vector3.zero;
    }
    protected virtual void OnEnable()
    {
        ActivePanelCount++;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    protected virtual void OnDisable()
    {
        if (PanelBase.IsHidingAll) return;

        ActivePanelCount--;
        if (ActivePanelCount == 0 && PopupBase.ActivePopupCount == 0)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}