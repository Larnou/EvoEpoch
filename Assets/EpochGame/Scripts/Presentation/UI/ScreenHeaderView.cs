using System;
using EpochGame.Progress;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EpochGame.Presentation
{
    public sealed class ScreenHeaderView : MonoBehaviour
    {
        [SerializeField] private Image _portrait;
        [SerializeField] private TMP_Text _levelLabel;
        [SerializeField] private TMP_Text _eraLabel;
        [SerializeField] private TMP_Text _populationValue;
        [SerializeField] private TMP_Text _populationCaption;
        [SerializeField] private Image _populationFill;
        [SerializeField] private TMP_Text _territoryValue;
        [SerializeField] private TMP_Text _territoryCaption;
        [SerializeField] private Image _territoryFill;
        [SerializeField] private TMP_Text _gloryValue;
        [SerializeField] private TMP_Text _crownValue;

        private void Awake()
        {
            ResolveLabels();
        }

        public void Show(string eraTitle, PlayerLevelProgress progress, long glory, long crowns)
        {
            ResolveLabels();
            if (_eraLabel != null)
            {
                _eraLabel.text = eraTitle;
            }

            if (_levelLabel != null)
            {
                _levelLabel.text = "Ур. " + progress.Level.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            SetStat(_populationValue, _populationCaption, progress.Population, "человек", "человека", "человек");
            SetFill(_populationFill, progress.PopulationFill);
            SetStat(_territoryValue, _territoryCaption, progress.Territory, "акр", "акра", "акров");

            SetFill(_territoryFill, progress.TerritoryFill);

            if (_gloryValue != null)
            {
                _gloryValue.text = glory.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            if (_crownValue != null)
            {
                _crownValue.text = crowns.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void ResolveLabels()
        {
            _populationValue = FindValue(_populationValue, "PopulationLabel", "Population");
            _populationCaption = FindCaption(_populationCaption, _populationValue);
            _territoryValue = FindValue(_territoryValue, "TerritoryLabel", "Territory");
            _territoryCaption = FindCaption(_territoryCaption, _territoryValue);
        }

        private TMP_Text FindValue(TMP_Text current, string legacyName, string groupName)
        {
            if (current != null)
            {
                return current;
            }

            var texts = GetComponentsInChildren<TMP_Text>(true);
            for (var i = 0; i < texts.Length; i++)
            {
                var text = texts[i];
                var parent = text.transform.parent;
                if (text.gameObject.name == "Value" && parent != null && (parent.name == legacyName || parent.name == groupName))
                {
                    return text;
                }
            }

            for (var i = 0; i < texts.Length; i++)
            {
                var text = texts[i];
                if (text.gameObject.name == legacyName || text.gameObject.name == groupName)
                {
                    return text;
                }
            }

            return null;
        }

        private static TMP_Text FindCaption(TMP_Text current, TMP_Text value)
        {
            if (current != null || value == null)
            {
                return current;
            }

            var caption = value.transform.parent != null ? value.transform.parent.Find("Caption") : null;
            return caption != null ? caption.GetComponent<TMP_Text>() : null;
        }

        private void SetStat(TMP_Text value, TMP_Text caption, long amount, string one, string few, string many)
        {
            if (value == null)
            {
                return;
            }

            var number = amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var word = Plural(amount, one, few, many);
            if (caption != null)
            {
                value.text = number;
                caption.text = word;
                return;
            }

            value.text = number + " " + word;
        }

        private static void SetFill(Image fill, float amount)
        {
            if (fill == null)
            {
                return;
            }

            var ratio = amount < 0f ? 0f : amount > 1f ? 1f : amount;
            fill.type = Image.Type.Simple;
            var rect = fill.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(ratio, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
