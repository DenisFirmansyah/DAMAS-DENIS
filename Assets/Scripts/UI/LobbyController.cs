using UnityEngine;
using UnityEngine.SceneManagement;
using DamasDenis.Stage;

namespace DamasDenis.UI
{
    /// <summary>Scene-local entry point for lobby navigation and stage selection.</summary>
    public sealed class LobbyController : UIScreen
    {
        [Header("Lobby Screens")]
        [SerializeField] private GameObject stageListScreen;
        [SerializeField] private GameObject unitListScreen;
        [SerializeField] private GameObject upgradeUnitScreen;
        [SerializeField] private GameObject journalScreen;

        public static LobbyController Instance { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void UnitList() => ShowOnly(unitListScreen);
        public void UpgradeUnit() => ShowOnly(upgradeUnitScreen);
        public void Journal() => ShowOnly(journalScreen);
        public void Back() => ShowOnly(stageListScreen);

        public void SelectStage(StageDefinition stage)
        {
            if (stage == null || string.IsNullOrWhiteSpace(stage.SceneName))
            {
                Debug.LogError("SelectStage requires a StageDefinition with a valid scene name.", this);
                return;
            }

            if (GameManager.Instance == null)
            {
                Debug.LogError("Cannot start gameplay because GameManager.Instance is missing.", this);
                return;
            }

            GameManager.Instance.StartGameplay(stage);
        }

        public void RoamingMode()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("Cannot start roaming because GameManager.Instance is missing.", this);
                return;
            }

            GameManager.Instance.StartRoaming();
        }

        private void ShowOnly(GameObject target)
        {
            if (target == null)
            {
                Debug.LogWarning("A LobbyController screen reference is not assigned.", this);
                return;
            }

            SetActiveIfAssigned(stageListScreen, target == stageListScreen);
            SetActiveIfAssigned(unitListScreen, target == unitListScreen);
            SetActiveIfAssigned(upgradeUnitScreen, target == upgradeUnitScreen);
            SetActiveIfAssigned(journalScreen, target == journalScreen);
        }

        private void SetActiveIfAssigned(GameObject screen, bool active)
        {
            if (screen != null)
                screen.SetActive(active);
        }
    }
}
