using System;
using EpochGame.Economy;

namespace EpochGame.Gathering
{
    public sealed class ManualGatheringService
    {
        private readonly ResourceWallet _wallet;
        private readonly Func<string, bool> _isTechnologyResearched;

        public ManualGatheringService(
            ResourceWallet wallet,
            Func<string, bool> isTechnologyResearched)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _isTechnologyResearched = isTechnologyResearched;
        }

        public bool IsUnlocked(ManualResourceNodeDefinition node)
        {
            if (node == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(node.RequiredTechnologyId))
            {
                return true;
            }

            return _isTechnologyResearched != null
                && _isTechnologyResearched(node.RequiredTechnologyId);
        }

        public bool TryGather(ManualResourceNodeDefinition node)
        {
            if (!IsUnlocked(node) || node.AmountPerTap <= 0)
            {
                return false;
            }

            _wallet.Add(node.Resource, node.AmountPerTap);
            return true;
        }
    }
}
