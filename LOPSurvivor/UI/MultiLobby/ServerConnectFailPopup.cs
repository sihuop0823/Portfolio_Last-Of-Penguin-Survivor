using System;
using UnityEngine;
using UnityEngine.UI;

public class ServerConnectFailPopup : Popup<ServerConnectFailPopup>
{
    public override PopupType PopupType => PopupType.ServerConnectFailPopup;
    public static ServerConnectFailPopup Instance;

    [SerializeField] private GameObject connectFailedPanel;
    [SerializeField] private Button ok_btn;

    public Action OffServerFailPopup; // InputField 처리를 위해 만드러써요

    public class Args : PopupArgument { }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        Hide();
        if (ok_btn != null) ok_btn.onClick.AddListener(Hide);
    }

    protected override void OnShow(PopupArgument popupArguments)
    {
        connectFailedPanel.SetActive(true);
    }

    protected override void OnHide()
    {
        connectFailedPanel.SetActive(false);
        OffServerFailPopup?.Invoke();
    }
}