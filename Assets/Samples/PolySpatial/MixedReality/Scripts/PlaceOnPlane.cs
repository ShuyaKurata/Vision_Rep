// PlaceOnPlane.cs (または PlaceOnPlaneAndMoveCharacter.cs を修正する場合)
using UnityEngine;
#if UNITY_INCLUDE_ARFOUNDATION
using UnityEngine.XR.ARFoundation;
#endif
using System.Collections; // 追加

public class PlaceOnPlane : MonoBehaviour // 元のスクリプト名に戻す想定
{
    [SerializeField]
    Transform m_CameraTransform;

    [SerializeField]
    GameObject m_HeadPoseReticlePrefab; // Prefabであることを明示

    GameObject m_SpawnedHeadPoseReticle;
    RaycastHit m_HitInfo;

    // --- 外部公開用のプロパティ ---
    public Vector3 TargetPosition { get; private set; } = Vector3.zero;
    public bool HasTarget { get; private set; } = false;
    // -----------------------------

    void Start()
    {
        m_SpawnedHeadPoseReticle = Instantiate(m_HeadPoseReticlePrefab, Vector3.zero, Quaternion.identity);
        m_SpawnedHeadPoseReticle.SetActive(false); // 最初は非表示
        StartCoroutine(InitializeReticlePosition()); // 最初の一瞬だけ待つ
    }

    // Start直後はARPlaneが認識できていない可能性があるので少し待つ
    IEnumerator InitializeReticlePosition()
    {
        yield return new WaitForSeconds(0.5f); // 0.5秒待つ (適宜調整)
        UpdateTargetPosition(); // 最初の位置決めを試みる
    }


    void Update()
    {
        UpdateTargetPosition();
    }

    void UpdateTargetPosition()
    {
#if UNITY_INCLUDE_ARFOUNDATION
        HasTarget = false; // デフォルトはターゲットなし
        if (Physics.Raycast(new Ray(m_CameraTransform.position, m_CameraTransform.forward), out m_HitInfo))
        {
            if (m_HitInfo.transform.TryGetComponent(out ARPlane plane))
            {
                TargetPosition = m_HitInfo.point; // ターゲット位置を更新
                m_SpawnedHeadPoseReticle.transform.SetPositionAndRotation(TargetPosition, Quaternion.FromToRotation(Vector3.up, m_HitInfo.normal));
                m_SpawnedHeadPoseReticle.SetActive(true); // 見つかったら表示
                HasTarget = true;
            }
            else
            {
                m_SpawnedHeadPoseReticle.SetActive(false); // ARPlane以外なら非表示
            }
        }
        else
        {
            m_SpawnedHeadPoseReticle.SetActive(false); // 何もヒットしなかったら非表示
        }
#endif
    }
}