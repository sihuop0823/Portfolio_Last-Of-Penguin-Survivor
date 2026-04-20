// Unity
using GlobalAudio;
using UnityEngine;

public abstract class PanelBase : MonoBehaviour
{
    public abstract PanelType PanelType { get; }

    public static int ActivePanelCount = 0; //Popup���� ó���� ���� Public

    public static bool IsHidingAll = false;

    /// <summary>
    /// Called when BatteryPanel was shown.
    /// </summary>
    /// <param name="panelArguments"></param>
    public abstract void OnShow(PanelArgument panelArguments);

    /// <summary>
    /// Called when BatteryPanel was hided.
    /// </summary>
    public abstract void OnHide();

    public virtual void OnBackButtonClicked()
    {
        PanelManager.Instance.Hide();
    }
}