using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CurveToAnalise
{
    X= 0,
    Y,
    Z
}

internal struct PicInfo
{
    internal bool IsAsending;
    internal CurveToAnalise axis;
}

internal class CurveAnaliser
{
    LinkedList<float> lastPointOfCurve;
    int lenghtOfAnalise = 6;
    CurveToAnalise curve;

    PicInfo picInfo;

    static internal System.Action<PicInfo> OnPicDetected;

    internal void StartRecordingCurve(CurveToAnalise axis)
    {
        curve = axis;
        lastPointOfCurve = new LinkedList<float>();
        RegisterEvents();
    }

    void UpdateCurve(float newValue)
    {
        lastPointOfCurve.AddLast(newValue);
        if (lastPointOfCurve.Count > lenghtOfAnalise)
        {
            lastPointOfCurve.RemoveFirst();
            AnaliseCurve();
        }
    }

    void AnaliseCurve ()
    {
        if ( Mathf.Abs( lastPointOfCurve.First.Value) > Mathf.Abs(lastPointOfCurve.Last.Value)+1f)
        {
            Debug.Log(lastPointOfCurve.First.Value + "     " + lastPointOfCurve.Last.Value);

            picInfo.axis = curve;

            if (Mathf.Sign(lastPointOfCurve.First.Value)> 0)
            {
                picInfo.IsAsending = true;
            }
            else
            {
                picInfo.IsAsending = false;
            }

            //string val = "Pic detected on " + picInfo.axis + "axis" + "\n"
            //        + "was assending = " + picInfo.IsAsending + "\n" 
            //        + "pente = " + GetSlope();
            //_debugManager.DisplayDebugInfo(val, 3);

            if (OnPicDetected != null)
                OnPicDetected(picInfo);
            //StopRecordingCurve();
        }
    }

    internal PicInfo GetPicInfo () { return picInfo; }

    internal float GetSlope ()
    {
        float slope;
        slope = (lastPointOfCurve.First.Value - lastPointOfCurve.Last.Value) / (lenghtOfAnalise - 1); // (yb - ya) / (xb-xa) ou y = acseleration et x = nb frame
        return slope;
    }

    internal void StopRecordingCurve()
    {
        UnRegisterEvents();
    }

    void UpdateAccelerationValues(Vector3 newAcceleration)
    {
        UpdateCurve(newAcceleration[(int)curve]);
    }

    void RegisterEvents()
    {
        //Sensor.OnAccelerationChanged += UpdateAccelerationValues;
    }

    void UnRegisterEvents()
    {
        //Sensor.OnAccelerationChanged -= UpdateAccelerationValues;
    }
}
