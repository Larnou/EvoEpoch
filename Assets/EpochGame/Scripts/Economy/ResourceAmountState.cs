using System;
using UnityEngine;

namespace EpochGame.Economy
{
    [Serializable]
    public sealed class ResourceAmountState
    {
        [SerializeField] private ResourceId _resource;
        [SerializeField] private double _amount;

        public ResourceAmountState()
        {
        }

        public ResourceAmountState(ResourceId resource, double amount)
        {
            _resource = resource;
            _amount = amount;
        }

        public ResourceId Resource => _resource;

        public double Amount
        {
            get => _amount;
            internal set => _amount = value;
        }
    }
}
