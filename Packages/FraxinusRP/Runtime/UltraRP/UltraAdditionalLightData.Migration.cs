using System;
using UnityEngine;

namespace FraxinusEngine.Rendering.Ultra
{
    public partial class UltraAdditionalLightData
    {
        // URP remains optional. Read its serialized data once, without reflected property
        // access (which would also require preserving those properties in IL2CPP builds).
        static readonly Type s_UniversalLightDataType = Type.GetType(
            "UnityEngine.Rendering.Universal.UniversalAdditionalLightData, Unity.RenderPipelines.Universal.Runtime");

        [Serializable]
        internal sealed class UniversalLightData
        {
            public bool m_UsePipelineSettings = true;
            public int m_AdditionalLightsShadowResolutionTier = 2;
            public uint m_RenderingLayers = 1;
            public bool m_CustomShadowLayers = false;
            public uint m_ShadowRenderingLayers = 1;
            public Vector2 m_LightCookieSize = Vector2.one;
            public Vector2 m_LightCookieOffset = Vector2.zero;
            public SoftShadowQuality m_SoftShadowQuality = SoftShadowQuality.UsePipelineSettings;
        }

        internal static UniversalLightData ReadUniversalLightData(GameObject gameObject)
        {
            if (s_UniversalLightDataType == null)
                return null;

            var legacyData = gameObject.GetComponent(s_UniversalLightDataType);
            if (legacyData == null)
                return null;

            var data = new UniversalLightData();
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(legacyData), data);
            data.m_AdditionalLightsShadowResolutionTier = Mathf.Clamp(
                data.m_AdditionalLightsShadowResolutionTier, additionalLightsShadowResolutionTierCustom,
                additionalLightsShadowResolutionTierHigh);
            data.m_SoftShadowQuality = (SoftShadowQuality)Mathf.Clamp((int)data.m_SoftShadowQuality,
                (int)SoftShadowQuality.UsePipelineSettings, (int)SoftShadowQuality.High);
            return data;
        }

        // The Inspector has its own Undo-aware component creation path. Initialize only
        // newly added components so opening an Inspector never overwrites authored data.
        internal static void InitializeFromUniversalLightData(UltraAdditionalLightData lightData)
        {
            var legacyData = ReadUniversalLightData(lightData.gameObject);
            if (legacyData != null)
                lightData.ApplyUniversalLightData(legacyData);
        }

        internal void ApplyUniversalLightData(UniversalLightData data)
        {
            _usePipelineSettings = data.m_UsePipelineSettings;
            _additionalLightsShadowResolutionTier = data.m_AdditionalLightsShadowResolutionTier;
            _softShadowQuality = data.m_SoftShadowQuality;
            _renderingLayers = data.m_RenderingLayers;
            _customShadowLayers = data.m_CustomShadowLayers;
            _shadowRenderingLayers = data.m_ShadowRenderingLayers;
            _lightCookieSize = data.m_LightCookieSize;
            _lightCookieOffset = data.m_LightCookieOffset;
            SyncLightAndShadowLayers();
        }
    }
}
