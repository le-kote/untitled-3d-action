using UnityEngine;

public class PlayerPortalHandler : MonoBehaviour
{
    private void Start()
    {
        // Убедитесь что у игрока есть тег "Player"
        gameObject.tag = "Player";
    }
    
    // Этот метод будет вызываться порталом для правильного перемещения камеры
    public void OnPortalTeleport(Transform newPosition, Quaternion newRotation)
    {
        // Перемещаем игрока
        transform.position = newPosition.position;
        transform.rotation = newRotation;
        
        // Если есть CharacterController, нужно его временно отключить
        if (TryGetComponent<CharacterController>(out var cc))
        {
            cc.enabled = false;
            cc.enabled = true;
        }
    }
}