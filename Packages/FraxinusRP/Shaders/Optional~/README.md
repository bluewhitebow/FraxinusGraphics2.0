# Optional ShaderGraph assets

These nine Unity 6 graph assets are retained with their original relative paths and `.meta` GUIDs for a later ShaderGraph 14 adaptation. Unity ignores this directory, so Decal, SpeedTree, Autodesk Interactive, fullscreen and VFX graph shaders are not imported or offered as supported UltraRP shaders in the Unity 2022 package.

No active material or serialized resource referenced these graph GUIDs when they were isolated. The matching UltraRP target and graph inspectors are retained in `Editor/UltraRPEditor/Optional~`; two material import processors that load the Autodesk graphs are retained there under `AssetPostProcessors`. Restoring the graphs requires adapting the target, templates, include paths, inspectors and importers together.

The active Unity 2022 defaults for SpeedTree 7 and 8 use the hand-written
shaders in `Shaders/Nature`. Their availability does not enable the optional
SpeedTree 8/9 graphs here. SpeedTree 9's default getter explicitly returns null.
