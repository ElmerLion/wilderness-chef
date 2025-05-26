using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;


[System.Serializable]
public class InventorySlot {
    public event Action OnItemChanged;
    public event Action OnQuantityChanged;
    public event Action OnDirtyChanged;

    public ItemSO itemSO { get; private set; }
    public int amount { get; private set; }

    public Item worldItem { get; private set; }


    public bool isDirty { get; private set; }

    public float timer { get; private set; }


    public InventorySlot(ItemSO item, int quantity, Item worldItem = null, bool isDirty = false, float timer = 0) {
        this.isDirty = isDirty;
        SetItem(item);
        AddAmount(quantity);
        SetWorldItem(worldItem);

        if (worldItem != null) {
            worldItem.Initialize(this, worldItem.transform.position);
        }

        this.timer = timer;
    }

    public void SetWorldItem(Item item) {
        worldItem = item;
        if (worldItem != null) {
            worldItem.Initialize(this, worldItem.transform.position);
        }
    }
    public void SetWorldItemParent(Transform parent) {
        if (worldItem != null) {
            worldItem.transform.SetParent(parent);
        }
    }

    public void SetItem(ItemSO item, int amount = 0) {
        itemSO = item;
        this.amount = amount;
        OnItemChanged?.Invoke();
    }
    public void SetAmount(int amount) {
        this.amount = amount;
        OnQuantityChanged?.Invoke();
    }

    public void AddAmount(int amount) {
        this.amount += amount;
        OnQuantityChanged?.Invoke();
    }

    public void Clear() {
        itemSO = null;
        amount = 0;
        worldItem = null;
        OnItemChanged?.Invoke();
    }

    public void SetTimer(float timer) {
        this.timer = timer;
    }

    public void RemoveAmount(int amount) {
        this.amount -= amount;
        if (this.amount <= 0) {
            SetItem(null);
            if (worldItem != null) {
                worldItem.DestroyItem();

            }
        }
        OnQuantityChanged?.Invoke();
    }

    public void SetIsDirty(bool isDirty) {
        if (itemSO != null && !itemSO.canGetDirty) return;

        this.isDirty = isDirty;

        OnDirtyChanged?.Invoke();
    }

    public ItemAmount GetItemAmount() {
        return new ItemAmount(itemSO, amount);
    }

    public override string ToString() {
        return "Item: " + itemSO + ", Amount: " + amount + ", World Item: " + (worldItem != null ? worldItem.name : "null");
    }

    public InventorySlotData GetSaveData() {
        InventorySlotData data =  new InventorySlotData(itemSO != null ? itemSO.nameString : "", amount, isDirty);
        return data;
    }

    public void RestoreFromData(InventorySlotData data, Transform parent) {
        if (data == null) return;

        itemSO = ItemManager.Instance.GetItem(data.itemName);
        amount = data.amount;
        isDirty = data.isDirty;
    }

}

[System.Serializable]
public class InventorySlotData {
    public string itemName;
    public int amount;
    public bool isDirty;

    public PlateSaveData plateData;

    public InventorySlotData(string itemName, int amount, bool isDirty) {
        this.itemName = itemName;
        this.amount = amount;
        this.isDirty = isDirty;
    }
}
