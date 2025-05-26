using System.Collections;
using UnityEngine;

public class TreeRustle : MonoBehaviour {
    [Header("Rustle Effect")]
    [SerializeField] private GameObject leafParticlePrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.1f, 0f);

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 20f;

    [Header("Timing")]
    [SerializeField] private float minInterval = 30f;
    [SerializeField] private float maxInterval = 150f;

    private float timer;
    private float nextRustleTime;

    private void Start() {
        ScheduleNextRustle();
    }

    private void Update() {
        Vector3 playerPos = Player.Instance.transform.position;
        float dist = Vector3.Distance(playerPos, transform.position);

        if (dist <= detectionRadius) {
            timer += Time.deltaTime;
            if (timer >= nextRustleTime) {
                Rustle();
                timer = 0f;
                ScheduleNextRustle();
            }
        } else {
            timer = 0f;
        }
    }

    private void ScheduleNextRustle() {
        nextRustleTime = Random.Range(minInterval, maxInterval);
    }

    private void Rustle() {
        AudioManager.Instance.PlaySound(AudioManager.Sound.BushRustle, 1f, true);

        Instantiate(
            leafParticlePrefab,
            transform.position + spawnOffset,
            Quaternion.identity
        );
    }
}
