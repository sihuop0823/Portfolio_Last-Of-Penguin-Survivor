using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StorageBuildingPanel : Panel<StorageBuildingPanel>
{
    public override PanelType PanelType => PanelType.WarehousePanel;

    public class Args : PanelArgument
    {
        public StorageBuilding storageBuilding;
    }

    private StorageBuilding building;
    private int maxStorageAmount;
    private int currentStorageAmount;

    [SerializeField] private Button btn_tap;

    [SerializeField] private GameObject tap;

    private GameObject prevTap;
    private Button prevBtn;

    [SerializeField] private List<StorageBuildingButton> inventoryButtons;

    [SerializeField] private List<StorageBuildingButton> storageButtons;
    [SerializeField] private TMP_Text txt_storageAmount;

    [SerializeField] private Button btn_back;

    public void TryMove(InventoryItem item, int index, bool isInventory)
    {
        if (isInventory)
        {
           MoveToStorage(item, index);
        }
        else
        {
            MoveToInventory(item, index);
        }
    }

  
    public void MoveToInventory(InventoryItem item, int index)
    {
        if (item == null || InventoryManager.Instance.IsFullInventory()) return;

        InventoryManager.Instance.AddItem(new InventoryItem(item.item, item.amount));

        if (LOPNetworkManager.Instance.isConnected == true)
        {
            LOPNetworkManager.Instance.RPC(building, "RemoveItem", index);
        }
        else if(LOPNetworkManager.Instance.isConnected == false)
        {
            building.StorageItemList.RemoveAt(index);
            InitButtons();
        }
    }


    // 나중에 stack형식이 아니라 마크처럼 index를 받아와야 할 수도 있을 때
    public void MoveToStorage(InventoryItem item, int index)
    {
        if (item == null || item.item == null)
        {
            return;
        }
        string itemIDToStore = item.item.itemID;
        int amountToStore = item.amount;

        ItemDatabase itemData = item.item;

        InventoryManager.Instance?.RemoveItem(item);

        if (LOPNetworkManager.Instance.isConnected == true)
        {
            LOPNetworkManager.Instance.RPC(building, "AddItem", itemIDToStore, amountToStore);
        }
        else if (LOPNetworkManager.Instance.isConnected == false)
        {
            InventoryItem newItemForStorage = new InventoryItem(itemData, amountToStore);
            building.TryAddItem(newItemForStorage);
            InitButtons();
        }
    }

    public override void OnShow(PanelArgument panelArguments)
    {
        if (panelArguments is not Args args)
        {
            Debug.LogError($"Cannot cast panelArguments to {nameof(CharacterDetailPanel)}'s Args.");
            return;
        }

        building = args.storageBuilding;
        maxStorageAmount = building.MaxStorageAmount;
        building.currentPanel = this;

        // 인벤토리 슬롯 초기화
        InitButtons();

        // 버튼
        AddListeners();
    }

    [LopRPC]
    public void InitButtons()
    {
        List<InventorySlotUI> inventoryItemSlots = InventoryManager.Instance.SlotUIs;
        for (int i = 0; i < inventoryItemSlots.Count; i++)
        {
            inventoryButtons[i].InitData(inventoryItemSlots[i].currentItem, i);
        }

        // 기존 상자 안에 아이템들 초기화
        for (int i = 0; i < storageButtons.Count; i++)
        {
            storageButtons[i].InitData(building.StorageItemList.Count <= i ? null : building.StorageItemList[i], i);
        }

        InitAmount();
    }

    private void InitAmount()
    {
        currentStorageAmount = building.StorageItemList.Count;

        txt_storageAmount.text = currentStorageAmount + " / " + maxStorageAmount;
    }

    private void AddListeners()
    {
        prevBtn = btn_tap;
        prevTap = tap;

        btn_tap.onClick.AddListener(() =>
        {
            prevBtn.targetGraphic.color = Color.gray;
            prevTap.SetActive(false);

            btn_tap.targetGraphic.color = Color.white;
            tap.SetActive(true);
            prevTap = tap;
        });

        btn_back.onClick.AddListener(PanelManager.Instance.HideAll);
    }

    public override void OnHide()
    {

    }

   
}