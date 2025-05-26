using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private TextMeshProUGUI itemAmountText;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Image itemImage;
    [SerializeField] private Image selectedImage;
    [SerializeField] private Image dirtyIndicator;

    private Animator animator;
    private InventorySlot inventorySlot;

    private void Awake() {
        GetComponent<Button>().onClick.AddListener(() => {
            SlotClicked();
        });

        animator = GetComponent<Animator>();
    }

    public void SlotClicked() {
        InventoryManager.Instance.SlotClicked(inventorySlot);
    }

    public void SetInventorySlot(InventorySlot inventorySlot) {
        this.inventorySlot = inventorySlot;

        inventorySlot.OnItemChanged += InventorySlot_OnItemChanged;
        inventorySlot.OnQuantityChanged += InventorySlot_OnQuantityChanged;
        inventorySlot.OnDirtyChanged += InventorySlot_OnDirtyChanged;

        InventorySlot_OnItemChanged();
        InventorySlot_OnDirtyChanged();
    }

    public InventorySlot GetInventorySlot() {
        return inventorySlot;
    }

    public void SetSelected(bool selected) {
        selectedImage.gameObject.SetActive(selected);
        if (selected) {
            if (animator == null) return;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName("SlotHover")) {
                animator.SetTrigger("Hover");
            }
        }
    }

    private void InventorySlot_OnDirtyChanged() {
        if (inventorySlot.itemSO == null) {
            DisableItemVisuals();
            return;
        }

        dirtyIndicator.gameObject.SetActive(inventorySlot.isDirty);

        string itemNameString = inventorySlot.itemSO.nameString + (inventorySlot.isDirty ? " (Dirty)" : "");

        itemNameText.text = inventorySlot.amount > 0 ? itemNameString : "";
    }

    private void InventorySlot_OnItemChanged() {
        if (inventorySlot.itemSO == null) {
            DisableItemVisuals();
            return;
        }

        animator.SetTrigger("OnAdd");

        itemImage.sprite = inventorySlot.itemSO.icon;
        itemImage.gameObject.SetActive(true);
        itemAmountText.text = "x" + inventorySlot.amount.ToString();

        string itemNameString = inventorySlot.itemSO.nameString + (inventorySlot.isDirty ? " (Dirty)" : "");

        itemNameText.text =  inventorySlot.amount > 0 ? itemNameString : "";
        itemAmountText.gameObject.SetActive(true);
        dirtyIndicator.gameObject.SetActive(inventorySlot.isDirty);
    }

    private void InventorySlot_OnQuantityChanged() {
        if (inventorySlot.itemSO == null) {
            DisableItemVisuals();
            return;
        }
        animator.SetTrigger("OnAdd");

        itemAmountText.text = "x" + inventorySlot.amount.ToString();
        string itemNameString = inventorySlot.itemSO.nameString + (inventorySlot.isDirty ? " (Dirty)" : "");

        itemNameText.text = inventorySlot.amount > 0 ? itemNameString : "";
        itemAmountText.gameObject.SetActive(true);
    }

    private void DisableItemVisuals() {
       itemImage.gameObject.SetActive(false);
        itemAmountText.gameObject.SetActive(false);
        itemNameText.gameObject.SetActive(false);
        dirtyIndicator.gameObject.SetActive(false);
        itemNameText.text = "";
    }

    public void OnPointerEnter(PointerEventData eventData) {
        itemNameText.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        itemNameText.gameObject.SetActive(false);
    }
}
