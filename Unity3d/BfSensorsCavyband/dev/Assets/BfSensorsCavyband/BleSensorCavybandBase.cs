using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BfSensorsCavyband
{
    public delegate void BleSensorCavybandDelegate(BleSensorCavybandBase a_sensor);
    public delegate void BleSensorCavybandConnectionDelegate(BleSensorCavybandBase a_sensor, bool a_isConnected);
    public delegate void BleSensorCavybandButtonDelegate(BleSensorCavybandBase a_sensor, bool a_isDown);
    public delegate void BleSensorCavybandMotionDataDelegate(BleSensorCavybandBase a_sensor, Quaternion a_rot, Vector3 a_acc);

    public abstract class BleSensorCavybandBase
    {
        protected static bool ENABLE_LOG
        {
            get
            {
                return BleSensorsManager.ENABLE_LOG;
            }
        }

        public byte[] BigToLittleEndianShort(byte[] a_source, byte[] a_dest, int a_offset)
        {
            Array.Copy(a_source, a_offset, a_dest, 0, 2);
            Array.Reverse(a_dest);
            return a_dest;
        }

        protected const string BUTTON_PRESSED_MESSAGE_HEADER = "D1";
        protected const string MOTION_DATA_MESSAGE_HEADER = "A1";
        protected const string SYSTEM_MESSAGE_HEADER = "C1";

        protected static short BUTTON_EVENT_TYPE_PRESSED = 1;
        protected static short BUTTON_EVENT_TYPE_RELEASED = 0;
        protected static short BUTTON_EVENT_TYPE_LONG_PRESSED = 3;

        protected static float QUATERNION_DIVISER = 16384f;
        protected static float ACCELERATION_DIVISER = 1000f;

        #region Properties
        protected string _bleAddress;
        public string BleAddress
        {
            get
            {
                return _bleAddress;
            }
        }

        protected string _sensorName;
        public string SensorName
        {
            get
            {
                return _sensorName;
            }
        }

        /// <summary>
        /// In Miliseconds
        /// </summary>
        protected int _gameModeInterval = 50;
        /// <summary>
        /// In Miliseconds
        /// </summary>
        public int GameModeInterval
        {
            get
            {
                return _gameModeInterval;
            }
        }

        protected bool _isInGameMode = false;
        public bool IsInGameMode
        {
            get
            {
                return _isInGameMode;
            }
        }

        protected bool _isConnected = false;
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
        }

        #region Motion Data
        protected Quaternion _currentRotation = Quaternion.identity;
        public Quaternion CurrentRotation
        {
            get
            {
                return _currentRotation;
            }
        }

        protected Vector3 _currentAcceleration = Vector3.zero;
        public Vector3 CurrentAcceleration
        {
            get
            {
                return _currentAcceleration;
            }
        }
        #endregion
        #endregion

        public virtual void TearDown()
        {
            onConnectionStatusChanged = null;
            onButtonPressed = null;
            //onButtonLongPressed = null;
            onMotionDataReceived = null;
        }

        #region Delegates
        public BleSensorCavybandConnectionDelegate onConnectionStatusChanged;
        public BleSensorCavybandButtonDelegate onButtonPressed;
        //public BleSensorCavybandDelegate onButtonLongPressed;
        public BleSensorCavybandMotionDataDelegate onMotionDataReceived;
        #endregion

        public BleSensorCavybandBase(string a_bleAddress)
        {
            _bleAddress = a_bleAddress;
        }

        #region Native Methods
        protected abstract string FetchDeviceNameNative();
        protected abstract void SendCommandNative(string a_commandToSend);

        protected abstract byte[] FetchLastDataNative();
        #endregion

        #region System
        public void QuerySystemStatus()
        {
            SendCommandNative("?SYSTEM\n");
        }
        #endregion
        
        #region Game Mode
        public void EnterGameMode()
        {
            EnterGameMode(_gameModeInterval);
        }
        public void EnterGameMode(int a_interval)
        {
            _gameModeInterval = a_interval;
            _isInGameMode = true;
            SendCommandNative("%OPR=3,4," + _gameModeInterval.ToString() + "\\n");
            if (ENABLE_LOG)
                Debug.Log("EnterGameMode : " + BleAddress);
        }

        public void ExitGameMode(bool a_shouldChangeState = true)
        {
            if(a_shouldChangeState)
                _isInGameMode = false;

            if (ENABLE_LOG)
                Debug.Log("ExitGameMode : " + BleAddress);
            SendCommandNative("%OPR=3,0\\n");
        }
        #endregion

        #region Notification
        public void NotifyApplicationPaused(bool a_isPaused)
        {
            if (IsConnected)
            {
                if (a_isPaused && _isInGameMode)
                    ExitGameMode(false);
                else if (!a_isPaused && _isInGameMode)
                    EnterGameMode();
            }
        }

        public void NotifyNewDataReceived()
        {
            byte[] data = FetchLastDataNative();
            ParseData(data);
        }

        public void NotifyReconnected()
        {
            _isConnected = true;

            if (ENABLE_LOG)
                Debug.Log("Sensor reconnected : " + BleAddress);

            if (_isInGameMode)
                EnterGameMode();

            if (onConnectionStatusChanged != null)
                onConnectionStatusChanged(this, _isConnected);
        }

        public void NotifyDisconnected()
        {
            _isConnected = false;

            if (ENABLE_LOG)
                Debug.Log("Sensor disconnected : " + BleAddress);

            if (onConnectionStatusChanged != null)
                onConnectionStatusChanged(this, _isConnected);
        }
        #endregion

        #region Data
        protected abstract void ParseData(byte[] a_data);

        protected abstract void ParseMotionData(byte[] a_data);
        protected abstract void ParseButtonEventData(byte[] a_data);
        protected abstract void ParseSystemData(byte[] a_data);
        #endregion
    }
}