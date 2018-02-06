using System;
using UnityEngine;

// A curve manage a LineRenderer in order to give a graphical representation of the acceleration over time
public class Curve : MonoBehaviour {

    // Keep track whether or not the gameobject is enable/disable
    internal static System.Action<bool> OnActivityModified;

    // Freezes the values when paused
    internal bool IsPaused = false;

    // Resolution
    private Vector3[] _baseLineValues;
    private int _numberOfValues = 800;

    // Id in { Unindentified, AccX, AccY, AccZ, SpeedX, SpeedY, SpeedZ}
    public ECurveId  Id               = ECurveId.Unindentified;

    // The graphical Unity tool
    private LineRenderer _lineRenderer = null;

    // Ordinate value displayed by the line renderer
    private Vector3      _unitValue    = Vector3.zero;

    // Inverse value of the resolution, calculated once and for all
    private float   _increment    = 0f;

    // Offset of the line in order to center it on the screen
    private float   _startX       = 0f;
    
    // buffer value to be displayed by the curve
    private float   _currentValue = 0f;

    internal bool IsActive {
        get {
            return gameObject.activeInHierarchy;
        }
    }

    // Use this for initialization
    internal int Init (int resolutionFactor, float startLeft) {
        Debug.Log("<b>Curve</b> Init");

        _unitValue  = Vector3.zero;

        // Initialization of the curve parameters
        _increment  = 1.0f / resolutionFactor;
        _startX     = startLeft;
        _numberOfValues = (int)(-2 * startLeft * resolutionFactor);

        // Get the reference on he line renderer
        if (_lineRenderer == null)
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        // Initialization of the line renderer in order to have the correct resolution (number of points) 
        // and to position the points properly from the start
        try
        {
            _lineRenderer.numPositions = _numberOfValues;
            _baseLineValues            = new Vector3[_numberOfValues];

            for (int i = 0; i < _numberOfValues; i++)
            {
                _unitValue.x = _startX + i * _increment;
                _baseLineValues[i] = _unitValue;
            }

            _lineRenderer.SetPositions(_baseLineValues);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }

        return _numberOfValues;
    }

    internal void SetValue(float newValue) {

        _currentValue = newValue;
    }


    // Update a particular point on the curve
    // index on curve give the absciss whereas the ordinate is set from the _currentValue
	internal void DoUpdate (int indexOnCurve) {

        if (indexOnCurve < 0 || !IsActive || IsPaused)
            return;

        if(indexOnCurve == 0)
        {
            _lineRenderer.SetPositions(_baseLineValues);
        }

        _unitValue.x = _startX + indexOnCurve * _increment;
        _unitValue.y = _currentValue;

        _lineRenderer.SetPosition(indexOnCurve, _unitValue);

    }

    private void OnEnable() {

        if(OnActivityModified != null)
            OnActivityModified(true);
    }

    private void OnDisable() {

        if (OnActivityModified != null)
            OnActivityModified(false);
    }
}
