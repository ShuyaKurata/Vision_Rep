// CharacterMovement.cs
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField]
    PlaceOnPlane m_PlaceOnPlaneScript; // インスペクターで設定する

    [SerializeField]
    float m_MoveSpeed = 1.0f;

    [SerializeField]
    float m_RotationSpeed = 5.0f; // 回転速度

    [SerializeField]
    float m_StoppingDistance = 0.1f; // 停止する距離

    void Update()
    {
        // PlaceOnPlaneスクリプトが設定されていて、ターゲットがある場合のみ処理
        if (m_PlaceOnPlaneScript != null && m_PlaceOnPlaneScript.HasTarget)
        {
            Vector3 targetPosition = m_PlaceOnPlaneScript.TargetPosition;
            MoveTowards(targetPosition);
        }
    }

    void MoveTowards(Vector3 target)
    {
        // キャラクターからターゲットへの方向ベクトル (Y軸無視)
        Vector3 direction = target - transform.position;
        direction.y = 0;

        // ターゲットまでの距離 (Y軸無視)
        float distance = direction.magnitude;

        // 停止距離より離れていれば移動
        if (distance > m_StoppingDistance)
        {
            // 移動処理
            Vector3 moveVector = direction.normalized * m_MoveSpeed * Time.deltaTime;
            transform.position += moveVector;

            // 回転処理 (ゼロベクトルでなければ)
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * m_RotationSpeed);
            }
        }
    }
}