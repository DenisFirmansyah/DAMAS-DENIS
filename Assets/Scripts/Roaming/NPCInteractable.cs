using UnityEngine;

namespace DamasDenis.Roaming
{
    /// <summary>A character who speaks a fixed set of dialogue lines when interacted with.</summary>
    public sealed class NpcInteractable : Interactable
    {
        [Tooltip("The lines this character says, in order. Create them with Assets > Create > Dialogue.")]
        [SerializeField] private DialogueData[] lines;
        [Tooltip("Leave empty to use the DialogueController in the scene.")]
        [SerializeField] private DialogueController dialogueController;

        /// <inheritdoc />
        public override void Interact(CharacterControl interactor)
        {
            DialogueController dialogue = ResolveDialogue(dialogueController);
            if (dialogue == null || lines == null || lines.Length == 0)
            {
                Debug.LogWarning($"{name}: needs a DialogueController in the scene and at least one line.", this);
                return;
            }

            dialogue.Play(lines);
        }
    }
}