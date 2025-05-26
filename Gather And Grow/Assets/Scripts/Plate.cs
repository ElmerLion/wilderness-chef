using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class Plate : MonoBehaviour {

    public List<char> vowels = new List<char> { 'a', 'e', 'i', 'o', 'u' };

    [Header("References")]
    [SerializeField] private SpriteRenderer plateSprite;

    [Header("Visible Ingredients")]
    [SerializeField] private GameObject ingredientPrefab;
    [SerializeField] private List<SpriteRenderer> oneIngredientPlacements;
    [SerializeField] private List<SpriteRenderer> twoIngredientPlacements;
    [SerializeField] private List<SpriteRenderer> threeIngredientPlacements;
    [SerializeField] private List<SpriteRenderer> fourIngredientPlacements;
    [SerializeField] private SpriteRenderer mainRolePlacement;

    [Header("Tooltip")]
    [SerializeField] private GameObject itemTooltipPrefab;
    [SerializeField] private GameObject itemTooltipParent;
    [SerializeField] private Transform itemToolTipPlacedPos;
    [SerializeField] private Transform itemToolTipHeldPos;

    private List<InventorySlot> ingredients;
    private List<ItemTooltipUI> itemTooltips;

    private InventorySlot inventorySlot;
    public Item item { get; private set; }

    public void Initialize(InventorySlot inventorySlot) {
        this.inventorySlot = inventorySlot;
        item = GetComponent<Item>();
        item.Initialize(inventorySlot, transform.position);

        ingredients = new List<InventorySlot>();
        itemTooltips = new List<ItemTooltipUI>();
       
        ResetIngredientRenders();
    }

    public void AddIngredientToPlate(InventorySlot inventorySlot, bool removeFromInput = true) {
        if (ingredients.Count >= 4) return;
        if (inventorySlot.isDirty) {
            Player.Instance.SetPlayerThought("I need to wash this ingredient before serving it!", 3f);
            return;
        }

        List<SpriteRenderer> spriteRenderers = GetIngredientPlacements(ingredients.Count + 1);

        if (spriteRenderers == null) {
            Debug.LogError("No sprite renderers found for the given ingredient count.");
            return;
        }

        InventorySlot newInvSlot = new InventorySlot(inventorySlot.itemSO, 1);
        if (removeFromInput) {
            inventorySlot.RemoveAmount(1);
        } 

        ingredients.Add(newInvSlot);

        ResetIngredientRenders();
        int index = 0;
        foreach (InventorySlot ingredientSlot in ingredients) {
            if (ingredientSlot.itemSO.plateRole == PlateRole.Any) {
                spriteRenderers[index].sprite = ingredientSlot.itemSO.icon;
                spriteRenderers[index].transform.localScale = ingredientSlot.itemSO.plateScale;
                index++;
                continue;
            }
            if (ingredientSlot.itemSO.plateRole == PlateRole.Main) {
                mainRolePlacement.sprite = ingredientSlot.itemSO.icon;
                spriteRenderers[index].sprite = null;
            }
        }

        if (newInvSlot.worldItem != null) {
            newInvSlot.worldItem.gameObject.SetActive(false);
            newInvSlot.worldItem.transform.SetParent(transform);
        }

        if (itemTooltipParent != null) {
            ItemTooltipUI itemTooltip = Instantiate(itemTooltipPrefab, itemTooltipParent.transform).GetComponent<ItemTooltipUI>();
            itemTooltip.SetItem(newInvSlot);
            itemTooltips.Add(itemTooltip);
        }

    }

    public InventorySlot RemoveLastIngredientFromPlate() {
        if (ingredients == null || ingredients.Count == 0) return null;

        int lastIndex = ingredients.Count - 1;
        InventorySlot removed = ingredients[lastIndex];
        ingredients.RemoveAt(lastIndex);

        ResetIngredientRenders();

        foreach (ItemTooltipUI itemTooltip in itemTooltips) {
            if (itemTooltip.GetSlot() == removed) {
                Destroy(itemTooltip.gameObject);
                break;
            }
        }

        int newCount = ingredients.Count;
        if (newCount > 0) {
            List<SpriteRenderer> renderers = GetIngredientPlacements(newCount);
            for (int i = 0; i < newCount; i++) {
                InventorySlot slot = ingredients[i];
                if (slot.itemSO.plateRole == PlateRole.Any) {
                    renderers[i].sprite = slot.itemSO.icon;
                    renderers[i].transform.localScale = slot.itemSO.plateScale;
                    continue;
                }
                if (slot.itemSO.plateRole == PlateRole.Main) {
                    mainRolePlacement.sprite = slot.itemSO.icon;
                    renderers[i].sprite = null;
                }
            }
        }

        return removed;
    }

    public InventorySlot GetLastIngredient() {
        return ingredients.Count > 0 ? ingredients[ingredients.Count - 1] : null;
    }

    public void SetVisiblityIngredients(bool isVisible) {
        transform.Find("Plate").gameObject.SetActive(isVisible);
        transform.Find("IngredientPlacements").gameObject.SetActive(isVisible);
    }

    public void SetSortingOrder(int sortingOrder) {
        YSorter ySorter = plateSprite.GetComponent<YSorter>();
        ySorter.SetSortingOffset(sortingOrder);

        foreach (SpriteRenderer spriteRenderer in oneIngredientPlacements) {
            spriteRenderer.gameObject.GetComponent<YSorter>().SetSortingOffset(sortingOrder + 20);   
        }
        foreach (SpriteRenderer spriteRenderer in twoIngredientPlacements) {
            spriteRenderer.gameObject.GetComponent<YSorter>().SetSortingOffset(sortingOrder + 20);
        }
        foreach (SpriteRenderer spriteRenderer in threeIngredientPlacements) {
            spriteRenderer.gameObject.GetComponent<YSorter>().SetSortingOffset(sortingOrder + 20);
        }
        foreach (SpriteRenderer spriteRenderer in fourIngredientPlacements) {
            spriteRenderer.gameObject.GetComponent<YSorter>().SetSortingOffset(sortingOrder + 20);
        }

        mainRolePlacement.gameObject.GetComponent<YSorter>().SetSortingOffset(sortingOrder + 10);
    }

    private List<SpriteRenderer> GetIngredientPlacements(int ingredientCount) {
        switch (ingredientCount) {
            case 1:
                return oneIngredientPlacements;
            case 2:
                return twoIngredientPlacements;
            case 3:
                return threeIngredientPlacements;
            case 4:
                return fourIngredientPlacements;
            default:
                return null;
        }
    }

    private void ResetIngredientRenders() {
        foreach (SpriteRenderer spriteRenderer in oneIngredientPlacements) {
            spriteRenderer.sprite = null;
        }
        foreach (SpriteRenderer spriteRenderer in twoIngredientPlacements) {
            spriteRenderer.sprite = null;
        }
        foreach (SpriteRenderer spriteRenderer in threeIngredientPlacements) {
            spriteRenderer.sprite = null;
        }
        foreach (SpriteRenderer spriteRenderer in fourIngredientPlacements) {
            spriteRenderer.sprite = null;
        }

        mainRolePlacement.sprite = null;
    }

    public bool DoesPlateMatchMeal(MealSO mealSO) {
        if (mealSO == null || ingredients == null || ingredients.Count <= 0) return false;

        int mealCount = ingredients.Count;

        if (mealCount < mealSO.ingredientItemsList.Count + mealSO.ingredientCategoryItems.Count) {
            Player.Instance.SetPlayerThought("It does not seem like this meal has been ordered.", 3f);
            return false;
        }

        List<InventorySlot> plateIngredients = ingredients;
        List<ItemSO> plateIngredientSOs = new List<ItemSO>();
        foreach (InventorySlot ingredient in plateIngredients) {
            plateIngredientSOs.Add(ingredient.itemSO);
        }

        bool allIngredientsMatch = true;

        List<ItemSO> itemsToCheck = new List<ItemSO>(plateIngredientSOs);

        foreach (ItemSO needIngredient in mealSO.ingredientItemsList) {
            if (!plateIngredientSOs.Contains(needIngredient)) {
                Player.Instance.SetPlayerThought($"I need to add {(vowels.Contains(needIngredient.nameString[0]) ? "an" : "a")} {needIngredient.nameString} before serving.", 3f);
                allIngredientsMatch = false;
                break;
            }
            itemsToCheck.Remove(needIngredient);
        }

        foreach (IngredientCategory requiredCat in mealSO.ingredientCategoryItems) {
            bool foundMatch = false;

            for (int i = 0; i < itemsToCheck.Count; i++) {
                if (itemsToCheck[i].ingredientCategory == requiredCat) {
                    itemsToCheck.RemoveAt(i);
                    foundMatch = true;
                    break;
                }
            }

            if (!foundMatch) {
                Player.Instance.SetPlayerThought($"I need to add some sort of {requiredCat}.", 3f);
                allIngredientsMatch = false;
                break;
            }
        }

        return allIngredientsMatch;
    }
    public InventorySlot GetInventorySlot() {
        return inventorySlot;
    }

    public void SetHeld(bool isHeld) {
        if (isHeld) {
            itemTooltipParent.transform.position = itemToolTipHeldPos.position;
        } else {
            itemTooltipParent.transform.position = itemToolTipPlacedPos.position;
        }
    }

    public List<InventorySlot> GetIngredients() {
        return ingredients;
    }

    public PlateSaveData GetSaveData() {
        PlateSaveData saveData = new PlateSaveData();

        saveData.plateInvSlot = inventorySlot.GetSaveData();
        saveData.ingredients = new List<InventorySlotData>();

        foreach (InventorySlot ingredient in ingredients) {
            InventorySlotData slotData = new InventorySlotData(ingredient.itemSO.nameString, ingredient.amount, ingredient.isDirty);
            saveData.ingredients.Add(slotData);
        }

        return saveData;
    }

    public void RestoreFromSaveData(PlateSaveData saveData) {
        ItemSO itemSO = ItemManager.Instance.GetItem(saveData.plateInvSlot.itemName);

        foreach (InventorySlotData slotData in saveData.ingredients) {
            ItemSO ingSO = ItemManager.Instance.GetItem(slotData.itemName);
            InventorySlot ingredient = new InventorySlot(ingSO, slotData.amount, null, slotData.isDirty);
            AddIngredientToPlate(ingredient);
        }
    }
}

public class PlateSaveData {
    public InventorySlotData plateInvSlot;
    public List<InventorySlotData> ingredients;
}
