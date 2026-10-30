using UnityEngine;
using UnityEngine.Rendering;

namespace FraxinusEngine.Rendering.Ultra
{
    public partial class UltraRenderPipelineRuntimeShaders
    {
        [SerializeField, ResourcePath("Shaders/Lighting/AmbientOcclusion/GroundTruthAmbientOcclusion.compute")]
        ComputeShader _groundTruthAmbientOcclusionCS;

        public ComputeShader groundTruthAmbientOcclusionCS
        {
            get => _groundTruthAmbientOcclusionCS;
            set => this.SetValueAndNotify(ref _groundTruthAmbientOcclusionCS, value);
        }
    }
}
