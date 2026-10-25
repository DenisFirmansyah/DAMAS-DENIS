using DamasDenis.Stage;
using System;
using UnityEngine;

namespace DamasDenis.Deployment
{
    /// <summary>The single source of truth for a staff member's health, damage and death.</summary>
    public sealed class StaffVitality : MonoBehaviour
    {
        [Min(1f)] [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float HealthFraction => maxHealth <= 0f ? 1f : currentHealth / maxHealth;
        public bool IsDead => currentHealth <= 0f;

        /// <summary>Raised once when health reaches zero, just before the unit is destroyed.</summary>
        public event Action<StaffVitality> Died;

        /// <summary>Sets max health from unit data and refills health.</summary>
        public void Initialize(float max)
        {
            maxHealth = Mathf.Max(1f, max);
            currentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f) return;
            currentHealth = Mathf.Max(0f, currentHealth - amount);
            if (IsDead)
            {
                DeployedUnit du = GetComponent<DeployedUnit>();
                string who = du != null && du.Data != null ? du.Data.DisplayName : gameObject.name;
                StageEventLog.Record(StageEvents.StaffDied, who);
                Died?.Invoke(this);
                Destroy(gameObject);
            }
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }
    }
}