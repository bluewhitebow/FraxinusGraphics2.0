# Shader Graph 14

FraxinusRP registers an independent **UltraRP** target for Shader Graph 14.0.12.
Its Lit and Unlit subtargets live in `Editor/UltraRPEditor/Shaders/ShaderGraph`;
their material inspectors live in `Shaders/ShaderGUI`. They use UltraRP shader
includes and `UltraForwardOnly` passes, with the `UltraPipeline` pipeline tag.
There is no dependency on the URP editor assembly.

## Required packages

Use the local `Fraxinus-ShaderGraph` package alongside FraxinusRP and the Unity
2022 Fraxinus RPCore fork. The project manifest selects Shader Graph with:

```json
"com.unity.shadergraph": "file:Fraxinus-ShaderGraph"
```

This is an exact copy of Shader Graph 14.0.12 with one integration change:
`Editor/AssemblyInfo.cs` grants friend access to `Fraxinus.UltraRP.Editor`.
Shader Graph's custom target API is internal. The registry package does not
grant this access and cannot replace this local fork without the same change.
The package identifier, version and asset GUIDs are preserved. See the fork's
`FraxinusChanges.md` for provenance. No Shader Graph 17 or Unity 6 Core package
is required.

## Creating graphs

Select an UltraRP pipeline asset for the project or active Quality level, then
use **Assets > Create > Shader Graph > UltraRP > Lit Shader Graph** or
**Unlit Shader Graph**. Existing graphs can select UltraRP in Graph Settings.
Expose ordinary Shader Graph properties to edit them in the material inspector.

Graph Settings control surface type, blending, alpha clipping, face culling,
depth testing/writing and shadow casting. Lit also exposes metallic/specular
workflow, normal space and receiving shadows. **Allow Material Override** moves
the applicable surface controls into the material inspector. Materials retain
automatic/user render-queue selection. Saving changed graph surface settings
refreshes the matching loaded materials through Shader Graph's save callback.

## Deferred rendering and existing renderer graphs

Lit and Unlit graphs use forward shading in both Forward and Deferred renderers.
Their color pass has `LightMode=UltraForwardOnly`; they do not write a GBuffer.
In a Deferred graph, connect the passes in this order:

`GBuffer -> Deferred lighting -> opaque DrawObjectsPass -> transparent DrawObjectsPass`

Configure the opaque fallback to select `UltraForwardOnly`. Forward renderer
draws and transparent draws should select both `UltraForward` and
`UltraForwardOnly`, as the supplied samples already do. Do not add
`UltraForward` to the Deferred opaque fallback: ordinary GBuffer materials also
have that pass and would be shaded again.

Existing custom renderer graphs that selected only `UltraForward` must add
`UltraForwardOnly` to their Forward/transparent draw lists. Existing Shader
Graph assets and materials can be reimported in place; their properties, GUIDs
and pass names are unchanged. Only pass selection tags change.

The graph depth/normal pass uses `DepthNormalsOnly` so that Deferred's partial
prepass includes it. It supports both world-space normals and the packed
octahedral normals used by the Deferred target. Alpha clipping and graph
vertex/normal inputs apply to that pass as well as the color pass.

## Implementation boundaries

| Area | Current implementation | Separate acceptance or missing support |
| --- | --- | --- |
| Lit and Unlit | Graph generation, exposed properties and graph-specific material inspectors. | Interactive graph editing, previews, multi-object inspector edits and Undo/Redo require manual checks. |
| Forward surfaces | Opaque/transparent states, alpha clipping, metallic/specular Lit, graph normal inputs, emission, static/dynamic lightmap sampling and fog. | Graph-based visual fixtures and Player results must be reported separately from C# compilation. Lighting keywords do not establish every lighting configuration. |
| Supporting passes | DepthOnly, DepthNormals (LightMode `DepthNormalsOnly`), ShadowCaster, SceneSelection, Picking, Meta and MotionVectors. Shadow positions use graph vertex outputs and the pipeline's shadow bias/clamping. | Forward and DepthNormals support rendering-layer MRT output. Punctual shadow image quality requires separate acceptance. |
| LOD cross-fade | Generates cross-fade variants for forward, depth, normals and shadow passes, using the pipeline dithering texture. | Variant compilation does not establish transitions in an animated LODGroup. |
| Deferred | Lit/Unlit color rendering uses the forward-only fallback after Stencil or Clustered lighting; graph normals participate in the partial prepass. | No SG GBuffer pass is generated. SG Lit uses forward lighting and its light limits, rather than the Clustered lighting kernel. |
| Baking / temporal paths | Meta exports Lit albedo/emission and Unlit base color in lightmap UV space. MotionVectors supports previous object/camera transforms, skin positions and None/TimeBased/Custom vertex motion modes. | The auxiliary fixture checks exported pixels and motion; a complete lightmapper bake and arbitrary graph deformation require separate acceptance. |
| Custom interpolators | Disabled by the Shader Graph target's default capability. | Descriptors retained in source do not enable the feature. |
| Advanced targets | Decal, Fullscreen, Canvas and Six-way Smoke remain in `Editor/UltraRPEditor/Optional~`. | They are not registered or supported. Their APIs, includes, material inspectors and rendering paths require separate adaptation. |
| VFX / DOTS / XR | VFX target support is disabled. This port's accepted baseline is ordinary non-XR rendering. | No VFX output, Entities integration or stereo acceptance is established. |

The nine inherited Unity 6 graph assets remain under `Shaders/Optional~`.
Some target URP or depend on advanced integration; they are not automatically
converted, imported or replaced by the new UltraRP graphs. External Engine
graphs and their dependencies require a separate copied-asset migration and
GUID review.

## Historical validation

The migration checkpoints exercised graph import, generated passes, Forward
rendering and Deferred fallback, plus Windows IL2CPP with High managed stripping.
The temporary validators and their sample-generation menus have been removed.
Use the standard graph creation menus above; historical results and source-version
limits are recorded in [Validation.md](Validation.md).

See [IL2CPP.md](IL2CPP.md) for custom-node preservation and the Windows native
plugin importer correction required by this host project's URP package.

## Auxiliary passes

Select the vertex motion mode in Graph Settings. `None` still includes rigid
object and camera motion; `TimeBased` evaluates the vertex graph using the
previous time, and `Custom` uses the Motion Vector vertex block: supply the object-space
displacement from the previous frame to the current frame. The pass subtracts
that displacement from the current vertex position. Motion
vectors use non-jittered current/previous camera matrices and preserve alpha
clipping and LOD cross-fade. Front-face inputs and negative-scale tangent signs
are supported in the ordinary surface passes.

Historical auxiliary-pass results and platform limits are recorded in
[Validation.md](Validation.md).
