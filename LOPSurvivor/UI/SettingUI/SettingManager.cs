using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance { get; private set; }

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

    //private void Update()
    //{
    //    Scene currentScene = SceneManager.GetActiveScene();

    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        if (currentScene.name == SceneName.Game || currentScene.name == SceneName.MultiGame)
    //        {
    //            if (PanelBase.ActivePanelCount > 0)
    //            {
    //                Debug.Log("패널이 열려 있어 SettingPopup을 열 수 없습니다.");
    //                return;
    //            }
    //        }

    //        if (SettingPopup.Instance.IsSettingOpen())
    //        {
    //            SettingPopup.Instance.Hide();
    //        }
    //        else
    //        {
    //            if (PopupBase.ActivePopupCount > 0)
    //            {
    //                Debug.Log("다른 팝업(인벤토리)이 열려 있어 SettingPopup을 열 수 없습니다.");
    //                return;
    //            }
    //            SettingPopup.Instance.Show(null);
    //        }
    //    }
    //}
}
