using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct TimeSettings
{
    public int Day;
    public int TimeInMinutes;
    public DateTime Start;

     public TimeSettings(int day, int hour, int minutes)
    {
        this.Day             = day;
        this.TimeInMinutes   = hour * 6 + minutes / 10;

        var now     = DateTime.Now;
        this.Start  = new DateTime(now.Year, now.Month, now.Day, hour, minutes, 0) - TimeSpan.FromDays(2 - day);
    }
}

public struct DataFrame
{
    public byte Counter;
    public byte[] Data;
}

public struct StepsData
{
    public int      Day;
    public int      Milestone;
    public DateTime Time;
    public int      Count;

    public StepsData(int day, DateTime start, int milestone, int count)
    {
        this.Day        = day;
        this.Milestone  = milestone;
        this.Count      = count;

        this.Time = start + TimeSpan.FromMinutes(milestone * 5);
    }

}

public class StepsHandler : MonoBehaviour {

    internal static Action<ECategory, bool> OnStepsResultRequested;
    internal static Action<List<StepsData>> OnStepsResultEnded;

    internal static TimeSettings CurrentTimeSettings;

    [Header("Reference Time Settings")]
    [SerializeField]
    private Dropdown _dropList;

    [SerializeField]
    private InputField _hoursInput;

    [SerializeField]
    private InputField _minutesInput;

    [SerializeField]
    private Toggle _reverseCommand;

    private int _startingHour;
    private int _startingMinutes;

    private static List<StepsData> _stepsValues = new List<StepsData>();
    private static byte _currentFrameId = 0x00;

    private void Awake()
    {
        if(_dropList == null)
            _dropList = GetComponentInChildren<Dropdown>();

        if (_reverseCommand == null)
            _reverseCommand = GetComponentInChildren<Toggle>();

    }

    public void OnValidButton()
    {

        if (Int32.TryParse(_hoursInput.text, out _startingHour) && Int32.TryParse(_minutesInput.text, out _startingMinutes))
        {
            CurrentTimeSettings = new TimeSettings(_dropList.value, _startingHour, _startingMinutes);
            if (MenuController.OnRequest != null)
            {
                MenuController.OnRequest(ECategory.StepsResult, _reverseCommand.isOn);
            }
        }
        else
        {
            Debug.LogError("Time input is not in adequate format for <b>StepsHandler</b>");
            return;
        }

        gameObject.SetActive(false);
    }

    public static void ClearData()
    {
        _stepsValues.Clear();
    }

    /// <summary>
    /// Parse a Sync Data Frame with format like :'$ DA 01 05 01 00 04 BE 02 00 00 AF 03 00 01 87 04 00 03 AA'
    /// meaning 'Header(DA)-day(0-2)-blockId(0-0x6C)-SubId(1-0x90)-TiltCount-StepsCount-StepsCount-blockId(0-0x6C)-SubId(1-0x90)-etc 
    /// </summary>
    /// <param name="rawDataFrame"></param>
    public static void AddStepsValue(object a_sensor, Queue<byte[]> rawDataBuffer)
    {
        while (rawDataBuffer.Count > 0)
        {
            byte[] rawDataFrame = rawDataBuffer.Dequeue();

            Debug.Log("StepsHandler::AddStepsValue with " + BitConverter.ToString(rawDataFrame));

            if (rawDataFrame.Length < 20 || rawDataFrame[1] != 0xDA)
            {
                Debug.LogWarning("Incorrect data in StepsHandler :: AddStepsValue with " + BitConverter.ToString(rawDataFrame));
                continue;
            }

            if (rawDataFrame[2] == 0xFF && rawDataFrame[3] == 0xFF)
            {
                Debug.Log("Ending data in StepsHandler :: AddStepsValue");

                if(OnStepsResultEnded != null && _stepsValues != null)
                    OnStepsResultEnded(_stepsValues);

                _currentFrameId = 0x00;
                return;
            }

            if (_currentFrameId == 0x00)
                ClearData();

            if (rawDataFrame[3] == _currentFrameId)
            {
                Debug.Log("Redundant Frame ID in StepsHandler :: AddStepsValue (Id = " + _currentFrameId + ")");
                return;
            }
            _currentFrameId = rawDataFrame[3];
            //Debug.Log("Current Frame Id : " + _currentFrameId);

            int day = (int)rawDataFrame[2];
            //Debug.Log("newData.Day : " + newData.Day);
            int milestone, count;

            for (int i = 4; i < 17; i+= 4)
            {
                milestone = (short)(0x00 | rawDataFrame[i]);
                //Debug.Log(i + " - newData.Time : " + milestone);

                count = (short)(rawDataFrame[i + 2] << 8 | rawDataFrame[i + 3]);
                //Debug.Log(i + " - newData.Count : " + count);
                StepsData sd = new StepsData(day, CurrentTimeSettings.Start, milestone, count);
                _stepsValues.Add(sd);
            }
            
        }

    }

    
}
