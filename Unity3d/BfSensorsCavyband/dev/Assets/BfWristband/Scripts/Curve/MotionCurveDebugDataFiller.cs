using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionCurveDebugDataFiller : MonoBehaviour {


    public float minTime = 1f;
    public float maxTime = 1.5f;

    public float minValue = -20f;
    public float maxValue = 20f;

    public TimedFloatCurve curve = null;

    private float _currentTime = 0f;
    private float _targetTime = 0f;

    public void SetDataSource(TimedFloatCurve a_curve)
    {
        curve = a_curve;
    }

	// Update is called once per frame
	void Update ()
    {
        _currentTime += Time.deltaTime;
        if (_currentTime >= _targetTime)
        {
            AddValue();
            _currentTime -= _targetTime;
            _targetTime = Random.Range(minTime, maxTime);
        }
	}

    void AddValue()
    {
        float a_value = Random.Range(minValue, maxValue);
        curve.AddValue(_targetTime, a_value);
    }
}
