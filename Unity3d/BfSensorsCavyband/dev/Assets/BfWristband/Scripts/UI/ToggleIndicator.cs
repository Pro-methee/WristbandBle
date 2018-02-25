using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BfWristband.Api
{
    /// <summary>
    /// A simple script to make a graphic toggle that changes its state and makes the wristband vibrate when a threshold is exceeded
    /// </summary>
    public class ToggleIndicator : MonoBehaviour
    {

        public Sprite[] BtnImages = new Sprite[2];
        public bool IsOn = false;

        private Image ImageComponent;

        // Use this for initialization
        void Awake()
        {
            if (ImageComponent == null)
            {
                ImageComponent = GetComponent<Image>();
            }

        }

        public void Toggle(bool isOn)
        {
            if (ImageComponent != null)
            {
                ImageComponent.sprite = isOn ? BtnImages[1] : BtnImages[0];

                if (isOn && EventsHandler.OnVibrationRequested != null)
                    EventsHandler.OnVibrationRequested("id");
            }
            else
                Debug.LogWarning("ImageComponent is not referenced in ToggleIndicator !");

        }

    }

}
