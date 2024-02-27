// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/energyeffectCode"
{
    Properties
    {
        _MainTex ("Texture2D", 2D) = "white" {}
        [HDR]_EffectColor ("Color",Color) = (0,0,0,0)

        _FresnelPower("Fresnel Power",Float) = 0
      

        

    }
    SubShader
    {
        Tags { "RenderType"="Transparent"  "QUEUE"="Transparent" }
        LOD 100
        Blend SrcAlpha One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
               
                float3 worldPos : TEXCOORD1;
                float3 vertex : TEXCOORD2;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _FresnelPower;
            float4 _EffectColor;

            v2f vert (appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld,v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.vertex = v.vertex;

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half2 flowUV = half2(frac(_Time.y),i.vertex.y - _Time.y);
                half3 noise = tex2D(_MainTex,flowUV);
                noise = 1-noise;
                half3 finalColor = noise * _EffectColor;
                return half4( finalColor,1);
            }
            ENDCG
        }
    }
}
