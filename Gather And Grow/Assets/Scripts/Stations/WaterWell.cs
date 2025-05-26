using UnityEngine;

public class WaterWell : MonoBehaviour, IInteractable {

    [SerializeField] private ItemSO waterItemSO;
    [SerializeField] private GameObject _interactPrompt;

    public GameObject InteractPrompt { get => _interactPrompt; set => _interactPrompt = value; }

    public void Interact() {
        InventoryManager.Instance.TryAddItem(new InventorySlot(waterItemSO, 1), false, false, true, transform.position);
    }

    public void InteractAlternate() {
        return;
    }
}
