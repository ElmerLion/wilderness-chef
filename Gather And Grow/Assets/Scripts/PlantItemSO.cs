using UnityEngine;

[CreateAssetMenu(fileName = "NewPlantItem", menuName = "ScriptableObjects/PlantItem")]
public class PlantItemSO : ItemSO {

    [Header("Plant Options")]
    public bool dirtyOnHarvest = false;
    public bool destroyable = false;
    public bool plantableInGardenPlot = true;
    [Space]
    public Sprite regrownSprite;
    public Sprite harvestedSprite;
    [Space]
    public float regrowTime = 60f;
    public int maxRegrowAmount = 2;
    public float harvestCooldown = 0.3f;
    [Space]
    public Vector3 plantScale = new Vector3(0.1f, 0.1f, 0.1f);

}
