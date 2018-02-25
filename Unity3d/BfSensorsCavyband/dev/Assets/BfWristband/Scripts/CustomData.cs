using System;

namespace BfWristband
{

    #region enumerations
    internal enum ESensorState { Idle, Scanning, Standby, GameMode, Disconnected };
    internal enum EDay         { Monday = 1, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday }
    #endregion enumerations

    #region Structures

    /// <summary>
    /// System info from a Wristband
    /// </summary>
    public struct DeviceInfo
    {
        // Not Used (Can be used for storing raw data returned from a request)
        internal string RawData;

        // Useless : Always equals to 5
        internal byte State;

        // Bitmask holding the following states
        internal byte func;

        // Functions states from the bitmask
        internal bool TimeEnabled;
        internal bool AlarmEnabled;
        internal bool LlaEnabled;   // If true, the band is vibrating when disconnected
        internal bool TiltEnabled;  // If true, keep tracks of the number of time the user is laying down
        internal bool StepsEnabled; // If true, keep tracks of the number of steps the user is doing during periods of 10 minutes

        // Versions
        internal byte HardwareVersion;
        internal byte FirmwareVersion;

        // Factory Calibration values
        internal bool IsCalibrated;
        internal int  MagFactoryOffsetX;
        internal int  MagFactoryOffsetY;
        internal int  MagFactoryOffsetZ;
        internal uint MagFactoryOffsetRadius;

        public override string ToString()
        {
            string desc;

            desc  = "State       : " + this.State + "\n";
            desc += "functions   : " + this.func + "\n";
            desc += "Time  on ?  : " + this.TimeEnabled + "\n";
            desc += "Alarm  on ? : " + this.AlarmEnabled + "\n";
            desc += "Lla  on ?   : " + this.LlaEnabled + "\n";
            desc += "Tilts on ?  : " + this.TiltEnabled + "\n";
            desc += "Steps on ?  : " + this.StepsEnabled + "\n";
            desc += "Hardware    : " + this.HardwareVersion + "\n";
            desc += "Firmware    : " + this.FirmwareVersion + "\n";
            desc += "Calibrated  : " + this.IsCalibrated + "\n";
            desc += "Mag X-Offset: " + this.MagFactoryOffsetX + "\n";
            desc += "Mag Y-Offset: " + this.MagFactoryOffsetY + "\n";
            desc += "Mag Z-Offset: " + this.MagFactoryOffsetZ + "\n";
            desc += "Mag Radius  : " + this.MagFactoryOffsetRadius + "\n";

            return desc;
        }

    }

    /// <summary>
    /// Wristband firmware is keeping track of the day of the week and the time of the day, expressed in minutes (600 = 10h00 AM)
    /// </summary>
    public struct TimeInfo
    {
        internal byte data1;
        internal byte data2;
        internal EDay Weekday;

        public TimeInfo(byte data1, byte data2, byte data3)
        {
            this.data1 = data1; // hours
            this.data2 = data2; // minutes

            this.Weekday = (EDay)data3;
        }

        public override string ToString()
        {
            int hours   = (int)(this.data1 << 8 | this.data2) / 60;
            int minutes = (int)(this.data1 << 8 | this.data2) % 60;

            string desc;
            desc  = this.Weekday + "\n";
            desc += String.Format("{0:00}:{1:00}", hours, minutes);

            return desc;
        }
    }

    /// <summary>
    /// This structure store data that are used for calculated the steps : This is the starting time of the measurement
    /// </summary>
    public struct StepsDataSettings
    {
        public int      DayId;          // '1' = Yesterday, '2' = Today
        public int      TimeInMinutes;  // The number of minutes since midnight
        public DateTime Start;          // The DateTime that is calculated from the 2 others information

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="day">'1' = Yesterday, '2' = Today</param>
        /// <param name="hour">The hour of the day</param>
        /// <param name="minutes">The minutes in the hour</param>
        public StepsDataSettings(int day, int hour, int minutes)
        {
            this.DayId          = day;
            this.TimeInMinutes  = hour * 6 + minutes / 10;

            var now    = DateTime.Now;
            this.Start = new DateTime(now.Year, now.Month, now.Day, hour, minutes, 0) - TimeSpan.FromDays(2 - day);
        }
    }

    /// <summary>
    /// This structure store steps count data for a 10 minutes period
    /// </summary>
    public struct StepsData
    {
        public int Day;         // '1' = Yesterday, '2' = Today
        public int Milestone;   // a block number that must be multiplied by 10 to get the number of minutes since midnight 
        public DateTime Time;   // The time of measurement expressed as a real Date Time
        public int Count;       // The number of steps performed during this 10 minutes period

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="day">'1' = Yesterday, '2' = Today</param>
        /// <param name="milestone">a block number that must be multiplied by 10 to get the number of minutes since midnight </param>
        /// <param name="count">The number of steps performed during this 10 minutes period</param>
        public StepsData(int day, int milestone, int count)
        {
            this.Day = day;
            this.Milestone = milestone;
            this.Count = count;

            this.Time = BleSensorHelpers.GetDateTime(day, milestone * 10);
        }

        public override string ToString()
        {
            return String.Format("{0:f} ({1},{2}) : {3} steps", this.Time, this.Day, this.Milestone, this.Count);
        }

    }
    #endregion Structures
}
