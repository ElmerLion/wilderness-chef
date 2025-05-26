using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using static CookbookUI;

public class CookbookUI : BaseUI {

    public static CookbookUI Instance { get; private set; }

    [Header("Other")]
    [SerializeField] private TextMeshProUGUI recipesDiscoveredText;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button previousPageButton;
    [SerializeField] private List<StationRecipeInfo> stationRecipeInfoList;

    [Header("Page 1")]
    [SerializeField] private GameObject recipeContainerPage1;
    [SerializeField] private TextMeshProUGUI page1Title;
    [SerializeField] private GameObject toolPrefab;
    [SerializeField] private GameObject stationPrefab;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private GameObject arrowPrefab;

    [Header("Page 2")]
    [SerializeField] private GameObject recipeContainerPage2;
    [SerializeField] private TextMeshProUGUI page2Title;

    private List<GameObject> pooledIngredients = new List<GameObject>();
    private List<GameObject> pooledTools = new List<GameObject>();
    private List<GameObject> pooledStations = new List<GameObject>();
    private List<GameObject> pooledArrows = new List<GameObject>();

    private List<RecipeSO> recipesToShow = new List<RecipeSO>();
    private int pageIndex = 0;
    private StationType currentStationType = StationType.None;
    private Transform previouslyPressedButton;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        nextPageButton.onClick.AddListener(OnNextPageButtonClicked);
        previousPageButton.onClick.AddListener(OnPreviousPageButtonClicked);

        foreach (StationRecipeInfo stationRecipeInfo in stationRecipeInfoList) {
            stationRecipeInfo.bookmarkButton.onClick.AddListener(() => {
                SetRecipesToShowByStationType(stationRecipeInfo.stationType);

                stationRecipeInfo.bookmarkButton.transform.position += new Vector3(0, 10f);

                if (previouslyPressedButton != null) {
                    previouslyPressedButton.position -= new Vector3(0, 10f);
                }
                previouslyPressedButton = stationRecipeInfo.bookmarkButton.transform;
            });
        }

        toolPrefab.SetActive(false);
        stationPrefab.SetActive(false);
        itemPrefab.SetActive(false);
        arrowPrefab.SetActive(false);
        gameObject.SetActive(false);

    }

    private void SetRecipesToShowByStationType(StationType stationType) {
        recipesToShow = CookBookManager.Instance.GetDiscoveredRecipesForStation(stationType).Where(r => r.showInCookbook).ToList();
        currentStationType = stationType;
        pageIndex = 0;
        ShowPairAt(0);
    }

    private void OnNextPageButtonClicked() {
        if (recipesToShow.Count <= 1) return;       

        pageIndex = (pageIndex + 2) % recipesToShow.Count;

        ShowPairAt(pageIndex);
    }

    private void OnPreviousPageButtonClicked() {
        if (recipesToShow.Count <= 1) return;

        pageIndex = ((pageIndex - 2) % recipesToShow.Count + recipesToShow.Count)
                    % recipesToShow.Count;

        ShowPairAt(pageIndex);
    }

    private void ShowPairAt(int start) {
        int count = recipesToShow.Count;
        if (count == 0) {
            ClearContainer(recipeContainerPage1);
            ClearContainer(recipeContainerPage2);
            return;
        }

        page1Title.gameObject.SetActive(true);

        start = ((start % count) + count) % count;

        SetPageToRecipe(1, recipesToShow[start]);

        if (count > 1) {
            SetPageToRecipe(2, recipesToShow[(start + 1) % count]);
            recipeContainerPage2.SetActive(true);
            page2Title.gameObject.SetActive(true);
        } else {
            recipeContainerPage2.SetActive(false);
            page2Title.gameObject.SetActive(false);
        }

        nextPageButton.interactable = (count > 1);
        previousPageButton.interactable = (count > 1);
    }

    private void ClearContainer(GameObject container) {
        page1Title.gameObject.SetActive(false);
        page2Title.gameObject.SetActive(false);

        for (int i = container.transform.childCount - 1; i >= 0; i--) {
            container.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public override void Show() {
        SetRecipesToShowByStationType(currentStationType);

        if (previouslyPressedButton == null) {
            Transform defaultButton = stationRecipeInfoList[stationRecipeInfoList.Count - 1].bookmarkButton.transform;
            defaultButton.position += new Vector3(0, 10f);
            previouslyPressedButton = defaultButton;
        }

        UpdateRecipesDiscoveredText();

        base.Show();
    }

    public void SetStationTypeRecipesToShow(List<RecipeSO> recipes) {
        recipesToShow = recipes.Where(r => r.showInCookbook).ToList();
        pageIndex = 0;
        ShowPairAt(0);
    }

    public void SetPageToRecipe(int pageNum, RecipeSO recipeSO) {
        StationType spriteType = currentStationType;
        if (currentStationType == StationType.None) {
            spriteType = recipeSO.stationTypeList[0];
        }

        GameObject container = pageNum == 1 ? recipeContainerPage1 : recipeContainerPage2;
        TextMeshProUGUI title = pageNum == 1 ? page1Title : page2Title;

        for (int i = container.transform.childCount - 1; i >= 0; i--) {
            container.transform.GetChild(i).gameObject.SetActive(false);
        }

        Sprite toolSprite = null;
        Sprite stationSprite = null;
        foreach (StationRecipeInfo stationRecipeInfo in stationRecipeInfoList) {
            if (stationRecipeInfo.stationType == spriteType) {
                toolSprite = stationRecipeInfo.toolImage;
                stationSprite = stationRecipeInfo.stationImage;
                break;
            }
        }

        int siblingIndex = 0;

        title.text = recipeSO.output[0].itemSO.nameString;

        foreach (ItemAmount itemAmount in recipeSO.input) {
            GameObject ingredientObject = GetPooledObject(itemPrefab, pooledIngredients);
            SetIngredient(ingredientObject, itemAmount.itemSO, container.transform);

            ingredientObject.transform.SetSiblingIndex(siblingIndex++);
        }

        GameObject arrowObject = GetPooledObject(arrowPrefab, pooledArrows);
        arrowObject.transform.SetParent(container.transform);
        arrowObject.SetActive(true);

        arrowObject.transform.SetSiblingIndex(siblingIndex++);
       

        GameObject toolObject = GetPooledObject(toolPrefab, pooledTools);
        toolObject.transform.Find("Sprite").GetComponent<Image>().sprite = toolSprite;
        toolObject.transform.SetParent(container.transform);
        toolObject.SetActive(true);

        toolObject.transform.SetSiblingIndex(siblingIndex++);


        GameObject arrowObject1 = GetPooledObject(arrowPrefab, pooledArrows);
        arrowObject1.transform.SetParent(container.transform);
        arrowObject1.SetActive(true);

        arrowObject1.transform.SetSiblingIndex(siblingIndex++);

        foreach (ItemAmount itemAmount in recipeSO.output) {
            GameObject outputObject = GetPooledObject(itemPrefab, pooledIngredients);
            SetIngredient(outputObject, itemAmount.itemSO, container.transform);

            outputObject.transform.SetSiblingIndex(siblingIndex++);
        }



    }

    private void SetIngredient(GameObject ingredientObject, ItemSO itemSO, Transform parent) {
        ingredientObject.transform.SetParent(parent);
        ingredientObject.transform.Find("Icon").GetComponent<Image>().sprite = itemSO.icon;
        TextMeshProUGUI nameText = ingredientObject.transform.Find("Name").GetComponent<TextMeshProUGUI>();
        nameText.text = itemSO.nameString;
        ingredientObject.SetActive(true);


    }


    private GameObject GetPooledObject(GameObject prefab, List<GameObject> pooledObjects) {
        foreach (GameObject pooledObject in pooledObjects) {
            if (!pooledObject.activeSelf) {
                return pooledObject;
            }
        }
        GameObject newObject = Instantiate(prefab, recipeContainerPage1.transform);
        newObject.SetActive(false);
        pooledObjects.Add(newObject);
        return newObject;
    }

    public void UpdateRecipesDiscoveredText() {
        recipesDiscoveredText.text = "Recipes Discovered: " + CookBookManager.Instance.GetDiscoveredRecipes().Count + "/" + CookBookManager.Instance.GetAllRecipesCount();
    }

    [System.Serializable]
    public class StationRecipeInfo {
        public StationType stationType;
        public Button bookmarkButton;
        public Sprite stationImage;
        public Sprite toolImage;
    }
    
}
