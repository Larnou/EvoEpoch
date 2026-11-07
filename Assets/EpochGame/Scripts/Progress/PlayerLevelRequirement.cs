using System;
using UnityEngine;

namespace EpochGame.Progress
{
    [Serializable]
    public sealed class PlayerLevelRequirement
    {
        [SerializeField] private int _level;
        [SerializeField] private long _population;
        [SerializeField] private long _territory;

        public PlayerLevelRequirement()
        {
        }

        public PlayerLevelRequirement(int level, long population, long territory)
        {
            _level = level;
            _population = population;
            _territory = territory;
        }

        public int Level => _level;

        public long Population => _population;

        public long Territory => _territory;
    }
}
