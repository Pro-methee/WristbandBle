using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BfWristband.Api
{
    public class StepsHandler : MonoBehaviour
    {
        // Steps data process is asynchronous : First the values are parsed from byte[] to StepsData, stored into a list, then the list is sent for UI display
        internal static Action<List<StepsData>> OnStepsResultEnded;

        [Header("Reference Time Settings")]
        [SerializeField]
        [Tooltip("Dropdown list to choose the day")]
        private Dropdown    _dropList;

        [SerializeField]
        [Tooltip("InputField that holds hour of the day")]
        private InputField  _hoursInput;

        [SerializeField]
        [Tooltip("InputField that holds minutes in the hour")]
        private InputField  _minutesInput;

        [SerializeField]
        [Tooltip("LineRenderer to display Steps results")]
        private LineRenderer _lineOutput;

        // a struct holding data (day, time) to get steps result (starting time)
        private StepsDataSettings   _currentSettings;

        // The list of data processed when receiving results from the wristband
        private List<StepsData>     _stepsValues    = new List<StepsData>();

        // Data blocks Id should range from 0x00 to 0x8F and 0xFF means that data stream is over
        private byte _currentBlockId = 0xFE;

        // Holding the max value for steps count is easier to scale the result when displaying the line renderer
        private int  _maxValue       = 0;

        private void Awake()
        {
            if (_dropList == null)
                _dropList = GetComponentInChildren<Dropdown>();

            // This event is first sent by the BleSensor (onSyncDataReceived) but it is mapped in Wristband Controller
            EventsHandler.OnStepsResultReceived += HandleStepsResult;

            // Simple event that is invoked when a UI cleaning is needed
            MenuController.OnCleanUIRequested   += HandleCleanRequest;

        }

        private void OnDestroy()
        {
            // CAUTION, this unsuscription must not be made in OnDisable because this gameobject is disabled each time OnValidButton is invoked
            EventsHandler.OnStepsResultReceived -= HandleStepsResult;
        }

        /// <summary>
        /// Callback triggered when time is validating in input panel
        /// </summary>
        public void OnValidButton()
        {
            int startingHour, startingMinutes;
            DateTime now = DateTime.Now;

            // First, parse data into suitable StepsDataSettings
            if (Int32.TryParse(_hoursInput.text, out startingHour) && Int32.TryParse(_minutesInput.text, out startingMinutes))
            {
                _currentSettings  = new StepsDataSettings(_dropList.value + 1, startingHour, startingMinutes);
            }

            // Reset storing values
            _stepsValues.Clear();
            _maxValue = 0;

            // Modify the following lines to get the Device Id from which you want to get step counts.
            string deviceId = "fakeId";
            if (BfWristband.BleSensorsManager.Instance != null)
                deviceId = BfWristband.BleSensorsManager.Instance.DefaultId;

            // Fire event that will request data through Wristband Controller and BleSensorBase
            if (EventsHandler.OnStepsResultRequested != null)
            {
                EventsHandler.OnStepsResultRequested(deviceId, _currentSettings);
            }

            // Then hide the panel
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Callback that receive a Sync Data Frame formated like :'$ DA 01 05 01 00 04 BE 02 00 00 AF 03 00 01 87 04 00 03 AA'
        /// meaning 'Header(DA)-day(0-2)-blockId(0-0x6C)-SubId(1-0x90)-TiltCount-StepsCount-StepsCount-blockId(0-0x6C)-SubId(1-0x90)-etc 
        /// </summary>
        /// <param name="rawDataFrame"></param>
        public void HandleStepsResult(string id, byte[] rawData)
        {
            Debug.Log("StepsHandler::AddStepsValue with " + BitConverter.ToString(rawData));

            // A 
            if (rawData.Length < 20 || rawData[1] != 0xDA)
            {
                Debug.LogWarning("Incorrect data in StepsHandler :: AddStepsValue with " + BitConverter.ToString(rawData));
                return;
            }

            // Last frame : $ DA FF FF 00 00 00 ....
            if (rawData[2] == 0xFF && rawData[3] == 0xFF)
            {
                //Debug.Log("Ending data in StepsHandler :: AddStepsValue");

                if(OnStepsResultEnded != null)
                    OnStepsResultEnded(_stepsValues);

                // Reset the current block Id for future uses
                _currentBlockId = 0xFE;

                // Graphic display
                DisplayLineResult();
                return;
            }

            // Do not take a repeated data in to account (just in case...)
            if (rawData[3] == _currentBlockId)
            {
                Debug.LogWarning("Redundant Frame ID in StepsHandler :: AddStepsValue (Id = " + _currentBlockId + ")");
                return;
            }
            _currentBlockId = rawData[3]; // and keep track of the current block

            // Store the values after they are interpreted
            _stepsValues.AddRange(ParseData(rawData));

        }

        /// <summary>
        /// This method parses 1 frame of steps data, received in the format : $ DA 01 08 00 00 55 09 00 02 DA 10 00 01 4F 11 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private StepsData[] ParseData(byte[] data)
        {
            // Every single Frame holds 4 data blocks of measurement
            StepsData[] parsedData = new StepsData[4];

            // 0x01 = 'Yesterday', 0x02 = 'Today'
            int day = (int)data[2];

            int milestone, count;
            for (int i = 1; i < 5; i++)
            {
                milestone = (short)(0x00 | data[i * 4]); // This is the time of the day in tens of minutes (e.g; 0x08 = 80 minutes = 01h20 AM)
                
                // The number of steps for a period of 10 minutes (e.g. 01 4F = (0000 0001 0000 0000 | 0100 1111) = 335 steps)
                count = (short)(data[i * 4 + 2] << 8 | data[i * 4 + 3]);

                // Create a struct result that calculate the TimeDate resulting from the day and milestone values
                parsedData[i - 1] = new StepsData(day, milestone, count);

                _maxValue = Math.Max(_maxValue, count);
            }

            return parsedData;
        }

        /// <summary>
        /// This method displays the steps counts in a basic way
        /// </summary>
        private void DisplayLineResult()
        {
            if(_lineOutput == null)
            {
                DebugOnScreen.Log("Unable to display Steps Result : LineOutput is not referenced in StepsHandler !");
                return;
            }

            _lineOutput.gameObject.SetActive(true);

            // Scale multipliers hereafter (38 and 20) should be calculated with regards to the screen resolution
            int len = _stepsValues.Count;
            float xFactor = 38.0f / len;
            float yFactor = 20.0f / _maxValue;

            Vector3 pos = Vector3.zero;
            _lineOutput.positionCount = len;
            for (int i = 0; i < len; i++)
            {
                pos.x = i * xFactor + 1; // Small offset from the left of the screen (+1)
                pos.y = _stepsValues[i].Count * yFactor - 10.0f; // Shift the curve to the bottom of the screen (-10)
                _lineOutput.SetPosition(i, pos);
            }

        }

        /// <summary>
        /// This callback is triggered when it is necessary to hide the line renderer (graphic result)
        /// </summary>
        private void HandleCleanRequest()
        {
            if (_lineOutput != null)
                _lineOutput.gameObject.SetActive(false);
        }


    }
}

