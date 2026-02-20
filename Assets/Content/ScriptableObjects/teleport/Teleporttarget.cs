using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Компонент, который нужно добавить на объекты, к которым можно телепортироваться
/// Позволяет настроить поведение при телепортации и добавить дополнительные эффекты
/// </summary>
public class TeleportTarget : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField]
    [Tooltip("Разрешена ли телепортация к этому объекту")]
    private bool _allowTeleport = true;

    [SerializeField]
    [Tooltip("Смещение позиции телепортации относительно точки касания")]
    private Vector3 _teleportOffset = Vector3.zero;

    [SerializeField]
    [Tooltip("Можно ли телепортироваться только когда игрок смотрит на определенную сторону объекта")]
    private bool _requireFacingDirection = false;

    [SerializeField]
    [Tooltip("Направление, с которого разрешена телепортация (локальное пространство объекта)")]
    private Vector3 _allowedDirection = Vector3.forward;

    [SerializeField]
    [Tooltip("Угол допуска для направления взгляда")]
    private float _directionTolerance = 45f;

    [Header("Visual Feedback")]
    [SerializeField]
    [Tooltip("Эффект при прибытии телепортации")]
    private GameObject _arrivalEffectPrefab;

    [SerializeField]
    [Tooltip("Звук при прибытии телепортации")]
    private AudioClip _arrivalSound;

    [SerializeField]
    [Tooltip("Цвет подсветки объекта при наведении")]
    private Color _highlightColor = Color.cyan;

    [Header("Events")]
    [SerializeField]
    private UnityEvent<GameObject> _onTeleportArrival;
    
    [SerializeField]
    private UnityEvent<GameObject> _onTeleportDeparture;

    [Header("Restrictions")]
    [SerializeField]
    [Tooltip("Минимальное расстояние от центра объекта для телепортации")]
    private float _minDistanceFromCenter = 0f;

    [SerializeField]
    [Tooltip("Максимальное расстояние от центра объекта для телепортации")]
    private float _maxDistanceFromCenter = float.MaxValue;

    [SerializeField]
    [Tooltip("Требовать, чтобы объект был в пределах видимости камеры")]
    private bool _requireInView = false;

    [SerializeField]
    [Tooltip("Слой, который считается препятствием для линии видимости")]
    private LayerMask _viewObstacleMask;

    // Приватные поля
    private Renderer[] _renderers;
    private Material[][] _originalMaterials;
    private AudioSource _audioSource;
    private bool _isHighlighted = false;

    private void Awake()
    {
        // Кэшируем все рендереры на объекте и его детях
        _renderers = GetComponentsInChildren<Renderer>();
        _originalMaterials = new Material[_renderers.Length][];
        
        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalMaterials[i] = _renderers[i].materials;
        }

        // Настраиваем AudioSource для звуков
        if (_arrivalSound != null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 1f; // 3D звук
        }
    }

    /// <summary>
    /// Вызывается при прибытии телепортации на этот объект
    /// </summary>
    /// <param name="teleportingObject">Объект, который телепортировался</param>
    public void OnTeleportArrival(GameObject teleportingObject)
    {
        if (!_allowTeleport)
            return;

        Debug.Log($"{teleportingObject.name} teleported to {gameObject.name}");

        // Воспроизводим эффект прибытия
        if (_arrivalEffectPrefab != null)
        {
            Instantiate(_arrivalEffectPrefab, transform.position, Quaternion.identity);
        }

        // Воспроизводим звук
        if (_audioSource != null && _arrivalSound != null)
        {
            _audioSource.PlayOneShot(_arrivalSound);
        }

        // Вызываем событие
        _onTeleportArrival?.Invoke(teleportingObject);

        // Дополнительная логика в зависимости от тега или имени объекта
        ApplySpecialBehavior(teleportingObject);
    }

    /// <summary>
    /// Вызывается перед телепортацией с этого объекта
    /// </summary>
    public void OnTeleportDeparture(GameObject teleportingObject)
    {
        _onTeleportDeparture?.Invoke(teleportingObject);
    }

    /// <summary>
    /// Базовая проверка - разрешена ли телепортация к этому объекту
    /// </summary>
    public bool CanTeleport()
    {
        return _allowTeleport;
    }

    /// <summary>
    /// Расширенная проверка с учетом направления взгляда и позиции
    /// </summary>
    public bool CanTeleportTo(GameObject teleportingObject, Vector3 hitPoint, Vector3 viewDirection)
    {
        if (!_allowTeleport)
            return false;

        // Проверка направления взгляда
        if (_requireFacingDirection)
        {
            Vector3 worldAllowedDirection = transform.TransformDirection(_allowedDirection.normalized);
            float angle = Vector3.Angle(viewDirection, -worldAllowedDirection); // Инвертируем, так как смотрим на объект
            
            if (angle > _directionTolerance)
            {
                Debug.Log($"Teleport denied: wrong facing direction. Angle: {angle}, required: <{_directionTolerance}");
                return false;
            }
        }

        // Проверка расстояния от центра
        float distanceFromCenter = Vector3.Distance(hitPoint, transform.position);
        if (distanceFromCenter < _minDistanceFromCenter || distanceFromCenter > _maxDistanceFromCenter)
        {
            Debug.Log($"Teleport denied: distance from center {distanceFromCenter} is out of range");
            return false;
        }

        // Проверка видимости
        if (_requireInView)
        {
            if (!IsInView(hitPoint))
            {
                Debug.Log("Teleport denied: not in view");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Проверяет, находится ли точка в пределах видимости
    /// </summary>
    private bool IsInView(Vector3 point)
    {
        if (Camera.main == null)
            return true;

        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(point);
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 && 
               viewportPoint.y >= 0 && viewportPoint.y <= 1 && 
               viewportPoint.z > 0;
    }

    /// <summary>
    /// Возвращает позицию для телепортации с учетом смещения
    /// </summary>
    public Vector3 GetTeleportPosition(Vector3 basePosition)
    {
        return basePosition + _teleportOffset;
    }

    /// <summary>
    /// Включает подсветку объекта
    /// </summary>
    public void Highlight(bool highlight)
    {
        if (highlight == _isHighlighted)
            return;

        _isHighlighted = highlight;

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] == null) continue;

            if (highlight)
            {
                // Создаем копии материалов для подсветки
                Material[] materials = _renderers[i].materials;
                for (int j = 0; j < materials.Length; j++)
                {
                    materials[j] = new Material(materials[j]);
                    if (materials[j].HasProperty("_Color"))
                    {
                        materials[j].color = _highlightColor;
                    }
                    if (materials[j].HasProperty("_EmissionColor"))
                    {
                        materials[j].EnableKeyword("_EMISSION");
                        materials[j].SetColor("_EmissionColor", _highlightColor * 0.5f);
                    }
                }
                _renderers[i].materials = materials;
            }
            else
            {
                // Восстанавливаем оригинальные материалы
                if (_originalMaterials[i] != null)
                {
                    _renderers[i].materials = _originalMaterials[i];
                }
            }
        }
    }

    /// <summary>
    /// Применяет специальное поведение в зависимости от тега или имени
    /// </summary>
    private void ApplySpecialBehavior(GameObject teleportingObject)
    {
        // Примеры специального поведения в зависимости от тега
        switch (gameObject.tag)
        {
            case "TeleportPoint":
                // Точка телепортации - просто логируем
                Debug.Log("Teleported to a teleport point");
                break;

            case "TeleportPlatform":
                // Платформа - активируем какие-то эффекты
                Debug.Log("Teleported to a platform");
                break;

            case "TeleportTrap":
                // Ловушка - наносим урон
                Debug.Log("Teleported to a trap!");
                break;
        }
    }

    private void OnDestroy()
    {
        // Очищаем созданные материалы при уничтожении
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null && _originalMaterials[i] != null)
            {
                _renderers[i].materials = _originalMaterials[i];
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Визуализация разрешенного направления в редакторе
        if (_requireFacingDirection)
        {
            Gizmos.color = Color.yellow;
            Vector3 direction = transform.TransformDirection(_allowedDirection.normalized);
            Gizmos.DrawRay(transform.position, direction * 2f);
            
            // Рисуем конус для визуализации допуска
            Vector3 right = Quaternion.Euler(0, _directionTolerance, 0) * direction;
            Vector3 left = Quaternion.Euler(0, -_directionTolerance, 0) * direction;
            Gizmos.DrawRay(transform.position, right * 2f);
            Gizmos.DrawRay(transform.position, left * 2f);
        }

        // Визуализация зон расстояния
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _minDistanceFromCenter);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _maxDistanceFromCenter);
    }
}