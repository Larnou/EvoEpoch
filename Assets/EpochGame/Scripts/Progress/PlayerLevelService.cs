using System.Collections.Generic;

namespace EpochGame.Progress
{
    public sealed class PlayerLevelService
    {
        private readonly IReadOnlyList<PlayerLevelRequirement> _requirements;

        public PlayerLevelService(IReadOnlyList<PlayerLevelRequirement> requirements)
        {
            _requirements = requirements;
        }

        public PlayerLevelProgress Evaluate(int level, long population, long territory)
        {
            var requirement = Find(level + 1);
            var requiredPopulation = requirement != null ? requirement.Population : 0;
            var requiredTerritory = requirement != null ? requirement.Territory : 0;
            return new PlayerLevelProgress(
                level,
                population,
                Fill(population, requiredPopulation),
                territory,
                Fill(territory, requiredTerritory));
        }

        private PlayerLevelRequirement Find(int level)
        {
            if (_requirements == null)
            {
                return null;
            }

            for (var i = 0; i < _requirements.Count; i++)
            {
                var requirement = _requirements[i];
                if (requirement != null && requirement.Level == level)
                {
                    return requirement;
                }
            }

            return null;
        }

        private static float Fill(long current, long required)
        {
            if (required <= 0)
            {
                return 1f;
            }

            if (current <= 0)
            {
                return 0f;
            }

            if (current >= required)
            {
                return 1f;
            }

            return (float)((double)current / required);
        }
    }
}
