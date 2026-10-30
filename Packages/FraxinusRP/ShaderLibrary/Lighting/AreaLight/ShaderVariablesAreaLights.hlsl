#ifndef FRAXINUS_SHADER_VARIABLES_AREA_LIGHTS_INCLUDED
#define FRAXINUS_SHADER_VARIABLES_AREA_LIGHTS_INCLUDED

// Kept in sync with AreaLightManager. Absolute world space; sizes are half extents.
// Fixed arrays avoid a StructuredBuffer requirement in the Forward material path.
#define FRAXINUS_MAX_AREA_LIGHTS 16
int _FraxinusAreaLightCount;
int _FraxinusLightingLutsReady;
float4 _FraxinusAreaLightPositionRange[FRAXINUS_MAX_AREA_LIGHTS]; // xyz: center, w: 1 / range^2
float4 _FraxinusAreaLightRightSize[FRAXINUS_MAX_AREA_LIGHTS];    // xyz: unit right, w: half width
float4 _FraxinusAreaLightUpSize[FRAXINUS_MAX_AREA_LIGHTS];       // xyz: unit up, w: half height
float4 _FraxinusAreaLightColor[FRAXINUS_MAX_AREA_LIGHTS];        // xyz: linear radiance
float4 _FraxinusAreaLightLayers[FRAXINUS_MAX_AREA_LIGHTS];       // low/high 16-bit mask, diffuse/specular dimmer

#endif
