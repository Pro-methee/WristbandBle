using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

internal class DebugSensorManager : MonoBehaviour
{
    #region Inspector Properties
    [Header("UI Ref")]
    public UnityEngine.UI.Text pauseLabel = null;
    public UnityEngine.UI.Toggle toggleAxisX = null;
    public UnityEngine.UI.Toggle toggleAxisY = null;
    public UnityEngine.UI.Toggle toggleAxisZ = null;
    public UnityEngine.UI.Toggle toggleAxisZX = null;

    public UnityEngine.UI.Toggle toggleNpad1 = null;
    public UnityEngine.UI.Toggle toggleNpad2 = null;

    [Header("Npad feedback component")]
    public SwitchPadCurveManager npad1 = null;
    public SwitchPadCurveManager npad2 = null;

    public Camera curveCamera = null;
    public CurveSnapshotWidget snapshotWidget = null;
    #endregion


    #region Properties
    protected bool _axisX = true;
    protected bool _axisY = true;
    protected bool _axisZ = true;
    protected bool _axisZX = true;

    protected bool _accel = true;
    protected bool _speed = false;

    protected bool _isPaused = false;

    protected bool _isNullRefExceptionRaised = false;
    #endregion

    void Awake()
    {
        toggleNpad1.isOn = npad1.IsActive;
        toggleNpad2.isOn = npad2.IsActive;

        toggleAxisX.isOn = _axisX;
        toggleAxisY.isOn = _axisY;
        toggleAxisZ.isOn = _axisZ;
        toggleAxisZX.isOn = _axisZX;

        SetLive();
    }

    void Update()
    {
        if (_isNullRefExceptionRaised)
            return;
#if UNITY_EDITOR
        try
        {
            npad1.accelRendererX.GetComponent<MotionCurveDebugDataFiller>().enabled = true;
            npad1.accelRendererX.GetComponent<MotionCurveDebugDataFiller>().SetDataSource(npad1.AccelX);
            npad1.accelRendererY.GetComponent<MotionCurveDebugDataFiller>().enabled = true;
            npad1.accelRendererY.GetComponent<MotionCurveDebugDataFiller>().SetDataSource(npad1.AccelY);
            npad1.accelRendererZ.GetComponent<MotionCurveDebugDataFiller>().enabled = true;
            npad1.accelRendererZ.GetComponent<MotionCurveDebugDataFiller>().SetDataSource(npad1.AccelZ);
            npad1.accelRendererMag.GetComponent<MotionCurveDebugDataFiller>().enabled = true;
            npad1.accelRendererMag.GetComponent<MotionCurveDebugDataFiller>().SetDataSource(npad1.AccelMag);

            npad2.accelRendererX.GetComponent<MotionCurveDebugDataFiller>().enabled = true;
            npad2.accelRendererX.GetComponent<MotionCurveDebugDataFiller>().SetDataSource(npad2.AccelX);
            npad2.accelRendererY.GetComponent<MotionCurveDebugDataFiller>().enabled = true;
            npad2.accelRendererY.GetComponent<MotionCurveDebugDataFiller>().SetDataSource(npad2.AccelY);
            npad2.accelRendererZ.GetComponent<MotionCurveDebugDataFiller>().enabled = true;
            npad2.accelRendererZ.GetComponent<MotionCurveDebugDataFiller>().SetDataSource(npad2.AccelZ);
            npad2.accelRendererMag.GetComponent<MotionCurveDebugDataFiller>().enabled = true;
            npad2.accelRendererMag.GetComponent<MotionCurveDebugDataFiller>().SetDataSource(npad2.AccelMag);
        }
        catch(System.ArgumentNullException e)
        {
            Debug.LogWarning(e.Message);
            _isNullRefExceptionRaised = true;
        }

#endif
    }

    #region Toggle
    public void ToggleAxisX()
    {
        _axisX = !_axisX;
        npad1.SetAxisXVisible(_axisX);
        npad2.SetAxisXVisible(_axisX);
    }

    public void ToggleAxisY()
    {
        _axisY = !_axisY;
        npad1.SetAxisYVisible(_axisY);
        npad2.SetAxisYVisible(_axisY);
    }

    public void ToggleAxisZ()
    {
        _axisZ = !_axisZ;
        npad1.SetAxisZVisible(_axisZ);
        npad2.SetAxisZVisible(_axisZ);
    }

    public void ToggleAxisZX()
    {
        _axisZX = !_axisZX;
        npad1.SetAxisZXVisible(_axisZX);
        npad2.SetAxisZXVisible(_axisZX);
    }

    public void TogglePad1()
    {
        npad1.IsActive = !npad1.IsActive;
    }

    public void TogglePad2()
    {
        npad2.IsActive = !npad2.IsActive;
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        if (_isPaused)
            SetPaused();
        else
            SetLive();
    }

    void SetPaused()
    {
        _isPaused = true;
        pauseLabel.text = "Resume";
        npad1.PauseRecording();
        npad2.PauseRecording();
    }

    void SetLive()
    {
        _isPaused = false;
        pauseLabel.text = "Pause";
        npad1.ResumeRecording();
        npad2.ResumeRecording();

        snapshotWidget.SetIndicatorVisible(false);
        snapshotWidget.SetValuesWindowVisible(false);
    }
    #endregion

    public void OnSnapshot(BaseEventData a_event)
    {
        PointerEventData pointerEvent = a_event as PointerEventData;
        float xRatio = pointerEvent.position.x / (float)Screen.width;

        TimedFloatCurveRenderer activeCurve = null;
        if (npad1.IsActive)
        {
            if (npad1.accelRendererX.IsActive)
                activeCurve = npad1.accelRendererX;
            else if (npad1.accelRendererY.IsActive)
                activeCurve = npad1.accelRendererY;
            else if (npad1.accelRendererZ.IsActive)
                activeCurve = npad1.accelRendererZ;
            else if (npad1.accelRendererMag.IsActive)
                activeCurve = npad1.accelRendererMag;
        }
        else if(npad2.IsActive)
        {
            if (npad2.accelRendererX.IsActive)
                activeCurve = npad2.accelRendererX;
            else if (npad2.accelRendererY.IsActive)
                activeCurve = npad2.accelRendererY;
            else if (npad2.accelRendererZ.IsActive)
                activeCurve = npad2.accelRendererZ;
            else if (npad2.accelRendererMag.IsActive)
                activeCurve = npad2.accelRendererMag;
        }

        if (activeCurve != null)
        {
            SetPaused();
            float xStart = curveCamera.WorldToViewportPoint(activeCurve.FirstWorldPosition).x;
            float xEnd = curveCamera.WorldToViewportPoint(activeCurve.LastWorldPosition).x;

            //xRatio = Mathf.Clamp(xRatio, xStart, xEnd);
            xRatio = Mathf.Clamp01((xRatio - xStart) / (xEnd - xStart));

            float closestRatio = 0f;

            TimedValue<float> val = null;

            val = npad1.accelRendererX.DataSource.ValueForCurvePercent(xRatio, ref closestRatio);
            float x1 = val != null ? val.value : 0f;

            val = npad1.accelRendererY.DataSource.ValueForCurvePercent(xRatio, ref closestRatio);
            float y1 = val != null ? val.value : 0f;

            val = npad1.accelRendererZ.DataSource.ValueForCurvePercent(xRatio, ref closestRatio);
            float z1 = val != null ? val.value : 0f;

            val = npad1.accelRendererMag.DataSource.ValueForCurvePercent(xRatio, ref closestRatio);
            float zx1 = val != null ? val.value : 0f;

            val = npad2.accelRendererX.DataSource.ValueForCurvePercent(xRatio, ref closestRatio);
            float x2 = val != null ? val.value : 0f;

            val = npad2.accelRendererY.DataSource.ValueForCurvePercent(xRatio, ref closestRatio);
            float y2 = val != null ? val.value : 0f;

            val = npad2.accelRendererZ.DataSource.ValueForCurvePercent(xRatio, ref closestRatio);
            float z2 = val != null ? val.value : 0f;

            val = npad2.accelRendererMag.DataSource.ValueForCurvePercent(xRatio, ref closestRatio);
            float zx2 = val != null ? val.value : 0f;

            activeCurve.DataSource.ValueForCurvePercent(xRatio, ref closestRatio);

            snapshotWidget.SetSnapshotValues(x1, y1, z1, zx1, x2, y2, z2, zx2);

            float uiRatio = Mathf.Lerp(xStart, xEnd, xRatio);
            snapshotWidget.SetIndicatorRatio(uiRatio);
            snapshotWidget.SetIndicatorVisible(true);
            snapshotWidget.SetValuesWindowVisible(true);
        }
    }
}
