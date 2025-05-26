using UnityEngine;
using System.Collections.Generic;

public class StationPlot : MonoBehaviour, IInteractable {

    [SerializeField] private List<StationSO> purchasableStations;
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private int prefabIndex;

    private bool hasBeenPurchased = false;
    private Vector3 scale;

    public GameObject InteractPrompt {
        get { return interactPrompt; }
        set { interactPrompt = value; }
    }

    private void Start() {
        StationManager.Instance.AddStationPlot(gameObject);
    }

    public void Interact() {
        PurchaseStationUI.Instance.Show(purchasableStations, PurchaseStation);
    }

    public void PurchaseStation(StationSO stationSO) {
        if (EconomyManager.Instance.CanRemoveMoney(stationSO.startingCost) == false) {
            Debug.Log("Not enough money to purchase the station.");
            return;
        }

        PurchaseStationUI.Instance.Hide();
        EconomyManager.Instance.RemoveMoney(stationSO.startingCost);

        // Instantiate into the same parent as everything else
        GameObject newStationGO = Instantiate(
            stationSO.prefab,
            transform.position,
            Quaternion.identity
        );

        // Deactivate this plot and register it
        hasBeenPurchased = true;
        gameObject.SetActive(false);
        
    }

    public void InteractAlternate() { /* no-op */ }

    public StationPlotSaveData GetSaveData() {
        StationPlotSaveData saveData = new StationPlotSaveData();
        saveData.prefabIndex = prefabIndex;
        saveData.hasBeenPurchased = hasBeenPurchased;
        saveData.position = transform.position;
        saveData.scale = transform.localScale;
        return saveData;
    }

    public void LoadSaveData(StationPlotSaveData saveData) {
        hasBeenPurchased = saveData.hasBeenPurchased;
        transform.position = saveData.position;
        scale = saveData.scale;
        gameObject.SetActive(!hasBeenPurchased);
    }
}

public class StationPlotSaveData {
    public int prefabIndex;
    public bool hasBeenPurchased;
    public Vector3 position;
    public Vector3 scale;
}
