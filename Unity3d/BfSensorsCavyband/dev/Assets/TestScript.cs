using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BfSensorsCavyband;

public class TestScript : MonoBehaviour {
    public GameObject[] cubes = null;

	// Use this for initialization
	void Start () {
        BleSensorsManager.Instance.onNewSensorConnected += OnNewSensorConnected;
    }
	
	// Update is called once per frame
	void Update () {
	}

    public void SwitchScanState()
    {
        if (BleSensorsManager.Instance.IsScanningForSensors)
            BleSensorsManager.Instance.StopScanningForSensors();
        else
            BleSensorsManager.Instance.StartScanningForSensors();
    }

    BleSensorCavybandBase _sensor = null;
    void OnNewSensorConnected(BleSensorCavybandBase a_sensor)
    {
        _sensor = a_sensor;
        
        Debug.LogError("New sensor : " + a_sensor.BleAddress);
    }

    public void SwitchGameMode()
    {
        if (_sensor.IsInGameMode)
            _sensor.ExitGameMode();
        else
            _sensor.EnterGameMode(20);
    }

    public void StartMonitoring()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Curves");
        BleSensorsManager.Instance.onNewSensorConnected -= OnNewSensorConnected;
    }
}
