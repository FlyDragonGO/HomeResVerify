Shader "Sprites/CustomDefault"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 2.0            
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"
        
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;                
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;                
            };

            fixed4 _Color;
            fixed2 _Flip;
            v2f Vert(appdata_t IN)
            {
                v2f OUT;
                
                OUT.vertex = float4(IN.vertex.xy * _Flip, IN.vertex.z, 1.0);
                OUT.vertex = UnityObjectToClipPos(OUT.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                return OUT;
            }

            sampler2D _MainTex;            
            fixed4 Frag(v2f IN) : SV_Target
            {
                fixed4 color = tex2D (_MainTex, IN.texcoord) * IN.color;
                color.rgb *= color.a;
                return color;
            }
        
        ENDCG
        }
    }
}
