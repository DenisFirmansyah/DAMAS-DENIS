using System;
using System.Collections.Generic;
using UnityEngine;

namespace DamasDenis.Roaming
{
    [Serializable]
    public sealed class EvidenceEventRecord
    {
        public string eventType;
        public string detail;
    }

    [Serializable]
    public sealed class EvidenceRecord
    {
        public string evidenceId;
        public bool collected;
        public List<EvidenceEventRecord> events = new List<EvidenceEventRecord>();
    }

    [Serializable]
    internal sealed class EvidenceRecordCollection
    {
        public List<EvidenceRecord> records = new List<EvidenceRecord>();
    }

    public static class EvidenceRecordStore
    {
        private const string PlayerPrefsKey = "DamasDenis.EvidenceRecords";

        /// <summary>
        /// Saves an attempt, then trims that Evidence's history to its best win and its latest attempt.
        /// </summary>
        public static void Save(EvidenceRecord record)
        {
            EvidenceRecordCollection collection = LoadAll();
            collection.records.Add(record);

            PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(collection));
            PlayerPrefs.Save();
        }

        public static List<EvidenceRecord> GetAll()
        {
            return LoadAll().records;
        }

        private static EvidenceRecordCollection LoadAll()
        {
            string json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return new EvidenceRecordCollection();
            return JsonUtility.FromJson<EvidenceRecordCollection>(json) ?? new EvidenceRecordCollection();
        }
    }
}
