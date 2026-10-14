using System.Collections.Generic;
using UnityEngine;

namespace DamasDenis.Deployment
{
    /// <summary>Session-only redeployment cooldowns, keyed by stable staff unlock IDs.</summary>
    public static class StaffCooldowns
    {
        private static readonly Dictionary<string, float> EndTimes = new Dictionary<string, float>();

        public static bool IsCoolingDown(string staffId)
        {
            float endTime;
            return !string.IsNullOrWhiteSpace(staffId) && EndTimes.TryGetValue(staffId, out endTime) && Time.time < endTime;
        }

        public static void StartCooldown(string staffId, float duration)
        {
            if (!string.IsNullOrWhiteSpace(staffId))
                EndTimes[staffId] = Time.time + Mathf.Max(0f, duration);
        }

        public static void ClearForNewStage()
        {
            EndTimes.Clear();
        }
    }
}
