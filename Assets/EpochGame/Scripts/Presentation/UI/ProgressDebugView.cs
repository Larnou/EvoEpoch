using System;
using UnityEngine;
using UnityEngine.UI;

namespace EpochGame.Presentation
{
    public sealed class ProgressDebugView : MonoBehaviour
    {
        [SerializeField] private Button _populationDecrease;
        [SerializeField] private Button _populationIncrease;
        [SerializeField] private Button _territoryDecrease;
        [SerializeField] private Button _territoryIncrease;

        public event Action<long> PopulationChanged;

        public event Action<long> TerritoryChanged;

        public void Initialize()
        {
            Bind(_populationDecrease, () => PopulationChanged?.Invoke(-1));
            Bind(_populationIncrease, () => PopulationChanged?.Invoke(1));
            Bind(_territoryDecrease, () => TerritoryChanged?.Invoke(-1));
            Bind(_territoryIncrease, () => TerritoryChanged?.Invoke(1));
        }

        private void OnDestroy()
        {
            Unbind(_populationDecrease);
            Unbind(_populationIncrease);
            Unbind(_territoryDecrease);
            Unbind(_territoryIncrease);
        }

        private static void Bind(Button button, Action action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action());
        }

        private static void Unbind(Button button)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }
}
