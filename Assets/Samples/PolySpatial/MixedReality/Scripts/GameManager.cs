using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
   [SerializeField] private GameObject redScreen; // ← RedScreenのImageコンポーネントをアサイン
    private Material redScreenMaterial;
     [SerializeField] 
    private TextMeshPro playerHpText; // ← TextMeshPro（3Dのやつ）を参照

    public int playerHP = 5;
    public int maxPlayerHP = 5;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        if (redScreen != null)
        {
            // 紐づいているMaterialを取得
            redScreenMaterial = redScreen.GetComponent<Renderer>().material;
            redScreenMaterial.renderQueue = 9999; // 通常透明オブジェクトは 3000

        }
        else
        {
            Debug.LogError("redScreen が設定されていません！");
        }
    }


    public void ReducePlayerHP(int amount)
    {
        


        if (playerHP <= 0)
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

            if (playerHpText != null)
            {
                playerHpText.text = $"HP: {playerHP}";
            }
        }
    }

    public IEnumerator FadeInOut(Material mat, string property, float durationPerPhase)
    {
        // フェードイン (0 → 1)
        float timer = 0f;
        while (timer < durationPerPhase)
        {
            timer += Time.deltaTime;
            float t = timer / durationPerPhase;
            float value = Mathf.Lerp(0f, 0.8f, t);
            mat.SetFloat(property, value);
            yield return null;
        }

        // フェードアウト (1 → 0)
        timer = 0f;
        while (timer < durationPerPhase)
        {
            timer += Time.deltaTime;
            float t = timer / durationPerPhase;
            float value = Mathf.Lerp(0.8f, 0f, t);
            mat.SetFloat(property, value);
            yield return null;
        }

        // 最後に完全に0にセットして終了
        mat.SetFloat(property, 0f);
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
        redScreen.SetActive(false);
    }

    public void GameOver()
    {
        Debug.Log("終わり");

        // 赤画面を完全表示に
        Color color = redScreenMaterial.color;
        color.a = 1f;
        redScreenMaterial.color = color;

        // 1秒後にシーンを切り替える
        StartCoroutine(LoadSceneAfterDelay());
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(1f); // 1秒待つ
        SceneManager.LoadScene("ProjectLauncher");
    }


}
