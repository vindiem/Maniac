Shader "Hidden/PixelateShader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _PixelSize ("Pixel Size", Float) = 8.0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform float _PixelSize;
            uniform sampler2D _MainTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                uv = floor(uv * _PixelSize) / _PixelSize;

                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}
