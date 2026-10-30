#ifndef FRAXINUS_AREA_LIGHT_EVALUATION_INCLUDED
#define FRAXINUS_AREA_LIGHT_EVALUATION_INCLUDED

// Rectangle LTC integration. Source attribution and license: ../README.md.
// Cookie, shadows and transmission are separate features.
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonLighting.hlsl"
#include "Packages/com.bluewhitebow.fraxinus-rp/ShaderLibrary/Lighting/Common/AreaLightCommon.hlsl"

struct FraxinusAreaLightData
{
    float3 positionWS;
    float inverseRangeSquared;
    float3 right;
    float halfWidth;
    float3 up;
    float halfHeight;
    float3 color;
    float diffuseDimmer;
    float specularDimmer;
};

struct FraxinusAreaPreLightData
{
    float3x3 basis;
    // SampleLtcMatrix returns transposed inverse matrices (for row vectors).
    float3x3 diffuseTransform;
    float3x3 specularTransform;
    float3x3 coatTransform;
    float3 specularFGD;
    float diffuseFGD;
    float coatWeight;
};

struct FraxinusAreaLighting
{
    float3 diffuse;
    float3 specular;
};

float FraxinusRectangleIrradiance(float3 center, float3 right, float3 up,
    float halfWidth, float halfHeight, float3x3 inverseTransform)
{
    // Reject before and after the LTC transform to avoid light leaking below the surface.
    if (center.z + halfWidth * abs(right.z) + halfHeight * abs(up.z) <= 0.0)
        return 0.0;

    float3 C = mul(inverseTransform, center);
    float3 A = mul(inverseTransform, right) * halfWidth;
    float3 B = mul(inverseTransform, up) * halfHeight;
    if (C.z + abs(A.z) + abs(B.z) <= 0.0)
        return 0.0;

    float4x3 vertices;
    vertices[0] = C - A - B;
    vertices[1] = C - A + B;
    vertices[2] = C + A + B;
    vertices[3] = C + A - B;
    float3 formFactor;
    return PolygonIrradiance(vertices, formFactor);
}

// Material-independent output: the caller applies its diffuse color and AO once.
FraxinusAreaLighting EvaluateFraxinusRectangle(float3 positionWS,
    FraxinusAreaPreLightData preLight, FraxinusAreaLightData light, bool evaluateSpecular)
{
    FraxinusAreaLighting result = (FraxinusAreaLighting)0;
    float3 toLight = light.positionWS - positionWS;
    // The emitter faces +cross(right, up), matching a Unity light's local +Z.
    if (dot(toLight, cross(light.right, light.up)) >= -1e-5 ||
        light.halfWidth <= 0.0 || light.halfHeight <= 0.0)
        return result;

    float attenuation = PillowWindowing(toLight, light.right, light.up,
        light.halfWidth, light.halfHeight, light.inverseRangeSquared, 1.0);
    if (attenuation <= 0.0)
        return result;

    float3 center = mul(preLight.basis, toLight);
    float3 right = mul(preLight.basis, light.right);
    float3 up = mul(preLight.basis, light.up);
    float diffuse = FraxinusRectangleIrradiance(center, right, up, light.halfWidth,
        light.halfHeight, transpose(preLight.diffuseTransform));
    result.diffuse = diffuse * preLight.diffuseFGD * light.diffuseDimmer;

    if (evaluateSpecular)
    {
        float specular = FraxinusRectangleIrradiance(center, right, up, light.halfWidth,
            light.halfHeight, transpose(preLight.specularTransform));
        result.specular = specular * preLight.specularFGD * light.specularDimmer;
        if (preLight.coatWeight > 0.0)
        {
            float coat = FraxinusRectangleIrradiance(center, right, up, light.halfWidth,
                light.halfHeight, transpose(preLight.coatTransform));
            result.diffuse *= 1.0 - preLight.coatWeight;
            result.specular = lerp(result.specular, coat * light.specularDimmer,
                preLight.coatWeight);
        }
    }

    // Fraxinus Forward omits 1/PI for punctual Lambert.
    // Convert the normalized LTC integral to that convention exactly once here.
    result.diffuse *= light.color * (attenuation * PI);
    result.specular *= light.color * (attenuation * PI);
    return result;
}

#endif
