using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MixerStation : BaseStation {
    [SerializeField] private ProgressBarUI progressBarUI;
    [SerializeField] private GameObject cookingEffect;

    private float processingTimer;
    private RecipeSO validRecipe;

    public override void Start() {
        base.Start();
        cookingEffect.SetActive(false);
    }

    private void Update() {
        if (processingTimer > 0f) {
            if (validRecipe == null) {
                ResetProcessing();
                return;
            }

            processingTimer -= Time.deltaTime;
            float progress = (validRecipe.timeToCook - processingTimer) / validRecipe.timeToCook;
            progressBarUI.SetProgress(progress);

            if (processingTimer <= 0f)
                ProcessRecipe();
        }
    }

    private void ResetProcessing() {
        processingTimer = 0f;
        progressBarUI.SetProgress(0f);
        cookingEffect.SetActive(false);
    }

    public override void Interact() {
        InventorySlot selectedSlot = InventoryManager.Instance.GetSelectedSlot();

        // 1) Remove last ingredient if player is empty-handed
        if (selectedSlot == null || selectedSlot.itemSO == null) {
            if (currentIngredients.Any()) {
                TryTakeIngredient(currentIngredients.Count - 1);
            }
            return;
        }

        if (CanAcceptAsNextIngredient(selectedSlot)) {
            AddIngredient(selectedSlot);
            TryStartProcessing();
        } else {
            Player.Instance.SetPlayerThought("This ingredient won't mix into anything useful...", 3f);
        }
    }

    private void TryStartProcessing() {
        if (!currentIngredients.Any())
            return;

        RecipeSO candidate = GetValidRecipeWithInput(currentIngredients);
        if (candidate == validRecipe)
            return;
        validRecipe = candidate;

        if (validRecipe == null)
            return;

        bool canProcess = validRecipe.input.All(req =>
            currentIngredients.Any(slot =>
                slot.itemSO == req.itemSO &&
                slot.amount >= req.amount
            )
        );

        if (!canProcess)
            return;

        processingTimer = validRecipe.timeToCook;
        progressBarUI.SetProgress(0f);
        cookingEffect.SetActive(true);
    }

    private void ProcessRecipe() {
        foreach (ItemAmount req in validRecipe.input) {
            InventorySlot slot = currentIngredients.FirstOrDefault(s => s.itemSO == req.itemSO);
            if (slot != null) {
                slot.RemoveAmount(req.amount);
                if (slot.amount <= 0)
                    currentIngredients.Remove(slot);
            }
        }

        foreach (ItemAmount outAmt in validRecipe.output) {
            InventorySlot newSlot = new InventorySlot(outAmt.itemSO, outAmt.amount);
            currentIngredients.Add(newSlot);

            GameObject go = Instantiate(itemPrefab, ingredientPlaceTranformList[currentIngredients.Count - 1]);
            Item item = go.GetComponent<Item>();
            newSlot.SetWorldItem(item);

            if (itemTooltips != null) {
                ItemTooltipUI tip = Instantiate(itemTooltipPrefab, itemTooltips.transform)
                                      .GetComponent<ItemTooltipUI>();
                tip.SetItem(newSlot);
            }
        }

        ResetProcessing();
        InvokeRecipeCompleted(validRecipe);
    }

    public override StationSaveData GetSaveData() {
        StationSaveData data = base.GetSaveData();
        data.processingTimer = processingTimer;
        data.validRecipeName = validRecipe != null ? validRecipe.name : string.Empty;
        return data;
    }

    public override void RestoreFromData(StationSaveData data) {
        base.RestoreFromData(data);
        processingTimer = data.processingTimer;
        validRecipe = CookBookManager.Instance.GetRecipeByName(data.validRecipeName);
        if (validRecipe != null) {
            float prog = (validRecipe.timeToCook - processingTimer) / validRecipe.timeToCook;
            progressBarUI.SetProgress(prog);
            if (processingTimer > 0f)
                cookingEffect.SetActive(true);
        }
    }
}
