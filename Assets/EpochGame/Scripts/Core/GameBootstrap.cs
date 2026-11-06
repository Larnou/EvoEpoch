using System.Collections.Generic;
using EpochGame.Database;
using EpochGame.Economy;
using EpochGame.Gathering;
using EpochGame.Population;
using EpochGame.Presentation;
using EpochGame.Progress;
using EpochGame.Technology;
using UnityEngine;

namespace EpochGame.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GameDatabase _database;
        [SerializeField] private ResourceBarView _resourceBar;
        [SerializeField] private ScreenHeaderView _header;
        [SerializeField] private TechnologyPanelView _technologyPanel;
        [SerializeField] private ManualResourceNodeView[] _nodes;

        private readonly List<ResourceAmountState> _amounts = new List<ResourceAmountState>();
        private readonly PopulationState _population = new PopulationState(3);
        private readonly TerritoryState _territory = new TerritoryState(1);
        private readonly GloryState _glory = new GloryState(10);
        private readonly CurrencyState _currency = new CurrencyState(500);
        private ResourceWallet _wallet;

        private void Awake()
        {
            _amounts.Add(new ResourceAmountState(ResourceId.Food, 0));
            _amounts.Add(new ResourceAmountState(ResourceId.Wood, 0));
            _wallet = new ResourceWallet(_amounts);

            if (_database == null)
            {
                Debug.LogError("Game bootstrap has no database.", this);
                return;
            }

            _database.BuildIndex();
            var technologies = new TechnologyState();
            var technologyService = new TechnologyService(technologies, _wallet);
            var gathering = new ManualGatheringService(_wallet, technologyService.IsResearched);

            if (_resourceBar != null)
            {
                _resourceBar.Initialize(_wallet);
            }

            if (_header != null)
            {
                var era = _database.StartingEra;
                _header.Show(
                    era != null ? era.Title : string.Empty,
                    _population.CurrentPopulation,
                    _territory.Area,
                    _glory.Value,
                    _currency.Crowns);
            }

            if (_technologyPanel != null)
            {
                _technologyPanel.Initialize(technologyService, _database, _wallet);
            }

            if (_nodes != null)
            {
                for (var i = 0; i < _nodes.Length; i++)
                {
                    if (_nodes[i] != null)
                    {
                        _nodes[i].Initialize(gathering);
                    }
                }
            }

            technologyService.Researched += _ => RefreshUnlocks();
            RefreshUnlocks();
        }

        private void RefreshUnlocks()
        {
            var stoneVisible = false;
            if (_nodes != null)
            {
                for (var i = 0; i < _nodes.Length; i++)
                {
                    var node = _nodes[i];
                    if (node == null)
                    {
                        continue;
                    }

                    node.Refresh();
                    if (node.Resource == ResourceId.Stone && node.IsShown)
                    {
                        stoneVisible = true;
                    }
                }
            }

            if (_resourceBar != null)
            {
                _resourceBar.SetVisible(ResourceId.Stone, stoneVisible);
            }
        }
    }
}
