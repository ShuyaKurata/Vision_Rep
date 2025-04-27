using UnityEngine;
#if UNITY_INCLUDE_XR_HANDS
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
#endif

namespace PolySpatial.Samples
{
    public class GripAmountAdjuster : MonoBehaviour
    {
        [SerializeField]
        Camera m_Camera; // カメラを指定
         [SerializeField]
        Transform m_PolySpatialCameraTransform;

#if UNITY_INCLUDE_XR_HANDS
        XRHandSubsystem m_HandSubsystem;
        XRHandJoint m_RightIndexTipJoint;
        XRHandJoint m_RightThumbTipJoint;
        XRHandJoint m_LeftIndexTipJoint;
        XRHandJoint m_LeftThumbTipJoint;
        float m_ScaledThreshold;

        const float k_PinchThreshold = 0.02f;

        void Start()
        {
            GetHandSubsystem();
            m_ScaledThreshold = k_PinchThreshold /m_PolySpatialCameraTransform.localScale.x; // カメラスケールに基づくスレッショルドの調整
        }

        void Update()
        {
            if (!CheckHandSubsystem())
                return;

            var updateSuccessFlags = m_HandSubsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

            if ((updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose) != 0)
            {
                // assign joint values
                m_RightIndexTipJoint = m_HandSubsystem.rightHand.GetJoint(XRHandJointID.IndexTip);
                m_RightThumbTipJoint = m_HandSubsystem.rightHand.GetJoint(XRHandJointID.ThumbTip);

                // Update the grip amount based on the pinch distance
                UpdateGripAmount(m_RightIndexTipJoint, m_RightThumbTipJoint);
            }

            if ((updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose) != 0)
            {
                // assign joint values
                m_LeftIndexTipJoint = m_HandSubsystem.leftHand.GetJoint(XRHandJointID.IndexTip);
                m_LeftThumbTipJoint = m_HandSubsystem.leftHand.GetJoint(XRHandJointID.ThumbTip);

                // Update the grip amount based on the pinch distance
                UpdateGripAmount(m_LeftIndexTipJoint, m_LeftThumbTipJoint);
            }
        }

        void GetHandSubsystem()
        {
            var xrGeneralSettings = XRGeneralSettings.Instance;
            if (xrGeneralSettings == null)
            {
                Debug.LogError("XR general settings not set");
            }

            var manager = xrGeneralSettings.Manager;
            if (manager != null)
            {
                var loader = manager.activeLoader;
                if (loader != null)
                {
                    m_HandSubsystem = loader.GetLoadedSubsystem<XRHandSubsystem>();
                    if (!CheckHandSubsystem())
                        return;

                    m_HandSubsystem.Start();
                }
            }
        }

        bool CheckHandSubsystem()
        {
            if (m_HandSubsystem == null)
            {
#if !UNITY_EDITOR
                Debug.LogError("Could not find Hand Subsystem");
#endif
                enabled = false;
                return false;
            }

            return true;
        }

        // 手の握り具合に基づいてgripAmountを更新するメソッド
        void UpdateGripAmount(XRHandJoint index, XRHandJoint thumb)
        {
            Debug.Log("yoisyo");
            if (index.trackingState != XRHandJointTrackingState.None &&
                thumb.trackingState != XRHandJointTrackingState.None)
            {
                // レイキャストを使ってカメラから対象オブジェクトを取得
                Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition); // ここでカメラからのレイを飛ばす
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    // ヒットしたオブジェクトのmaterial.gripAmountを調整
                    Renderer renderer = hit.collider.GetComponent<Renderer>();
                    Vector3 indexPOS = Vector3.zero;
                    Vector3 thumbPOS = Vector3.zero;
                        if (index.TryGetPose(out Pose indexPose))
                    {
                        // adjust transform relative to the PolySpatial Camera transform
                        indexPOS = m_PolySpatialCameraTransform.InverseTransformPoint(indexPose.position);
                    }

                    if (thumb.TryGetPose(out Pose thumbPose))
                    {
                        // adjust transform relative to the PolySpatial Camera adjustments
                        thumbPOS = m_PolySpatialCameraTransform.InverseTransformPoint(thumbPose.position);
                    }
                    if (renderer != null)
                    {
                        float gripAmount = Mathf.Clamp01(Vector3.Distance(indexPOS, thumbPOS) / k_PinchThreshold);
                        Material material = renderer.material;

                        if (material.HasProperty("_GripAmount")) // 物体にGripAmountのプロパティがあるかチェック
                        {
                            material.SetFloat("_GripAmount", gripAmount); // gripAmountを設定
                        }
                    }
                }
            }
        }
#endif
    }
}
