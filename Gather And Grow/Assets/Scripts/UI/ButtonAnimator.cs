using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private Animator animator;

    private void Start() {
        animator = GetComponent<Animator>();
    
    }

    public void OnPointerEnter(PointerEventData eventData) {
        animator.SetBool("Hover", true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        animator.SetBool("Hover", false);
    }
}
