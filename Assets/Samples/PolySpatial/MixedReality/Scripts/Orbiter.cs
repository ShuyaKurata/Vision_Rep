// using UnityEngine;

// public class Orbiter : MonoBehaviour
// {
//     public Transform centralSphere;
//     public float radius = 2f;
//     public float speed = 1f;

//     [Tooltip("軌道面の傾き角度（度）")]
//     [Range(0, 360)]
//     public float orbitTiltAngle = 60f;

//     private float angle = 0f;

//     void Update()
//     {
//         if (centralSphere == null) return;

//         angle += speed * Time.deltaTime;

//         // 基本のXZ平面で円運動
//         Vector3 localOffset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

//         // 軌道面をX軸まわりに回転（＝Z軸を基準に円を描く面を傾ける）
//         Quaternion tiltRotation = Quaternion.Euler(orbitTiltAngle, 0f, 0f);
//         Vector3 rotatedOffset = tiltRotation * localOffset;

//         transform.position = centralSphere.position + rotatedOffset;
//     }
// }
using UnityEngine;

public class Orbiter : MonoBehaviour
{
    public Transform centralSphere;
    public float orbitRadiusMultiplier = 1.2f;
    public float orbitalPeriod = 1f;
    public float scaleMultiplier = 0.2f;

    [Tooltip("軌道面の傾き角度（度）")]
    [Range(0, 360)]
    public float orbitTiltAngle = 60f;

    private float angle = 0f;

    void Update()
    {
        if (centralSphere == null) return;
        // 中心球の基準サイズ
        float centralRadius = centralSphere.localScale.x * 0.5f;

        // 半径を中心球のスケールから算出
        float radius = (centralSphere.localScale.x * 0.5f) * orbitRadiusMultiplier;

        // 自身のスケールを中心球に合わせて調整
        transform.localScale = Vector3.one * centralRadius * scaleMultiplier;

        // 角速度 = 2π / 周期
        float angularSpeed = (2f * Mathf.PI) / orbitalPeriod;

        angle += angularSpeed * Time.deltaTime;

        // 基本のXZ平面で円運動
        Vector3 localOffset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

        // 軌道面をX軸まわりに傾ける
        Quaternion tiltRotation = Quaternion.Euler(orbitTiltAngle, 0f, 0f);
        Vector3 rotatedOffset = tiltRotation * localOffset;

        // 新しい位置を設定
        transform.position = centralSphere.position + rotatedOffset;
    }
}
