using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
   [SerializeField] private GameObject redScreen; // ← RedScreenのImageコンポーネントをアサイン
    private Material redScreenMaterial;
    [SerializeField] private GameObject blackScreen; // ← blackScreenのImageコンポーネントをアサイン
    private Material blackScreenMaterial;
     [SerializeField] 
    private TextMeshPro playerHpText; // ← TextMeshPro（3Dのやつ）を参照
    [SerializeField]
    private GameObject returnButton;
      [SerializeField] 
    private Slider slider;
     [SerializeField] 
    private GameObject monsterPrefab;

    public float spawnDistance = 2f;

    public float playerHP = 5f;
    public float maxPlayerHP = 5f;

    private float fadeDuration = 1f;
    private float displayDuration = 2f;
    private float typingSpeed = 0.2f;

     [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip alertClip;
    [SerializeField]
    private AudioClip startClip;
    [SerializeField]
    private AudioClip clearClip;

    Dictionary<int, GameObject> enemyInstances = new Dictionary<int, GameObject>();
    private int killCount = 0;
    private int clearCondition = 7;
    public Material backgroundMaterial; // 対象のマテリアル

     public Image[] heartImages;
    public Sprite fullHeartSprite;
    public Sprite halfHeartSprite;
    public Sprite emptyHeartSprite;

    private float startTime;

    void Awake()
    {
        startTime = Time.time;
        Instance = this;
        if (redScreen != null)
        {
            // 紐づいているMaterialを取得
            redScreenMaterial = redScreen.GetComponent<Renderer>().material;
            redScreenMaterial.renderQueue = 9998; // 通常透明オブジェクトは 3000

        }
        else
        {
            Debug.LogError("redScreen が設定されていません！");
        }
        if (blackScreen != null)
        {
            // 紐づいているMaterialを取得
            blackScreenMaterial = blackScreen.GetComponent<Renderer>().material;
            blackScreenMaterial.renderQueue = 9999; // 通常透明オブジェクトは 3000

        }
        if( backgroundMaterial != null)
            backgroundMaterial.SetFloat("_ClipTime", 1f);

         UpdateHearts();
        StartCoroutine(GameFlow());

    }
    void Update()
    {
        
    }

    IEnumerator GameFlow()
    {
        Transform cam = Camera.main.transform;

        audioSource.PlayOneShot(startClip);
        yield return new WaitForSeconds(2f);
        // playerHpText.text = "モンスターの気配がする";
        // // playerHpText.text = "monster is coming";

        // yield return new WaitForSeconds(4f);
        // playerHpText.text = "デコピンで倒してください";
        StartCoroutine(ShowMessages());

        // 一体目：正面
        yield return new WaitForSeconds(6.5f);
        SpawnMonster(cam.position + cam.forward * spawnDistance);

        // 二体目・三体目：右斜め・左斜め
        yield return new WaitForSeconds(5f);
        Vector3 rightDiagonal = (cam.forward + cam.right).normalized;
        Vector3 leftDiagonal = (cam.forward - cam.right).normalized;
        SpawnMonster(cam.position + rightDiagonal * spawnDistance);
        SpawnMonster(cam.position + leftDiagonal * spawnDistance);

        // 四体目〜七体目：カメラを囲むように前後左右
        yield return new WaitForSeconds(5f);
        Vector3[] directions = new Vector3[]
        {
            cam.forward,                  // 前
            -cam.forward,                // 後
            cam.right,                   // 右
            -cam.right                   // 左
        };
        foreach (var dir in directions)
        {
            SpawnMonster(cam.position + dir.normalized * spawnDistance);
        }
    }

    private IEnumerator ShowMessages()
    {
        Color c = playerHpText.color;
        c.a = 0f; // 透明にする
        playerHpText.color = c;

        // メッセージ表示
        playerHpText.text = "モンスターの気配がする";
        
        // フェードイン
        yield return StartCoroutine(FadeText(playerHpText, 0f, 1f, fadeDuration));

        
        yield return new WaitForSeconds(displayDuration);

        // フェードアウト
        yield return StartCoroutine(FadeText(playerHpText, 1f, 0f, fadeDuration));

        // // テキストをクリア
        // playerHpText.text = "";

        // タイプライター効果で新しいメッセージを表示
        // string newMessage = "デコピンで倒してください";
        playerHpText.text = "デコピンで倒してください";
        // yield return StartCoroutine(TypeText(playerHpText, newMessage, typingSpeed));
        yield return StartCoroutine(FadeText(playerHpText, 0f, 1f, fadeDuration));
    }

    private IEnumerator FadeText(TMP_Text text, float startAlpha, float endAlpha, float duration)
    {
        Color color = text.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            text.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // 最終的なアルファ値を設定
        text.color = new Color(color.r, color.g, color.b, endAlpha);
    }

    // private IEnumerator TypeText(TMP_Text text, string message, float speed)
    // {
    //     text.text = message;
    //     text.maxVisibleCharacters = 0;

    //     for (int i = 0; i <= message.Length; i++)
    //     {
    //         text.maxVisibleCharacters = i;
    //         yield return new WaitForSeconds(speed);
    //     }
    // }


    public void ReducePlayerHP(int amount)
    {
        


        if (playerHP <= 1)
        {
            playerHP = 0;
            Debug.Log("プレイヤー死亡！");
            if (playerHpText != null)
            {
                playerHpText.text = "GAME OVER";
            }
            // ゲームオーバー処理など
            GameOver();
        }else{
            playerHP -= amount;
            Debug.Log("痛いー");
            // StartCoroutine(FadeInOut(redScreenMaterial, "_alpha", 2f));
            // redScreen.transform.localScale *= 0.8f;
            StartCoroutine(FadeInOut2(redScreenMaterial,  2f));
            // StartCoroutine(FadeInOut(redScreenMaterial, "_hp", 0.5f));
            // redScreenMaterial.SetFloat("_hp", playerHP+1);
            // redScreenMaterial.color = new Color(1f, 0f, 0f, 1f); // 赤、完全不透明

            // if (playerHpText != null)
            // {
            //     playerHpText.text = $"HP: {playerHP}";
            // }
            
            if(slider != null)
            slider.value = (float)playerHP / (float)maxPlayerHP; ;
            if (playerHP <= 2)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.clip = alertClip;
                    audioSource.volume = 0.4f;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
            

        }
        UpdateHearts();
    }

    // public IEnumerator FadeInOut(Material mat, string property, float durationPerPhase)
    // {
    //     // フェードイン (0 → 1)
    //     float timer = 0f;
    //     while (timer < durationPerPhase)
    //     {
    //         timer += Time.deltaTime;
    //         float t = timer / durationPerPhase;
    //         float value = Mathf.Lerp(0f, 0.8f, t);
    //         mat.SetFloat(property, value);
    //         yield return null;
    //     }

    //     // フェードアウト (1 → 0)
    //     timer = 0f;
    //     while (timer < durationPerPhase)
    //     {
    //         timer += Time.deltaTime;
    //         float t = timer / durationPerPhase;
    //         float value = Mathf.Lerp(0.8f, 0f, t);
    //         mat.SetFloat(property, value);
    //         yield return null;
    //     }

    //     // 最後に完全に0にセットして終了
    //     mat.SetFloat(property, 0f);
    // }
    public IEnumerator FadeIn(Material mat, float durationPerPhase)
    {
        Color color = mat.color;
        // フェードイン (0 → 1)
        float timer = 0f;
        while (timer < durationPerPhase)
        {
            timer += Time.deltaTime;
            float t = timer / durationPerPhase;
            float value = Mathf.Lerp(0f, 0.8f, t);
            color.a = value; 
            mat.color = color;
            yield return null;
        }

    }

    public IEnumerator FadeInOut2(Material mat, float durationPerPhase)
    {
        Color color = mat.color;
        // フェードイン (0 → 1)
        float timer = 0f;
        while (timer < durationPerPhase)
        {
            timer += Time.deltaTime;
            float t = timer / durationPerPhase;
            float value = Mathf.Lerp(0f, 1.0f, t);
            color.a = value; 
            mat.color = color;
            yield return null;
        }

        // フェードアウト (1 → 0)
        timer = 0f;
        while (timer < durationPerPhase)
        {
            timer += Time.deltaTime;
            float t = timer / durationPerPhase;
            float value = Mathf.Lerp(1.0f, 0f, t);
            color.a = value; 
            mat.color = color;
            yield return null;
        }

        
    }

    public void GameClear()
    {

        audioSource.Stop();
        // クリア時のサウンドを再生
        if (clearClip != null)
        {
            audioSource.volume = 1f;
            audioSource.PlayOneShot(clearClip);

        }else{
            Debug.Log("clearsound is not defined??");
        }
        // スローモーションを適用
        Time.timeScale = 0.1f;

        // 秒後にTime.timeScaleを元に戻すコルーチンを開始
        StartCoroutine(RestoreTimeScaleAfterDelay(3f));

        
         // 赤いスクリーンを非表示にする
        if (redScreen != null)
        {
            redScreen.SetActive(false);
        }

        // プレイヤーのHPテキストを更新
        if (playerHpText != null)
        {
            playerHpText.text = "YOU WIN!";
        }

        // フェード処理を開始
        StartCoroutine(FadeClipTime());

        Debug.Log("clear");

        float clearTime = Time.time - startTime;
        Debug.Log("経過時間: " + clearTime + "秒");
             // プレイヤーのHPテキストを更新
        if (playerHpText != null)
        {
            playerHpText.text = $"<align=\"center\">THANK YOU\n{clearTime:F2}秒</align>";

        }

        // リターンボタンを表示
        if (returnButton != null)
        {
            returnButton.SetActive(true);
        }
    }

    // Time.timeScaleを元に戻すコルーチン
    private IEnumerator RestoreTimeScaleAfterDelay(float delay)
    {
        // Time.timeScaleの影響を受けない待機
        yield return new WaitForSecondsRealtime(delay);

        // Time.timeScaleを元に戻す
        Time.timeScale = 1f;
    }


    public void GameOver()
    {
        Debug.Log("終わり");

        // 赤画面を完全表示に
        Color color = redScreenMaterial.color;
        color.a = 1f;
        redScreenMaterial.color = color;

        blackScreen.SetActive(true);
        FadeIn(blackScreenMaterial,1f);


        // 1秒後にシーンを切り替える
        StartCoroutine(LoadSceneAfterDelay());
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(1f); // 1秒待つ
        SceneManager.LoadScene("ProjectLauncher");
    }
    // void SpawnMonster(Vector3 position)
    // {
    //     GameObject monster = Instantiate(monsterPrefab, position, Quaternion.identity);
    //     var movement = monster.GetComponent<EnemyMovement>();
    //     if (movement != null)
    //     {
    //         movement.mainCamera = Camera.main;
    //         movement.m_PlayerTransform = Camera.main.transform;
    //     }
    // }


    public void SpawnMonster(Vector3 pos)
    {
        GameObject obj = Instantiate(monsterPrefab, pos, Quaternion.identity);
        int id = obj.GetInstanceID();
        enemyInstances.Add(id, obj);

        var enemy = obj.GetComponent<EnemyMovement>();
        enemy.Initialize(id);
    }

    public IEnumerator DestroyEnemy(int id)
    {
        killCount++;
        playerHpText.text = "あと" + (clearCondition - killCount) + "体です";

        if(killCount >= clearCondition){
            GameClear();
        }
        
        if (enemyInstances.ContainsKey(id))
        {
            // 死亡モーションの長さだけ待ってから削除
            yield return new WaitForSeconds(3f);
            Destroy(enemyInstances[id]);
            enemyInstances.Remove(id);
        }
    }


   

        private IEnumerator FadeClipTime()
        {
            

            float startValue = backgroundMaterial.GetFloat("_ClipTime");
            float elapsed = 0f;
            float duration = 2f;


            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float newValue = Mathf.Lerp(startValue, 0f, elapsed / duration);
                backgroundMaterial.SetFloat("_ClipTime", newValue);
                yield return null;
            }

            backgroundMaterial.SetFloat("_ClipTime", 0f);
            yield return new WaitForSeconds(3f); 
            // Destroy(gameObject); // オブジェクトを削除
        }
    public void Recover(){
        if(playerHP <= maxPlayerHP - 0.5)
        playerHP += 0.5f;
         if(slider != null)
        slider.value = (float)playerHP / (float)maxPlayerHP; 
        if(playerHP >= 3)
            {
                // HPが回復したらアラートを止める
                if (audioSource.isPlaying && audioSource.clip == alertClip)
                {
                    audioSource.Stop();
                    audioSource.loop = false;
                }
            }
        UpdateHearts();
    }
    public void UpdateHearts()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < playerHP)
            {
                if (playerHP - i >= 1)
                    heartImages[i].sprite = fullHeartSprite;
                else
                    heartImages[i].sprite = halfHeartSprite;
            }
            else
            {
                heartImages[i].sprite = emptyHeartSprite;
            }
        }
    }


}
