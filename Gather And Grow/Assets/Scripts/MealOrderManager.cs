using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;
using QFSW.QC;
using UnityEditor;

public class MealOrderManager : MonoBehaviour {

    public static MealOrderManager Instance { get; private set; }

    public event Action<Meal> OnNewOrderGenerated;
    public event Action<Meal> OnOrderCompleted;

    [SerializeField] private float timeBetweenNewMealsMin = 20f;
    [SerializeField] private float timeBetweenNewMealsMax = 60f;
    [SerializeField] private float timeToCompleteMin = 100f;
    [SerializeField] private float timeToCompleteMax = 300f;
    [SerializeField] private int maxOrdersActive = 5;

    private List<MealSO> unlockedMeals = new List<MealSO>();
    private List<Meal> activeOrders = new List<Meal>();
    private List<Meal> ordersToRemove = new List<Meal>();

    public List<char> vowels = new List<char> { 'a', 'e', 'i', 'o', 'u' };

    private float timeToNextOrder = 0f;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SaveManager.OnGameLoaded += OnGameLoaded;
        SaveManager.OnGameSaved += OnGameSaved;
    }

    private void Update() {
        foreach (Meal order in activeOrders) {
            order.UpdateTime(Time.deltaTime);
            if (order.completed) {
                ordersToRemove.Add(order);
            }
        }
        foreach (Meal order in ordersToRemove) {
            activeOrders.Remove(order);
        }
        if (ordersToRemove.Count > 0) {
            ordersToRemove.Clear();
        }

        if (timeToNextOrder > 0f) {
            timeToNextOrder -= Time.deltaTime;
        }
        if ((activeOrders.Count < maxOrdersActive && timeToNextOrder <= 0f) || activeOrders.Count <= 0) {
            GenerateNewOrder();
            timeToNextOrder = UnityEngine.Random.Range(timeBetweenNewMealsMin, timeBetweenNewMealsMax);
        }
    }

    private void GenerateNewOrder() {
        if (activeOrders.Count < maxOrdersActive) {
            // Put more weight on current rank meals
            List<MealSO> mealsToChooseFrom = new List<MealSO>();
            float currentRankWeight = 0.7f;
            
            List<MealSO> currentRankMeals = RankManager.Instance.GetCurrentRank().unlockedMeals;

            if (currentRankMeals.Count > 0 && UnityEngine.Random.value < currentRankWeight) {
                mealsToChooseFrom = currentRankMeals;
            } else {
                mealsToChooseFrom = unlockedMeals;
            }

            MealSO newOrderSO = mealsToChooseFrom[UnityEngine.Random.Range(0, mealsToChooseFrom.Count)];
            float timeToComplete = UnityEngine.Random.Range(timeToCompleteMin, timeToCompleteMax);
            Meal newOrder = new Meal(newOrderSO, timeToComplete);
            activeOrders.Add(newOrder);
            OnNewOrderGenerated?.Invoke(newOrder);

            AudioManager.Instance.PlaySound(AudioManager.Sound.NewOrder);

            newOrder.OnMealExpired += NewOrder_OnMealExpired;
            newOrder.OnMealHurryUp += PlayTimeTicking;
            newOrder.OnMealCritical += PlayTimeTicking;
            newOrder.OnMealHalfway += PlayTimeTicking;

            
        }
    }

    private void PlayTimeTicking() {
        AudioManager.Instance.PlaySound(AudioManager.Sound.TimerTicking, 3f);
    }

    private void NewOrder_OnMealExpired() {
        Player.Instance.SetPlayerThought("Oops... Too slow...", 3f);
        AudioManager.Instance.PlaySound(AudioManager.Sound.FailedOrder);
    }

    public bool DeliverOrder(Plate plate) {
        int mealCount = plate.GetIngredients().Count;


        foreach (Meal order in activeOrders) {
            bool allIngredientsMatch = plate.DoesPlateMatchMeal(order.mealSO);

            if (allIngredientsMatch) {
                order.CompleteMeal();
                InventoryManager.Instance.SetHeldPlate(null);
                Destroy(plate.gameObject);

                OnOrderCompleted?.Invoke(order);
                return true;
            }
        }
        return false;
    }

    [Command]
    public void CompleteActiveOrder() {
        if (activeOrders.Count <= 0) return;

        Meal firstOrder = activeOrders[0];
        if (firstOrder != null) {
            firstOrder.CompleteMeal();
            activeOrders.Remove(firstOrder);
            OnOrderCompleted?.Invoke(firstOrder);
        }
    }

    public void AddUnlockedMeal(MealSO meal) {
        if (!unlockedMeals.Contains(meal)) {
            unlockedMeals.Add(meal);
        }
    }

    private bool IsMealInQueue(MealSO mealSO) {
        foreach (Meal order in activeOrders) {
            if (order.mealSO == mealSO) {
                return true;
            }
        }
        return false;
    }

    public void SetMaxActiveOrders(int maxOrders) {
        maxOrdersActive = maxOrders;
    }

    [Command]
    public void AddItemsForMeal(string mealName) {
        foreach (MealSO mealSO in unlockedMeals) {
            if (mealSO.name.ToLower() == mealName.ToLower()) {
                foreach (ItemSO ingredient in mealSO.ingredientItemsList) {
                    InventoryManager.Instance.TryAddItem(new InventorySlot(ingredient, 1));
                }
                return;
            }
        }
    }


    [Command]
    public void ResetOrders() {
        foreach (Meal order in activeOrders) {
            order.CompleteMeal();
        }

        activeOrders.Clear();
        timeToNextOrder = 0f;
    }

    private void OnGameSaved(string path) {
        List<MealSaveData> saveData = new List<MealSaveData>();
        foreach (Meal order in activeOrders) {
            MealSaveData data = new MealSaveData();
            data.mealSOName = order.mealSO.name;
            data.timer = order.timer;
            data.timeToComplete = order.timeToComplete;
            saveData.Add(data);
        }
        ES3.Save("ActiveOrders", saveData, path);
    }

    private void OnGameLoaded(string path) {
        if (ES3.KeyExists("ActiveOrders", path)) {
            List<MealSaveData> saveData = ES3.Load<List<MealSaveData>>("ActiveOrders", path);
            foreach (MealSaveData data in saveData) {
                MealSO mealSO = GetMealSOByName(data.mealSOName);
                if (mealSO != null) {
                    Meal order = new Meal(mealSO, data.timeToComplete, data.timer);
                    activeOrders.Add(order);
                    OnNewOrderGenerated?.Invoke(order);
                }
            }
        }
    }

    private MealSO GetMealSOByName(string name) {
        foreach (MealSO meal in unlockedMeals) {
            if (meal.name == name) {
                return meal;
            }
        }
        return null;
    }

    private void OnDestroy() {
        SaveManager.OnGameLoaded -= OnGameLoaded;
        SaveManager.OnGameSaved -= OnGameSaved;
    
    }
}

public class Meal {

    public event Action OnMealHalfway;
    public event Action OnMealHurryUp;
    public event Action OnMealCritical;
    public event Action OnMealCompleted;
    public event Action OnMealExpired;

    public MealSO mealSO;
    public float timer;
    public float timeToComplete;
    public bool completed;
    private bool halfwayFired, hurryFired, criticalFired, expiredFired;

    public Meal(MealSO mealSO, float timeToComplete, float timer = -1) {
        this.mealSO = mealSO;
        this.timeToComplete = timeToComplete;
        this.timer = timer == -1 ? timeToComplete : timer;
    }

    public void CompleteMeal() {
        completed = true;
        OnMealCompleted?.Invoke();
    }

    public void UpdateTime(float d) {
        if (completed || expiredFired) return;
        timer -= d;
        if (!halfwayFired && timer <= timeToComplete * .5f) { halfwayFired = true; OnMealHalfway?.Invoke(); }
        if (!hurryFired && timer <= timeToComplete * .25f) { hurryFired = true; OnMealHurryUp?.Invoke(); }
        if (!criticalFired && timer <= timeToComplete * .1f) { criticalFired = true; OnMealCritical?.Invoke(); }
        if (timer <= 0f && !expiredFired) { expiredFired = true; OnMealExpired?.Invoke(); }

        
    }

}

public class MealSaveData {
    public string mealSOName;
    public float timer;
    public float timeToComplete;
}
