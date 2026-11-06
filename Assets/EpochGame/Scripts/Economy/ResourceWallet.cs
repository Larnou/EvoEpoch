using System;
using System.Collections.Generic;

namespace EpochGame.Economy
{
    public sealed class ResourceWallet
    {
        private readonly List<ResourceAmountState> _amounts;
        private readonly Dictionary<ResourceId, ResourceAmountState> _index;

        public ResourceWallet(List<ResourceAmountState> amounts)
        {
            _amounts = amounts ?? throw new ArgumentNullException(nameof(amounts));
            _index = new Dictionary<ResourceId, ResourceAmountState>(_amounts.Count);

            for (var i = 0; i < _amounts.Count; i++)
            {
                var state = _amounts[i];
                if (state == null || _index.ContainsKey(state.Resource))
                {
                    continue;
                }

                _index.Add(state.Resource, state);
            }
        }

        public event Action<ResourceId, double> Changed;

        public double Get(ResourceId resource)
        {
            return _index.TryGetValue(resource, out var state) ? state.Amount : 0;
        }

        public void Add(ResourceId resource, double amount)
        {
            if (amount <= 0)
            {
                return;
            }

            var state = GetOrCreate(resource);
            state.Amount += amount;
            Changed?.Invoke(resource, state.Amount);
        }

        public bool Has(ResourceId resource, double amount)
        {
            return amount >= 0 && Get(resource) >= amount;
        }

        public bool CanAfford(IReadOnlyList<ResourceCost> costs)
        {
            return TryGetSpendTotals(costs, out _);
        }

        public bool TrySpend(IReadOnlyList<ResourceCost> costs)
        {
            if (!TryGetSpendTotals(costs, out var totals))
            {
                return false;
            }

            foreach (var pair in totals)
            {
                var state = _index[pair.Key];
                state.Amount -= pair.Value;
                Changed?.Invoke(pair.Key, state.Amount);
            }

            return true;
        }

        private bool TryGetSpendTotals(
            IReadOnlyList<ResourceCost> costs,
            out Dictionary<ResourceId, double> totals)
        {
            totals = new Dictionary<ResourceId, double>();
            if (costs == null || costs.Count == 0)
            {
                return true;
            }

            for (var i = 0; i < costs.Count; i++)
            {
                var cost = costs[i];
                if (cost == null || cost.Amount < 0)
                {
                    totals = null;
                    return false;
                }

                if (cost.Amount == 0)
                {
                    continue;
                }

                totals.TryGetValue(cost.Resource, out var current);
                totals[cost.Resource] = current + cost.Amount;
            }

            foreach (var pair in totals)
            {
                if (Get(pair.Key) < pair.Value)
                {
                    totals = null;
                    return false;
                }
            }

            return true;
        }

        private ResourceAmountState GetOrCreate(ResourceId resource)
        {
            if (_index.TryGetValue(resource, out var state))
            {
                return state;
            }

            state = new ResourceAmountState(resource, 0);
            _amounts.Add(state);
            _index.Add(resource, state);
            return state;
        }
    }
}
