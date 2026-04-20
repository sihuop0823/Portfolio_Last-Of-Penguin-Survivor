using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [SerializeField] private Transform slotsContainer; // 슬롯들을 담고 있는 부모 오브젝트
    [SerializeField] private Transform quickBarContainer; // 퀵바 슬롯들을 담고 있는 부모 오브젝트
    [SerializeField] private List<InventorySlotUI> slotUIs; // 모든 슬롯들 참조
    [SerializeField] private DraggableUI draggableUI;

    [SerializeField] private List<InventoryItem> startingItems;  // 시작 아이템 리스트 (test)
    [SerializeField] private InventoryUIController InventoryUIController;

    [Header("Drag")]
    public int DragCount;
    public bool isDragging;

    public float showInfoHoverTime;

    private Coroutine hoverRoutine;

    // 창고 건축물에서 참조
    public List<InventorySlotUI> SlotUIs => slotUIs;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 초기화나 슬롯 동기화 여기서 처리하면 될듯
        SlotUpdate();
    }


    public void HideAllShowInfo()
    {
        foreach(InventorySlotUI slot in slotUIs)
        {
            if (slot.TryGetComponent(out DroppableUI draggableUI))
            {
                if(draggableUI.itemInfoObject != null)
                {
                    draggableUI.itemInfoObject.SetActive(false);
                }
            }
        }
    }

    public void StartSlotHover(Action showAction)
    {
        EndSlotHover();
        hoverRoutine = StartCoroutine(Co_HoverDelay(showAction));
    }

    public void EndSlotHover()
    {
        if (hoverRoutine != null)
        {
            StopCoroutine(hoverRoutine);
            hoverRoutine = null;
        }
    }

    private IEnumerator Co_HoverDelay(Action showAction)
    {
        yield return new WaitForSeconds(showInfoHoverTime);
        showAction?.Invoke();
        hoverRoutine = null;
    }
    public void SlotUpdate()
    {
        // 슬롯들 찾아서 리스트로 저장
        foreach (InventorySlotUI slot in quickBarContainer.GetComponentsInChildren<InventorySlotUI>())
        {
            if (!slotUIs.Contains(slot)) slotUIs.Add(slot);
        }
        foreach (InventorySlotUI slot in slotsContainer.GetComponentsInChildren<InventorySlotUI>())
        {
            if (!slotUIs.Contains(slot)) slotUIs.Add(slot);
        }

        // 시작 아이템을 슬롯에 할당
        for (int i = 0; i < startingItems.Count && i < slotUIs.Count; i++)
        {
            slotUIs[i].SetItem(startingItems[i].Clone(), true);
            draggableUI = slotUIs[i].gameObject.GetComponent<DraggableUI>();
            draggableUI.enabled = true;
        }
        // 시작 아이템 그냥 슬롯 돌면서 꽂아주는 기능.
    }

    // 아이템 드롭 처리
    public void DropItem(InventorySlotUI toSlot, InventorySlotUI fromSlot)
    {
        var toItem = toSlot.GetItem();
        var fromItem = fromSlot.GetItem();
        if (toItem == null)
        {
            toSlot.SetItem(fromItem); // 아이템 놓기
            fromSlot.ClearItem(); //빈칸에 놔야하니 칸 클리어
        }
        else if (toItem.CanStackWith(fromItem))
        {
            int sumAmount = toItem.amount + fromItem.amount;
            if (sumAmount > toItem.item.maxStack)
            {
                toItem.amount = toItem.item.maxStack;
                fromItem.amount = sumAmount - toItem.item.maxStack;
                fromSlot.UpdateUI();
            }
            else
            {
                int added = toItem.Add(fromItem.amount);
                if (added == fromItem.amount) fromSlot.ClearItem();
                else fromSlot.UpdateUI();
            }
        }
        else
        {
            SwapItems(toSlot, fromSlot);
        }

        /*
         * toItem이 null이면 빈칸에 놓는거
         * 둘이 CanStackWith이으로 처리되면 합치기
         * 아니면 Swap (위치 바꾸기)
         */

        if (QuickslotNumberBtn.Instance != null && QuickslotNumberBtn.Instance.currentSlotIndex >= 0)
        {
            QuickslotNumberBtn.Instance.UpdateQuickSlot();
        }
    }

    // 두 아이템 위치 스왑
    public void SwapItems(InventorySlotUI a, InventorySlotUI b)
    {
        var tmp = a.GetItem(); // a 칸 임시저장 후 서로 복사해주는 원리
        a.SetItem(b.GetItem());
        b.SetItem(tmp);
    }

    public void AddItem(InventoryItem item)
    {
        // 아이템 추가 로직
        // 아이템이 스택 가능한지 확인
        if (!IsCanAddItem(item)) return;
        QuestProgressManager.Instance.ColletItem(item.item.itemID, item.amount);
        Debug.Log($"{item.item.itemID},{item.amount}");
        foreach (InventorySlotUI slot in slotUIs)
        {
            if (slot.currentItem == null || slot.currentItem.item == null) continue;
            InventoryItem currentItem = slot.GetItem();
            if (currentItem != null && currentItem.CanStackWith(item))
            {
                int added = currentItem.Add(item.amount);
                slot.UpdateUI(); // UI 업데이트
                if (added == item.amount)
                {
                    Debug.Log($"아이템 추가! : {item.item.itemID}");
                    QuickslotNumberBtn.Instance.UpdateQuickSlot();
                    return; // 다 추가되면 종료
                }
                QuickslotNumberBtn.Instance.UpdateQuickSlot();
                item.amount -= added; // 남은 양 업데이트
                QuickslotNumberBtn.Instance.OnChangeItem.Invoke();
            }
        }
        // 스택 가능한 슬롯이 없으면 빈 슬롯에 추가
        foreach (InventorySlotUI slot in slotUIs)
        {
            if (slot.currentItem == null || slot.currentItem.item == null)
            {
                slot.SetItem(item.Clone());
                slot.UpdateUI();
                Debug.Log($"새 칸에 아이템 추가! : {item.item.itemID}");
                QuickslotNumberBtn.Instance.UpdateQuickSlot();

                return;
            }

        }
        Debug.Log("아이템 추가 실패! | 아이템을 넣을 공간이 없습니다");
    }

    public int CountItems(string itemID)
    {
        int count = 0;
        foreach (InventorySlotUI slot in slotUIs)
        {
            if (slot.currentItem == null || slot.currentItem.item == null) continue;
            if (string.Equals(itemID, slot.currentItem.item.itemID))
            {
                count += slot.currentItem.amount; // 아이템 ID가 일치하면 수량 합산
            }
        }

        return count; // 총 아이템 수량 반환
    }

    public bool IsFullInventory()
    {
        foreach (InventorySlotUI slot in slotUIs)
        {
            if (slot.currentItem == null || slot.currentItem.item == null) return false;
        }
        return true;
    }

    public bool IsCanAddItem(InventoryItem item)
    {
        if (!IsFullInventory()) return true;

        foreach (InventorySlotUI slot in slotUIs)
        {
            var currentItem = slot.GetItem();
            if (currentItem != null && currentItem.CanStackWith(item))
            {
                return true;
            }
        }

        foreach (InventorySlotUI slot in slotUIs)
        {
            if (slot.GetItem() == null)
            {
                return true;
            }
        }

        // 3. 둘 다 없으면 인벤토리 꽉참
        return false;
    }

    public InventoryItem GetItem(string itemID)
    {
        foreach (InventorySlotUI slot in slotUIs)
        {
            if (slot.currentItem == null || slot.currentItem.item == null) continue;
            if (slot.currentItem != null && slot.currentItem.item.itemID == itemID)
            {
                return slot.currentItem; // 아이템이 있으면 반환
            }
        }

        return null; // 아이템이 없으면 null 반환
    }

    public int GetItemAmount(string targetItemID)
    {
        int totalAmount = 0;

        foreach (InventorySlotUI slot in slotUIs)
        {
            if (slot.currentItem == null || slot.currentItem.item == null) continue;

            if (slot.currentItem.item.itemID == targetItemID)
            {
                totalAmount += slot.currentItem.amount;
            }
        }

        return totalAmount;
    }

    public bool RemoveItem(InventoryItem item)
    {
        int itemCount = item.amount;

        foreach (InventorySlotUI slot in slotUIs)
        {
            if (slot.currentItem == null || slot.currentItem.item == null) continue;

            if (slot.currentItem.item.itemID == item.item.itemID)
            {
                if (slot.currentItem.amount >= itemCount)
                {
                    slot.currentItem.amount -= itemCount;
                    itemCount = 0;
                    if (slot.currentItem.amount <= 0)
                    {
                        slot.ClearItem();
                    }
                    slot.UpdateUI();
                    QuickslotNumberBtn.Instance.OnChangeItem.Invoke();
                    return true;
                }
                else
                {
                    itemCount -= slot.currentItem.amount;
                    slot.ClearItem();
                }

                slot.UpdateUI();
                QuickslotNumberBtn.Instance.OnChangeItem.Invoke();
            }
        }

        return itemCount <= 0;
    }

    public bool isHandleItem(InventoryItem item)
    {
        if (QuickslotNumberBtn.Instance.selectedItem.item == item.item && item.amount <= QuickslotNumberBtn.Instance.selectedItem.amount) return true;
        else return false;
    }

    public bool IsHaveItem(InventoryItem item)
    {
        InventoryItem tempItem = new InventoryItem(item.item, item.amount);

        foreach (InventorySlotUI slot in slotUIs)
        {
            if (slot.currentItem == null || slot.currentItem.item == null) continue;
            if (slot.currentItem != null && slot.currentItem.item.itemID == item.item.itemID)
            {
                if (slot.currentItem.amount >= item.amount)
                {
                    return true; // 아이템이 충분히 있으면 true 반환
                }
                else
                {
                    tempItem.amount -= slot.currentItem.amount;
                }
            }
        }

        return false; // 아이템이 충분하지 않으면 false 반환
    }
}
