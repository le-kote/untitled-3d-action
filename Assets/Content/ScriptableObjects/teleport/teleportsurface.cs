using UnityEngine;
using UnityEngine.Events;

public class TeleportSurface : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField]
    private bool _allowTeleport = true;

    [SerializeField]
    private Vector3 _teleportOffset = Vector3.zero;

    [SerializeField]
    private bool _requireFacingDirection = false;

    [SerializeField]
    private Vector3 _allowedDirection = Vector3.forward;

    [SerializeField]
    private float _directionTolerance = 45f;

    [Header("Visual Feedback")]
    [SerializeField]
    private GameObject _arrivalEffectPrefab;

    [SerializeField]
    private AudioClip _arrivalSound;

    [SerializeField]
    private Color _highlightColor = Color.cyan;

    [Header("Events")]
    [SerializeField]
    private UnityEvent<GameObject> _onTeleportArrival;
    
    [SerializeField]
    private UnityEvent<GameObject> _onTeleportDeparture;

    [Header("Restrictions")]
    [SerializeField]
    private float _minDistanceFromCenter = 0f;

    [SerializeField]
    private float _maxDistanceFromCenter = float.MaxValue;

    private Renderer[] _renderers;
    private Material[][] _originalMaterials;
    private AudioSource _audioSource;
    private bool _isHighlighted = false;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _originalMaterials = new Material[_renderers.Length][];
        
        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalMaterials[i] = _renderers[i].materials;
        }

        if (_arrivalSound != null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 1f;
        }
    }

    public void OnTeleportArrival(GameObject teleportingObject)
    {
        if (!_allowTeleport)
            return;

        if (_arrivalEffectPrefab != null)
        {
            Instantiate(_arrivalEffectPrefab, transform.position, Quaternion.identity);
        }

        if (_audioSource != null && _arrivalSound != null)
        {
            _audioSource.PlayOneShot(_arrivalSound);
        }

        _onTeleportArrival?.Invoke(teleportingObject);
    }

    public void OnTeleportDeparture(GameObject teleportingObject)
    {
        _onTeleportDeparture?.Invoke(teleportingObject);
    }

    public bool CanTeleport()
    {
        return _allowTeleport;
    }

    public bool CanTeleportTo(GameObject teleportingObject, Vector3 hitPoint, Vector3 viewDirection)
    {
        if (!_allowTeleport)
            return false;

        if (_requireFacingDirection)
        {
            Vector3 worldAllowedDirection = transform.TransformDirection(_allowedDirection.normalized);
            float angle = Vector3.Angle(viewDirection, -worldAllowedDirection);
            
            if (angle > _directionTolerance)
                return false;
        }

        float distanceFromCenter = Vector3.Distance(hitPoint, transform.position);
        if (distanceFromCenter < _minDistanceFromCenter || distanceFromCenter > _maxDistanceFromCenter)
            return false;

        return true;
    }

    public Vector3 GetTeleportPosition(Vector3 basePosition)
    {
        return basePosition + _teleportOffset;
    }

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
                if (_originalMaterials[i] != null)
                {
                    _renderers[i].materials = _originalMaterials[i];
                }
            }
        }
    }

    private void OnDestroy()
    {
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
        if (_requireFacingDirection)
        {
            Gizmos.color = Color.yellow;
            Vector3 direction = transform.TransformDirection(_allowedDirection.normalized);
            Gizmos.DrawRay(transform.position, direction * 2f);
            
            Vector3 right = Quaternion.Euler(0, _directionTolerance, 0) * direction;
            Vector3 left = Quaternion.Euler(0, -_directionTolerance, 0) * direction;
            Gizmos.DrawRay(transform.position, right * 2f);
            Gizmos.DrawRay(transform.position, left * 2f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _minDistanceFromCenter);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _maxDistanceFromCenter);
    }
}