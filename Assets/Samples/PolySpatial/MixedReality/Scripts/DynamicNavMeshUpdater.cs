// DynamicNavMeshUpdater.cs
using UnityEngine;
using Unity.AI.Navigation;         // NavMeshSurface を使うために必要
using UnityEngine.AI;             // NavMesh, NavMeshTriangulation
using UnityEngine.XR.ARFoundation; // ARPlaneManager を使うために必要
using System.Collections;         // Coroutine を使うために必要
using System.Collections.Generic; // List を使うために必要

// シーンに必要なコンポーネントが存在することを保証
// NavMeshSurfaceに加えて、MeshFilterとMeshRendererも必要
[RequireComponent(typeof(NavMeshSurface), typeof(MeshFilter), typeof(MeshRenderer))]
public class DynamicNavMeshUpdater : MonoBehaviour
{
    [Header("参照")]
    [SerializeField]
    ARPlaneManager m_ARPlaneManager; // シーン内のARPlaneManagerへの参照

    [Header("更新設定")]
    [SerializeField]
    float m_UpdateDelay = 1.0f; // NavMesh更新の最小間隔（秒）

    [Header("可視化設定 (Game View)")]
    [SerializeField]
    Material m_NavMeshVisualizationMaterial; // NavMesh表示用の半透明マテリアル (Inspectorで設定)
    [SerializeField]
    bool m_VisualizeNavMeshInGameView = true; // Game Viewでの可視化を有効にするか

    // コンポーネント参照
    NavMeshSurface m_NavMeshSurface;
    MeshFilter m_MeshFilter;
    MeshRenderer m_MeshRenderer;

    // NavMesh更新用
    Coroutine m_UpdateCoroutine;
    bool m_NeedsNavMeshUpdate = false;

    // 可視化用メッシュ
    Mesh m_NavMeshVisualGeometry;

    void Awake()
    {
        // NavMeshSurface 取得
        m_NavMeshSurface = GetComponent<NavMeshSurface>();
        if (m_NavMeshSurface == null)
        {
            Debug.LogError("DynamicNavMeshUpdater: NavMeshSurface component not found on this GameObject.", this);
            enabled = false;
            return;
        }

        // 可視化用コンポーネント取得
        m_MeshFilter = GetComponent<MeshFilter>();
        m_MeshRenderer = GetComponent<MeshRenderer>();

        // 可視化用メッシュの初期化
        m_NavMeshVisualGeometry = new Mesh();
        m_NavMeshVisualGeometry.name = "NavMesh Visual Geometry";
        m_MeshFilter.mesh = m_NavMeshVisualGeometry;

        // マテリアル設定
        if (m_NavMeshVisualizationMaterial != null)
        {
            m_MeshRenderer.material = m_NavMeshVisualizationMaterial;
        }
        else if (m_VisualizeNavMeshInGameView) // 可視化が有効なのにマテリアルがない場合は警告
        {
            Debug.LogWarning("DynamicNavMeshUpdater: NavMesh Visualization Material is not assigned in the inspector. Visualization might not work correctly.", this);
            // 簡単なデフォルトマテリアルを設定することも可能
            // var defaultShader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
            // if(defaultShader) m_MeshRenderer.material = new Material(defaultShader) { color = new Color(0, 0, 1, 0.5f) };
        }

        // 可視化レンダラーの初期状態設定
        m_MeshRenderer.enabled = m_VisualizeNavMeshInGameView;

        // Transformをリセット (NavMeshの頂点はワールド座標なので)
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;


        // ARPlaneManagerの参照を自動で見つける（Inspectorで設定されていない場合）
        if (m_ARPlaneManager == null)
        {
            m_ARPlaneManager = FindObjectOfType<ARPlaneManager>();
            if (m_ARPlaneManager == null)
            {
                Debug.LogError("DynamicNavMeshUpdater: ARPlaneManager not found in the scene. Please assign it in the inspector or ensure one exists.", this);
                enabled = false;
                return;
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("DynamicNavMeshUpdater: ARPlaneManager was found automatically. Assigning it in the inspector is recommended.", this); }
            #endif
        }

        Debug.Log("DynamicNavMeshUpdater Initialized.", this);
    }

    void OnEnable()
    {
        if (m_ARPlaneManager != null)
        {
            m_ARPlaneManager.planesChanged += OnPlanesChanged;
            Debug.Log("Subscribed to ARPlaneManager.planesChanged event.", this);
        }
        RequestNavMeshUpdate(); // 初期ビルドをリクエスト
    }

    void OnDisable()
    {
        if (m_ARPlaneManager != null)
        {
            m_ARPlaneManager.planesChanged -= OnPlanesChanged;
            Debug.Log("Unsubscribed from ARPlaneManager.planesChanged event.", this);
        }
        if (m_UpdateCoroutine != null)
        {
            StopCoroutine(m_UpdateCoroutine);
            m_UpdateCoroutine = null;
        }
    }

    void OnPlanesChanged(ARPlanesChangedEventArgs eventArgs)
    {
        if (eventArgs.added.Count > 0 || eventArgs.updated.Count > 0 || eventArgs.removed.Count > 0)
        {
            RequestNavMeshUpdate();
        }
    }

    void RequestNavMeshUpdate()
    {
        m_NeedsNavMeshUpdate = true;
        if (m_UpdateCoroutine == null)
        {
            m_UpdateCoroutine = StartCoroutine(DelayedNavMeshUpdate());
        }
    }

    IEnumerator DelayedNavMeshUpdate()
    {
        yield return new WaitForSeconds(m_UpdateDelay);

        if (m_NeedsNavMeshUpdate)
        {
            Debug.Log("Building NavMesh...");
            m_NeedsNavMeshUpdate = false;

            // NavMeshをビルド
            m_NavMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh Built Successfully.");

            // --- ここから可視化処理 ---
            if (m_VisualizeNavMeshInGameView)
            {
                UpdateVisualizationMesh(); // ビルド直後に可視化メッシュを更新
            }
            // --- 可視化処理ここまで ---
        }
        m_UpdateCoroutine = null;
    }

    // NavMeshの形状を取得して可視化メッシュを更新する関数
    void UpdateVisualizationMesh()
    {
        // NavMeshの三角形分割情報を取得
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();

        if (triangulation.vertices.Length == 0)
        {
            // NavMeshが存在しない場合はメッシュをクリア
            if (m_NavMeshVisualGeometry.vertexCount > 0) // 不要なクリアを避ける
            {
                m_NavMeshVisualGeometry.Clear();
                // Debug.Log("NavMesh empty, clearing visualization.");
            }
            m_MeshRenderer.enabled = false; // メッシュがないのでレンダラーも無効化
            return;
        }

        // 取得した情報でメッシュを更新
        m_NavMeshVisualGeometry.Clear(); // 既存のデータをクリア
        m_NavMeshVisualGeometry.vertices = triangulation.vertices;
        m_NavMeshVisualGeometry.triangles = triangulation.indices;
        // 必要であれば法線を再計算 (ライティングを使うマテリアルの場合)
        // m_NavMeshVisualGeometry.RecalculateNormals();

        // メッシュが更新されたのでレンダラーを有効化
        m_MeshRenderer.enabled = true;

        // Debug.Log($"NavMesh visualization updated with {triangulation.vertices.Length} vertices and {triangulation.indices.Length / 3} triangles.");
    }

    // Inspectorで可視化設定を変更したときに即時反映させる (任意)
    void OnValidate()
    {
        // エディタ実行中でなくても参照を取得できるように試みる
        if (m_MeshRenderer == null) m_MeshRenderer = GetComponent<MeshRenderer>();

        // 可視化の有効/無効をレンダラーに反映
        if (m_MeshRenderer != null)
        {
            m_MeshRenderer.enabled = m_VisualizeNavMeshInGameView;
        }
    }

     // --- NavMeshSurfaceの設定に関する注意点は省略 (前回のコードと同じ) ---
}