using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private bool _alwaysActive = false;

    [SerializeField]
    private float _visibleRange = 4f;

    [SerializeField]
    private Damageable _damageable;

    private Slider _slider;
    

    void Start()
    {
        _slider = GetComponent<Slider>();

        _damageable.OnDamageChanged += (arg1, arg2) => OnValueChanged(arg1, arg2);
    }

    void Update()
    {
        
    }

    public void OnValueChanged(float curValue, float maxValue)
    {
        _slider.value = 1 - curValue / maxValue;
    }
}
