using EpochGame.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace EpochGame.Presentation
{
    public sealed class ResourceBarView : MonoBehaviour
    {
        [SerializeField] private ResourceSlotView[] _slots;
        [SerializeField] private Button _toggle;

        private ResourceWallet _wallet;

        public void Initialize(ResourceWallet wallet)
        {
            if (_wallet != null)
            {
                _wallet.Changed -= OnResourceChanged;
            }

            if (_toggle != null)
            {
                _toggle.onClick.RemoveListener(Toggle);
            }

            _wallet = wallet;
            if (_wallet == null)
            {
                Debug.LogError("Resource bar has no wallet.", this);
                return;
            }

            _wallet.Changed += OnResourceChanged;
            if (_toggle != null)
            {
                _toggle.onClick.AddListener(Toggle);
            }

            RefreshAll();
            gameObject.SetActive(false);
        }

        private void Toggle()
        {
            var open = !gameObject.activeSelf;
            gameObject.SetActive(open);
            if (!open)
            {
                return;
            }

            transform.SetAsLastSibling();
            RefreshAll();
        }

        public void SetVisible(ResourceId resource, bool visible)
        {
            if (_slots == null)
            {
                return;
            }

            for (var i = 0; i < _slots.Length; i++)
            {
                var slot = _slots[i];
                if (slot == null || slot.Resource != resource)
                {
                    continue;
                }

                slot.gameObject.SetActive(visible);
                if (visible && _wallet != null)
                {
                    slot.SetAmount(_wallet.Get(resource));
                }
            }
        }

        private void OnDestroy()
        {
            if (_wallet != null)
            {
                _wallet.Changed -= OnResourceChanged;
            }

            if (_toggle != null)
            {
                _toggle.onClick.RemoveListener(Toggle);
            }
        }

        private void OnResourceChanged(ResourceId resource, double amount)
        {
            if (_slots == null)
            {
                return;
            }

            for (var i = 0; i < _slots.Length; i++)
            {
                var slot = _slots[i];
                if (slot != null && slot.Resource == resource)
                {
                    slot.SetAmount(amount);
                }
            }
        }

        private void RefreshAll()
        {
            if (_slots == null)
            {
                return;
            }

            for (var i = 0; i < _slots.Length; i++)
            {
                var slot = _slots[i];
                if (slot != null)
                {
                    slot.SetAmount(_wallet.Get(slot.Resource));
                }
            }
        }
    }
}
