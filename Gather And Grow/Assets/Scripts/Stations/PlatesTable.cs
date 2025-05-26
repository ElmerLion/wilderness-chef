using UnityEngine;

public class PlatesTable : MonoBehaviour, IInteractable {

    [SerializeField] private ItemSO plateSO;
    [SerializeField] private GameObject platePrefab;
    [SerializeField] private GameObject _interactPrompt;

    public GameObject InteractPrompt { get => _interactPrompt; set => InteractPrompt = value; }

    public void Interact() {
        Plate plate = Instantiate(platePrefab, transform.position, Quaternion.identity).GetComponent<Plate>();
        plate.Initialize(new InventorySlot(plateSO, 1, plate.gameObject.GetComponent<Item>()));
        InventoryManager.Instance.SetHeldPlate(plate);
    }

    public void InteractAlternate() {
        return;
    }
}
