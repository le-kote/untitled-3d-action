using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Portal : MonoBehaviour
{
    [Header("Portal Connection")]
    [SerializeField] private string _portalID = "default";
    [SerializeField] private Portal _connectedPortal;
    
    [Header("Settings")]
    [SerializeField] private string[] _teleportableTags = { "Player" };
    
    [Header("Exit Point")]
    [SerializeField] private Transform _exitPoint;
    
    // СТАТИЧЕСКОЕ ПОЛЕ - общее для ВСЕХ порталов
    private static float _globalLastTeleportTime = 0f;
    private static float _globalCooldown = 0.5f; // Общий КД для всей системы
    
    private Collider _collider;
    
    void Start()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
        
        if (_exitPoint == null)
            _exitPoint = transform;
        
        Debug.Log($"Portal {name} started with ID: {_portalID}");
        
        if (_connectedPortal == null)
            FindPortalByID();
    }
    
    void FindPortalByID()
    {
        Portal[] allPortals = FindObjectsByType<Portal>(FindObjectsSortMode.None);
        
        foreach (var portal in allPortals)
        {
            if (portal != this && portal._portalID == _portalID)
            {
                _connectedPortal = portal;
                Debug.Log($"✓ Portal {name} connected to {portal.name}");
                break;
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Проверяем ОБЩИЙ глобальный кулдаун
        if (Time.time < _globalLastTeleportTime + _globalCooldown)
        {
            Debug.Log($"Global cooldown: {Time.time - _globalLastTeleportTime:F2} < {_globalCooldown}");
            return;
        }
        
        if (_connectedPortal == null)
        {
            Debug.LogError($"Portal {name} has no connected portal!");
            return;
        }
        
        bool validTag = false;
        foreach (string tag in _teleportableTags)
        {
            if (other.CompareTag(tag))
            {
                validTag = true;
                break;
            }
        }
        
        if (!validTag) return;
        
        // Устанавливаем ОБЩЕЕ время последней телепортации
        _globalLastTeleportTime = Time.time;
        
        // Простое перемещение в точку выхода связанного портала
        other.transform.position = _connectedPortal._exitPoint.position;
        
        // Опционально: сохраняем направление
        // other.transform.rotation = _connectedPortal._exitPoint.rotation;
        
        if (other.TryGetComponent<CharacterController>(out var cc))
        {
            cc.enabled = false;
            cc.enabled = true;
        }
        
        Debug.Log($"✓ Teleported {other.name} from {name} to {_connectedPortal.name}");
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider>().bounds.size);
        
        if (_exitPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_exitPoint.position, 0.3f);
            Gizmos.DrawRay(_exitPoint.position, _exitPoint.forward * 1f);
        }
        
        if (_connectedPortal != null && _connectedPortal != this)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, _connectedPortal.transform.position);
        }
    }
}