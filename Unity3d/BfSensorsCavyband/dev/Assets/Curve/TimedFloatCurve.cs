using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedValue<T>
{
    public float deltatime = 0f;
    public T value;

    public TimedValue(T a_value, float a_deltatime)
    {
        value = a_value;
        deltatime = a_deltatime;
    }
}

public class TimedFloatCurve
{
    public static float NsToSeconds(long a_nsCount)
    {
        return a_nsCount * Mathf.Pow(10f, -9f);
    }

    public delegate void TimedFloatCurveValueCallback(TimedFloatCurve a_curve, TimedValue<float> a_newValues);
    public delegate void TimedFloatCurveCallback(TimedFloatCurve a_curve);

    #region Properties
    private LinkedList<TimedValue<float>> _values;
    public LinkedList<TimedValue<float>> Values
    {
        get
        {
            return _values;
        }
    }

    public TimedFloatCurveValueCallback onValueAdded = null;
    public TimedFloatCurveValueCallback onValueTimedout = null;
    public TimedFloatCurveCallback onValuesChanged = null;

    private float _lifespan = 0.5f;// evaluated range in second.
    internal float LifeSpan
    {
        get
        {
            return _lifespan;
        }
        set
        {
            _lifespan = value;
        }
    }

    protected float _totalDeltatime = 0f;
    public float TotalLifespan
    {
        get
        {
            return _totalDeltatime;
        }
    }
    #endregion

    #region Interface
    internal TimedFloatCurve(float a_lifespan)
    {
        _lifespan = a_lifespan;
        _values = new LinkedList<TimedValue<float>>();
    }

    internal void ClearData()
    {
        if (_values != null)
        {
            _values.Clear();
            _totalDeltatime = 0f;

            if (onValuesChanged != null)
                onValuesChanged(this);
        }
    }
    
    internal void AddValue(long a_nsDeltatime, float a_newValue)
    {
        float deltaTime = NsToSeconds(a_nsDeltatime);
        AddValue(deltaTime, a_newValue);
    }

    internal void AddValue(float a_deltaTime, float a_newValue)
    {
        _totalDeltatime += a_deltaTime;
        TimedValue<float> addedValue = new TimedValue<float>(a_newValue, a_deltaTime);

        LinkedListNode<TimedValue<float>> oldest = _values.First;
        while (oldest != null && _totalDeltatime >= _lifespan)
        {
            _values.RemoveFirst();
            _totalDeltatime = _totalDeltatime - oldest.Value.deltatime;

            if (onValueTimedout != null)
                onValueTimedout(this, oldest.Value);

            oldest = oldest.Next;
        }

        _values.AddLast(addedValue);

        if (onValueAdded != null)
            onValueAdded(this, addedValue);

        if (onValuesChanged != null)
            onValuesChanged(this);
    }
    #endregion

    #region ClosestValue
    public TimedValue<float> ValueForCurvePercent(float a_targetTimePercent, ref float a_closestTime)
    {
        float targetElapsedTime = a_targetTimePercent * _lifespan;
        return ClosestValueForTimeElapsed(targetElapsedTime, ref a_closestTime);
    }

    public TimedValue<float> ClosestValueForTimeElapsed(float a_timeElapsed, ref float a_closestTime)
    {
        LinkedListNode<TimedValue<float>> current = _values.First;

        float previousCumulDeltatime = 0f;
        float cumulDeltatime = 0f;
        while (current != null)
        {
            cumulDeltatime += current.Value.deltatime;

            if (a_timeElapsed > cumulDeltatime)
            {
                previousCumulDeltatime = cumulDeltatime;
            }
            else if (Mathf.Abs(a_timeElapsed - cumulDeltatime) > Mathf.Abs(a_timeElapsed - cumulDeltatime))
            {
                a_closestTime = cumulDeltatime;
                return current.Value;
            }
            else
            {
                a_closestTime = previousCumulDeltatime;
                if (current.Previous != null)
                    return current.Previous.Value;
                else
                    return null;
            }

            current = current.Next;
        }

        a_closestTime = previousCumulDeltatime;
        return null;
    }
    #endregion
}