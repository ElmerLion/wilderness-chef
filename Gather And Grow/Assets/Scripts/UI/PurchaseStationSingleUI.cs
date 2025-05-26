using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseStationSingleUI : MonoBehaviour {

    [SerializeField] private Image stationImage;
    [SerializeField] private TextMeshProUGUI stationNameText;
    [SerializeField] private TextMeshProUGUI stationCost;

    private Button button;
    private StationSO stationSO;

    private void Start() {
        button = GetComponent<Button>();
    }

    public void Initialize(StationSO stationInfo, Action<StationSO> onPurchase) {
        if (button == null) {
            button = GetComponent<Button>();
        }

        stationImage.sprite = stationInfo.sprite;
        stationNameText.text = stationInfo.nameString;
        stationCost.text = stationInfo.startingCost + "C";

        stationSO = stationInfo;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => {
            onPurchase?.Invoke(stationSO);
        });
    }
    
}
