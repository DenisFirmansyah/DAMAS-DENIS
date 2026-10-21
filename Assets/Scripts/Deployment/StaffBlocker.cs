using System.Collections.Generic;
using DamasDenis.Enemies;
using UnityEngine;

namespace DamasDenis.Deployment
{
    /// <summary>
    /// Holds enemies in place while this unit stands on their route.
    /// Added automatically by DeployedUnit when the unit's BlockCount is above 0.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StaffBlocker : MonoBehaviour
    {
        [Tooltip("Read-only at runtime: how many enemies this unit can hold.")]
        [SerializeField] private int capacity;
        [Tooltip("Read-only at runtime: how many it is holding now.")]
        [SerializeField] private int blockedNow;

        private readonly List<EnemyInstance> blocked = new List<EnemyInstance>();
        private StaffVitality vitality;

        /// <summary>The first enemy currently held, or null.</summary>
        public EnemyInstance FirstBlocked
        {
            get
            {
                blocked.RemoveAll(e => e == null);
                blockedNow = blocked.Count;
                return blocked.Count > 0 ? blocked[0] : null;
            }
        }

        /// <summary>Sets how many enemies this unit can hold.</summary>
        public void Initialize(int blockCount)
        {
            capacity = Mathf.Max(0, blockCount);
        }

        /// <summary>Tries to hold an enemy. Returns false when all slots are used.</summary>
        public bool TryBlock(EnemyInstance enemy)
        {
            if (enemy == null) return false;
            if (blocked.Contains(enemy)) return true;

            blocked.RemoveAll(e => e == null);
            if (blocked.Count >= capacity) return false;

            blocked.Add(enemy);
            blockedNow = blocked.Count;
            return true;
        }

        /// <summary>Called by an enemy that died or left, to free its slot.</summary>
        public void Release(EnemyInstance enemy)
        {
            blocked.Remove(enemy);
            blockedNow = blocked.Count;
        }

        /// <summary>Enemies hit the unit that blocks them. Health lives in StaffVitality.</summary>
        public void TakeDamage(float amount)
        {
            if (vitality == null) vitality = GetComponent<StaffVitality>();
            if (vitality != null) vitality.TakeDamage(amount);
        }

        // Covers death and retreat: the unit is destroyed, so everything it held walks on.
        private void OnDestroy()
        {
            foreach (EnemyInstance enemy in blocked.ToArray())
            {
                if (enemy != null) enemy.ReleaseFromBlocker();
            }
            blocked.Clear();
        }
    }
}