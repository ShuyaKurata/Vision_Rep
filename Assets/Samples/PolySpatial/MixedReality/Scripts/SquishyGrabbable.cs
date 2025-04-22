using UnityEngine;
using UnityEngine.XR.Hands;

public class GazeGrabManager : MonoBehaviour
{
    [SerializeField] private XRHandSubsystem handSubsystem;
    [SerializeField] private float grabDistance = 0.15f;
    [SerializeField] private Camera mainCamera;

    private GameObject currentGrabbableObject;
    private Material currentMaterial;
    private float gripAmount = 0f;

    void Update()
    {
        if (handSubsystem == null || !handSubsystem.running)
            return;

        var rightHand = handSubsystem.rightHand;
        if (!rightHand.isTracked)
            return;

        // 指の曲がり具合やピンチ距離をチェック
        float indexBend = GetFingerBend(rightHand, XRHandJointID.IndexTip, XRHandJointID.IndexProximal);
        float middleBend = GetFingerBend(rightHand, XRHandJointID.MiddleTip, XRHandJointID.MiddleProximal);
        float ringBend = GetFingerBend(rightHand, XRHandJointID.RingTip, XRHandJointID.RingProximal);
        float pinkyBend = GetFingerBend(rightHand, XRHandJointID.LittleTip, XRHandJointID.LittleProximal);
        float averageBend = (indexBend + middleBend + ringBend + pinkyBend) / 4f;

        Pose thumbPose, indexPose;
        if (!rightHand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out thumbPose) ||
            !rightHand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out indexPose))
            return;

        float pinch = Vector3.Distance(thumbPose.position, indexPose.position);

        Pose palmPose;
        if (!rightHand.GetJoint(XRHandJointID.Palm).TryGetPose(out palmPose))
            return;

        float distanceToObject = currentGrabbableObject != null
            ? Vector3.Distance(palmPose.position, currentGrabbableObject.transform.position)
            : Mathf.Infinity;

        // 注視対象取得（未選択時のみ）
        if (currentGrabbableObject == null)
        {
            GameObject target = GetTargetObjectByGaze();
            if (target != null)
            {
                currentGrabbableObject = target;
                currentMaterial = target.GetComponent<Renderer>()?.material;
            }
        }

        // 掴む条件を満たしているか
        bool isGripping = (averageBend > 0.3f && pinch < 0.04f && distanceToObject < grabDistance && currentGrabbableObject != null);

        // GripAmount 補間
        gripAmount = Mathf.Lerp(gripAmount, isGripping ? 1f : 0f, Time.deltaTime * 10f);

        Debug.Log(gripAmount);

        // マテリアル更新
        if (currentMaterial != null)
        {
            currentMaterial.SetFloat("_GripAmount", gripAmount);
            currentMaterial.SetColor("_Color", Color.Lerp(Color.white, Color.cyan, gripAmount));
        }

        // 離したら対象をリセット
        if (!isGripping && gripAmount <= 0.01f)
        {
            currentGrabbableObject = null;
            currentMaterial = null;
        }
    }

    GameObject GetTargetObjectByGaze()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.0f)) // 1m以内
        {
            if (hit.collider.CompareTag("Grabbable"))
            {
                return hit.collider.gameObject;
            }
        }
        return null;
    }

    float GetFingerBend(XRHand hand, XRHandJointID tip, XRHandJointID baseJoint)
    {
        if (hand.GetJoint(tip).TryGetPose(out Pose tipPose) &&
            hand.GetJoint(baseJoint).TryGetPose(out Pose basePose))
        {
            float dist = Vector3.Distance(tipPose.position, basePose.position);
            return Mathf.Clamp01(1f - dist * 5f); // 短い距離 = よく曲がってる
        }
        return 0f;
    }
}
