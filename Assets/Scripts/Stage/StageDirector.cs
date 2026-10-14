using System.Collections;
using Core.PlayerState;
using DamasDenis.Deployment;
using UnityEngine;

namespace DamasDenis.Stage
{
    /// <summary>Owns the configured briefing → survival → relocation/loss stage loop.</summary>
    public sealed class StageDirector : MonoBehaviour
    {
        private enum StagePhase { Briefing, Battle, Ending, Complete }

        [SerializeField] private StageDefinition stage;
        [SerializeField] private StageState stageState;
        [SerializeField] private PlayerState playerState;
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private DialogueController dialogueController;

        private StagePhase phase;
        private float elapsedSeconds;
        private StageRecord record;

        private void Awake()
        {
            if (stageState == null) stageState = StageState.Instance ?? FindObjectOfType<StageState>();
            if (stageState == null) stageState = gameObject.AddComponent<StageState>();
            if (playerState == null) playerState = PlayerState.Instance;
            if (waveManager != null) waveManager.autoStart = false;
            if (dialogueController != null) dialogueController.playOnStart = false;
        }

        private void OnEnable()
        {
            if (stageState != null) stageState.OnBaseDefeated += HandleBaseDefeated;
            StageEventLog.OnRecorded += HandleStageEvent;
        }

        private void OnDisable()
        {
            if (stageState != null) stageState.OnBaseDefeated -= HandleBaseDefeated;
            StageEventLog.OnRecorded -= HandleStageEvent;
        }

        private IEnumerator Start()
        {
            yield return null;
            if (stageState == null) stageState = StageState.Instance ?? FindObjectOfType<StageState>();
            if (playerState == null) playerState = PlayerState.Instance;
            if (stage == null || stageState == null)
            {
                Debug.LogError("StageDirector needs a StageDefinition and StageState.", this);
                enabled = false;
                yield return null;
            }

            stageState.Initialize(stage.BaseHealth, stage.StartingResources);
            stageState.OnBaseDefeated -= HandleBaseDefeated;
            stageState.OnBaseDefeated += HandleBaseDefeated;
            StaffCooldowns.ClearForNewStage();
            record = new StageRecord { stageId = stage.StageId };
            phase = StagePhase.Briefing;
            StageEventLog.Record("stage_briefing", stage.StageId);

            if (dialogueController == null)
                BeginBattle();
            else
                dialogueController.Play(stage.Briefing, BeginBattle);
        }

        private void Update()
        {
            if (phase != StagePhase.Battle) return;

            elapsedSeconds += Time.deltaTime;
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

        private void MarkComplete()
        {
            phase = StagePhase.Complete;
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
    }
}
