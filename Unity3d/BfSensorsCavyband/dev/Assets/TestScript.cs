using UnityEngine;
using UnityEngine.UI;

using BfSensorsCavyband;

public enum ECategory { GameMode, StepsMode, StepsResult, System, Time, Tilts, Vibration }

public class TestScript : MonoBehaviour {

    public enum ESensorState { Idle, Scanning, Standby, GameMode, Disconnected }
    
    public static System.Action<ECategory, object> OnInfoReceived;

    public static string s_Status               = "NO STATUS";
    public static string s_DebugOnScreenMessage = "NO DEBUG MESSAGE";
    public static string s_RawData              = "NO DATA";
    public static int    s_Counter = 0;

    private BleSensorCavybandBase _sensor = null;
    private ESensorState          _sensorState;

    private GUIStyle _gStyle;

    private static DeviceInfo _deviceInfo;
    public static DeviceInfo DeviceInfo {
        get { return _deviceInfo; }
        set { _deviceInfo = value;}
    }

    private static TimeInfo _timeInfo;
    public static TimeInfo TimeInfo {
        get { return _timeInfo; }
        set { _timeInfo = value; }
    }


    void CalculationTest()
    {

        Debug.Log(System.String.Format("({0:f})\n", System.DateTime.Now));

    }

    // Use this for initialization
    void Awake () {

        if(BleSensorsManager.Instance != null)
            BleSensorsManager.Instance.onNewSensorConnected += HandleNewSensorConnected;

        MenuController.OnRequest += HandleRequest;

        _gStyle = new GUIStyle();
        _gStyle.fontStyle = FontStyle.Bold;
        _gStyle.fontSize  = 24;

        DontDestroyOnLoad(gameObject);

        //CalculationTest();
    }
	
    private void SetConnectionStatus(ESensorState newState)
    {
        switch (newState)
        {
            case ESensorState.Disconnected:
                if(_sensorState == ESensorState.Scanning)
                {
                    _sensorState = ESensorState.Disconnected;
                }
                break;

            default:
                _sensorState = newState;
                break;
        }

        s_Status = _sensorState.ToString();
    }

    public void SwitchScanState()
    {
        if (BleSensorsManager.Instance == null) return;

        if (BleSensorsManager.Instance.IsScanningForSensors)
        {
            BleSensorsManager.Instance.StopScanningForSensors();
            SetConnectionStatus(ESensorState.Disconnected);
        }
        else
        {
            SetConnectionStatus(ESensorState.Scanning);
            BleSensorsManager.Instance.StartScanningForSensors();
        }
            
    }

    public void SwitchGameMode()
    {
        if (_sensor == null) return;

        if (_sensor.IsInGameMode) {
            HandleRequest(ECategory.GameMode, false);
        }
        else {
            HandleRequest(ECategory.GameMode, true);
        }
            
    }

    public void StartMonitoring()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Curves");

        if (BleSensorsManager.Instance != null)
            BleSensorsManager.Instance.onNewSensorConnected -= HandleNewSensorConnected;
    }

#region Handlers
    private void HandleRequest(ECategory category, bool arg)
    {
        if(_sensor == null) {
            s_DebugOnScreenMessage = "No sensor reference in Editor";
            return;
        }

        s_RawData = "";

        switch (category)
        {
            case ECategory.GameMode:
                s_DebugOnScreenMessage = _sensor.SensorName + " Game Mode requested : " + arg;

                if (arg) {
                    _sensor.EnterGameMode(20);
                    SetConnectionStatus(ESensorState.GameMode);
                }
                else {
                    _sensor.ExitGameMode();
                    SetConnectionStatus(ESensorState.Standby);
                }
                break;

            case ECategory.StepsMode:
                s_DebugOnScreenMessage = _sensor.SensorName + " step activity required " + arg;
                _sensor.SetStepMode(arg);
                break;

            case ECategory.StepsResult:
                var settings = StepsHandler.CurrentTimeSettings;
                s_DebugOnScreenMessage = _sensor.SensorName + " step results required : " + settings.Day + ", " + settings.TimeInMinutes;
                _sensor.GetStepModeResult(settings.Day, settings.TimeInMinutes, arg);
                break;

            case ECategory.System:
                s_DebugOnScreenMessage = _sensor.SensorName + " system info requested";
                _sensor.QuerySystemStatus();
                break;

            case ECategory.Tilts:
                break;

            case ECategory.Time:
                s_DebugOnScreenMessage = _sensor.SensorName + " system time requested (Setup ? " + arg + ")";

                 if(arg) // Set System Time
                {
                    _sensor.SetupSystemTime(System.DateTime.Now);
                }
                else // Get System Time
                {
                    _sensor.QuerySystemTime();
                }
                break;

            case ECategory.Vibration:
                s_DebugOnScreenMessage = _sensor.SensorName + " requested to vibrate";
                _sensor.Vibrate();
                break;

            default:
                break;
        }
    }

    private void HandleNewSensorConnected(BleSensorCavybandBase a_sensor)
    {
        _sensor = a_sensor;

        _sensor.onConnectionStatusChanged += HandleSensorConnectionStatus;
        _sensor.onSystemInfoReceived      += HandleSystemInfoReceived;
        //_sensor.onSyncDataReceived        += HandleRawData;
        _sensor.onSyncDataReceived        += StepsHandler.AddStepsValue;

        // Synchronize System Time
        HandleRequest(ECategory.Time, true);

        //Debug.LogError("New sensor : " + a_sensor.BleAddress);
    }

    private void HandleSensorConnectionStatus(BleSensorCavybandBase _sensor, bool isConnected)
    {
        ESensorState newState = isConnected ? ESensorState.Standby : ESensorState.Disconnected;

        SetConnectionStatus(newState);
    }

    private void HandleSystemInfoReceived(BleSensorCavybandBase sensor, ECategory category, object info)
    {
        if(OnInfoReceived != null) {
             OnInfoReceived(category, info);
        }

    }

    private void HandleRawData(BleSensorCavybandBase sensor, byte[] data)
    {
        if (_sensor != null)
        {
            LogOnScreen(System.BitConverter.ToString(data));
        }
        else
        {
            s_DebugOnScreenMessage = "No sensor reference in Editor";
        }
    }
    #endregion Handlers

    private void OnGUI()
    {
        _gStyle.normal.textColor = Color.yellow;
        GUI.Label(new Rect(new Vector2(100, 300), Vector2.one * 50), s_Status, _gStyle);

        _gStyle.normal.textColor = Color.green;
        GUI.Label(new Rect(new Vector2(100, 350), new Vector2(200, 200)), s_DebugOnScreenMessage, _gStyle);

        _gStyle.normal.textColor = Color.black;
        GUI.Label(new Rect(new Vector2(100, 450), new Vector2(200, 800)), s_RawData, _gStyle);
    }

    private void OnDisable()
    {
        _sensor.TearDown();

        MenuController.OnRequest     -= HandleRequest; 
        _sensor.onSystemInfoReceived -= HandleSystemInfoReceived;
        //_sensor.onSyncDataReceived   -= HandleRawData;
        _sensor.onSyncDataReceived   -= StepsHandler.AddStepsValue;
    }

    public static void LogOnScreen(string message)
    {
        if (s_Counter > 30)
        {
            s_RawData = message + "\n";
            s_Counter = 0;
        }
        else
        {
            s_RawData += message + "\n";
            s_Counter++;
        }
    }


}
