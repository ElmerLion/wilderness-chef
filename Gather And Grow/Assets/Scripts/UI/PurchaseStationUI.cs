using UnityEngine;
using System.Collections.Generic;
using System;

public class PurchaseStationUI : BaseUI {

    public static PurchaseStationUI Instance { get; private set; }

    [SerializeField] private int maxStationCount = 6;
    [SerializeField] private Transform stationContainer;
    [SerializeField] private GameObject stationButtonPrefab;

    private List<PurchaseStationSingleUI> purchaseStationSingleUIs = new List<PurchaseStationSingleUI>();

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        for (int i = 0; i < maxStationCount; i++) {
            Transform newStationUI = Instantiate(stationButtonPrefab, stationContainer).transform;
            PurchaseStationSingleUI purchaseStationSingleUI = newStationUI.GetComponent<PurchaseStationSingleUI>();
            purchaseStationSingleUIs.Add(purchaseStationSingleUI);
        }

        stationButtonPrefab.SetActive(false);
        Hide();
    }

    public void Show(List<StationSO> stationInfoList, Action<StationSO> onPurchase) {
        if (stationInfoList == null || stationInfoList.Count == 0) {
            Debug.LogWarning("PurchaseStationUI.Show called with no stationInfoList");
            return;
        }
        if (onPurchase == null) {
            Debug.LogWarning("PurchaseStationUI.Show called with no onPurchase action");
            return;
        }

        int uiCount = purchaseStationSingleUIs.Count;
        int dataCount = stationInfoList.Count;

        for (int i = 0; i < uiCount; i++) {
            PurchaseStationSingleUI ui = purchaseStationSingleUIs[i];
            if (i < dataCount) {
                ui.gameObject.SetActive(true);
                ui.Initialize(stationInfoList[i], onPurchase);
            } else {
                ui.gameObject.SetActive(false);
            }
        }

        base.Show();
    }
    
}
