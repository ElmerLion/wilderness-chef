using UnityEngine;

public class TrashStation : MonoBehaviour, IInteractable {
    [SerializeField] private GameObject _interactPrompt;

    public GameObject InteractPrompt { get => _interactPrompt; set => InteractPrompt = value; }

    private Animator animator;

    private void Start() {
        animator = GetComponent <Animator>();
    }

    public void Interact() {
        InventorySlot selectedSlot = InventoryManager.Instance.GetSelectedSlot();
        if (selectedSlot == null) {
            InventorySlot heldPlate = InventoryManager.Instance.GetHeldPlate();
            heldPlate.Clear();
            AudioManager.Instance.PlaySound(AudioManager.Sound.ThrowTrash, transform.position, 0.5f, true);
            animator.SetTrigger("Throw");
            return;
        }

        selectedSlot.Clear();
        AudioManager.Instance.PlaySound(AudioManager.Sound.ThrowTrash, transform.position, 0.5f, true);
        animator.SetTrigger("Throw");
        return;

    }

    public void InteractAlternate() {
        return;
    }
}
