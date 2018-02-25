using System;
using System.Collections.Generic;
using UnityEngine;

using BfWristband;

namespace BfWristband.Api
{
    /// <summary>
    /// This class is an sample implementation of BfWristband.BleSensorsManager and BleSensorCavybandBase classes.
    /// Its purpose is to make the connection workflow, to keep a reference on the BleSensorCavybandBase Sensor and to map events from the EventsHandler
    /// </summary>
    internal class WristbandController : MonoBehaviour
    {
        // Holds a reference to the BleSensor. Should be replaced by a list of sensors
        private BleSensorBase _sensor = null;

        // The state of connection
        private ESensorState          _sensorState;

        // Use this for initialization
        private void Awake()
        {

            if (BleSensorsManager.Instance != null)
                BleSensorsManager.Instance.onNewSensorConnected += HandleNewSensorConnected;

            // Make subscription for delegates
            Subscribe();

            DontDestroyOnLoad(gameObject);

        }

        /// <summary>
        /// More handlers should be added to this method depending of the Wristband functionalities implemented
        /// </summary>
        private void Subscribe()
        {
            EventsHandler.OnGameModeRequested       += HandleGameModeRequest;
            EventsHandler.OnSystemInfoRequested     += HandleSystemRequest;
            EventsHandler.OnTimeSystemRequested     += HandleTimeSystemRequest;
            EventsHandler.OnStepsModeRequested      += HandleStepsModeRequest;
            EventsHandler.OnStepsResultRequested    += HandleStepsResultRequest;
            EventsHandler.OnVibrationRequested      += HandleVibrationRequest;
        }

        /// <summary>
        /// Do not forget to unsuscribe from events of the Subscribe method
        /// </summary>
        private void Unsubscribe()
        {
            EventsHandler.OnGameModeRequested       -= HandleGameModeRequest;
            EventsHandler.OnSystemInfoRequested     -= HandleSystemRequest;
            EventsHandler.OnTimeSystemRequested     -= HandleTimeSystemRequest;
            EventsHandler.OnStepsModeRequested      -= HandleStepsModeRequest;
            EventsHandler.OnStepsResultRequested    -= HandleStepsResultRequest;
            EventsHandler.OnVibrationRequested      -= HandleVibrationRequest;
        }

        /// <summary>
        /// Changes the connection state during the connection flow
        /// </summary>
        /// <param name="newState">The state that should be achieved</param>
        private void SetConnectionStatus(ESensorState newState)
        {
            switch (newState)
            {
                case ESensorState.Disconnected:
                    if (_sensorState == ESensorState.Scanning)
                    {
                        _sensorState = ESensorState.Disconnected;
                    }
                    break;

                default:
                    _sensorState = newState;
                    break;
            }

            DebugOnScreen.Log(_sensorState.ToString(), DebugOnScreen.ELogType.Status);
        }

        /// <summary>
        /// Toggles the scanning activity On/Off and update the connection state
        /// </summary>
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

        /// <summary>
        /// Toggles the Game Mode On/ Off. 
        /// Realtime activity can only be measured when Game Mode is on, but Steps and Tilts (Dayly activity) can be tracked whatever the mode
        /// </summary>
        public void SwitchGameMode()
        {
            if (_sensor == null) return;

            if (_sensor.IsInGameMode) {
                HandleGameModeRequest(_sensor.BleAddress, false);
            }
            else {
                HandleGameModeRequest(_sensor.BleAddress, true);
            }

        }

        /// <summary>
        /// Launches a scene in which the functionalities can be tested and unsuscribe from onNewSensorConnected
        /// </summary>
        public void StartMonitoring()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Curves");

            if (BleSensorsManager.Instance != null)
                BleSensorsManager.Instance.onNewSensorConnected -= HandleNewSensorConnected;
        }

        #region Handlers
        /// <summary>
        /// Delegate invoked when a sensor is connected
        /// </summary>
        /// <param name="a_sensor"></param>
        private void HandleNewSensorConnected(BleSensorBase a_sensor)
        {
            // Keeps a reference on the BleSensor
            _sensor = a_sensor;

            // Keeps track of this connection status for this sensor
            _sensor.onConnectionStatusChanged   += HandleSensorConnectionStatus;
            HandleSensorConnectionStatus(_sensor, true);

            // Maps events that are sent by the device when it is requested for data
            _sensor.onSystemInfoReceived += (BleSensorBase sensor, DeviceInfo info) => { EventsHandler.OnSystemInfoReceived(sensor.BleAddress, info); };
            _sensor.onSystemTimeReceived += (BleSensorBase sensor, TimeInfo info)   => { EventsHandler.OnTimeInfoReceived(sensor.BleAddress, info); };
            _sensor.onSyncDataReceived   += (BleSensorBase sensor, byte[] data)     => { EventsHandler.OnStepsResultReceived(sensor.BleAddress, data); };

            /**********************************************************/
            // DEBUG ONLY : Comments this line for Developement Build
            _sensor.onDataReceived += HandleRawDataReceived;
            /**********************************************************/

            // Synchronize System Time
            HandleTimeSystemRequest(_sensor.BleAddress, true);

            //Debug.LogError("New sensor : " + a_sensor.BleAddress);
        }

        /// <summary>
        /// Delegate invoked when a sensor connection status changes
        /// </summary>
        /// <param name="_sensor"></param>
        /// <param name="isConnected"></param>
        private void HandleSensorConnectionStatus(BleSensorBase _sensor, bool isConnected)
        {
            ESensorState newState = isConnected ? ESensorState.Standby : ESensorState.Disconnected;

            SetConnectionStatus(newState);
        }

        /// <summary>
        /// Delegate invoked when a request is made to a sensor in order to get System Info (Functions enabled, HW/FW versions, calibration, ...)
        /// </summary>
        /// <param name="id">Not used in this version because only one sensor is managed</param>
        private void HandleSystemRequest(string id)
        {
            if(_sensor == null) {
                DebugOnScreen.Log("HandleSystemRequest failed : No Sensor referenced in WristbandController");
                return;
            }

            DebugOnScreen.Log(_sensor.SensorName + " system info requested");
            _sensor.QuerySystemStatus();
        }

        /// <summary>
        /// Delegate invoked when a request is made to a sensor in order to get Time Info or to make a synchronisation
        /// </summary>
        /// <param name="id">Not used in this version because only one sensor is managed</param>
        /// <param name="doSetup">if true, the System Time of the device is synchronised with the tablet / PC / Phone </param>
        private void HandleTimeSystemRequest(string id, bool doSetup)
        {
            if (_sensor == null) {
                DebugOnScreen.Log("HandleTimeSystemRequest failed : No Sensor referenced in WristbandController");
                return;
            }

            DebugOnScreen.Log(_sensor.SensorName + " system time requested (Setup ? " + doSetup + ")");

            if (doSetup) // Set System Time
            {
                _sensor.SetupSystemTime(System.DateTime.Now);
            }
            else // Get System Time
            {
                _sensor.QuerySystemTime();
            }
        }

        /// <summary>
        /// Delegate invoked when a request is made to a sensor in order to enter or exit the Game Mode
        /// </summary>
        /// <param name="id">Not used in this version because only one sensor is managed</param>
        /// <param name="isActivityRequested">If true, enters the Game Mode / Exits if false</param>
        private void HandleGameModeRequest(string id, bool isActivityRequested)
        {
            if (_sensor == null) {
                DebugOnScreen.Log("HandleGameModeRequest failed : No Sensor referenced in WristbandController");
                return;
            }

            DebugOnScreen.Log(_sensor.SensorName + " Game Mode requested : " + isActivityRequested);

            if (isActivityRequested) {
                _sensor.EnterGameMode(20);
                SetConnectionStatus(ESensorState.GameMode);
            }
            else {
                _sensor.ExitGameMode();
                SetConnectionStatus(ESensorState.Standby);
            }
        }

        /// <summary>
        /// Delegate invoked when a request is made to a sensor in order to start steps count measurement or to stop it
        /// </summary>
        /// <param name="id">Not used in this version because only one sensor is managed</param>
        /// <param name="isActivityRequested">If true, starts the measurement / Stops it if false</param>
        private void HandleStepsModeRequest(string id, bool isActivityRequested)
        {
            if (_sensor == null) {
                DebugOnScreen.Log("HandleStepsModeRequest failed : No Sensor referenced in WristbandController");
                return;
            }

            DebugOnScreen.Log(_sensor.SensorName + " step activity required " + isActivityRequested);

            _sensor.SetStepMode(isActivityRequested);
        }

        /// <summary>
        /// Delegate invoked when a request is made to a sensor in order to get the results from steps measurement
        /// </summary>
        /// <param name="id">Not used in this version because only one sensor is managed</param>
        /// <param name="settings">A struct that holds data concerning the moment of the day from which we should get the values</param>
        private void HandleStepsResultRequest(string id, StepsDataSettings settings)
        {
            if (_sensor == null)
            {
                DebugOnScreen.Log("HandleStepsResultRequest failed : No Sensor referenced in WristbandController");
                return;
            }

            string logMsg = String.Format("{0} step results required : {1:f} ({2},{3}) > cmd : ", _sensor.SensorName, settings.Start, settings.DayId, settings.TimeInMinutes);

            if (settings.DayId == 0)
            {
                logMsg += _sensor.GetStepModeResult() + "\n";
            }
            else
            {
                logMsg += _sensor.GetStepModeResult(settings.DayId, settings.TimeInMinutes) + "\n";
            }

            DebugOnScreen.Log(logMsg);
        }

        /// <summary>
        /// Delegate invoked when a request is made to a sensor in order to vibrate
        /// </summary>
        /// <param name="id">Not used in this version because only one sensor is managed</param>
        private void HandleVibrationRequest(string id)
        {
            if (_sensor == null) {
                DebugOnScreen.Log("HandleStepsModeRequest failed : No Sensor referenced in WristbandController");
                return;
            }

            DebugOnScreen.Log(_sensor.SensorName + " requested to vibrate");

            _sensor.Vibrate();
        }

        /// <summary>
        /// Delegate invoked when each time data are sent by the device (DEBUG PURPOSE ONLY)
        /// </summary>
        /// <param name="sensor">Not used in this version because only one sensor is managed</param>
        /// <param name="data">Array of bytes (raw data)</param>
        private void HandleRawDataReceived(BleSensorBase sensor, byte[] data)
        {
            if(!sensor.IsInGameMode)
                DebugOnScreen.Log(System.BitConverter.ToString(data), DebugOnScreen.ELogType.RawData);
        }
        #endregion Handlers

        private void OnDisable()
        {
            Unsubscribe();

            if (_sensor == null) {
                DebugOnScreen.Log("_sensor.TearDown() failed : No Sensor referenced in WristbandController");
                return;
            }
            else {
                _sensor.TearDown();
            }

        }


    }
}

