using System.Collections;
using UnityEngine;

public class AirEffectController : MonoBehaviour
{
    public float duration = 2f;
    private Material material;

    void Start()
    {
        // オブジェクトのマテリアルを取得（Rendererが必要）
        material = GetComponent<Renderer>().material;
        StartCoroutine(FadeOutAndDestroy());
    }

    IEnumerator FadeOutAndDestroy()
    {
        float elapsed = 0f;

        // 初期色を取得（アルファ付き）
        Color color = material.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            material.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // 最後にオブジェクトを削除
        Destroy(gameObject);
    }
}
