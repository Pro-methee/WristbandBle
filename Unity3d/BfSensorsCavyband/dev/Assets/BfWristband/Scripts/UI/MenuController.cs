using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using BfWristband;

namespace BfWristband.Api
{
    internal class MenuController : MonoBehaviour
    {
        internal static Action OnCleanUIRequested;

        [SerializeField]
        private GameObject InfoPanel = null;

        [SerializeField]
        [Tooltip("Objects that only visible in Game Mode")]
        private GameObject[] _gameModeAssets = new GameObject[0];

        private ToggleButton[] _toggleButtons;
        private StepsHandler   _stepsHandler;

        private string _deviceID;

        private void Start()
        {
            // List of buttons that have a 2-States behaviour
            _toggleButtons = GetComponentsInChildren<ToggleButton>();

            if(BleSensorsManager.Instance != null)
                _deviceID = BleSensorsManager.Instance.DefaultId;

            EventsHandler.OnSystemInfoReceived  += HandleSystemInfo;
            EventsHandler.OnTimeInfoReceived    += HandleTimeInfo;

            // StepsHandler is in charge with getting steps count result
            StepsHandler.OnStepsResultEnded += HandleSyncResult;

            if(_stepsHandler == null)
                _stepsHandler = FindObjectOfType<StepsHandler>();

            // Hide all objects that are only visible in Game Mode
            foreach(var go in _gameModeAssets)
            {
                go.SetActive(false);
            }

            // Synchronize System info so that 2-States buttons start with the correct state
            GetSystemInfo();
        }

        private void OnDisable()
        {
            EventsHandler.OnSystemInfoReceived  -= HandleSystemInfo;
            EventsHandler.OnTimeInfoReceived    -= HandleTimeInfo;

            StepsHandler.OnStepsResultEnded -= HandleSyncResult;

            OnCleanUIRequested = null;
        }


        #region Handlers
        /// <summary>
        /// Callback invoked as a result of receiving System Info from the wristband. It displays parsed data as result
        /// </summary>
        /// <param name="id">Not used in this version because only one sensor is managed</param>
        /// <param name="deviceInfo">The structure holding results parsed from the raw data</param>
        private void HandleSystemInfo(string id, DeviceInfo deviceInfo)
        {
            SetToggleSelectedState("Steps Mode", deviceInfo.StepsEnabled);

            DisplayInfo(deviceInfo.ToString());

        }

        /// <summary>
        /// Callback invoked as a result of receiving Time System Info from the wristband. It displays parsed data as result
        /// </summary>
        /// <param name="id">Not used in this version because only one sensor is managed</param>
        /// <param name="timeInfo">The structure holding results parsed from the raw data</param>
        private void HandleTimeInfo(string id, TimeInfo timeInfo)
        {
            DisplayInfo(timeInfo.ToString());
        }

        /// <summary>
        /// Callback invoked as a result of receiving Steps Data from the wristband after a request have been made
        /// </summary>
        /// <param name="stepsData">A list of StepsData that are resulting from parsing the raw data</param>
        private void HandleSyncResult(List<StepsData> stepsData)
        {
            //Debug.Log("<b>MenuController</b> HandleSyncResult " + stepsData.Count);
            string msg = "";

            int len = stepsData.Count;
            for (int i = 0; i < len; i++)
            {
                msg += "\n" + stepsData[i].ToString();
            }
            
            DisplayInfo(msg);
        }
#endregion Handlers


#region Buttons Delegates
        /// <summary>
        /// Make a request to Enters/Exits game mode
        /// </summary>
        public void SetGameMode()
        {
            bool arg = GetToggleSelectedState("Game Mode");

            foreach (var go in _gameModeAssets)
            {
                go.SetActive(arg);
            }

            if (EventsHandler.OnGameModeRequested != null)
                EventsHandler.OnGameModeRequested(_deviceID, arg);
        }

        /// <summary>
        /// Make a request to get System Info from a Wristband device (Functions enabled, HW/FW versions, calibration...)
        /// </summary>
        public void GetSystemInfo()
        {
            if (EventsHandler.OnSystemInfoRequested != null)
                EventsHandler.OnSystemInfoRequested(_deviceID);
        }

        /// <summary>
        /// Make a request to get Time Info from a Wristband device
        /// </summary>
        public void GetTimeInfo()
        {
            if (EventsHandler.OnTimeSystemRequested != null)
                EventsHandler.OnTimeSystemRequested(_deviceID, false);
        }

        /// <summary>
        /// Make a request to a Wristband to make it vibrate and clean Debug UI
        /// </summary>
        public void DoVibration()
        {
            if (OnCleanUIRequested != null)
                OnCleanUIRequested();

            if (EventsHandler.OnVibrationRequested != null)
                EventsHandler.OnVibrationRequested(_deviceID);
        }

        /// <summary>
        /// Make a request to Enters/Exits steps mode
        /// </summary>
        public void SetStepsMode()
        {
            bool arg = GetToggleSelectedState("Steps Mode");

            if (EventsHandler.OnStepsModeRequested != null)
                EventsHandler.OnStepsModeRequested(_deviceID, arg);
        }

        /// <summary>
        /// Quit Application
        /// </summary>
        public void Quit()
        {
            Application.Quit();
        }
#endregion Buttons Delegates


#region UI Behaviour

        /// <summary>
        /// Get the state of a 2-States button (ToggleButton instance) from its label
        /// </summary>
        /// <param name="buttonLabel">The button label we are looking for</param>
        /// <returns>The state of the button (True if On, false if Off)</returns>
        private bool GetToggleSelectedState(string buttonLabel)
        {
            int len = _toggleButtons.Length;
            for (int i = 0; i < len; i++)
            {
                if (_toggleButtons[i].Label.ToUpper().IndexOf(buttonLabel.ToUpper()) == 0)
                {
                    return !_toggleButtons[i].IsSelected;
                }
            }

            Debug.LogWarning("Unable to find " + buttonLabel + " button in MenuController._toggleButtons");
            return false;
        }

        /// <summary>
        /// Set the state of a 2-States button (ToggleButton instance) from its label
        /// </summary>
        /// <param name="buttonLabel">The button label we are looking for</param>
        /// <param name="state">The state we want to achieve</param>
        private void SetToggleSelectedState(string buttonLabel, bool state)
        {
            int len = _toggleButtons.Length;
            for (int i = 0; i < len; i++)
            {
                if (_toggleButtons[i].Label.ToUpper().IndexOf(buttonLabel.ToUpper()) == 0)
                {
                    _toggleButtons[i].ForceButtonState(state);
                    return;
                }
            }

            Debug.LogWarning("Unable to find " + buttonLabel + " button in MenuController._toggleButtons");
        }

        /// <summary>
        /// Display information in a Canvas panel
        /// </summary>
        /// <param name="message">The text we want to display</param>
        private void DisplayInfo(string message)
        {
            InfoPanel.GetComponentInChildren<Text>().text = message;
            InfoPanel.SetActive(true);
        }
#endregion UI Behaviour

    }
}

