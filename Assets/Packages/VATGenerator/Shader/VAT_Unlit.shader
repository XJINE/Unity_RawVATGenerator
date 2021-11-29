Shader "VAT/Unlit"
{
    Properties
    {
        [NoScaleOffset]   _MainTex        ("Base",        2D)     = "white" {}
        [NoScaleOffset]   _AnimTex        ("PositionTex", 2D)     = "white" {} 
                          _AnimTex_Length ("Length(t,f)", Vector) = (0, 0, 0, 0)
                          _AnimTex_FPS    ("FPS",         Float)  = 30
        [Toggle(_REPEAT)] _AnimTex_Repeat ("Repeat",      Float)  = 0
                          _AnimTex_Time   ("Time",        Float)  = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM

            #pragma multi_compile _ _REPEAT
            #pragma vertex   vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "VAT.cginc"

            struct appdata
            {
                uint   vid    : SV_VertexID;
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            sampler2D _MainTex;

            v2f vert(appdata v)
            {
                float t = NormalizeAnimTime(_AnimTex_Time);

                v.vertex.xyz = AnimTexPosition(v.vid, t);

                v2f o;
                o.vertex = UnityObjectToClipPos(float4(v.vertex.xyz, 1));
                o.uv     = v.uv;

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }

            ENDCG
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            CGPROGRAM

            #pragma multi_compile_shadowcaster
            #pragma multi_compile _ _REPEAT
            #pragma vertex   vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "VAT.cginc"

            struct appdata
            {
                uint   vid    : SV_VertexID;
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                V2F_SHADOW_CASTER;
            };

            sampler2D _MainTex;
            
            v2f vert(appdata v)
            {
                float t = NormalizeAnimTime(_AnimTex_Time);

                v.vertex.xyz = AnimTexPosition(v.vid, t);

                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i);
            }

            ENDCG
        }
    }

    FallBack Off
}