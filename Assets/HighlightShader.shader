Shader "Custom/HighlightShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Highlight Color", Color) = (1,1,1,1)
        _Thickness ("Highlight Thickness", Float) = 0.05
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Thickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float alpha = col.a;

                // ѕровер€ем соседние пиксели дл€ создани€ границы
                float outline = 0;
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        float2 offset = float2(x, y) * _Thickness / _MainTex_ST.xy;
                        float neighborAlpha = tex2D(_MainTex, i.uv + offset).a;
                        if (alpha > 0.5 && neighborAlpha < 0.5)
                        {
                            outline = 1;
                        }
                    }
                }

                if (outline > 0)
                {
                    return _Color;
                }
                return float4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}