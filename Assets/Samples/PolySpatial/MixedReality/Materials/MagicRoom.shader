Shader "Custom/Unlit/MagicRoom"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _NoiseSpeed ("Noise Speed", Float) = 0.1
        _NoiseScale ("Noise Scale", Float) = 5.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1" }
        Cull Front // 内側を描画！
        ZWrite On
        Lighting Off
        Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float _NoiseSpeed;
            float _NoiseScale;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _TimeY;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * _NoiseScale + _Time.y * _NoiseSpeed;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float noise = (sin(i.uv.x) + cos(i.uv.y)) * 0.5 + 0.5;
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                col.rgb *= noise;
                return col;
            }
            ENDCG
        }
    }
}
