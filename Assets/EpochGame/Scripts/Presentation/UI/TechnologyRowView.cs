using EpochGame.Database;
using EpochGame.Economy;
using EpochGame.Technology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EpochGame.Presentation
{
    public sealed class TechnologyRowView : MonoBehaviour
    {
        [SerializeField] private TechnologyDefinition _technology;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;

        private TechnologyService _service;
        private GameDatabase _database;

        public TechnologyDefinition Technology => _technology;

        public void Initialize(TechnologyService service, GameDatabase database)
        {
            _service = service;
            _database = database;
            if (_button == null)
            {
                Debug.LogError("Technology row has no button.", this);
                return;
            }

            _button.onClick.RemoveListener(OnClick);
            _button.onClick.AddListener(OnClick);
            Refresh();
        }

        public void Refresh()
        {
            if (_technology == null || _service == null || _label == null || _button == null)
            {
                return;
            }

            var availability = _service.GetAvailability(_technology);
            _button.interactable = availability == TechnologyAvailability.Available;
            _label.text = _technology.Title + "\n" + Describe(availability);
        }

        private void OnClick()
        {
            if (_service != null && _service.TryResearch(_technology))
            {
                Refresh();
            }
        }

        private string Describe(TechnologyAvailability availability)
        {
            if (availability == TechnologyAvailability.Completed)
            {
                return "Изучено";
            }

            if (availability == TechnologyAvailability.Locked)
            {
                return "Нужно: " + PrerequisiteTitle();
            }

            return FormatCosts(_technology.Costs);
        }

        private string PrerequisiteTitle()
        {
            var prerequisites = _technology.PrerequisiteIds;
            if (prerequisites == null || prerequisites.Length == 0 || _database == null)
            {
                return "предыдущее исследование";
            }

            var prerequisite = _database.GetTechnology(prerequisites[0]);
            return prerequisite != null ? prerequisite.Title : prerequisites[0];
        }

        private static string FormatCosts(ResourceCost[] costs)
        {
            if (costs == null || costs.Length == 0)
            {
                return "Бесплатно";
            }

            var text = string.Empty;
            for (var i = 0; i < costs.Length; i++)
            {
                var cost = costs[i];
                if (cost == null)
                {
                    continue;
                }

                if (text.Length > 0)
                {
                    text += ", ";
                }

                text += FormatAmount(cost.Amount) + " " + ResourceName(cost.Resource);
            }

            return text;
        }

        private static string FormatAmount(double amount)
        {
            return amount == System.Math.Truncate(amount)
                ? amount.ToString("0", System.Globalization.CultureInfo.InvariantCulture)
                : amount.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string ResourceName(ResourceId resource)
        {
            switch (resource)
            {
                case ResourceId.Food:
                    return "еды";
                case ResourceId.Wood:
                    return "дерева";
                case ResourceId.Stone:
                    return "камня";
                default:
                    return resource.ToString();
            }
        }
    }
}
