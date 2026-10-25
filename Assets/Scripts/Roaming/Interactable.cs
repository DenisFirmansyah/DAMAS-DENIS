using UnityEngine;

namespace DamasDenis.Roaming
{
    /// <summary>
    /// Base for everything the player can press F on. CharacterControl finds these through
    /// the IInteractable interface, so subclasses only decide WHAT happens.
    /// The collider that triggers it must be on this object or a child, on the Interactable layer.
    /// </summary>
    public abstract class Interactable : MonoBehaviour, IInteractable
    {
        [Tooltip("Short text for the on-screen prompt, e.g. 'Talk' or 'Open'.")]
        [SerializeField] private string prompt = "Interact";

        /// <summary>Short text for a prompt, e.g. "Talk".</summary>
        public virtual string Prompt => prompt;

        /// <summary>Runs once when the player presses the interact key on this.</summary>
        public abstract void Interact(CharacterControl interactor);

        /// <summary>Uses the assigned DialogueController, or finds the one in the scene.</summary>
        protected static DialogueController ResolveDialogue(DialogueController assigned)
        {
            return assigned != null ? assigned : FindFirstObjectByType<DialogueController>();
        }
    }
}