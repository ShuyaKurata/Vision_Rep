using UnityEngine;
#if UNITY_INCLUDE_XR_HANDS
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
#endif

namespace PolySpatial.Samples
{
    public class DecopinShooter : MonoBehaviour
    {
        [SerializeField]
        GameObject m_SpawnPrefab;

        [SerializeField]
        Transform m_PolySpatialCameraTransform;

        [SerializeField]
float m_ShotForce = 3f; // 発射の強さ（適宜調整）


#if UNITY_INCLUDE_XR_HANDS
        XRHandSubsystem m_HandSubsystem;
        XRHandJoint m_IndexTip;
        XRHandJoint m_ThumbTip;
        XRHandJoint m_Wrist;
        XRHandJoint m_Palm;

        Vector3 m_PreviousThumbIndexDelta;
        float m_PinchDistance;
        float m_ScaledThreshold;
        bool m_IsAiming;
        bool m_HasFired;

        const float k_PinchThreshold = 0.02f;
        const float k_SpeedThreshold = 1.0f;
        const float k_AimingStabilityThreshold = 0.001f;
        const float k_DirectionThreshold = 0.7f;

           // フィールドに追加
Vector3 m_PreviousIndexPos;
bool m_HasPreviousIndexPos = false;

        void Start()
        {
            GetHandSubsystem();
            m_ScaledThreshold = k_PinchThreshold / m_PolySpatialCameraTransform.localScale.x;
        }

        void Update()
        {
            if (!CheckHandSubsystem())
                return;

            var updateSuccessFlags = m_HandSubsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
            if ((updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose) != 0)
            {
                var hand = m_HandSubsystem.rightHand;

                m_IndexTip = hand.GetJoint(XRHandJointID.IndexTip);
                m_ThumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
                m_Wrist = hand.GetJoint(XRHandJointID.Wrist);
                m_Palm = hand.GetJoint(XRHandJointID.MiddleMetacarpal); // Palm がなければ代用

                TryDecopin();
            }
        }

      

void TryDecopin()
{
    if (m_IndexTip.TryGetPose(out Pose indexPose) &&
        m_ThumbTip.TryGetPose(out Pose thumbPose))
    {
        var indexPos = indexPose.position;
        var thumbPos = thumbPose.position;

        var delta = thumbPos - indexPos;
        var distance = delta.magnitude;

        if (distance < m_ScaledThreshold &&
            (delta - m_PreviousThumbIndexDelta).sqrMagnitude < k_AimingStabilityThreshold)
        {
            m_IsAiming = true;
            m_HasFired = false;
        }

        m_PreviousThumbIndexDelta = delta;

        // ここで速度を計算
        if (m_IsAiming && !m_HasFired && m_HasPreviousIndexPos)
        {
            Vector3 velocity = (indexPos - m_PreviousIndexPos) / Time.deltaTime;
            float speed = velocity.magnitude;

            if (m_Wrist.TryGetPose(out Pose wristPose) && m_Palm.TryGetPose(out Pose palmPose))
            {
                Vector3 handForward = (palmPose.position - wristPose.position).normalized;
                float dot = Vector3.Dot(velocity.normalized, handForward);

                if (speed > k_SpeedThreshold && dot > k_DirectionThreshold)
                {
                    GameObject obj = Instantiate(m_SpawnPrefab, indexPos, Quaternion.identity);

                    // 進行方向に力を加える
                    Rigidbody rb = obj.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.velocity = velocity.normalized * m_ShotForce; // ← ここで速度方向に飛ばす
                    }

                    m_HasFired = true;
                    m_IsAiming = false;
                }
            }
        }

        m_PreviousIndexPos = indexPos;
        m_HasPreviousIndexPos = true;

        if (distance > m_ScaledThreshold * 1.2f)
        {
            m_IsAiming = false;
        }
    }
}


        void GetHandSubsystem()
        {
            var settings = XRGeneralSettings.Instance;
            if (settings?.Manager?.activeLoader != null)
            {
                m_HandSubsystem = settings.Manager.activeLoader.GetLoadedSubsystem<XRHandSubsystem>();
                if (m_HandSubsystem != null)
                    m_HandSubsystem.Start();
            }
        }

        bool CheckHandSubsystem()
        {
            if (m_HandSubsystem == null)
            {
#if !UNITY_EDITOR
                Debug.LogError("Hand Subsystem not found.");
#endif
                enabled = false;
                return false;
            }

            return true;
        }
#endif
    }
}
