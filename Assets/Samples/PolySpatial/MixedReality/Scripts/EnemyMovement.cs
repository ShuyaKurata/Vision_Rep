// EnemyMovement.cs
using UnityEngine;
using UnityEngine.AI; // NavMeshAgent を使うために必要
using UnityEngine.XR;


// NavMeshAgentコンポーネントがアタッチされていることを保証
[RequireComponent(typeof(NavMeshAgent))]
// Rigidbodyも衝突判定に必要なので追加を推奨 (Inspectorからでも可)
[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    [Header("追跡設定")]
    [SerializeField]
    Transform m_PlayerTransform; // インスペクターで追跡対象（プレイヤー、カメラなど）を設定する

    [Header("NavMeshAgent 設定")]
    [SerializeField]
    float m_StoppingDistance = 0.5f; // プレイヤーにどれだけ近づいたら停止するか (Agentの設定と同期)

    [SerializeField]
    float m_MoveSpeed = 3.5f; // NavMeshAgentの移動速度 (Agentの設定と同期)

    [SerializeField]
    float m_RotationSpeed = 120f; // NavMeshAgentの回転速度 (AgentのAngular Speedと同期)

    [Header("衝突設定")]
    [Tooltip("このタグを持つオブジェクトに衝突すると破壊されます")]
    [SerializeField]
    string m_DestructionObjectTag = "DamagingSphere"; // 衝突時に破壊されるオブジェクトのタグ

    NavMeshAgent m_Agent; // NavMeshAgentへの参照
    Rigidbody m_Rigidbody; // Rigidbodyへの参照
    bool m_IsInitialized = false;

    void Awake()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Rigidbody = GetComponent<Rigidbody>();

        // Rigidbodyの設定 (Is Kinematic推奨)
        if (m_Rigidbody != null)
        {
            m_Rigidbody.isKinematic = true; // NavMeshAgentで動かす場合、物理演算の影響を受けないようにする
            m_Rigidbody.useGravity = false; // 重力もNavMeshAgentに任せるか、不要なら切る
        }
        else
        {
            Debug.LogWarning("EnemyMovement: Rigidbody component not found, adding one. Collision detection might not work without it.", this);
            m_Rigidbody = gameObject.AddComponent<Rigidbody>();
            m_Rigidbody.isKinematic = true;
            m_Rigidbody.useGravity = false;
        }


        if (m_Agent == null)
        {
            Debug.LogError("EnemyMovement: NavMeshAgent component not found on this GameObject.", this);
            enabled = false; // エラー時はスクリプトを無効化
            return;
        }

        // 追跡対象が設定されているか確認
        if (m_PlayerTransform == null)
        {
            Debug.LogError("EnemyMovement: Player Transform is not assigned in the inspector. Please assign the target.", this);
            // 必要であればここでプレイヤーをタグなどで検索する処理を追加
            // var playerObject = GameObject.FindWithTag("Player");
            // if (playerObject != null) m_PlayerTransform = playerObject.transform;
            // else enabled = false;
            enabled = false;
            return;
        }

        // タグが設定されているか確認
        if (string.IsNullOrEmpty(m_DestructionObjectTag))
        {
            Debug.LogWarning("EnemyMovement: Destruction Object Tag is not set in the inspector. Collision destruction will not work.", this);
        }
        // タグが存在するかチェック (任意)
        // try { UnityEditor.TagManager.CheckTag(m_DestructionObjectTag); } // Editor専用
        // catch { Debug.LogError($"EnemyMovement: Tag '{m_DestructionObjectTag}' is not defined in the Tag Manager.", this); }


        // NavMeshAgentのパラメータを設定
        ApplyAgentSettings();

        m_IsInitialized = true;
        Debug.Log("Enemy Initialized.", this);
    }

    void Update()
    {
            Debug.Log("un");
            Debug.Log(m_PlayerTransform.position);
            Debug.Log(Camera.main.transform.position);
        //     InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);

        // if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 headPos))
        // {
        //     Debug.Log("頭の位置: " + headPos);
        // }

            Debug.Log("komasi");

        // 初期化済みで、追跡対象があり、AgentがNavMesh上で有効な場合
        if (m_IsInitialized && m_PlayerTransform != null && m_Agent.isActiveAndEnabled && m_Agent.isOnNavMesh)
{
    Vector3 targetPos = m_PlayerTransform.position;

    NavMeshHit hit;
    // 地面のNavMesh上に「近い位置」があるか探す（半径2.0f以内）
    if (NavMesh.SamplePosition(targetPos, out hit, 2.0f, NavMesh.AllAreas))
    {
        m_Agent.SetDestination(hit.position); // ← NavMesh上の位置にスナップして渡す
    }
}

        else if (m_IsInitialized && m_Agent.isActiveAndEnabled && !m_Agent.isOnNavMesh)
        {
             // Debug.LogWarning("Enemy is not on NavMesh. Waiting for NavMesh generation or repositioning.", this);
             // 必要に応じてNavMeshに最も近い点を探して移動させるなどの処理を追加することもできる
             // NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas);
             // if(hit.hit) m_Agent.Warp(hit.position);
        }

    }

    // --- 衝突処理 ---
    void OnCollisionEnter(Collision collision)
    {
        // 衝突した相手のゲームオブジェクトが指定されたタグを持っているか確認
        if (!string.IsNullOrEmpty(m_DestructionObjectTag) && collision.gameObject.CompareTag(m_DestructionObjectTag))
        {
            // このエネミーオブジェクトを破壊する
            DestroyEnemy();
        }
    }

    // 敵を破壊する処理
    void DestroyEnemy()
    {
        Debug.Log($"Enemy destroyed by object with tag '{m_DestructionObjectTag}'.", this);
        // 必要であれば破壊エフェクトの生成などをここで行う
        // 例: Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    // --- Inspectorでの設定値の動的反映（任意）---
    void OnValidate()
    {
        if (m_Agent == null) m_Agent = GetComponent<NavMeshAgent>();
        if (m_Agent != null) ApplyAgentSettings();

        // エディタ上でRigidbodyのKinematic設定を強制する（任意）
        if (m_Rigidbody == null) m_Rigidbody = GetComponent<Rigidbody>();
        if (m_Rigidbody != null)
        {
             m_Rigidbody.isKinematic = true;
             m_Rigidbody.useGravity = false;
        }
    }

    // NavMeshAgentにInspectorの設定値を適用するヘルパー関数
    void ApplyAgentSettings()
    {
        if (m_Agent != null)
        {
            m_Agent.stoppingDistance = m_StoppingDistance;
            m_Agent.speed = m_MoveSpeed;
            m_Agent.angularSpeed = m_RotationSpeed;
        }
    }
}
