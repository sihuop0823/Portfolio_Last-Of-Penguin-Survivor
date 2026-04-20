using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private bool isOnQuickSlot;
    [SerializeField] private Image slotImage; // 아이템 아이콘 이미지
    [SerializeField] private TMP_Text amountText; // 수량 텍스트
    [SerializeField] private Sprite emptyFrame; // 빈 슬롯일 때 보여줄 프레임

    public InventoryItem currentItem; // 현재 슬롯에 들어있는 가상 아이템

    public Image IconImage => slotImage; // 드래그 프리뷰용 외부 접근자

    public int slotIndex;

    // 슬롯 UI 갱신
    public void UpdateUI()
    {
        if (currentItem == null || currentItem.item == null) //슬롯이 빈 경우 && 아이템 정보가 없는 경우
        {
            slotImage.sprite = emptyFrame;
            slotImage.color = Color.white;
            if (amountText != null) amountText.text = "";
        }
        else //slotImage에 Icon 할당 및 AmountText 표기
        {
            slotImage.sprite = currentItem.item.icon;
            slotImage.color = Color.white;
            GetComponent<DraggableUI>().enabled = true; // 드래그 가능하게 설정
            if (amountText != null)
                amountText.text = currentItem.amount > 1 ? currentItem.amount.ToString() : "";
        }
    }

    // 아이템을 슬롯에 설정
    public void SetItem(InventoryItem item, bool isForce = false)
    {
        currentItem = item;
        UpdateUI();
        if(isOnQuickSlot && !isForce)
        {
            GetComponentInParent<QuickslotNumberBtn>().InitSelectedSlot();
        }
    }

    // 슬롯 비우기
    public void ClearItem()
    {
        currentItem = null;
        UpdateUI();
        QuickslotNumberBtn.Instance.OnChangeItem.Invoke();
    }

    // 현재 아이템 반환 (외부에서 조회)
    public InventoryItem GetItem() => currentItem; 
}
