using DamasDenis.UI;
using UnityEngine;

namespace DamasDenis.Roaming
{
    /// <summary>An object (door, elevator, terminal) that sends the player back to the lobby.</summary>
    public sealed class LobbyExitInteractable : Interactable
    {
        [SerializeField] private RoamingUIController uiController;

        /// <inheritdoc />
        public override void Interact(CharacterControl interactor)
        {
            if (uiController == null)
            {
                Debug.LogWarning($"{name}: needs a RoamingUIController assigned.", this);
                return;
            }

            uiController.ReturnToLobby();
        }
    }
}