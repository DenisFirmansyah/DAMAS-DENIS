using DamasDenis.Deployment;
using UnityEngine;

namespace DamasDenis.Enemies
{
    [DisallowMultipleComponent]
    public sealed class EnemyAttackProjectile : MonoBehaviour
    {
        [Min(0.01f)] [SerializeField] private float hitDistance = 0.15f;
        [SerializeField] private float aimHeight = 0.5f;
        [Min(0.1f)] [SerializeField] private float lifetime = 5f;

        private Transform target;
        private float speed;
        private float damage;

        public void Setup(Transform targetTransform, float projectileSpeed, float projectileDamage)
        {
            if (targetTransform == null)
            {
                Debug.LogError($"{name}: cannot launch an enemy projectile without a target.", this);
                Destroy(gameObject);
                return;
            }

            target = targetTransform;
            speed = Mathf.Max(0.01f, projectileSpeed);
            damage = Mathf.Max(0f, projectileDamage);
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 aimPoint = target.position + Vector3.up * aimHeight;
            transform.position = Vector3.MoveTowards(
                transform.position,
                aimPoint,
                speed * Time.deltaTime);

            if ((aimPoint - transform.position).sqrMagnitude <= hitDistance * hitDistance)
            {
                ApplyDamage();
                Destroy(gameObject);
            }
        }

        private void ApplyDamage()
        {
            StaffBlocker blocker = target.GetComponentInParent<StaffBlocker>();
            if (blocker != null)
            {
                blocker.TakeDamage(damage);
                return;
            }

            StaffVitality vitality = target.GetComponentInParent<StaffVitality>();
            if (vitality != null)
            {
                vitality.TakeDamage(damage);
                return;
            }

            Debug.LogError($"{name}: projectile target '{target.name}' has no damageable component.", this);
        }
    }
}
