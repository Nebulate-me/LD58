Shader "Unlit/2DFlash"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FlashAmount ("Flash Amount", Range(0,1)) = 0
        _DarkenAmount ("Darken Amount", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };
            sampler2D _MainTex; float4 _MainTex_ST;
            float _FlashAmount;
            float _DarkenAmount;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                // brighten texture toward white
                tex.rgb = lerp(tex.rgb, 1, _FlashAmount);
                if (_FlashAmount <= 0.01f)
                {
                tex.rgb = lerp(tex.rgb, 0, _DarkenAmount);    
                }
                
                return tex;
            }
            ENDCG
        }
    }
}
