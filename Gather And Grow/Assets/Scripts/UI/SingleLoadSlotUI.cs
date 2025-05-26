using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SingleLoadSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private List<GameObject> filledSlotObjects;
    [SerializeField] private List<GameObject> emptySlotObjects;

    [Header("Filled Slot")]
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI deliveredMealsText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteButton;

    [Header("Empty Slot")]
    [SerializeField] private Button createButton;

    private Animator animator;
    private bool isEmpty;
    private string saveID;

    private void Start() {
        animator = GetComponent<Animator>();
    }

    public void Initialize(string saveFile, bool isEmpty) {
        saveID = saveFile;
        this.isEmpty = isEmpty;

        if (isEmpty) {
            createButton.onClick.AddListener(() => {
                SaveManager.Instance.SaveGame(saveFile);
                LoadGameUI.Instance.CreateWorld(saveFile);
            });
            foreach (GameObject obj in filledSlotObjects) {
                obj.SetActive(false);
            }
            foreach (GameObject obj in emptySlotObjects) {
                obj.SetActive(true);
            }
            return;
        }

        foreach (GameObject obj in emptySlotObjects) {
            obj.SetActive(false);
        }
        foreach (GameObject obj in filledSlotObjects) {
            obj.SetActive(true);
        }

        string saveFilePath = SaveManager.SavePath + saveFile + ".sav";

        string fileName = ES3.Load<string>("SaveName", saveFilePath, "No Name");
        string rank = ES3.Load<string>("LatestRankName", saveFilePath, "No Rank");
        int deliveredMeals = ES3.Load<int>(RankManager.DeliveredMealsKey, saveFilePath, 0);
        string date = ES3.Load<string>("SavedDate", saveFilePath, "No Date");

        title.text = fileName;
        rankText.text = rank;
        deliveredMealsText.text = "Delivered Meals:  " + deliveredMeals;
        dateText.text = "Last Played: \n" + date;

        loadButton.onClick.AddListener(() => {
            SaveManager.Instance.LoadGame(saveFile);
        });

        deleteButton.onClick.AddListener(() => {
            SaveManager.Instance.DeleteSaveGame(saveFile);
            DeleteSlot();
            // Set as empty
        });
    }

    public void DeleteSlot() {
        Initialize(saveID, true);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (isEmpty) return;
        animator.SetBool("Hover", true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (isEmpty) return;
        animator.SetBool("Hover", false);
    }
}
