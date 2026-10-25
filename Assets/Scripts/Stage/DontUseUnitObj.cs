// DontUseUnitObjective.cs  (star 3 example)
using DamasDenis.Deployment;
using UnityEngine;
namespace DamasDenis.Stage
{
    [CreateAssetMenu(fileName = "Objective_DontUseUnit", menuName = "DAMAS DENIS/Objectives/Don't Use Unit")]
    public sealed class DontUseUnitObjective : StageObjective
    {
        [SerializeField] private UnitDeploymentData forbiddenUnit;

        public override bool IsMet(StageRecord record)
        {
            if (forbiddenUnit == null) return true;
            foreach (StageEventRecord e in record.events)
                if (e.eventType == StageEvents.StaffDeployed && e.detail == forbiddenUnit.DisplayName)
                    return false;
            return true;
        }
    }
}