using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour {

    public static EconomyManager Instance { get; private set; }

    public event Action<int> OnMoneyChanged;

    private int currentMoney;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SaveManager_OnGameLoaded(SaveManager.CurrentSaveFilePath);

        SaveManager.OnGameSaved += SaveManager_OnGameSaved;
        SaveManager.OnGameLoaded += SaveManager_OnGameLoaded;
    }

    private void SaveManager_OnGameLoaded(string path) {
        currentMoney = ES3.Load("currentMoney", path, 200);

        OnMoneyChanged?.Invoke(currentMoney);
    }

    private void SaveManager_OnGameSaved(string path) {
        ES3.Save("currentMoney", currentMoney, path);
    }

    public void AddMoney(int amount) {
        currentMoney += amount;
        OnMoneyChanged?.Invoke(currentMoney);
    }

    public void RemoveMoney(int amount) {
        if (currentMoney >= amount) {
            currentMoney -= amount;
            OnMoneyChanged?.Invoke(currentMoney);
        } 
    }

    public bool CanRemoveMoney(int amount) {
        return currentMoney >= amount;
    }

    private void OnDestroy() {
        SaveManager.OnGameSaved -= SaveManager_OnGameSaved;
        SaveManager.OnGameLoaded -= SaveManager_OnGameLoaded;
    }

}
