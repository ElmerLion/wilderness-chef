using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseStation : MonoBehaviour, IInteractable {

    public static event Action<RecipeSO> OnRecipeCompleted;
    public event Action<InventorySlot> OnIngredientRemoved;
    

    [Header("References")]
    [SerializeField] protected GameObject itemTooltips;
    [SerializeField] protected GameObject _interactPrompt;

    [Header("Station Settings")]
    [SerializeField] private StationType type;
    [SerializeField] protected List<Transform> ingredientPlaceTranformList = new List<Transform>();
    [SerializeField] private int maxCountPerIngredient = 3;
    [SerializeField] protected bool onlyIngredients = true;

    protected GameObject itemTooltipPrefab;
    protected GameObject itemPrefab;
    protected GameObject platePrefab;
    private GameObject newRecipeUnlockedAnim;

    protected List<InventorySlot> currentIngredients = new List<InventorySlot>();
    protected List<RecipeSO> validRecipes = new List<RecipeSO>();

    public GameObject InteractPrompt { get => _interactPrompt; set => _interactPrompt = value; }

    protected int maxIngredientCount => ingredientPlaceTranformList.Count;


    public StationType Type { get => type; }

    private void Awake() {
        itemPrefab = Resources.Load<GameObject>("Item");
        platePrefab = Resources.Load<GameObject>("Plate");
        itemTooltipPrefab = Resources.Load<GameObject>("ItemTooltip");
        newRecipeUnlockedAnim = Resources.Load<GameObject>("NewRecipeUnlocked");

    }

    public virtual void Start() {
        if (CookBookManager.Instance == null) return;

        validRecipes = CookBookManager.Instance.GetRecipesForStation(type);
        StationManager.Instance.AddStation(gameObject);


    }


    public virtual void Interact() {
        InventorySlot selectedSlot = InventoryManager.Instance.GetSelectedSlot();

        Plate plate = null;

        if (InventoryManager.Instance.hasHeldPlate && currentIngredients.Count <= 0) {
            AddIngredient(InventoryManager.Instance.GetHeldPlate());
            return;
        }

        if (selectedSlot.itemSO != null && CanAddToPlate(out plate)) {
            AddIngredient(selectedSlot, true, plate);
            return;
        }

        if (selectedSlot.itemSO != null) {
            if (CanAddIngredient(selectedSlot)) {
                AddIngredient(selectedSlot);
                return;
            }
        }

        if (currentIngredients.Count > 0 &&
            (InventoryManager.Instance.HasSpaceInInventoryForItem(currentIngredients[0])
            || (InventoryManager.Instance.hasHeldPlate && selectedSlot.itemSO == null))) {

            TryTakeIngredient(currentIngredients.Count - 1);
            return;
        }
    }
    public virtual void InteractAlternate() {
        InventorySlot plateSlot = InventoryManager.Instance.GetHeldPlate();

        if (plateSlot != null) {
            Plate plate = plateSlot.worldItem.GetComponent<Plate>();
            InventorySlot plateIngredient = plate.GetLastIngredient();
            if (CanAddIngredient(plateIngredient)) {
                plate.RemoveLastIngredientFromPlate();
                AddIngredient(plateIngredient);
            }
        }

        if (currentIngredients.Count <= 0) return;

        if (currentIngredients[0].worldItem.gameObject.TryGetComponent(out Plate placedPlate)) {
            InventorySlot ingredientToTake = placedPlate.GetLastIngredient();

            if (ingredientToTake != null) {
                if (InventoryManager.Instance.TryAddItem(ingredientToTake)) {
                    placedPlate.RemoveLastIngredientFromPlate();
                } else {
                    Player.Instance.SetPlayerThought("I need to free up some space in my inventory.", 2f);
                }
            }
        }
    }

    public void AddIngredient(InventorySlot playerSlot, bool removeFromInput = true, Plate placedPlate = null) {

        // Hantera med en temp slot? Men tas det bort rätt då?
        RecipeSO altRecipe = GetValidRecipeWithInput(new List<InventorySlot> { playerSlot });
        InventorySlot tempSlot = null;

        if (altRecipe != null
            && altRecipe.alternativeItemInput != null
            && playerSlot.itemSO == altRecipe.alternativeItemInput
            && altRecipe.input.Count == 1) {

            tempSlot = new InventorySlot(
                altRecipe.input[0].itemSO, 
                1,                     
                null,                  
                playerSlot.isDirty,
                playerSlot.timer
            );
        } else {
            tempSlot = playerSlot;
        }

        if (placedPlate != null) {
            placedPlate.AddIngredientToPlate(playerSlot);
            return;
        }

        if (type != StationType.None && type != StationType.Washing && !DoesAnyRecipeInclude(playerSlot.itemSO)) {
            Player.Instance.SetPlayerThought("Hmm... I don't think this will work here.", 2f);
            return;
        }

        if (type != StationType.Washing && type != StationType.None && playerSlot.isDirty) {
            Player.Instance.SetPlayerThought("This ingredient is dirty, I need to wash it first.", 2f);
            return;
        }

        for (int i = 0; i < currentIngredients.Count; i++) {
            InventorySlot existing = currentIngredients[i];
            if (existing.itemSO == tempSlot.itemSO && existing.isDirty == tempSlot.isDirty) {
                int spaceLeft = maxCountPerIngredient - existing.amount;
                int amountToAdd = Mathf.Min(tempSlot.amount, spaceLeft);
                if (amountToAdd <= 0) return;

                existing.AddAmount(amountToAdd);

                if (removeFromInput)
                    playerSlot.RemoveAmount(amountToAdd); 

                return;
            }
        }

        if (currentIngredients.Count >= maxIngredientCount) {
            Player.Instance.SetPlayerThought("I can't fit any more ingredients...", 2f);
            return;
        }

        int amountToAddNew = Mathf.Min(tempSlot.amount, maxCountPerIngredient);
        if (amountToAddNew <= 0) return;

        InventorySlot newSlot = new InventorySlot(tempSlot.itemSO, amountToAddNew, null, tempSlot.isDirty, tempSlot.timer);

        playerSlot.SetTimer(0);

        Item placedItem;
        int insertIndex = currentIngredients.Count;
        if (tempSlot.worldItem == null) {
            placedItem = Instantiate(itemPrefab, ingredientPlaceTranformList[insertIndex])
                             .GetComponent<Item>();
        } else {
            placedItem = tempSlot.worldItem;
            placedItem.transform.SetParent(ingredientPlaceTranformList[insertIndex]);
            placedItem.transform.position = ingredientPlaceTranformList[insertIndex].position;
            placedItem.gameObject.SetActive(true);
        }

        if (newSlot.itemSO.ingredientCategory == IngredientCategory.Liquid)
            placedItem.gameObject.SetActive(false);
        else
            placedItem.gameObject.SetActive(true);

        if (removeFromInput) {
            if (playerSlot.worldItem != null
                && playerSlot.worldItem.TryGetComponent<Plate>(out Plate plateObj)) {
                InventoryManager.Instance.SetHeldPlate(null);
                plateObj.SetVisiblityIngredients(true);
            } else {
                playerSlot.RemoveAmount(amountToAddNew);
            }
        }

        if (itemTooltips != null) {
            ItemTooltipUI tooltip = Instantiate(itemTooltipPrefab, itemTooltips.transform)
                                       .GetComponent<ItemTooltipUI>();
            tooltip.SetItem(newSlot);
        }

        newSlot.SetWorldItem(placedItem);
        currentIngredients.Add(newSlot);
    }


    public bool TryTakeIngredient(int index) {
        if (index < 0 || index >= currentIngredients.Count) {
            Debug.LogWarning("Invalid index for ingredient.");
            return false;
        }
        InventorySlot inventorySlot = currentIngredients[index];

        if (InventoryManager.Instance.hasHeldPlate && inventorySlot.worldItem.TryGetComponent(out Plate stationPlate)) {
            return false;
        } 

        if (inventorySlot.itemSO == null) {
            currentIngredients.RemoveAt(index);
            return false;
        }

        if (InventoryManager.Instance.TryAddItem(inventorySlot, false, true)) {
            if (inventorySlot.amount <= 0 || (inventorySlot.worldItem != null && inventorySlot.worldItem.TryGetComponent(out Plate plate))) {
                currentIngredients.Remove(inventorySlot);
            }
            OnIngredientRemoved?.Invoke(inventorySlot);
            return true;
        } else {
            Player.Instance.SetPlayerThought("I need to free up some space in my inventory.", 2f);
        }
        return false;
    }

    protected bool CanAddIngredient(InventorySlot inputSlot) {
        if (inputSlot == null) return false;

        if (currentIngredients.Count < maxIngredientCount && (inputSlot.worldItem == null || !inputSlot.worldItem.TryGetComponent(out Plate plate))) {
            return true;
        }

        foreach (InventorySlot ingredientSlot in currentIngredients) {
            if (ingredientSlot.itemSO == inputSlot.itemSO && ingredientSlot.isDirty == inputSlot.isDirty) {
                return true;
            }
        }
        return false;
    }

    protected bool CanAddToPlate(out Plate plate) {
        if (currentIngredients.Count <= 0) {
            plate = null;
            return false;
        }
        if (currentIngredients[0] != null && currentIngredients[0].worldItem != null) {
            
            return currentIngredients[0].worldItem.TryGetComponent(out plate);
        }

        plate = null;
        return false;
    }

    protected bool CanAcceptAsNextIngredient(InventorySlot candidate) {
        // if empty, allow if any recipe needs it
        if (currentIngredients.Count == 0) {
            foreach (RecipeSO r in validRecipes)
                foreach (ItemAmount ia in r.input)
                    if (ia.itemSO == candidate.itemSO)
                        return true;
            return false;
        }

        // otherwise make the combined‐type list
        List<ItemSO> combined = new List<ItemSO>();
        foreach (InventorySlot s in currentIngredients)
            if (!combined.Contains(s.itemSO))
                combined.Add(s.itemSO);
        if (!combined.Contains(candidate.itemSO))
            combined.Add(candidate.itemSO);

        // now check for any recipe that covers all combined types (and isn’t too short)
        foreach (RecipeSO r in validRecipes) {
            if (combined.Count > r.input.Count)
                continue;

            bool coversAll = true;
            foreach (ItemSO t in combined) {
                bool found = false;
                foreach (ItemAmount ia in r.input)
                    if (ia.itemSO == t) {
                        found = true;
                        break;
                    }
                if (!found) {
                    coversAll = false;
                    break;
                }
            }
            if (coversAll)
                return true;
        }

        return false;
    }

    protected RecipeSO GetValidRecipeWithInput(List<InventorySlot> inputSlots) {
        // Build a sorted list of the ItemSOs we're holding
        List<ItemSO> heldTypes = new List<ItemSO>();
        foreach (var slot in inputSlots)
            heldTypes.Add(slot.itemSO);
        heldTypes.Sort((a, b) => a.nameString.CompareTo(b.nameString));

        // Try every recipe configured for this station
        foreach (RecipeSO recipe in validRecipes) {
            // Quick reject on count
            if (recipe.input.Count != heldTypes.Count)
                continue;

            // Build sorted list of the recipe's true inputs
            List<ItemSO> required = new List<ItemSO>();
            foreach (var ia in recipe.input)
                required.Add(ia.itemSO);
            required.Sort((a, b) => a.nameString.CompareTo(b.nameString));

            // Is this a single‐ingredient recipe with an alternative?
            bool hasAlt = (recipe.alternativeItemInput != null && required.Count == 1);

            // Compare one by one
            bool match = true;
            for (int i = 0; i < heldTypes.Count; i++) {
                ItemSO h = heldTypes[i];
                ItemSO r = required[i];

                if (h == r) {
                    // exact match
                    continue;
                }
                if (hasAlt && h == recipe.alternativeItemInput) {
                    // rawEgg counts as crackedEgg under the hood
                    continue;
                }

                match = false;
                break;
            }

            if (match)
                return recipe;
        }

        // no recipe found
        return null;
    }


    protected void InvokeRecipeCompleted(RecipeSO recipeSO) {
        AudioManager.Instance.PlaySound(AudioManager.Sound.RecipeCompleted, 0.5f);
        if (newRecipeUnlockedAnim != null && !CookBookManager.Instance.IsRecipeDiscovered(recipeSO)) {
            GameObject newRecipeUnlocked = Instantiate(newRecipeUnlockedAnim, transform.position + new Vector3(0, 1.5f), Quaternion.identity);
            Destroy(newRecipeUnlocked, 2f);
        }
        OnRecipeCompleted?.Invoke(recipeSO);
    }

    protected bool DoesAnyRecipeInclude(ItemSO itemSO) {
        foreach (RecipeSO recipe in validRecipes) {
            foreach (ItemAmount itemAmount in recipe.input) {
                if (itemAmount.itemSO == itemSO) {
                    Debug.Log("Found matching recipe: " + recipe.name);
                    return true;
                }
            }
            if (recipe.alternativeItemInput != null && recipe.alternativeItemInput == itemSO) {
                Debug.Log("Found matching recipe with alternative input: " + recipe.name);
                return true;
            }
        }
        return false;
    }

    public List<RecipeSO> GetValidRecipes() {
        return validRecipes;
    }

    public virtual StationSaveData GetSaveData() {
        StationSaveData saveData = new StationSaveData {
            type = type,
            position = transform.position,
            rotation = transform.rotation,
            ingredients = new List<IngredientData>()
        };

        foreach (InventorySlot slot in currentIngredients) {
            IngredientData ingredientData = new IngredientData {
                itemName = slot.itemSO.nameString,
                amount = slot.amount,
                isDirty = slot.isDirty,
                slotIndex = currentIngredients.IndexOf(slot)
            };
            saveData.ingredients.Add(ingredientData);
        }

        if (currentIngredients.Count == 1 && currentIngredients[0].worldItem != null && currentIngredients[0].worldItem.TryGetComponent(out Plate plate)) {
            saveData.plateData = plate.GetSaveData();
        }

        return saveData;
    }

    public virtual void RestoreFromData(StationSaveData saveData) {
        transform.position = saveData.position;
        transform.rotation = saveData.rotation;

        foreach (InventorySlot slot in currentIngredients) {
            Destroy(slot.worldItem.gameObject);
        }
        currentIngredients.Clear();

        if (saveData.plateData != null) {
            Plate newPlate = Instantiate(platePrefab, ingredientPlaceTranformList[0]).GetComponent<Plate>();
            ItemSO itemSO = ItemManager.Instance.GetItem(saveData.plateData.plateInvSlot.itemName);
            InventorySlot plateInvSlot = new InventorySlot(itemSO, 1, newPlate.gameObject.GetComponent<Item>());
            newPlate.Initialize(plateInvSlot);
            newPlate.RestoreFromSaveData(saveData.plateData);
            currentIngredients.Add(plateInvSlot);
            return;
        }

        foreach (IngredientData ingredientData in saveData.ingredients) {
            ItemSO itemSO = ItemManager.Instance.GetItem(ingredientData.itemName);
            if (itemSO == null) continue;

            InventorySlot newSlot = new InventorySlot(itemSO, ingredientData.amount, null, ingredientData.isDirty);

            Transform parent = ingredientPlaceTranformList[ingredientData.slotIndex];
            GameObject placedItem = Instantiate(itemPrefab, parent);
            newSlot.SetWorldItem(placedItem.GetComponent<Item>());

            if (itemSO.ingredientCategory == IngredientCategory.Liquid)
                placedItem.SetActive(false);
            else
                placedItem.SetActive(true);

            currentIngredients.Add(newSlot);
        }

        if (itemTooltips != null) {
            foreach (InventorySlot slot in currentIngredients) {
                ItemTooltipUI tooltip = Instantiate(itemTooltipPrefab, itemTooltips.transform)
                                           .GetComponent<ItemTooltipUI>();
                tooltip.SetItem(slot);
            }
        }
    }

}

[System.Serializable]
public class StationSaveData {
    public StationType type;
    public Vector3 position;
    public Quaternion rotation;
    public List<IngredientData> ingredients;

    // ← explicit cooking-station fields (nullable when not used)
    public int processingCount;
    public List<float> processingTimers;
    public List<int> processingStates;
    public List<string> processingRecipeNames;

    // Other Fields
    public float processingTimer;
    public int cuttingProgress;
    public string validRecipeName;
    public PlateSaveData plateData;

}

[System.Serializable]
public class IngredientData {
    public string itemName;
    public int amount;
    public bool isDirty;
    public int slotIndex;
}

public enum StationType {
    None,
    FryingPan,
    Oven,
    Pot,
    Cutting,
    Mixing,
    Washing,
}