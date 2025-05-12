using System.Collections;
using UnityEngine;

public class SquishyController : MonoBehaviour
{
    [SerializeField] private float destroyDuration = 1.0f;
    [SerializeField] private ParticleSystem destroyEffect;
    [SerializeField] private AudioSource destroySound;

    private bool isDestroyed = false;
    private float timer = 0f;
    private float rotateSpeed = 0.3f;
    private float riseSpeed = 0.2f;
    public void DestroyItem()
    {
        Debug.Log("i am destroyitem");
        StartCoroutine(DestroyEffect());
        GameManager.Instance.Recover();
        
    }

    void Update()
    {
        

                transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        // タイマーを更新
        timer += Time.deltaTime;
        if (timer >= 7f)
        {
            StartCoroutine(DestroyAndFadeOut(gameObject,Color.white,Color.white));
        }
    }

    IEnumerator DestroyEffect()
    {
        if (isDestroyed) yield break;
        isDestroyed = true;

        if (destroySound != null)
        {
            destroySound.PlayOneShot(destroySound.clip);
        }

        if (destroyEffect != null)
        {
            destroyEffect.transform.position = transform.position;
            destroyEffect.Play();
        }

        yield return StartCoroutine(DestroyAndFadeOut(gameObject,Color.white,new Color(250/255, 1, 0, 1)));
    }

    // private IEnumerator DestroyAndFadeOut(GameObject sphereObject)
    // {
    //     Renderer rend = sphereObject.GetComponent<Renderer>();
    //     Material mat = rend.material;
    //     Vector3 originalScale = sphereObject.transform.localScale;
    //     float elapsed = 0f;

    //     while (elapsed < destroyDuration)
    //     {
    //         float t = elapsed / destroyDuration;
    //         rend.material.SetFloat("_GripAmount", t);
    //         elapsed += Time.deltaTime;
    //         yield return null;
    //     }

    //     Destroy(sphereObject);
    // }
    private IEnumerator DestroyAndFadeOut(
        GameObject sphereObject,
        Color startColor,
        Color endColor)
    {
        Renderer rend = sphereObject.GetComponent<Renderer>();
        Material mat = rend.material;
        float elapsed = 0f;

        while (elapsed < destroyDuration)
        {
            float t = elapsed / destroyDuration;
            mat.SetFloat("_GripAmount", t);
            Color currentColor = Color.Lerp(startColor, endColor, t);
            mat.SetColor("_GreenAmount", currentColor);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mat.SetColor("_GreenAmount", endColor);


        Destroy(sphereObject);
    }

}

