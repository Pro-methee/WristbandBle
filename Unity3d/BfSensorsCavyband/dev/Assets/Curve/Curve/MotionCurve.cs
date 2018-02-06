using System;
using UnityEngine;
using System.Collections.Generic;


// A curve manage a LineRenderer in order to give a graphical representation of the acceleration over time
public class MotionCurve : MonoBehaviour
{
   /* #region Inspector Properties
    // The graphical Unity tool
    private LineRenderer _lineRenderer = null;
    #endregion

    #region Properties
    //List of ordinate timed value
    private LinkedList<TimedValue<float>> _values;

    public Vector3 FirstWorldPosition
    {
        get
        {
            return transform.position;
        }
    }

    public Vector3 LastWorldPosition
    {
        get
        {
            if (_lineRenderer.positionCount > 1)
            {
                Vector3 pos = _lineRenderer.GetPosition(_lineRenderer.positionCount - 1);
                return transform.position + pos;
            }

            return transform.position;
        }
    }

    public float ValueForRatioX(float a_targetXRatio, ref float a_closestRatio)
    {
        float value = 0f;
        float previousRatio = 0f;

        LinkedListNode<TimedValue<float>> current = _values.First;

        long cumulTime = 0L;
        while (current != null)
        {
            cumulTime += current.Value.deltatime;
            float curveRatio = NsToSeconds(cumulTime) / _lifespan;

            if (a_targetXRatio > curveRatio)
            {
                value = current.Value.value;
                previousRatio = curveRatio;
            }
            else if (Mathf.Abs(a_targetXRatio - previousRatio) > Mathf.Abs(a_targetXRatio - curveRatio))
            {
                a_closestRatio = curveRatio;
                return current.Value.value;
            }
            else
            {
                a_closestRatio = previousRatio;
                return value;
            }
            
            current = current.Next;
        }

        a_closestRatio = previousRatio;
        return value;
    }

    public void ClearData()
    {
        if (_values != null)
        {
            _values.Clear();
            _totalDeltatime = 0L;
            RefreshCurve();
        }
    }

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

    //The scale of the ordinate value.
    private float _yMaxValue = 1f;
    private float _yMaxWorldOffset = 1f;

    private float _scaleX = 10f;
    internal float ScaleX
    {
        get
        {
            return _scaleX;
        }
        set
        {
            _scaleX = value;
        }
    }
    #endregion

    internal bool IsActive
    {
        get
        {
            return _lineRenderer != null &&
                    _lineRenderer.gameObject.activeInHierarchy;
        }
    }

    // Use this for initialization
    internal void Init (float a_lifespan, float a_scaleX, float a_yMaxValue, float a_yMaxWorldOffset)
    {
        _lifespan = a_lifespan;
        _scaleX = a_scaleX;
        _yMaxValue = a_yMaxValue;
        _yMaxWorldOffset = a_yMaxWorldOffset;
        _values = new LinkedList<TimedValue<float>>();

        // Get the reference on he line renderer
        if (_lineRenderer == null)
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        RefreshCurve();
    }

    protected long _totalDeltatime = 0L;
    internal void AddValue(long a_nsDeltatime, float a_newValue)
    {
        _totalDeltatime += a_nsDeltatime;
        TimedValue <float> addedValue = new TimedValue<float>(a_newValue, a_nsDeltatime);

        LinkedListNode<TimedValue<float>> oldest = _values.First;
        while (oldest != null &&
                NsToSeconds(_totalDeltatime) >= _lifespan)
        {
            _values.RemoveFirst();
            _totalDeltatime = _totalDeltatime - oldest.Value.deltatime;
            oldest = oldest.Next;
        }

        _values.AddLast(addedValue);

        RefreshCurve();
    }

    public float NsToSeconds(long a_nsCount)
    {
        return a_nsCount * Mathf.Pow(10f, -9f);
    }

    void RefreshCurve()
    {
        Vector3[] positions = new Vector3[_values.Count];

        LinkedListNode<TimedValue<float>> current = _values.First;

        int index = 0;
        float x = 0f;
        float y = 0f;

        long cumulTime = 0L;
        while (current != null)
        {
            cumulTime += current.Value.deltatime;

            if (_scaleX > 0f)
                x = (NsToSeconds(cumulTime) / _lifespan) * _scaleX;
            else
                x = index;

            if (_yMaxValue > 0f)
                y = (current.Value.value / _yMaxValue) * _yMaxWorldOffset;
            else
                y = current.Value.value;

            positions[index] = new Vector3(x, y, 0f);
            current = current.Next;
            index++;
        }

        if (_lineRenderer != null)
        {
            _lineRenderer.positionCount = positions.Length;
            _lineRenderer.SetPositions(positions);
        }
    }*/

   /* private void OnEnable() {

        if(OnActivityModified != null)
            OnActivityModified(true);
    }

    private void OnDisable() {

        if (OnActivityModified != null)
            OnActivityModified(false);
    }*/
}
