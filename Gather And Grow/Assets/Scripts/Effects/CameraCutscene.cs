using Unity.Cinemachine;
using UnityEngine;

public class CameraCutscene : MonoBehaviour {


    [Header("Cutscene Settings")]
    [SerializeField] private CinemachineCamera cutsceneCamera;
    [SerializeField] private Transform startPos;
    [SerializeField] private Transform endPos;
    [SerializeField] private float cutsceneDuration = 5f;

    private float elapsed;
    private bool playing;

    private void Awake() {
        cutsceneCamera.Priority = 0;
    }

    private void Update() {
        if (!playing) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / cutsceneDuration);

        cutsceneCamera.transform.position = Vector3.Lerp(startPos.position + new Vector3(0,0,-10), endPos.position + new Vector3(0, 0, -10), t);
        cutsceneCamera.transform.rotation = Quaternion.Lerp(startPos.rotation, endPos.rotation, t);

        if (t >= 1f ) {
            EndCutscene();
        }
    }

    public void PlayCutscene() {
        elapsed = 0f;
        playing = true;
        cutsceneCamera.Priority = 20; 
    }

    private void EndCutscene() {
        playing = false;
        cutsceneCamera.Priority = 0;
        cutsceneCamera.transform.position = startPos.position + new Vector3(0, 0, -10);
        cutsceneCamera.transform.rotation = startPos.rotation;
    }

}
