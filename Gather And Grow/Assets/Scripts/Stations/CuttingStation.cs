using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CuttingStation : BaseStation {

    [SerializeField] private ProgressBarUI progressBarUI;
    [SerializeField] private TextMeshPro interactPromptText;

    private int cuttingProgress;
    private RecipeSO validRecipe;

    private Animator animator;

    public override void Start() {
        base.Start();

        animator = GetComponent<Animator>();
    }

    public override void InteractAlternate() {
        if (currentIngredients.Count == 0) {
            return;
        }
        if (validRecipe == null) {
            validRecipe = GetValidRecipeWithInput(currentIngredients);
        }

        Cut();
    }

    public override void Interact() {
        base.Interact();

        if (currentIngredients.Count > 0) {
            validRecipe = GetValidRecipeWithInput(currentIngredients);

            if (validRecipe != null) {
                interactPromptText.text = "F";
                return;
            }
        }

        interactPromptText.text = "E";
        cuttingProgress = 0;
        validRecipe = null;
        progressBarUI.SetProgress(0);

    }

    private void Cut() {
       if (currentIngredients.Count == 0 || validRecipe == null) {
            return;
        }

        cuttingProgress++;
        progressBarUI.SetProgress((float)(validRecipe.progressMax - cuttingProgress) / validRecipe.progressMax);

        AudioManager.Instance.PlaySound(AudioManager.Sound.KnifeChopping, transform.position, 0.3f);

        animator.SetBool("Cut", true);
        StartCoroutine(ResetCutting(animator.GetCurrentAnimatorClipInfo(0)[0]));
        //CameraShake.Instance.ShakeCamera(0.1f, 0.2f);

        if (validRecipe.cuttingStationAnimColor != null && validRecipe.cuttingStationAnimColor.Length >= 2) {
            EffectInstance effectInstance = ParticleEffectsManager.Instance.Play(EffectType.StationCut, currentIngredients[0].worldItem.transform.position, false);
            ParticleSystem particleSystem = effectInstance.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule mainModule = particleSystem.main;
            mainModule.startColor = new ParticleSystem.MinMaxGradient(validRecipe.cuttingStationAnimColor[0], validRecipe.cuttingStationAnimColor[1]);
            effectInstance.Init(EffectType.StationCut, ParticleEffectsManager.Instance);
        }

        if (cuttingProgress >= validRecipe.progressMax) {
            cuttingProgress = 0;
            progressBarUI.SetProgress(0);
            animator.SetBool("Cut", false);
            interactPromptText.text = "E";

            currentIngredients[0].SetItem(validRecipe.output[0].itemSO, validRecipe.output[0].amount);
            InvokeRecipeCompleted(validRecipe);

            validRecipe = GetValidRecipeWithInput(currentIngredients);
        }
    }

    private IEnumerator ResetCutting(AnimatorClipInfo clipInfo) {
        yield return new WaitForSeconds(clipInfo.clip.length);
        animator.SetBool("Cut", false);
    }

    public override StationSaveData GetSaveData() {
        StationSaveData data = base.GetSaveData();  

        data.validRecipeName = validRecipe != null ? validRecipe.name : string.Empty;
        data.cuttingProgress = cuttingProgress;

        return data;
    }

    public override void RestoreFromData(StationSaveData data) {
        base.RestoreFromData(data); 

        cuttingProgress = data.cuttingProgress;
        validRecipe = CookBookManager.Instance.GetRecipeByName(data.validRecipeName);

        if (validRecipe != null) {
            progressBarUI.SetProgress((float)(validRecipe.progressMax - cuttingProgress) / validRecipe.progressMax);
        }
    }

}
