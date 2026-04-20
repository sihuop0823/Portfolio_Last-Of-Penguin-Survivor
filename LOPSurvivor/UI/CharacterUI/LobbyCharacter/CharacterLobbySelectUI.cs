using GlobalAudio;
using Lop.Survivor;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CharacterLobbySelectUI : MonoBehaviour
{
    [Header("CharacterSelectPanel")]
    [SerializeField] private Image img_main;
    [SerializeField] private TMP_Text txt_name;
    [SerializeField] private TMP_Text txt_quote;
    [SerializeField] private Slider slider_hp;
    [SerializeField] private Slider slider_hunger;
    [SerializeField] private Slider slider_speed;

    [SerializeField] private Button btn_detail;
    [SerializeField] private Button btn_survivor_start;
    [SerializeField] private GameObject penguin;
    [SerializeField]  private GameObject camera;
    [SerializeField] private Vector3[] penguinPos;

    [SerializeField] private CharacterLobbyData selected;
    [SerializeField] private AudioClip clickClip;
    private void Start()
    {
        //btn_detail.interactable = false;
        //btn_survivor_start.interactable = false;

        btn_detail.gameObject.SetActive(false);
        btn_survivor_start.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        btn_survivor_start.onClick.RemoveAllListeners();
        btn_survivor_start.onClick.AddListener(() =>
        {
            if (selected != null)
            {
                AudioManager.Instance.PlaySFX(clickClip);
                PanelManager.Instance.Show(PanelType.LoadingPanel, new PanelArgument()); // 캐릭 안고르면 비활
                GameManager.Instance.isSpawn = true;
                WorldInputManager.Instance.isLoading = true;
            }
        });
        if(camera == null)
        {
            camera = GameObject.FindGameObjectWithTag("RenderCamera");
        }
    }

    public void SelectCharacter(CharacterLobbyData data)
    {
        selected = data;
        Debug.Log($"SelectCharacter: {data.displayName}");
        img_main.sprite = data.image;
        txt_name.text = data.displayName;
        txt_quote.text = data.quote;

        UnitCode unitCode = (UnitCode)data.penguinId;
        CharacterStat stat = CharacterStat.Create(unitCode);
        slider_hp.value = Round(stat.maxHp / 150f);

        slider_hunger.value = Round(stat.maxHunger/ 150f);
        slider_speed.value = Round(stat.moveSpeed / 8);

        AudioManager.Instance.PlaySFX(clickClip);

        btn_detail.gameObject.SetActive(true); // 캐릭터 선택시 활성화
        btn_survivor_start.gameObject.SetActive(true); //캐릭 선택시 게임 시작 활성화
        camera.transform.position = penguinPos[data.penguinId];
        GameManager.Instance.Setpengin(data.penguinId);
        OverCharacterInfoInGame(data);

    }
    private float Round(float v) => Mathf.Round(v * 5f) / 5f; // slider가 1이 최대니까 0.2 단위로 쪼개주기!

    public void OpenDetail()
    {
        if (selected == null)
        {
            Debug.LogWarning("No Choice Character");
            return;
        }
        AudioManager.Instance.PlaySFX(clickClip);

        PanelManager.Instance.Show(PanelType.CharacterDetailPanel, new CharacterDetailPanel.Args
        {
            characterData = selected
        });

    }
    /// <summary>
    /// 인게임으로 선택한 캐릭터 Prefab하고 정보 넘기는 함수
    /// </summary>
    private void OverCharacterInfoInGame(CharacterLobbyData data)
    {
        // TODO :: 일단 GameManager가 없다고 뜨고 있습니다.
        // 또한 지금 당장은 캐릭터를 선택하고 그 3d 프리펩과 캐릭터 능력치를 넘겨줄 필요는 없다 생각하여 주석처리 해놓았습니다

        /*if (GameManager.Instance == null || data == null) return;

        GameManager.Instance.pengin = data.penguin3dPrefab;
        GameManager.Instance.penuginId = data.penguinId;*/
    }
}
