using UnityEngine;

[CreateAssetMenu(fileName = "Animal", menuName = "ScriptableObjects/Animal", order = 1)]
public class AnimalSO : ScriptableObject {

    [Header("Animal Settings")]
    public string nameString;
    public ItemSO droppedItemSO;
    public int maxDropAmount;
    public int minDropAmount;
    public int maxHealth;
    public GameObject prefab;
    public AudioManager.Sound animalDamaged;
    public AudioManager.Sound animalIdle;

    [Header("Passive Drops")]
    public ItemSO passiveItemDropSO;
    public float timerMaxBetweenDrops;
    public float timerMinBetweenDrops;
    public float dropChance;

    [Header("Alternate Interact")]
    public ItemSO alternateItemSODrop;
    public int alternateAmountToDrop;
    public float alternateCooldown;


}
