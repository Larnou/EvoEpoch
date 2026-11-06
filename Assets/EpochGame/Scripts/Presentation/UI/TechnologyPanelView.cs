using EpochGame.Database;
using EpochGame.Economy;
using EpochGame.Era;
using EpochGame.Technology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EpochGame.Presentation
{
    public sealed class TechnologyPanelView : MonoBehaviour
    {
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _eraButton;
        [SerializeField] private TMP_Text _eraTitle;
        [SerializeField] private TMP_Text _eraCounter;
        [SerializeField] private TMP_Text _eraChevron;
        [SerializeField] private GameObject _technologyList;
        [SerializeField] private EraDefinition _era;
        [SerializeField] private TechnologyRowView[] _rows;

        private TechnologyService _service;
        private GameDatabase _database;
        private ResourceWallet _wallet;

        public void Initialize(TechnologyService service, GameDatabase database, ResourceWallet wallet)
        {
            if (_service != null)
            {
                _service.Researched -= OnResearched;
            }

            if (_wallet != null)
            {
                _wallet.Changed -= OnResourceChanged;
            }

            _service = service;
            _database = database;
            _wallet = wallet;
            if (_service != null)
            {
                _service.Researched += OnResearched;
            }

            if (_wallet != null)
            {
                _wallet.Changed += OnResourceChanged;
            }

            Bind(_openButton, Open);
            Bind(_closeButton, Close);
            Bind(_eraButton, ToggleEra);
            SetExpanded(false);
            gameObject.SetActive(false);

            if (_rows == null)
            {
                RefreshCounter();
                return;
            }

            for (var i = 0; i < _rows.Length; i++)
            {
                if (_rows[i] != null)
                {
                    _rows[i].Initialize(_service, _database);
                }
            }

            RefreshCounter();
        }

        private void OnDestroy()
        {
            if (_service != null)
            {
                _service.Researched -= OnResearched;
            }

            if (_wallet != null)
            {
                _wallet.Changed -= OnResourceChanged;
            }

            Unbind(_openButton, Open);
            Unbind(_closeButton, Close);
            Unbind(_eraButton, ToggleEra);
        }

        private void Open()
        {
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
            SetExpanded(false);
            Refresh();
        }

        private void Close()
        {
            gameObject.SetActive(false);
        }

        private void ToggleEra()
        {
            var expanded = _technologyList != null && _technologyList.activeSelf;
            SetExpanded(!expanded);
        }

        private void SetExpanded(bool expanded)
        {
            if (_technologyList != null)
            {
                _technologyList.SetActive(expanded);
            }

            if (_eraChevron != null)
            {
                _eraChevron.text = expanded ? "▼" : "▲";
            }
        }

        private void OnResearched(string technologyId)
        {
            Refresh();
        }

        private void OnResourceChanged(ResourceId resource, double amount)
        {
            Refresh();
        }

        private void Refresh()
        {
            RefreshCounter();
            if (_rows == null)
            {
                return;
            }

            for (var i = 0; i < _rows.Length; i++)
            {
                if (_rows[i] != null)
                {
                    _rows[i].Refresh();
                }
            }
        }

        private void RefreshCounter()
        {
            var era = _era != null ? _era : _database != null ? _database.StartingEra : null;
            if (_eraTitle != null && era != null)
            {
                _eraTitle.text = era.Title;
            }

            var ids = era != null ? era.TechnologyIds : null;
            var total = ids != null ? ids.Length : 0;
            var researched = _service != null ? _service.CountResearched(ids) : 0;
            if (_eraCounter != null)
            {
                _eraCounter.text = researched.ToString("00") + "/" + total.ToString("00");
            }
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        private static void Unbind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(action);
            }
        }
    }
}
