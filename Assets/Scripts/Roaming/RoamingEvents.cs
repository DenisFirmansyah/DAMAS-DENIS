namespace DamasDenis.Roaming
{
    /// <summary>Event names written to StageEventLog and read back by objectives.</summary>
    public static class RoamingEvents
    {
        public const string EvidenceCollected = "evidence_collected";
        public const string AllEvidenceCollected = "all_evidence_collected";
        public const string RoomUnlocked = "room_unlocked";
        public const string PartsSent = "parts_sent";
        public const string PartsDelayed = "parts_delayed";
    }
}