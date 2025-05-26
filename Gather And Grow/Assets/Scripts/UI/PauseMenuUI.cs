using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuUI : BaseUI {
    public static PauseMenuUI Instance { get; private set; }

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake() {
        Instance = this;

        resumeButton.onClick.AddListener(OnResumeButtonClicked);
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        optionsButton.onClick.AddListener(OnOptionsButtonClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
    }

    private void Start() {
        Hide();
    }

    private void OnResumeButtonClicked() {
        Hide();
    }

    private void OnSaveButtonClicked() {
        SaveManager.Instance.SaveGame(SaveManager.CurrentSaveFileName);
    }

    private void OnOptionsButtonClicked() {
        OptionsMenuUI.Instance.Show();
    }

    private void OnMainMenuButtonClicked() {
        SaveManager.Instance.SaveGame(SaveManager.CurrentSaveFileName);
        Loader.Load(Loader.Scene.MainMenuScene);
    }

}
