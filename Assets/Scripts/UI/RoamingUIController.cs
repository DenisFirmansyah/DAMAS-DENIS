using DamasDenis.Roaming;
using TMPro;
using UnityEngine;

namespace DamasDenis.UI
{
    /// <summary>Owns the Roaming scene's UI input: Escape toggles pause, and the "[F] ..." prompt.</summary>
    public sealed class RoamingUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterControl character;

        [Header("Pause")]
        [SerializeField] private RoamingPauseScreen pauseScreen;

        [Header("Interact Prompt")]
        [SerializeField] private GameObject promptRoot;
        [SerializeField] private TextMeshProUGUI promptText;
        [Tooltip("Only a label. CharacterControl owns the real key.")]
        [SerializeField] private string keyLabel = "F";

        private bool inputWasEnabled = true;
        private string shownPrompt;

        private void Awake()
        {
            if (promptRoot != null) promptRoot.SetActive(false);
        }

        private void Update()
        {
            HandlePause();
            RefreshPrompt();
        }

        private void LateUpdate()
        {
            if (character != null) inputWasEnabled = character.InputEnabled;
        }

        private void HandlePause()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            if (pauseScreen.IsVisible) { pauseScreen.Hide(); return; }

            if (GamePause.IsPaused || !inputWasEnabled) return;

            pauseScreen.Show();
        }

        private void RefreshPrompt()
        {
            IInteractable target = character != null ? character.CurrentInteractable : null;
            bool visible = target != null && character.InputEnabled && !GamePause.IsPaused;

            if (promptRoot.activeSelf != visible) promptRoot.SetActive(visible);
            if (!visible) { shownPrompt = null; return; }

            string text = $"[{keyLabel}] {target.Prompt}";
            if (text == shownPrompt) return;
            shownPrompt = text;
            promptText.text = text;
        }

        public void ReturnToLobby()
        {
            GameManager.Instance.LoadLobby();
        }
    }
}