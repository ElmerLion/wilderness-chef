using UnityEngine;

public class Bush : MonoBehaviour {

    [SerializeField] private GameObject leafParticlePrefab;

    private Animator animator;

    private void Start() {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.TryGetComponent(out Player player)) {
            AudioManager.Instance.PlaySound(AudioManager.Sound.BushRustle);
            Instantiate(leafParticlePrefab, transform.position + new Vector3(0, 0.1f, 0), Quaternion.identity);

            if (animator != null) {
                animator.SetTrigger("Wobble");
            }
        }
    }

}
