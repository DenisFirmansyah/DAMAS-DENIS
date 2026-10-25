using TMPro;
using UnityEngine;

namespace DamasDenis.Roaming
{
    /// <summary>
    /// A terminal or collectible that opens a UI panel. The first time it is opened it can
    /// also mark evidence as collected (which doors can require). Closes with F or Escape,
    /// or from a Close button calling Close().
    /// </summary>
    public sealed class EvidenceInteractable : Interactable
    {
        [Header("UI")]
        [Tooltip("The panel to show. Starts hidden.")]
        [SerializeField] private GameObject panel;
        [Tooltip("Optional. Filled with Title below when opened.")]
        [SerializeField] private TextMeshProUGUI titleText;
        [Tooltip("Optional. Filled with Body below when opened.")]
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private string title;
        [TextArea(3, 10)] [SerializeField] private string body;
        [SerializeField] private KeyCode closeKey = KeyCode.F;

        [Header("Evidence")]
        [Tooltip("Marked collected the first time this opens. Empty = just a UI, nothing to collect.")]
        [SerializeField] private string evidenceId;

        private CharacterControl user;
        private bool isOpen;
        private int openedFrame;

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);
        }

        /// <inheritdoc />
        public override void Interact(CharacterControl interactor)
        {
            if (isOpen || panel == null) return;

            isOpen = true;
            openedFrame = Time.frameCount;
            user = interactor;
            if (user != null) user.InputEnabled = false;

            if (titleText != null) titleText.text = title;
            if (bodyText != null) bodyText.text = body;
            panel.SetActive(true);

            if (RoamingProgress.MarkDone(evidenceId))
                RoamingEventLog.Record(RoamingEvents.EvidenceCollected, evidenceId);
        }

        private void Update()
        {
            // Skip the frame we opened on: the same F press would otherwise close it instantly.
            if (!isOpen || Time.frameCount == openedFrame) return;

            if (Input.GetKeyDown(closeKey) || Input.GetKeyDown(KeyCode.Escape))
                Close();
        }

        /// <summary>Hides the panel and gives movement back. Also usable from a UI Button's OnClick.</summary>
        public void Close()
        {
            if (!isOpen) return;

            isOpen = false;
            if (panel != null) panel.SetActive(false);
            if (user != null) user.InputEnabled = true;
            user = null;
        }

        // Never leave the player frozen if this object is turned off or destroyed while open.
        private void OnDisable() => Close();
    }
}