using System.Collections.Generic;
using DamasDenis.Stage;
using UnityEngine;

namespace DamasDenis.Core
{
    /// <summary>
    /// Persistent player state across gameplay (not reset per stage).
    /// Holds persistent meta-progression resources (Research Credits for unit upgrades)
    /// and player progress referencing the StageRecordStore.
    /// </summary>
    public class PlayerState : MonoBehaviour
    {
        private static PlayerState _instance;
        public static PlayerState Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PlayerState>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("PlayerState");
                        _instance = go.AddComponent<PlayerState>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private const string PrefsResearchCreditsKey = "DamasDenis.ResearchCredits";

        [Header("Persistent Progression - Unit Upgrade Currency")]
        [SerializeField] private int researchCredits;

        public int ResearchCredits => researchCredits;

        public event System.Action<int> OnResearchCreditsChanged;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadPersistentData();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        private void LoadPersistentData()
        {
            researchCredits = PlayerPrefs.GetInt(PrefsResearchCreditsKey, 0);
        }

        private void SavePersistentData()
        {
            PlayerPrefs.SetInt(PrefsResearchCreditsKey, researchCredits);
            PlayerPrefs.Save();
        }

        #region Persistent Economy (Unit Upgrades)

        /// <summary>
        /// Adds persistent research credits (e.g. earned from stage completion) and saves.
        /// </summary>
        public void AddResearchCredits(int amount)
        {
            if (amount <= 0) return;

            researchCredits += amount;
            SavePersistentData();
            OnResearchCreditsChanged?.Invoke(researchCredits);
        }

        /// <summary>
        /// Attempts to spend persistent research credits on unit upgrades.
        /// </summary>
        public bool TrySpendResearchCredits(int amount)
        {
            if (amount <= 0) return true;

            if (researchCredits >= amount)
            {
                researchCredits -= amount;
                SavePersistentData();
                OnResearchCreditsChanged?.Invoke(researchCredits);
                return true;
            }

            return false;
        }

        public void SetResearchCredits(int amount)
        {
            researchCredits = Mathf.Max(0, amount);
            SavePersistentData();
            OnResearchCreditsChanged?.Invoke(researchCredits);
        }

        #endregion

        #region Player Progress (Stage Records)

        /// <summary>
        /// Retrieves all completed and recorded stage attempts from StageRecordStore.
        /// </summary>
        public List<StageRecord> GetAllStageRecords()
        {
            return StageRecordStore.GetAll();
        }

        /// <summary>
        /// Checks whether a specific stage has been completed with a win.
        /// </summary>
        public bool HasCompletedStage(string stageId)
        {
            if (string.IsNullOrEmpty(stageId)) return false;

            List<StageRecord> records = StageRecordStore.GetAll();
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].stageId == stageId && records[i].won)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the total count of distinct stages won.
        /// </summary>
        public int GetCompletedStageCount()
        {
            List<StageRecord> records = StageRecordStore.GetAll();
            HashSet<string> wonStages = new HashSet<string>();
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].won)
                    wonStages.Add(records[i].stageId);
            }
            return wonStages.Count;
        }

        /// <summary>
        /// Gets the latest recorded attempt for a stage.
        /// </summary>
        public StageRecord GetLatestRecord(string stageId)
        {
            List<StageRecord> records = StageRecordStore.GetAll();
            for (int i = records.Count - 1; i >= 0; i--)
            {
                if (records[i].stageId == stageId)
                    return records[i];
            }
            return null;
        }

        /// <summary>
        /// Records a completed or failed stage run to persistent storage.
        /// </summary>
        public void RecordStageCompletion(StageRecord record)
        {
            if (record != null)
                StageRecordStore.Save(record);
        }

        /// <summary>Best star count earned on a stage across all attempts. 0 if never won.</summary>
        public int GetBestStars(string stageId)
        {
            if (string.IsNullOrEmpty(stageId)) return 0;

            int best = 0;
            List<StageRecord> records = StageRecordStore.GetAll();
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].stageId != stageId || !records[i].won) continue;

                // Records saved before stars existed have stars = 0 but were still wins.
                best = Mathf.Max(best, Mathf.Max(1, records[i].stars));
            }
            return best;
        }

        /// <summary>Total stars across the given stages. The lobby can use this for a total.</summary>
        public int GetTotalStars(IReadOnlyList<StageDefinition> stages)
        {
            int total = 0;
            for (int i = 0; i < stages.Count; i++)
                if (stages[i] != null) total += GetBestStars(stages[i].StageId);
            return total;
        }

        /// <summary>
        /// Clears all persistent player progress and research credits.
        /// </summary>
        [ContextMenu("Debug/Reset All Progress")]
        public void ResetAllProgress()
        {
            researchCredits = 0;
            PlayerPrefs.DeleteKey(PrefsResearchCreditsKey);
            PlayerPrefs.DeleteKey("DamasDenis.StageRecords");
            PlayerPrefs.Save();
            OnResearchCreditsChanged?.Invoke(researchCredits);
            Debug.Log("Progress has been reset!");
        }

        #endregion
    }
}
