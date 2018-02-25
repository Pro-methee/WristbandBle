using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedFloatCurveRenderer : MonoBehaviour
{
    #region Inspector Properties
    private LineRenderer _lineRenderer = null;
    #endregion

    #region Properties
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

    private TimedFloatCurve _dataSource = null;
    public TimedFloatCurve DataSource
    {
        get
        {
            return _dataSource;
        }
    }

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
    internal void Init(float a_scaleX, float a_yMaxValue, float a_yMaxWorldOffset)
    {
        _scaleX = a_scaleX;
        _yMaxValue = a_yMaxValue;
        _yMaxWorldOffset = a_yMaxWorldOffset;

        // Get the reference on he line renderer
        if (_lineRenderer == null)
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }
    }

    void RefreshCurve()
    {
        Vector3[] positions = new Vector3[_dataSource.Values.Count];

        LinkedListNode<TimedValue<float>> current = _dataSource.Values.First;

        int index = 0;
        float x = 0f;
        float y = 0f;
        float lifespan = _dataSource.LifeSpan;

        float cumulDeltatime = 0f;
        while (current != null)
        {
            cumulDeltatime += current.Value.deltatime;

            if (_scaleX > 0f)
                x = (cumulDeltatime / lifespan) * _scaleX;
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
    }

    #region Events
    public void SetDataSource(TimedFloatCurve a_dataSource)
    {
        if (_dataSource != null)
        {
            UnregisterFromDataSourceEvents();
        }

        _dataSource = a_dataSource;
        if (_dataSource != null)
        {
            RegisterToDataSourceEvents();
            RefreshCurve();
        }
    }

    private void RegisterToDataSourceEvents()
    {
       _dataSource.onValuesChanged += OnValuesChanged;
    }

    private void UnregisterFromDataSourceEvents()
    {
        _dataSource.onValuesChanged -= OnValuesChanged;
    }

    void OnValuesChanged(TimedFloatCurve a_datasource)
    {
        RefreshCurve();
    }
    #endregion
}
