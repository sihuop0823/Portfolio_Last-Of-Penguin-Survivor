using UnityEngine;

public class ShipyardPanel : ManufacturingPanel
{
    public override PanelType PanelType => PanelType.Shipyard;

    protected override void MakeItem()
    {
        // 제작에 필요한 재료의 개수를 확인
        for (int i = 0; i < recipe.needItems.Length; i++)
        {
            // 제작할 아이템에 필요한 재료의 종류와 개수를 확인
            ItemDatabase needItemData = ItemGenerator.Instance.GetItemData(recipe.needItems[i].itemId);
            int needItemAmount = recipe.needItems[i].amount * makeCount;

            // 인벤토리에 충분한 개수의 재료가 있는지 확인
            if (!InventoryManager.Instance.IsHaveItem(new InventoryItem(needItemData, needItemAmount)))
            {
                // 재료가 부족할 경우 로그 출력
                Debug.LogError($"재료 부족: {needItemData.itemName} x{needItemAmount}");
                return;
            }
        }

        // 재료를 소비하여 아이템 제작
        for (int i = 0; i < recipe.needItems.Length; i++)
        {
            InventoryItem item = new InventoryItem(
                ItemGenerator.Instance.GetItemData(recipe.needItems[i].itemId),
                recipe.needItems[i].amount * makeCount
            );

            // 인벤토리에서 재료가 제거되었는지 확인
            bool removed = InventoryManager.Instance.RemoveItem(item);
            if (!removed)
            {
                // 재료가 제거 되지 않으면 오류 로그 출력
                Debug.LogError($"재료 제거 실패: {item.item.itemName}");
                return;
            }
        }
        building.GetComponent<ShipyardBuilding>().ShowShip();
        PanelManager.Instance.HideAll();

        ChangedMakeCount();
    }
}
