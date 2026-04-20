using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 원재료만을 사용해 가공목적의 건축물 UI (제공소, 제련소)
// MakingBuilding에서 내구도 고치는 시스템만 추가됨
public abstract class ManufacturingPanel : MakingBuildingPanel
{
    [Header("Manufacturing Panel")]
    [SerializeField] private Button btn_make_tap;
    [SerializeField] private Button btn_fix_tap;

    [SerializeField] private GameObject makeTap;
    [SerializeField] private GameObject fixTap;

    [SerializeField] private TMP_Text txt_durabilityPercent;
    [SerializeField] private Image img_durabilityFill;

    [SerializeField] private Button btn_fix;

    [SerializeField] private TMP_Text needItemText;
    // 각각의 패널들에서 만들 수 있는 아이템의 재료
    [SerializeField] private NeedItemUI[] fixNeedItemUIs;

    protected Action onClickTap;

    public override void OnShow(PanelArgument panelArguments)
    {
        base.OnShow(panelArguments);

        if (building.IsDestroy)
        {
            btn_fix_tap.onClick.Invoke();

            btn_make_tap.interactable = false;
        }

        float percent = building.Durability / (float)building.Data.maxDurability;

        txt_durabilityPercent.text = (percent * 100).ToString() + "%";
        img_durabilityFill.fillAmount = percent;

        ShowNeedText();
    }

    protected override void OnClickAddListeners()
    {
        base.OnClickAddListeners();

        onClickTap += () =>
        {
            btn_make_tap.targetGraphic.color = Color.gray;
            btn_fix_tap.targetGraphic.color = Color.gray;

            fixTap.SetActive(false);
            makeTap.SetActive(false);
        };

        btn_make_tap.onClick.AddListener(() =>
        {
            onClickTap.Invoke();

            btn_make_tap.targetGraphic.color = Color.white;
            makeTap.SetActive(true);

        });
        btn_fix_tap.onClick.AddListener(() =>
        {
            onClickTap.Invoke();

            btn_fix_tap.targetGraphic.color = Color.white;
            fixTap.SetActive(true);
        });

        btn_fix.onClick.AddListener(() =>
        {
            FixDurability();
        });
    }


    private void ShowNeedText()
    {
        if (needItemText != null)
        {
            //string text = "";
            //for (int i = 0; i < building.Data.fixItems.Length; i++)
            //{
            //    string itemName = ItemGenerator.Instance.GetItemData(building.Data.fixItems[i].itemId).itemName;
            //    int amount = building.Data.fixItems[i].amount;
            //    text += $"{itemName} x{amount}, ";

            //}

            for (int i = 0; i < building.Data.fixItems.Length; i++)
            {
                if (fixNeedItemUIs[i].gameObject.activeSelf) fixNeedItemUIs[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < building.Data.fixItems.Length; i++)
            {
                fixNeedItemUIs[i].gameObject.SetActive(true);

                ItemDatabase needItemData = ItemGenerator.Instance.GetItemData(building.Data.fixItems[i].itemId);
                int needItemAmount = building.Data.fixItems[i].amount;

                fixNeedItemUIs[i].img_icon.sprite = needItemData.icon;

                int currentAmount = InventoryManager.Instance.GetItemAmount(needItemData.itemID);

                if (currentAmount >= needItemAmount)
                {
                    fixNeedItemUIs[i].txt_amount.text = $"{currentAmount} / {needItemAmount}";
                    fixNeedItemUIs[i].txt_amount.color = Color.black;
                }
                else
                {
                    fixNeedItemUIs[i].txt_amount.text = $"{currentAmount} / {needItemAmount}";
                    fixNeedItemUIs[i].txt_amount.color = Color.red;
                }
            }
        }
    }

    private void FixDurability()
    {
        Debug.Log("건축물 수리 시도. 내구도: " + building.Durability);
        for (int i = 0; i < building.Data.fixItems.Length; i++)
        {
            string requiredId = building.Data.fixItems[i].itemId;
            int requiredAmount = building.Data.fixItems[i].amount;

            int currentAmount = InventoryManager.Instance.GetItemAmount(requiredId);

            if (currentAmount < requiredAmount)
            {
                Debug.Log($"재료 부족. (ID: {requiredId}, 필요: {requiredAmount}, 보유: {currentAmount})");

                return;
            }
        }

        building.RepairDurability();
        QuestPanel.Instance.IncreaseProgress("MQ_Antarctica_5_1", 1);

        for (int i = 0; i < building.Data.fixItems.Length; i++)
        {
            InventoryItem item = new InventoryItem(
                ItemGenerator.Instance.GetItemData(building.Data.fixItems[i].itemId),
                building.Data.fixItems[i].amount
            );
            InventoryManager.Instance.RemoveItem(item);
        }


        float percent = building.Durability / (float)building.Data.maxDurability;
        Debug.Log(percent);
        txt_durabilityPercent.text = (percent * 100).ToString() + "%";
        img_durabilityFill.fillAmount = percent;

        btn_make_tap.interactable = true;
        Debug.Log("건축물 수리 완료. 내구도: " + building.Durability);
        // 수리 비율 예시: 내구도 75 % → 25 % 수리 → 원재료 5개 * 3(1배율 기준)
        // ?? 뭔말인지 물어보기

        // 팝업을 열어야돼
        // 수리 필요 재화랑
        // 현재 재화
    }
}
