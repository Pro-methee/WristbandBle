using System;
using System.Collections.Generic;

using BfWristband;

namespace BfWristband.Api
{
    /// <summary>
    /// This class is used for regrouping Events called when making a request to a Wristband device or when receiving the resulting data
    /// </summary>
    /// <remarks>The first string parameter should be used to send the device ID</remarks>
    public static class EventsHandler
    {
        #region Sending request

        /// <summary>
        /// Events fired when making a System Info request to a Wristband
        /// </summary>
        public static Action<string>        OnSystemInfoRequested;

        /// <summary>
        /// Events fired when making a Time Info request to a Wristband (Get or Set)
        /// </summary>
        public static Action<string, bool>  OnTimeSystemRequested;

        /// <summary>
        /// Events fired when asking a Wristband to enter/exit Game Mode
        /// </summary>
        public static Action<string, bool>  OnGameModeRequested;

        /// <summary>
        /// Events fired when asking a Wristband to enter/exit Steps Measurement Mode
        /// </summary>
        public static Action<string, bool>  OnStepsModeRequested;

        /// <summary>
        /// Events fired when asking a Wristband to yield result of Steps measurement
        /// </summary>
        public static Action<string, StepsDataSettings> OnStepsResultRequested;

        /// <summary>
        /// Events fired when asking a Wristband to vibrate
        /// </summary>
        public static Action<string>        OnVibrationRequested;

        #endregion Sending Request

        #region Receiving result from request

        /// <summary>
        /// Events fired when a System Info request has been made and results are received
        /// </summary>
        public static Action<string, DeviceInfo> OnSystemInfoReceived;

        /// <summary>
        /// Events fired when a Time System Info request has been made and results are received
        /// </summary>
        public static Action<string, TimeInfo>   OnTimeInfoReceived;

        /// <summary>
        /// Events fired when a OnStepsResultRequested has been sent and results are received
        /// </summary>
        public static Action<string, byte[]>     OnStepsResultReceived;

        #endregion Receiving result from request

    }
}

