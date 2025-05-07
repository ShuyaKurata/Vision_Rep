using UnityEngine;
using System.Collections;

#if UNITY_INCLUDE_XR_HANDS
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
#endif

namespace PolySpatial.Samples
{
    public class SquishyTest : MonoBehaviour
    {
        [SerializeField]
        GameObject target;

        private Transform m_PolySpatialCameraTransform;
        private GameObject backgroundCube;
        public Material targetMaterial; // 対象のマテリアル
        public float duration = 2.0f;    // フェードアウトにかける時間（秒）
        [SerializeField]
        private GameObject gameManager; 



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
            // 1つしかないカメラのTransformを取得するならタグとか付けておくと安全
            GameObject cameraObj = GameObject.FindWithTag("PolySpatialCamera");
            if (cameraObj != null)
            {
                m_PolySpatialCameraTransform = cameraObj.transform;
            }
            else
            {
                Debug.LogWarning("PolySpatialCameraが見つからない！");
            }

            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            GameObject _backgroundCube = null;

            foreach (GameObject obj in allObjects)
            {
                if (obj.CompareTag("BackgroundCube"))
                {
                    _backgroundCube = obj;
                    break;
                }
            }
            foreach (GameObject obj in allObjects)
            {
                if (obj.CompareTag("GameManager"))
                {
                    gameManager = obj;
                    break;
                }
            }

            if (_backgroundCube == null)
            {
                Debug.LogWarning("BackgroundCubeが見つからない！");
            }
            else
            {
                Debug.Log("BackgroundCubeが見つかった！");
            }

            backgroundCube = _backgroundCube;
            targetMaterial = backgroundCube.GetComponent<Renderer>().material;
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
                // m_RightIndexTipJoint = m_HandSubsystem.rightHand.GetJoint(XRHandJointID.IndexTip);
                // m_RightThumbTipJoint = m_HandSubsystem.rightHand.GetJoint(XRHandJointID.ThumbTip);

                // Update the grip amount based on the pinch distance
                // UpdateGripAmount(m_RightIndexTipJoint, m_RightThumbTipJoint);
                 UpdateGripAmount(m_HandSubsystem.rightHand);
            }

            if ((updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose) != 0)
            {
                // assign joint values
                // m_LeftIndexTipJoint = m_HandSubsystem.leftHand.GetJoint(XRHandJointID.IndexTip);
                // m_LeftThumbTipJoint = m_HandSubsystem.leftHand.GetJoint(XRHandJointID.ThumbTip);

                // Update the grip amount based on the pinch distance
                // UpdateGripAmount(m_LeftIndexTipJoint, m_LeftThumbTipJoint);
                UpdateGripAmount(m_HandSubsystem.leftHand);
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

        // // 手の握り具合に基づいてgripAmountを更新するメソッド
        // void UpdateGripAmount(XRHandJoint index, XRHandJoint thumb)
        // {

        //     if (index.trackingState != XRHandJointTrackingState.None &&
        //         thumb.trackingState != XRHandJointTrackingState.None)
        //     {
            
        //             // ヒットしたオブジェクトのmaterial.gripAmountを調整
        //             Renderer renderer = target.GetComponent<Renderer>();
        //             Vector3 indexPOS = Vector3.zero;
        //             Vector3 thumbPOS = Vector3.zero;

        //                 if (index.TryGetPose(out Pose indexPose))
        //             {
        //                 // adjust transform relative to the PolySpatial Camera transform
        //                 indexPOS = m_PolySpatialCameraTransform.InverseTransformPoint(indexPose.position);
        //             }

        //             if (thumb.TryGetPose(out Pose thumbPose))
        //             {
        //                 // adjust transform relative to the PolySpatial Camera adjustments
        //                 thumbPOS = m_PolySpatialCameraTransform.InverseTransformPoint(thumbPose.position);
        //             }
        //             if (renderer != null)
        //             {
        //                 float distance = Vector3.Distance(indexPOS, thumbPOS);
        //                 float gripAmount = 1.0f - Mathf.Clamp01(distance / k_PinchThreshold);

        //                 Material material = renderer.material;

        //                 if (material.HasProperty("_GripAmount")) // 物体にGripAmountのプロパティがあるかチェック
        //                 {
        //                     material.SetFloat("_GripAmount", gripAmount); // gripAmountを設定
        //                 }
        //                 Debug.Log($"Pinch Distance: {distance} | Grip Amount: {gripAmount}");

        //                 // Mathf.Approximately でほぼ同値を判定
        //                 if (gripAmount >0.5)
        //                 {
        //                     DestroySphere();
        //                 }
        //             }

        //     }
        // }


        bool isReadyToGrip = false; // 最初はfalse、パーでtrueになる
        const float k_GripThreshold = 0.1f; // ここ追加！
        private float gripHoldTimer = 0f; 
        private const float requiredHoldTime = 2f; // 2秒

        void UpdateGripAmount(XRHand hand)
        {
            if (hand == null || hand.GetJoint(XRHandJointID.Palm).trackingState == XRHandJointTrackingState.None)
                return;

            XRHandJoint middleProximal = hand.GetJoint(XRHandJointID.MiddleProximal);
                Vector3 middleProximalPos = Vector3.zero;


            if (middleProximal.TryGetPose(out Pose middleProximalPose))
            {
                middleProximalPos = m_PolySpatialCameraTransform.InverseTransformPoint(middleProximalPose.position);
            }
            else
            {
                // pose取れなかった時の処理（必要なら）
                Debug.LogWarning("middle proximal pose not available.");
                return; // pose取れないならここで処理をスキップしてもOK
            }

            float totalDistance = 0f;
            int fingerCount = 0;

            XRHandJointID[] fingerTips = {
                XRHandJointID.IndexTip,
                XRHandJointID.MiddleTip,
                XRHandJointID.RingTip,
                XRHandJointID.LittleTip
            };

            foreach (var fingerTipID in fingerTips)
            {
                XRHandJoint fingerTip = hand.GetJoint(fingerTipID);

                if (fingerTip.trackingState != XRHandJointTrackingState.None && fingerTip.TryGetPose(out Pose fingerPose))
                {
                    Vector3 fingerPos = m_PolySpatialCameraTransform.InverseTransformPoint(fingerPose.position);
                    float distance = Vector3.Distance(fingerPos, middleProximalPos);
                    totalDistance += distance;
                    fingerCount++;
                }
            }

            if (fingerCount > 0)
            {
                float averageDistance = totalDistance / fingerCount;
                float gripAmount = 1.0f - Mathf.Clamp01(averageDistance / k_GripThreshold);

                
                Debug.Log($"Average Finger Distance: {averageDistance} | Grip Amount: {gripAmount}");

                // 手首とオブジェクトの距離チェック
                XRHandJoint wrist = hand.GetJoint(XRHandJointID.Wrist);
                float wristToObjectDistance = 0f;

                if (wrist.trackingState != XRHandJointTrackingState.None && wrist.TryGetPose(out Pose wristPose))
                {
                    Vector3 wristPos = m_PolySpatialCameraTransform.InverseTransformPoint(wristPose.position);
                    wristToObjectDistance = Vector3.Distance(wristPos, target.transform.position);
                }

                // ここでパー状態を検出（gripAmountが小さいとき）
                if (wristToObjectDistance < 0.3f)
                {
                    if (gripAmount < 0.2f ){
                    isReadyToGrip = true;
                    }
                }else{
                    isReadyToGrip = false;
                }
                


                // グーで、パー状態を経由して、手首も近づいてたらOK
                if ( isReadyToGrip)
                {
                    Renderer renderer = target.GetComponent<Renderer>();
                    if (renderer != null && renderer.material.HasProperty("_GripAmount"))
                    {
                        renderer.material.SetFloat("_GripAmount", gripAmount);
                    }
                    if (gripAmount > 0.4f)
                    {
                        gripHoldTimer += Time.deltaTime; // 毎フレーム、時間を足していく
                        if (gripHoldTimer >= requiredHoldTime)
                        {
                            DestroySphere();
                            gripHoldTimer = 0f; // タイマーリセット（1回だけ呼びたいなら必要）
                        }
                    }
                    else
                    {
                        gripHoldTimer = 0f; // 0.6を下回ったらタイマーリセット
                    }

                    
                    // isReadyToGrip = false; // 一度グリップが成立したらリセット
                }else{
                    Renderer renderer = target.GetComponent<Renderer>();
                    if (renderer != null && renderer.material.HasProperty("_GripAmount"))
                    {
                        renderer.material.SetFloat("_GripAmount", 0);
                    }
                }
            }
        }

//         void UpdateGripAmount(XRHand hand)
// {
//     if (hand == null )
//     {
//         Debug.LogWarning("Hand is not tracked.");
//         return;
//     }

//     XRHandJoint palm = hand.GetJoint(XRHandJointID.Palm);
//     if (palm.trackingState == XRHandJointTrackingState.None)
//     {
//         Debug.LogWarning("Palm joint is not tracked.");
//         return;
//     }
//     if(palm == null){
//         Debug.Log("whre is palm!");
//     }

//     if (palm.TryGetPose(out Pose palmPose))
//     {
//         Vector3 palmPos = m_PolySpatialCameraTransform.InverseTransformPoint(palmPose.position);
//         // ここで palmPos を使用した処理を行う
//     }
//     else
//     {
//         Debug.LogWarning("Palm pose not available.");
//     }
//     Debug.Log("unkoburiburi");
// }



        public void DestroySphere()
        {
            
            // 背景Cubeを表示 (最初は非アクティブにしておく)
            // if (backgroundCube != null)
            // {
            //     backgroundCube.SetActive(true);
                
            // }
            //  Destroy(this.gameObject); // 完全に削除！

             if (targetMaterial != null)
            {
                StartCoroutine(FadeClipTime());
            }
            else
            {
                Debug.LogWarning("ターゲットマテリアルが設定されていません。");
            }

        }

        private IEnumerator FadeClipTime()
        {
            // レンダラーだけ無効化
            var rend = GetComponent<Renderer>();
            if (rend != null) rend.enabled = false;

            float startValue = targetMaterial.GetFloat("_ClipTime");
            float elapsed = 0f;

            GameManager gameManagerSc = gameManager.GetComponent<GameManager>();
            gameManagerSc.GameClear();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float newValue = Mathf.Lerp(startValue, 0f, elapsed / duration);
                targetMaterial.SetFloat("_ClipTime", newValue);
                yield return null;
            }

            targetMaterial.SetFloat("_ClipTime", 0f);
            Destroy(gameObject); // オブジェクトを削除
        }
#endif
    }
}
