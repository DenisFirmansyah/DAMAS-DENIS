using System.Collections.Generic;
using UnityEngine;

namespace DamasDenis.Roaming
{
    /// <summary>
    /// Permanent "this is done" flags for roaming: doors opened, evidence collected.
    /// Everything lives under ONE PlayerPrefs key, so ClearAll can wipe it
    /// (PlayerPrefs cannot list keys, so per-id keys could never be reset).
    /// IDs must be unique across doors and evidence.
    /// </summary>
    public static class RoamingProgress
    {
        private const string PrefsKey = "DamasDenis.RoamingProgress";

        private static HashSet<string> done;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => done = null;

        private static HashSet<string> Done
        {
            get
            {
                if (done != null) return done;

                done = new HashSet<string>();
                string raw = PlayerPrefs.GetString(PrefsKey, string.Empty);
                foreach (string id in raw.Split('\n'))
                {
                    if (!string.IsNullOrWhiteSpace(id)) done.Add(id);
                }
                return done;
            }
        }

        /// <summary>True if this id was marked done, now or in an earlier session.</summary>
        public static bool IsDone(string id)
        {
            return !string.IsNullOrWhiteSpace(id) && Done.Contains(id);
        }

        /// <summary>Marks an id done and saves. Returns true only the FIRST time (use it to log once).</summary>
        public static bool MarkDone(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Done.Add(id)) return false;

            PlayerPrefs.SetString(PrefsKey, string.Join("\n", Done));
            PlayerPrefs.Save();
            return true;
        }

        /// <summary>Forgets all roaming progress. Call from a "new game" / reset path.</summary>
        public static void ClearAll()
        {
            done = new HashSet<string>();
            PlayerPrefs.DeleteKey(PrefsKey);
            PlayerPrefs.Save();
        }
    }
}