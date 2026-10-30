using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using FraxinusEngine.Rendering.Ultra;
using UnityEditor;
using UnityEditor.Rendering;

namespace FraxinusEditor.Rendering.Ultra
{
    internal class UltraRPBuildData : IDisposable
    {
        // Instance can be null when building AssetBundles, otherwise the lifetime will be given by UltraRPPreprocessBuild
        static UltraRPBuildData s_instance = null;

        public static UltraRPBuildData instance => s_instance ??= new(EditorUserBuildSettings.activeBuildTarget, Debug.isDebugBuild);

        public bool buildingPlayerForUltraRenderPipeline { get; private set; }

        public List<UltraRenderPipelineAsset> renderPipelineAssets { get; private set; } = new List<UltraRenderPipelineAsset>();

        public UltraRPBuildData(BuildTarget buildTarget, bool isDevelopmentBuild)
        {
            s_instance = this;

            buildingPlayerForUltraRenderPipeline = CoreBuildData.instance.buildingPlayerForRenderPipeline &&
                                                   CoreBuildData.instance.currentRenderPipelineAssetType == typeof(UltraRenderPipelineAsset);

            if (buildingPlayerForUltraRenderPipeline)
            {
                foreach (var asset in CoreBuildData.instance.renderPipelineAssets)
                {
                    Assert.IsTrue(asset is UltraRenderPipelineAsset);
                    renderPipelineAssets.Add(asset as UltraRenderPipelineAsset);
                }
            }
        }

        public void Dispose()
        {
            renderPipelineAssets?.Clear();

            buildingPlayerForUltraRenderPipeline = false;

            CoreBuildData.instance.Dispose();

            s_instance = null;
        }
    }
}
