using UnityEngine;
using System.Collections;


namespace BfSensorsCavyband
{
    public class BfSensorsNativeReceiver : MonoBehaviour
    {
        void OnApplicationPause(bool a_isPaused)
        {
            BleSensorsManager.Instance.NotifyApplicationPaused(a_isPaused);
        }

        void OnDestroy()
        {
            BleSensorsManager.Instance.Reset();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        #region Message Infos
        public void NewSensorConnected(string a_bleAddress)
        {
            BleSensorsManager.Instance.NotifyNewSensorConnectedAtAddress(a_bleAddress);
        }

        public void SensorReconnected(string a_bleAddress)
        {
            BleSensorCavybandBase sensor = null;
            if (BleSensorsManager.Instance.SensorsByBleAddress.TryGetValue(a_bleAddress, out sensor))
            {
                sensor.NotifyReconnected();
            }
        }

        public void SensorDisconnected(string a_bleAddress)
        {
            BleSensorCavybandBase sensor = null;
            if (BleSensorsManager.Instance.SensorsByBleAddress.TryGetValue(a_bleAddress, out sensor))
            {
                sensor.NotifyDisconnected();
            }
        }

        public void OnSensorDataReceived(string a_bleAddress)
        {
            BleSensorCavybandBase sensor = null;
            if (BleSensorsManager.Instance.SensorsByBleAddress.TryGetValue(a_bleAddress, out sensor))
            {
                sensor.NotifyNewDataReceived();
            }
        }
        #endregion
    }
}
