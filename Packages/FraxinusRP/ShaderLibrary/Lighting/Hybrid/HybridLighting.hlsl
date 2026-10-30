#ifndef FRAXINUS_HYBRID_LIGHTING_INCLUDED
#define FRAXINUS_HYBRID_LIGHTING_INCLUDED

// BRDF/FGD integration with the Chan 2024 rough diffuse model.
// Source attribution and license: ../README.md.
// Fraxinus owns light lookup, shadows, GI and exposure.
#include "Packages/com.bluewhitebow.fraxinus-rp/ShaderLibrary/PreIntegratedFGD.hlsl"

half3 FraxinusHybridRoughDiffuse(half3 diffuseColor, float roughness, float NdotH, float VdotH)
{
    float alpha = roughness * roughness;
    float scale = max(0.55 - 0.2 * roughness, 1.25 - 1.6 * roughness);
    float bias = saturate(4.0 * alpha);
    float rough = scale * (NdotH + bias) / (NdotH + 0.025) * VdotH * VdotH;
    return diffuseColor * (lerp(1.0, rough, roughness) + alpha * 0.38);
}

float3 FraxinusHybridSpecular(BRDFData brdf, float NdotH, float NdotL, float NdotV, float LdotH)
{
    float lambdaV = GetSmithJointGGXPartLambdaV(NdotV, brdf.roughness);
    float DV = DV_SmithJointGGX(NdotH, NdotL, NdotV, brdf.roughness, lambdaV);
    return min(DV * F_Schlick(brdf.specular, LdotH) * PI, 1000.0);
}

half3 FraxinusHybridDirect(BRDFData brdf, BRDFData coatBRDF, half3 lightColor,
    half3 lightDirection, float attenuation, half3 normal, half3 viewDirection,
    half coatMask, bool specularHighlightsOff)
{
    float NdotL = saturate(dot(normal, lightDirection));
    float NdotV = ClampNdotV(saturate(dot(normal, viewDirection)));
    float3 H = SafeNormalize(float3(lightDirection) + float3(viewDirection));
    float NdotH = saturate(dot(normal, H));
    float LdotH = saturate(dot(lightDirection, H));
    float3 value = FraxinusHybridRoughDiffuse(brdf.diffuse, brdf.perceptualRoughness, NdotH, LdotH);
#if !defined(_SPECULARHIGHLIGHTS_OFF)
    if (!specularHighlightsOff)
    {
        value += FraxinusHybridSpecular(brdf, NdotH, NdotL, NdotV, LdotH);
#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
        float coatFresnel = kDielectricSpec.x + kDielectricSpec.a * Pow4(1.0 - saturate(NdotV));
        value = value * (1.0 - coatMask * coatFresnel) +
            FraxinusHybridSpecular(coatBRDF, NdotH, NdotL, NdotV, LdotH) * coatMask;
#endif
    }
#endif
    return half3(value * lightColor * (attenuation * NdotL));
}

half3 FraxinusHybridGlobalIllumination(BRDFData brdf, BRDFData coatBRDF, half coatMask,
    InputData inputData, half occlusion)
{
    // Assign both resource branches to one initialized result. This avoids an implicit
    // early-return value crossing the texture-sampling branch in the D3D11 compiler.
    half3 color = half3(0.0, 0.0, 0.0);
    if (_FraxinusLightingLutsReady == 0)
    {
        // Custom graphs without lighting resources keep the existing GI path.
        color = GlobalIllumination(brdf, coatBRDF, coatMask, inputData.bakedGI, occlusion,
            inputData.positionWS, inputData.normalWS, inputData.viewDirectionWS, inputData.normalizedScreenSpaceUV);
    }
    else
    {
        float NdotV = ClampNdotV(saturate(dot(inputData.normalWS, inputData.viewDirectionWS)));
        float3 reflection = reflect(-inputData.viewDirectionWS, inputData.normalWS);
        float3 specularFGD;
        float diffuseFGD, reflectivity;
        GetPreIntegratedFGDGGXAndDisneyDiffuse(NdotV, brdf.perceptualRoughness, brdf.specular,
            specularFGD, diffuseFGD, reflectivity);
        half3 diffuseAO = GTAOMultiBounce(occlusion, brdf.albedo);
        float specularVisibility = GetSpecularOcclusionFromAmbientOcclusion(NdotV, occlusion, brdf.roughness);
        half3 specularAO = GTAOMultiBounce(specularVisibility, brdf.specular);
        half3 diffuse = ApplyAPRTGI(inputData.bakedGI, inputData.positionWS, inputData.normalWS);
        half3 environment = GlossyEnvironmentReflection(reflection, inputData.positionWS,
            brdf.perceptualRoughness, 1.0h, inputData.normalizedScreenSpaceUV);
        float3 energyCompensation = 1.0 + brdf.specular * (rcp(max(reflectivity, 1e-4)) - 1.0);
        color = diffuse * brdf.diffuse * diffuseFGD * diffuseAO +
            environment * specularFGD * energyCompensation * specularAO;

#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
        float3 coatFGD;
        float unusedDiffuse, unusedReflectivity;
        GetPreIntegratedFGDGGXAndDisneyDiffuse(NdotV, coatBRDF.perceptualRoughness, coatBRDF.specular,
            coatFGD, unusedDiffuse, unusedReflectivity);
        half3 coatEnvironment = GlossyEnvironmentReflection(reflection, inputData.positionWS,
            coatBRDF.perceptualRoughness, 1.0h, inputData.normalizedScreenSpaceUV);
        float coatFresnel = kDielectricSpec.x + kDielectricSpec.a * Pow4(1.0 - saturate(NdotV));
        color = color * (1.0 - coatMask * coatFresnel) + coatEnvironment * coatFGD * coatMask * specularAO;
#endif
        if (IsOnlyAOLightingFeatureEnabled())
            color = occlusion.xxx;
    }
    return color;
}

#endif
