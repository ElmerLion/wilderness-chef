using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {

    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    private void Start() {
        playButton.onClick.AddListener(OnContinueButtonClicked);
        optionsButton.onClick.AddListener(OnOptionsButtonClicked);
        quitButton.onClick.AddListener(OnQuitButtonClicked);
    }

    private void OnContinueButtonClicked() {
        LoadGameUI.Instance.Show();
    }

    private void OnOptionsButtonClicked() {
        OptionsMenuUI.Instance.Show();
    }

    private void OnQuitButtonClicked() {
        Application.Quit();
    }
    
}
