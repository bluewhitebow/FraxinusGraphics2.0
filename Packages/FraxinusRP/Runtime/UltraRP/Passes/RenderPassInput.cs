namespace FraxinusEngine.Rendering.Ultra
{
    public struct RenderPassInputSummary
    {
        internal bool requiresIntermediateTexture;
        internal bool requiresDepthTexture;
        internal bool requiresColorTexture;
        internal bool requiresColorTextureCreated;
        internal bool requiresMotionVectors;
        internal bool requiresNormalsTexture;
        internal bool requiresDepthPrepass;
        // Early screen-space consumers need all opaque depth, including deferred materials.
        internal bool requiresFullDepthPrepass;
        internal bool disableFramebufferFetch;
    }
}
