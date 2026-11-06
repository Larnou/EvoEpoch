using EpochGame.Economy;
using UnityEngine;

namespace EpochGame.Gathering
{
    [CreateAssetMenu(
        fileName = "ManualResourceNode",
        menuName = "EpochGame/Manual Resource Node")]
    public sealed class ManualResourceNodeDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private ResourceId _resource;
        [SerializeField] private double _amountPerTap;
        [SerializeField] private string _requiredTechnologyId;

        public string Id => _id;

        public ResourceId Resource => _resource;

        public double AmountPerTap => _amountPerTap;

        public string RequiredTechnologyId => _requiredTechnologyId;
    }
}
