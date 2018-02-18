using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using BfSensorsCavyband;

internal class MenuController : MonoBehaviour {

    internal static Action<ECategory, bool> OnRequest;

    public GameObject InfoPanel = null;

    private ToggleButton[] _toggleButtons;


    private void Start()
    {
        _toggleButtons = GetComponentsInChildren<ToggleButton>();

        TestScript.OnInfoReceived       += HandleInfo;
        StepsHandler.OnStepsResultEnded += HandleSyncResult;

        DoRequest(ECategory.System, false);
    }

    private void OnDisable()
    {
        TestScript.OnInfoReceived -= HandleInfo;
        StepsHandler.OnStepsResultEnded -= HandleSyncResult;
    }

    private void HandleInfo(ECategory category, object info)
    {
        string msg = "";

        if (category == ECategory.System)
        {
            var deviceInfo = (DeviceInfo)info;

            msg += "State       : " + deviceInfo.State + "\n";
            msg += "functions   : " + deviceInfo.func + "\n";
            msg += "Time  on ?  : " + deviceInfo.TimeEnabled + "\n";
            msg += "Alarm  on ? : " + deviceInfo.AlarmEnabled + "\n";
            msg += "Lla  on ?   : " + deviceInfo.LlaEnabled + "\n";
            msg += "Tilts on ?  : " + deviceInfo.TiltEnabled + "\n";
            msg += "Steps on ?  : " + deviceInfo.StepsEnabled + "\n";
            msg += "Hardware    : " + deviceInfo.HardwareVersion + "\n";
            msg += "Firmware    : " + deviceInfo.SoftwareVersion + "\n";
            msg += "Calibrated  : " + deviceInfo.IsCalibrated + "\n";
            msg += "Mag X-Offset: " + deviceInfo.MagFactoryOffsetX + "\n";
            msg += "Mag Y-Offset: " + deviceInfo.MagFactoryOffsetY + "\n";
            msg += "Mag Z-Offset: " + deviceInfo.MagFactoryOffsetZ + "\n";
            msg += "Mag Radius  : " + deviceInfo.MagFactoryOffsetRadius + "\n";

            SetToggleSelectedState("Steps Mode", deviceInfo.StepsEnabled);

            TestScript.DeviceInfo = deviceInfo;

            DisplayInfo(msg);
        }

        if (category == ECategory.Time)
        {
            var deviceInfo  = (BfSensorsCavyband.TimeInfo)info;
            int hours       = (int)(deviceInfo.data1 << 8 | deviceInfo.data2) / 60;
            int minutes     = (int)(deviceInfo.data1 << 8 | deviceInfo.data2) % 60;

            msg = (BfSensorsCavyband.EDay)deviceInfo.Weekday + "\n";
            msg += String.Format("{0:00}:{1:00}", hours, minutes);

            TestScript.TimeInfo = deviceInfo;

            DisplayInfo(msg);
        }

    }

    private void HandleSyncResult(List<StepsData> stepsData)
    {
        Debug.Log("<b>MenuController</b> HandleSyncResult " + stepsData.Count);
        string msg = "";

        int len = stepsData.Count;

        for (int i = 0; i < len; i++)
        {
            msg += String.Format("{0:f}, Steps : {1} \n", stepsData[i].Time, stepsData[i].Count);
        }

        DisplayInfo(msg);
    }


    public static void DoRequest(ECategory category, bool arg = false)
    {
        if (OnRequest != null) {
            OnRequest(category, arg);
        }
    }

    public void SetGameMode()
    {
        bool arg = GetToggleSelectedState("Game Mode");
        //Debug.Log("SetGameMode " + arg);

        DoRequest(ECategory.GameMode, arg);
    }

    public void GetSystemInfo()
    {
        DoRequest(ECategory.System);
    }

    public void GetTimeInfo()
    {
        DoRequest(ECategory.Time);
    }

    public void DoVibration()
    {
        DoRequest(ECategory.Vibration);
    }

    public void SetStepsMode()
    {
        bool arg = GetToggleSelectedState("Steps Mode");
        //Debug.Log("SetStepsMode " + arg);

        DoRequest(ECategory.StepsMode, arg);
    }

    public void GetStepMode()
    {
        DoRequest(ECategory.StepsResult);
    }

    public void Quit()
    {
        Application.Quit();
    }

    private bool GetToggleSelectedState(string buttonLabel)
    {
        int len = _toggleButtons.Length;
        for(int i = 0; i < len; i++)
        {
            if (_toggleButtons[i].Label.ToUpper().IndexOf(buttonLabel.ToUpper()) == 0){
                return !_toggleButtons[i].IsSelected;
            }
        }

        Debug.LogWarning("Unable to find " + buttonLabel + " button in MenuController._toggleButtons");
        return false;
    }

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

    private void DisplayInfo(string message)
    {
        InfoPanel.GetComponentInChildren<Text>().text = message;
        InfoPanel.SetActive(true);
    }
}
