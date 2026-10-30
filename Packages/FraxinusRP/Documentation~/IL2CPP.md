# IL2CPP and managed stripping

ScriptableNode discovers nodes, port fields, custom port callbacks and type adapters through reflection. `Runtime/link.xml` preserves the package's `Fraxinus.Runtime` assembly. On Unity 2022.3, `ScriptableNodeLinkerProcessor` explicitly passes this file to UnityLinker through `IUnityLinkerProcessor.GenerateAdditionalLinkXmlFile`; package-local `link.xml` files are not discovered automatically. This does not create a persistent file under the project's `Assets` folder.

The package preservation file covers only `Fraxinus.Runtime`. Custom nodes, parameters, type adapters and callbacks in a project's own assemblies need their own preservation rules. Place a `link.xml` under the project's `Assets` folder and preserve the relevant types and reflected members, or apply appropriate `UnityEngine.Scripting.Preserve` attributes. A type-level `[Preserve]` alone preserves its default constructor, not every field and callback.

For example, a project can preserve specific extension types as follows, using their actual assembly and full type names:

```xml
<linker>
  <assembly fullname="MyGame.Graphs">
    <type fullname="MyGame.Graphs.MyNode" preserve="all" />
    <type fullname="MyGame.Graphs.MyTypeAdapter" preserve="all" />
  </assembly>
</linker>
```

## Historical IL2CPP acceptance

On 2026-10-19, the migration acceptance built and ran a
Windows x64 Development Player with **IL2CPP**, **High** managed stripping and
**Release** C++ compilation in Unity 2022.3.62f3. The official matching Windows
IL2CPP support module and Visual Studio 2022 C++ tools are required.

`Build-ShaderGraph-IL2CPP-02.log` and its `.exitcode` record a successful real
build. `ShaderGraph-IL2CPP-Player-RenderTexture.log` and
`ShaderGraph-IL2CPP-Player-Backbuffer.log` each record exit 0, node-validation
success and actual Lit/Unlit Shader Graph pixel success. The Player runs three
preserved custom nodes in Assembly-CSharp, sends an integer through a numeric
conversion to a float port, copies the graph through JsonUtility, checks that
copied edges belong to the copy, and executes that copy through the synchronous
JobGraphProcessor fallback. Expected output is 20. Both capture modes also
validate the active pipeline and reject rendering errors.

These fixtures establish the tested external node/field/process preservation
pattern. They do not automatically preserve arbitrary project types, custom
port callbacks or project-specific adapters. Test those under the intended
stripping level with their own declarations.

The first build exposed unrelated URP native-plugin source importers enabled on
Windows. IL2CPP flattened their headers and failed on `Unity/IUnityGraphics.h`.
The companion URP package now excludes those 12 source/header importers from
Windows targets, retaining their GUIDs and other platform settings. The second
build confirms `AdditionalCppFiles` is empty. This is a Windows import-scope
correction; Apple native-plugin builds remain unverified.

The fixture's platform acceptance is Windows/D3D11; other IL2CPP targets need
their own device runs. The temporary acceptance scripts have been removed;
see [Validation.md](Validation.md) for the historical evidence and source versions.

Unity documentation: [Managed code stripping in Unity 2022.3](https://docs.unity3d.com/2022.3/Documentation/Manual/ManagedCodeStripping.html).
