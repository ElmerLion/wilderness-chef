
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FeastAnimal", menuName = "ScriptableObjects/FeastAnimalSO", order = 1)]
public class FeastAnimalSO : ScriptableObject {
    public string animalName;
    public List<MealSO> potentialRequests;
    public Sprite rightSprite;
    public Vector3 scale;
}
