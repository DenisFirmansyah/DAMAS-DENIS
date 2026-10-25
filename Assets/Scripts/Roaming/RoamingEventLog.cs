using System;

namespace DamasDenis.Roaming
{
    public static class RoamingEventLog
    {
        public static event Action<string, string> OnRecorded;

        public static void Record(string eventType, string detail)
        {
            OnRecorded?.Invoke(eventType, detail);
        }
    }
}
