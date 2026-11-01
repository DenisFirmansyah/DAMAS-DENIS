using UnityEngine;
using UnityEngine.UI;

namespace DamasDenis.UI
{
    /// <summary>Plays a click cue when this button is pressed.</summary>
    [RequireComponent(typeof(Button))]
    public sealed class UIButtonSfx : MonoBehaviour
    {
        [SerializeField] private SfxCue clickCue;

        private void Awake() => GetComponent<Button>().onClick.AddListener(Play);

        /// <summary>Also callable from a UnityEvent for non-button actions.</summary>
        public void Play() => AudioManager.Instance?.PlaySFX(clickCue);
    }
}