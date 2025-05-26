using TMPro;
using UnityEngine;

public class WashingStation : BaseStation {

    [SerializeField] private TextMeshPro interactPromptText;
    [SerializeField] private ProgressBarUI progressBarUI;
    [SerializeField] private Color ingredientTransparecny;

    private int washingProgress;
    private int maxWashingProgress = 3;

    public override void Interact() {
        base.Interact();

        if (currentIngredients.Count > 0) {
            interactPromptText.text = "F";
            currentIngredients[0].worldItem.GetSpriteRenderer().color = ingredientTransparecny;
        } else {
            interactPromptText.text = "E";
            washingProgress = 0;
        }

        

    }

    public override void InteractAlternate() {
        if (currentIngredients.Count == 0) {
            return;
        }

        if (currentIngredients[0].itemSO != null) {
            
            InventorySlot ingredient = currentIngredients[0];

            if (ingredient.isDirty) {
                Wash();
            }

        }
    }

    private void Wash() {
        washingProgress++;
        progressBarUI.SetProgress(washingProgress / (float)maxWashingProgress);
        AudioManager.Instance.PlaySound(AudioManager.Sound.WaterSplash, transform.position, 0.5f, true);
        ParticleEffectsManager.Instance.Play(EffectType.WaterSplash, transform.position);

        if (washingProgress >= maxWashingProgress) {
            washingProgress = 0;
            interactPromptText.text = "E";

            currentIngredients[0].SetIsDirty(false);
            progressBarUI.SetProgress(0);

        }
    }

    public override StationSaveData GetSaveData() {
        StationSaveData data = base.GetSaveData();

        data.cuttingProgress = washingProgress;

        return data;
    }

    public override void RestoreFromData(StationSaveData data) {
        base.RestoreFromData(data);

        washingProgress = data.cuttingProgress;
        progressBarUI.SetProgress(washingProgress / (float)maxWashingProgress);
    }

}
