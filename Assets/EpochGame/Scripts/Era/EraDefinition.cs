using UnityEngine;

namespace EpochGame.Era
{
    [CreateAssetMenu(fileName = "Era", menuName = "EpochGame/Era")]
    public sealed class EraDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _title;
        [SerializeField] private int _order;
        [SerializeField] private string[] _technologyIds;

        public string Id => _id;

        public string Title => _title;

        public int Order => _order;

        public string[] TechnologyIds => _technologyIds;
    }
}
