using UnityEngine;

namespace DamasDenis.UI
{
    /// <summary>Pause panel. Holding time is tied to being visible, so the two can't drift apart.</summary>
    public sealed class RoamingPauseScreen : UIScreen
    {
        protected override void OnBeforeShow() => GamePause.Hold(PauseReason.PauseMenu);
        protected override void OnBeforeHide() => GamePause.Release(PauseReason.PauseMenu);

        // Safety: if the panel is disabled while open, never leave the game frozen.
        protected override void OnDisable()
        {
            base.OnDisable();
            GamePause.Release(PauseReason.PauseMenu);
        }

        /// <summary>Resume button OnClick.</summary>
        public void Resume() => Hide();

        /// <summary>Quit button OnClick. LoadLobby already calls GamePause.ReleaseAll.</summary>
        public void QuitToLobby() => GameManager.Instance.LoadLobby();
        
    }
}