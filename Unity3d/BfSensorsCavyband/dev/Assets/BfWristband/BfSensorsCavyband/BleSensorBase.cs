using System.Collections.Generic;
using UnityEngine;
using System;

namespace BfWristband
{
    public delegate void BleSensorDelegate(BleSensorBase a_sensor);
    public delegate void BleSensorConnectionDelegate(BleSensorBase a_sensor, bool a_isConnected);
    public delegate void BleSensorButtonDelegate(BleSensorBase a_sensor, bool a_isDown);
    public delegate void BleSensorMotionDataDelegate(BleSensorBase a_sensor, Quaternion a_rot, Vector3 a_acc);
    public delegate void BleSensorDataSyncDelegate(BleSensorBase a_sensor,byte[] data);
    public delegate void BleSensorSystemDelegate(BleSensorBase a_sensor, DeviceInfo deviceInfo);
    public delegate void BleSensorTimeDelegate(BleSensorBase a_sensor, TimeInfo timeInfo);
    public delegate void BleSensorGlobalDelegate(BleSensorBase a_sensor, byte[] data);

    public abstract class BleSensorBase
    {
        protected static bool ENABLE_LOG
        {
            get
            {
                return BleSensorsManager.ENABLE_LOG;
            }
        }

        public static byte[] BigToLittleEndianShort(byte[] a_source, byte[] a_dest, int a_offset)
        {
            Array.Copy(a_source, a_offset, a_dest, 0, 2);
            Array.Reverse(a_dest);
            return a_dest;
        }

        protected const string BUTTON_PRESSED_MESSAGE_HEADER = "D1";
        protected const string MOTION_DATA_MESSAGE_HEADER = "A1";
        protected const string SYSTEM_MESSAGE_HEADER      = "C1";
        protected const string TIME_DATA_HEADER           = "C3";
        protected const string SYNC_DATA_HEADER           = "DA";

        protected static short BUTTON_EVENT_TYPE_PRESSED        = 1;
        protected static short BUTTON_EVENT_TYPE_RELEASED       = 0;
        protected static short BUTTON_EVENT_TYPE_LONG_PRESSED   = 3;

        protected static float QUATERNION_DIVISER   = 16384f;
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
        #endregion Motion Data

        #region Sync Data
        protected Queue<byte> _tiltValues = new Queue<byte>();
        //public Queue<byte> TiltValues {
        //    get {
        //        return _tiltValues;
        //    }
        //}

        protected Queue<byte[]> _stepValues = new Queue<byte[]>();
        //public Queue<byte> StepValues {
        //    get {
        //        return _stepValues;
        //    }
        //}
        #endregion Sync Data

        #endregion Properties

        public virtual void TearDown()
        {
            onConnectionStatusChanged = null;
            onButtonPressed = null;
            //onButtonLongPressed = null;
            onMotionDataReceived = null;
            onSystemInfoReceived = null;
            onSystemTimeReceived = null;

            onDataReceived = null;

        }

        #region Delegates

        public BleSensorConnectionDelegate  onConnectionStatusChanged;
        public BleSensorButtonDelegate      onButtonPressed;
        //public BleSensorCavybandDelegate onButtonLongPressed;
        public BleSensorMotionDataDelegate  onMotionDataReceived;
        public BleSensorDataSyncDelegate    onSyncDataReceived;
        public BleSensorSystemDelegate      onSystemInfoReceived;
        public BleSensorTimeDelegate        onSystemTimeReceived;
        public BleSensorGlobalDelegate      onDataReceived;
        #endregion

        public BleSensorBase(string a_bleAddress)
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

        public void QuerySystemTime()
        {
            SendCommandNative("?TIME\n");
        }

        public void SetupSystemTime(DateTime date)
        {
            int day = (int)(date.DayOfWeek);
            int currentDayOfWeek     = day == 0 ? 7 : day;
            int currentTimeInMinutes = date.Hour * 60 + date.Minute;

            string SetupCmd = "%TIME=" + currentTimeInMinutes + "," + currentDayOfWeek + "\n";

            SendCommandNative(SetupCmd);
            
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
            SendCommandNative("%OPR=3,4," + _gameModeInterval.ToString() + "\n");
            if (ENABLE_LOG)
                Debug.Log("EnterGameMode : " + BleAddress);
        }

        public void ExitGameMode(bool a_shouldChangeState = true)
        {
            if(a_shouldChangeState)
                _isInGameMode = false;

            if (ENABLE_LOG)
                Debug.Log("ExitGameMode : " + BleAddress);
            SendCommandNative("%OPR=3,0\n");
        }
        #endregion

        #region Vibration
        /// <summary>
        /// Request a Vibration in the wristband
        /// </summary>
        /// <param name="count">counter number ( support value< 99)</param>
        /// <param name="intensity">10-99% of full power (0 = off)</param>
        /// <param name="onPeriod">support value<999ms</param>
        /// <param name="offPeriod">support value<2000ms</param>
        public void Vibrate(uint count = 1, uint intensity = 99, uint onPeriod = 200, uint offPeriod = 500)
        {
            string[] args = new string[4];

            args[0] = Math.Min(count, 99).ToString();

            if (intensity > 9)
                args[1] = Math.Min(intensity, 99).ToString();
            else
                args[1] = "0";

            args[2] = Math.Min(onPeriod, 998).ToString();

            args[3] = Math.Min(intensity, 1999).ToString();

            SendCommandNative("%VIB=" + args[0] + "," + args[1] + "," + args[2] + "," + args[3] + "\n");
        }
        #endregion

        #region Steps Mode
        public void SetStepMode(bool isActivityRequired)
        {
            if(isActivityRequired)
                SendCommandNative("%CFG=5,1\n");
            else
                SendCommandNative("%CFG=5,0\n");

        }

        public string GetStepModeResult()
        {
            string cmd = "%SYNC=0,0\n";

            SendCommandNative(cmd);

            return cmd;
        }


        public string GetStepModeResult(int day, int time)
        {
            day     = Mathf.Clamp(day, 1, 2);
            time    = Mathf.Clamp(time, 0, 143);

            string cmd = "%SYNC=" + day + "," + time + "\\n";

            SendCommandNative(cmd);

            return cmd;
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
        protected abstract void ParseSystemTime(byte[] a_data);
        protected abstract void ParseSyncData(byte[] a_data);
        #endregion
    }
}