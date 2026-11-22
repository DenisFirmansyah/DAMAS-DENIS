using DamasDenis.Enemies;
using UnityEngine;

namespace DamasDenis.Deployment
{
    internal static class HomingProjectileMotion
    {
        public static bool MoveAndCheckHit(Transform projectile, Transform target, float speed, float aimHeight, float hitDistance)
        {
            Vector3 aim = target.position + Vector3.up * aimHeight;
            projectile.position = Vector3.MoveTowards(projectile.position, aim, speed * Time.deltaTime);
            return (aim - projectile.position).sqrMagnitude <= hitDistance * hitDistance;
        }
    }

    /// <summary>Homing projectile. Damages its target when it gets close enough; no physics needed.</summary>
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 5f;
        [Tooltip("The projectile hits when it is this close to its target (world units).")]
        [Min(0.01f)] [SerializeField] private float hitDistance = 0.15f;
        [SerializeField] private float aimHeight = 0.5f;

        private EnemyInstance target;
        private float speed;
        private float damage;

        /// <summary>Launches the projectile at an enemy.</summary>
        public void Setup(Transform targetEnemy, float projectileSpeed, float projectileDamage)
        {
            target = targetEnemy != null ? targetEnemy.GetComponentInParent<EnemyInstance>() : null;
            speed = projectileSpeed;
            damage = projectileDamage;
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (target == null || target.IsDead) { Destroy(gameObject); return; }

            if (HomingProjectileMotion.MoveAndCheckHit(transform, target.transform, speed, aimHeight, hitDistance))
            {
                target.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
