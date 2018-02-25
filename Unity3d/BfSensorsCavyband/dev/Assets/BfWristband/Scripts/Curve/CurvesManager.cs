using UnityEngine;

public enum ECurveId { Unindentified, AccX, AccY, AccZ, SpeedX, SpeedY, SpeedZ}


/// <summary>
/// This class manage an array of Curves and updates their values when commanded to do so by the UiManager
/// </summary>
public class CurvesManager : MonoBehaviour {

    // The offset of the curves in order to have them centered on the screen
    // To Do : Calculate this value automatically
    public float LeftStartDistance = -70f; 

    // This value is passed to each curve in order to determine its resolution
    [SerializeField] private int _resolutionFactor = 5;

    // Positions of the horizontal lines (+/- 1 g) for visual estimation purpose
    [SerializeField] private float _thresholdSup = 9.81f;
    [SerializeField] private float _thresholdInf = -9.81f;

    // The curves that are managed (e.g. Acceleration X, Y and Z)
    private Curve[] _curves = new Curve[0];
    private LineRenderer _thresholdInfRenderer = null;
    private LineRenderer _thresholdSupRenderer = null;

    // Buffer values for GC optimizations
    private int _numberOfValues = 0;
    private int _indexOnCurves  = 0;

    // Initializa each curve ass well as the thresohold lines
    internal Curve[] Init() {

        _curves = GetComponentsInChildren<Curve>();
        foreach (var curve in _curves)
        {
            _numberOfValues = curve.Init(_resolutionFactor, LeftStartDistance);
        }

        var lineRd = GetComponentsInChildren<LineRenderer>();
        _thresholdInfRenderer = lineRd[4];
        _thresholdSupRenderer = lineRd[3];

        _thresholdInfRenderer.SetPosition(0, new Vector3(LeftStartDistance, _thresholdInf));
        _thresholdInfRenderer.SetPosition(1, new Vector3(-LeftStartDistance, _thresholdInf));
        _thresholdSupRenderer.SetPosition(0, new Vector3(LeftStartDistance, _thresholdSup));
        _thresholdSupRenderer.SetPosition(1, new Vector3(-LeftStartDistance, _thresholdSup));

        registerEvents();

        // All curves are invisible at the beginning
        SetAllCurvesVisibility(false);

        return _curves;
    }

    // Getter
    internal Curve GetCurveById(ECurveId id) {

        Curve myCurve = null;

        for(int i = 0; i < _curves.Length; i++)
        {
            if (_curves[i].Id == id)
            {
                return _curves[i];
            }
        }

        return myCurve;
    }

    // Getter
    internal int GetCurveIndex(ECurveId id) {

        for (int i = 0; i < _curves.Length; i++)
        {
            if (_curves[i].Id == id)
            {
                return i;
            }
        }

        return -1;
    }

    // Turn visibility of a given curve on/off 
    internal void SetCurveVisibility(ECurveId id, bool isVisible) {

        Curve myCurve = GetCurveById(id);

        try
        {
            myCurve.gameObject.SetActive(isVisible);
        }
        catch(System.Exception e)
        {
            Debug.LogError(e);
        }

    }

    //  Turn visibility of a all curves on/off 
    internal void SetAllCurvesVisibility(bool isVisible) {

        foreach(var curve in _curves)
        {
            SetCurveVisibility(curve.Id, isVisible);
        }
        
    }

    // Freeze the curves in order to get enough time to make an analysis (called by a button)
    public void SetPause() {

        foreach(var c in _curves)
        {
            c.IsPaused = !c.IsPaused;
        }

    }

    // Check and Toggle curves activity
    private void CheckCurvesActivity(bool isCurveEnabled) {

        bool isAnyCurveVisible;

        if (!isCurveEnabled)
        {
            isAnyCurveVisible = false;
            foreach (var c in _curves)
            {
                isAnyCurveVisible = isAnyCurveVisible || c.gameObject.activeInHierarchy;
            }
        }
        else
        {
            isAnyCurveVisible = true;
        }

        _thresholdInfRenderer.gameObject.SetActive(isAnyCurveVisible);
        _thresholdSupRenderer.gameObject.SetActive(isAnyCurveVisible);
    }

    // Curves are updated each frame
    private void Update() {


        foreach (var curve in _curves)
        {
            curve.DoUpdate(_indexOnCurves);
        }

        if (++_indexOnCurves >= _numberOfValues)
        {
            _indexOnCurves = 0;
        }

    }

    private void registerEvents() {
        //Debug.Log("<b>PiqManagerEntryPoint</b> registerEvents");

        Curve.OnActivityModified += CheckCurvesActivity;
    }

    private void unRegisterEvents() {
        //Debug.Log("<b>PiqManagerEntryPoint</b> registerEvents");

        Curve.OnActivityModified -= CheckCurvesActivity;
    }

    private void OnDestroy() {
        unRegisterEvents();
    }

}
