using UnityEngine;

namespace DamasDenis.Stage
{
    /// <summary>A star objective. Judged once, from the finished StageRecord.</summary>
    public abstract class StageObjective : ScriptableObject
    {
        [SerializeField] private string title = "Objective";
        [TextArea] [SerializeField] private string description;
        public string Title => title;
        public string Description => description;

        /// <summary>True if the finished stage satisfied this objective.</summary>
        public abstract bool IsMet(StageRecord record);
    }
}