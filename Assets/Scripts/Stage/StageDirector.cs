using System.Collections;
using DamasDenis.Core;
using DamasDenis.Deployment;
using DamasDenis.Enemies;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DamasDenis.Stage
{
    /// <summary>
    /// Owns the stage lifecycle (briefing → survival → relocation/loss).
    /// Observes scene loading to dynamically bind to additive stage map scenes and their unit containers.
    /// </summary>
    public sealed class StageDirector : MonoBehaviour
    {
        private enum StagePhase { AwaitingStage, Briefing, Battle, Ending, Complete }

        [Header("Stage Definition (Default / Fallback)")]
        [SerializeField] private StageDefinition stage;

        [Header("Controllers & References")]
        [SerializeField] private StageState stageState;
        [SerializeField] private PlayerState playerState;
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private DeploymentController deploymentController;
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private StageCameraFramer cameraFramer;

        private StagePhase phase = StagePhase.AwaitingStage;
        private float elapsedSeconds;
        private StageRecord record;
        private Scene currentStageScene;

        public StageDefinition CurrentStage => stage;
        public bool IsBattleActive => phase == StagePhase.Battle;
        private bool wavesSpawned;

        public event System.Action<StageRecord> StageFinished;
        public float RemainingSeconds => phase == StagePhase.Battle && stage != null ? Mathf.Max(0f, stage.SurvivalSeconds - elapsedSeconds) : 0f;

        private void Awake()
        {
            if (stageState == null) stageState = StageState.Instance ?? FindObjectOfType<StageState>();
            if (stageState == null) stageState = gameObject.AddComponent<StageState>();
            if (playerState == null) playerState = PlayerState.Instance;
            if (deploymentController == null) deploymentController = FindObjectOfType<DeploymentController>();
            if (waveManager != null) waveManager.autoStart = false;
            if (dialogueController != null) dialogueController.playOnStart = false;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            SceneManager.sceneUnloaded += HandleSceneUnloaded;

            if (stageState != null) stageState.OnBaseDefeated += HandleBaseDefeated;
            StageEventLog.OnRecorded += HandleStageEvent;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneUnloaded -= HandleSceneUnloaded;

            if (stageState != null) stageState.OnBaseDefeated -= HandleBaseDefeated;
            StageEventLog.OnRecorded -= HandleStageEvent;
        }

        private void Start()
        {
            if (stageState == null) stageState = StageState.Instance ?? FindObjectOfType<StageState>();
            if (playerState == null) playerState = PlayerState.Instance;
            if (deploymentController == null) deploymentController = FindObjectOfType<DeploymentController>();

            // Check if a stage map scene is already loaded (e.g. multi-scene workflow in editor)
            Scene alreadyLoadedStage = FindLoadedStageScene();
            if (alreadyLoadedStage.IsValid() && alreadyLoadedStage.isLoaded)
            {
                BindStageScene(alreadyLoadedStage);
            }
        }

        private void HandleSceneLoaded(Scene loadedScene, LoadSceneMode mode)
        {
            if (IsStageScene(loadedScene))
            {
                BindStageScene(loadedScene);
            }
        }

        private void HandleSceneUnloaded(Scene unloadedScene)
        {
            if (currentStageScene.IsValid() && currentStageScene == unloadedScene)
            {
                if (waveManager != null) waveManager.StopWaves();

                if (waveManager != null) waveManager.OnWavesCompleted -= HandleWavesCompleted;

                waveManager = null;
                phase = StagePhase.AwaitingStage;
                currentStageScene = default;
            }
        }

        private void BindStageScene(Scene stageScene)
        {
            currentStageScene = stageScene;
            elapsedSeconds = 0f;

            // 1. Resolve StageDefinition from StageMapContext, GameManager, or fallback
            StageMapContext mapContext = FindMapContextInScene(stageScene);
            if (mapContext != null && mapContext.StageDefinition != null)
            {
                stage = mapContext.StageDefinition;
            }
            else if (GameManager.Instance != null && GameManager.Instance.CurrentStageDefinition != null)
            {
                stage = GameManager.Instance.CurrentStageDefinition;
            }

            if (stage == null)
            {
                Debug.LogWarning($"StageDirector: Stage scene '{stageScene.name}' loaded, but no StageDefinition was found.", this);
                return;
            }

            // 2. Resolve Unit Container in the stage scene & wire to DeploymentController
            Transform stageUnitContainer = null;
            if (mapContext != null)
            {
                stageUnitContainer = mapContext.GetOrCreateUnitContainer();
            }
            else
            {
                stageUnitContainer = FindOrCreateUnitContainerInScene(stageScene);
            }

            if (deploymentController == null)
                deploymentController = FindObjectOfType<DeploymentController>();

            if (deploymentController != null && stageUnitContainer != null)
            {
                deploymentController.SetUnitContainer(stageUnitContainer);
            }

            if (cameraFramer != null) cameraFramer.Frame();

            // 3. Resolve WaveManager in the stage scene
            if (waveManager == null)
                waveManager = FindWaveManagerInScene(stageScene);

            wavesSpawned = false;
            if (waveManager != null)
            {
                waveManager.OnWavesCompleted -= HandleWavesCompleted;
                waveManager.OnWavesCompleted += HandleWavesCompleted;
            }

            // 4. Initialize StageState for this stage
            if (stageState == null) stageState = StageState.Instance ?? FindObjectOfType<StageState>();
            if (stageState == null) stageState = gameObject.AddComponent<StageState>();

            stageState.Initialize(stage.BaseHealth, stage.StartingResources);
            stageState.OnBaseDefeated -= HandleBaseDefeated;
            stageState.OnBaseDefeated += HandleBaseDefeated;

            StaffCooldowns.ClearForNewStage();
            record = new StageRecord { stageId = stage.StageId };
            phase = StagePhase.Briefing;
            StageEventLog.Record("stage_briefing", stage.StageId);

            // 4. Start briefing or jump directly to battle
            if (dialogueController == null)
                BeginBattle();
            else
                dialogueController.Play(stage.Briefing, BeginBattle);
        }

        private void HandleWavesCompleted() { wavesSpawned = true; }

        private void Update()
        {
            if (phase != StagePhase.Battle) return;

            elapsedSeconds += Time.deltaTime;

            // Early win first, so a clear on the same frame as the timer still counts as cleared.
            if (wavesSpawned && EnemyInstance.Active.Count == 0)
            {
                StageEventLog.Record(StageEvents.AllEnemiesCleared, stage.StageId);
                FinishStage(true);
                return;
            }

            if (elapsedSeconds >= stage.SurvivalSeconds)
                FinishStage(true);
        }

        private void BeginBattle()
        {
            if (phase != StagePhase.Briefing) return;
            phase = StagePhase.Battle;
            StageEventLog.Record("battle_started", stage.StageId);
            if (waveManager != null) waveManager.BeginWaves();
        }

        private void HandleBaseDefeated()
        {
            if (phase == StagePhase.Battle) FinishStage(false);
        }

        private void FinishStage(bool won)
        {
            if (phase == StagePhase.Ending || phase == StagePhase.Complete) return;
            phase = StagePhase.Ending;
            if (waveManager != null) waveManager.StopWaves();

            record.won = won;
            record.elapsedSeconds = elapsedSeconds;
            if (won) EvaluateStars();
            StageEventLog.Record(won ? "stage_won" : "stage_lost", stage.StageId);

            if (won)
            {
                if (playerState != null)
                    playerState.AddResearchCredits(stage.CompletionResources);

                if (stage.IsOptionalStage && stage.UnlockedStaff != null)
                {
                    StaffUnlocks.Unlock(stage.UnlockedStaff.UnlockId);
                    StageEventLog.Record("staff_unlocked", stage.UnlockedStaff.DisplayName);
                }
            }

            if (playerState != null)
                playerState.RecordStageCompletion(record);
            else
                StageRecordStore.Save(record);

            if (won && dialogueController != null)
                dialogueController.Play(stage.RelocationDialogue, MarkComplete);
            else
                MarkComplete();
        }

        private void EvaluateStars()
        {
            record.starTwoMet = stage.StarTwoObjective != null && stage.StarTwoObjective.IsMet(record);
            record.starThreeMet = stage.StarThreeObjective != null && stage.StarThreeObjective.IsMet(record);
            record.stars = 1 + (record.starTwoMet ? 1 : 0) + (record.starThreeMet ? 1 : 0);
            StageEventLog.Record(StageEvents.StarsAwarded, record.stars.ToString());
        }

        private void MarkComplete()
        {
            phase = StagePhase.Complete;
            StageFinished?.Invoke(record);
        }

        private void HandleStageEvent(string eventType, string detail)
        {
            if (record == null || phase == StagePhase.Complete) return;
            record.events.Add(new StageEventRecord
            {
                eventType = eventType,
                detail = detail,
                elapsedSeconds = elapsedSeconds
            });
        }

        #region Helpers

        private Scene FindLoadedStageScene()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (IsStageScene(scene) && scene.isLoaded)
                    return scene;
            }
            return default;
        }

        private bool IsStageScene(Scene scene)
        {
            if (!scene.IsValid()) return false;

            string sceneName = scene.name;
            if (sceneName == GameManager.MainMenuScene ||
                sceneName == GameManager.GameplayScene ||
                sceneName == GameManager.ReviewScene ||
                sceneName == "Bootstrap")
            {
                return false;
            }

            if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.CurrentStage))
            {
                return sceneName == GameManager.Instance.CurrentStage;
            }

            return true;
        }

        private StageMapContext FindMapContextInScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded) return null;

            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                StageMapContext ctx = rootObjects[i].GetComponentInChildren<StageMapContext>();
                if (ctx != null) return ctx;
            }
            return null;
        }

        private Transform FindOrCreateUnitContainerInScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded) return null;

            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                if (rootObjects[i].name.Equals("Unit Container", System.StringComparison.OrdinalIgnoreCase) ||
                    rootObjects[i].name.Equals("UnitContainer", System.StringComparison.OrdinalIgnoreCase))
                {
                    return rootObjects[i].transform;
                }
            }

            GameObject containerGo = new GameObject("Unit Container");
            SceneManager.MoveGameObjectToScene(containerGo, scene);
            return containerGo.transform;
        }

        private WaveManager FindWaveManagerInScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return null;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                WaveManager manager = root.GetComponentInChildren<WaveManager>(true);

                if (manager != null)
                    return manager;
            }

            return null;
        }

        #endregion
    }
}
