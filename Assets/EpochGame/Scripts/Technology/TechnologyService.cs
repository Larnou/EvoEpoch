using System;
using System.Collections.Generic;
using EpochGame.Economy;

namespace EpochGame.Technology
{
    public sealed class TechnologyService
    {
        private readonly TechnologyState _state;
        private readonly ResourceWallet _wallet;

        public TechnologyService(TechnologyState state, ResourceWallet wallet)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        }

        public event Action<string> Researched;

        public bool IsResearched(string technologyId)
        {
            return _state.IsResearched(technologyId);
        }

        public int CountResearched(IReadOnlyList<string> technologyIds)
        {
            if (technologyIds == null)
            {
                return 0;
            }

            var researched = 0;
            for (var i = 0; i < technologyIds.Count; i++)
            {
                if (IsResearched(technologyIds[i]))
                {
                    researched++;
                }
            }

            return researched;
        }

        public bool CanResearch(TechnologyDefinition technology)
        {
            return GetAvailability(technology) == TechnologyAvailability.Available;
        }

        public TechnologyAvailability GetAvailability(TechnologyDefinition technology)
        {
            if (technology == null || string.IsNullOrEmpty(technology.Id))
            {
                return TechnologyAvailability.Locked;
            }

            if (_state.IsResearched(technology.Id))
            {
                return TechnologyAvailability.Completed;
            }

            if (!ArePrerequisitesResearched(technology))
            {
                return TechnologyAvailability.Locked;
            }

            if (!_wallet.CanAfford(technology.Costs))
            {
                return TechnologyAvailability.TooExpensive;
            }

            return TechnologyAvailability.Available;
        }

        public bool TryResearch(TechnologyDefinition technology)
        {
            if (!CanResearch(technology))
            {
                return false;
            }

            if (!_wallet.TrySpend(technology.Costs))
            {
                return false;
            }

            _state.Add(technology.Id);
            Researched?.Invoke(technology.Id);
            return true;
        }

        private bool ArePrerequisitesResearched(TechnologyDefinition technology)
        {
            var prerequisites = technology.PrerequisiteIds;
            if (prerequisites == null)
            {
                return true;
            }

            for (var i = 0; i < prerequisites.Length; i++)
            {
                if (!_state.IsResearched(prerequisites[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
