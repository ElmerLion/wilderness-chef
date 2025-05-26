using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;

public class Player : MonoBehaviour {

    public static Player Instance { get; private set; }

    [Header("Player Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private LayerMask interactionLayerMask;
    [SerializeField] private LayerMask collisionsLayerMask;
    [SerializeField] private Vector2 capsuleSize = new Vector2(0.8f, 1.2f);

    [Header("Player Thoughts")]
    [SerializeField] private TextMeshPro thoughtText;

    [Header("Character Animation")]
    [SerializeField] private SpriteRenderer playerCharacter;
    [SerializeField] private Sprite characterForward;
    [SerializeField] private Sprite characterBack;
    [SerializeField] private Sprite characterLeft;
    [SerializeField] private Sprite characterRight;
    [SerializeField] private Transform plateHoldTransform;
    [SerializeField] private Vector3 plateHoldPosForward;
    [SerializeField] private Vector3 plateHoldPosLeft;
    [SerializeField] private Vector3 plateHoldPosRight;

    [Header("Camera Zoom")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float zoomStep = 1f;           
    [SerializeField] private int maxZoomOutSteps = 2;       
    [SerializeField] private int maxZoomInSteps = 2;
    [SerializeField] private float zoomLerpSpeed = 8f;
    private int zoomStepIndex = 0;                        
    private float initialCameraSize;
    private float targetCameraSize;

    private Animator animator;
    private IInteractable previousInteractable;
    private RaycastHit2D previousInteractHit;

    private float thoughtTimer;
    private Vector3 originalThoughtTextPos;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnInteractPerformed += HandleInteraction;
        GameInput.Instance.OnInteractAlternate += HandleAlternateInteraction;
        GameInput.Instance.OnZoomPerformed += HandleZoom;
        animator = GetComponent<Animator>();

        initialCameraSize = cinemachineCamera.Lens.OrthographicSize;
        targetCameraSize = initialCameraSize;
        originalThoughtTextPos = thoughtText.transform.localPosition;
        thoughtText.gameObject.SetActive(false);

        SaveManager.OnGameLoaded += SaveManager_OnGameLoaded;
        SaveManager.OnGameSaved += SaveManager_OnGameSaved;
    }

    private void HandleZoom(Vector2 delta) {
        float scrollY = delta.y;

        if (scrollY > 0f && zoomStepIndex > -maxZoomInSteps) {
            zoomStepIndex--;
        }
        else if (scrollY < 0f && zoomStepIndex < maxZoomOutSteps) {
            zoomStepIndex++;
        }

        targetCameraSize = initialCameraSize + zoomStepIndex * zoomStep;
    }

    private void SaveManager_OnGameSaved(string obj) {
        ES3.Save("PlayerPos", transform.position, obj);
    }

    private void SaveManager_OnGameLoaded(string obj) {
        transform.position = ES3.Load<Vector3>("PlayerPos", obj, transform.position);
    }

    private void Update() {
        HandleMovement();
        CheckForInteractable();

        if (thoughtTimer > 0f) {
            thoughtTimer -= Time.deltaTime;
            if (thoughtTimer <= 0f) {
                thoughtText.gameObject.SetActive(false);
            }
        }
    }

    private void LateUpdate() {
        float current = cinemachineCamera.Lens.OrthographicSize;
        float next = Mathf.Lerp(current, targetCameraSize, zoomLerpSpeed * Time.deltaTime);
        cinemachineCamera.Lens.OrthographicSize = next;
    }

    private void HandleMovement() {
        Vector2 inputVector = GameInput.Instance.GetMovementInputNormalized();
        Vector2 moveDir = inputVector.normalized;
        float moveDistance = Time.deltaTime * moveSpeed;

        RaycastHit2D blockHit = Physics2D.CapsuleCast(
        transform.position,
        capsuleSize,
        CapsuleDirection2D.Vertical,
        0f,
        moveDir,
        moveDistance,
        collisionsLayerMask
    );

        if (blockHit.collider != null) {
            if (blockHit.collider.TryGetComponent<UnlockableArea>(out var area)) {
                int neededMeals = area.rank.minMealsDelivered - RankManager.Instance.GetDeliveredMeals();
                MessageUI.Instance.ShowMessage(
                    $"You need to be a {area.rank.name} to enter this area! " +
                    $"Deliver {neededMeals} more meals to rank up.",
                    2f
                );
                return;
            }


            Vector2 dirX = new Vector2(moveDir.x, 0).normalized;
            RaycastHit2D hitX = Physics2D.CapsuleCast(
                transform.position,
                capsuleSize,
                CapsuleDirection2D.Vertical,
                0f,
                dirX,
                moveDistance,
                collisionsLayerMask
            );
            if (Mathf.Abs(moveDir.x) > 0.5f && hitX.collider == null) {
                moveDir = dirX;
            } else {
                // Try Y only
                Vector2 dirY = new Vector2(0, moveDir.y).normalized;
                RaycastHit2D hitY = Physics2D.CapsuleCast(
                    transform.position,
                    capsuleSize,
                    CapsuleDirection2D.Vertical,
                    0f,
                    dirY,
                    moveDistance,
                    collisionsLayerMask
                );
                if (Mathf.Abs(moveDir.y) > 0.5f && hitY.collider == null) {
                    moveDir = dirY;
                } else {
                    // Blocked in both axes
                    animator.SetBool("IsWalking", false);
                    return;
                }
            }
        }


        transform.position += (Vector3)(moveDir * moveDistance);
        animator.SetBool("IsWalking", moveDir != Vector2.zero);

        if (moveDir != Vector2.zero) {
            // horizontal vs vertical dominance
            Plate heldPlate = null;
            if (plateHoldTransform.childCount > 0) {
                heldPlate = plateHoldTransform.GetChild(0).GetComponent<Plate>();
            }
     
            if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y)) {
                // left or right
                if (heldPlate != null) {
                    heldPlate.SetVisiblityIngredients(true);
                }
                if (moveDir.x > 0f) {
                    playerCharacter.sprite = characterRight;
                    plateHoldTransform.localPosition = plateHoldPosRight;
                } else {
                    playerCharacter.sprite = characterLeft;
                    plateHoldTransform.localPosition = plateHoldPosLeft;
                }
            } else {
                // up or down
                if (moveDir.y > 0f) {
                    playerCharacter.sprite = characterBack;
                    plateHoldTransform.localPosition = plateHoldPosForward;
                    if (heldPlate != null) {
                        heldPlate.SetVisiblityIngredients(false);
                    }
                } else {
                    playerCharacter.sprite = characterForward;
                    plateHoldTransform.localPosition = plateHoldPosForward;
                    if (heldPlate != null) {
                        heldPlate.SetVisiblityIngredients(true);
                    }
                }
            }
        }
    }

    private void SetVisibility(List<Transform> transforms, bool isVisible) {
        foreach (Transform t in transforms) {
            t.gameObject.SetActive(isVisible);
        }
    }

    private void CheckForInteractable() {
        // don’t show prompts if a UI is open
        if (UIManager.Instance.IsAnyUIOpen()) {
            // if we had something highlighted, clear it
            if (previousInteractable != null) {
                previousInteractable.InteractPrompt?.SetActive(false);
                previousInteractable = null;
            }
            return;
        }

        // 1) Raycast to see what’s under the cursor/direction
        Vector2 direction = GetFacingDirection();
        Vector2 origin = (Vector2)transform.position + direction * 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, 1.3f, interactionLayerMask);

        // 2) Figure out the new interactable (if any)
        IInteractable current = null;
        if (hit.collider != null) {
            current = hit.collider.GetComponent<IInteractable>();
        }

        // 3) If we switched targets, hide the old prompt
        if (previousInteractable != null && previousInteractable != current) {
            previousInteractable.InteractPrompt?.SetActive(false);
        }

        // 4) Show the new prompt (if any)
        if (current != null) {
            current.InteractPrompt?.SetActive(true);
        }

        // 5) Remember for next frame
        previousInteractable = current;
    }

    private void HandleInteraction() {
        if (UIManager.Instance.IsAnyUIOpen()) {
            UIManager.Instance.CloseOpenUI();
            return;
        }

        Vector2 direction = GetFacingDirection();
        Vector2 origin = (Vector2)transform.position + direction * 0.1f;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, 1.3f, interactionLayerMask);

        Debug.DrawRay(origin, direction * 1f, Color.red, 1.3f);

        if (hit.collider != null) {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null) {
                interactable.Interact();
            }
        }
    }

    private void HandleAlternateInteraction() {
        if (UIManager.Instance.IsAnyUIOpen()) {
            UIManager.Instance.CloseOpenUI();
            return;
        }

        Vector2 direction = GetFacingDirection();
        Vector2 origin = (Vector2)transform.position + direction * 0.1f;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, 1.3f, interactionLayerMask);

        Debug.DrawRay(origin, direction * 1f, Color.red, 1.3f);

        if (hit.collider != null) {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null) {
                interactable.InteractAlternate();
            }
        }
    }

    public Vector2 GetFacingDirection() {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f;

        Vector2 direction = (mouseWorldPosition - transform.position).normalized;

        return direction;
    }

    public void SetPlayerThought(string thought, float duration) {
        InventorySlot plateSlot = InventoryManager.Instance.GetHeldPlate();
        if (plateSlot != null) {
            Plate plate = plateSlot.worldItem.GetComponent<Plate>();
            if (plate.GetIngredients().Count > 0) {
                thoughtText.transform.localPosition = originalThoughtTextPos + new Vector3(0, 0.5f);
            } else {
                thoughtText.transform.localPosition = originalThoughtTextPos;
            }
        } else {
            thoughtText.transform.localPosition = originalThoughtTextPos;
        }

        thoughtText.text = thought;
        thoughtText.gameObject.SetActive(true);
        thoughtTimer = duration;
    }

    public void CheckPlateVisibility() {
        if (playerCharacter.sprite == characterBack) {
            if (plateHoldTransform.childCount > 0) {
                Plate heldPlate = plateHoldTransform.GetChild(0).GetComponent<Plate>();
                heldPlate.SetVisiblityIngredients(false);
            }
        } else {
            if (plateHoldTransform.childCount > 0) {
                Plate heldPlate = plateHoldTransform.GetChild(0).GetComponent<Plate>();
                heldPlate.SetVisiblityIngredients(true);
            }
        }
    }

    private void OnDestroy() {
        GameInput.Instance.OnInteractPerformed -= HandleInteraction;
        GameInput.Instance.OnInteractAlternate -= HandleAlternateInteraction;

        SaveManager.OnGameLoaded -= SaveManager_OnGameLoaded;
        SaveManager.OnGameSaved -= SaveManager_OnGameSaved;
    }
}



