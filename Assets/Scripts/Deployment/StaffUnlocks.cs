using UnityEngine;

namespace DamasDenis.Deployment
{
    public static class StaffUnlocks
    {
        private const string KeyPrefix = "DamasDenis.StaffUnlock.";

        public static bool IsUnlocked(string unlockId)
        {
            return !string.IsNullOrWhiteSpace(unlockId) && PlayerPrefs.GetInt(KeyPrefix + unlockId, 0) == 1;
        }

        public static void Unlock(string unlockId)
        {
            if (string.IsNullOrWhiteSpace(unlockId))
                return;

            PlayerPrefs.SetInt(KeyPrefix + unlockId, 1);
            PlayerPrefs.Save();
        }

        public static void Clear(string unlockId)
        {
            if (string.IsNullOrWhiteSpace(unlockId))
                return;

            PlayerPrefs.DeleteKey(KeyPrefix + unlockId);
            PlayerPrefs.Save();
        }
    }
}
