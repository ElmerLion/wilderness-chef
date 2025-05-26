using System;
using UnityEngine;

public class Animal : MonoBehaviour, IInteractable {

    public event Action<Animal> OnAnimalDied;

    [SerializeField] private AnimalSO animalSO;
    [SerializeField] private float moveSpeed = 2f; // Movement speed for non-idle state
    [SerializeField] private float changeDirectionTimeMin = 1f; // Min time for moving
    [SerializeField] private float changeDirectionTimeMax = 3f; // Max time for moving

    [SerializeField] private float idleTimeMin = 1f; // Min time to idle
    [SerializeField] private float idleTimeMax = 2f; // Max time to idle
    [SerializeField][Range(0f, 1f)] private float idleChance = 0.3f; // Chance to idle rather than move
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private GameObject _interactPrompt;

    [Header("Obstacle Avoidance (CapsuleCast)")]
    [SerializeField]
    private LayerMask obstacleMask;
    [SerializeField, Tooltip("How far ahead to check for obstacles")]
    private float obstacleCheckDistance = 0.2f;
    [SerializeField, Tooltip("Size of the capsule used for obstacle detection")]
    private Vector2 obstacleCapsuleSize = new Vector2(0.5f, 1f);
    [SerializeField, Tooltip("Orientation of the capsule")]
    private CapsuleDirection2D obstacleCapsuleDirection = CapsuleDirection2D.Vertical;

    private AnimalSpawner parentSpawner;
    private Animator animator;
    private int currentHealth;
    private float stateTimer;
    private Vector3 moveDirection;
    private float passiveDropTimer;
    private float alternateTimer;
    private float timeSinceLastIdleSound = 0f;

    private AudioSource prevSrc;

    public GameObject InteractPrompt { get => _interactPrompt; set => _interactPrompt = value; }

    public void Initialize(AnimalSpawner animalSpawner) {
        parentSpawner = animalSpawner;
    }

    private void Start() {
        animator = GetComponent<Animator>();

        currentHealth = animalSO.maxHealth;
        passiveDropTimer = UnityEngine.Random.Range(5, 30);
        ChooseNewState();

        if (AnimalManager.Instance == null) return;

        AnimalManager.Instance.AddAnimal(this);
    }

    private void Update() {
        timeSinceLastIdleSound += Time.deltaTime;

        HandleMovement();
        UpdateSpriteDirection();
        HandlePassiveDrop();
    }

    private void HandleAlternateDrop() {
        if (animalSO.alternateItemSODrop == null || alternateTimer < 0) {
            return;
        }
        alternateTimer -= Time.deltaTime;
    }

    private void HandlePassiveDrop() {
        if (animalSO.passiveItemDropSO == null || InventoryManager.Instance == null) {
            return;
        }
       passiveDropTimer -= Time.deltaTime;

        if (passiveDropTimer <= 0f) {
            passiveDropTimer = UnityEngine.Random.Range(animalSO.timerMinBetweenDrops, animalSO.timerMaxBetweenDrops);
            if (UnityEngine.Random.value < animalSO.dropChance) {
                int amount = UnityEngine.Random.Range(animalSO.minDropAmount, animalSO.maxDropAmount);
                InventoryManager.Instance.DropItem(animalSO.passiveItemDropSO, transform.position, amount);
            }
        }
    }

    private void HandleMovement() {
        float deltaTime = Time.deltaTime;
        stateTimer -= deltaTime;

        if (stateTimer <= 0f) {
            ChooseNewState();
        }

        if (moveDirection != Vector3.zero) {
            RaycastHit2D hit = Physics2D.CapsuleCast(
                transform.position,                
                obstacleCapsuleSize,               
                obstacleCapsuleDirection,          
                0f,                                
                moveDirection,                     
                obstacleCheckDistance,             
                obstacleMask                       
            );

            if (hit.collider != null) {
                ChooseNewState(skipIdle: true);
                return;
            }

            transform.position += moveDirection * moveSpeed * deltaTime;
        }
    }

    private void UpdateSpriteDirection() {
        // Only update horizontal look direction if we're moving horizontally.
        if (moveDirection.x > 0.01f) {
            spriteRenderer.flipX = true;
        } else if (moveDirection.x < -0.01f) {
            spriteRenderer.flipX = false;
        }
    }

    private void ChooseNewState(bool skipIdle = false) {
        if (UnityEngine.Random.value < 0.05f && timeSinceLastIdleSound >= 30f && AudioManager.Instance != null) {
            AudioManager.Instance.PlaySound(animalSO.animalIdle, transform.position, 0.2f, true);
            timeSinceLastIdleSound = 0f;
        }
        // Randomly decide whether to idle or move
        if (!skipIdle && UnityEngine.Random.value < idleChance) {
            // Idle state: stop moving.
            moveDirection = Vector3.zero;
            stateTimer = UnityEngine.Random.Range(idleTimeMin, idleTimeMax);
            animator.SetBool("IsWalking", false); // Set idle animation
        } else {
            // Moving state: choose a random direction on the X and Y axis.
            animator.SetBool("IsWalking", true); // Set walking animation
            float randomX = UnityEngine.Random.Range(-1f, 1f);
            float randomY = UnityEngine.Random.Range(-1f, 1f);
            moveDirection = new Vector3(randomX, randomY, 0f).normalized;
            stateTimer = UnityEngine.Random.Range(changeDirectionTimeMin, changeDirectionTimeMax);
        }
    }

    public void Interact() {
        currentHealth--;
        animator.SetTrigger("Damage");

        if (prevSrc != null) {
            prevSrc.Stop();
            prevSrc = AudioManager.Instance.PlaySound(animalSO.animalDamaged, transform.position, 0.2f, true);
        } else {
            prevSrc = AudioManager.Instance.PlaySound(animalSO.animalDamaged, transform.position, 0.2f, true);
        }
        
        ParticleEffectsManager.Instance.Play(EffectType.HitSmoke, transform.position + new Vector3(0, 0.1f, 0));
        ChooseNewState(true);

        if (currentHealth <= 0) {
            int amount = UnityEngine.Random.Range(animalSO.minDropAmount, animalSO.maxDropAmount);
            InventoryManager.Instance.DropItem(animalSO.droppedItemSO, transform.position, amount);

            OnAnimalDied?.Invoke(this);

            ParticleEffectsManager.Instance.Play(EffectType.AnimalDeath, transform.position + new Vector3(0, 0.1f, 0));

            Destroy(gameObject);
        }
    }

    public void InteractAlternate() {
        if (animalSO.alternateItemSODrop == null || alternateTimer >= animalSO.alternateCooldown) return;
        alternateTimer = animalSO.alternateCooldown;

        int amount = animalSO.alternateAmountToDrop;
        InventoryManager.Instance.DropItem(animalSO.alternateItemSODrop, transform.position, amount);
    }

    public AnimalSaveData GetSaveData() {
        return new AnimalSaveData {
            animalName = animalSO.nameString,
            spawnerId = parentSpawner.SpawnerID,
            position = transform.position,
            passiveDropTimer = passiveDropTimer,
            alternateTimer = alternateTimer,
            currentHealth = currentHealth
        };
    }

    public void RestoreFromData(AnimalSaveData data) {
        transform.position = data.position;
        passiveDropTimer = data.passiveDropTimer;
        alternateTimer = data.alternateTimer;
        currentHealth = data.currentHealth;


        AnimalSpawner spawner = AnimalManager.Instance.GetSpawnerById(data.spawnerId);
        if (spawner != null) {
            parentSpawner = spawner;
            parentSpawner.AddAnimal(this);
        }

        if (currentHealth <= 0) {
            Destroy(gameObject);
        }
    }
}

public class AnimalSaveData {
    public string animalName;
    public string spawnerId;
    public Vector3 position;
    public float passiveDropTimer;
    public float alternateTimer;
    public int currentHealth;
}
