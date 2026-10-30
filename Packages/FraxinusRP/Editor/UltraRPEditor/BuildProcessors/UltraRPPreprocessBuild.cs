using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using FraxinusEngine.Rendering.Ultra;

using UnityEditor;

namespace FraxinusEditor.Rendering.Ultra
{
    class UltraRPPreprocessBuild : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => int.MinValue + 100;

        private static UltraRPBuildData s_buildData = null;

        public void OnPreprocessBuild(BuildReport report)
        {
            ClearBuildState();
            bool isDevelopmentBuild = (report.summary.options & BuildOptions.Development) != 0;
            s_buildData = new UltraRPBuildData(EditorUserBuildSettings.activeBuildTarget, isDevelopmentBuild);

            if (s_buildData.buildingPlayerForUltraRenderPipeline)
            {
                try
                {
                    if (!UltraRPBuildDataValidator.IsProjectValidForBuilding(report, out var message))
                        throw new BuildFailedException(message);
                    UltraRPShaderBuildDiagnostics.Begin(report);
                    UltraRenderPipelineGlobalSettings.Ensure()?.PrepareGraphicsSettingsForPlayerBuild();
                    UltraRPShaderPrefiltering.PrepareForBuild(isDevelopmentBuild);
                    LogIncludedAssets(s_buildData.renderPipelineAssets);
                }
                catch
                {
                    ClearBuildState();
                    UltraRPShaderPrefiltering.RestorePending();
                    throw;
                }
            }
        }

        internal static void LogIncludedAssets(List<UltraRenderPipelineAsset> assetsList)
        {
            using (GenericPool<StringBuilder>.Get(out var assetsIncluded))
            {
                assetsIncluded.Clear();

                assetsIncluded.AppendLine($"{assetsList.Count} UltraRP assets included in build");

                foreach (var ultraAsset in assetsList)
                {
                    assetsIncluded.AppendLine($"- {ultraAsset.name} - {AssetDatabase.GetAssetPath(ultraAsset)}");
                    foreach (var rendererGraph in ultraAsset.rendererGraphList)
                    {
                        if (rendererGraph != null)
                            assetsIncluded.AppendLine($"\t - {rendererGraph.name} - {AssetDatabase.GetAssetPath(rendererGraph)} - {rendererGraph.GetType()}");
                    }
                }

                Debug.Log(assetsIncluded);
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            try { UltraRPShaderBuildDiagnostics.Complete(report); }
            finally
            {
                // Clear the in-memory state before recovery, which can fail on disk I/O.
                ClearBuildState();
                UltraRPShaderPrefiltering.RestorePending();
            }
        }

        static void ClearBuildState()
        {
            UltraRPShaderBuildDiagnostics.End();
            s_buildData?.Dispose();
            s_buildData = null;
        }
    }
}
