using UnityEngine;
using UnityEngine.Events;

namespace DamasDenis.Roaming
{
    /// <summary>
    /// One step of a roaming task (e.g. "collect parts" or "store parts"). Optionally requires
    /// an earlier step. When done it is saved in RoamingProgress and logged once.
    /// </summary>
    public sealed class ProgressStepInteractable : Interactable
    {
        [Header("Identity")]
        [Tooltip("Unique id, saved in RoamingProgress. e.g. stage1.parts.collected")]
        [SerializeField] private string stepId = "step.new";
        [Tooltip("Step that must be done first. Empty = no requirement.")]
        [SerializeField] private string requiredStepId;
        [Tooltip("Event name written to RoamingEventLog, e.g. evidence_collected or parts_sent.")]
        [SerializeField] private string eventType = RoamingEvents.EvidenceCollected;

        [Header("Feedback")]
        [Tooltip("Said when the player tries this before the required step is done. Optional.")]
        [SerializeField] private DialogueData[] lockedLines;
        [SerializeField] private DialogueController dialogueController;
        [Tooltip("Turned OFF when done (the pickup visual + its collider). Optional.")]
        [SerializeField] private GameObject hideWhenDone;
        [Tooltip("Runs once when completed (e.g. show a 'stored' visual). Optional.")]
        [SerializeField] private UnityEvent onCompleted;

        private void Start()
        {
            if (RoamingProgress.IsDone(stepId)) ApplyDone(false);
        }

        public void Configure(string id, string requiredId)
        {
            stepId = id;
            requiredStepId = requiredId;
        }
        
        public override void Interact(CharacterControl interactor)
        {
            if (RoamingProgress.IsDone(stepId)) return;

            bool locked = !string.IsNullOrWhiteSpace(requiredStepId) &&
                          !RoamingProgress.IsDone(requiredStepId);
            if (locked)
            {
                DialogueController dialogue = ResolveDialogue(dialogueController);
                if (dialogue != null && lockedLines != null && lockedLines.Length > 0)
                    dialogue.Play(lockedLines);
                return;
            }

            // MarkDone is true only the first time, so the event is logged once.
            if (RoamingProgress.MarkDone(stepId))
                RoamingEventLog.Record(eventType, stepId);

            ApplyDone(true);
        }

        private void ApplyDone(bool fresh)
        {
            if (hideWhenDone != null) hideWhenDone.SetActive(false);
            if (fresh) onCompleted?.Invoke();
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(stepId))
                Debug.LogWarning($"{name}: Step Id is empty, so it can't be saved.", this);
        }
    }
}