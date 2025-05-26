using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MealCardUI : MonoBehaviour {

    [Header("References")]
    [SerializeField] private GameObject mealIngredientPrefab;
    [SerializeField] private Transform mealIngredientContainer;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Timer Colors")]
    [SerializeField] private Color timerNormalColor;
    [SerializeField] private Color timerHalfwayColor;
    [SerializeField] private Color timerHurryUpColor;
    [SerializeField] private Color timerCriticalColor;

    private Animator animator;
    private Meal meal;
    private Image background;

    private void Start() {
        animator = GetComponent<Animator>();
    }

    public void SetMeal(Meal meal) {
        this.meal = meal;
        background = transform.Find("Background").GetComponent<Image>();

        UpdateMealCard();
        mealIngredientPrefab.SetActive(false);
        gameObject.SetActive(true);
    }

    public void UpdateMealCard() {
        foreach (Transform child in mealIngredientContainer) {
            if (child.gameObject == mealIngredientPrefab) continue;
            Destroy(child.gameObject);
        }

        MealSO mealSO = meal.mealSO;
        titleText.text = mealSO.nameString;
        titleText.ForceMeshUpdate();

        foreach (ItemSO itemIngredient in mealSO.ingredientItemsList) {
            GameObject ingredientGO = Instantiate(mealIngredientPrefab, mealIngredientContainer);
            ingredientGO.transform.Find("Sprite").GetComponent<Image>().sprite = itemIngredient.icon;
            ingredientGO.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = itemIngredient.nameString;
            ingredientGO.SetActive(true);
        }

        foreach (IngredientCategory ingredientCategory in mealSO.ingredientCategoryItems) {
            GameObject ingredientGO = Instantiate(mealIngredientPrefab, mealIngredientContainer);
            ingredientGO.transform.Find("Sprite").GetComponent<Image>().sprite = MealOrderUI.Instance.GetDefaultIcon(ingredientCategory);
            ingredientGO.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "(Any " + ingredientCategory.ToString() + ")";
            ingredientGO.SetActive(true);
        }

        meal.OnMealHalfway += Meal_OnMealHalfway;
        meal.OnMealHurryUp += Meal_OnMealHurryUp;
        meal.OnMealCritical += Meal_OnMealCritical;
        meal.OnMealCompleted += Meal_Destroy;
        meal.OnMealExpired += Meal_Destroy;

    }

    private void Meal_Destroy() {
        meal.OnMealHalfway -= Meal_OnMealHalfway;
        meal.OnMealHurryUp -= Meal_OnMealHurryUp;
        meal.OnMealCritical -= Meal_OnMealCritical;
        meal.OnMealCompleted -= Meal_Destroy;
        meal.OnMealExpired -= Meal_Destroy;

        StartCoroutine(WaitForExitEnd());
    }

    private void Meal_OnMealHalfway() {
        background.color = timerHalfwayColor;
    }
    private void Meal_OnMealHurryUp() {
        background.color = timerHurryUpColor;
    }
    private void Meal_OnMealCritical() {
        background.color = timerCriticalColor;
    }

    private IEnumerator WaitForExitEnd() {
        animator.SetTrigger("Exit");

        yield return null;

        var clips = animator.GetCurrentAnimatorClipInfo(0);
        float exitLength = clips.Length > 0
            ? clips[0].clip.length
            : 0f;

        yield return new WaitForSeconds(exitLength);
        
        Destroy(gameObject);
    }

}
