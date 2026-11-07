namespace EpochGame.Progress
{
    public readonly struct PlayerLevelProgress
    {
        public PlayerLevelProgress(
            int level,
            long population,
            float populationFill,
            long territory,
            float territoryFill)
        {
            Level = level;
            Population = population;
            PopulationFill = populationFill;
            Territory = territory;
            TerritoryFill = territoryFill;
        }

        public int Level { get; }

        public long Population { get; }

        public float PopulationFill { get; }

        public long Territory { get; }

        public float TerritoryFill { get; }
    }
}
