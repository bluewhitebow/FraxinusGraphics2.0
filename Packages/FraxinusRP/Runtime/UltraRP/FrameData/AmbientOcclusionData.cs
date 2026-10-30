using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace FraxinusEngine.Rendering.Ultra
{
    // A camera-local snapshot. Child contexts must not borrow screen-space visibility.
    internal sealed class AmbientOcclusionData : ContextItem
    {
        internal bool active, downSample;
        internal int width, height, traceWidth, traceHeight, orthographic, normalsOct;
        internal int downsampleKernel, traceKernel, denoiseKernel;
        internal float directLightingStrength;
        internal ComputeShader shader;
        internal TextureHandle output;
        internal GraphicsFormat outputFormat;
        internal Matrix4x4 projection, inverseProjection, view;
        internal Vector4 parameters, sampling;

        public override void Reset()
        {
            active = downSample = false;
            width = height = traceWidth = traceHeight = orthographic = normalsOct = 0;
            downsampleKernel = traceKernel = denoiseKernel = 0;
            directLightingStrength = 0;
            shader = null;
            output = TextureHandle.nullHandle;
            outputFormat = GraphicsFormat.None;
            projection = inverseProjection = view = default;
            parameters = sampling = default;
        }
    }
}
