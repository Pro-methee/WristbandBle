using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using BfWristband;
using BfWristband.Api;

internal class SwitchPadCurveManager : MonoBehaviour
{
    #region Inspector Properties
    public int index = 0;
    private BleSensorBase _sensor = null;

    public AnimationCurve strengthCurve = new AnimationCurve();

    public TimedFloatCurveRenderer accelRendererX = null;
    public TimedFloatCurveRenderer accelRendererY = null;
    public TimedFloatCurveRenderer accelRendererZ = null;
    public TimedFloatCurveRenderer accelRendererMag = null;

    public TimedFloatCurveRenderer speedX = null;
    public TimedFloatCurveRenderer speedY = null;
    public TimedFloatCurveRenderer speedZ = null;

    public GameObject controller = null;

    public ToggleIndicator Indicator = null;
    [Range(0, 100f)]
    public float DetectionThreshold = 50f;
    public float AccelerationMagnitude;
    private Text _magnitudeDisplay;

    #endregion

    #region Properties
    protected TimedFloatCurve _accelX = null;
    public TimedFloatCurve AccelX
    {
        get
        {
            return _accelX;
        }
    }
    protected TimedFloatCurve _accelY = null;
    public TimedFloatCurve AccelY
    {
        get
        {
            return _accelY;
        }
    }
    protected TimedFloatCurve _accelZ = null;
    public TimedFloatCurve AccelZ
    {
        get
        {
            return _accelZ;
        }
    }

    protected TimedFloatCurve _accelMag = null;
    public TimedFloatCurve AccelMag
    {
        get
        {
            return _accelMag;
        }
    }

    internal bool IsActive
    {
        get
        {
            return gameObject.activeSelf;
        }
        set
        {
            gameObject.SetActive(value);
        }
    }

    private Quaternion _quaternion = new Quaternion();

    protected bool _isAxisXVisible = true;
    protected bool _isAxisYVisible = true;
    protected bool _isAxisZVisible = true;
    protected bool _isAxisZXVisible = true;

    #endregion

    void Awake()
    {
        if (Indicator == null)
        {
            Indicator = FindObjectOfType<ToggleIndicator>();
        }
        _magnitudeDisplay = Indicator.gameObject.GetComponentInChildren<Text>();

        float yMaxWorldOffset = 10f;
        float yMaxAccel = 6f;

        _accelX = new TimedFloatCurve(2f);
        _accelY = new TimedFloatCurve(2f);
        _accelZ = new TimedFloatCurve(2f);
        _accelMag = new TimedFloatCurve(2f);

        accelRendererX.Init(40f, yMaxAccel, yMaxWorldOffset);
        accelRendererY.Init(40f, yMaxAccel, yMaxWorldOffset);
        accelRendererZ.Init(40f, yMaxAccel, yMaxWorldOffset);
        accelRendererMag.Init(40f, yMaxAccel * 3f, yMaxWorldOffset);

        accelRendererX.SetDataSource(_accelX);
        accelRendererY.SetDataSource(_accelY);
        accelRendererZ.SetDataSource(_accelZ);
        accelRendererMag.SetDataSource(_accelY);

        if (BleSensorsManager.Instance == null) return;

        int i = 0;
        foreach (BleSensorBase each in BleSensorsManager.Instance.SensorsByBleAddress.Values)
        {
            if (index == i)
            {
                _sensor = each;
                break;
            }
            i++;
        }

        if(_sensor != null)
            _sensor.onMotionDataReceived += OnMotionDataReceived;

        /*accelZX.Init(42f, yMaxAccel, yMaxWorldOffset);
        speedX.Init(42f, 0.5f, yMaxWorldOffset);
        speedY.Init(42f, 0.5f, yMaxWorldOffset);
        speedZ.Init(42f, 0.5f, yMaxWorldOffset);*/

    }

    protected void OnMotionDataReceived(BleSensorBase a_sensor, Quaternion a_quaternion, Vector3 a_acc)
    {
        Debug.LogError("Adding Value");

        float deltaTime = Time.deltaTime;

        _accelX.AddValue(deltaTime, a_acc.x);
        _accelY.AddValue(deltaTime, a_acc.y);
        _accelZ.AddValue(deltaTime, a_acc.z);
        _accelMag.AddValue(deltaTime, a_acc.magnitude);

        controller.transform.rotation = a_quaternion;

        AccelerationMagnitude = a_acc.magnitude;
        if(Indicator != null)
        {
            Indicator.Toggle(AccelerationMagnitude >= DetectionThreshold);
            _magnitudeDisplay.text = string.Format("{0}", AccelerationMagnitude);
        }
    }

    public void SetAxisXVisible(bool a_visible)
    {
        _isAxisXVisible = a_visible;
        accelRendererX.gameObject.SetActive(_isAxisXVisible);
        speedX.gameObject.SetActive(_isAxisXVisible);
    }

    public void SetAxisYVisible(bool a_visible)
    {
        _isAxisYVisible = a_visible;
        accelRendererY.gameObject.SetActive(_isAxisYVisible);
        speedY.gameObject.SetActive(_isAxisYVisible);
    }

    public void SetAxisZVisible(bool a_visible)
    {
        _isAxisZVisible = a_visible;
        accelRendererZ.gameObject.SetActive(_isAxisZVisible);
        speedZ.gameObject.SetActive(_isAxisZVisible);
    }

    public void SetAxisZXVisible(bool a_visible)
    {
        _isAxisZXVisible = a_visible;
        accelRendererMag.gameObject.SetActive(_isAxisZXVisible);
    }

    #region Pause
    protected bool _isRecording = true;
    public void PauseRecording()
    {
        if (_isRecording)
        {
            _isRecording = false;
            enabled = false;
        }
    }

    public void ResumeRecording()
    {
        if (!_isRecording)
        {
            _isRecording = true;
            enabled = true;
            _accelX.ClearData();
            _accelY.ClearData();
            _accelZ.ClearData();
            _accelMag.ClearData();
        }
    }

    void OnStrengthDetected(float a_strength)
    {
        float clampedStrength = Mathf.Clamp(a_strength, 0f, 10f);
        Debug.LogError(strengthCurve.Evaluate(clampedStrength).ToString("0.00") + " / " + a_strength.ToString("0.00"));
    }
    #endregion
}
