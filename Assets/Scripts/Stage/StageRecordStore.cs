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

        public static void Save(StageRecord record)
        {
            StageRecordCollection collection = LoadAll();
            collection.records.Add(record);
            PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(collection));
            PlayerPrefs.Save();
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
