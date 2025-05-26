using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour {

    [SerializeField] private Image progress;
    [SerializeField] private Image checkImage;
    [SerializeField] private Sprite greenProgressBarSprite;
    [SerializeField] private Sprite redProgressBarSprite;

    private void Awake() {
        SetProgress(0);
    }

    private void Start() {
        SetProgressColor(false);
    }

    public void SetProgress(float normalizedValue) {
        progress.fillAmount = normalizedValue;

        if (progress.fillAmount <= 0) {
            gameObject.SetActive(false);
        } else {
            gameObject.SetActive(true);
        }

        if (normalizedValue >= 1) {
            checkImage.gameObject.SetActive(true);
        } else {
            checkImage.gameObject.SetActive(false);
        }
    }

    public void SetProgressColor(bool isRed) {
        if (isRed) {
            progress.sprite = redProgressBarSprite;
        } else {
            progress.sprite = greenProgressBarSprite;
        }
    }
    
}
