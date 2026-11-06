using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EpochGame.Presentation
{
    public sealed class ScreenHeaderView : MonoBehaviour
    {
        [SerializeField] private Image _portrait;
        [SerializeField] private TMP_Text _eraLabel;
        [SerializeField] private TMP_Text _populationLabel;
        [SerializeField] private TMP_Text _territoryLabel;
        [SerializeField] private TMP_Text _gloryValue;
        [SerializeField] private TMP_Text _crownValue;

        public void Show(string eraTitle, long population, long territoryArea, long glory, long crowns)
        {
            if (_eraLabel != null)
            {
                _eraLabel.text = eraTitle;
            }

            if (_populationLabel != null)
            {
                _populationLabel.text = population + " " + Plural(population, "человек", "человека", "человек");
            }

            if (_territoryLabel != null)
            {
                _territoryLabel.text = territoryArea + " " + Plural(territoryArea, "акр", "акра", "акров");
            }

            if (_gloryValue != null)
            {
                _gloryValue.text = glory.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            if (_crownValue != null)
            {
                _crownValue.text = crowns.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public void SetPortrait(Sprite portrait)
        {
            if (_portrait != null)
            {
                _portrait.sprite = portrait;
                _portrait.enabled = portrait != null;
            }
        }

        private static string Plural(long value, string one, string few, string many)
        {
            var absolute = Math.Abs(value) % 100;
            var lastDigit = absolute % 10;
            if (absolute > 10 && absolute < 20)
            {
                return many;
            }

            if (lastDigit == 1)
            {
                return one;
            }

            if (lastDigit > 1 && lastDigit < 5)
            {
                return few;
            }

            return many;
        }
    }
}
