using UnityEngine;

public class CameraComponent : MonoBehaviour
{
    public Transform target;

    private float horigontalMove = 180f;

    public void MoveHorizontal()
    {
        transform.RotateAround(target.position, Vector3.up, horigontalMove);

    }
}
