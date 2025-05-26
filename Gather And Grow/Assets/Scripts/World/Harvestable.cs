using System;
using UnityEditor;
using UnityEngine;

public class Harvestable : MonoBehaviour, IInteractable {

    [Header("Save")]
    [SerializeField] private string saveID;
    public string SaveID => saveID;

    [Header("General")]
    [SerializeField] private PlantItemSO itemSO;
    [SerializeField] private GameObject _interactPrompt;

    [Header("Destroyable")]
    [SerializeField] public int maxHealth;

    private bool isDestroyable = false;
    private bool setAsDirtyOnHarvest = false;

    private float harvestCooldown = 0.3f;
    private float regrowTimeMax = 30f;
    private int maxRegrownItemCount = 3;

    private Sprite harvestedSprite;
    private Sprite regrownSprite;

    public GameObject InteractPrompt { get => _interactPrompt; set => _interactPrompt = value; }

    public int RegrownItemCount {get; private set; } = 1;
    public int CurrentHealth { get; private set; }
    public float harvestTimer { get; private set; }
    public float RegrowTimer { get; private set; }
    public bool IsDestroyed { get; private set; } = false;

    private SpriteRenderer spriteRenderer;

    public void Initialize(PlantItemSO plantItemSO, bool generateNewId = true) {
        this.itemSO = plantItemSO;

        regrownSprite = plantItemSO.regrownSprite;
        harvestedSprite = plantItemSO.harvestedSprite;
        regrowTimeMax = plantItemSO.regrowTime;
        maxRegrownItemCount = plantItemSO.maxRegrowAmount;

        RegrownItemCount = 0;
        RegrowTimer = 0f;

        isDestroyable = plantItemSO.destroyable;
        setAsDirtyOnHarvest = plantItemSO.dirtyOnHarvest;
        harvestCooldown = plantItemSO.harvestCooldown;

        spriteRenderer.transform.localScale = plantItemSO.plantScale;
        
        spriteRenderer.sprite = harvestedSprite;

        if (generateNewId) {
            saveID = System.Guid.NewGuid().ToString();
        } 
    }

    private void Awake() {
        if (!transform.TryGetComponent(out spriteRenderer)) {
            spriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        }

        spriteRenderer.material = new Material(spriteRenderer.material);

        regrownSprite = itemSO.regrownSprite;
        harvestedSprite = itemSO.harvestedSprite;
        regrowTimeMax = itemSO.regrowTime;
        maxRegrownItemCount = itemSO.maxRegrowAmount;

        isDestroyable = itemSO.destroyable;
        setAsDirtyOnHarvest = itemSO.dirtyOnHarvest;
        harvestCooldown = itemSO.harvestCooldown;
    }

    private void Start() {
        if (HarvestableManager.Instance == null) {
            return;
        }
        CurrentHealth = maxHealth;
        IsDestroyed = false;
        HarvestableManager.Instance.RegisterHarvestable(this);

        _interactPrompt.gameObject.SetActive(false);
    }

    private void Update() {
        if (harvestTimer > 0) {
            harvestTimer -= Time.deltaTime;
        }
        if (!isDestroyable && RegrownItemCount != maxRegrownItemCount) {
            RegrowTimer += Time.deltaTime;
            if (RegrowTimer >= regrowTimeMax) {
                RegrownItemCount++;
                RegrowTimer = 0f;
                spriteRenderer.sprite = regrownSprite;
            }
        }
    }

    public void Interact() {
        if (harvestTimer > 0) {
            return;
        }
        harvestTimer = harvestCooldown;

        Harvest();
    }

    public void Harvest() {
        if (InventoryManager.Instance.hasHeldPlate) return;

        if (isDestroyable) {
            if (CurrentHealth <= 0) {
                InventoryManager.Instance.TryAddItem(new InventorySlot(itemSO, 1, null, setAsDirtyOnHarvest), true, false, true, transform.position);
                Destroy(gameObject);
            } else {
                CurrentHealth--;
            }
        } else {

            if (RegrownItemCount <= 0) {
                return;
            }

            if (InventoryManager.Instance.TryAddItem(new InventorySlot(itemSO, 1, null, setAsDirtyOnHarvest), true, false, true, transform.position)) {
                AudioManager.Instance.PlaySound(AudioManager.Sound.PickingPlant, transform.position);
                if (itemSO.dirtyOnHarvest) {
                    ParticleEffectsManager.Instance.Play(EffectType.DirtPickup, transform.position - new Vector3(0, 0.1f, 0));
                }
                RegrownItemCount--;
                if (RegrownItemCount == 0) {
                    spriteRenderer.sprite = harvestedSprite;
                }
            }
        }

        if (!isDestroyable && RegrownItemCount == 0) {
            spriteRenderer.sprite = harvestedSprite;
        }

    }

    public void ApplyState(HarvestableData data) {
        if (data.isDestroyed) {
            Destroy(gameObject);
            return;
        }
        CurrentHealth = data.currentHealth;
        RegrownItemCount = data.regrownItemCount;
        RegrowTimer = data.regrowTimer;

        spriteRenderer.sprite =
                (RegrownItemCount > 0) ? regrownSprite : harvestedSprite;
    }

    public void InteractAlternate() {
        return;
    }

    public float GetHarvestTimer() {
        return harvestTimer;
    }

    public void ForceSaveID(string id) {
        this.saveID = id;
    }


#if UNITY_EDITOR
    private void OnValidate() {
        if (string.IsNullOrEmpty(saveID)) {
            saveID = System.Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
            return;
        }
        foreach (Harvestable other in FindObjectsByType<Harvestable>(FindObjectsSortMode.None)) {
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
public struct HarvestableData {
    public string saveID;
    public bool isDestroyed;
    public int currentHealth;
    public int regrownItemCount;
    public float regrowTimer;
}

