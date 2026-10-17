using UnityEngine;
using Enemies;

namespace DamasDenis.Stage
{
    /// <summary>
    /// Root context component placed in a stage map scene (e.g. Stage 1.unity).
    /// Provides the stage definition, unit container transform, and enemy path for that specific map.
    /// </summary>
    public sealed class StageMapContext : MonoBehaviour
    {
        public static StageMapContext Current { get; private set; }

        [Header("Stage Identity")]
        [SerializeField] private StageDefinition stageDefinition;

        [Header("Stage Hierarchy References")]
        [Tooltip("Parent transform where units deployed during this stage are instantiated")]
        [SerializeField] private Transform unitContainer;

        [Tooltip("The enemy navigation route configured for this stage map")]
        [SerializeField] private EnemyPath enemyPath;

        public StageDefinition StageDefinition => stageDefinition;
        public Transform UnitContainer => unitContainer;
        public EnemyPath EnemyPath => enemyPath;

        private void Awake()
        {
            Current = this;
        }

        private void OnDestroy()
        {
            if (Current == this)
                Current = null;
        }

        /// <summary>
        /// Ensures a valid unit container transform exists in the stage scene.
        /// </summary>
        public Transform GetOrCreateUnitContainer()
        {
            if (unitContainer != null)
                return unitContainer;

            // Search for an existing container in the scene
            Transform found = transform.Find("Unit Container");
            if (found == null)
            {
                GameObject newContainer = new GameObject("Unit Container");
                newContainer.transform.SetParent(transform, false);
                found = newContainer.transform;
            }

            unitContainer = found;
            return unitContainer;
        }
    }
}
