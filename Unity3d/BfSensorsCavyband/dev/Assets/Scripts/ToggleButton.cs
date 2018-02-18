using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleButton : Button {

    public string   Label       = "ButtonName";
    public bool     IsSelected  = false;

    public Color SelectedColor   = Color.green;
    public Color DeselectedColor = Color.yellow;

    private Text _labelComponent;
    private string _labelSuffix;

	// Use this for initialization
	protected override void Awake () {

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

    public void ForceButtonState(bool newState) {
        IsSelected = newState;
        UpdateButtonGraphics();
    }

    private void UpdateButtonGraphics()
    {
        if(_labelComponent == null)
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
