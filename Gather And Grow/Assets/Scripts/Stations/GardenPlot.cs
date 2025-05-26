using UnityEditor;
using UnityEngine;
using System;

public class GardenPlot : MonoBehaviour, IInteractable {

    [Header("Save")]
    [SerializeField] private string saveID;
    public string SaveID => saveID;

    [Header("References")]
    [SerializeField] private Transform plantSpawnPos;
    [SerializeField] private GameObject _interactPrompt;

    private GameObject basePlantPrefab;
    private Harvestable plantedHarvestable;
    private PlantItemSO plantedItem;

    private bool hasPlantedItem;

    private float growthTimer;

    public GameObject InteractPrompt { get => _interactPrompt; set => InteractPrompt = value; }

    private void Awake() {
        basePlantPrefab = Resources.Load<GameObject>("BasePlant");
    }

    private void Start() {
        HarvestableManager.Instance.RegisterGardenPlot(this);
    }

    private void Update() {
        if (growthTimer > 0f) {
            growthTimer -= Time.deltaTime;
        }
    }

    public void Interact() {
        InventorySlot selectedSlot = InventoryManager.Instance.GetSelectedSlot();

        if (selectedSlot != null && selectedSlot.itemSO != null && selectedSlot.itemSO is PlantItemSO) {
            PlantItemSO plantItemSO = (PlantItemSO)selectedSlot.itemSO;
            if (plantItemSO.plantableInGardenPlot && !hasPlantedItem) {
                PlantItem(selectedSlot, plantItemSO);
                return;
            }
        } 

        if (hasPlantedItem) {
            plantedHarvestable.Harvest();
        }
    }

    private void PlantItem(InventorySlot selectedSlot, PlantItemSO plantItemSO) {
        plantedItem = plantItemSO;
        plantedHarvestable = Instantiate(basePlantPrefab, plantSpawnPos.position, Quaternion.identity).GetComponent<Harvestable>();
        plantedHarvestable.Initialize(plantedItem);
        plantedHarvestable.transform.SetParent(transform);
        plantedHarvestable.gameObject.SetActive(true);

        hasPlantedItem = true;

        selectedSlot.RemoveAmount(1);
    }

    public void InteractAlternate() {
        if (hasPlantedItem && plantedHarvestable != null) {
            if (plantedHarvestable.RegrownItemCount > 0) {
                InventoryManager.Instance.TryAddItem(new InventorySlot(plantedItem, plantedHarvestable.RegrownItemCount, null, plantedItem.dirtyOnHarvest), true);
            }

            AudioManager.Instance.PlaySound(AudioManager.Sound.PickingPlant, transform.position);
            Destroy(plantedHarvestable.gameObject);
            plantedHarvestable = null;
            hasPlantedItem = false;
        }
    }


    public GardenPlotSaveData GetSaveData() {
        GardenPlotSaveData saveData = new GardenPlotSaveData();
        saveData.saveID = saveID;
        if (plantedItem != null) {
            saveData.plantedItemName = plantedItem.nameString;
            saveData.harvestableId = plantedHarvestable.SaveID;
            saveData.growthTimer = growthTimer;
        } else {
            saveData.plantedItemName = "";
            saveData.harvestableId = "";
            saveData.growthTimer = 0f;
        }

        return saveData;
    }

    public void RestoreFromData(GardenPlotSaveData saveData) {
        if (string.IsNullOrEmpty(saveData.harvestableId))
            return;

        PlantItemSO plantedItem = ItemManager.Instance.GetItem(saveData.plantedItemName) as PlantItemSO;

        Harvestable h = HarvestableManager.Instance.GetHarvestableById(saveData.harvestableId);
        if (h == null) {
            GameObject go = Instantiate(basePlantPrefab, plantSpawnPos.position, Quaternion.identity, transform);
            h = go.GetComponent<Harvestable>();
            h.ForceSaveID(saveData.harvestableId);                
            h.Initialize(plantedItem, false);                            
            HarvestableManager.Instance.RegisterHarvestable(h);
        }

        plantedHarvestable = h;

        growthTimer = saveData.growthTimer;
        hasPlantedItem = true;
    }



#if UNITY_EDITOR
    private void OnValidate() {
        if (string.IsNullOrEmpty(saveID)) {
            saveID = System.Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
            return;
        }
        foreach (GardenPlot other in FindObjectsByType<GardenPlot>(FindObjectsSortMode.None)) {
            if (other == this) continue;
            if (other.SaveID == saveID) {
                saveID = System.Guid.NewGuid().ToString();
                EditorUtility.SetDirty(this);
                break;
            }
        }
    }
#endif

}

[Serializable]
public class GardenPlotSaveData {
    public string saveID;
    public string plantedItemName;
    public string harvestableId;
    public float growthTimer;
}
