#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
using HDRKeywords = UnityEngine.Rendering.HDROutputUtils.ShaderKeywords;
using ShaderKeywordFilter = UnityEditor.ShaderKeywordFilter;

namespace FraxinusEngine.Rendering.Ultra
{
    // This partial class is used for Shader Keyword Prefiltering
    // It's an editor only file and used when making builds to determine what keywords can
    // be removed early in the Shader Processing stage based on the settings in each UltraRP Asset
    public partial class UltraRenderPipelineAsset
    {
        internal enum PrefilteringMode
        {
            Remove,                     // Removes the keyword
            Select,                     // Keeps the keyword
            SelectOnly                  // Selects the keyword and removes others
        }

        internal enum PrefilteringModeMainLightShadows
        {
            Remove,                     // Removes the keyword
            SelectMainLight,            // Selects MainLightShadows variant & Removes OFF variant
            SelectMainLightAndOff,      // Selects MainLightShadows & OFF variants
            SelectMainLightAndCascades, // Selects MainLightShadows, MainLightShadowCascades & Removes OFF variant
            SelectAll,                  // Selects MainLightShadows, MainLightShadowCascades & OFF variant
        }

        internal enum PrefilteringModeAdditionalLights
        {
            Remove,                     // Removes the keyword
            SelectVertex,               // Selects Vertex & Removes OFF variant
            SelectVertexAndOff,         // Selects Vertex & OFF variant
            SelectPixel,                // Selects Pixel  & Removes OFF variant
            SelectPixelAndOff,          // Selects Pixel  & OFF variant
            SelectAll                   // Selects Vertex, Pixel & OFF variant
        }

        // Platform specific filtering overrides
        [ShaderKeywordFilter.ApplyRulesIfGraphicsAPI(GraphicsDeviceType.OpenGLES3, GraphicsDeviceType.OpenGLCore)]
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.WriteRenderingLayers)]
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.DBufferMRT1)]
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.DBufferMRT2)]
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.DBufferMRT3)]
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.USE_LEGACY_LIGHTMAPS)]
        private const bool k_commonGLDefaults = true;

        // Foveated Rendering
#if FRAXINUSRP_ENABLE_XR && ENABLE_VR && ENABLE_XR_MODULE
        [ShaderKeywordFilter.ApplyRulesIfNotGraphicsAPI(GraphicsDeviceType.PlayStation5NGGC, GraphicsDeviceType.Metal)]
#endif
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.FoveatedRenderingNonUniformRaster)]
        private const bool k_prefilterFoveatedRenderingNonUniformRaster = true;

        // User can change cascade count at runtime so we have to include both MainLightShadows and MainLightShadowCascades.
        // ScreenSpaceShadows renderer feature has separate filter attribute for keeping MainLightShadowScreen.
        // NOTE: off variants are atm always removed when shadows are supported
        [ShaderKeywordFilter.RemoveIf(PrefilteringModeMainLightShadows.Remove, keywordNames: new[] { ShaderKeywordStrings.MainLightShadows, ShaderKeywordStrings.MainLightShadowCascades, ShaderKeywordStrings.MainLightShadowScreen })]
        [ShaderKeywordFilter.SelectIf(PrefilteringModeMainLightShadows.SelectMainLight, keywordNames: ShaderKeywordStrings.MainLightShadows)]
        [ShaderKeywordFilter.SelectIf(PrefilteringModeMainLightShadows.SelectMainLightAndOff, keywordNames: new[] { "", ShaderKeywordStrings.MainLightShadows })]
        [ShaderKeywordFilter.SelectIf(PrefilteringModeMainLightShadows.SelectMainLightAndCascades, keywordNames: new[] { ShaderKeywordStrings.MainLightShadows, ShaderKeywordStrings.MainLightShadowCascades })]
        [ShaderKeywordFilter.SelectIf(PrefilteringModeMainLightShadows.SelectAll, keywordNames: new[] { "", ShaderKeywordStrings.MainLightShadows, ShaderKeywordStrings.MainLightShadowCascades, ShaderKeywordStrings.MainLightShadowScreen })]
        [SerializeField] private PrefilteringModeMainLightShadows _prefilteringModeMainLightShadows = PrefilteringModeMainLightShadows.SelectAll;

        // Additional Lights
        // clustered renderer can override PerVertex/PerPixel to be disabled
        // NOTE: off variants are atm always kept when additional lights are enabled due to XR perf reasons
        // multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        [ShaderKeywordFilter.RemoveIf(PrefilteringModeAdditionalLights.Remove, keywordNames: new string[] { ShaderKeywordStrings.AdditionalLightsVertex, ShaderKeywordStrings.AdditionalLightsPixel })]
        [ShaderKeywordFilter.SelectIf(PrefilteringModeAdditionalLights.SelectVertex, keywordNames: ShaderKeywordStrings.AdditionalLightsVertex)]
        [ShaderKeywordFilter.SelectIf(PrefilteringModeAdditionalLights.SelectVertexAndOff, keywordNames: new string[] { "", ShaderKeywordStrings.AdditionalLightsVertex })]
        [ShaderKeywordFilter.SelectIf(PrefilteringModeAdditionalLights.SelectPixel, keywordNames: ShaderKeywordStrings.AdditionalLightsPixel)]
        [ShaderKeywordFilter.SelectIf(PrefilteringModeAdditionalLights.SelectPixelAndOff, keywordNames: new string[] { "", ShaderKeywordStrings.AdditionalLightsPixel })]
        [ShaderKeywordFilter.SelectIf(PrefilteringModeAdditionalLights.SelectAll, keywordNames: new string[] { "", ShaderKeywordStrings.AdditionalLightsVertex, ShaderKeywordStrings.AdditionalLightsPixel })]
        [SerializeField] private PrefilteringModeAdditionalLights _prefilteringModeAdditionalLight = PrefilteringModeAdditionalLights.SelectAll;

        // Additional Lights Shadows
        [ShaderKeywordFilter.RemoveIf(PrefilteringMode.Remove, keywordNames: ShaderKeywordStrings.AdditionalLightShadows)]
        [ShaderKeywordFilter.SelectIf(PrefilteringMode.Select, keywordNames: new string[] { "", ShaderKeywordStrings.AdditionalLightShadows })]
        [ShaderKeywordFilter.SelectIf(PrefilteringMode.SelectOnly, keywordNames: ShaderKeywordStrings.AdditionalLightShadows)]
        [SerializeField] private PrefilteringMode _prefilteringModeAdditionalLightShadows = PrefilteringMode.Select;

        // XR Specific keywords
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: new[] {
            ShaderKeywordStrings.BlitSingleSlice, ShaderKeywordStrings.XROcclusionMeshCombined
        })]
        [SerializeField] private bool _prefilterXRKeywords = false;

        // Forward+
        [ShaderKeywordFilter.RemoveIf(PrefilteringMode.Remove, keywordNames: ShaderKeywordStrings.ForwardPlus)]
        [ShaderKeywordFilter.SelectIf(PrefilteringMode.Select, keywordNames: new[] { "", ShaderKeywordStrings.ForwardPlus })]
        [ShaderKeywordFilter.SelectIf(PrefilteringMode.SelectOnly, keywordNames: ShaderKeywordStrings.ForwardPlus)]
        [SerializeField] private PrefilteringMode _prefilteringModeForwardPlus = PrefilteringMode.Select;

        // Deferred Rendering
        [ShaderKeywordFilter.RemoveIf(PrefilteringMode.Remove, keywordNames: new[] {
            ShaderKeywordStrings._DEFERRED_FIRST_LIGHT, ShaderKeywordStrings._DEFERRED_MAIN_LIGHT,
            ShaderKeywordStrings._DEFERRED_MIXED_LIGHTING, ShaderKeywordStrings._GBUFFER_NORMALS_OCT
        })]
        [SerializeField] private PrefilteringMode _prefilteringModeDeferredRendering = PrefilteringMode.Select;

        // Screen Space Occlusion
        [ShaderKeywordFilter.RemoveIf(PrefilteringMode.Remove, keywordNames: ShaderKeywordStrings.ScreenSpaceOcclusion)]
        [ShaderKeywordFilter.SelectIf(PrefilteringMode.Select, keywordNames: new[] { "", ShaderKeywordStrings.ScreenSpaceOcclusion })]
        [ShaderKeywordFilter.SelectIf(PrefilteringMode.SelectOnly, keywordNames: ShaderKeywordStrings.ScreenSpaceOcclusion)]
        [SerializeField] private PrefilteringMode _prefilteringModeScreenSpaceOcclusion = PrefilteringMode.Select;

        // Rendering Debugger
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.DEBUG_DISPLAY)]
        [SerializeField] private bool _prefilterDebugKeywords = false;

        // Filters out WriteRenderingLayers if nothing requires the feature
        // TODO: Implement a different filter triggers for different passes (i.e. per-pass filter attributes)
        [ShaderKeywordFilter.ApplyRulesIfNotGraphicsAPI(GraphicsDeviceType.OpenGLES3, GraphicsDeviceType.OpenGLCore)]
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.WriteRenderingLayers)]
        [SerializeField] private bool _prefilterWriteRenderingLayers = false;

        // HDR Output
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: new[] {
            HDRKeywords.HDR_INPUT, HDRKeywords.HDR_COLORSPACE_CONVERSION, HDRKeywords.HDR_ENCODING, HDRKeywords.HDR_COLORSPACE_CONVERSION_AND_ENCODING
        })]
        [SerializeField] private bool _prefilterHDROutput = false;

        // Alpha Output
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings._ENABLE_ALPHA_OUTPUT)]
        [SerializeField] private bool _prefilterAlphaOutput = false;

        // Screen Space Ambient Occlusion (SSAO) specific keywords
        //[ShaderKeywordFilter.RemoveIf(true, keywordNames: ScreenSpaceAmbientOcclusion.k_SourceDepthNormalsKeyword)]
        //[SerializeField] private bool m_PrefilterSSAODepthNormals = false;
        //[ShaderKeywordFilter.RemoveIf(true, keywordNames: ScreenSpaceAmbientOcclusion.k_SourceDepthLowKeyword)]
        //[SerializeField] private bool m_PrefilterSSAOSourceDepthLow = false;
        //[ShaderKeywordFilter.RemoveIf(true, keywordNames: ScreenSpaceAmbientOcclusion.k_SourceDepthMediumKeyword)]
        //[SerializeField] private bool m_PrefilterSSAOSourceDepthMedium = false;
        //[ShaderKeywordFilter.RemoveIf(true, keywordNames: ScreenSpaceAmbientOcclusion.k_SourceDepthHighKeyword)]
        //[SerializeField] private bool m_PrefilterSSAOSourceDepthHigh = false;
        //[ShaderKeywordFilter.RemoveIf(true, keywordNames: ScreenSpaceAmbientOcclusion.k_AOInterleavedGradientKeyword)]
        //[SerializeField] private bool m_PrefilterSSAOInterleaved = false;
        //[ShaderKeywordFilter.RemoveIf(true, keywordNames: ScreenSpaceAmbientOcclusion.k_AOBlueNoiseKeyword)]
        //[SerializeField] private bool m_PrefilterSSAOBlueNoise = false;
        //[ShaderKeywordFilter.RemoveIf(true, keywordNames: ScreenSpaceAmbientOcclusion.k_SampleCountLowKeyword)]
        //[SerializeField] private bool m_PrefilterSSAOSampleCountLow = false;
        //[ShaderKeywordFilter.RemoveIf(true, keywordNames: ScreenSpaceAmbientOcclusion.k_SampleCountMediumKeyword)]
        //[SerializeField] private bool m_PrefilterSSAOSampleCountMedium = false;
        //[ShaderKeywordFilter.RemoveIf(true, keywordNames: ScreenSpaceAmbientOcclusion.k_SampleCountHighKeyword)]
        //[SerializeField] private bool m_PrefilterSSAOSampleCountHigh = false;

        // Decals
        [ShaderKeywordFilter.ApplyRulesIfNotGraphicsAPI(GraphicsDeviceType.OpenGLES3, GraphicsDeviceType.OpenGLCore)]
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.DBufferMRT1)]
        [SerializeField] private bool _prefilterDBufferMRT1 = false;

        [ShaderKeywordFilter.ApplyRulesIfNotGraphicsAPI(GraphicsDeviceType.OpenGLES3, GraphicsDeviceType.OpenGLCore)]
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.DBufferMRT2)]
        [SerializeField] private bool _prefilterDBufferMRT2 = false;

        [ShaderKeywordFilter.ApplyRulesIfNotGraphicsAPI(GraphicsDeviceType.OpenGLES3, GraphicsDeviceType.OpenGLCore)]
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.DBufferMRT3)]
        [SerializeField] private bool _prefilterDBufferMRT3 = false;

        // Decal Layers - Gets overridden in Decal renderer feature if enabled.
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.DecalLayers)]
        private const bool k_decalLayersDefault = true;

        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.SoftShadowsLow)]
        [SerializeField] private bool _prefilterSoftShadowsQualityLow = false;
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.SoftShadowsMedium)]
        [SerializeField] private bool _prefilterSoftShadowsQualityMedium = false;
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.SoftShadowsHigh)]
        [SerializeField] private bool _prefilterSoftShadowsQualityHigh = false;
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.SoftShadows)]
        [SerializeField] private bool _prefilterSoftShadows = false;

        // Screen Coord Override - Controlled by the Global Settings
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.SCREEN_COORD_OVERRIDE)]
        [SerializeField] private bool _prefilterScreenCoord = false;

        // Native Render Pass
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.RenderPassEnabled)]
        [SerializeField] private bool _refilterNativeRenderPass = false;

        // Use legacy lightmaps (GPU resident drawer)
        [ShaderKeywordFilter.ApplyRulesIfNotGraphicsAPI(GraphicsDeviceType.OpenGLES3, GraphicsDeviceType.OpenGLCore)]
        // Both native lightmap states are retained until their runtime usage is proven.
        [SerializeField] private bool _prefilterUseLegacyLightmaps = false;

        // Reflection probe blending (_REFLECTION_PROBE_BLENDING)
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.ReflectionProbeBlending)]
        [SerializeField] private bool _prefilterReflectionProbeBlending = false;

        // Reflection probe box projection (_REFLECTION_PROBE_BOX_PROJECTION)
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.ReflectionProbeBoxProjection)]
        [SerializeField] private bool _prefilterReflectionProbeBoxProjection = false;

        // Build-derived capability removals. False always keeps both OFF and ON.
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.LOD_FADE_CROSSFADE)]
        [SerializeField] private bool _prefilterLODCrossFade;
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: new[] { ShaderKeywordStrings.ProbeVolumeL1, ShaderKeywordStrings.ProbeVolumeL2 })]
        [SerializeField] private bool _prefilterProbeVolumes;
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.ProbeVolumeAPRT)]
        [SerializeField] private bool _prefilterAPRTVolumes;
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.LightLayers)]
        [SerializeField] private bool _prefilterLightLayers;
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: ShaderKeywordStrings.LightCookies)]
        [SerializeField] private bool _prefilterLightCookies;
        [ShaderKeywordFilter.RemoveIf(true, keywordNames: new[] { ShaderKeywordStrings.LightmapShadowMixing, ShaderKeywordStrings.ShadowsShadowMask })]
        [SerializeField] private bool _prefilterMixedLighting;

        /// <summary>
        /// Data used for Shader Prefiltering. Gathered after going through the URP Assets,
        /// Renderers and Renderer Features in OnPreprocessBuild() inside ShaderPreprocessor.cs.
        /// </summary>
        internal struct ShaderPrefilteringData
        {
            public PrefilteringMode forwardPlusPrefilteringMode;
            public PrefilteringMode deferredPrefilteringMode;
            public PrefilteringModeMainLightShadows mainLightShadowsPrefilteringMode;
            public PrefilteringModeAdditionalLights additionalLightsPrefilteringMode;
            public PrefilteringMode additionalLightsShadowsPrefilteringMode;
            public PrefilteringMode screenSpaceOcclusionPrefilteringMode;
            public bool useLegacyLightmaps;

            public bool stripXRKeywords;
            public bool stripHDRKeywords;
            public bool stripAlphaOutputKeywords;
            public bool stripDebugDisplay;
            public bool stripScreenCoordOverride;
            public bool stripWriteRenderingLayers;
            public bool stripDBufferMRT1;
            public bool stripDBufferMRT2;
            public bool stripDBufferMRT3;
            public bool stripNativeRenderPass;
            public bool stripSoftShadowsQualityLow;
            public bool stripSoftShadowsQualityMedium;
            public bool stripSoftShadowsQualityHigh;

            public bool stripSSAOBlueNoise;
            public bool stripSSAOInterleaved;
            public bool stripSSAODepthNormals;
            public bool stripSSAOSourceDepthLow;
            public bool stripSSAOSourceDepthMedium;
            public bool stripSSAOSourceDepthHigh;
            public bool stripSSAOSampleCountLow;
            public bool stripSSAOSampleCountMedium;
            public bool stripSSAOSampleCountHigh;

            public bool stripSoftShadows;
            public bool stripLODCrossFade;
            public bool stripProbeVolumes;
            public bool stripAPRTVolumes;
            public bool stripLightLayers;
            public bool stripLightCookies;
            public bool stripMixedLighting;
            public bool stripReflectionProbeBlending;
            public bool stripReflectionProbeBoxProjection;

            public static ShaderPrefilteringData GetDefault()
            {
                return new ShaderPrefilteringData()
                {
                    forwardPlusPrefilteringMode = PrefilteringMode.Select,
                    deferredPrefilteringMode = PrefilteringMode.Select,
                    mainLightShadowsPrefilteringMode = PrefilteringModeMainLightShadows.SelectAll,
                    additionalLightsPrefilteringMode = PrefilteringModeAdditionalLights.SelectAll,
                    additionalLightsShadowsPrefilteringMode = PrefilteringMode.Select,
                    screenSpaceOcclusionPrefilteringMode = PrefilteringMode.Select,
                };
            }
        }

        /// <summary>
        /// Uses the data collected in the OnPreprocessBuild() to set the Shader Prefiltering variables.
        /// </summary>
        /// <param name="prefilteringData"></param>
        internal void UpdateShaderKeywordPrefiltering(ref ShaderPrefilteringData prefilteringData)
        {
            _prefilteringModeForwardPlus = prefilteringData.forwardPlusPrefilteringMode;
            _prefilteringModeDeferredRendering = prefilteringData.deferredPrefilteringMode;
            _prefilteringModeMainLightShadows = prefilteringData.mainLightShadowsPrefilteringMode;
            _prefilteringModeAdditionalLight = prefilteringData.additionalLightsPrefilteringMode;
            _prefilteringModeAdditionalLightShadows = prefilteringData.additionalLightsShadowsPrefilteringMode;
            _prefilteringModeScreenSpaceOcclusion = prefilteringData.screenSpaceOcclusionPrefilteringMode;
            _prefilterUseLegacyLightmaps = prefilteringData.useLegacyLightmaps;

            _prefilterXRKeywords = prefilteringData.stripXRKeywords;
            _prefilterHDROutput = prefilteringData.stripHDRKeywords;
            _prefilterAlphaOutput = prefilteringData.stripAlphaOutputKeywords;
            _prefilterDebugKeywords = prefilteringData.stripDebugDisplay;
            _prefilterWriteRenderingLayers = prefilteringData.stripWriteRenderingLayers;
            _prefilterScreenCoord = prefilteringData.stripScreenCoordOverride;
            _prefilterDBufferMRT1 = prefilteringData.stripDBufferMRT1;
            _prefilterDBufferMRT2 = prefilteringData.stripDBufferMRT2;
            _prefilterDBufferMRT3 = prefilteringData.stripDBufferMRT3;
            _refilterNativeRenderPass = prefilteringData.stripNativeRenderPass;

            _prefilterSoftShadowsQualityLow = prefilteringData.stripSoftShadowsQualityLow;
            _prefilterSoftShadowsQualityMedium = prefilteringData.stripSoftShadowsQualityMedium;
            _prefilterSoftShadowsQualityHigh = prefilteringData.stripSoftShadowsQualityHigh;
            _prefilterSoftShadows = prefilteringData.stripSoftShadows;

            //m_PrefilterSSAOBlueNoise = prefilteringData.stripSSAOBlueNoise;
            //m_PrefilterSSAOInterleaved = prefilteringData.stripSSAOInterleaved;
            //m_PrefilterSSAODepthNormals = prefilteringData.stripSSAODepthNormals;
            //m_PrefilterSSAOSourceDepthLow = prefilteringData.stripSSAOSourceDepthLow;
            //m_PrefilterSSAOSourceDepthMedium = prefilteringData.stripSSAOSourceDepthMedium;
            //m_PrefilterSSAOSourceDepthHigh = prefilteringData.stripSSAOSourceDepthHigh;
            //m_PrefilterSSAOSampleCountLow = prefilteringData.stripSSAOSampleCountLow;
            //m_PrefilterSSAOSampleCountMedium = prefilteringData.stripSSAOSampleCountMedium;
            //m_PrefilterSSAOSampleCountHigh = prefilteringData.stripSSAOSampleCountHigh;

            _prefilterLODCrossFade = prefilteringData.stripLODCrossFade;
            _prefilterProbeVolumes = prefilteringData.stripProbeVolumes;
            _prefilterAPRTVolumes = prefilteringData.stripAPRTVolumes;
            _prefilterLightLayers = prefilteringData.stripLightLayers;
            _prefilterLightCookies = prefilteringData.stripLightCookies;
            _prefilterMixedLighting = prefilteringData.stripMixedLighting;
            _prefilterReflectionProbeBlending = prefilteringData.stripReflectionProbeBlending;
            _prefilterReflectionProbeBoxProjection = prefilteringData.stripReflectionProbeBoxProjection;
        }
    }
}
#endif
