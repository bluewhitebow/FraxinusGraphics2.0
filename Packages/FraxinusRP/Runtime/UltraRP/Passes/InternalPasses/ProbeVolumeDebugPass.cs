using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace FraxinusEngine.Rendering.Ultra
{
#if UNITY_EDITOR
    [InjectPass(typeof(DrawObjectsPass), InjectionPoint.After, true)]
    [InjectPass(typeof(DeferredObjectPass), InjectionPoint.After, true)]
    public class ProbeVolumeDebugPass : InjectPass
    {
        class WriteApvData
        {
            public ComputeShader computeShader;
            public BufferHandle resultBuffer;
            public Vector2 clickCoordinates;
            public TextureHandle depthBuffer;
            public TextureHandle normalBuffer;
        }

        private ComputeShader _computeShader;

        public ProbeVolumeDebugPass(): base()
        {
            if (RenderPipelineGraphicsSettingsCompatibility.TryGetRenderPipelineSettings<UltraPipelineDebugShaders>(out var debugShaders))
                _computeShader = debugShaders.probeVolumeSamplingDebugComputeShader;
        }

        protected override void OnCameraCleanup(CommandBuffer cmd){ }

        protected override void OnDispose() { }

        public override bool IsEligibleToInject(ScriptableRenderer renderer, ScriptableRenderPassNode node)
        {
            var nodes = renderer.sortedNodes;
            var lastOpaqueDrawPass = nodes.OfType<DrawObjectsPass>().LastOrDefault(n => n.isOpaque && n.active && renderer.IsSameContext(n, node));
            var lastDeferredPass = nodes.OfType<DeferredObjectPass>().LastOrDefault(p => p.active && renderer.IsSameContext(p, node));
            int drawPassIndex = lastOpaqueDrawPass != null ? nodes.IndexOf(lastOpaqueDrawPass) : -1;
            int deferredPassIndex = lastDeferredPass != null ? nodes.IndexOf(lastDeferredPass) : -1;

            ScriptableRenderPassNode targetNode = null;
            if (drawPassIndex > deferredPassIndex)
                targetNode = lastOpaqueDrawPass;
            else if (deferredPassIndex >= 0)
                targetNode = lastDeferredPass;

            return node == targetNode;
        }

        public override void Execute(RenderGraph renderGraph, ScriptableRenderContext context, ContextContainer frameData)
        {
            if (UltraRenderPipeline.asset == null || !UltraRenderPipeline.asset.supportProbeVolume)
                return;

            CameraData cameraData       = frameData.Get<CameraData>();
            ResourceData resourceData   = frameData.Get<ResourceData>();

            TextureHandle cameraDepthTexture = resourceData.cameraDepthTexture;
            TextureHandle cameraNormalsTexture = resourceData.cameraNormalsTexture;

            if (!ProbeReferenceVolumeProvider.instance.isInitialized)
                return;

            if (ProbeReferenceVolumeProvider.instance.GetProbeSamplingDebugResources(cameraData.camera, out var resultBuffer, out Vector2 coords))
            {
                using (var builder = renderGraph.AddComputePass<WriteApvData>(_passName, out var passData, _profilingSampler))
                {
                    passData.clickCoordinates = coords;
                    passData.computeShader = _computeShader;

                    passData.resultBuffer = renderGraph.ImportBuffer(resultBuffer);
                    passData.depthBuffer = cameraDepthTexture;
                    passData.normalBuffer = cameraNormalsTexture;

                    builder.UseBuffer(passData.resultBuffer, AccessFlags.Write);
                    builder.UseTexture(passData.depthBuffer, AccessFlags.Read);
                    builder.UseTexture(passData.normalBuffer, AccessFlags.Read);

                    builder.SetRenderFunc((WriteApvData data, ComputeGraphContext ctx) =>
                    {
                        int kernel = data.computeShader.FindKernel("ComputePositionNormal");

                        ctx.cmd.SetComputeTextureParam(data.computeShader, kernel, "_CameraDepthTexture", data.depthBuffer);
                        ctx.cmd.SetComputeTextureParam(data.computeShader, kernel, "_NormalBufferTexture", data.normalBuffer);
                        ctx.cmd.SetComputeVectorParam(data.computeShader, "_positionSS", new Vector4(data.clickCoordinates.x, data.clickCoordinates.y, 0.0f, 0.0f));
                        ctx.cmd.SetComputeBufferParam(data.computeShader, kernel, "_ResultBuffer", data.resultBuffer);
                        ctx.cmd.DispatchCompute(data.computeShader, kernel, 1, 1, 1);
                    });
                }
            }
        }
    }
#endif
}
