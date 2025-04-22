Shader "Custom/UnlitSkyDome"
{
    Properties
    {
        _MainTex ("Panorama Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Background" }
        Cull Front // 内側を見せる
        ZWrite Off
        Lighting Off

        Pass
        {
            SetTexture [_MainTex] { combine texture }
        }
    }
}
