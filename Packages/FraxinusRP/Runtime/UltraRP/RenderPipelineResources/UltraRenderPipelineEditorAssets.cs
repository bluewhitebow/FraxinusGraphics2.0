#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace FraxinusEngine.Rendering.Ultra
{
    [Serializable]
    [SupportedOnRenderPipeline(typeof(UltraRenderPipelineAsset))]
    [UnityEngine.Rendering.Categorization.CategoryInfo(Name = "R: Editor Assets", Order = 1000), HideInInspector]
    class UltraRenderPipelineEditorAssets : IRenderPipelineResources
    {
        public int version => 0;

        bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => false;

        [SerializeField]
        [ResourcePath("Editor/UltraRPEditor/Volume/DefaultVolumeProfile.asset")]
        private VolumeProfile _defaultSettingsVolumeProfile;
        
        public VolumeProfile defaultVolumeProfile
        {
            get => _defaultSettingsVolumeProfile;
            set => this.SetValueAndNotify(ref _defaultSettingsVolumeProfile, value);
        }
    }
}
#endif
