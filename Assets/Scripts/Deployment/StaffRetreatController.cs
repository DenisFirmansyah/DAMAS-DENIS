using System;
using System.Collections;
using DamasDenis.Stage;
using UnityEngine;

namespace DamasDenis.Deployment
{
    /// <summary>
    /// Carries out one unit's medevac: a timed extraction, then refund, redeploy cooldown and
    /// removal. Started only by Medevac, which owns charges and capacity.
    /// </summary>
    public sealed class StaffRetreatController : MonoBehaviour
    {
        [Header("Processing")]
        [Min(0f)] [SerializeField] private float baseProcessingSeconds = 1f;
        [Min(0f)] [SerializeField] private float secondsPerDeploymentCost = 0.1f;
        [Range(0f, 1f)] [SerializeField] private float resourceRefundFraction = 0.5f;
        [Min(0f)] [SerializeField] private float cooldownSeconds = 8f;

        private DeployedUnit deployedUnit;
        private Action onEnded;

        /// <summary>True while the extraction countdown runs.</summary>
        public bool IsProcessing { get; private set; }
        /// <summary>0 to 1 progress of the extraction, for a UI bar.</summary>
        public float Progress01 { get; private set; }

        internal bool IsDeployed
        {
            get
            {
                // DeployedUnit may be added after Instantiate, so resolve it lazily.
                if (deployedUnit == null) deployedUnit = GetComponent<DeployedUnit>();
                return deployedUnit != null && deployedUnit.Data != null && deployedUnit.Cell != null;
            }
        }

        /// <summary>Starts the extraction. Only Medevac calls this, after it has spent a charge.</summary>
        internal void Begin(Action ended)
        {
            onEnded = ended;
            IsProcessing = true;
            StageEventLog.Record("medevac_started", deployedUnit.Data.DisplayName);
            StartCoroutine(Process());
        }

        private IEnumerator Process()
        {
            UnitDeploymentData data = deployedUnit.Data;
            float duration = baseProcessingSeconds + data.DeploymentCost * secondsPerDeploymentCost;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                Progress01 = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            Progress01 = 1f;

            StageState stageState = StageState.Instance;
            if (stageState != null)
                stageState.AddStageResources(Mathf.RoundToInt(data.DeploymentCost * resourceRefundFraction));

            StaffCooldowns.StartCooldown(data.UnlockId, cooldownSeconds);
            StageEventLog.Record("staff_retreated", data.DisplayName);
            Destroy(gameObject);
        }

        // Runs on completion AND when the unit dies mid-process (a coroutine stops silently
        // with its object), so the medevac slot is always freed.
        private void OnDestroy() => onEnded?.Invoke();
    }
}