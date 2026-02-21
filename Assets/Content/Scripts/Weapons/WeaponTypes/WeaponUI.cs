using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class WeaponUI : MonoBehaviour
{
    [Header("Weapon Info")]
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private TextMeshProUGUI _weaponNameText;
    
    [Header("Ammo")]
    [SerializeField] private TextMeshProUGUI _currentAmmoText;
    [SerializeField] private TextMeshProUGUI _totalAmmoText;
    [SerializeField] private Image _ammoFillImage;
    
    [Header("Reloading")]
    [SerializeField] private GameObject _reloadingPanel;
    [SerializeField] private Image _reloadProgressBar;
    [SerializeField] private TextMeshProUGUI _reloadText;
    
    [Header("Shotgun Specific")]
    [SerializeField] private GameObject _shellsPanel;
    [SerializeField] private Image[] _shellIcons;
    [SerializeField] private Color _shellFullColor = Color.white;
    [SerializeField] private Color _shellEmptyColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    
    private void Start()
    {
        if (_reloadingPanel != null)
            _reloadingPanel.SetActive(false);
            
        if (_shellsPanel != null)
            _shellsPanel.SetActive(false);
    }
    
    public void UpdateWeaponInfo(Sprite icon, string weaponName)
    {
        if (_weaponIcon != null)
            _weaponIcon.sprite = icon;
            
        if (_weaponNameText != null)
            _weaponNameText.text = weaponName;
    }
    
    public void UpdateAmmo(int currentAmmo, int totalAmmo, int maxAmmo)
    {
        // Обновляем текстовые поля
        if (_currentAmmoText != null)
            _currentAmmoText.text = currentAmmo.ToString();
            
        if (_totalAmmoText != null)
            _totalAmmoText.text = totalAmmo.ToString();
        
        // Обновляем заполнение прогресс-бара
        if (_ammoFillImage != null)
        {
            float fillAmount = maxAmmo > 0 ? (float)currentAmmo / maxAmmo : 0;
            _ammoFillImage.fillAmount = fillAmount;
        }
        
        // Обновляем иконки патронов для дробовика
        UpdateShells(currentAmmo, maxAmmo);
        
        Debug.Log($"UI Updated - Current: {currentAmmo}, Total: {totalAmmo}, Max: {maxAmmo}");
    }
    
    public void UpdateShells(int currentAmmo, int maxAmmo)
    {
        if (_shellIcons == null || _shellIcons.Length == 0)
            return;
        
        if (_shellsPanel != null && !_shellsPanel.activeSelf)
            _shellsPanel.SetActive(true);
            
        for (int i = 0; i < _shellIcons.Length; i++)
        {
            if (_shellIcons[i] == null) continue;
            
            if (i < currentAmmo)
            {
                // Полный патрон
                _shellIcons[i].color = _shellFullColor;
                _shellIcons[i].transform.localScale = Vector3.one;
                _shellIcons[i].gameObject.SetActive(true);
            }
            else if (i < maxAmmo)
            {
                // Пустой патрон (место под патрон)
                _shellIcons[i].color = _shellEmptyColor;
                _shellIcons[i].transform.localScale = Vector3.one * 0.9f;
                _shellIcons[i].gameObject.SetActive(true);
            }
            else
            {
                // Лишние иконки (если maxAmmo меньше чем количество иконок)
                _shellIcons[i].gameObject.SetActive(false);
            }
        }
    }
    
    public void ShowReloading(float duration)
    {
        if (_reloadingPanel != null)
        {
            _reloadingPanel.SetActive(true);
            
            if (_reloadProgressBar != null)
            {
                _reloadProgressBar.fillAmount = 0f;
                _reloadProgressBar.DOFillAmount(1f, duration).SetEase(Ease.Linear);
            }
            
            if (_reloadText != null)
                _reloadText.text = "RELOADING...";
        }
    }
    
    public void HideReloading()
    {
        if (_reloadingPanel != null)
            _reloadingPanel.SetActive(false);
    }
    
    public void EnableShotgunMode(int maxShells)
    {
        if (_shellsPanel != null)
        {
            _shellsPanel.SetActive(true);
            
            // Активируем только нужное количество иконок
            if (_shellIcons != null)
            {
                for (int i = 0; i < _shellIcons.Length; i++)
                {
                    if (_shellIcons[i] != null)
                        _shellIcons[i].gameObject.SetActive(i < maxShells);
                }
            }
        }
    }
}