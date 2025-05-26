using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Animations;

public class AnimalManager : MonoBehaviour {

    public static AnimalManager Instance { get; private set; }

    [SerializeField] private List<AnimalSO> allAnimalSOs;
    [SerializeField] private List<AnimalSpawner> allAnimalSpawners;

    private Dictionary<string, AnimalSpawner> spawnersById = new Dictionary<string, AnimalSpawner>();
    private const string AnimalsKey = "allAnimals";

    private List<Animal> worldAnimals;

    private void Awake() {
        Instance = this;

        foreach (AnimalSpawner spawner in allAnimalSpawners) {
            if (!spawnersById.ContainsKey(spawner.SpawnerID)) {
                spawnersById.Add(spawner.SpawnerID, spawner);
            }
        }
    }

    private void Start() {
        worldAnimals = new List<Animal>();

        SaveManager.OnGameSaved += OnGameSaved;
        SaveManager.OnGameLoaded += OnGameLoaded;
    }

    private void OnGameSaved(string path) {
        List<AnimalSaveData> animalSaveData = new List<AnimalSaveData>();

        List<Animal> animalsToRemove = new List<Animal>();

        foreach (Animal animal in worldAnimals) {
            if (animal == null) {
                Debug.LogWarning("Animal is null");
                animalsToRemove.Add(animal);
                continue;
            }
            animalSaveData.Add(animal.GetSaveData());
        }

        foreach (Animal animal in animalsToRemove) {
            Debug.Log("Removing animal that is null ");
            worldAnimals.Remove(animal);
        }

        ES3.Save(AnimalsKey, animalSaveData, path);
    }

    private void OnGameLoaded(string path) {
        if (ES3.KeyExists(AnimalsKey, path)) {
            foreach (Animal a in worldAnimals.ToArray()) {
                if (a != null) Destroy(a.gameObject);
            }
            worldAnimals.Clear();

            List<AnimalSaveData> saveData = ES3.Load<List<AnimalSaveData>>(AnimalsKey, path);

            foreach (AnimalSaveData data in saveData) {
                Animal animal = Instantiate(GetAnimalSOByName(data.animalName).prefab).GetComponent<Animal>();
                animal.RestoreFromData(data);
            }
        }

        Debug.Log("Loaded animals: " + worldAnimals.Count);
    }

    public void AddAnimal(Animal animal) {
        if (animal == null) {
            Debug.LogError("Animal is null");
            return;
        }
        animal.OnAnimalDied += Animal_OnAnimalDied;
        worldAnimals.Add(animal);
    }

    private void Animal_OnAnimalDied(Animal animal) {
        worldAnimals.Remove(animal);
    }

    public AnimalSO GetAnimalSOByName(string name) {
        foreach (AnimalSO animalSO in allAnimalSOs) {
            if (animalSO.nameString == name) {
                return animalSO;
            }
        }
        return null;
    }

    public AnimalSpawner GetSpawnerById(string id) {
        if (spawnersById.ContainsKey(id)) {
            return spawnersById[id];
        } else {
            Debug.LogError("Spawner with ID " + id + " not found.");
            return null;
        }
    }

    private void OnDestroy() {
        SaveManager.OnGameSaved -= OnGameSaved;
        SaveManager.OnGameLoaded -= OnGameLoaded;

        foreach (Animal animal in worldAnimals) {
            if (animal != null) {
                animal.OnAnimalDied -= Animal_OnAnimalDied;
            }
        }
    }
}
