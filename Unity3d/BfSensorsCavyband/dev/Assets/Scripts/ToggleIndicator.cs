using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleIndicator : MonoBehaviour {

    public Sprite[] BtnImages = new Sprite[2];
    public bool IsOn = false;

    private Image ImageComponent;

	// Use this for initialization
	void Awake () {
		if(ImageComponent == null)
        {
            ImageComponent = GetComponent<Image>();
        }
	}
	
    public void Toggle(bool isOn)
    {
        if (ImageComponent != null)
            ImageComponent.sprite = isOn ? BtnImages[1] : BtnImages[0];
        else
            Debug.LogWarning("ImageComponent is not referenced in ToggleIndicator !");
    }

	// Update is called once per frame
	void Update () {
        //Toggle(IsOn);
	}
}
