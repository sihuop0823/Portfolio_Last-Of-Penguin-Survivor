using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class QuickslotNumberBtn : MonoBehaviour
{
    public static QuickslotNumberBtn Instance { get; private set; }

    [SerializeField] private Image[] quickSlotImages;
    public InventorySlotUI[] inventorySlots;

    public UnityEvent OnChangeItem; // ���� ���� �̺�Ʈ (�ɼ�)
    public InventoryItem selectedItem; // ���� ���õ� ������ (�ɼ�)

    public bool WearableParka;
    public UnityEvent OnWearableParka;

    [Header("Color Set")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;

    public bool cantChange = false;

    public int currentSlotIndex { get; private set; } = -1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // ������ �� ��� ���� normalColor �ʱ�ȭ
        for (int i = 0; i < quickSlotImages.Length; i++)
            quickSlotImages[i].sprite = normalSprite;
    }

    void Update()
    {
        if(cantChange || (WorldInputManager.Instance.gameInputType != WorldInputManager.GameInputType.isOpenMinimap && WorldInputManager.Instance.gameInputType != WorldInputManager.GameInputType.None)) return;
        if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
        {
            float scroll = Input.mouseScrollDelta.y;

            if (scroll > 0f) ScrollQuickBar(-1);  // ���� = ���� ���� (���⿡ ���� �ݴ��)
            else if (scroll < 0f) ScrollQuickBar(+1);  // �Ʒ��� = ���� ����


        }
        ChangeQuickBar(); // Ű �Է¿� ���� ������ ����
    }




    private void ChangeQuickBar()
    {
        if (WorldInputManager.Instance.gameInputType != WorldInputManager.GameInputType.isOpenMinimap && WorldInputManager.Instance.gameInputType != WorldInputManager.GameInputType.None) return;
        // 1~9
        for (int i = 0; i < 9 && i < quickSlotImages.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
                //Debug.Log(i+1);
                return;
            }
        }
        // 0
        if (quickSlotImages.Length > 9 && Input.GetKeyDown(KeyCode.Alpha0))
        {
            SelectSlot(9);
            //Debug.Log("0");
            return;
        }
        // -
        if (quickSlotImages.Length > 10 && Input.GetKeyDown(KeyCode.Minus))
        {
            SelectSlot(10);
            //Debug.Log("-");
            return;
        }
        // =
        if (quickSlotImages.Length > 11 && Input.GetKeyDown(KeyCode.Equals))
        {
            SelectSlot(11);
            //Debug.Log("=");
            return;
        }
    }

    public bool IsParkaEquipped()
    {
        return WearableParka;
    }

    public void InitSelectedSlot()
    {
        if (currentSlotIndex >= 0 && currentSlotIndex < quickSlotImages.Length)
            quickSlotImages[currentSlotIndex].sprite = normalSprite;

        // �� ����
        quickSlotImages[currentSlotIndex].sprite = selectedSprite;

        selectedItem = inventorySlots[currentSlotIndex].currentItem;
        OnChangeItem?.Invoke(); // ���� ���� �̺�Ʈ ȣ�� (�ɼ�)
    }

    private void SelectSlot(int index)
    {
        // �̹� ���õ� ������ �ٽ� ������ ����
        if (currentSlotIndex == index)
        {
            quickSlotImages[currentSlotIndex].sprite = normalSprite;
            currentSlotIndex = -1; // �ƹ� �͵� ���� �� �� ����
            selectedItem = null;
            OnChangeItem?.Invoke(); // ���� ���� �̺�Ʈ ȣ�� (�ɼ�)
            return;
        }

        // ���� ���� ����
        if (currentSlotIndex >= 0 && currentSlotIndex < quickSlotImages.Length)
            quickSlotImages[currentSlotIndex].sprite = normalSprite;

        // �� ����
        currentSlotIndex = index;
        quickSlotImages[currentSlotIndex].sprite = selectedSprite;

        selectedItem = inventorySlots[currentSlotIndex].currentItem;
        OnChangeItem?.Invoke(); // ���� ���� �̺�Ʈ ȣ�� (�ɼ�)
    }

    public void UpdateQuickSlot()
    {
        if (currentSlotIndex >= 0 && currentSlotIndex < quickSlotImages.Length)
        {
            quickSlotImages[currentSlotIndex].sprite = normalSprite;
        }

        if (currentSlotIndex >= inventorySlots.Length)
        {
            return;
        }

        quickSlotImages[currentSlotIndex].sprite = selectedSprite;

        selectedItem = inventorySlots[currentSlotIndex].currentItem;
        OnChangeItem?.Invoke();
    }

    private void ScrollQuickBar(int direction)
    {
        if (quickSlotImages == null || quickSlotImages.Length == 0) return;

        // �ƹ� �͵� ���� �� �� ���¸� 0���� ����
        int next = currentSlotIndex < 0 ? 0 : currentSlotIndex;

        // ȸ��(����)
        next = (next + direction) % quickSlotImages.Length;
        if (next < 0) next += quickSlotImages.Length;

        SelectSlot(next);
    }
}