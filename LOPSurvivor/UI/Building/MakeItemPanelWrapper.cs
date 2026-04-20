using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class NeedItemUIWrapper
{
    public GameObject gameObject;
    public Image img_icon;
    public TMP_Text txt_amount;
}

[Serializable] // 기존 MakeItemButton에서 사용되던 변수들
public class ItemButtonData
{
    [Header("Button Components")]
    public Button button;
    public GameObject outline;
    public Image icon;
    public TMP_Text nameText;
    
    [Header("Recipe Data")]
    public ItemRecipeDatabase recipe;
    
    [Header("State")]
    public bool isSelected;
}

/// <summary>
/// MakeItemButton의 기능을 포함한 통합된 MakeItemPanel
/// 기존 MakeItemButton과 MakeItemPanel의 기능을 하나로 통합
/// </summary>
public abstract class MakeItemPanelWrapper : MakeItemPanel
{
    [Header("Item Buttons (Integrated MakeItemButton functionality)")]
    [SerializeField] private ItemButtonData[] itemButtons;
    [SerializeField] private Transform buttonContainer;

    // 상태 변수
    protected ItemButtonData selectedButton;

    /// <summary>
    /// 아이템 버튼을 선택했을 때 호출 (기존 MakeItemButton.OnClick 기능)
    /// </summary>
    /// <param name="buttonData">선택된 버튼 데이터</param>
    public virtual void SelectItemButton(ItemButtonData buttonData)
    {
        // 이전 선택 해제
        if (selectedButton != null)
        {
            selectedButton.outline.SetActive(false);
            selectedButton.isSelected = false;
        }

        // 새 선택
        selectedButton = buttonData;
        buttonData.outline.SetActive(true);
        buttonData.isSelected = true;

        // 아이템 정보 표시
        DisplayItemInfo(buttonData.recipe);
        
        // 버튼 선택 이벤트 (하위 클래스에서 오버라이드 가능)
        OnItemButtonSelected(buttonData);
    }

    /// <summary>
    /// 아이템 정보를 패널에 표시 (기존 MakeItemPanel.SelectOne 기능)
    /// </summary>
    /// <param name="itemRecipe">선택된 아이템 레시피</param>
    protected virtual void DisplayItemInfo(ItemRecipeDatabase itemRecipe)
    {
        makeItemData = ItemGenerator.Instance.GetItemData(itemRecipe.returnItemId);
        recipe = itemRecipe;

        img_item.sprite = makeItemData.icon;
        txt_itemName.text = makeItemData.itemName;
        txt_itemInfo.text = makeItemData.itemDescription;

        makeCount = 1;
        ChangedMakeCount();
    }

    // 기존에 MakeItemButton 구성 요소에 대한 호환성
    public override void SelectOne(MakeItemButton clickingButton, ItemRecipeDatabase itemRecipe)
    {
        // 기본 필드가 있는 경우 이전 개요 숨기기
        if (clickedMakeButton != null && clickedMakeButton != clickingButton)
        {
            clickedMakeButton.HideOutline();
        }

        clickedMakeButton = clickingButton;
        DisplayItemInfo(itemRecipe);
    }

    /// <summary>
    /// 버튼 선택 시 추가 처리 (하위 클래스에서 오버라이드 가능)
    /// </summary>
    /// <param name="buttonData">선택된 버튼 데이터</param>
    protected virtual void OnItemButtonSelected(ItemButtonData buttonData)
    {
        // 하위 클래스에서 특별한 처리가 필요한 경우 오버라이드
    }

    /// <summary>
    /// 동적으로 아이템 버튼 추가
    /// </summary>
    /// <param name="itemRecipe">추가할 아이템 레시피</param>
    /// <param name="buttonPrefab">버튼 프리팹</param>
    public virtual void AddItemButton(ItemRecipeDatabase itemRecipe, GameObject buttonPrefab)
    {
        GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
        
        // 버튼 컴포넌트 설정
        Button button = buttonObj.GetComponent<Button>();
        GameObject outline = buttonObj.transform.Find("Outline")?.gameObject;
        Image icon = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
        TMP_Text nameText = buttonObj.transform.Find("NameText")?.GetComponent<TMP_Text>();

        if (button != null)
        {
            // 버튼 데이터 생성
            ItemButtonData buttonData = new ItemButtonData
            {
                button = button,
                outline = outline,
                icon = icon,
                nameText = nameText,
                recipe = itemRecipe,
                isSelected = false
            };

            // 버튼 클릭 이벤트 연결
            button.onClick.AddListener(() => SelectItemButton(buttonData));

            // UI 설정
            SetupButtonUI(buttonData);
        }
    }

    /// <summary>
    /// 버튼 UI 설정
    /// </summary>
    /// <param name="buttonData">설정할 버튼 데이터</param>
    protected virtual void SetupButtonUI(ItemButtonData buttonData)
    {
        if (buttonData.recipe == null) return;

        var itemData = ItemGenerator.Instance.GetItemData(buttonData.recipe.returnItemId);
        
        if (buttonData.icon != null)
            buttonData.icon.sprite = itemData.icon;
        
        if (buttonData.nameText != null)
            buttonData.nameText.text = itemData.itemName;
    }

    /// <summary>
    /// 정적 버튼들 초기화
    /// </summary>
    protected virtual void InitializeItemButtons()
    {
        foreach (var buttonData in itemButtons)
        {
            if (buttonData.button != null)
            {
                buttonData.button.onClick.AddListener(() => SelectItemButton(buttonData));
                SetupButtonUI(buttonData);
            }
        }
    }

    public override void OnShow(PanelArgument panelArguments)
    {
        // 베이스에서 기존 버튼/입력 리스너를 연결하도록 맡깁니다.
        base.OnShow(panelArguments);
        WireLegacyMakeItemButtons();
        InitializeItemButtons();
    }

    private void WireLegacyMakeItemButtons()
    {
        // 기존 MakeItemButton 컴포넌트가 있을 경우, private 필드에 이 패널을 주입하여 NRE 방지
        var legacyButtons = GetComponentsInChildren<MakeItemButton>(true);
        if (legacyButtons == null || legacyButtons.Length == 0) return;

        FieldInfo panelField = typeof(MakeItemButton).GetField("makeItemPanel", BindingFlags.NonPublic | BindingFlags.Instance);
        if (panelField == null) return;

        foreach (var legacyButton in legacyButtons)
        {
            try
            {
                // 이미 지정되어 있으면 건너뜀
                var current = panelField.GetValue(legacyButton) as MakeItemPanel;
                if (current == null)
                {
                    panelField.SetValue(legacyButton, this);
                }
            }
            catch { }
        }
    }

    // 베이스의 OnClickAddListeners 사용 (override 제거)

    protected override void MakeItem()
    {
        if (recipe == null)
        {
            Debug.LogWarning("제작할 아이템이 선택되지 않았습니다.");
            return;
        }

        if (InventoryManager.Instance.IsFullInventory())
        {
            Debug.Log("인벤토리가 꽉참");
            return;
        }

        // 재료 소모
        for (int i = 0; i < recipe.needItems.Length; i++)
        {
            InventoryItem item = new InventoryItem(
                ItemGenerator.Instance.GetItemData(recipe.needItems[i].itemId), 
                recipe.needItems[i].amount * makeCount);
            InventoryManager.Instance.RemoveItem(item);
        }

        // 완성품을 인벤토리에 추가
        InventoryManager.Instance.AddItem(new InventoryItem(makeItemData, makeCount));

        // 제작 완료 이벤트 (하위 클래스에서 오버라이드 가능)
        OnItemMade(makeItemData, makeCount);
        
        // 재료 소모 후 UI 업데이트
        ChangedMakeCount();
    }

    /// <summary>
    /// 아이템 제작 완료 시 호출 (하위 클래스에서 오버라이드 가능)
    /// </summary>
    /// <param name="itemData">제작된 아이템 데이터</param>
    /// <param name="count">제작된 수량</param>
    protected virtual void OnItemMade(ItemDatabase itemData, int count)
    {
        QuestProgressManager.Instance.CraftItem(itemData.itemID, count);
        // 하위 클래스에서 특별한 처리가 필요한 경우 오버라이드
    }

    /// <summary>
    /// 선택된 버튼의 아웃라인 숨기기 (기존 MakeItemButton.HideOutline 기능)
    /// </summary>
    public virtual void HideSelectedOutline()
    {
        if (selectedButton != null)
        {
            selectedButton.outline.SetActive(false);
            selectedButton.isSelected = false;
            selectedButton = null;
        }
    }

    /// <summary>
    /// 모든 버튼의 선택 상태 초기화
    /// </summary>
    public virtual void ClearAllSelections()
    {
        foreach (var buttonData in itemButtons)
        {
            if (buttonData != null)
            {
                buttonData.outline.SetActive(false);
                buttonData.isSelected = false;
            }
        }
        
        selectedButton = null;
    }

    public override void OnHide()
    {
        // 패널 숨김 시 정리 작업
        HideSelectedOutline();
    }
}


