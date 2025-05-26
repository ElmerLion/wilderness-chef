using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour {
    public static CameraShake Instance { get; private set; }

    [SerializeField] private CinemachineCamera virtualCamera;

    private CinemachineBasicMultiChannelPerlin perlinNoise;
    private float shakeDuration;
    private float shakeTimer;
    private bool isShaking = false;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        perlinNoise = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
    }

    public void ShakeCamera(float intensity, float duration, float frequency = 1f) {
        if (isShaking) return;

        perlinNoise.AmplitudeGain = intensity;
        perlinNoise.FrequencyGain = frequency;
        shakeDuration = duration;
        shakeTimer = duration;

        isShaking = true;
    }


    private void Update() {
        if (shakeTimer > 0) {
            shakeTimer -= Time.deltaTime;

            if (shakeTimer <= 0) {
                perlinNoise.AmplitudeGain = 0f;
                isShaking = false;
            }
        }
    }
}
