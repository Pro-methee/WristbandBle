using UnityEngine;
using UnityEngine.UI;

namespace BfWristband.Api
{
    /// <summary>
    /// A script to make a 2-States button that changes its color and label depending of the state
    /// </summary>
    public class ToggleButton : Button
    {
        [Tooltip("Override the button label")]
        public string Label     = "ButtonName";

        [Tooltip("On/Off state of the button")]
        public bool IsSelected  = false;

        public Color SelectedColor   = Color.green;
        public Color DeselectedColor = Color.yellow;

        private Text _labelComponent;
        private string _labelSuffix;

        // Use this for initialization
        protected override void Awake()
        {
            base.Awake();

            _labelComponent = GetComponentInChildren<Text>();

            UpdateButtonGraphics();

            onClick.AddListener(HandleClick);
        }

        public void HandleClick()
        {
            IsSelected = !IsSelected;

            UpdateButtonGraphics();
        }

        public void ForceButtonState(bool newState)
        {
            IsSelected = newState;
            UpdateButtonGraphics();
        }

        private void UpdateButtonGraphics()
        {
            if (_labelComponent == null)
            {
                Debug.LogWarning("No reference found in <b>ToggleButton</b> for _labelComponent !");
            }

            _labelSuffix = IsSelected ? " (On)" : " (Off)";
            _labelComponent.text = Label + _labelSuffix;

            var myColors = colors;
            myColors.highlightedColor = IsSelected ? SelectedColor : DeselectedColor;
            myColors.normalColor = myColors.highlightedColor;
            colors = myColors;
        }
    }
}

