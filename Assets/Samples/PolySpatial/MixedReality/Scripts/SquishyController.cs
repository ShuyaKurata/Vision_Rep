using UnityEngine;

public class SquishyController : MonoBehaviour
{
    [SerializeField] private float destroyDuration = 1.0f;
    [SerializeField] private ParticleSystem destroyEffect;
    [SerializeField] private AudioSource destroySound;
    [SerializeField] private GameObject backgroundCube;

    private bool isDestroyed = false;

    public void DestroySphere()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // 破壊エフェクト
        if (destroyEffect != null)
        {
            destroyEffect.transform.position = transform.position;
            destroyEffect.Play();
        }

        // サウンド
        if (destroySound != null)
        {
            destroySound.Play();
        }

        // 背景Cubeを表示 (最初は非アクティブにしておく)
        if (backgroundCube != null)
        {
            backgroundCube.SetActive(true);
            backgroundCube.transform.localScale = Vector3.zero;
            StartCoroutine(ExpandCube(backgroundCube));
        }

        // Sphereのフェードアウトと破壊
        StartCoroutine(DestroyAndFadeOut(gameObject));
    }

    private System.Collections.IEnumerator DestroyAndFadeOut(GameObject sphereObject)
    {
        Renderer rend = sphereObject.GetComponent<Renderer>();
        Material mat = rend.material;
        Vector3 originalScale = sphereObject.transform.localScale;
        float elapsed = 0f;

        while (elapsed < destroyDuration)
        {
            float t = elapsed / destroyDuration;
            sphereObject.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            Color color = mat.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            mat.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(sphereObject); // 完全に削除！
    }

    private System.Collections.IEnumerator ExpandCube(GameObject cube)
    {
        Vector3 targetScale = Vector3.one;
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            cube.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, Mathf.SmoothStep(0, 1, t));
            elapsed += Time.deltaTime;
            yield return null;
        }

        cube.transform.localScale = targetScale;
    }
}
