using EpochGame.Economy;
using UnityEngine;

namespace EpochGame.Technology
{
    [CreateAssetMenu(fileName = "Technology", menuName = "EpochGame/Technology")]
    public sealed class TechnologyDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _title;
        [SerializeField] private string _eraId;
        [SerializeField] private string[] _prerequisiteIds;
        [SerializeField] private ResourceCost[] _costs;
        [SerializeField] private string[] _unlockBuildingIds;
        [SerializeField] private string[] _unlockManualResourceNodeIds;

        public string Id => _id;

        public string Title => _title;

        public string EraId => _eraId;

        public string[] PrerequisiteIds => _prerequisiteIds;

        public ResourceCost[] Costs => _costs;

        public string[] UnlockBuildingIds => _unlockBuildingIds;

        public string[] UnlockManualResourceNodeIds => _unlockManualResourceNodeIds;
    }
}
