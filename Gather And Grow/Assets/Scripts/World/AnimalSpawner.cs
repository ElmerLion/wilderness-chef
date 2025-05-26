using System;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class AnimalSpawner : MonoBehaviour {

    [SerializeField]
    private string spawnerID;
    public string SpawnerID => spawnerID;

#if UNITY_EDITOR
    private void OnValidate() {
        if (string.IsNullOrEmpty(spawnerID)) {
            spawnerID = GUID.Generate().ToString();
            EditorUtility.SetDirty(this);
            return;
        }
        foreach (AnimalSpawner other in FindObjectsByType<AnimalSpawner>(FindObjectsSortMode.None)) {
            if (other == this) continue;
            if (other.spawnerID == spawnerID) {
                spawnerID = GUID.Generate().ToString();
                EditorUtility.SetDirty(this);
                break;
            }
        }
    }
#endif

    [SerializeField] private GameObject animalPrefabToSpawn;
    [SerializeField] private float spawnTimerMax = 200f;
    [SerializeField] private int maxSpawnedAnimals = 2;

    public bool IsActive { get; set; } = false;
    private int spawnedAnimalsCount = 0;
    private float spawnTimer;

    private void Start() {
        spawnTimer = 10;
    }

    private void Update() {
        if (!IsActive) {
            return;
        }
        if (spawnedAnimalsCount >= maxSpawnedAnimals) {
            return;
        }
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f) {
            SpawnAnimal();
            spawnTimer = spawnTimerMax;
        }
    }

    private void SpawnAnimal() {

        GameObject animal = Instantiate(animalPrefabToSpawn, transform.position, Quaternion.identity);
        animal.transform.SetParent(transform);
        spawnedAnimalsCount++;

        Animal animalClass = animal.GetComponent<Animal>();

        animalClass.Initialize(this);
        animalClass.OnAnimalDied += AnimalSpawner_OnAnimalDied;
    }

    private void AnimalSpawner_OnAnimalDied(Animal a) {
        spawnedAnimalsCount--;
    }

    public void AddAnimal(Animal animal) {
        if (animal == null) {
            Debug.LogWarning("Animal is null");
            return;
        }
        animal.OnAnimalDied += AnimalSpawner_OnAnimalDied;
        spawnedAnimalsCount++;
    }
}
