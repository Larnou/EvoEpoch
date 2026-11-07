using System;
using UnityEngine;

namespace EpochGame.Population
{
    [Serializable]
    public sealed class PopulationState
    {
        [SerializeField] private long _currentPopulation;

        public PopulationState()
        {
        }

        public PopulationState(long currentPopulation)
        {
            _currentPopulation = currentPopulation;
        }

        public long CurrentPopulation => _currentPopulation;

        public void Add(long amount)
        {
            if (amount > 0)
            {
                _currentPopulation = amount > long.MaxValue - _currentPopulation
                    ? long.MaxValue
                    : _currentPopulation + amount;
                return;
            }

            if (amount < 0)
            {
                _currentPopulation = _currentPopulation < -amount ? 0 : _currentPopulation + amount;
            }
        }
    }
}
