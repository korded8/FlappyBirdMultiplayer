Shader "Custom/BirdShader"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}

        _RedColor ("Red Mask Color", Color) = (1,0,0,1)
        _GreenColor ("Green Mask Color", Color) = (0,1,0,1)
        _BlueColor ("Blue Mask Color", Color) = (0,0,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _MaskTex;

            fixed4 _RedColor;
            fixed4 _GreenColor;
            fixed4 _BlueColor;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, i.uv);
                fixed4 mask = tex2D(_MaskTex, i.uv);

                // Mieszamy kolory maski z bazow¹ tekstur¹
                fixed3 maskColor = _RedColor.rgb * mask.r +
                                   _GreenColor.rgb * mask.g +
                                   _BlueColor.rgb * mask.b;

                // Waga maski dla mieszania z bazow¹ tekstur¹
                fixed maskWeight = saturate(mask.r + mask.g + mask.b);

                // Finalny kolor: mieszanka bazowej tekstury i kolorów z maski
                fixed3 finalColor = lerp(baseCol.rgb, maskColor, maskWeight);

                // Zachowujemy alpha z bazowej tekstury
                return fixed4(finalColor, baseCol.a);
            }
            ENDCG
        }
    }
}