using Michsky.UI.Heat;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuUI : BaseUI {

    public static OptionsMenuUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Button closeButton;

    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider environmentVolumeSlider;

    private void Awake() {
        Instance = this;
        closeButton.onClick.AddListener(Hide);
    }

    private void Start() {

        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        masterVolumeSlider.value = master * 100;
        float music = PlayerPrefs.GetFloat("MusicVolume", 1f);
        musicVolumeSlider.value = music * 100;
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);
        sfxVolumeSlider.value = sfx * 100;
        float environment = PlayerPrefs.GetFloat("EnvironmentVolume", 1f);
        environmentVolumeSlider.value = environment * 100;

        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        environmentVolumeSlider.onValueChanged.AddListener(SetEnvironmentVolume);

        Hide();
    }

    private void SetMasterVolume(float value) {
        float normalizedValue = value / 100;
        AudioManager.Instance.SetVolume(normalizedValue, AudioManager.AudioType.Master);
        PlayerPrefs.SetFloat("MasterVolume", normalizedValue);
    }

    private void SetMusicVolume(float value) {
        float normalizedValue = value / 100;
        AudioManager.Instance.SetVolume(normalizedValue, AudioManager.AudioType.Music);
        PlayerPrefs.SetFloat("MusicVolume", normalizedValue);
    }

    private void SetSFXVolume(float value) {
        float normalizedValue = value / 100;
        AudioManager.Instance.SetVolume(normalizedValue, AudioManager.AudioType.SFX);
        PlayerPrefs.SetFloat("SFXVolume", normalizedValue);
    }

    private void SetEnvironmentVolume(float value) {
        float normalizedValue = value / 100;
        AudioManager.Instance.SetVolume(normalizedValue, AudioManager.AudioType.Environment);
        PlayerPrefs.SetFloat("EnvironmentVolume", normalizedValue);
    }
    
}
