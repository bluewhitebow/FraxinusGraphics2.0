using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace FraxinusEngine.Rendering.Ultra
{
    /// <summary>Screen-space ambient visibility evaluated before opaque lighting.</summary>
    [Serializable, VolumeComponentMenu("Lighting/Ground Truth Ambient Occlusion")]
    [SupportedOnRenderPipeline(typeof(UltraRenderPipelineAsset))]
    public sealed class GroundTruthAmbientOcclusion : VolumeComponent, IPostProcessComponent
    {
        public BoolParameter enable = new BoolParameter(false);
        [Tooltip("Trace at half resolution and reconstruct full-resolution visibility using depth-aware filtering.")]
        public BoolParameter downSample = new BoolParameter(true);
        public ClampedFloatParameter intensity = new ClampedFloatParameter(1f, 0f, 4f);
        [Tooltip("Occluder search radius in world units.")]
        public ClampedFloatParameter radius = new ClampedFloatParameter(1f, 0.05f, 5f);
        [Tooltip("Relax distant horizon samples to reduce occlusion behind thin objects.")]
        public ClampedFloatParameter thickness = new ClampedFloatParameter(0.5f, 0.001f, 1f);
        [Tooltip("Optional influence on direct lighting, including rectangle lights. Zero affects indirect lighting only.")]
        public ClampedFloatParameter directLightingStrength = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter blurSharpness = new ClampedFloatParameter(0.5f, 0f, 1f);
        [AdditionalProperty]
        public ClampedIntParameter stepCount = new ClampedIntParameter(6, 2, 32);
        [AdditionalProperty]
        public ClampedIntParameter directionCount = new ClampedIntParameter(2, 1, 6);
        [AdditionalProperty, Tooltip("Search radius limit in trace pixels at half-resolution 1080p; scales with camera and tracing resolution to preserve its screen-space extent.")]
        public ClampedIntParameter maximumRadiusInPixels = new ClampedIntParameter(40, 16, 256);

        public bool IsActive() => enable.value && intensity.value > 0f && radius.value > 0f;
    }
}
