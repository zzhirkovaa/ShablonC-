using UnityEngine;
using UnityEngine.UI; 

public class HealthBarSlider : MonoBehaviour
{
    [SerializeField] private Slider healthSlider; 
    private IHealth _healthSource;
    private Transform _mainCameraTransform;

    void Start()
    {
        _healthSource = GetComponentInParent<IHealth>();
        _mainCameraTransform = Camera.main.transform;

        if (_healthSource != null)
        {
            healthSlider.maxValue = 1f; 
            healthSlider.value = _healthSource.CurrentHealth / _healthSource.MaxHealth;

            _healthSource.OnHealthChanged += UpdateSlider;
        }
    }

    private void UpdateSlider(float healthPercentage)
    {
        if (healthSlider != null)
        {
            healthSlider.value = healthPercentage;
        }
    }

    void LateUpdate()
    {
        if (_mainCameraTransform != null)
        {
            transform.LookAt(transform.position + _mainCameraTransform.rotation * Vector3.forward,
                             _mainCameraTransform.rotation * Vector3.up);
        }
    }

    private void OnDestroy()
    {
        if (_healthSource != null)
        {
            _healthSource.OnHealthChanged -= UpdateSlider;
        }
    }
}