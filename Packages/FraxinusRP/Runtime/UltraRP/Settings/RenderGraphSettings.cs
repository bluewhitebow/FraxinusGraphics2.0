using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace FraxinusEngine.Rendering.Ultra
{
    [Serializable]
    [SupportedOnRenderPipeline(typeof(UltraRenderPipelineAsset))]
    [UnityEngine.Rendering.Categorization.CategoryInfo(Name = "Render Graph", Order = 50)]
    [UnityEngine.Rendering.Categorization.ElementInfo(Order = -10)]
    public class RenderGraphSettings : IRenderPipelineGraphicsSettings
    {
        public int version => 0;

        bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;
    }

}