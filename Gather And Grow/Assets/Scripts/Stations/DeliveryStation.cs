using System.Collections;
using UnityEngine;

public class DeliveryStation : MonoBehaviour, IInteractable {

    [SerializeField] private GameObject servingDishPop;
    [SerializeField] private GameObject _interactPrompt;
    private Animator popAnimator;

    public GameObject InteractPrompt { get => _interactPrompt; set => InteractPrompt = value; }

    private void Start() {
        popAnimator = servingDishPop.GetComponent<Animator>();
        servingDishPop.SetActive(false);
    }

    public void Interact() {
        InventorySlot plateSlot = InventoryManager.Instance.GetHeldPlate();

        if (plateSlot == null) {
            Debug.Log("No plate in hand");
            return;
        }

        if (plateSlot.worldItem.gameObject.TryGetComponent(out Plate plate)) {
            if (MealOrderManager.Instance.DeliverOrder(plate)) {
                ShowPop();
                AudioManager.Instance.PlaySound(AudioManager.Sound.DeliverySuccess, transform.position, 1, true);
                EconomyManager.Instance.AddMoney(50);
                ParticleEffectsManager.Instance.Play(EffectType.CoinGain, transform.position);
            }
        }

    }

    private void ShowPop() {
        servingDishPop.SetActive(true);

        StartCoroutine(HidePopWhenDone());
    }

    private IEnumerator HidePopWhenDone() {
        var clips = popAnimator.GetCurrentAnimatorClipInfo(0);
        if (clips.Length == 0)
            yield break;

        float clipLength = clips[0].clip.length;

        yield return new WaitForSeconds(clipLength);

        servingDishPop.SetActive(false);
    }


    public void InteractAlternate() {
        return;
    }
}
