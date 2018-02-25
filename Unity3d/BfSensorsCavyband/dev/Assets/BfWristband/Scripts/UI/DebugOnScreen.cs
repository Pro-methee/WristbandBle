using UnityEngine;

using BfWristband.Api;

/// <summary>
/// This class is used for displaying OnGUI messages on screen in a debugging process
/// </summary>
public class DebugOnScreen : MonoBehaviour {

    public enum ELogType { Status, Debug, RawData}

    private static string s_Status = "NO STATUS";
    private static string s_DebugOnScreenMessage = "NO DEBUG MESSAGE";
    private static string s_RawData = "NO DATA";
    private static int    s_Counter = 0;

    [Tooltip("Enable to display OnGUI debug message while in Editor")]
    public bool      IsEnabled;

    private GUIStyle _gStyle;

    // Use this for initialization
    private void Awake () {

        _gStyle             = new GUIStyle();
        _gStyle.fontStyle   = FontStyle.Bold;
        _gStyle.fontSize    = 20;

        MenuController.OnCleanUIRequested += HandleCleanRequest;

    }

    private void OnGUI()
    {
        if (!IsEnabled) return;

        _gStyle.normal.textColor = Color.yellow;
        GUI.Label(new Rect(new Vector2(10, 10), Vector2.one * 50), s_Status, _gStyle);

        _gStyle.normal.textColor = Color.green;
        GUI.Label(new Rect(new Vector2(10, 30), new Vector2(100, 100)), s_DebugOnScreenMessage, _gStyle);

        _gStyle.normal.textColor = Color.black;
        GUI.Label(new Rect(new Vector2(10, 150), new Vector2(200, 800)), s_RawData, _gStyle);
    }

    public static void Log(string message, ELogType logType = ELogType.Debug)
    {
        switch (logType)
        {
            case ELogType.Status:
                s_Status = message;
                break;

            case ELogType.Debug:
                s_DebugOnScreenMessage = message;
                break;

            case ELogType.RawData:
                if (s_Counter > 500)
                {
                    s_RawData = message + "\n";
                    s_Counter = 0;
                }
                else
                {
                    s_RawData += message + "\n";
                    s_Counter++;
                }
                break;
        }
    }

    private void HandleCleanRequest()
    {
        s_Status = "";
        s_DebugOnScreenMessage = "";
        s_RawData = "";
        s_Counter = 0;
    }
}
