using System;
using System.Collections.Generic;
using UnityEngine;

namespace DamasDenis.Stage
{
    [Serializable]
    public sealed class StageEventRecord
    {
        public string eventType;
        public string detail;
        public float elapsedSeconds;
    }

    [Serializable]
    public sealed class StageRecord
    {
        public string stageId;
        public bool won;
        public float elapsedSeconds;
        public int stars;
        public bool starTwoMet;
        public bool starThreeMet;
        public List<StageEventRecord> events = new List<StageEventRecord>();
    }

    [Serializable]
    internal sealed class StageRecordCollection
    {
        public List<StageRecord> records = new List<StageRecord>();
    }

    public static class StageRecordStore
    {
        private const string PlayerPrefsKey = "DamasDenis.StageRecords";

        /// <summary>
        /// Saves an attempt, then trims that stage's history to its best win and its latest attempt.
        /// </summary>
        public static void Save(StageRecord record)
        {
            StageRecordCollection collection = LoadAll();
            collection.records.Add(record);
            Prune(collection.records, record.stageId);

            PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(collection));
            PlayerPrefs.Save();
        }

        private static void Prune(List<StageRecord> records, string stageId)
        {
            int latest = -1;
            int best = -1;
            int bestStars = -1;

            // Records are stored oldest first, so the last match is the latest attempt.
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].stageId != stageId) continue;

                latest = i;
                if (!records[i].won) continue;

                // Old wins saved before stars existed have stars = 0; they still count as 1 star.
                int stars = Mathf.Max(1, records[i].stars);

                // ">=" so a tie goes to the more recent win, which often makes best == latest.
                if (stars >= bestStars)
                {
                    bestStars = stars;
                    best = i;
                }
            }

            // Walk backwards so removing an entry never shifts an index we still need.
            for (int i = records.Count - 1; i >= 0; i--)
            {
                if (records[i].stageId == stageId && i != latest && i != best)
                    records.RemoveAt(i);
            }
        }

        public static List<StageRecord> GetAll()
        {
            return LoadAll().records;
        }

        private static StageRecordCollection LoadAll()
        {
            string json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return new StageRecordCollection();
            return JsonUtility.FromJson<StageRecordCollection>(json) ?? new StageRecordCollection();
        }
    }
}
