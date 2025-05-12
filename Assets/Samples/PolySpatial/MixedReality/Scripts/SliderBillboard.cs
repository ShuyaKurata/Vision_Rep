using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderBillboard : MonoBehaviour
{
    Transform sliderTransform;
    Transform background;
    Transform fillArea;
    RectTransform fillRect;

    void Start()
    {
        // SliderオブジェクトのTransformを取得
        sliderTransform = GetComponent<Transform>();

        // BackgroundとFill AreaのTransformを取得
        background = sliderTransform.Find("Background");
        fillArea = sliderTransform.Find("Fill Area");

        // FillのRectTransformを取得
        fillRect = fillArea.Find("Fill").GetComponent<RectTransform>();
    }

    void Update()
    {
        // FillをBackgroundの上に描画する
        // fillArea.SetSiblingIndex(background.GetSiblingIndex() + 1);

        // スライダーをカメラの方向に向ける
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

        // // Z座標をカメラに近づける（例：-0.01f）
        // Vector3 position = fillRect.localPosition;
        // position.z = -0.01f; // カメラに近づけるために負の値を設定
        // fillRect.localPosition = position;
    }
}
