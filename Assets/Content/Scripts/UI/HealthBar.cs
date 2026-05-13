using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private bool _alwaysActive = false;

    [SerializeField]
    private float _visibleRange = 4f;

    private Slider _slider;


    void Start()
    {
        _slider = GetComponent<Slider>();
    }

    void Update()
    {

    }

    public void OnDamageChanged(Component sender, object eventData)
    {
        if (eventData is not DamageChangedData data)
            return;

        _slider.value = 1 - data.CurrentDamage / data.MaxHealth;

    }
}
