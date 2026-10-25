using UnityEngine;
namespace DamasDenis.Stage
{
    [CreateAssetMenu(fileName = "Objective_ClearAll", menuName = "DAMAS DENIS/Objectives/Clear All Enemies")]
    public sealed class ClearAllEnemiesObjective : StageObjective
    {
        public override bool IsMet(StageRecord record)
        {
            bool cleared = false;
            foreach (StageEventRecord e in record.events)
            {
                if (e.eventType == StageEvents.EnemyLeaked) return false;
                if (e.eventType == StageEvents.AllEnemiesCleared) cleared = true;
            }
            return cleared;
        }
    }
}