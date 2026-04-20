using UnityEngine;
using System;
using System.Collections;

using System.Collections.Generic;
public class InventoryOnOff : Popup<InventoryOnOff>
{
    public override PopupType PopupType => PopupType.InventoryPopup;

    public GameObject InventoryUI;
    [SerializeField] private GameObject QuickSlotUI;

    public static InventoryOnOff Instance;
    public static event Action OnInventoryOpen;
    public static event Action OnInventoryClose;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        QuickSlotUI.SetActive(true);
        StartCoroutine(InitialHideRoutine());
    }

    private IEnumerator InitialHideRoutine()
    {
        yield return new WaitForEndOfFrame();
        Hide();
    }

    protected override void OnShow(PopupArgument args)
    {
        InventoryUI.SetActive(true);
        if (DraggableUI.currentDraggable != null)
        {
            DraggableUI.currentDraggable.OnEndDrag(null);
        }

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.characterController != null)
            {
                GameManager.Instance.characterController.ChangeCharacterState(CharacterState.UsingUI);

            }
        }
        OnInventoryOpen?.Invoke();
    }

    protected override void OnHide()
    {
        InventoryUI.SetActive(false);
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.characterController != null)
            {
                GameManager.Instance.characterController.ChangeCharacterState(CharacterState.Alive);
            }
        }
      
        InventoryManager.Instance.HideAllShowInfo();
        OnInventoryClose?.Invoke();
    }
}