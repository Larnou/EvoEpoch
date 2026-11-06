using System;
using System.Globalization;
using EpochGame.Economy;
using TMPro;
using UnityEngine;

namespace EpochGame.Presentation
{
    public sealed class ResourceSlotView : MonoBehaviour
    {
        [SerializeField] private ResourceId _resource;
        [SerializeField] private string _title;
        [SerializeField] private TMP_Text _amountLabel;

        public ResourceId Resource => _resource;

        public void SetAmount(double amount)
        {
            if (_amountLabel == null)
            {
                Debug.LogError("Resource slot has no amount label.", this);
                return;
            }

            var value = Format(amount);
            _amountLabel.text = string.IsNullOrEmpty(_title) ? value : _title + " " + value;
        }

        private static string Format(double amount)
        {
            if (amount == Math.Truncate(amount))
            {
                return amount.ToString("0", CultureInfo.InvariantCulture);
            }

            return amount.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
