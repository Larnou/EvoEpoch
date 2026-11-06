using EpochGame.Economy;
using EpochGame.Gathering;
using UnityEngine;
using UnityEngine.UI;

namespace EpochGame.Presentation
{
    public sealed class ManualResourceNodeView : MonoBehaviour
    {
        [SerializeField] private ManualResourceNodeDefinition _node;
        [SerializeField] private Button _button;

        private ManualGatheringService _gathering;

        public ResourceId Resource => _node != null ? _node.Resource : ResourceId.Food;

        public bool IsShown { get; private set; }

        public void Initialize(ManualGatheringService gathering)
        {
            _gathering = gathering;
            if (_button == null)
            {
                Debug.LogError("Resource node has no button.", this);
                return;
            }

            _button.onClick.RemoveListener(OnClick);
            _button.onClick.AddListener(OnClick);
            Refresh();
        }

        public void Refresh()
        {
            IsShown = _gathering != null && _gathering.IsUnlocked(_node);
            gameObject.SetActive(IsShown);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClick);
            }
        }

        private void OnClick()
        {
            if (_gathering == null || _node == null)
            {
                Debug.LogError("Resource node is not wired.", this);
                return;
            }

            _gathering.TryGather(_node);
        }
    }
}
