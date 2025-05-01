using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
   [SerializeField] private Image redScreenImage; // ← RedScreenのImageコンポーネントをアサイン
    private Material redScreenMaterial;

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
        if (redScreenImage != null)
        {
            // 紐づいているMaterialを取得
            redScreenMaterial = redScreenImage.material;
        }
        else
        {
            Debug.LogError("redScreenImage が設定されていません！");
        }
    }


    public void ReducePlayerHP(int amount)
    {
        playerHP -= amount;
        Debug.Log("痛いー");
        StartCoroutine(FadeInOut(redScreenMaterial, "_alpha", 1f));
        StartCoroutine(FadeInOut(redScreenMaterial, "_hp", 0.5f));
        // redScreenMaterial.color = new Color(1f, 0f, 0f, 1f); // 赤、完全不透明
      


        if (playerHP <= 0)
        {
            playerHP = 0;
            Debug.Log("プレイヤー死亡！");
            // ゲームオーバー処理など
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
            float value = Mathf.Lerp(0f, 1f, t);
            mat.SetFloat(property, value);
            yield return null;
        }

        // フェードアウト (1 → 0)
        timer = 0f;
        while (timer < durationPerPhase)
        {
            timer += Time.deltaTime;
            float t = timer / durationPerPhase;
            float value = Mathf.Lerp(1f, 0f, t);
            mat.SetFloat(property, value);
            yield return null;
        }

        // 最後に完全に0にセットして終了
        mat.SetFloat(property, 0f);
    }


}
