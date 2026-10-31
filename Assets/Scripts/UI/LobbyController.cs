using UnityEngine;
using UnityEngine.SceneManagement;
using DamasDenis.Stage;
using DamasDenis.Core;
using TMPro;

namespace DamasDenis.UI
{
    /// <summary>Scene-local entry point for lobby navigation and stage selection.</summary>
    public sealed class LobbyController : UIScreen
    {
        [Header("Lobby Screens")]
        [SerializeField] private GameObject lobbyScreen;
        [SerializeField] private GameObject stageListScreen;
        [SerializeField] private GameObject unitListScreen;
        [SerializeField] private GameObject upgradeUnitScreen;
        [SerializeField] private GameObject journalScreen;
        [SerializeField] private GameObject optionScreen;
        [SerializeField] private GameObject exitScreen;
        [SerializeField] private TextMeshProUGUI researchCreditsText;

        private PlayerState subscribedPlayerState;

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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleBackInput();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void StageList() => ShowOnly(stageListScreen);
        public void UnitList() => ShowOnly(unitListScreen);
        public void UpgradeUnit() => ShowOnly(upgradeUnitScreen);
        public void Journal() => ShowOnly(journalScreen);
        public void Options() => ShowOnly(optionScreen);
        public void Back() => ShowOnly(lobbyScreen);
        public void ExitScreen() => ShowOnly(exitScreen);
        public void ExitGame()
        {
            Application.Quit();
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }

        private void SubscribePlayerState()
        {
            PlayerState playerState = PlayerState.Instance;
            if (playerState != null && subscribedPlayerState != playerState)
            {
                UnsubscribePlayerState();
                subscribedPlayerState = playerState;
                subscribedPlayerState.OnResearchCreditsChanged += UpdateResearchCreditsUI;
            }
        }

        private void UnsubscribePlayerState()
        {
            if (subscribedPlayerState == null)
                return;

            subscribedPlayerState.OnResearchCreditsChanged -= UpdateResearchCreditsUI;
            subscribedPlayerState = null;
        }

        private void UpdateResearchCreditsUI(int credits)
        {
            if (researchCreditsText != null)
                researchCreditsText.text = credits.ToString();
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

        private void HandleBackInput()
        {
            if (IsScreenActive(exitScreen))
                return;

            if (IsScreenActive(lobbyScreen))
            {
                ExitScreen();
                return;
            }

            Back();
        }

        private bool IsScreenActive(GameObject screen)
        {
            return screen != null && screen.activeSelf;
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
            SetActiveIfAssigned(optionScreen, target == optionScreen);
            SetActiveIfAssigned(exitScreen, target == exitScreen);
        }

        private void SetActiveIfAssigned(GameObject screen, bool active)
        {
            if (screen != null) {
                screen.SetActive(active);
            }
        }
    }
}
