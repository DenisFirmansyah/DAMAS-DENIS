using System.Collections;
using UnityEngine;
using Core.PlayerState;
using Core.Waypoint;
using DamasDenis.Stage;

namespace Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyInstance : MonoBehaviour {
        [Header("Data Source")]
        public EnemyData enemyData;

        [Header("References")]
        [SerializeField] private Rigidbody2D rb;
        private Animator anim;
        private SpriteRenderer sr;

        [Header("Runtime Attributes")]
        [SerializeField] private float currentHealth;
        private Waypoint target;
        private bool isMoving = true;
        private bool canMove = false;
        void Start() {
            currentHealth = enemyData.maxHealth;

            if (enemyData.enemyPrefab != null) {
                GameObject visualObj = Instantiate(enemyData.enemyPrefab, transform);
                visualObj.transform.localPosition = new Vector3(0, -0.5f, 0);
                
                anim = visualObj.GetComponentInChildren<Animator>();
                sr = visualObj.GetComponentInChildren<SpriteRenderer>();
            }

            if (LevelManager.main != null) {
                target = LevelManager.main.startPoint.GetComponent<Waypoint>();
            }

            StartCoroutine(StartMovementAfterDelay(1));
        }

        private void FixedUpdate() {
            if (target == null || !canMove) return;

            Move();
        }

        IEnumerator StartMovementAfterDelay(float delay) {
            yield return new WaitForSeconds(delay);
            canMove = true;
        }

        void Move() {
            float step = enemyData.speed * Time.fixedDeltaTime;
            float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);

            if (distanceToTarget <= step) {
                rb.MovePosition(target.transform.position);
                
                target = target.GetNextStep();

                if (target == null) {
                    ReachEndOfPath(); 
                }
                
            } else {
                Vector2 direction = (target.transform.position - transform.position).normalized;
                rb.MovePosition(rb.position + direction * step);

                if (anim != null) {
                    anim.SetFloat("Horizontal", direction.x);
                    anim.SetFloat("Vertical", direction.y);
                    anim.SetFloat("Speed", 1f);
                }

                if (sr != null) {
                    if (direction.x < -0.1f) sr.flipX = true;
                    else if (direction.x > 0.1f) sr.flipX = false;
                }
            }
        }

        void ReachEndOfPath() {
            StageState stageState = StageState.Instance ?? FindObjectOfType<StageState>();
            stageState?.TakeDamage(enemyData.damage);
            Destroy(gameObject);
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Tower") || collision.gameObject.CompareTag("Unit") || collision.gameObject.CompareTag("Staff"))
            {
                isMoving = false;
                
                // 2. Start attacking
                // StartAttack(collision.gameObject);
            }
        }

        public void TakeDamage(float amount) {
            currentHealth -= amount;
            if (currentHealth <= 0) {
                Die();
            }
        }

        void Die() {
            StageState stageState = StageState.Instance ?? FindObjectOfType<StageState>();
            stageState?.AddStageResources(enemyData.reward);
            
            Destroy(gameObject);
        }
    }
}
