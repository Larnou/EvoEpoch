using System;
using UnityEngine;

namespace EpochGame.Economy
{
    [Serializable]
    public sealed class ResourceCost
    {
        [SerializeField] private ResourceId _resource;
        [SerializeField] private double _amount;

        public ResourceCost()
        {
        }

        public ResourceCost(ResourceId resource, double amount)
        {
            _resource = resource;
            _amount = amount;
        }

        public ResourceId Resource => _resource;

        public double Amount => _amount;
    }
}
