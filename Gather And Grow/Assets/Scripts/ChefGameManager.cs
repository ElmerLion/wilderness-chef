using UnityEngine;

public class ChefGameManager : MonoBehaviour {
    private void Start() {
        if (SaveManager.LoadGameOnStart) {
            SaveManager.LoadActiveSaveFile();
        }
    }
}
