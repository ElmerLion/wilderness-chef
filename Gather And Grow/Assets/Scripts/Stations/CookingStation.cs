using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CookingStation : BaseStation {
    
    public enum State {
        Idle,
        Frying,
        Finished,
        Burnt
    }

    [Header("References")]
    [SerializeField] private EffectType cookingEffect;
    [SerializeField] private AudioManager.Sound cookingSound;
    [SerializeField] private Transform effectPos;
    [SerializeField] private ProgressBarUI progressBarUI;
    [SerializeField] private Color ingredientTransparency;

    private int prevIngCount;
    private RecipeSO activeRecipe;
    private float cookingTimer;
    private State currentState;

    private Color defaultColor = Color.white;

    private AudioSource audioSource;
    private EffectInstance effectInstance;

    // Check for matching ingredients when trying to add another one
    public override void Start() {
        base.Start();

        OnIngredientRemoved += CookingStation_OnIngredientRemoved;
    }

    private void CookingStation_OnIngredientRemoved(InventorySlot obj) {
        if (obj.worldItem == null) return;

        obj.worldItem.GetSpriteRenderer().color = defaultColor;
    }

    private void Update() {
        if (currentIngredients.Count <= 0 || activeRecipe == null) return;
        InventorySlot firstIngredient = currentIngredients[0];

        switch (currentState) {
            case State.Idle:
                if (firstIngredient.itemSO != activeRecipe.output[0].itemSO) {
                    progressBarUI.SetProgressColor(!activeRecipe.showInCookbook);
                    currentState = State.Frying;
                }
                break;
            case State.Frying:
                if (activeRecipe == null) return;

                cookingTimer -= Time.deltaTime;
                foreach (InventorySlot ingSlot in currentIngredients) {
                    ingSlot.SetTimer(cookingTimer);
                }
                progressBarUI.SetProgress((activeRecipe.timeToCook - cookingTimer) / activeRecipe.timeToCook);

                if (cookingTimer <= 0) {
                    SetNewOutput(firstIngredient, activeRecipe);
                    InvokeRecipeCompleted(activeRecipe);

                    activeRecipe = GetValidRecipeWithInput(currentIngredients);
                    foreach (InventorySlot ingSlot in currentIngredients) {
                        ingSlot.SetTimer(0);
                    }

                    if (activeRecipe == null) return;
                    progressBarUI.SetProgress(0);
                    progressBarUI.SetProgressColor(true);
                    cookingTimer = activeRecipe.timeToCook;
                    currentState = State.Finished;
                }
                break;
            case State.Finished:
                if (activeRecipe == null) return;
                cookingTimer -= Time.deltaTime;
                foreach (InventorySlot ingSlot in currentIngredients) {
                    ingSlot.SetTimer(cookingTimer);
                }
                progressBarUI.SetProgress((activeRecipe.timeToCook - cookingTimer) / activeRecipe.timeToCook);
                if (cookingTimer <= 0) {
                    effectInstance?.Stop();
                    SetNewOutput(firstIngredient, activeRecipe);
                    currentState = State.Burnt;
                }

                break;
            case State.Burnt:
                SetNewOutput(firstIngredient, activeRecipe);
                progressBarUI.SetProgress(0);
                foreach (InventorySlot ingSlot in currentIngredients) {
                    ingSlot.SetTimer(0);
                }
                progressBarUI.SetProgressColor(false);
                currentState = State.Idle;
                break;
        }
    }

    private void SetNewOutput(InventorySlot slot, RecipeSO recipe) {
        if (slot == null || recipe == null) return;

        slot.SetItem(recipe.output[0].itemSO, recipe.output[0].amount);

        for (int i = 1; i < currentIngredients.Count; i++) {
            InventorySlot ingSlot = currentIngredients[i];
            if (ingSlot.worldItem != null) {
                ingSlot.worldItem.DestroyItem();
            }
            ingSlot.Clear();
            currentIngredients.Remove(ingSlot);
        }
    }

    public override void Interact() {
        InventorySlot selectedSlot = InventoryManager.Instance.GetSelectedSlot();

        // 1) Remove last ingredient if player is empty-handed
        if (selectedSlot == null || selectedSlot.itemSO == null) {
            if (currentIngredients.Any()) {
                TryTakeIngredient(currentIngredients.Count - 1);

                if (currentIngredients.Count <= 0) {
                    currentState = State.Idle;
                    progressBarUI.SetProgress(0);
                    effectInstance?.Stop();
                }
            }
            return;
        }

        if (CanAcceptAsNextIngredient(selectedSlot)) {
            AddIngredient(selectedSlot);
            activeRecipe = GetValidRecipeWithInput(currentIngredients);

            InventorySlot ingToUse = currentIngredients[currentIngredients.Count - 1];

            ingToUse.worldItem.GetSpriteRenderer().color = ingredientTransparency;

            currentState = State.Idle;

            if (effectInstance == null) {
                effectInstance = ParticleEffectsManager.Instance.Play(cookingEffect, effectPos.position);
            }

            if (activeRecipe != null && ingToUse.timer <= 0) {
                ingToUse.SetTimer(activeRecipe.timeToCook);
                cookingTimer = activeRecipe.timeToCook;
            } else {
                cookingTimer = ingToUse.timer;
            }
        } else {
            Player.Instance.SetPlayerThought("This ingredient won't work with this...", 3f);
        }

        prevIngCount = currentIngredients.Count;
    }

    public override StationSaveData GetSaveData() {
        StationSaveData data = base.GetSaveData();
        /*
        List<float> timers = new List<float>();
        List<int> states = new List<int>();
        List<string> recipeNames = new List<string>();

        foreach (ProcessingIngredient processIng in processingIngredients) {
            timers.Add(processIng.cookingTimer);
            states.Add((int)processIng.currentState);
            recipeNames.Add(processIng.activeRecipe.name);
        }

        data.processingCount = processingIngredients.Count;
        data.processingTimers = timers;
        data.processingStates = states;
        data.processingRecipeNames = recipeNames;*/


        return data;
    }

    public override void RestoreFromData(StationSaveData saveData) {
        base.RestoreFromData(saveData);

        /*int processingCount = saveData.processingCount;

        processingIngredients.Clear();

        for (int i = 0; i < processingCount; i++) {
            float timer = saveData.processingTimers[i];
            State state = (State)saveData.processingStates[i];
            string recipeName = saveData.processingRecipeNames[i];
            RecipeSO recipe = CookBookManager.Instance.GetRecipeByName(recipeName);

            var slot = currentIngredients[i];
            var pi = new ProcessingIngredient(slot) {
                cookingTimer = timer,
                currentState = state,
                activeRecipe = recipe
            };
            slot.OnItemChanged += InventorySlot_OnItemChanged;
            processingIngredients.Add(pi);
        }

        if (processingIngredients.Count > 0) {
            effectInstance = ParticleEffectsManager.Instance.Play(cookingEffect, effectPos.position);
        }*/
    }
}

