using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemTooltipUI : MonoBehaviour {

    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image dirtyIndicator;

    private InventorySlot inventorySlot;

    public void SetItem(InventorySlot inventorySlot) {
        this.inventorySlot = inventorySlot;
        UpdateItem();

        inventorySlot.OnItemChanged += UpdateItem;
        inventorySlot.OnQuantityChanged += UpdateItem;
        inventorySlot.OnDirtyChanged += UpdateItem;
    }

    private void UpdateItem() {
        if (inventorySlot == null || inventorySlot.itemSO == null) {
            Destroy(gameObject);
            return;
        }

        icon.sprite = inventorySlot.itemSO.icon;
        amountText.text = inventorySlot.amount.ToString();
        dirtyIndicator.gameObject.SetActive(inventorySlot.isDirty);
    }

    public InventorySlot GetSlot() {
        return inventorySlot;
    }
    
}
