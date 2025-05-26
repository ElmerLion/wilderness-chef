using TMPro;
using UnityEngine;

public class EconomyUI : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI moneyText;

    private void Start() {
        UpdateMoneyText();
        EconomyManager.Instance.OnMoneyChanged += UpdateMoneyText;
    }

    private void UpdateMoneyText(int amount = 0) {
        moneyText.text = amount + "C";
    }

}
