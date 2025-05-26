using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using QFSW.QC;
using System.Collections;

public class RankManager : MonoBehaviour {

    public static RankManager Instance { get; private set; }

    [SerializeField] private GameObject rankUpUI;
    [SerializeField] private bool testingMode = false;
    [SerializeField] private List<Rank> ranks;

    public const string UnlockedRanksKey = "UnlockedRanks";
    public const string DeliveredMealsKey = "DeliveredMeals";

    private List<Rank> unlockedRanks = new List<Rank>();
    private int deliveredMeals;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        MealOrderManager.Instance.OnOrderCompleted += OnOrderCompleted;

        foreach (Rank rank in ranks) {
            if (rank.unlockedArea != null) {
                rank.unlockedArea.Initialize(rank);
            }
            if (testingMode) {
                UnlockRank(rank);
            }
        }

        CheckRank();

        if  (rankUpUI != null) {
            rankUpUI.gameObject.SetActive(false);
        } 

        SaveManager.OnGameLoaded += OnGameLoaded;
        SaveManager.OnGameSaved += OnGameSaved;
    }

    private void OnOrderCompleted(Meal meal) {
        deliveredMeals++;
        CheckRank();
    }

    private void CheckRank() {
        foreach (Rank rank in ranks) {
            if (deliveredMeals >= rank.minMealsDelivered) {
                UnlockRank(rank);
            }
        }
    }

    private void UnlockRank(Rank rank) {
        if (unlockedRanks.Contains(rank)) {
            return;
        }

        MessageUI.Instance.ShowMessage($"You are now a {rank.name}! New meals{(rank.unlockedArea != null ? " and areas" : "")} have been unlocked!", 4f);

        if (rank.unlockedArea != null) {
            rank.unlockedArea.gameObject.SetActive(false);
        }

        foreach (AnimalSpawner spawner in rank.spawnersToActivate) {
            spawner.IsActive = true;
        }

        foreach (MealSO meal in rank.unlockedMeals) {
            MealOrderManager.Instance.AddUnlockedMeal(meal);
        }

        if (rank.maxOrdersActive > 0) {
            MealOrderManager.Instance.SetMaxActiveOrders(rank.maxOrdersActive);
        }

        unlockedRanks.Add(rank);

        AudioManager.Instance.PlaySound(AudioManager.Sound.RankUp, 0.5f);
        AudioManager.Instance.PlaySound(AudioManager.Sound.RankUp1, 0.4f);
        ParticleEffectsManager.Instance.Play(EffectType.Confetti, Player.Instance.transform.position);

        if (rankUpUI != null) {
            StartCoroutine(ShowRankUpUI());
        }

        if (rank == ranks[ranks.Count - 1]) {
            FeastManager.Instance.RequestFeast();
        }
    }

    public IEnumerator ShowRankUpUI() {
        rankUpUI.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        rankUpUI.gameObject.SetActive(false);
    }

    public int GetDeliveredMeals() {
       return deliveredMeals;
    }

    [Command]
    public void UnlockAllRanks() {
        testingMode = true;
        foreach (Rank rank in ranks) {
            UnlockRank(rank);
        }
    }

    [Command]
    public void UnlockNextRank() {
        if (unlockedRanks.Count >= ranks.Count) {
            return;
        }
        UnlockRank(ranks[unlockedRanks.Count]);
    }

    public Rank GetCurrentRank() {
        if (unlockedRanks.Count == 0) {
            return null;
        }
        return unlockedRanks[unlockedRanks.Count - 1];
    }

    private void OnGameLoaded(string path) {
        int unlockedRankCount = ES3.Load(UnlockedRanksKey, path, 0);
        deliveredMeals = ES3.Load(DeliveredMealsKey, path, 0);
        for (int i = 0; i < unlockedRankCount; i++) {
            if (i < ranks.Count) {
                Rank rank = ranks[i];
                UnlockRank(rank);
            }
        }
    }

    private void OnGameSaved(string path) {
        ES3.Save(UnlockedRanksKey, unlockedRanks.Count, path);
        ES3.Save("LatestRankName", GetCurrentRank().name, path);
        ES3.Save(DeliveredMealsKey, deliveredMeals, path);
    }

    private void OnDestroy() {
        MealOrderManager.Instance.OnOrderCompleted -= OnOrderCompleted;
        SaveManager.OnGameLoaded -= OnGameLoaded;
        SaveManager.OnGameSaved -= OnGameSaved;
    }


    [System.Serializable]
    public class Rank {

        public string name;
        public int minMealsDelivered;
        public int maxOrdersActive;
        public UnlockableArea unlockedArea;
        public List<AnimalSpawner> spawnersToActivate;
        public List<MealSO> unlockedMeals;

    }
    
}
