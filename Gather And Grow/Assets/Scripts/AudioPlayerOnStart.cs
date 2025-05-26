using UnityEngine;

public class AudioPlayerOnStart : MonoBehaviour {

    [SerializeField] private AudioManager.Sound sound;
    [SerializeField] private float volumeMultiplier = 0.5f;


    void Start() {
        AudioManager.Instance.PlaySound(sound, transform.position, volumeMultiplier);
    }

}
