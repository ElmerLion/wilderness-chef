using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeastSlotStation : BaseStation {


    public enum Placement {
        Left,
        Right,
    }

    [SerializeField] private Placement placement;
    [SerializeField] private TextMeshProUGUI mealRequestName;
    [SerializeField] private List<Image> ingredientImageList;
    [SerializeField] private Transform heartEffectSpawnPoint;

    public event Action OnPlateAdded;
    public Plate placedPlate { get; private set; }

    private FeastAnimalSO feastAnimalSO;
    private MealSO requestedMeal;

    public void Initialize(FeastAnimalSO feastAnimalSO) {
        this.feastAnimalSO = feastAnimalSO;
        requestedMeal = feastAnimalSO.potentialRequests[UnityEngine.Random.Range(0, feastAnimalSO.potentialRequests.Count)];

        SpriteRenderer spriteRenderer = transform.Find("SpriteParent").GetChild(0).GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = feastAnimalSO.rightSprite;
        spriteRenderer.transform.localScale = feastAnimalSO.scale;
        spriteRenderer.transform.rotation = GetRotation();

        mealRequestName.text = requestedMeal.nameString;

        List<Image> ingredientImageListToUse = new List<Image>(ingredientImageList);
        for (int i = 0; i < requestedMeal.ingredientItemsList.Count; i++) {
            ingredientImageList[i].sprite = requestedMeal.ingredientItemsList[i].icon;
            ingredientImageList[i].gameObject.SetActive(true);
            ingredientImageListToUse.Remove(ingredientImageList[i]);
        }

        foreach (IngredientCategory ingredientCategory in requestedMeal.ingredientCategoryItems) {
            if (ingredientImageListToUse.Count <= 0) break;
            Image ingredientImage = ingredientImageListToUse[0];
            ingredientImage.sprite = MealOrderUI.Instance.GetDefaultIcon(ingredientCategory);
            ingredientImage.gameObject.SetActive(true);
            ingredientImageListToUse.RemoveAt(0);
        }

        foreach (Image ingredientImage in ingredientImageListToUse) {
            ingredientImage.gameObject.SetActive(false);
        }
    }

    public override void Interact() {
        if (!InventoryManager.Instance.hasHeldPlate && currentIngredients.Count <= 0) {
            Player.Instance.SetPlayerThought("I should serve it on a plate.", 2f);
            return;
        }

        InventorySlot plateSlot = InventoryManager.Instance.GetHeldPlate();

        if (plateSlot != null && plateSlot.worldItem != null) {
            if (plateSlot.worldItem.TryGetComponent(out Plate plate)) {
                if (plate.DoesPlateMatchMeal(requestedMeal)) {
                    base.Interact();

                    placedPlate = plate;
                    OnPlateAdded?.Invoke();

                    ParticleEffectsManager.Instance.Play(EffectType.Hearts, heartEffectSpawnPoint.position);
                    return;
                }
            }
        }

        if (currentIngredients.Count > 0) {
            base.Interact();
            placedPlate = null;

            OnPlateAdded?.Invoke();
        }


    }

   

    public override void InteractAlternate() {
        return;
    }

    public Quaternion GetRotation() {
        switch (placement) {
            case Placement.Left:
                return Quaternion.Euler(0, -180, 0);
            case Placement.Right:
                return Quaternion.Euler(0, 0, 0);
        }
        return Quaternion.identity;
    }

}
