Shader "Custom/UIMask"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255                 
    
        _Center("Center", vector) = (0, 0, 0, 0)
        _Silder ("_Silder", Range (0,1000)) = 1000 // sliders
        _Gradient("Gradient", Range(0, 1000)) = 0
        _Extend("Extend", vector) = (0, 0, 0, 0)
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
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
 
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha        
 
        Pass
        {
            Name "Modify"
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0            

 
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            #pragma multi_compile __ USE_RECT USE_ROUND

            struct appdata_t
            {
                float4 vertex   : POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
 
            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                half4 worldPosition : TEXCOORD1;
                #if USE_ROUND
                    half inner_round : TEXCOORD2;
                    half rc_gradient : TEXCOORD3;
                #endif

                UNITY_VERTEX_OUTPUT_STEREO             
            };
            
            fixed4 _Color;                        
            half _Silder;
            half2 _Center;
            half _Gradient;
            half2 _Extend;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = (IN.vertex);
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
 
                OUT.texcoord = IN.texcoord;
                
                OUT.color = IN.color * _Color;

                #if USE_ROUND  
                    OUT.inner_round = _Silder - _Gradient;
                    OUT.rc_gradient = 1.0 / _Gradient;
                #endif
                return OUT;
            }
 
            sampler2D _MainTex;
 
            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, IN.texcoord) * IN.color;                               

                #ifdef USE_RECT
                    float gradient = min(min(_Extend.x, _Extend.y), _Gradient);
                    float4 outter = float4(_Center.x - _Extend.x, _Center.x + _Extend.x, _Center.y - _Extend.y, _Center.y + _Extend.y);
                    float4 inner = float4(outter.x + gradient, outter.y - gradient, outter.z + gradient, outter.w - gradient);

                    float4 wp = IN.worldPosition;
                    if (wp.x > inner.x && wp.x < inner.y && wp.y > inner.z && wp.y < inner.w)  // inner mask area
                    {
                        color.a = 0;
                    }
                    else if (wp.x > outter.x && wp.x < outter.y && wp.y > outter.z && wp.y < outter.w) // outter gradient area
                    {
                        half a = 1;
                        if (wp.x > inner.y) // right gradient area
                        {
                            if (wp.y < inner.z) // top-right round corner
                            {
                                half xx = wp.x - inner.y;
                                half yy = inner.z - wp.y;
                                a = (xx * xx + yy * yy) / (gradient * gradient);
                            }
                            else if (wp.y > inner.w) // bottom-right round corner
                            {
                                half xx = wp.x - inner.y;
                                half yy = wp.y - inner.w;
                                a = (xx * xx + yy * yy) / (gradient * gradient);
                            }
                            else // right
                            {
                                a = (wp.x - inner.y) / (outter.y - inner.y);
                                a *= a;
                            }
                        }
                        else if (wp.x < inner.x) // left gradient area
                        {
                            if (wp.y < inner.z) // top-left round corner
                            {
                                half xx = inner.x - wp.x;
                                half yy = inner.z - wp.y;
                                a = (xx * xx + yy * yy) / (gradient * gradient);
                            }
                            else if (wp.y > inner.w) // bottom-left round corner
                            {
                                half xx = inner.x - wp.x;
                                half yy = wp.y - inner.w;
                                a = (xx * xx + yy * yy) / (gradient * gradient);
                            }
                            else // left
                            {
                                a = (inner.x - wp.x) / (inner.x - outter.x);
                                a *= a;
                            }
                        }
                        else // top & bottom gradient area
                        {
                            if (wp.y < inner.z) a = (inner.z - wp.y) / (inner.z - outter.z);
                            if (wp.y > inner.w) a = (wp.y - inner.w) / (outter.w - inner.w);
                            a *= a;
                        }

                        if (a > 1) a = 1;
                        color.a *= a;
                    }
                    color.rgb *= color.a;
                #endif

                #ifdef USE_ROUND
                    /*float dist = distance(IN.worldPosition.xy, _Center.xy);                    
                    half fact1 = step(IN.inner_round, dist);
                    half fact2 = step(dist, _Silder);
                    half fact3 = -(IN.inner_round - dist) * IN.rc_gradient;
                    half fact5 = (fact1 * fact2 * fact3) + (1 - fact2);
                    color.a *= fact5;*/

                    half2 dist = IN.worldPosition.xy - _Center.xy;                    
                    dist.x = length(dist);
                    half deno = (dist.x - IN.inner_round) * IN.rc_gradient;
                    half blur = color.a * deno;                                        
                    if(dist.x < IN.inner_round)
                    {
                        color.a = 0;
                    }
                    else if(dist.x < _Silder)
                    {                        
                        color.a *= deno;
                    }


                    /*if(_Gradient > _Silder)
                    {
                        _Gradient = _Silder;
                    }  */
                    /*
                    float dis = distance(IN.worldPosition.xy,_Center.xy);
                    if(dis < _Silder - _Gradient)
                    {
                        color.a = 0;
                    }
                    else if(dis < _Silder)
                    {
                        float factor = ((_Gradient + dis - _Silder) / _Gradient);// * 0.95 + 0.05;
                        color.a *= factor;
                    }*/                    
                #endif
                                
                return color;
            
            }
        ENDCG
        }
    }
}