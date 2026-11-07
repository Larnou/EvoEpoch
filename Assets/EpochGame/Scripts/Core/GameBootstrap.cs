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
        [SerializeField] private ProgressDebugView _progressDebug;
        [SerializeField] private TechnologyPanelView _technologyPanel;
        [SerializeField] private ManualResourceNodeView[] _nodes;

        private readonly List<ResourceAmountState> _amounts = new List<ResourceAmountState>();
        private readonly PopulationState _population = new PopulationState(3);
        private readonly TerritoryState _territory = new TerritoryState(1);
        private readonly PlayerLevelState _level = new PlayerLevelState(1);
        private readonly GloryState _glory = new GloryState(10);
        private readonly CurrencyState _currency = new CurrencyState(500);
        private ResourceWallet _wallet;
        private PlayerLevelService _levels;

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

            _levels = new PlayerLevelService(_database.PlayerLevels);
            if (_progressDebug != null)
            {
                _progressDebug.Initialize();
                _progressDebug.PopulationChanged += OnPopulationChanged;
                _progressDebug.TerritoryChanged += OnTerritoryChanged;
            }

            RefreshHeader();

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

        private void OnDestroy()
        {
            if (_progressDebug == null)
            {
                return;
            }

            _progressDebug.PopulationChanged -= OnPopulationChanged;
            _progressDebug.TerritoryChanged -= OnTerritoryChanged;
        }

        private void OnPopulationChanged(long amount)
        {
            _population.Add(amount);
            RefreshHeader();
        }

        private void OnTerritoryChanged(long amount)
        {
            _territory.Add(amount);
            RefreshHeader();
        }

        private void RefreshHeader()
        {
            if (_header == null || _levels == null)
            {
                return;
            }

            var era = _database != null ? _database.StartingEra : null;
            _header.Show(
                era != null ? era.Title : string.Empty,
                _levels.Evaluate(_level.Level, _population.CurrentPopulation, _territory.Area),
                _glory.Value,
                _currency.Crowns);
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
