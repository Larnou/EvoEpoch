using System;
using UnityEngine;

namespace EpochGame.Progress
{
    [Serializable]
    public sealed class PlayerLevelState
    {
        [SerializeField] private int _level = 1;

        public PlayerLevelState()
        {
        }

        public PlayerLevelState(int level)
        {
            _level = level;
        }

        public int Level => _level;
    }
}
