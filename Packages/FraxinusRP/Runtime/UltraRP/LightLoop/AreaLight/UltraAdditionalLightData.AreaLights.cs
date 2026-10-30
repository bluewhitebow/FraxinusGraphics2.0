using System.Collections.Generic;
using UnityEngine;

namespace FraxinusEngine.Rendering.Ultra
{
    public partial class UltraAdditionalLightData
    {
        internal const float minimumAreaLightSize = 0.001f;

        // Register every enabled data component so changing Light.type at runtime
        // does not require polling or creating components during rendering.
        static readonly List<UltraAdditionalLightData> s_AreaLightCandidates = new();
        internal static IReadOnlyList<UltraAdditionalLightData> areaLightCandidates => s_AreaLightCandidates;

        [SerializeField] Vector2 _areaSize = new(0.5f, 0.5f);
#if UNITY_EDITOR
        [SerializeField, HideInInspector] bool _areaSizeInitialized;
#endif
        [SerializeField, HideInInspector] LightmapBakeType _areaLightBakeType = LightmapBakeType.Realtime;
        [SerializeField, Range(0f, 1f)] float _areaDiffuseDimmer = 1f;
        [SerializeField, Range(0f, 1f)] float _areaSpecularDimmer = 1f;

        /// <summary>Rectangle width and height in world units, independent of transform scale.</summary>
        public Vector2 areaSize
        {
            get => ClampAreaSize(_areaSize);
            set
            {
                _areaSize = ClampAreaSize(value);
#if UNITY_EDITOR
                _areaSizeInitialized = true;
                SyncAreaSizeToLight();
#endif
            }
        }

        /// <summary>Diffuse contribution of a realtime rectangle light.</summary>
        public float areaDiffuseDimmer
        {
            get => ClampAreaDimmer(_areaDiffuseDimmer);
            set => _areaDiffuseDimmer = ClampAreaDimmer(value);
        }

        /// <summary>Specular contribution of a realtime rectangle light.</summary>
        public float areaSpecularDimmer
        {
            get => ClampAreaDimmer(_areaSpecularDimmer);
            set => _areaSpecularDimmer = ClampAreaDimmer(value);
        }

        internal bool isRealtimeAreaLight
        {
            get
            {
                if (!light || light.type != LightType.Rectangle)
                    return false;
#if UNITY_EDITOR
                return light.lightmapBakeType == LightmapBakeType.Realtime;
#else
                return _areaLightBakeType == LightmapBakeType.Realtime;
#endif
            }
        }

        static Vector2 ClampAreaSize(Vector2 value)
        {
            return new Vector2(ClampAreaDimension(value.x), ClampAreaDimension(value.y));
        }

        static float ClampAreaDimension(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value)
                ? minimumAreaLightSize : Mathf.Max(minimumAreaLightSize, value);
        }

        static float ClampAreaDimmer(float value)
        {
            return float.IsNaN(value) ? 0f : Mathf.Clamp01(value);
        }

        void OnEnable()
        {
#if UNITY_EDITOR
            InitializeAreaSizeFromLight();
#endif
            if (!s_AreaLightCandidates.Contains(this))
                s_AreaLightCandidates.Add(this);
        }

        void OnDisable()
        {
            s_AreaLightCandidates.Remove(this);
        }

#if UNITY_EDITOR
        // Light.areaSize is editor-only in Unity 2022. Persist it on this
        // component before building so the player never needs that API.
        internal void InitializeAreaSizeFromLight()
        {
            if (!light || light.type != LightType.Rectangle)
                return;

            // Light.lightmapBakeType is also editor-only in Unity 2022.
            // Preserve the authored mode independently of the last GI bake.
            _areaLightBakeType = light.lightmapBakeType;
            if (_areaSizeInitialized)
                return;

            _areaSize = ClampAreaSize(light.areaSize);
            _areaSizeInitialized = true;
        }

        internal void SyncAreaSizeToLight()
        {
            if (light && light.type == LightType.Rectangle)
                light.areaSize = areaSize;
        }

        void OnValidate()
        {
            InitializeAreaSizeFromLight();
            _areaSize = ClampAreaSize(_areaSize);
            _areaDiffuseDimmer = ClampAreaDimmer(_areaDiffuseDimmer);
            _areaSpecularDimmer = ClampAreaDimmer(_areaSpecularDimmer);
            SyncAreaSizeToLight();
        }
#endif
    }
}
