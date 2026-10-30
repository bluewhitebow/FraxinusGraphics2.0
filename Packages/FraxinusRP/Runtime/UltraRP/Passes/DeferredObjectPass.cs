using FraxinusEngine.Base;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace FraxinusEngine.Rendering.Ultra
{
    [Serializable, NodeMenuItem("UltraRP/DeferredObjectPass")]
    public class DeferredObjectPass : ScriptableRenderPassNode
    {
        [Input("Camera Target")]
        public CameraTargetBufferNodeParameter inCameraTarget;

        [Input("In Gbuffer")]
        public TextureHandlesNodeParameter inGbuffer;

        [Input("Main ShadowMap")]
        public TextureHandleNodeParameter mainShadowMap;

        [Input("Additional ShadowMap")]
        public TextureHandleNodeParameter additionalShadowMap;

        [Output("Camera Target")]
        internal CameraTargetBufferNodeParameter outCameraTarget;

        [SerializeField] private LayerMask _layerMask;
        public LayerMask layerMask
        {
            get => _layerMask;
            set
            {
                _layerMask = value;
            }
        }

        [SerializeField] private bool _cluster;
        public bool cluster
        {
            get => _cluster;
            set
            {
                _cluster = value;
            }
        }

        public override LayerMask GetOpaqueLayerMask()
        {
            return layerMask;
        }

        public override RenderingMode RequestRenderingMode()
        {
            return _cluster ? RenderingMode.DeferredCluster : RenderingMode.Deferred;
        }

        public override ScriptableRenderPassInput input => _cluster ? ScriptableRenderPassInput.Depth : base.input;

        public DeferredObjectPass() : base()
        {
            base.profilingSampler = new ProfilingSampler(nameof(DeferredObjectPass));
        }

        protected override void Enable()
        {
            base.Enable();
        }

        public override void SetupPass()
        {
            CameraTargetBuffer buffer = inCameraTarget.value;

            ConfigurePortValue(outCameraTarget, buffer);
        }

        public override bool SupportedInCurrentFrame(ContextContainer frameData)
        {
            return frameData.Get<RenderingData>().hasDeferred;
        }

        public override void Execute(RenderGraph renderGraph, ScriptableRenderContext context, ContextContainer frameData)
        {
            CameraTargetBuffer buffer = inCameraTarget.value;
            TextureHandle[] gbuffer = inGbuffer.value;

            if (!buffer.IsValid() || gbuffer is null)
                return;

            TextureHandle colorTarget = buffer.color;
            TextureHandle depthTarget = buffer.depth;

            ResourceData resourceData   = frameData.Get<ResourceData>();
            RenderingData renderingData = frameData.Get<RenderingData>();
            CameraData cameraData       = frameData.Get<CameraData>();
            LightData lightData         = frameData.Get<LightData>();

            var renderer = cameraData.renderer;
            var deferredLights = renderer.GetModule<DeferredLights>();
            var gBufferPass = renderer.GetModule<GBufferPass>();
            var deferredPass = renderer.GetModule<DeferredPass>();

            TextureHandle activeColorTexture = colorTarget;
            TextureHandle activeDepthTexture = depthTarget;

            TextureHandle mainShadowsTexture = mainShadowMap.value;
            TextureHandle additionalShadowsTexture = additionalShadowMap.value;

            deferredPass.Render(renderGraph, frameData, activeColorTexture, activeDepthTexture, gbuffer, mainShadowsTexture, additionalShadowsTexture, cluster);
        }
    }
}
