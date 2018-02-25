using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimedFloatCurveAnalyser
{
    #region Average
    public static float AverageValueOverTime(TimedFloatCurve a_curve, float a_timeInSec)
    {
        float average = 0f;
        int processedValueCount = 0;
        float totalTimeElpased = 0f;

        if (a_timeInSec < 0f)
        {
            Debug.LogError("AverageOverTime - Negative time window given : " + a_timeInSec.ToString());
            return 0f;
        }

        LinkedListNode<TimedValue<float>> current = a_curve.Values.Last;
        while (current != null)
        {
            average += current.Value.value;
            processedValueCount++;
            totalTimeElpased += current.Value.deltatime;
            if (totalTimeElpased >= a_timeInSec)
                break;

            current = current.Previous;
        }

        if (processedValueCount > 0)
            average = average / (float)processedValueCount;

        return average;
    }

    public static float AverageValueOverValues(TimedFloatCurve a_curve, int a_valueCount)
    {
        float average = 0f;

        int processedValueCount = 0;

        if (a_valueCount < 0)
        {
            Debug.LogError("AverageOverLastValues - Negative count given : " + a_valueCount.ToString());
            return 0f;
        }

        LinkedListNode<TimedValue<float>> current = a_curve.Values.Last;
        while (current != null)
        {
            average += current.Value.value;
            processedValueCount++;
            if (processedValueCount >= a_valueCount)
                break;

            current = current.Previous;
        }

        if (processedValueCount > 0)
            average = average / (float)processedValueCount;

        return average;
    }
    #endregion

    #region Slope
    public static float AverageSlopeOverTime(TimedFloatCurve a_curve, float a_timeInSec)
    {
        float slope = 0f;
        int processedValueCount = 0;
        float totalTimeElpased = 0f;

        if (a_timeInSec < 0f)
        {
            Debug.LogError("AverageSlopeOverTime - Negative time window given : " + a_timeInSec.ToString());
            return 0f;
        }

        LinkedListNode<TimedValue<float>> current = a_curve.Values.Last;
        while (current != null && current.Previous != null)
        {
            slope += (current.Value.value - current.Previous.Value.value) / current.Value.deltatime;
            processedValueCount++;
            totalTimeElpased += current.Value.deltatime;
            if (totalTimeElpased >= a_timeInSec)
                break;

            current = current.Previous;
        }

        Debug.LogError("Processed Value Count Time : " + processedValueCount.ToString());
        if (processedValueCount > 0)
            slope = slope / (float)processedValueCount;

        return slope;
    }

    public static float AverageSlopeOverValues(TimedFloatCurve a_curve, int a_valueCount)
    {
        float slope = 0f;
        int processedValueCount = 0;

        if (a_valueCount < 0)
        {
            Debug.LogError("AverageSlopeOverValues - Negative count given : " + a_valueCount.ToString());
            return 0f;
        }

        LinkedListNode<TimedValue<float>> current = a_curve.Values.Last;
        while (current != null && current.Previous != null)
        {
            slope += (current.Value.value - current.Previous.Value.value) / current.Value.deltatime;
            processedValueCount++;
            if (processedValueCount >= a_valueCount)
                break;

            current = current.Previous;
        }

        Debug.LogError("Processed Value Count Time : " + processedValueCount.ToString());
        if (processedValueCount > 0)
            slope = slope / (float)processedValueCount;

        return slope;
    }

    public static float CurrentSlope(TimedFloatCurve a_curve)
    {
        float slope = 0f;

        LinkedListNode<TimedValue<float>> current = a_curve.Values.Last;
        if (current != null && current.Previous != null)
            slope += (current.Value.value - current.Previous.Value.value) / current.Value.deltatime;

        return slope;
    }
    #endregion

    #region Peak
    public static float MaxOverTime(TimedFloatCurve a_curve, float a_timeInSec)
    {
        float max = float.MinValue;
        float totalTimeElpased = 0f;

        if (a_timeInSec < 0f)
        {
            Debug.LogError("MaxOverTime - Negative time window given : " + a_timeInSec.ToString());
            return 0f;
        }

        LinkedListNode<TimedValue<float>> current = a_curve.Values.Last;
        while (current != null && current.Previous != null)
        {
            max = Mathf.Max(max, current.Value.value);
            totalTimeElpased += current.Value.deltatime;
            if (totalTimeElpased >= a_timeInSec)
                break;

            current = current.Previous;
        }

        return max;
    }

    public static float MaxOverValues(TimedFloatCurve a_curve, int a_valueCount)
    {
        float max = float.MinValue;
        int processedValueCount = 0;

        if (a_valueCount < 0)
        {
            Debug.LogError("MaxOverValues - Negative count given : " + a_valueCount.ToString());
            return 0f;
        }

        LinkedListNode<TimedValue<float>> current = a_curve.Values.Last;
        while (current != null)
        {
            max = Mathf.Max(max, current.Value.value);
            processedValueCount++;
            if (processedValueCount >= a_valueCount)
                break;

            current = current.Previous;
        }

        return max;
    }

    public static float MinOverTime(TimedFloatCurve a_curve, float a_timeInSec)
    {
        float min = float.MaxValue;
        float totalTimeElpased = 0f;

        if (a_timeInSec < 0f)
        {
            Debug.LogError("MinOverTime - Negative time window given : " + a_timeInSec.ToString());
            return 0f;
        }

        LinkedListNode<TimedValue<float>> current = a_curve.Values.Last;
        while (current != null && current.Previous != null)
        {
            min = Mathf.Min(min, current.Value.value);
            totalTimeElpased += current.Value.deltatime;
            if (totalTimeElpased >= a_timeInSec)
                break;

            current = current.Previous;
        }

        return min;
    }

    public static float MinOverValues(TimedFloatCurve a_curve, int a_valueCount)
    {
        float min = float.MaxValue;
        int processedValueCount = 0;

        if (a_valueCount < 0)
        {
            Debug.LogError("MinOverValues - Negative count given : " + a_valueCount.ToString());
            return 0f;
        }

        LinkedListNode<TimedValue<float>> current = a_curve.Values.Last;
        while (current != null)
        {
            min = Mathf.Min(min, current.Value.value);
            processedValueCount++;
            if (processedValueCount >= a_valueCount)
                break;

            current = current.Previous;
        }

        return min;
    }
    #endregion
}
