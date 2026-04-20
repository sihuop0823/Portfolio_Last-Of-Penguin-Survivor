// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;
using UnityEngine.UI;

public class _01_Lobby : MonoBehaviour
{
    private void Awake()
    {
        // Fade In �� ȿ�� �߰�
        // �ʱ� �г� ����
        PanelManager.Instance.Show(PanelType.TitlePanel, new TitlePanel.Args());
    }
}