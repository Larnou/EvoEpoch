using EpochGame.Economy;
using UnityEngine;

namespace EpochGame.Presentation
{
    public sealed class ResourceBarView : MonoBehaviour
    {
        [SerializeField] private ResourceSlotView[] _slots;

        private ResourceWallet _wallet;

        public void Initialize(ResourceWallet wallet)
        {
            if (_wallet != null)
            {
                _wallet.Changed -= OnResourceChanged;
            }

            _wallet = wallet;
            if (_wallet == null)
            {
                Debug.LogError("Resource bar has no wallet.", this);
                return;
            }

            _wallet.Changed += OnResourceChanged;
            RefreshAll();
        }

        private void OnDestroy()
        {
            if (_wallet != null)
            {
                _wallet.Changed -= OnResourceChanged;
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
