using System;
using UnityEngine;

namespace EpochGame.Economy
{
    [Serializable]
    public sealed class CurrencyState
    {
        [SerializeField] private long _crowns;

        public CurrencyState()
        {
        }

        public CurrencyState(long crowns)
        {
            _crowns = crowns;
        }

        public long Crowns => _crowns;
    }
}
