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
            //TestScript.LogOnScreen(BitConverter.ToString(a_data) );

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

                case TIME_DATA_HEADER:
                    ParseSystemTime(a_data);
                    break;

                case SYNC_DATA_HEADER:
                    ParseSyncData(a_data);
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

            DeviceInfo info = new DeviceInfo();

            info.RawData = BitConverter.ToString(a_data);
            info.State   = a_data[2];

            info.func = a_data[3];
            info.TimeEnabled  = (a_data[3] & (1 << 1)) == ( 1 << 1);
            info.AlarmEnabled = (a_data[3] & (1 << 2)) == ( 1 << 2);
            info.LlaEnabled   = (a_data[3] & (1 << 3)) == ( 1 << 3);
            info.TiltEnabled  = (a_data[3] & (1 << 4)) == ( 1 << 4);
            info.StepsEnabled = (a_data[3] & (1 << 5)) == ( 1 << 5);

            info.HardwareVersion = a_data[4];
            info.HardwareVersion = a_data[5];

            info.IsCalibrated    = a_data[6] == 3;

            info.MagFactoryOffsetX = (short)(a_data[8]  << 8 | a_data[9]);
            info.MagFactoryOffsetY = (short)(a_data[10] << 8 | a_data[11]);
            info.MagFactoryOffsetZ = (short)(a_data[12] << 8 | a_data[13]);
            info.MagFactoryOffsetRadius = (ushort)(a_data[14] << 8 | a_data[15]);

            if (onSystemInfoReceived != null)
                onSystemInfoReceived(this, ECategory.System, info);
        }

        protected override void ParseSystemTime(byte[] a_data)
        {
            if (ENABLE_LOG)
                Debug.Log("ParseSystemTime : " + BleAddress);

            TimeInfo info = new TimeInfo(a_data[2], a_data[3], a_data[4]);

            if (onSystemInfoReceived != null)
                onSystemInfoReceived(this, ECategory.Time, info);
        }


        protected override void ParseSyncData(byte[] a_data)
        {
            if (ENABLE_LOG)
                Debug.Log("ParseSyncData : " + BleAddress);

            _stepValues.Enqueue(a_data);

            if (a_data[2] == 0xFF && a_data[3] == 0xFF && onSyncDataReceived != null)
                onSyncDataReceived(this, _stepValues);

            TestScript.s_RawData += BitConverter.ToString(a_data) + "\n";
        }
        #endregion
    }

    public struct DeviceInfo
    {
        internal string RawData;

        internal byte State;

        internal byte func;

        internal bool TimeEnabled;
        internal bool AlarmEnabled;
        internal bool LlaEnabled;
        internal bool TiltEnabled;
        internal bool StepsEnabled;

        internal byte  HardwareVersion;
        internal byte  SoftwareVersion;

        internal bool IsCalibrated;
        internal int  MagFactoryOffsetX;
        internal int  MagFactoryOffsetY;
        internal int  MagFactoryOffsetZ;
        internal uint  MagFactoryOffsetRadius;

    }

    public enum EDay { Monday = 1, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday}

    public struct TimeInfo
    {
        internal byte data1;
        internal byte data2;
        internal EDay Weekday;

        public TimeInfo(byte data1, byte data2, byte data3)
        {
            this.data1 = data1;
            this.data2 = data2;

            this.Weekday = (EDay)data3;
        }
    }
}