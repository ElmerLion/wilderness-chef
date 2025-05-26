using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class AnimalCoop : MonoBehaviour, IInteractable {

    [SerializeField] private float productionTimerMax = 300f;
    [SerializeField] private AnimalSO animalSO;
    [SerializeField] private List<Transform> productPlacements;
    [SerializeField] private GameObject _interactPrompt;
    [SerializeField] private ProgressBarUI progressBar;

    private List<InventorySlot> producedItems = new List<InventorySlot>();

    private int maxProducts => productPlacements.Count;

    public GameObject InteractPrompt { get => _interactPrompt; set => InteractPrompt = value; }

    private GameObject itemPrefab;
    private float productionTimer;

    private void Start() {
        itemPrefab = Resources.Load<GameObject>("Item");
        productionTimer = productionTimerMax;
    }

    private void Update() {
        if (producedItems.Count >= maxProducts) return;

        productionTimer -= Time.deltaTime;
        progressBar.SetProgress((productionTimerMax - productionTimer) / productionTimerMax);
        if (productionTimer <= 0) {
            AddNewProduct();
            productionTimer = productionTimerMax;
        }

    }

    private void AddNewProduct() {
        if (producedItems.Count >= maxProducts) {
            return;
        }

        ItemSO productSO = animalSO.passiveItemDropSO != null ? animalSO.passiveItemDropSO : animalSO.alternateItemSODrop;
        if (productSO == null) {
            Debug.LogWarning("Assigned animal has no products!");
            return;
        }

        InventorySlot newProduct = new InventorySlot(productSO, 1);

        Item item = Instantiate(itemPrefab, transform).GetComponent<Item>();
        item.transform.position = productPlacements[producedItems.Count].position;
        item.transform.Find("Sprite").GetComponent<SpriteRenderer>().sortingOrder = 100;
        newProduct.SetWorldItem(item);

        Debug.Log("Added new product");
        producedItems.Add(newProduct);
    }

    public void Interact() {
        if (producedItems.Count == 0) {
            Player.Instance.SetPlayerThought("There is no " + animalSO.passiveItemDropSO.nameString.ToLower() + " here to collect.", 4f);
            return;
        }

        if (InventoryManager.Instance.TryAddItem(producedItems[0], false, true)) {
            producedItems.RemoveAt(0);
        }
    }

    public void InteractAlternate() {
        return;
    }
}
