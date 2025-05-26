using UnityEngine;
using System.Collections.Generic;
using QFSW.QC;

public class FeastManager : MonoBehaviour {

    public enum FeastAnimalType {
        Fox,
        Bear,
        Goat,
        Owl,
        Squirrel,
        Deer,
    }

    public static FeastManager Instance { get; private set; }

    [SerializeField] private List<FeastSlotStation> feastSlots;
    [SerializeField] private List<FeastAnimalSO> potentialAnimals;
    [SerializeField] private CameraCutscene feastCutscene;
    [SerializeField] private int animalsPerFeast;

    private void Awake() {
        Instance = this;

        foreach (FeastSlotStation slot in feastSlots) {
            slot.gameObject.SetActive(false);
        }
    }



    public void RequestFeast() {
        List<FeastAnimalSO> selectedAnimals = new List<FeastAnimalSO>();
        List<FeastAnimalSO> availableRequests = new List<FeastAnimalSO>(potentialAnimals);

        for (int i = 0; i < animalsPerFeast; i++) {
            if (availableRequests.Count == 0) break;

            int randomIndex = Random.Range(0, availableRequests.Count);
            FeastAnimalSO selectedAnimal = availableRequests[randomIndex];
            availableRequests.RemoveAt(randomIndex);

            selectedAnimals.Add(selectedAnimal);
        }

        for (int i = 0; i < feastSlots.Count; i++) {
            if (i < selectedAnimals.Count) {
                feastSlots[i].Initialize(selectedAnimals[i]);
                feastSlots[i].OnPlateAdded += FeastSlot_OnPlateAdded;
                feastSlots[i].gameObject.SetActive(true);
            } 
        }


    }

    private void FeastSlot_OnPlateAdded() {
        CheckBeginFeast();
    }

    private void CheckBeginFeast() {
        Debug.Log("Checking to begin feast");

        bool allPlatesAdded = true;
        foreach (FeastSlotStation slot in feastSlots) {
            if (slot.placedPlate == null) {
                allPlatesAdded = false;
                break;
            }
        }

        Debug.Log("AllPlatesAdded: " + allPlatesAdded);
        if (!allPlatesAdded) return;

        BeginFeast();

    }

    [Command]
    private void BeginFeast() {
        Debug.Log("Feast Has Begun");
        feastCutscene.PlayCutscene();
    }

}
