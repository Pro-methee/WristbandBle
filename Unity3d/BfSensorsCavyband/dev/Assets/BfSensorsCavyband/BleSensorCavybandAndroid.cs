using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BfSensorsCavyband
{
    public class BleSensorCavybandAndroid : BleSensorCavybandBase
    {
        #region Properties
        AndroidJavaObject _javaObject = null;
        #endregion

        public BleSensorCavybandAndroid(string a_bleAddress, AndroidJavaObject a_object) : base(a_bleAddress)
        {
            _javaObject = a_object;
            _sensorName = FetchDeviceNameNative();
        }

        public override void TearDown()
        {
            base.TearDown();
            _javaObject = null;
        }

        #region Native Methods
        protected override string FetchDeviceNameNative()
        {
            string name = _javaObject.Call<string>("getDisplayName");
            return name;
        }

        protected override void SendCommandNative(string a_commandToSend)
        {
            if (ENABLE_LOG)
                Debug.Log("SendCommandNative : " + BleAddress);

            _javaObject.Call("sendCommand", a_commandToSend);
        }

        protected override byte[] FetchLastDataNative()
        {
            byte[] dataFetched = _javaObject.Call<byte[]>("getLastDataReceived");
            return dataFetched;
        }
        #endregion

        #region Data
        protected override void ParseData(byte[] a_data)
        {
            string header = ParseHeader(a_data);
            switch (header)
            {
                case BUTTON_PRESSED_MESSAGE_HEADER:
                    ParseButtonEventData(a_data);
                    break;

                case MOTION_DATA_MESSAGE_HEADER:
                    ParseMotionData(a_data);
                    break;

                case SYSTEM_MESSAGE_HEADER:
                    ParseSystemData(a_data);
                    break;

                default:
                    if (ENABLE_LOG)
                        Debug.LogError("Unhandled message header \"" + header + "\" : " + BleAddress);
                    break;
            }
        }

        protected string ParseHeader(byte[] a_data)
        {
            string header = BitConverter.ToString(new byte[] { a_data[1] });
            return header;
        }

        byte[] _conversionArray = new byte[2];
        protected override void ParseMotionData(byte[] a_data)
        {
            if (ENABLE_LOG)
                Debug.Log("ParseMotionData : " + BleAddress);

            //Convert from Big Endian to little endian.
            _currentRotation.w = BitConverter.ToInt16(BigToLittleEndianShort(a_data, _conversionArray, 2), 0) / QUATERNION_DIVISER;
            _currentRotation.x = BitConverter.ToInt16(BigToLittleEndianShort(a_data, _conversionArray, 4), 0) / QUATERNION_DIVISER;
            _currentRotation.y = BitConverter.ToInt16(BigToLittleEndianShort(a_data, _conversionArray, 6), 0) / QUATERNION_DIVISER;
            _currentRotation.z = BitConverter.ToInt16(BigToLittleEndianShort(a_data, _conversionArray, 8), 0) / QUATERNION_DIVISER;
            // RightHand To LeftHand Quaternion
            _currentRotation = new Quaternion(_currentRotation.x, _currentRotation.z, _currentRotation.y, -_currentRotation.w);

            //x,y,z -> x,z,y to lefthand expressed in unity.
            _currentAcceleration.x = BitConverter.ToInt16(BigToLittleEndianShort(a_data, _conversionArray, 10), 0) / ACCELERATION_DIVISER;
            _currentAcceleration.z = BitConverter.ToInt16(BigToLittleEndianShort(a_data, _conversionArray, 12), 0) / ACCELERATION_DIVISER;
            _currentAcceleration.y = BitConverter.ToInt16(BigToLittleEndianShort(a_data, _conversionArray, 14), 0) / ACCELERATION_DIVISER;

            if (onMotionDataReceived != null)
                onMotionDataReceived(this, _currentRotation, _currentAcceleration);
        }

        protected override void ParseButtonEventData(byte[] a_data)
        {
            if (ENABLE_LOG)
                Debug.Log("ParseButtonEventData : " + BleAddress);

            short buttonEventType = BitConverter.ToInt16(BigToLittleEndianShort(a_data, _conversionArray, 2), 0);
            /*if (buttonEventType == BUTTON_EVENT_TYPE_PRESSED)
            {
                if (onButtonPressed != null)
                    onButtonPressed(this, true);
            }
            else if (buttonEventType == BUTTON_EVENT_TYPE_RELEASED)
            {
                if (onButtonPressed != null)
                    onButtonPressed(this, false);
            }
            else if (buttonEventType == BUTTON_EVENT_TYPE_LONG_PRESSED)
            {
                if (onButtonLongPressed != null)
                    onButtonLongPressed(this);
            }*/

            if (onButtonPressed != null)
                onButtonPressed(this, true);
        }

        protected override void ParseSystemData(byte[] a_data)
        {
            if (ENABLE_LOG)
                Debug.Log("ParseSystemData : " + BleAddress);
        }
        #endregion
    }
}