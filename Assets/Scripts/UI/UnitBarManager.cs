using UnityEngine;
using DamasDenis.Deployment;
using DamasDenis.Stage;
using DamasDenis.Core;
using TMPro;

namespace DamasDenis.UI
{
    /// <summary>
    /// Unit Bar Manager: Orchestrates the unit bar lifecycle.
    /// - Loads player staff set on scene start
    /// - Updates resource displays
    /// - Handles stage transitions
    /// </summary>
    public class UnitBarManager : MonoBehaviour
    {
        [Header("References")]
        public UnitBar unitBar;
        public StageState stageState;
        public PlayerState playerState;
        public StageDirector stageDirector;
        public StageDefinition CurrentStage => stageDirector != null ? stageDirector.CurrentStage : null;

        [Header("UI Text")]
        public TextMeshProUGUI researchCreditsText;
        public TextMeshProUGUI goldText;

        [Header("Events")]
        public System.Action onUnitBarReady;

        [SerializeField] private DeploymentController deploymentController;

        private void Awake()
        {
            // Auto-find references
            if (unitBar == null) unitBar = FindObjectOfType<UnitBar>();
            if (stageState == null) stageState = FindObjectOfType<StageState>();
            if (playerState == null) playerState = FindObjectOfType<PlayerState>();
        }

        private void Start() => Initialize();

        /// <summary>
        /// Initialize the unit bar. Call after the unit bar is set up in the scene.
        /// </summary>
        public void Initialize()
        {
            if (unitBar == null)
            {
                Debug.LogError("UnitBar not found. Assign it in Inspector.");
                return;
            }

            // Load player staff set
            unitBar.LoadPlayerStaffSet();
            unitBar.UnitChosen += OnUnitChosen;

            // Subscribe to resource changes
            if (playerState != null)
            {
                playerState.OnResearchCreditsChanged += UpdateResearchCredits;
            }

            // Subscribe to stage changes
            if (stageState != null)
            {
                stageState.OnStageResourceChanged += OnStageResourcesChanged;
                OnStageResourcesChanged(stageState.CurrentStageResources); // initial state
            }

            // Initial resource update
            unitBar.UpdateResources();

            onUnitBarReady?.Invoke();
            Debug.Log("Unit Bar Manager initialized.");
        }

        private void OnStageResourcesChanged(int gold)
        {
            UpdateGold(gold);
            unitBar.UpdateAffordability(gold);
        }

        private void OnUnitChosen(UnitDeploymentData unit)
        {
            if (deploymentController != null)
                deploymentController.BeginPlacement(unit);
        }

        private void UpdateResearchCredits(int credits)
        {
            if (researchCreditsText != null)
            {
                researchCreditsText.text = credits.ToString();
            }
        }

        private void UpdateGold(int gold)
        {
            if (goldText != null)
            {
                goldText.text = gold.ToString();
            }
        }

        /// <summary>
        /// Reload the unit bar when switching stages.
        /// </summary>
        public void ReloadUnitBar()
        {
            if (unitBar != null)
            {
                unitBar.LoadPlayerStaffSet();
                unitBar.UpdateResources();

                // Update stage state
                if (stageState != null && CurrentStage != null)
                {
                    var deployable = unitBar.GetDeployableUnits(CurrentStage);
                    Debug.Log($"Stage {CurrentStage.StageId}: {deployable.Count} deployable units");
                }
            }
        }

        private void OnDestroy()
        {
            if (unitBar != null) unitBar.UnitChosen -= OnUnitChosen;
            if (playerState != null) playerState.OnResearchCreditsChanged -= UpdateResearchCredits;
            if (stageState != null) stageState.OnStageResourceChanged -= OnStageResourcesChanged;
        }
    }
}
