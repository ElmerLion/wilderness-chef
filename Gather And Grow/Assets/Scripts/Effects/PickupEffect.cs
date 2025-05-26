using System.Collections;
using TMPro;
using UnityEngine;

public class PickupEffect : MonoBehaviour {
    [Header("Animation Settings")]
    [SerializeField] private float riseDistance = 1.0f;
    [SerializeField] private float riseDuration = 0.2f;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private AnimationCurve riseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.Linear(0, 1, 1, 0);

    [Header("References")]
    [SerializeField]  private TextMeshPro amountText;
    [SerializeField]  private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private Transform targetTransform;

    public void Play(Sprite icon, int count, Vector3 from, Transform to) {
        startPosition = from;
        targetTransform = to;
        transform.position = from;

        spriteRenderer.sprite = icon;
        amountText.text = count + "x";

        int sortingOrder = Player.Instance.transform.Find("PlayerSprite").GetComponent<SpriteRenderer>().sortingOrder + 100;
        spriteRenderer.sortingOrder = sortingOrder;
        amountText.sortingOrder = sortingOrder + 50;

        // detach so we can move freely
        transform.SetParent(null);
        StartCoroutine(AnimatePickup());
    }

    private IEnumerator AnimatePickup() {
        // 1) Rise up
        float timer = 0.0f;
        while (timer < riseDuration) {
            timer += Time.deltaTime;
            float t = timer / riseDuration;
            float curveT = riseCurve.Evaluate(t);
            transform.position = startPosition + Vector3.up * (riseDistance * curveT);
            yield return null;
        }

        // 2) Move toward target, fading out
        Vector3 midPosition = transform.position;
        Color initialColor = spriteRenderer.color;
        timer = 0.0f;

        while (timer < moveDuration) {
            timer += Time.deltaTime;
            float t = timer / moveDuration;

            float moveT = moveCurve.Evaluate(t);
            transform.position = Vector3.Lerp(midPosition, targetTransform.position, moveT);

            float fadeT = fadeCurve.Evaluate(t);
            spriteRenderer.color = new Color(
                initialColor.r,
                initialColor.g,
                initialColor.b,
                fadeT
            );

            yield return null;
        }

        Destroy(gameObject);
    }
}
