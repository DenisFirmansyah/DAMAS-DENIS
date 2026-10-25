using UnityEngine;

namespace DamasDenis.Roaming
{
    /// <summary>
    /// A door that can require earlier progress (e.g. a collected keycard) and, once opened,
    /// stays open forever, including after the scene reloads.
    /// Put the closed visual, its blocking collider and the interaction collider under
    /// "Closed State". Turning that object off removes all three at once.
    /// </summary>
    public sealed class DoorInteractable : Interactable
    {
        [Header("Identity")]
        [Tooltip("Unique and never changed once shipped. Saved in RoamingProgress.")]
        [SerializeField] private string doorId = "door.new";

        [Header("Lock")]
        [Tooltip("Progress id that must be done first (e.g. an evidence id). Empty = never locked.")]
        [SerializeField] private string requiredProgressId;
        [Tooltip("Shown when the player tries the door while it is locked. Optional.")]
        [SerializeField] private DialogueData[] lockedLines;
        [SerializeField] private DialogueController dialogueController;

        [Header("States")]
        [Tooltip("Visual + blocking collider + interaction collider of the closed door.")]
        [SerializeField] private GameObject closedState;
        [Tooltip("Visual of the open door, with no collider. Optional.")]
        [SerializeField] private GameObject openState;

        private bool isOpen;

        /// <summary>True once the door has been opened (now or in an earlier session).</summary>
        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (RoamingProgress.IsDone(doorId))
                ApplyOpenState();
            else if (openState != null)
                openState.SetActive(false);
        }

        /// <inheritdoc />
        public override void Interact(CharacterControl interactor)
        {
            if (isOpen) return;

            bool locked = !string.IsNullOrWhiteSpace(requiredProgressId) &&
                          !RoamingProgress.IsDone(requiredProgressId);
            if (locked)
            {
                PlayLockedFeedback();
                return;
            }

            // MarkDone is true only the first time, so the event is logged once.
            if (RoamingProgress.MarkDone(doorId))
                RoamingEventLog.Record(RoamingEvents.RoomUnlocked, doorId);

            ApplyOpenState();
        }

        private void PlayLockedFeedback()
        {
            if (lockedLines == null || lockedLines.Length == 0) return;

            DialogueController dialogue = ResolveDialogue(dialogueController);
            if (dialogue != null) dialogue.Play(lockedLines);
        }

        private void ApplyOpenState()
        {
            isOpen = true;
            if (closedState != null) closedState.SetActive(false);
            if (openState != null) openState.SetActive(true);
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(doorId))
                Debug.LogWarning($"{name}: Door Id is empty, so opening it can't be saved.", this);
        }

        /// <summary>Right-click the component header to wipe saved progress while testing in the editor.</summary>
        [ContextMenu("Clear ALL roaming progress")]
        private void ClearProgress() => RoamingProgress.ClearAll();
    }
}