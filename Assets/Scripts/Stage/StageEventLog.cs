using System;

namespace DamasDenis.Stage
{
    public static class StageEventLog
    {
        public static event Action<string, string> OnRecorded;

        public static void Record(string eventType, string detail)
        {
            OnRecorded?.Invoke(eventType, detail);
        }
    }
}
