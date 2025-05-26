using UnityEngine;
using System.Collections.Generic;
using QFSW.QC;

public class ItemManager : MonoBehaviour {

    public static ItemManager Instance { get; private set; }

    [SerializeField] private List<ItemSO> allItems;

    private void Awake() {
        Instance = this;
    }

    public ItemSO GetItem(string itemName) {
        foreach (ItemSO item in allItems) {
            if (item.nameString.ToLower().Replace(" ", "") == itemName.ToLower().Replace(" ", "")) {
                return item;
            }
        }
        return null;
    }

    [Command("AddItem")]
    public void AddItemToInventory(string name, int amount = 1) {
        ItemSO itemSO = GetItem(name);
        if (itemSO != null) {
            InventoryManager.Instance.TryAddItem(new InventorySlot(itemSO, amount));
        } else {
            Debug.Log($"Item with name {name} not found.");
        }
    }

}
