using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishingPopup : Popup<FishingPopup>
{
    public override PopupType PopupType => PopupType.FishingPopup;

    public static FishingPopup Instance { get; private set; }

    [Header("FishingUI")]
    [SerializeField] private GameObject FishingUI;
    [SerializeField] private GameObject caughtIcon;
    [SerializeField] private GameObject moveFish;

    public bool isFishingPopupOpen;

    [Header("Fishing Text")]
    [SerializeField] private TMP_Text txt_success;
    [SerializeField] private TMP_Text txt_currentFishing;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected override void OnHide()
    {
        FishingUI.SetActive(false);
        isFishingPopupOpen = false;
        caughtIcon.SetActive(false);
        txt_currentFishing.gameObject.SetActive(false);
    }
    protected override void OnShow(PopupArgument popupArguments)
    {

    }

    public void ShowWaitingState()
    {
        txt_currentFishing.gameObject.SetActive(true);
        txt_currentFishing.text = "F 를 눌러 낚시를 취소하세요";
        txt_currentFishing.color = Color.white;
    }

    public void ShowCaughtState()
    {
        caughtIcon.SetActive(true);
        txt_currentFishing.gameObject.SetActive(true);
        txt_currentFishing.text = "F 를 눌러 낚시를 시작하세요!";
        txt_currentFishing.color = Color.green;
    }

    public void ShowFishingState()
    {
        caughtIcon.SetActive(false);
        txt_currentFishing.gameObject.SetActive(false);
        isFishingPopupOpen = true;
        FishingUI.SetActive(true);

        moveFish.transform.localPosition = new Vector3(-300, 0, 0);

        txt_success.text = "F를 눌러 물고기를 잡으세요!";
        txt_success.color = Color.black;
    }

    public void ShowResult(bool isSuccess)
    {
        if (isSuccess)
        {
            txt_success.text = "성공!";
            txt_success.color = Color.green;
            isFishingPopupOpen = false;
        }
        else
        {
            txt_success.text = "실패...";
            txt_success.color = Color.red;
            isFishingPopupOpen = false;
        }
    }

    public GameObject GetMoveFish()
    {
        return moveFish;
    }

    public void OnCloseButtonClicked()
    {
        Hide();
    }

    public bool IsSuccess(float successDistance)
    {
        return Mathf.Abs(moveFish.transform.localPosition.x) <= successDistance;
    }
}