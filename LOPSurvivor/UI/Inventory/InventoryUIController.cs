using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUIController : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private int slotCount = 20;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private Button buildingButton; // New building chat_enter_btn
    [SerializeField] private Image img_bg;

    public List<InventorySlotUI> InventorySlotList = new List<InventorySlotUI>();
    public int slotIndex; // 슬롯 번호
     
    void Awake()
    {

        for (int i = 0; i < slotCount; i++)
        {
            GameObject box = Instantiate(slotPrefab, slotsContainer);
            InventorySlotUI slot = box.GetComponent<InventorySlotUI>();
            slot.slotIndex = i + 1; // 인덱스 부여
            slot.UpdateUI(); // 초기 UI 업데이트
            InventorySlotList.Add(slot);
        }
        img_bg.enabled = true;
        inventoryUI.SetActive(false);
    }
}