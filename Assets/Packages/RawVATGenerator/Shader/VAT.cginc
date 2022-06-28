#ifndef VAT_INCLUDE
#define VAT_INCLUDE

sampler2D _AnimTex;
sampler2D _AnimTex_Normal;
half4     _AnimTex_TexelSize;
half4     _AnimTex_Normal_TexelSize;
float4    _AnimTex_Length;
float     _AnimTex_FPS;
float     _AnimTex_Repeat;

#ifdef INSTANCING_ON

UNITY_INSTANCING_BUFFER_START(VATProps)
UNITY_DEFINE_INSTANCED_PROP(float, _AnimTex_Time)
UNITY_INSTANCING_BUFFER_END(VATProps)

#else

float _AnimTex_Time;

#endif

inline float NormalizeAnimTime(float time)
{
    #ifdef _REPEAT
    return clamp(time % _AnimTex_Length.x, 0, _AnimTex_Length.x);
    #else
    return clamp(time, 0, _AnimTex_Length.x);
    #endif
}

float3 AnimTexPosition(uint vid, float time)
{
    float frame  = min(time * _AnimTex_FPS, _AnimTex_Length.y);
    float frame1 = floor(frame);
    float frame2 = min(frame1 + 1, _AnimTex_Length.y);
    float lerpT  = frame - frame1;

    float4 uv1 = 0, uv2 = 0;
    uv1.xy = (0.5 + float2(vid, frame1)) * _AnimTex_TexelSize;
    uv2.xy = (0.5 + float2(vid, frame2)) * _AnimTex_TexelSize;

    float3 pos1 = tex2Dlod(_AnimTex, uv1).rgb;
    float3 pos2 = tex2Dlod(_AnimTex, uv2).rgb;

    return lerp(pos1, pos2, lerpT);
}

float3 AnimTexNormal(uint vid, float time)
{
    float frame  = min(time * _AnimTex_FPS, _AnimTex_Length.y);
    float frame1 = floor(frame);
    float frame2 = min(frame1 + 1, _AnimTex_Length.y);
    float lerpT  = frame - frame1;

    float4 uv1 = 0, uv2 = 0;
    uv1.xy = (0.5 + float2(vid, frame1)) * _AnimTex_Normal_TexelSize;
    uv2.xy = (0.5 + float2(vid, frame2)) * _AnimTex_Normal_TexelSize;

    float3 normal1 = tex2Dlod(_AnimTex_Normal, uv1).rgb;
    float3 normal2 = tex2Dlod(_AnimTex_Normal, uv2).rgb;

    return lerp(normal1, normal2, lerpT);
}

#endif