using TMPro;
using UnityEngine;

public class Item : MonoBehaviour {

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TextMeshPro amountText;
    [SerializeField] private bool canBePickedUp = false;
    [SerializeField] private bool showAmountOnOne = true;

    private InventorySlot inventorySlot;

    private void Awake() {
        if (gameObject.TryGetComponent(out Rigidbody2D rb)) {
            rb.gravityScale = 0;
        } else {
            Debug.LogWarning("Item prefab does not have a Rigidbody2D component.");
        }
    }

    public void Initialize(InventorySlot inventorySlot, Vector3 spawnPos) {
        if (this.inventorySlot != null && this.inventorySlot != inventorySlot) {
            this.inventorySlot.OnItemChanged -= InventorySlot_OnItemChanged;
            this.inventorySlot.OnQuantityChanged -= InventorySlot_OnItemChanged;
        }

        this.inventorySlot = inventorySlot;
        spriteRenderer.sprite = inventorySlot.itemSO.icon;

        inventorySlot.OnItemChanged += InventorySlot_OnItemChanged;
        inventorySlot.OnQuantityChanged += InventorySlot_OnItemChanged;

        if ((inventorySlot.amount == 1 && showAmountOnOne) || inventorySlot.amount > 1) {
            amountText.gameObject.SetActive(true);
        } else {
            amountText.gameObject.SetActive(false);
        }

        amountText.text = "x" + inventorySlot.amount.ToString();

        transform.position = spawnPos;

        gameObject.SetActive(true);
    }

    public void InventorySlot_OnItemChanged() {
        if (inventorySlot.itemSO == null) return;

        spriteRenderer.sprite = inventorySlot.itemSO.icon;

        if ((inventorySlot.amount == 1 && showAmountOnOne) || inventorySlot.amount > 1) {
            amountText.gameObject.SetActive(true);
        } else {
            amountText.gameObject.SetActive(false);
        }

        amountText.text = "x" + inventorySlot.amount.ToString();
    }

    public void UpdateSprite() {
        spriteRenderer.sprite = inventorySlot.itemSO.icon;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!canBePickedUp) return;
        if (collision.gameObject.TryGetComponent(out Player player)) {
            if (InventoryManager.Instance.TryAddItem(inventorySlot, false, true)) {
                canBePickedUp = false;

                DestroyItem();
            }
        }
    }

    public void DestroyItem() {
       Destroy(gameObject);
    }

    public InventorySlot GetInventorySlot() {
        return inventorySlot;
    }

    public SpriteRenderer GetSpriteRenderer() {
        return spriteRenderer;
    }


    private void OnDestroy() {
        inventorySlot.SetWorldItem(null);
        inventorySlot.OnItemChanged -= InventorySlot_OnItemChanged;
        inventorySlot.OnQuantityChanged -= InventorySlot_OnItemChanged;
    }

}
