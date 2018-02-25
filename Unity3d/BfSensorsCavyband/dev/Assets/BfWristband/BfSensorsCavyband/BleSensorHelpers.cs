using System;


namespace BfWristband
{
    internal static class BleSensorHelpers
    {
        /// <summary>
        /// This method return the bit value for a given byte and a given bit index (Caution : Big Endian standard !)
        /// </summary>
        /// <param name="valueToCheck">the byte value we want to process</param>
        /// <param name="bitToCheck">the bit index in the byte value (LSB = 0)</param>
        /// <returns>true if bit = 1 or false if bit = 0</returns>
        /// <example>if valueToCheck = "0x6E" = 0110 1110, this method returns false for bitToCheck = 0,4 or 7 and true for bitToCheck = 1,2,3,5 or 6 </example>
        internal static bool IsBitOn(byte valueToCheck, int bitToCheck)
        {
            return ((valueToCheck >> bitToCheck) & 1) != 0;
        }


        /// <summary>
        /// This method calculate a DateTime using date values treated by the Wristband firmware
        /// </summary>
        /// <param name="dayId">1 = Yesterday, 2 = today</param>
        /// <param name="minutes">Time of the day is expressed in minutes or tens of minutes in firmware</param>
        /// <returns>a System.DateTime value</returns>
        internal static DateTime GetDateTime(int dayId, int minutes)
        {
            dayId = dayId < 2 ? 1 : 2; // 1 = Yesterday, 2 = today

            DateTime now = DateTime.Now;
            DateTime calculatedDate = new DateTime(now.Year, now.Month, now.Day, minutes / 60, minutes % 60, 0) - TimeSpan.FromDays(2 - dayId);

            return calculatedDate;

        }


    }
}
