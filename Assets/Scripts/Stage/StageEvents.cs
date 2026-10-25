namespace DamasDenis.Stage
{
    /// <summary>Event names written to StageEventLog and read back by objectives.</summary>
    public static class StageEvents
    {
        public const string StaffDeployed = "staff_deployed";
        public const string StaffDied = "staff_died";
        public const string EnemyKilled = "enemy_killed";
        public const string EnemyLeaked = "enemy_leaked";
        public const string AllEnemiesCleared = "all_enemies_cleared";
        public const string StarsAwarded = "stars_awarded";
    }
}