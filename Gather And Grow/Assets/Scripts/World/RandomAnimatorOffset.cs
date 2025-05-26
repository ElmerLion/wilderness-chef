using UnityEngine;

public class RandomAnimatorOffset : MonoBehaviour {
    private void Start() {
        Animator animator = GetComponent<Animator>();

        // Optional: small speed variation
        animator.speed = Random.Range(0.95f, 1.05f);

        // Optional: offset the animation start time
        animator.Play(animator.GetCurrentAnimatorStateInfo(0).shortNameHash, 0, Random.value);
    }
}
