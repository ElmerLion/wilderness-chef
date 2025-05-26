using TMPro;
using UnityEngine;

public class MessageUI : MonoBehaviour {

    public static MessageUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI messageText;

    private float timer;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        HideMessage();
    }

    private void Update() {
        if (gameObject.activeSelf) {
            timer -= Time.deltaTime;
            if (timer <= 0) {
                HideMessage();
            }
        }
    
    }

    public void ShowMessage(string message, float duration) {
        messageText.text = message;
        timer = duration;
        gameObject.SetActive(true);
    }

    public void HideMessage() {
        gameObject.SetActive(false);
    }




}
