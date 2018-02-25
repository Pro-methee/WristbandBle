using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class AxisIndicator : MonoBehaviour {

    [SerializeField] private Slider   _accelerationSlider = null;
    [SerializeField] private Slider   _speedSlider        = null;

    [SerializeField]
    [Range(0f, 20f)]
    private float _maxAcceleration = 15f;

    [SerializeField]
    [Range(0f, 20f)]
    private float _maxSpeed = 10f;

    private float _accelerationRatio = 1f;
    private float _speedRatio        = 1f;

    private void Awake() {
        //Init();
    }

    // Use this for initialization
    internal void Init () {

        var sliders = GetComponentsInChildren<Slider>();

        if (_accelerationSlider == null && sliders.Length > 0)
        {
            _accelerationSlider = sliders[0];
            _accelerationRatio  = 1.0f / _maxAcceleration;
            _accelerationSlider.value = 0f;
        }
            

        if (_speedSlider == null && sliders.Length > 1)
        {
            _speedSlider = sliders[1];
            _speedRatio  = 1.0f / _maxSpeed;
            _speedSlider.value = 0f;
        }

    }

    internal void UpdateAcceleration(float acceleration) {

        _accelerationSlider.value = acceleration * _accelerationRatio;
    }

    internal void UpdateSpeed(float speed) {
        _speedSlider.value        = speed* _speedRatio;
    }

    internal void SetAccelerationVisibility(bool isVisible) {
        _accelerationSlider.gameObject.SetActive(isVisible);
    }

    internal void SetSpeedVisibility(bool isVisible) {
        _speedSlider.gameObject.SetActive(isVisible);
    }

}
