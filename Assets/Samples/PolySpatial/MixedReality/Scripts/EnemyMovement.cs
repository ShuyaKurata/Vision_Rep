// EnemyMovement.cs
using UnityEngine;
using UnityEngine.AI; // NavMeshAgent を使うために必要
using UnityEngine.XR;
using TMPro;
using UnityEngine.UI;
using System.Collections;

// NavMeshAgentコンポーネントがアタッチされていることを保証
[RequireComponent(typeof(NavMeshAgent))]
// Rigidbodyも衝突判定に必要なので追加を推奨 (Inspectorからでも可)
[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    [Header("追跡設定")]
    [SerializeField]
    public Transform m_PlayerTransform; // インスペクターで追跡対象（プレイヤー、カメラなど）を設定する

    [Header("NavMeshAgent 設定")]
    [SerializeField]
    float m_StoppingDistance = 0.5f; // プレイヤーにどれだけ近づいたら停止するか (Agentの設定と同期)

    [SerializeField]
    float m_MoveSpeed = 1f; // NavMeshAgentの移動速度 (Agentの設定と同期)

    [SerializeField]
    float m_RotationSpeed = 120f; // NavMeshAgentの回転速度 (AgentのAngular Speedと同期)

    [Header("衝突設定")]
    [Tooltip("このタグを持つオブジェクトに衝突すると破壊されます")]
    [SerializeField]
    string m_DestructionObjectTag = "DamagingSphere"; // 衝突時に破壊されるオブジェクトのタグ

    NavMeshAgent m_Agent; // NavMeshAgentへの参照
    Rigidbody m_Rigidbody; // Rigidbodyへの参照
    bool m_IsInitialized = false;

    [Header("テキスト")]
    [SerializeField]
    private int hp = 3;
    [SerializeField]
    private int maxHp = 3;
    [SerializeField] 
    private TextMeshPro hpText; // ← TextMeshPro（3Dのやつ）を参照
     [SerializeField] 
    private Slider slider;
     [SerializeField] 
    private TextMeshPro hitAttention; // ← TextMeshPro（3Dのやつ）を参照

    [Header("ドロップ")]
     [SerializeField] 
    private GameObject dropItem;

    [Header("アニメーション")]
    [SerializeField] private Animator animator; // Animator参照
    [SerializeField] private float attackInterval = 2f; // 攻撃間隔
    [SerializeField] private float attackRange = 1.5f; // 攻撃範囲

    private float attackTimer = 0f;
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isDamageMotioning = false;

      [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip hitClip;
    [SerializeField]
    private AudioClip throughClip;
    [SerializeField]
    private AudioClip voiceClip;
    [SerializeField]
    private AudioClip walkClip;

    public Camera mainCamera; // Inspectorでアサイン可能にする
    
     private int instanceId;
     private float _value;

    public void Initialize(int id)
    {
        instanceId = id;
        
    }
    public void RequestDestruction()
    {
        // GameManager.Instance.DestroyEnemy(instanceId);
        StartCoroutine(GameManager.Instance.DestroyEnemy(instanceId));

    }



    void Awake()
    {
        audioSource.PlayOneShot(walkClip);
        m_Agent = GetComponent<NavMeshAgent>();
        m_Rigidbody = GetComponent<Rigidbody>();


        if (mainCamera == null)
        {
            mainCamera = Camera.main; // 念のため自動取得
        }
        if(m_PlayerTransform == null){
            m_PlayerTransform = mainCamera.transform;
        }
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

        UpdateHPText();

        m_IsInitialized = true;
        Debug.Log("Enemy Initialized.", this);
    }

    void Update()
    {


            if (m_IsInitialized && m_PlayerTransform != null && m_Agent.isActiveAndEnabled && m_Agent.isOnNavMesh)
        {
           // 元のワールド座標
            Vector3 a = transform.position;
            Vector3 b = m_PlayerTransform.position;

            // Y を 0 に
            a.y = 0f;
            b.y = 0f;

            // 水平距離だけを計算
            float distance = Vector3.Distance(a, b);
            // Debug.Log($"distance={distance}");

            if(!isDead && !isDamageMotioning) {
                if (distance > m_StoppingDistance && !isAttacking)
                {
                    
                    // 移動中
                    animator.SetBool("isMoving", true);
                    animator.SetBool("Attackable", false);
                    animator.SetBool("isIdle", false);

                    Vector3 targetPos = m_PlayerTransform.position;
                    NavMeshHit hit;

                    if (NavMesh.SamplePosition(targetPos, out hit, 2.0f, NavMesh.AllAreas))
                    {
                        m_Agent.isStopped = false;
                        m_Agent.SetDestination(hit.position);
                    }
                }
                else
                {

                    m_Agent.isStopped = true;
                    m_Agent.ResetPath();

                        // プレイヤーに到達 → 待機 or 攻撃
                        animator.SetBool("isMoving", false);
                        animator.SetBool("isIdle", true);
                        animator.SetBool("Attackable", true);


                        
                        // attackTimer += Time.deltaTime;
                        // if (attackTimer >= attackInterval)
                        // {
                        //     attackTimer = 0f;
                        //     animator.SetTrigger("Attack"); // 攻撃モーション1回だけ
                        //     AttackPlayerIfInRange();

                        //     isAttacking = true;
                        // }
                }
            }
        }
        else if (m_IsInitialized && m_Agent.isActiveAndEnabled && !m_Agent.isOnNavMesh)
        {
            Debug.Log("i can 't move now");
             // Debug.LogWarning("Enemy is not on NavMesh. Waiting for NavMesh generation or repositioning.", this);
             // 必要に応じてNavMeshに最も近い点を探して移動させるなどの処理を追加することもできる
             // NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas);
             // if(hit.hit) m_Agent.Warp(hit.position);
        }

                    // AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    // if (stateInfo.IsTag("Attack") && stateInfo.normalizedTime >= 1f)
                    // {
                    //     AttackPlayerIfInRange();
                    // }

    }

    // // --- 衝突処理 ---
    // void OnCollisionEnter(Collision collision)
    // {
    //     // 衝突した相手のゲームオブジェクトが指定されたタグを持っているか確認
    //     if (!string.IsNullOrEmpty(m_DestructionObjectTag) && collision.gameObject.CompareTag(m_DestructionObjectTag))
    //     {
    //         // このエネミーオブジェクトを破壊する
    //         DestroyEnemy();
    //     }
    // }

    void AttackPlayerIfInRange()
    {
        float distance = Vector3.Distance(transform.position, m_PlayerTransform.position);
        if (distance <= attackRange)
        {
            PlayHitSound();
            Debug.Log("攻撃ヒット！プレイヤーにダメージを与える");
            GameManager.Instance.ReducePlayerHP(1); // GameManagerにダメージ処理を依頼
        }else{
            PlayThroughSound();
        }
         isAttacking = false;
        
    }
     void StartAttackCallBack(){
        isAttacking = true;
    }

    void StartDamageCallBack(){
        hitAttention.gameObject.SetActive(true); 
         m_Agent.isStopped = true;
        m_Agent.ResetPath();

        isDamageMotioning = true;
        animator.SetBool("isDamaged", isDamageMotioning);
    }

    void EndDamageCallBack()
    {
        hitAttention.gameObject.SetActive(false); 
         isDamageMotioning = false;
         animator.SetBool("isDamaged", isDamageMotioning);
        
    }

    void MovingCallBack()
    {
        audioSource.PlayOneShot(walkClip);
        
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


    public void TakeDamage(int amount)
    {
        if (isDead) return;

        audioSource.PlayOneShot(voiceClip);
        hp -= amount;
        UpdateHPText();
        if (hp <= 0)
        {
            // StartCoroutine(Die());
            Die();
        }
        StartDamageCallBack();
    }

    private void Die()
    {
        isDead = true;
        m_Agent.isStopped = true; // NavMesh止める
        animator.SetBool("isDead", true); // 死亡モーション再生
        Debug.Log("敵死亡");

        // ドロップアイテム生成
        // Vector3 spawnPos = transform.position + new Vector3(0f, 1.0f, 0f);
        Instantiate(dropItem, transform.position, transform.rotation);


        
        // Destroy(gameObject);
        RequestDestruction();
    }

    private void UpdateHPText()
    {
        if (hpText != null)
        {
            hpText.text = $"HP: {hp}";
        }
        if(slider != null){
        
        if (maxHp > 0)
        {
            _value = Mathf.Clamp01((float)hp / (float)maxHp);
        }
        else
        {
            Debug.LogWarning("maxHpが0以下です。スライダー更新をスキップします。");
            return; // もしくは _value = 0 にしておく
        }
        slider.value = _value;
        // slider.minValue = 0;
        // slider.maxValue = maxHp;
        // slider.value = hp;
        // slider.value = 0.5f;

        Debug.Log("slider hp"+hp+"maxhp"+ maxHp+"result"+(float)hp / (float)maxHp);
        Debug.Log("value"+ slider.value);
        Debug.Log("max"+ slider.maxValue);
        // slider.value = (float)hp / 3f; 
        }
    }

    public void PlayHitSound() {
        audioSource.PlayOneShot(hitClip);
    }

    public void PlayThroughSound() {
        audioSource.PlayOneShot(throughClip);
    }

}
