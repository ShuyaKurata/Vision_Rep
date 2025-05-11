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

    public int playerHP = 5;
    public int maxPlayerHP = 5;

    private float fadeDuration = 1f;
    private float displayDuration = 2f;
    private float typingSpeed = 0.2f;

    void Awake()
    {
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
        StartCoroutine(GameFlow());
    }

    IEnumerator GameFlow()
    {
        Transform cam = Camera.main.transform;

        yield return new WaitForSeconds(2f);
        // playerHpText.text = "モンスターの気配がする";
        // // playerHpText.text = "monster is coming";

        // yield return new WaitForSeconds(4f);
        // playerHpText.text = "デコピンで倒してください";
        StartCoroutine(ShowMessages());

        // 一体目：正面
        yield return new WaitForSeconds(5f);
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
            

            slider.value = (float)playerHP / (float)maxPlayerHP; ;
        }
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

    public void GameClear(){
        Debug.Log("clera");
        if(redScreen != null){
        redScreen.SetActive(false);
        }
         if(playerHpText != null){
        playerHpText.text = "THANK YOU!";
        }
        returnButton.SetActive(true);
        
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
    void SpawnMonster(Vector3 position)
    {
        GameObject monster = Instantiate(monsterPrefab, position, Quaternion.identity);
        var movement = monster.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.mainCamera = Camera.main;
            movement.m_PlayerTransform = Camera.main.transform;
        }
    }


}
