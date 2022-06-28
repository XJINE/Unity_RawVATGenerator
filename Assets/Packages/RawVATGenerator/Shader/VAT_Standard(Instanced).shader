Shader "VAT/Standard(Instanced)"
{
    Properties
    {
        _Color      ("Color",        Color)      = (1, 1, 1, 1)
        _MainTex    ("Albedo (RGB)", 2D)         = "white" {}
        _Glossiness ("Smoothness",   Range(0,1)) = 0.5
        _Metallic   ("Metallic",     Range(0,1)) = 0.0

        [NoScaleOffset]   _AnimTex        ("PositionTex", 2D)     = "white" {} 
        [NoScaleOffset]   _AnimTex_Normal ("NormalTex",   2D)     = "white" {}
                          _AnimTex_Length ("Length(t,f)", Vector) = (0, 0, 0, 0)
                          _AnimTex_FPS    ("FPS",         Float)  = 30
        [Toggle(_REPEAT)] _AnimTex_Repeat ("Repeat",      Float)  = 0
                          _AnimTex_Time   ("Time",        Float)  = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM

        #pragma multi_compile_instancing
        #pragma multi_compile _ _REPEAT
        #pragma surface surf Standard fullforwardshadows addshadow vertex:vert

        #include "VAT.cginc"

        struct appdata
        {
            float4 vertex    : POSITION;
            float4 tangent   : TANGENT;
            float3 normal    : NORMAL;
            float4 texcoord  : TEXCOORD0;
            float4 texcoord1 : TEXCOORD1;
            float4 texcoord2 : TEXCOORD2;
            float4 texcoord3 : TEXCOORD3;
            #if defined(SHADER_API_XBOX360)
            half4  texcoord4 : TEXCOORD4;
            half4  texcoord5 : TEXCOORD5;
            #endif
            fixed4 color : COLOR;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            uint vid : SV_VertexID;
        };

        struct Input
        {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;
        half      _Glossiness;
        half      _Metallic;
        fixed4    _Color;

        void vert(inout appdata v)
        {
            float t = UNITY_ACCESS_INSTANCED_PROP(VATProps, _AnimTex_Time);
                  t = NormalizeAnimTime(t);

            v.vertex.xyz = AnimTexPosition(v.vid, t);
            v.normal     = normalize(AnimTexNormal(v.vid, t));
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

            o.Albedo     = c.rgb;
            o.Metallic   = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha      = c.a;
        }

        ENDCG
    }

    FallBack "Diffuse"
}