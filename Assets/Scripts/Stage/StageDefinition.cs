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
        [SerializeField] private string stageName;
        [SerializeField] private string stageDesc;
        [SerializeField] private bool isOptionalStage;

        [Header("Battle tuning")]
        [Min(1f)] [SerializeField] private float survivalSeconds = 120f;
        [Min(1f)] [SerializeField] private float baseHealth = 100f;
        [Min(0)] [SerializeField] private int startingResources = 20;
        [Min(0)] [SerializeField] private int completionResources = 20;

        [Header("Presentation")]
        [SerializeField] private DialogueData[] briefing;
        [SerializeField] private DialogueData[] relocationDialogue;

        [Header("Star objectives (star 1 = completing the stage)")]
        [SerializeField] private StageObjective starTwoObjective;
        [SerializeField] private StageObjective starThreeObjective;

        [Header("Optional-stage reward")]
        [SerializeField] private UnitDeploymentData unlockedStaff;

        public string StageId { get { return stageId; } }
        public string SceneName { get { return sceneName; } }
        public string StageName { get { return stageName; } }
        public string StageDesc { get { return stageDesc; } }
        public bool IsOptionalStage { get { return isOptionalStage; } }
        public float SurvivalSeconds { get { return survivalSeconds; } }
        public float BaseHealth { get { return baseHealth; } }
        public int StartingResources { get { return startingResources; } }
        public int CompletionResources { get { return completionResources; } }
        public int CompletionResearchCredits { get { return completionResources; } }
        public DialogueData[] Briefing { get { return briefing; } }
        public DialogueData[] RelocationDialogue { get { return relocationDialogue; } }
        public StageObjective StarTwoObjective { get { return starTwoObjective; } }
        public StageObjective StarThreeObjective { get { return starThreeObjective; } }
        public UnitDeploymentData UnlockedStaff { get { return unlockedStaff; } }
    }
}
