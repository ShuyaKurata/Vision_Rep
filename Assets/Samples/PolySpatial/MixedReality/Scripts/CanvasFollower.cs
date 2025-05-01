using UnityEngine;

public class CanvasFollower : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float distance = 1.0f;

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // カメラ前方に配置
        transform.position = cameraTransform.position + cameraTransform.forward * distance;

        // カメラの方向に向ける
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
    }
}
