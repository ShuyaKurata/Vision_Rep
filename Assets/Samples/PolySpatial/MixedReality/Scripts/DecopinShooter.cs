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
float m_ShotForce = 20f; // 発射の強さ（適宜調整）

         public float rotateSpeed = 0.1f; // 毎秒90度回転（Z軸回転なら Vector3.forward）

         [SerializeField]
        private AudioSource audio;

         [SerializeField]
        private GameObject airEffect;


#if UNITY_INCLUDE_XR_HANDS
        XRHandSubsystem m_HandSubsystem;
        XRHandJoint m_IndexTip;
        XRHandJoint m_ThumbTip;
        XRHandJoint m_Wrist;
        XRHandJoint m_Palm;

        bool middleExtended;
          bool ringExtended;  
          bool pinkyExtended;

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

         private bool objCreated = false;
        private GameObject obj;
        float duration = 2.0f; // スケーリングにかける時間（秒）
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one * 0.2f;
        float elapsedTime = 0f;

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
                
                middleExtended = IsFingerExtended(hand, XRHandJointID.MiddleTip, XRHandJointID.MiddleMetacarpal);
                ringExtended = IsFingerExtended(hand, XRHandJointID.RingTip, XRHandJointID.RingMetacarpal);
                pinkyExtended = IsFingerExtended(hand, XRHandJointID.LittleTip, XRHandJointID.LittleMetacarpal);


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
            (delta - m_PreviousThumbIndexDelta).sqrMagnitude < k_AimingStabilityThreshold &&
            middleExtended && ringExtended && pinkyExtended)
        {
            m_IsAiming = true;
            m_HasFired = false;
        }

        m_PreviousThumbIndexDelta = delta;

        // // ここで速度を計算
        // if (m_IsAiming && !m_HasFired && m_HasPreviousIndexPos)
        // {
        //     Vector3 velocity = (indexPos - m_PreviousIndexPos) / Time.deltaTime;
        //     float speed = velocity.magnitude;

        //     if (m_Wrist.TryGetPose(out Pose wristPose) && m_Palm.TryGetPose(out Pose palmPose))
        //     {
        //         Vector3 handForward = (palmPose.position - wristPose.position).normalized;
        //         float dot = Vector3.Dot(velocity.normalized, handForward);

        //         if (speed > k_SpeedThreshold && dot > k_DirectionThreshold)
        //         {
        //             GameObject obj = Instantiate(m_SpawnPrefab, indexPos, Quaternion.identity);

        //             // 進行方向に力を加える
        //             Rigidbody rb = obj.GetComponent<Rigidbody>();
        //             if (rb != null)
        //             {
        //                 rb.velocity = velocity.normalized * m_ShotForce; // ← ここで速度方向に飛ばす
        //             }

        //             m_HasFired = true;
        //             m_IsAiming = false;
        //         }
        //     }
        // }



        if (m_IsAiming && !m_HasFired && m_HasPreviousIndexPos)
        {
            Vector3 velocity = (indexPos - m_PreviousIndexPos) / Time.deltaTime;
            float speed = velocity.magnitude;
            Debug.Log(speed);

            if (!objCreated)
            {
                obj = Instantiate(m_SpawnPrefab, indexPos, Quaternion.identity);
                objCreated = true;
            }

            if(obj != null){
                obj.transform.position = indexPos;
                // 回転
                // obj.transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

                if (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsedTime / duration);
                    obj.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                }

                if (m_Wrist.TryGetPose(out Pose wristPose) && m_Palm.TryGetPose(out Pose palmPose))
                {
                    Vector3 handForward = (palmPose.position - wristPose.position).normalized;
                    float dot = Vector3.Dot(velocity.normalized, handForward);

                    if (speed > k_SpeedThreshold && dot > k_DirectionThreshold && velocity.sqrMagnitude > 0.1f)
                    {

                        Instantiate(airEffect, indexPos, Quaternion.identity);
                        
                        // audio.clip = myClip;
                        audio.spatialBlend = 1f;        // 空間オーディオっぽくする
                        audio.minDistance = 1f;         // 音量が減衰し始める距離
                        audio.maxDistance = 10f;        // 音が完全に聞こえなくなる距離
                        audio.loop = false;             // ループ再生（必要に応じて）
                        audio.PlayOneShot(audio.clip);



                        Rigidbody rb = null;
                        if (obj != null)
                        {
                            rb = obj.GetComponent<Rigidbody>();
                        }
                        if (rb != null)
                        {
                            rb.velocity = velocity.normalized * m_ShotForce;
                        }

                        GreenSphere objScript = obj.GetComponent<GreenSphere>();
                        objScript.Fired = true;
                        m_HasFired = true;
                        m_IsAiming = false;
                    }
                }
            }
        }


        m_PreviousIndexPos = indexPos;
        m_HasPreviousIndexPos = true;

        // if (distance > m_ScaledThreshold * 1.2f)
        // {
        //     m_IsAiming = false;
        // }
        if(!m_IsAiming){
            objCreated = false;
            elapsedTime = 0;
        }
    }
}


bool IsFingerExtended(XRHand hand, XRHandJointID tipID, XRHandJointID metacarpalID)
{
    XRHandJoint tipJoint = hand.GetJoint(tipID);
    XRHandJoint metacarpalJoint = hand.GetJoint(metacarpalID);

    if (tipJoint.TryGetPose(out Pose tipPose) && metacarpalJoint.TryGetPose(out Pose metacarpalPose)&&m_Wrist.TryGetPose(out Pose wristPose) && m_Palm.TryGetPose(out Pose palmPose))
    {
        Vector3 fingerDirection = (tipPose.position - metacarpalPose.position).normalized;
        Vector3 handForward = (palmPose.position - wristPose.position).normalized;

        Debug.Log("tin");
        Debug.Log(fingerDirection);
        Debug.Log(handForward);

        return Vector3.Dot(fingerDirection, handForward) > 0.9f;
    }

    return false;
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
