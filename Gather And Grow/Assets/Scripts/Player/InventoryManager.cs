using Mono.CSharp;
using NUnit.Framework.Internal.Execution;
using QFSW.QC;
using Ricimi;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour {

    public static InventoryManager Instance { get; private set; }

    public event Action<InventorySlot, int> OnSelectedSlotChanged;

    [SerializeField] private RecipeSO debugRecipe;

    [Header("References")]
    [SerializeField] private GameObject droppedItemPrefab;
    [SerializeField] private Transform plateHoldPos;
    [SerializeField] private Plate platePrefab;

    [Header("Settings")]
    [SerializeField] private int inventorySlotCount = 10;


    private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    private InventorySlot heldItem;
    private InventorySlot selectedSlot;
    private Plate heldPlate;
    private int selectedSlotIndex;

    private PickupEffect pickupEffectPrefab;

    public bool hasHeldItem {
        get {
            return heldItem.itemSO != null;
        }
    }

    public bool hasHeldPlate {
        get {
            return heldPlate != null;
        }
    }

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        pickupEffectPrefab = Resources.Load<PickupEffect>("PickupEffect");

        for (int i = 0; i < inventorySlotCount; i++) {
            inventorySlots.Add(new InventorySlot(null, 0));
        }
        InventoryUI.Instance.InitializeInventoryUI(inventorySlots);

        heldItem = new InventorySlot(null, 0);

        GameInput.Instance.OnPrimaryPerformed += Instance_OnPrimaryPerformed;
        GameInput.Instance.OnSwitchSlotPerformed += Instance_OnSwitchSlotPerformed;

        SetSelectedSlot(0);

        SaveManager.OnGameSaved += SaveManager_OnGameSaved;
        SaveManager.OnGameLoaded += SaveManager_OnGameLoaded;
    }

    private void SaveManager_OnGameLoaded(string path) {
        if (ES3.KeyExists("InventorySlots", path)) {
            List<InventorySlotData> loadedSlots = ES3.Load<List<InventorySlotData>>("InventorySlots", path);
            for (int i = 0; i < loadedSlots.Count; i++) {
                ItemSO itemSO = ItemManager.Instance.GetItem(loadedSlots[i].itemName);
                inventorySlots[i].SetItem(itemSO, loadedSlots[i].amount);
                inventorySlots[i].SetIsDirty(loadedSlots[i].isDirty);
                inventorySlots[i].SetWorldItem(null);
            }
            Debug.Log("Loaded inventory slots " + inventorySlots.Count);
        }

        if (ES3.KeyExists("InventoryPlate", path)) {
            PlateSaveData plateData = ES3.Load<PlateSaveData>("InventoryPlate", path);
            if (plateData != null) {
                ItemSO itemSO = ItemManager.Instance.GetItem(plateData.plateInvSlot.itemName);
                heldPlate = Instantiate(platePrefab, plateHoldPos.position, Quaternion.identity);
                heldPlate.transform.SetParent(plateHoldPos);
                heldPlate.Initialize(new InventorySlot(itemSO, 1, heldPlate.gameObject.GetComponent<Item>()));
                heldPlate.RestoreFromSaveData(plateData);
                SetHeldPlate(heldPlate);
                InventoryUI.Instance.SetPlateHeld(true);
            }
        } 
    }

    private void SaveManager_OnGameSaved(string path) {
        List<InventorySlotData> inventorySlotDatas = new List<InventorySlotData>();

        foreach (InventorySlot slot in inventorySlots) {
            InventorySlotData data = slot.GetSaveData();
            inventorySlotDatas.Add(data);
        }

        if (hasHeldPlate) {
            PlateSaveData plateData = heldPlate.GetSaveData();
            ES3.Save("InventoryPlate", plateData, path);
        } else {
            ES3.DeleteKey("InventoryPlate", path);
        }

        ES3.Save("InventorySlots", inventorySlotDatas, path);
    }

    private void Update() {
        HandleSlotSelection();
    }

    private void Instance_OnSwitchSlotPerformed(int slotIndex) {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Count)
            return;
        SetSelectedSlot(slotIndex);
    }


    private void Instance_OnPrimaryPerformed() {
        if (UIHelpers.IsPointerOverUI(out List<GameObject> uiHits)) {
            return;
        }

        DropInventorySlot(heldItem);
        InventoryUI.Instance.SetHeldItemUI(heldItem);
    }

    private void HandleSlotSelection() {
        if (hasHeldPlate) return;

        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0f) {
            if (scrollDelta > 0) {
                selectedSlotIndex--;
            } else {
                selectedSlotIndex++;
            }

            if (selectedSlotIndex < 0)
                selectedSlotIndex = inventorySlots.Count - 1;
            if (selectedSlotIndex >= inventorySlots.Count)
                selectedSlotIndex = 0;

            SetSelectedSlot(inventorySlots[selectedSlotIndex]);
        }
    }

    public void SlotClicked(InventorySlot clickedSlot) {
        Debug.Log("Slot clicked. Held Item: " + heldItem?.itemSO?.nameString + " clicked slot: " + clickedSlot.itemSO?.nameString);

        if (heldItem.itemSO == null && clickedSlot.itemSO != null) {
            Debug.Log("Setting held item to " + clickedSlot.itemSO.nameString + " amount: " + clickedSlot.amount);
            SetHeldItem(clickedSlot);
            return;
        }
        if (heldItem.itemSO != null && (clickedSlot.itemSO == null || clickedSlot.itemSO == heldItem.itemSO)) {
            Debug.Log("Setting clicked slot to held item " + heldItem.itemSO.nameString + " amount: " + heldItem.amount);
            SetClickedSlotToHeldItem(clickedSlot);
            return;
        }
    }

    private void SetHeldItem(InventorySlot clickedSlot) {
        if (clickedSlot.itemSO == null) {
            return;
        }

        heldItem.SetItem(clickedSlot.itemSO, clickedSlot.amount);
        heldItem.SetWorldItem(clickedSlot.worldItem);
        heldItem.SetIsDirty(clickedSlot.isDirty);

        InventoryUI.Instance.SetHeldItemUI(heldItem);

        clickedSlot.SetItem(null);
        clickedSlot.SetWorldItem(null);
        clickedSlot.SetIsDirty(false);
    }

    private void SetClickedSlotToHeldItem(InventorySlot clickedSlot) {
        if (clickedSlot.itemSO != heldItem.itemSO) {
            clickedSlot.SetItem(heldItem.itemSO, heldItem.amount);
            clickedSlot.SetWorldItem(heldItem.worldItem);
            clickedSlot.SetIsDirty(heldItem.isDirty);

            ResetHeldItem();
        } else if (clickedSlot.itemSO == heldItem.itemSO && clickedSlot.isDirty == heldItem.isDirty) {
            int spaceLeft = clickedSlot.itemSO.maxStackSize - clickedSlot.amount;
            int toAdd = Mathf.Min(spaceLeft, heldItem.amount);
            clickedSlot.AddAmount(toAdd);

            heldItem.RemoveAmount(toAdd);

            if (heldItem.amount <= 0) {
                ResetHeldItem();
            }
        }

        InventoryUI.Instance.SetHeldItemUI(heldItem);
    }

    private void ResetHeldItem() {
        heldItem.SetItem(null);
        heldItem.SetWorldItem(null);
        heldItem.SetIsDirty(false);
    }

    public bool TryAddItem(InventorySlot inputInvSlot, bool dropExcess = false, bool removeFromInput = false, bool spawnEffect = true, Vector3 fxStartingPos = default) {
        if (heldPlate != null) {
            heldPlate.AddIngredientToPlate(inputInvSlot);
            return true;
        }
        if (inputInvSlot.worldItem != null && inputInvSlot.worldItem.gameObject.TryGetComponent(out Plate plate)) {
            SetHeldPlate(plate);
            return true;
        }

        ItemSO itemSO = inputInvSlot.itemSO;
        int amount = inputInvSlot.amount;
        Item worldItem = inputInvSlot.worldItem;

        foreach (InventorySlot slot in inventorySlots) {
            if (slot.itemSO == null) {
                int toAdd = Mathf.Min(amount, itemSO.maxStackSize);

                if (spawnEffect)
                    SpawnPickupEffect(itemSO, amount, worldItem, fxStartingPos);

                slot.SetItem(itemSO);
                slot.AddAmount(toAdd);
                slot.SetIsDirty(inputInvSlot.isDirty);

                slot.SetTimer(inputInvSlot.timer);

                if (worldItem != null) {
                    slot.SetWorldItem(worldItem);
                }
                if (removeFromInput && !hasHeldPlate) {
                    inputInvSlot.RemoveAmount(toAdd);
                }

                PlaySoundForItem(itemSO);

                return true;
            } else if (slot.itemSO == itemSO && slot.amount != itemSO.maxStackSize && slot.isDirty == inputInvSlot.isDirty) {
                int spaceLeft = itemSO.maxStackSize - slot.amount;
                int toAdd = Mathf.Min(spaceLeft, amount);

                if (spawnEffect)
                    SpawnPickupEffect(itemSO, amount, worldItem, fxStartingPos);

                slot.AddAmount(toAdd);
                amount -= toAdd;
                if (removeFromInput && !hasHeldPlate) {
                    inputInvSlot.RemoveAmount(toAdd);
                }
                if (inputInvSlot.timer > 0) {
                    slot.SetTimer(inputInvSlot.timer);
                }
                if (worldItem != null && slot.worldItem == null) {
                    slot.SetWorldItem(worldItem);
                }

                PlaySoundForItem(itemSO);

                if (amount <= 0) {
                    return true;
                }

            } 
        }

        

        if (dropExcess) {
            DropInventorySlot(new InventorySlot(itemSO, amount, worldItem, inputInvSlot.isDirty));
        }
        return false;
    }

    private void SpawnPickupEffect(ItemSO itemSO, int amount, Item worldItem, Vector3 origin) {
        Vector3 startPosition;

        if (worldItem != null) {
            //effectRenderer = worldItem.transform.Find("Sprite").GetComponent<SpriteRenderer>();
            startPosition = worldItem.transform.position;
        } else {
            GameObject ghostGO = Instantiate(
                pickupEffectPrefab.gameObject,
                origin,
                Quaternion.identity
            );
            startPosition = origin;

            PickupEffect ghostFx = ghostGO.GetComponent<PickupEffect>();
            ghostFx.Play(itemSO.icon, amount, startPosition, Player.Instance.transform);
            return;
        }

        // if we had a worldItem, just trigger the effect on its own prefab
        PickupEffect fx = Instantiate(
            pickupEffectPrefab,
            startPosition,
            Quaternion.identity
        );
        fx.Play(itemSO.icon, amount, startPosition, Player.Instance.transform);
    }

    private void PlaySoundForItem(ItemSO itemSO) {
        AudioManager.Sound soundToPlay = AudioManager.Sound.BasicItemPickup;

        if (itemSO.ingredientCategory == IngredientCategory.Protein) {
            soundToPlay = AudioManager.Sound.MeatPickup;
        } else if (itemSO.ingredientCategory == IngredientCategory.Liquid) {
            soundToPlay = AudioManager.Sound.LiquidPickup;
        }

        AudioManager.Instance.PlaySound(soundToPlay, 0.5f, true);
    }

    public void DropInventorySlot(InventorySlot inventorySlot, Vector3 dropOrigin = new Vector3()) {
        if (heldPlate != null) return;

        ItemSO itemSO = inventorySlot.itemSO;
        int amount = inventorySlot.amount;

        if (amount <= 0) {
            return;
        }

        if (dropOrigin == Vector3.zero) {
            Vector2 dropDirection = Player.Instance.GetFacingDirection();
            Vector3 dropDirectionVector = new Vector3(dropDirection.x, dropDirection.y, 0);
            dropOrigin = dropDirectionVector * 1.5f + Player.Instance.transform.position;
        }

        if (inventorySlot.worldItem != null) {
            InventorySlot newInvSlot = new InventorySlot(itemSO, amount, inventorySlot.worldItem, true);

            newInvSlot.worldItem.transform.position = dropOrigin + Player.Instance.transform.forward * 2f;
            newInvSlot.worldItem.gameObject.SetActive(true);
        } else {
            GameObject droppedItemGO = Instantiate(droppedItemPrefab, dropOrigin, Quaternion.identity);
            Item droppedItem = droppedItemGO.GetComponent<Item>();
            InventorySlot newInvSlot = new InventorySlot(itemSO, amount, droppedItem, true);
            droppedItem.Initialize(newInvSlot, dropOrigin + Player.Instance.transform.forward * 2f);
        }


        inventorySlot.Clear();
    }

    public void DropItem(ItemSO itemSO, Vector3 dropOrigin, int amount = 1) {
        DropInventorySlot(new InventorySlot(itemSO, amount), dropOrigin);
    }

    public bool HasItems(List<ItemAmount> items) {
        foreach (ItemAmount item in items) {
            bool found = false;

            foreach (InventorySlot slot in inventorySlots) {
                if (slot.itemSO == item.itemSO && slot.amount >= item.amount) {
                    found = true;
                    break;
                }
            }

            if (!found) {
                return false;
            }
        }

        return true;
    }

    public void RemoveItems(List<ItemAmount> items) {
        foreach (ItemAmount item in items) {
            foreach (InventorySlot slot in inventorySlots) {
                if (slot.itemSO == item.itemSO) {
                    slot.AddAmount(-item.amount);
                    if (slot.amount <= 0) {
                        slot.Clear();
                    }
                    break;
                }
            }
        }
    }

    public void SetHeldPlate(Plate plate) {
        if (heldPlate == null) {
            DropInventorySlot(heldItem);
            SetHeldItem(heldItem);
            InventoryUI.Instance.SetPlateHeld(plate != null);

            heldPlate = plate;
            heldPlate.transform.position = plateHoldPos.position;
            heldPlate.transform.SetParent(plateHoldPos);
            heldPlate.SetSortingOrder(200);

            plate.SetHeld(true);
            Player.Instance.CheckPlateVisibility();
            return;
        } 
        if (plate == null) {
            if (heldPlate != null) {
                heldPlate.SetHeld(false);
            }
            heldPlate.transform.rotation = Quaternion.identity;
            heldPlate.SetSortingOrder(95);
            heldPlate = null;
            
            InventoryUI.Instance.SetPlateHeld(false);
        }
    }

    public bool HasSpaceInInventoryForItem(InventorySlot invSlot) {
        if (invSlot == null || invSlot.itemSO == null) return false;

        foreach (InventorySlot slot in inventorySlots) {
            if (slot.itemSO == null) {
                return true;
            } else if (slot.itemSO == invSlot.itemSO && slot.amount < invSlot.itemSO.maxStackSize) {
                return true;
            }
        }
        return false;
    }

    public InventorySlot GetHeldItem() {
        return heldItem;
    }

    public InventorySlot GetHeldPlate() {
        return heldPlate != null ? heldPlate.GetInventorySlot() : null;
    }

    public InventorySlot GetSelectedSlot() {
        return selectedSlot;
    }

    public void SetSelectedSlot(int index) {
        if (index < 0 || index >= inventorySlots.Count) {
            Debug.LogWarning($"Index {index} is out of bounds for inventory slots.");
            return;
        }

        selectedSlot = inventorySlots[index];
        selectedSlotIndex = index;
        OnSelectedSlotChanged?.Invoke(selectedSlot, selectedSlotIndex);
    }

    public void SetSelectedSlot(InventorySlot selectedSlot) {
        this.selectedSlot = selectedSlot;

        selectedSlotIndex = inventorySlots.IndexOf(selectedSlot);

        OnSelectedSlotChanged?.Invoke(selectedSlot, selectedSlotIndex);
    }

    [Command]
    public void ClearInventory() {
        foreach (InventorySlot slot in inventorySlots) {
            slot.Clear();
        }
    }

    private void OnDestroy() {
        GameInput.Instance.OnPrimaryPerformed -= Instance_OnPrimaryPerformed;
        GameInput.Instance.OnSwitchSlotPerformed -= Instance_OnSwitchSlotPerformed;

        SaveManager.OnGameSaved -= SaveManager_OnGameSaved;
        SaveManager.OnGameLoaded -= SaveManager_OnGameLoaded;
    }
}