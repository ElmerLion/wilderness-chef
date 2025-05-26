using UnityEngine;

[CreateAssetMenu(fileName = "StationSO", menuName = "ScriptableObjects/StationSO", order = 1)]
public class StationSO : ScriptableObject {
    public string nameString;
    public int startingCost;
    public Sprite sprite;
    public GameObject prefab;
}
    
