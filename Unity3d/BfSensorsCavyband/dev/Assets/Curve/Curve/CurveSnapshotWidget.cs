using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurveSnapshotWidget : MonoBehaviour
{
    #region Inspector Properties
    public GameObject valueWindow = null;
    public Text x1 = null;
    public Text y1 = null;
    public Text z1 = null;
    public Text zx1 = null;

    public Text x2 = null;
    public Text y2 = null;
    public Text z2 = null;
    public Text zx2 = null;

    public RectTransform snapshotIndicatorTransform = null;
    #endregion

    #region Indicator
    public void SetIndicatorRatio(float a_viewportXRatio)
    {
        Vector3 localPos = snapshotIndicatorTransform.localPosition;
        localPos.x = (-0.5f + a_viewportXRatio) * 1920f;
        snapshotIndicatorTransform.localPosition = localPos;
    }

    public void SetIndicatorVisible(bool a_visible)
    {
        snapshotIndicatorTransform.gameObject.SetActive(a_visible);
    }

    public bool IsIndicatorVisible
    {
        get
        {
            return snapshotIndicatorTransform.gameObject.activeSelf;
        }
    }
    #endregion

    #region Value Window
    public void SetSnapshotValues(float a_x1, float a_y1, float a_z1, float a_zx1,
                                    float a_x2, float a_y2, float a_z2, float a_zx2)
    {
        x1.text = "x1 = " + a_x1.ToString("0.##");
        y1.text = "y1 = " + a_y1.ToString("0.##");
        z1.text = "z1 = " + a_z1.ToString("0.##");
        zx1.text = "zx1 = " + a_zx1.ToString("0.##");

        x2.text = "x2 = " + a_x2.ToString("0.##");
        y2.text = "y2 = " + a_y2.ToString("0.##");
        z2.text = "z2 = " + a_z2.ToString("0.##");
        zx2.text = "zx1 = " + a_zx2.ToString("0.##");
    }

    public void SetValuesWindowVisible(bool a_visible)
    {
        valueWindow.gameObject.SetActive(a_visible);
    }

    public bool IsValueWindowVisible
    {
        get
        {
            return valueWindow.gameObject.activeSelf;
        }
    }
    #endregion
}
