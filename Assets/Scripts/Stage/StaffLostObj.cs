using UnityEngine;
namespace DamasDenis.Stage
{
    [CreateAssetMenu(fileName = "Objective_StaffLost", menuName = "DAMAS DENIS/Objectives/Staff Lost")]
    public sealed class StaffLostObjective : StageObjective
    {
        [SerializeField, Min(0)] private int maxLost = 0;

        public override bool IsMet(StageRecord record)
        {
            int lost = 0;
            foreach (StageEventRecord e in record.events)
                if (e.eventType == StageEvents.StaffDied) lost++;
            return lost <= maxLost;
        }
    }
}