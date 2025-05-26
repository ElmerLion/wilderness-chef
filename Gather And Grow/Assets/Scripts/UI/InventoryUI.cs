using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour {

    public static InventoryUI Instance { get; private set; }

    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private Transform inventorySlotsParent;
    [SerializeField] private Transform blockOverlay;
    [SerializeField] private Image heldItemIcon;
    [SerializeField] private TextMeshProUGUI heldItemAmountText;
    [SerializeField] private GameObject heldItemObject;

    private List<InventorySlotUI> inventorySlotsUI = new List<InventorySlotUI>();
    private Vector3 velocity;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        heldItemIcon.gameObject.SetActive(false);
        heldItemAmountText.gameObject.SetActive(false);
        blockOverlay.gameObject.SetActive(false);

        if (InventoryManager.Instance.hasHeldPlate) {
            SetPlateHeld(true);
        }

        InventoryManager.Instance.OnSelectedSlotChanged += Instance_OnSelectedSlotChanged;
    }

    private void Instance_OnSelectedSlotChanged(InventorySlot obj, int index) {
        foreach (InventorySlotUI slotUI in inventorySlotsUI) {
            slotUI.SetSelected(false);
        }

        if (index >= 0 && index < inventorySlotsUI.Count) {
            inventorySlotsUI[index].SetSelected(true);
        }
    }

    private void Update() {
        if (InventoryManager.Instance.hasHeldItem) {
            Vector3 targetPosition = Input.mousePosition + new Vector3(32f, -32f, 0f);
            heldItemObject.transform.position = Vector3.SmoothDamp(
                heldItemObject.transform.position,
                targetPosition,
                ref velocity,
                0.05f
            );
        }

        
    }

    public void SetHeldItemUI(InventorySlot heldItemSlot) {
        if (heldItemSlot.itemSO == null) {
            heldItemIcon.gameObject.SetActive(false);
            heldItemAmountText.gameObject.SetActive(false);
            return;
        }

        Vector3 targetPosition = Input.mousePosition + new Vector3(32f, -32f, 0f);
        heldItemObject.transform.position = targetPosition;

        heldItemAmountText.gameObject.SetActive(true);
        heldItemIcon.sprite = heldItemSlot.itemSO.icon;
        heldItemAmountText.text = "x" + heldItemSlot.amount.ToString();
        heldItemIcon.gameObject.SetActive(true);
    }

    public void SetPlateHeld(bool plateHeld) {
        blockOverlay.gameObject.SetActive(plateHeld);
    }

    public void InitializeInventoryUI(List<InventorySlot> inventorySlots) {

        inventorySlotsUI.Clear();

        foreach (InventorySlot slot in inventorySlots) {
            GameObject slotObject = Instantiate(inventorySlotPrefab, inventorySlotsParent);
            InventorySlotUI slotUI = slotObject.GetComponent<InventorySlotUI>();
            slotUI.SetInventorySlot(slot);
            inventorySlotsUI.Add(slotUI);
        }

    }
}
