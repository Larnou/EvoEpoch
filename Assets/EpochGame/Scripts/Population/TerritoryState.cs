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
    }
}
