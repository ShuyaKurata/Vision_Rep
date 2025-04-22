Shader "Custom/SquishyGelColor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GripAmount ("Grip Amount", Range(0, 1)) = 0.0
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _GripColor ("Grip Color", Color) = (1, 0.5, 0.5, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _GripAmount;
            fixed4 _BaseColor;
            fixed4 _GripColor;

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

            float3 Squish(float3 pos, float grip)
            {
                float yScale = lerp(1.0, 0.5, grip);
                float xzScale = lerp(1.0, 1.2, grip);
                return float3(pos.x * xzScale, pos.y * yScale, pos.z * xzScale);
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 squished = Squish(v.vertex.xyz, _GripAmount);
                o.vertex = UnityObjectToClipPos(float4(squished, 1.0));
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw; // ← これが TRANSFORM_TEX の代替
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                fixed4 lerpedColor = lerp(_BaseColor, _GripColor, _GripAmount);
                return texColor * lerpedColor;
            }
            ENDCG
        }
    }
}
