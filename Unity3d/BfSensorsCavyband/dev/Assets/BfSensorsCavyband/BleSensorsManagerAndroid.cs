using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BfSensorsCavyband
{
#if UNITY_ANDROID
    public class BleSensorsManagerAndroid : BleSensorsManagerBase
    {
        protected AndroidJavaClass _managerClass;
        protected AndroidJavaClass ManagerClass
        {
            get
            {
                if (_managerClass == null)
                    _managerClass = new AndroidJavaClass("com.breakfirst.bfsensorscavyband.BleSensorsInterop");

                return _managerClass;
            }
        }

        protected AndroidJavaObject _javaObject;

        public BleSensorsManagerAndroid()
        {
            _javaObject = ManagerClass.CallStatic<AndroidJavaObject>("getInstance");
        }

        protected override BleSensorCavybandBase FetchSensorForBleAddressNative(string a_bleAddress)
        {
            AndroidJavaObject sensorObject = _javaObject.Call<AndroidJavaObject>("getSensorForAddress", a_bleAddress);
            BleSensorCavybandAndroid cavyband = new BleSensorCavybandAndroid(a_bleAddress, sensorObject);
            return cavyband;
        }

        protected override void ResetNative()
        {
            _javaObject.Call("reset");
        }

        protected override void StartScanningForSensorsNative()
        {
            _javaObject.Call("startScanningForSensors");
        }

        protected override void StopScanningForSensorsNative()
        {
            _javaObject.Call("stopScanningForSensors");
        }
    }
#endif
}