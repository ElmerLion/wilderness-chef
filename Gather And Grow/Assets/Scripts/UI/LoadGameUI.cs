using NUnit.Framework;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class LoadGameUI : BaseUI {

    public static LoadGameUI Instance { get; private set; }

    [Header("Load Game UI")]
    [SerializeField] private Transform loadSlotsContainer;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Button closeButton;

    [Header("New World Creation")]
    [SerializeField] private Transform newWorldParent;
    [SerializeField] private Button createNewWorldButton;
    [SerializeField] private TMP_InputField worldNameInputField;

    private List<SingleLoadSlotUI> loadSlots = new List<SingleLoadSlotUI>();

    private int maxSaveSlots = 3;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        LoadGameSlots();

        slotPrefab.SetActive(false);

        closeButton.onClick.AddListener(Hide);

        Hide();
    }



    private void LoadGameSlots() {
        string[] saveFiles = new string[0];
        try {
            saveFiles = ES3.GetFiles();
        } catch (System.Exception e) {
            Debug.LogError("Error getting save files: " + e.Message);
        }

        for (int i = 0; i < maxSaveSlots; i++) {
            string saveID = "SaveSlot" + i ;

            GameObject filledSlot = Instantiate(slotPrefab, loadSlotsContainer);
            SingleLoadSlotUI slotUI = filledSlot.GetComponent<SingleLoadSlotUI>();
            loadSlots.Add(slotUI);

            if (saveFiles.ToList().Contains(saveID + ".sav")) {
                slotUI.Initialize(saveID, false);
            } else {
                slotUI.Initialize(saveID, true);
            }
            

            


        }
    }

    public void CreateWorld(string saveId) {
        newWorldParent.gameObject.SetActive(true);

        createNewWorldButton.onClick.RemoveAllListeners();
        createNewWorldButton.onClick.AddListener(() => {
            string saveFilePath = SaveManager.SavePath + saveId + ".sav";
            ES3.Save("SaveName", worldNameInputField.text, saveFilePath);
            SaveManager.Instance.LoadGame(saveId);
            Hide();
        });
    }

    public override void Show() {
        base.Show();
        newWorldParent.gameObject.SetActive(false);
    }
    
}
