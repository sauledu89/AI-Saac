using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; 
    public float smoothSpeed = 5f; // Ajustar velocidad de seguimiento

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 newPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, newPosition, smoothSpeed * Time.deltaTime);
        }
    }
}