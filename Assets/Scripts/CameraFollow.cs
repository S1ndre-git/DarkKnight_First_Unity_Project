using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0f, 2f, -10f);

    void LateUpdate()
    {
        if (player == null) return;

        transform.position = player.position + offset;
    }
}