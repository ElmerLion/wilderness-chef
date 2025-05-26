using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class CookBookManager : MonoBehaviour, IInteractable {

    public static CookBookManager Instance { get; private set; }
    public GameObject InteractPrompt { get => _interactPrompt; set => InteractPrompt = value; }

    [SerializeField] private GameObject _interactPrompt;
    [SerializeField] private List<RecipeSO> allRecipes = new List<RecipeSO>();

    private Dictionary<StationType, List<RecipeSO>> recipesByStation;
    private List<RecipeSO> discoveredRecipes = new List<RecipeSO>();
    private int allRecipesCount;

    private void Awake() {
        Instance = this;

        recipesByStation = new Dictionary<StationType, List<RecipeSO>>();
        foreach (RecipeSO recipeSO in allRecipes) {
            foreach (StationType stationType in recipeSO.stationTypeList) {
                if (!recipesByStation.ContainsKey(stationType)) {
                    recipesByStation[stationType] = new List<RecipeSO>();
                }
                recipesByStation[stationType].Add(recipeSO);
            }
        }
        allRecipesCount = 0;
        foreach (RecipeSO recipe in allRecipes) {
            if (recipe.showInCookbook) {
                allRecipesCount++;
            }
        }
    }

    private void Start() {
        BaseStation.OnRecipeCompleted += BaseStation_OnRecipeCompleted;

        SaveManager.OnGameLoaded += OnGameLoaded;
        SaveManager.OnGameSaved += OnGameSaved;
    }

    private void BaseStation_OnRecipeCompleted(RecipeSO obj) {
        AddDiscoveredRecipe(obj);
    }

    public void AddDiscoveredRecipe(RecipeSO recipe) {
        if (!recipe.showInCookbook) return;

        if (!discoveredRecipes.Contains(recipe)) {
            discoveredRecipes.Add(recipe);
        }
    }

    public List<RecipeSO> GetDiscoveredRecipes() {
        return discoveredRecipes;
    }

    public int GetAllRecipesCount() {

        return allRecipesCount;
    }

    public List<RecipeSO> GetRecipesForStation(StationType stationType) {
        if (recipesByStation.ContainsKey(stationType)) {
            return recipesByStation[stationType];
        }
        return allRecipes;
    }

    public List<RecipeSO> GetDiscoveredRecipesForStation(StationType stationType) {
        List<RecipeSO> stationRecipes = GetRecipesForStation(stationType);

        List<RecipeSO> discoveredStationRecipes = new List<RecipeSO>();
        foreach (RecipeSO recipe in stationRecipes) {
            if (discoveredRecipes.Contains(recipe)) {
                discoveredStationRecipes.Add(recipe);
            }
        }

        return discoveredStationRecipes;
    }

    public RecipeSO GetRecipeByName(string recipeName) {
       foreach (RecipeSO recipe in allRecipes) {
            if (recipe.name == recipeName) {
                return recipe;
            }
        }
        return null;
    }

    public bool IsRecipeDiscovered(RecipeSO recipe) {
        return discoveredRecipes.Contains(recipe);
    }

    public void Interact() {
        CookbookUI.Instance.Show();
    }

    public void InteractAlternate() {
        return;
    }

    private void OnGameSaved(string path) {
        List<string> discoveredRecipeNames = new List<string>();
        foreach (RecipeSO recipe in discoveredRecipes) {
            discoveredRecipeNames.Add(recipe.name);
        }
        ES3.Save("DiscoveredRecipes", discoveredRecipeNames, path);
    }

    private void OnGameLoaded(string path) {
        if (ES3.KeyExists("DiscoveredRecipes", path)) {
            List<string> discoveredRecipeNames = ES3.Load<List<string>>("DiscoveredRecipes", path);
            foreach (string recipeName in discoveredRecipeNames) {
                RecipeSO recipe = GetRecipeByName(recipeName);
                if (recipe != null) {
                    AddDiscoveredRecipe(recipe);
                }
            }
        }
    }

    private void OnDestroy() {
        BaseStation.OnRecipeCompleted -= BaseStation_OnRecipeCompleted;
        SaveManager.OnGameLoaded -= OnGameLoaded;
        SaveManager.OnGameSaved -= OnGameSaved;
    }

    [System.Serializable]
    public class StationRecipes {
        public StationType type;
        public List<RecipeSO> recipes;  
    }
}
