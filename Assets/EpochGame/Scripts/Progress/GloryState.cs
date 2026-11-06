using System;
using UnityEngine;

namespace EpochGame.Progress
{
    [Serializable]
    public sealed class GloryState
    {
        [SerializeField] private long _value;

        public GloryState()
        {
        }

        public GloryState(long value)
        {
            _value = value;
        }

        public long Value => _value;
    }
}
