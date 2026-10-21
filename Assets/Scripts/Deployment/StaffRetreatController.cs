using System.Collections;
using DamasDenis.Stage;
using UnityEngine;

namespace DamasDenis.Deployment
{
    public enum MedicRequirement
    {
        Never,
        Always,
        OnlyBelowHealthThreshold
    }

    public sealed class StaffRetreatController : MonoBehaviour
    {
        [Header("Processing")]
        [Min(0f)] [SerializeField] private float baseProcessingSeconds = 1f;
        [Min(0f)] [SerializeField] private float secondsPerDeploymentCost = 0.1f;
        [Range(0f, 1f)] [SerializeField] private float resourceRefundFraction = 0.5f;
        [Min(0f)] [SerializeField] private float cooldownSeconds = 8f;

        [Header("Medic requirement")]
        [SerializeField] private MedicRequirement medicRequirement = MedicRequirement.OnlyBelowHealthThreshold;
        [Range(0f, 1f)] [SerializeField] private float healthThreshold = 0.1f;
        [Min(0)] [SerializeField] private int medicRangeTiles = 2;

        private DeployedUnit deployedUnit;
        private StaffVitality vitality;
        private StaffCombat combat;
        private bool processing;

        private void Awake()
        {
            vitality = GetComponent<StaffVitality>();
            combat = GetComponent<StaffCombat>();
        }

        public bool TryBeginRetreat()
        {
            // DeployedUnit may be added after Instantiate (see DeploymentController.ConfirmPlacement),
            // so resolve it here instead of in Awake.
            if (deployedUnit == null) deployedUnit = GetComponent<DeployedUnit>();
            if (processing || deployedUnit == null || deployedUnit.Data == null || !MedicRequirementMet())
                return false;

            StartCoroutine(ProcessRetreat());
            return true;
        }

        private bool MedicRequirementMet()
        {
            bool needsMedic = medicRequirement == MedicRequirement.Always ||
                              (medicRequirement == MedicRequirement.OnlyBelowHealthThreshold && HealthFraction <= healthThreshold);
            if (!needsMedic) return true;

            foreach (DeployedUnit candidate in FindObjectsByType<DeployedUnit>(FindObjectsSortMode.None))
            {
                Vector2Int a = deployedUnit.Cell.Coordinates, b = candidate.Cell.Coordinates;
                int tiles = Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
                if (tiles <= medicRangeTiles) return true;
            }
            return false;
        }

        private float HealthFraction
        {
            get
            {
                if (vitality != null) return vitality.HealthFraction;
                return combat == null ? 1f : combat.HealthFraction;
            }
        }

        private IEnumerator ProcessRetreat()
        {
            processing = true;
            float duration = baseProcessingSeconds + deployedUnit.Data.DeploymentCost * secondsPerDeploymentCost;
            yield return new WaitForSeconds(duration);

            StageState stageState = StageState.Instance;
            if (stageState != null)
                stageState.AddStageResources(Mathf.RoundToInt(deployedUnit.Data.DeploymentCost * resourceRefundFraction));

            StaffCooldowns.StartCooldown(deployedUnit.Data.UnlockId, cooldownSeconds);
            StageEventLog.Record("staff_retreated", deployedUnit.Data.DisplayName);
            Destroy(gameObject);
        }
    }
}
