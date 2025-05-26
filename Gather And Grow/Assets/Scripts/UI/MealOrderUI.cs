using UnityEngine;

public class MealOrderUI : MonoBehaviour {
    
    public static MealOrderUI Instance { get; private set; }

    public Sprite GetDefaultIcon(IngredientCategory ingredientCategory) {
        switch (ingredientCategory) {
            case IngredientCategory.Protein:
                return defaultProtein;
            case IngredientCategory.Carb:
                return defaultCarb;
            case IngredientCategory.Vegetable:
                return defaultVegetable;
            case IngredientCategory.Spice:
                return defaultSpice;
            default:
                return null;
        }
    }

    [SerializeField] private GameObject mealOrderPrefab;
    [SerializeField] private Transform mealOrderContainer;

    [Header("Icons")]
    [SerializeField] private Sprite defaultProtein;
    [SerializeField] private Sprite defaultCarb;
    [SerializeField] private Sprite defaultVegetable;
        [SerializeField] private Sprite defaultSpice;

    private void Start() {
        MealOrderManager.Instance.OnNewOrderGenerated += MealOrderManager_OnNewOrderGenerated;

        mealOrderPrefab.SetActive(false);
    }

    private void Awake() {
        Instance = this;
    }

    private void MealOrderManager_OnNewOrderGenerated(Meal newOrder) {
        GameObject mealOrderGO = Instantiate(mealOrderPrefab, mealOrderContainer);
        MealCardUI mealCardUI = mealOrderGO.GetComponent<MealCardUI>();
        mealCardUI.SetMeal(newOrder);
    }
    
}
