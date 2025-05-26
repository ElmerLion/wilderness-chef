using UnityEngine;
using TMPro;

public class YSorter : MonoBehaviour {
    [SerializeField] private int sortingOffset = 0;
    [SerializeField] private float precision = 100f;

    private SpriteRenderer spriteRenderer;
    private ParticleSystemRenderer particleSystemRenderer;
    private Canvas canvas;
    private TextMeshPro textMeshPro;
    private Renderer textMeshRenderer;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
        canvas = GetComponent<Canvas>();
        textMeshPro = GetComponent<TextMeshPro>();
        if (textMeshPro != null) {
            textMeshRenderer = textMeshPro.GetComponent<Renderer>();
        }
    }

    private void LateUpdate() {
        int sortingOrder = -(int)(transform.position.y * precision) + sortingOffset;

        if (spriteRenderer != null) {
            spriteRenderer.sortingOrder = sortingOrder;
        }

        if (particleSystemRenderer != null) {
            particleSystemRenderer.sortingOrder = sortingOrder;
        }

        if (canvas != null) {
            canvas.sortingOrder = sortingOrder;
        }

        if (textMeshRenderer != null) {
            textMeshRenderer.sortingOrder = sortingOrder;
        }
    }

    public void SetSortingOffset(int offset) {
        sortingOffset = offset;
    }
}
