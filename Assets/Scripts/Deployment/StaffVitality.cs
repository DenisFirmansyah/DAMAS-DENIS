using UnityEngine;

namespace DamasDenis.Deployment
{
    public sealed class StaffVitality : MonoBehaviour
    {
        [Min(1f)] [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;

        public float HealthFraction { get { return maxHealth <= 0f ? 1f : currentHealth / maxHealth; } }

        public void TakeDamage(float amount)
        {
            currentHealth = Mathf.Max(0f, currentHealth - amount);
        }
    }
}
