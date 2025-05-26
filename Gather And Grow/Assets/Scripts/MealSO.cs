using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Meal", menuName = "ScriptableObjects/MealSO")]
public class MealSO : ScriptableObject {

    public string nameString;
    public Sprite finalMeal;
    public Complexity complexity;
    public List<ItemSO> ingredientItemsList;
    public List<IngredientCategory> ingredientCategoryItems;
    

    public enum Complexity {
        Easy,
        Medium,
        Hard,
        VeryHigh,
    }
}
