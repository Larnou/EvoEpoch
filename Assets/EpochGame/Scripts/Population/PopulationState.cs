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
    }
}
