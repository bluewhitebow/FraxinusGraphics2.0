using System;
using FraxinusEngine.Rendering.Ultra;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace FraxinusEditor.Rendering.Ultra
{
    // Use Core's interface explicitly; the source package also retains unrelated
    // interfaces with the same short names in this namespace.
    internal sealed class UltraRPDeferredComputeShaderStripper :
        UnityEditor.Rendering.IComputeShaderVariantStripper,
        UnityEditor.Rendering.IComputeShaderVariantStripperScope
    {
        UltraRPBuildData _buildData;
        bool _stripUnusedVariants;
        bool _clusteredUnused;
        bool _canStripShader;
        int _removed;

        // Core discovers public parameterless constructors, including on internal types.
        public UltraRPDeferredComputeShaderStripper() { }

        public bool active => IsEnabledForCurrentBuild();

        static bool IsEnabledForCurrentBuild()
        {
            if (!BuildPipeline.isBuildingPlayer ||
                !UltraRPBuildData.instance.buildingPlayerForUltraRenderPipeline)
                return false;
            var globalSettings = GraphicsSettings.GetSettingsForRenderPipeline<UltraRenderPipeline>()
                as UltraRenderPipelineGlobalSettings;
            if (globalSettings == null)
                return false;
            var settings = RenderPipelineGraphicsSettingsCompatibility
                .GetRenderPipelineSettings<UltraRenderPipelineShaderStrippingSetting>(globalSettings);
            return settings != null && settings.stripUnusedVariants;
        }

        void PrepareBuild()
        {
            var buildData = UltraRPBuildData.instance;
            if (ReferenceEquals(_buildData, buildData))
                return;
            _buildData = buildData;
            _stripUnusedVariants = IsEnabledForCurrentBuild();
            _clusteredUnused = false;
            if (!_stripUnusedVariants)
                return;
            bool known = UltraRPPostProcessShaderStripper.TryAnalyzeDeclaredBuild(
                CoreBuildData.instance.renderPipelineAssets, out var usage, out int graphCount, out string reason);
            _clusteredUnused = known && UltraRPPostProcessShaderStripper.ShouldStripClusteredCompute(true, new[] { usage });
            Debug.Log("UltraRP DeferredLit compute stripping: " + (_clusteredUnused ? "remove" : "retain") +
                ", declared renderer graphs examined: " + graphCount + ". " + reason +
                ". Undeclared runtime graphs or lighting modes require stripUnusedVariants=false.");
        }

        internal static bool ShouldStripComputeShader(bool stripUnusedVariants, bool clusteredUnused, string path, string name)
        {
            return stripUnusedVariants && clusteredUnused && name == "DeferredLit" &&
                string.Equals(path?.Replace('\\', '/'),
                    UltraRenderPipelineAsset.packagePath + "/Shaders/Lighting/DeferredLit.compute", StringComparison.Ordinal);
        }

        public void BeforeShaderStripping(ComputeShader shader)
        {
            PrepareBuild();
            _removed = 0;
            _canStripShader = ShouldStripComputeShader(_stripUnusedVariants, _clusteredUnused,
                AssetDatabase.GetAssetPath(shader), shader.name);
        }

        public bool CanRemoveVariant(ComputeShader shader, string kernelName, ShaderCompilerData data)
        {
            // Unknown shaders must return false. Using a skipper here could make
            // Core's All() evaluate true when every registered stripper skips.
            if (_canStripShader)
                ++_removed;
            return _canStripShader;
        }

        public void AfterShaderStripping(ComputeShader shader)
        {
            if (_removed > 0)
                Debug.Log("UltraRP compute stripping: " + shader.name + ", removed " + _removed + " variants.");
        }
    }
}
