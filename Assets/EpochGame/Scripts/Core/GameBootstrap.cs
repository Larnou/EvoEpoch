using System.Collections.Generic;
using EpochGame.Economy;
using EpochGame.Gathering;
using EpochGame.Presentation;
using UnityEngine;

namespace EpochGame.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private ResourceBarView _resourceBar;
        [SerializeField] private ManualResourceNodeView[] _nodes;

        private readonly List<ResourceAmountState> _amounts = new List<ResourceAmountState>();
        private ResourceWallet _wallet;

        private void Awake()
        {
            _amounts.Add(new ResourceAmountState(ResourceId.Food, 0));
            _amounts.Add(new ResourceAmountState(ResourceId.Wood, 0));

            _wallet = new ResourceWallet(_amounts);
            var gathering = new ManualGatheringService(_wallet, _ => false);

            if (_resourceBar == null)
            {
                Debug.LogError("Game bootstrap has no resource bar.", this);
            }
            else
            {
                _resourceBar.Initialize(_wallet);
            }

            if (_nodes == null)
            {
                return;
            }

            for (var i = 0; i < _nodes.Length; i++)
            {
                if (_nodes[i] != null)
                {
                    _nodes[i].Initialize(gathering);
                }
            }
        }
    }
}
