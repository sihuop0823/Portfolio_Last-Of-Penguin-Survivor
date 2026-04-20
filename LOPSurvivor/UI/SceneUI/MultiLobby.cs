// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;
using UnityEngine.UI;

public class MultiLobby : MonoBehaviour
{
    private void Awake()
    {
        PanelManager.Instance.Show(PanelType.MultiCharacterSelectPanel, new MultiCharacterSelectPanel.Args());
    }
}