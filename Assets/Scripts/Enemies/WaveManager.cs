using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DamasDenis.Enemies
{
    public class WaveManager : MonoBehaviour {
        
        [System.Serializable]
        public class WaveSegment {
            [Tooltip("Must be the prefab containing the EnemyInstance script")]
            public GameObject enemyPrefab;
            [Tooltip("The route (and spawn point) this group uses.")]
            public EnemyPath enemyPath;
            public int count;
            public float spawnDelay;
            public bool waitForClear; 
        }

        [SerializeField] private Transform enemyContainer;

        public List<WaveSegment> waves; 
        public bool autoStart = false;
        private List<GameObject> activeEnemies = new List<GameObject>();
        private bool hasStarted;
        private bool stopped;
        public System.Action OnWavesCompleted;

        public Transform EnemyContainer
        {
            get => enemyContainer;
            set => enemyContainer = value;
        }

        public void SetEnemyContainer(Transform container)
        {
            enemyContainer = container;
        }

        void Start() {
            if (autoStart) BeginWaves();
        }

        public void BeginWaves() {
            if (hasStarted) return;
            hasStarted = true;
            StartCoroutine(SpawnWaveRoutine());
        }

        public void StopWaves() {
            stopped = true;
            StopAllCoroutines();
        }

        IEnumerator SpawnWaveRoutine() {
            foreach (WaveSegment segment in waves) {
                if (segment.enemyPath == null)
                {
                    Debug.LogError("WaveManager: a wave segment has no Enemy Path and was skipped.", this);
                    continue;
                }
                if (stopped) yield break;
                
                for (int i = 0; i < segment.count; i++) {
                    if (stopped) yield break;
                    
                    GameObject enemy = Instantiate(segment.enemyPrefab, segment.enemyPath.SpawnPosition, Quaternion.identity, enemyContainer);
                    EnemyInstance instance = enemy.GetComponent<EnemyInstance>();
                    if (instance != null) instance.Initialize(segment.enemyPath);
                    activeEnemies.Add(enemy);
                    
                    yield return new WaitForSeconds(segment.spawnDelay);
                }

                if (segment.waitForClear) {
                    Debug.Log("Wave Paused: Waiting for enemies to clear...");
                    
                    while (activeEnemies.Count > 0) {
                        activeEnemies.RemoveAll(item => item == null);

                        if (activeEnemies.Count > 0) {
                            yield return new WaitForSeconds(0.5f);
                        }
                    }
                    
                    Debug.Log("Wave Cleared! Moving to next segment.");
                }
            }
            
            Debug.Log("All Waves Complete!");
            OnWavesCompleted?.Invoke();
        }
    }
}
