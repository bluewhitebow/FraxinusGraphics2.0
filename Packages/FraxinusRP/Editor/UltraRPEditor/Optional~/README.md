# Optional integrations

Shader Graph 14's basic UltraRP **Lit** and **Unlit** target sources are active
in `../Shaders/ShaderGraph`, together with their templates and includes. Their
material inspectors are active in `../Shaders/ShaderGUI`, and BaseShaderGUI
uses Shader Graph 14's exposed-property drawer. The editor assembly requires
the local `Fraxinus-ShaderGraph` fork's friend access. See
[ShaderGraph.md](../../../Documentation~/ShaderGraph.md) for setup, implemented
features, validation commands and current acceptance results.

The **Decal, Fullscreen, Canvas and Six-way Smoke** target sources remain here,
as does **VFX Graph** integration. Unity ignores directories ending in `~`;
these sources are not compiled or registered. Restoring them requires adapting
their version-specific APIs, material inspectors, includes and shader templates,
then testing their actual rendering paths. A shader ID or shared enum does not
enable an excluded target.

`AssetPostProcessors` retains the Arnold and 3ds Max physical-material import
processors that load the inherited Autodesk graphs. They are excluded from
compilation with their source GUIDs preserved. The nine inherited Unity 6 graph
assets remain separately in `Shaders/Optional~`. The new basic target does not
automatically restore those graphs or their importers.
