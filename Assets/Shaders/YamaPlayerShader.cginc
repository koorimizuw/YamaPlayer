float2 AdjustAspect(float2 uv, float4 texelSize, float aspectRatio)
{
    float aspect = texelSize.z / aspectRatio;
    if (texelSize.w > aspect) uv.x = ((uv - 0.5) / (aspect / texelSize.w)) + 0.5;
    if (texelSize.w < aspect) uv.y = ((uv.y - 0.5) / (texelSize.w / aspect)) + 0.5;
    return uv;
}

float4 GetFixedAVProTexture(sampler2D tex, float2 uv)
{
#if UNITY_UV_STARTS_AT_TOP
    uv.y = 1 - uv.y;
#endif
    
    float4 color = tex2D(tex, uv);
#if !UNITY_COLORSPACE_GAMMA
    color.rgb = pow(color.rgb, 2.2);
#endif
    return color;
}

float4 GetTexture(sampler2D tex, float2 uv, float4 texelSize, float aspectRatio, bool flip = false)
{
    if (flip) uv.x = 1 - uv.x;
    uv = AdjustAspect(uv, texelSize, aspectRatio);
    if (any(uv < 0 || uv > 1)) return 0;
    return tex2D(tex, uv);

}