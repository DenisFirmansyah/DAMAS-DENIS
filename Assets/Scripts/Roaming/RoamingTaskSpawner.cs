using System;
using DamasDenis.Core;
using DamasDenis.Stage;
using UnityEngine;

namespace DamasDenis.Roaming
{
    /// <summary>
    /// Spawns the collect/store objects for every stage the player has won, skipping steps
    /// already done. Also answers "what should the player do next?" for the objective helper.
    /// </summary>
    public sealed class RoamingTaskSpawner : MonoBehaviour
    {
        [Serializable]
        private sealed class Entry
        {
            [Tooltip("The task appears once this stage has been won.")]
            public StageDefinition stage;
            public ProgressStepInteractable collectPrefab;
            public Transform collectPoint;
            public ProgressStepInteractable storePrefab;
            public Transform storePoint;
            public string collectText = "Collect the parts";
            public string storeText = "Bring them to storage";
            [NonSerialized] public Transform collectInstance;
            [NonSerialized] public Transform storeInstance;
        }

        [SerializeField] private Entry[] entries;
        [SerializeField] private string allDoneText = "Nothing left to do. Head back when you're ready.";



        private void Start()
        {
            foreach (Entry e in entries)
                if (IsUnlocked(e)) SpawnEntry(e);
        }

        /// <summary>Text for the first unfinished step of any unlocked task.</summary>
        public string GetObjectiveText()
        {
            foreach (Entry e in entries)
            {
                if (!IsUnlocked(e)) continue;
                if (!RoamingProgress.IsDone(CollectedId(e))) return e.collectText;
                if (!RoamingProgress.IsDone(StoredId(e))) return e.storeText;
            }
            return allDoneText;
        }

        private static bool IsUnlocked(Entry e) =>
            e.stage != null && PlayerState.Instance.HasCompletedStage(e.stage.StageId);

        private static string CollectedId(Entry e) => $"{e.stage.StageId}.parts.collected";
        private static string StoredId(Entry e) => $"{e.stage.StageId}.parts.stored";

        private void SpawnEntry(Entry e)
        {
            if (e.collectPrefab == null || e.collectPoint == null ||
                e.storePrefab == null || e.storePoint == null)
            {
                Debug.LogWarning($"Roaming task for '{e.stage.name}' is missing a prefab or point.", this);
                return;
            }

            if (RoamingProgress.IsDone(StoredId(e))) return; // fully finished, nothing to show

            if (!RoamingProgress.IsDone(CollectedId(e)))
            {
                var collect = Instantiate(e.collectPrefab, e.collectPoint.position, e.collectPoint.rotation, transform);
                collect.Configure(CollectedId(e), string.Empty);
                e.collectInstance = collect.transform;
            }

            var store = Instantiate(e.storePrefab, e.storePoint.position, e.storePoint.rotation, transform);
            store.Configure(StoredId(e), CollectedId(e));
            e.storeInstance = store.transform;
        }

        public Transform GetObjectiveTarget()
        {
            foreach (Entry e in entries)
            {
                if (!IsUnlocked(e)) continue;
                if (!RoamingProgress.IsDone(CollectedId(e))) return e.collectInstance;
                if (!RoamingProgress.IsDone(StoredId(e))) return e.storeInstance;
            }
            return null;
        }
    }
}