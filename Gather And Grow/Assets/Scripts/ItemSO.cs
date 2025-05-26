using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "ScriptableObjects/Item")]
public class ItemSO : ScriptableObject {

    [Header("Item Settings")]
    public string nameString;
    public Sprite icon;
    public int maxStackSize = 20;
    public bool canGetDirty = true;

    [Header("Ingredient Options")]
    public bool isIngredient = true;
    public IngredientCategory ingredientCategory;
    public PlateRole plateRole = PlateRole.Any;
    public Vector3 plateScale = new Vector3(0.02f, 0.02f, 0.02f);

}

public enum PlateRole {
    Any,
    Main,
    Top,
}

public enum IngredientCategory {
    Protein,
    Carb,
    Vegetable,
    Spice,
    Liquid,
    None,
}

[System.Serializable]
public class ItemAmount {
    public ItemSO itemSO;
    public int amount;

    public ItemAmount(ItemSO itemSO, int amount) {
        this.itemSO = itemSO;
        this.amount = amount;
    }
}
