using UnityEngine;
using UnityEngine.Rendering;
using FraxinusEngine.Rendering.Ultra;
using UnityEditor;
using UnityEditor.Rendering;

namespace FraxinusEditor.Rendering.Ultra
{
    using CED = CoreEditorDrawer<UltraRenderPipelineSerializedCamera>;

    static partial class UltraRenderPipelineCameraUI
    {
        public partial class Rendering
        {
            static bool s_postProcessingWarningShown = false;

            private static readonly CED.IDrawer postProcessingWarningDrawer = CED.Conditional(
                (serialized, owner) => IsAnyRendererHasPostProcessingEnabled(serialized, UltraRenderPipeline.asset) && serialized.renderPostProcessing.boolValue,
                (serialized, owner) =>
                {
                    EditorGUILayout.HelpBox(Styles.disabledPostprocessing, MessageType.Warning);
                    s_postProcessingWarningShown = true;
                });

            private static readonly CED.IDrawer msaaWarningDrawer = CED.Conditional(
                (serialized, owner) => !IsSGSR2Selected() && (GraphicsSettings.currentRenderPipeline is UltraRenderPipelineAsset asset && asset.msaaSampleCount > 1) && serialized.baseCameraSettings.allowMSAA.boolValue == true && (AntialiasingMode)serialized.antialiasing.intValue == AntialiasingMode.TemporalAntiAliasing,
                (serialized, owner) => EditorGUILayout.HelpBox(Styles.msaaWarning, MessageType.Warning));

            private static readonly CED.IDrawer postProcessingStopNaNsWarningDrawer = CED.Conditional(
                (serialized, owner) => !s_postProcessingWarningShown && IsAnyRendererHasPostProcessingEnabled(serialized, UltraRenderPipeline.asset) && serialized.stopNaNs.boolValue,
                (serialized, owner) =>
                {
                    EditorGUILayout.HelpBox(Styles.disabledPostprocessing, MessageType.Warning);
                    s_postProcessingWarningShown = true;
                });

            private static readonly CED.IDrawer postProcessingDitheringWarningDrawer = CED.Conditional(
                (serialized, owner) => !s_postProcessingWarningShown && IsAnyRendererHasPostProcessingEnabled(serialized, UltraRenderPipeline.asset) && serialized.dithering.boolValue,
                (serialized, owner) =>
                {
                    EditorGUILayout.HelpBox(Styles.disabledPostprocessing, MessageType.Warning);
                    s_postProcessingWarningShown = true;
                });

            public static readonly CED.IDrawer drawer;

            public static readonly CED.IDrawer drawerPreset;

            static Rendering()
            {
                drawer = CED.AdditionalPropertiesFoldoutGroup(
                    CameraUI.Rendering.Styles.header,
                    Expandable.Rendering,
                    s_expandedState,
                    ExpandableAdditional.Rendering,
                    s_expandedAdditionalState,
                    CED.Group(
                        CED.Group(
                            DrawerRenderingRenderer
                        ),
                        baseCameraRenderTypeDrawer,
                        overlayCameraRenderTypeDrawer,
                        CED.Group(
                            CameraUI.Rendering.Drawer_Rendering_CullingMask,
                            CameraUI.Rendering.Drawer_Rendering_OcclusionCulling
                        )
                    ),
                    CED.noop,
                    FoldoutOption.Indent,
                    (serialized, owner) => s_postProcessingWarningShown = false
                );

                drawerPreset = CED.FoldoutGroup(
                    CameraUI.Rendering.Styles.header,
                    Expandable.Rendering,
                    s_expandedState,
                    FoldoutOption.Indent,
                    CED.Group(
                        CameraUI.Rendering.Drawer_Rendering_CullingMask,
                        CameraUI.Rendering.Drawer_Rendering_OcclusionCulling
                    )
                );
            }

            static readonly CED.IDrawer baseCameraRenderTypeDrawer = CED.Conditional(
                   (serialized, owner) => (CameraRenderType)serialized.cameraType.intValue == CameraRenderType.Base,
                CED.Group(
                    DrawerRenderingRenderPostProcessing
                ),
                postProcessingWarningDrawer,
                CED.Group(
                    DrawerRenderingAntialiasing
                ),
                msaaWarningDrawer,
                CED.Conditional(
                        (serialized, owner) => !serialized.antialiasing.hasMultipleDifferentValues && !IsSGSR2Selected(),
                        CED.Group(
                            GroupOption.Indent,
                            new[]{
                                CED.Conditional(
                                    (serialized, owner) => (AntialiasingMode)serialized.antialiasing.intValue ==
                                    AntialiasingMode.SubpixelMorphologicalAntiAliasing,
                                    CED.Group(
                                        DrawerRenderingSMAAQuality
                                    )),
                                CED.Conditional(
                                    (serialized, owner) => (AntialiasingMode)serialized.antialiasing.intValue ==
                                    AntialiasingMode.TemporalAntiAliasing,
                                    CED.Group(
                                        DrawerRenderingTAAQuality
                                    ))
                            }
                    )
                ),
                CED.Group(
                    CameraUI.Rendering.Drawer_Rendering_StopNaNs
                    ),
                    postProcessingStopNaNsWarningDrawer,
                CED.Group(
                    CameraUI.Rendering.Drawer_Rendering_Dithering
                    ),
                    postProcessingDitheringWarningDrawer,
                CED.Group(
                    DrawerRenderingRenderShadows,
                    DrawerRenderingPriority
                )
            );

            static readonly CED.IDrawer overlayCameraRenderTypeDrawer = CED.Conditional(
                (serialized, owner) => (CameraRenderType)serialized.cameraType.intValue == CameraRenderType.Overlay,
                CED.Group(
                    DrawerRenderingRenderPostProcessing
                    ),

                CED.Group(
                    DrawerRenderingClearDepth,
                    DrawerRenderingRenderShadows
                )
            );

            static void DrawerRenderingRenderer(UltraRenderPipelineSerializedCamera p, Editor owner)
            {
                var rpAsset = UltraRenderPipeline.asset;

                int selectedRendererOption = p.renderer.intValue;
                EditorGUI.BeginChangeCheck();

                Rect controlRect = EditorGUILayout.GetControlRect(true);
                EditorGUI.BeginProperty(controlRect, Styles.rendererType, p.renderer);

                EditorGUI.showMixedValue = p.renderer.hasMultipleDifferentValues;
                int selectedRenderer = EditorGUI.IntPopup(controlRect, Styles.rendererType, selectedRendererOption, rpAsset.rendererDisplayList, rpAsset.rendererIndexList);
                EditorGUI.EndProperty();

                if (!rpAsset.ValidateRendererGraphList())
                    EditorGUILayout.HelpBox(Styles.noRendererError, MessageType.Error);
                else if (!rpAsset.ValidateRendererGraph(selectedRendererOption))
                {
                    EditorGUILayout.HelpBox(Styles.missingRendererWarning, MessageType.Warning);
                    var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
                    if (GUI.Button(rect, Styles.selectRenderPipelineAsset))
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath<UltraRenderPipelineAsset>(AssetDatabase.GetAssetPath(UltraRenderPipeline.asset));

                    GUILayout.Space(5);
                }

                if (EditorGUI.EndChangeCheck())
                    p.renderer.intValue = selectedRenderer;
            }

            static void DrawerRenderingAntialiasing(UltraRenderPipelineSerializedCamera p, Editor owner)
            {
                if (IsSGSR2Selected())
                {
                    EditorGUILayout.HelpBox("SGSR2 supplies anti-aliasing. This camera's AA selection is preserved for other upscaling filters. Enable Post Processing and disable MSAA, stacking and dynamic resolution; unsupported cameras use Bilinear without temporal jitter.", MessageType.Info);
                    return;
                }

                Rect antiAliasingRect = EditorGUILayout.GetControlRect();
                EditorGUI.BeginProperty(antiAliasingRect, Styles.antialiasing, p.antialiasing);
                {
                    EditorGUI.BeginChangeCheck();
                    int selectedValue = (int)(AntialiasingMode)EditorGUI.EnumPopup(antiAliasingRect, Styles.antialiasing, (AntialiasingMode)p.antialiasing.intValue);
                    if (EditorGUI.EndChangeCheck())
                        p.antialiasing.intValue = selectedValue;
                }
                EditorGUI.EndProperty();
            }

            static bool IsSGSR2Selected()
            {
                var pipeline = UltraRenderPipeline.asset;
                return pipeline != null && pipeline.upscalingFilter == UpscalingFilterSelection.SGSR2 && pipeline.renderScale <= 1.0f;
            }

            static void DrawerRenderingSMAAQuality(UltraRenderPipelineSerializedCamera p, Editor owner)
            {
                EditorGUILayout.PropertyField(p.antialiasingQuality, Styles.antialiasingQuality);
            }

            static void DrawerRenderingTAAQuality(UltraRenderPipelineSerializedCamera p, Editor owner)
            {
                EditorGUILayout.PropertyField(p.taaQuality, Styles.antialiasingQuality);

                {
                    // FSR overrides TAA CAS settings. Disable this setting when FSR is enabled.
                    bool disableSharpnessControl = UltraRenderPipeline.asset != null ?
                        (UltraRenderPipeline.asset.upscalingFilter == UpscalingFilterSelection.FSR) : false;
                    using var disable = new EditorGUI.DisabledScope(disableSharpnessControl);

                    EditorGUILayout.Slider(p.taaContrastAdaptiveSharpening, 0.0f, 1.0f, Styles.taaContrastAdaptiveSharpening);
                }

                bool additionalPropertiesVisible = s_expandedState[Expandable.Rendering] && s_expandedAdditionalState[ExpandableAdditional.Rendering];
                if (additionalPropertiesVisible)
                {
                    p.taaFrameInfluence.floatValue = 1.0f - EditorGUILayout.Slider(Styles.taaBaseBlendFactor, 1.0f - p.taaFrameInfluence.floatValue, 0.6f, 0.98f);
                    EditorGUILayout.Slider(p.taaJitterScale, 0.0f, 1.0f, Styles.taaJitterScale);
                    EditorGUILayout.Slider(p.taaMipBias, -1.0f, 0.0f, Styles.taaMipBias);

                    if (p.taaQuality.intValue >= (int)TemporalAAQuality.Medium)
                        EditorGUILayout.Slider(p.taaVarianceClampScale, 0.6f, 1.2f, Styles.taaVarianceClampScale);

                    bool isEditorInDeveloperMode = EditorPrefs.GetBool("DeveloperMode");
                    if (isEditorInDeveloperMode)
                    {
                        UltraRenderPipelineCameraEditor ultraRPCamEditor = owner as UltraRenderPipelineCameraEditor;
                        if (ultraRPCamEditor != null && ultraRPCamEditor.camera != null &&
                            ultraRPCamEditor.camera.TryGetComponent<UltraAdditionalCameraData>(out var urpAddCamData))
                        {
                            ref var taa = ref urpAddCamData.taaSettings;
                            var rect = GUILayoutUtility.GetRect(Styles.taaResetHistory, GUIStyle.none);
                            rect.x = rect.x + 32;
                            rect.width = rect.width - 32;
                            rect.height += 4; // avoid clipping the label.
                            if (GUI.Button(rect, Styles.taaResetHistory))
                                taa.resetHistoryFrames += 2; // XR both eyes
                        }
                    }
                }
            }

            static void DrawerRenderingClearDepth(UltraRenderPipelineSerializedCamera p, Editor owner)
            {
                EditorGUILayout.PropertyField(p.clearDepth, Styles.clearDepth);
            }

            static void DrawerRenderingRenderShadows(UltraRenderPipelineSerializedCamera p, Editor owner)
            {
                EditorGUILayout.PropertyField(p.renderShadows, Styles.renderingShadows);
            }

            static void DrawerRenderingRenderPostProcessing(UltraRenderPipelineSerializedCamera p, Editor owner)
            {
                EditorGUILayout.PropertyField(p.renderPostProcessing, Styles.renderPostProcessing);
            }

            static void DrawerRenderingPriority(UltraRenderPipelineSerializedCamera p, Editor owner)
            {
                EditorGUILayout.PropertyField(p.baseCameraSettings.depth, Styles.priority);
            }

            static bool IsAnyRendererHasPostProcessingEnabled(UltraRenderPipelineSerializedCamera p, UltraRenderPipelineAsset rpAsset)
            {
                int selectedRendererOption = p.renderer.intValue;

                if (selectedRendererOption < -1 || selectedRendererOption >= rpAsset.rendererGraphList.Length || p.renderer.hasMultipleDifferentValues)
                    return false;

                var rendererData = selectedRendererOption == -1 ? rpAsset.scriptableRendererGraph : rpAsset.rendererGraphList[selectedRendererOption];

                var forwardRendererData = rendererData as UltraRendererGraph;
                if (forwardRendererData != null && forwardRendererData.postProcessData == null)
                    return true;
                else
                    return false;
            }
        }
    }
}
