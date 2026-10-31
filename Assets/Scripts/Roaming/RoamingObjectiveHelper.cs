using TMPro;
using UnityEngine;
using DamasDenis.Roaming;

namespace DamasDenis.UI
{
    /// <summary>Shows the spawner's current objective. Refreshes whenever a roaming event is logged.</summary>
    public sealed class RoamingObjectiveHelper : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private RoamingTaskSpawner spawner;

        private void OnEnable()
        {
            RoamingEventLog.OnRecorded += OnRecorded;
            Refresh();
        }

        private void OnDisable() => RoamingEventLog.OnRecorded -= OnRecorded;

        private void OnRecorded(string eventType, string detail) => Refresh();

        private void Refresh()
        {
            if (label != null && spawner != null) label.text = spawner.GetObjectiveText();
        }
    }
}