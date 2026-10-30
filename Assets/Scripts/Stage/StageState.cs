using UnityEngine;
using DamasDenis.UI;

namespace DamasDenis.Stage
{
    /// <summary>
    /// Runtime stage state: owns the active stage's base health and in-stage placement resources.
    /// Resets each stage and does not persist across scenes.
    /// </summary>
    public class StageState : MonoBehaviour
    {
        public static StageState Instance { get; private set; }

        [Header("Stage Attributes (Health)")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("Stage Economy (Placement Cost Currency)")]
        [SerializeField] private int maxStageResources = 100;
        [SerializeField] private int currentStageResources;

        [Header("Passive Resource Regeneration")]
        [SerializeField] private bool enableRegen = true;
        [SerializeField] private float regenInterval = 1f;
        [SerializeField] private int regenAmount = 1;
        private float regenTimer;

        public event System.Action<float> OnHealthChanged;
        public event System.Action<int> OnStageResourceChanged;
        public event System.Action OnBaseDefeated;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public int MaxStageResources => maxStageResources;
        public int CurrentStageResources => currentStageResources;
        public bool EnableRegen
        {
            get => enableRegen;
            set => enableRegen = value;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            currentHealth = maxHealth;
            currentStageResources = Mathf.Clamp(currentStageResources, 0, maxStageResources);
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(currentHealth);
            OnStageResourceChanged?.Invoke(currentStageResources);
        }

        private void Update()
        {
            if (enableRegen)
                HandleRegeneration();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Initialize(float baseHealth, int startingResources, int maxResources = 100)
        {
            Time.timeScale = 1f;
            maxHealth = baseHealth;
            currentHealth = maxHealth;
            maxStageResources = Mathf.Max(maxResources, startingResources);
            currentStageResources = Mathf.Clamp(startingResources, 0, maxStageResources);
            regenTimer = 0f;

            OnHealthChanged?.Invoke(currentHealth);
            OnStageResourceChanged?.Invoke(currentStageResources);
        }

        private void HandleRegeneration()
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= regenInterval)
            {
                AddStageResources(regenAmount);
                regenTimer = 0f;
            }
        }

        public void AddStageResources(int amount)
        {
            if (currentStageResources >= maxStageResources)
                return;

            currentStageResources += amount;
            if (currentStageResources > maxStageResources)
                currentStageResources = maxStageResources;

            OnStageResourceChanged?.Invoke(currentStageResources);
        }

        public bool TrySpendStageResources(int amount)
        {
            if (currentStageResources >= amount)
            {
                currentStageResources -= amount;
                OnStageResourceChanged?.Invoke(currentStageResources);
                return true;
            }
            return false;
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            OnHealthChanged?.Invoke(currentHealth);

            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                OnBaseDefeated?.Invoke();
            }
        }
    }
}
