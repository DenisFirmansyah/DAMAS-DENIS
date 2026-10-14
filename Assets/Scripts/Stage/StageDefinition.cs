using DamasDenis.Deployment;
using UnityEngine;

namespace DamasDenis.Stage
{
    [CreateAssetMenu(fileName = "StageDefinition", menuName = "DAMAS DENIS/Stage Definition")]
    public sealed class StageDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string stageId = "stage_01";
        [Tooltip("Scene name configured in Build Settings for this selectable stage.")]
        [SerializeField] private string sceneName;
        [SerializeField] private bool isOptionalStage;

        [Header("Battle tuning")]
        [Min(1f)] [SerializeField] private float survivalSeconds = 120f;
        [Min(1f)] [SerializeField] private float baseHealth = 100f;
        [Min(0)] [SerializeField] private int startingResources = 20;
        [Min(0)] [SerializeField] private int completionResources = 20;

        [Header("Presentation")]
        [SerializeField] private DialogueData[] briefing;
        [SerializeField] private DialogueData[] relocationDialogue;

        [Header("Optional-stage reward")]
        [SerializeField] private UnitDeploymentData unlockedStaff;

        public string StageId { get { return stageId; } }
        public string SceneName { get { return sceneName; } }
        public bool IsOptionalStage { get { return isOptionalStage; } }
        public float SurvivalSeconds { get { return survivalSeconds; } }
        public float BaseHealth { get { return baseHealth; } }
        public int StartingResources { get { return startingResources; } }
        /// <summary>Persistent Research Credits awarded upon completing the stage, spent on unit upgrades.</summary>
        public int CompletionResources { get { return completionResources; } }
        public int CompletionResearchCredits { get { return completionResources; } }
        public DialogueData[] Briefing { get { return briefing; } }
        public DialogueData[] RelocationDialogue { get { return relocationDialogue; } }
        public UnitDeploymentData UnlockedStaff { get { return unlockedStaff; } }
    }
}
