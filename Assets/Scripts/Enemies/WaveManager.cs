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
            public int count;
            public float spawnDelay;
            public bool waitForClear; 
        }

        public List<WaveSegment> waves; 
        public bool autoStart = true;
        private List<GameObject> activeEnemies = new List<GameObject>();
        private bool hasStarted;
        private bool stopped;
        public System.Action OnWavesCompleted;

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
                if (stopped) yield break;
                
                for (int i = 0; i < segment.count; i++) {
                    if (stopped) yield break;
                    Vector3 startPos = EnemyPath.Instance != null ? EnemyPath.Instance.SpawnPosition : Vector3.zero;
                    
                    GameObject enemy = Instantiate(segment.enemyPrefab, startPos, Quaternion.identity);
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
