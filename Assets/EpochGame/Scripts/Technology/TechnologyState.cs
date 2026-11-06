using System;
using System.Collections.Generic;
using UnityEngine;

namespace EpochGame.Technology
{
    [Serializable]
    public sealed class TechnologyState
    {
        [SerializeField] private List<string> _researchedIds = new List<string>();

        public bool IsResearched(string technologyId)
        {
            return !string.IsNullOrEmpty(technologyId) && _researchedIds.Contains(technologyId);
        }

        internal void Add(string technologyId)
        {
            if (!IsResearched(technologyId))
            {
                _researchedIds.Add(technologyId);
            }
        }
    }
}
