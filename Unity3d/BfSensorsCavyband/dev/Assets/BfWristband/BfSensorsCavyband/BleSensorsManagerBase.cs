using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BfWristband
{
    public abstract class BleSensorsManagerBase
    {
        protected static bool ENABLE_LOG
        {
            get
            {
                return BleSensorsManager.ENABLE_LOG;
            }
        }
        #region Properties
        protected List<string> _sensorsId = new List<string>();
        public List<string> SensorsId {
            get {
                return _sensorsId;
            }
        }
        public string       DefaultId {
            get {
                if (_sensorsId.Count == 0)
                    _sensorsId.Add("FakeId");
                return _sensorsId[0];
            }
        }


        protected Dictionary<string, BleSensorBase> _sensorsByBleAddress = new Dictionary<string, BleSensorBase>();
        public Dictionary<string, BleSensorBase> SensorsByBleAddress
        {
            get
            {
                return _sensorsByBleAddress;
            }
        }

        protected bool _isScanningForSensors = false;
        public bool IsScanningForSensors
        {
            get
            {
                return _isScanningForSensors;
            }
        }
        #endregion

        #region Delegates
        public BleSensorDelegate onNewSensorConnected = null;
        #endregion

        public void NotifyApplicationPaused(bool a_isPaused)
        {
            foreach (BleSensorBase each in _sensorsByBleAddress.Values)
            {
                each.NotifyApplicationPaused(a_isPaused);
            }

            if (a_isPaused && _isScanningForSensors)
                StopScanningForSensorsNative();
            else if (!a_isPaused && _isScanningForSensors)
                StartScanningForSensorsNative();
        }

        public void NotifyNewSensorConnectedAtAddress(string a_bleAddress)
        {
            BleSensorBase sensor = FetchSensorForBleAddressNative(a_bleAddress);

            if (sensor != null)
            {
                _sensorsByBleAddress.Add(a_bleAddress, sensor);

                if (ENABLE_LOG)
                    Debug.Log("NotifyNewSensorConnectedAtAddress : " + a_bleAddress);

                if (onNewSensorConnected != null)
                    onNewSensorConnected(sensor);
            }
        }

        public void Reset()
        {
            foreach (BleSensorBase each in _sensorsByBleAddress.Values)
            {
                each.TearDown();
            }
            _sensorsByBleAddress.Clear();
            ResetNative();
        }

        public void StartScanningForSensors()
        {
            if (!_isScanningForSensors)
            {
                _isScanningForSensors = true;
                if (ENABLE_LOG)
                    Debug.Log("StartScanningForSensors");
                StartScanningForSensorsNative();
            }
        }

        public void StopScanningForSensors()
        {
            if (_isScanningForSensors)
            {
                _isScanningForSensors = false;

                if (ENABLE_LOG)
                    Debug.Log("StopScanningForSensors");
                StopScanningForSensorsNative();
            }
        }

        #region Native Plateform specific
        protected abstract void ResetNative();

        protected abstract void StartScanningForSensorsNative();
        protected abstract void StopScanningForSensorsNative();

        protected abstract BleSensorBase FetchSensorForBleAddressNative(string a_bleAddress);
        #endregion
    }
}