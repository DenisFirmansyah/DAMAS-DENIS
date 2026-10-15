using Enemies;
using UnityEngine;

namespace DamasDenis.Deployment
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 5f;

        private Transform target;
        private float speed;
        private float damage;

        public void Setup(Transform targetEnemy, float projectileSpeed, float projectileDamage)
        {
            target = targetEnemy;
            speed = projectileSpeed;
            damage = projectileDamage;
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            // Move towards the target's current position
            Vector2 direction = (target.position - transform.position).normalized;
            transform.Translate(direction * speed * Time.deltaTime, Space.World);

            // Rotate to face the target
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy"))
            {
                EnemyInstance enemy = collision.GetComponent<EnemyInstance>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
    }
}
