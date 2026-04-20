using GlobalAudio;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MultiInputPanel : Panel<MultiInputPanel>
{
    /// <summary>
    /// Panel의 Type을 정의
    /// </summary>
    public override PanelType PanelType => PanelType.MultiInputPanel;

    /// <summary>
    /// Panel 구성에 필요한 변수들 선언
    /// </summary>
    /// 

    private LOPNetworkManager NetworkManager;

    public class Args : PanelArgument
    {

    }

    [Header("MultiInputPanel")]
    [SerializeField] private Button btn_server_open;
    [SerializeField] private Button btn_server_back;
    [SerializeField] private GameObject enter_txt;

    [Header("Connect UI")]
    [SerializeField] private GameObject connecting_obj;
    [SerializeField] private TextMeshProUGUI txt_loading;
    [SerializeField] private TextMeshProUGUI txt_tip;
    [SerializeField] private string[] tipList;

    [Header("InputField")]
    [SerializeField] private TMP_InputField IdInputField;
    [SerializeField] private TMP_InputField PortInputField;
    [SerializeField] private TMP_InputField NameInputField;

    private string IdInputFieldSum;
    private string PortInputFieldSum;
    private string NameInputFieldSum;

    private const string IdInputFieldKey = "IdInputFieldKey";
    private const string PortInputFieldKey = "PortInputFieldKey";
    private const string NameInputFieldKey = "NameInputFieldKey";

    [SerializeField] private AudioClip clickClip;

    public IPInputValidator iPInputValidator;

    private void Start()
    {
        enter_txt.SetActive(false);
        connecting_obj.SetActive(false);
        LOPNetworkManager.Instance.OnConnectionFailed += CallServerDisconnectPanel;
        LOPNetworkManager.Instance.OnConnecting += ConnectingObj;

        
    }

    private void Update()
    {
        bool isAllValid = iPInputValidator.isIpValid && iPInputValidator.isPortValid && iPInputValidator.isNicknameValid;

        if (enter_txt.activeSelf != isAllValid)
        {
            enter_txt.SetActive(isAllValid);
        }

        HandleEnterKey(isAllValid);
    }

    private void DataLoad()
    {
        IdInputField.text = PlayerPrefs.GetString(IdInputFieldKey, "");
        PortInputField.text = PlayerPrefs.GetString(PortInputFieldKey, "");
        NameInputField.text = PlayerPrefs.GetString(NameInputFieldKey, "");

        iPInputValidator.InputFieldState();
    }

    private void HandleEnterKey(bool isAllValid)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // 모든 값이 다 채워져서 유효 -> isEnterValid true
            if (isAllValid)
            {
                MultiServerEnter();
            }
            else
            {
                if (IdInputField.isFocused)
                {
                    PortInputField.Select(); // IP -> Port
                }
                else if (PortInputField.isFocused)
                {
                    NameInputField.Select(); // Port -> Name
                }
             
            }
        }
    }

    public override void OnShow(PanelArgument panelArguments)
    {
        Debug.Log($"MultiInputPanel.OnShow()");

        if (panelArguments is not Args args)
        {
            Debug.LogError($"Cannot cast panelArguments to {nameof(MultiInputPanel)}'s Args.");
            return;
        }

        btn_server_back.onClick.RemoveAllListeners();
        btn_server_back.onClick.AddListener(OnBackButtonClicked);

        btn_server_open.onClick.AddListener(MultiServerEnter);

        DataLoad();
    }

    /// <summary>
    /// Panel이 닫힐 때 처리해줘야 하는 작업을 처리
    /// ex) 네트워크 연결 중이라면 취소하기
    /// ex) 진행 중인 코루틴 등 취소하기
    /// </summary>
    /// 
    public override void OnHide()
    {

    }


    public override void OnBackButtonClicked()
    {
        AudioManager.Instance.PlaySFX(clickClip);
        Debug.Log($"MultiInputPanel.OnBackButtonClicked()");
        base.OnBackButtonClicked();
    }

    private void MultiCharacterSelectPanel()
    {
        // 멀티 캐릭터 SelectPanel 추가 되면 만들 듯
    }

    private void MultiServerEnter()
    {
        AudioManager.Instance.PlaySFX(clickClip);
        IdInputFieldSum = IdInputField.text;
        PortInputFieldSum = PortInputField.text;
        NameInputFieldSum = NameInputField.text;

        PlayerPrefs.SetString(IdInputFieldKey, IdInputField.text);
        PlayerPrefs.SetString(PortInputFieldKey, PortInputField.text);
        PlayerPrefs.SetString(NameInputFieldKey, NameInputField.text);
        PlayerPrefs.Save();

        Debug.Log($"[MultiServerEnter] IdInputFieldSum: {IdInputFieldSum}");
        Debug.Log($"[MultiServerEnter] PortInputFieldSum: {PortInputFieldSum}");
        Debug.Log($"[MultiServerEnter] NameInputFieldSum: {NameInputFieldSum}");

        StartCoroutine(ConnectWhenReady());
    }

    private IEnumerator ConnectWhenReady()
    {
        while (LOPNetworkManager.Instance == null)
            yield return null;

        LOPNetworkManager.Instance.Connect(IdInputFieldSum, PortInputFieldSum, NameInputFieldSum);
    }

    private void CallServerDisconnectPanel()
    {
       ServerConnectFailPopup.Instance.Show(new ServerConnectFailPopup.Args());
    }

    public void ConnectingObj(bool isLoading)
    {
        connecting_obj.SetActive(isLoading);

        SetRandomTip();
    }

    private IEnumerator LoadingAnim()
    {
        string baseText = "Server Loading";
        while (true)
        {

            for (int i = 0; i <= 3; i++)
            {
                txt_loading.text = baseText + new string('.', i);
                yield return new WaitForSeconds(0.2f);
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    private void SetRandomTip()
    {
        if (tipList == null || tipList.Length == 0) return;

        int randomIndex = Random.Range(0, tipList.Length);
        string randomTip = tipList[randomIndex];

        txt_tip.text = "Tip : " + randomTip;
    }

    void OnDestroy()
    {
        LOPNetworkManager.Instance.OnConnectionFailed -= CallServerDisconnectPanel;
        LOPNetworkManager.Instance.OnConnecting -= ConnectingObj;
    }
}
