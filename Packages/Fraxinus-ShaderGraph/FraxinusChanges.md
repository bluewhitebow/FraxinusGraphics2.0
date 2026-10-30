# Fraxinus Shader Graph 14 fork

Source: Unity `com.unity.shadergraph@14.0.12` from this project's resolved PackageCache, copied 2026-10-19. Package identifier, version, license and asset GUIDs are unchanged.

`Editor/AssemblyInfo.cs` grants `Fraxinus.UltraRP.Editor` friend access to Shader Graph's internal custom-target API. No Shader Graph behavior is changed. FraxinusRP uses its own target and editor assembly; it does not depend on URP.
