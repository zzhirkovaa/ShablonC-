using UnityEngine;
using UnityEngine.UI;

public class AbilityCooldownUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private Color _readyColor = Color.green;
    [SerializeField] private Color _cooldownColor = Color.white;

    private void Awake()
    {
        if (_iconImage == null) _iconImage = GetComponent<Image>();
    }

    public void UpdateFill(float currentCooldown, float maxCooldown)
    {
        if (_iconImage == null) return;

        if (currentCooldown > 0)
        {
            _iconImage.color = _cooldownColor;
            _iconImage.fillAmount = (maxCooldown - currentCooldown) / maxCooldown;
        }
        else
        {
            _iconImage.color = _readyColor;
            _iconImage.fillAmount = 1f;
        }
    }
}