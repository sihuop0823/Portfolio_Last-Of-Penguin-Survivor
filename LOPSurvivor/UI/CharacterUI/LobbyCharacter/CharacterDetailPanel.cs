using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Collections;

public class CharacterDetailPanel : Panel<CharacterDetailPanel>
{
    public override PanelType PanelType => PanelType.CharacterDetailPanel;

    public class Args : PanelArgument
    {
        public CharacterLobbyData characterData;
    }

    [Header("CharacterDetailPanel")]
    [SerializeField] private Image img_main;
    [SerializeField] private TMP_Text txt_name;
    [SerializeField] private TMP_Text txt_quote;
    [SerializeField] private TMP_Text txt_trait;
    [SerializeField] private Slider slider_hp;
    [SerializeField] private Slider slider_hunger;
    [SerializeField] private Slider slider_vitality;
    [SerializeField] private Slider slider_speed;

    [SerializeField] private Button btn_detail_back = null;

    [SerializeField] private AudioClip clickClip;
    private float Round(float v) => Mathf.Round(v * 5f) / 5f;

    public override void OnShow(PanelArgument panelArguments)
    {
        if (panelArguments is not Args args)
        {
            Debug.LogError($"Cannot cast panelArguments to {nameof(CharacterDetailPanel)}'s Args.");
            return;
        }

        UpdateUI(args.characterData);

        btn_detail_back.onClick.RemoveAllListeners();
        btn_detail_back.onClick.AddListener(OnBackButtonClicked);

        //TODO : UI ó���� ������ �ϸ� �ɵ�
    }

    public override void OnHide()
    {

    }

    public override void OnBackButtonClicked()
    {
        Debug.Log("CharacterDetailPanel: Back button clicked");
        base.OnBackButtonClicked();
    }

    public void UpdateUI(CharacterLobbyData data)
    {

        if (data == null)
        {
            Debug.LogError("Null data");
            return;
        }

        img_main.sprite = data.image;
        txt_name.text = data.displayName;
        txt_quote.text = data.quote;
        txt_trait.text = data.traitDescription;

        UnitCode unitCode = (UnitCode)data.penguinId;
        CharacterStat stat = CharacterStat.Create(unitCode);
        slider_hp.value = Round(stat.maxHp / 150f);
        slider_vitality.value = Round(data.vitality);

        slider_hunger.value = Round(stat.maxHunger / 150f);
        slider_speed.value = Round(stat.moveSpeed / 8);


        /* TODO : �г��� �Ѵ� Prefab�̿��� CharacterSelectPanel ���� [SerializeField]�� �����ؼ� ���⼭ ó���� ���ϴ°� �ƽ���..��
         �̰͵� �𸣰ھ GPT�� ������ ���� �޾Ҵ�.*/
    }

    private void OnLoadingPanel()
    {
        PanelManager.Instance.Show(PanelType.LoadingPanel, new LoadingPanel.Args("_02_Game"));
    }
}
