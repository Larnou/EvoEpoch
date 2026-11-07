using System;
using UnityEngine;

namespace EpochGame.Population
{
    [Serializable]
    public sealed class TerritoryState
    {
        [SerializeField] private long _area;

        public TerritoryState()
        {
        }

        public TerritoryState(long area)
        {
            _area = area;
        }

        public long Area => _area;

        public void Add(long amount)
        {
            if (amount > 0)
            {
                _area = amount > long.MaxValue - _area
                    ? long.MaxValue
                    : _area + amount;
                return;
            }

            if (amount < 0)
            {
                _area = _area < -amount ? 0 : _area + amount;
            }
        }
    }
}
