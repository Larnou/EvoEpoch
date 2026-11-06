using System.Collections.Generic;
using EpochGame.Era;
using EpochGame.Gathering;
using EpochGame.Technology;
using UnityEngine;

namespace EpochGame.Database
{
    [CreateAssetMenu(fileName = "GameDatabase", menuName = "EpochGame/Game Database")]
    public sealed class GameDatabase : ScriptableObject
    {
        [SerializeField] private EraDefinition[] _eras;
        [SerializeField] private TechnologyDefinition[] _technologies;
        [SerializeField] private ManualResourceNodeDefinition[] _manualNodes;

        private Dictionary<string, EraDefinition> _erasById;
        private Dictionary<string, TechnologyDefinition> _technologiesById;
        private Dictionary<string, ManualResourceNodeDefinition> _nodesById;

        public IReadOnlyList<TechnologyDefinition> Technologies => _technologies;

        public void BuildIndex()
        {
            _erasById = Index(_eras, era => era.Id);
            _technologiesById = Index(_technologies, technology => technology.Id);
            _nodesById = Index(_manualNodes, node => node.Id);
        }

        public EraDefinition StartingEra
        {
            get
            {
                EraDefinition starting = null;
                if (_eras == null)
                {
                    return null;
                }

                for (var i = 0; i < _eras.Length; i++)
                {
                    var era = _eras[i];
                    if (era == null)
                    {
                        continue;
                    }

                    if (starting == null || era.Order < starting.Order)
                    {
                        starting = era;
                    }
                }

                return starting;
            }
        }

        public TechnologyDefinition GetTechnology(string id)
        {
            if (string.IsNullOrEmpty(id) || _technologiesById == null)
            {
                return null;
            }

            _technologiesById.TryGetValue(id, out var technology);
            return technology;
        }

        public ManualResourceNodeDefinition GetManualNode(string id)
        {
            if (string.IsNullOrEmpty(id) || _nodesById == null)
            {
                return null;
            }

            _nodesById.TryGetValue(id, out var node);
            return node;
        }

        private static Dictionary<string, T> Index<T>(T[] items, System.Func<T, string> id)
            where T : Object
        {
            var index = new Dictionary<string, T>();
            if (items == null)
            {
                return index;
            }

            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item == null)
                {
                    continue;
                }

                var key = id(item);
                if (!string.IsNullOrEmpty(key) && !index.ContainsKey(key))
                {
                    index.Add(key, item);
                }
            }

            return index;
        }
    }
}
