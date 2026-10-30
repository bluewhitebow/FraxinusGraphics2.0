using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace FraxinusEngine.Rendering.Ultra
{
    // Shared read-only by same-view child contexts. All handles belong to this graph.
    internal sealed class AreaLightFrameData : ContextItem
    {
        internal int lightCount;
        internal TextureHandle ltc, fgd;

        public override void Reset()
        {
            lightCount = 0;
            ltc = fgd = TextureHandle.nullHandle;
        }
    }
}
