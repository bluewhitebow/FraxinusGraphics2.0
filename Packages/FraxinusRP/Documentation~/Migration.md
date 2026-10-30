# Unity 2022.3 migration

Source: `GameX/Packages/com.bluewhitebow.fraxinus-engine`, commit `bf25c32`.
Target: Unity 2022.3.62f3 and Fraxinus RPCore 14.0.99.

This package keeps
`Fraxinus.Runtime`, `Fraxinus.Editor` and the original node namespaces so saved
graphs retain their managed-reference and serialized type identities. UltraRP
editor and shader assemblies use independent names to coexist with URP.

Only Base/ScriptableNode and UltraRP modules and their rendering assets are
imported. Engine physics and unrelated modules are excluded. Core remains the
existing Unity 2022 Fraxinus fork.

`GuidRemapping.json` records the 365 imported GUIDs which collided with packages
already in this project. Each replacement is derived deterministically from
the new package ID and asset path. References in imported text assets, shader
GUID tables, and metadata were rewritten together. Imported source paths use
`Packages/com.bluewhitebow.fraxinus-rp`.

## Acceptance milestones

- [x] Package compiles alongside the existing URP packages.
- [x] ScriptableNode graph execution and asset round-trip pass.
- [x] UltraRP renders a scene from a saved renderer graph.
- [x] URP to UltraRP to URP switching preserves settings ownership.
- [x] Standalone Windows Mono Player builds and renders with runtime resources, to both RenderTexture and screen.
- [x] Baseline Deferred, shadows, transparency and HDR/Bloom are checked.
- [x] Compatibility boundaries for ShaderGraph, temporal upscaling and GI are recorded.

Unity 6's native GPU Resident Drawer has no Unity 2022 native backend. It must
stay disabled; normal renderer lists and the SRP Batcher are the base path.
UltraRP Lit/Unlit targets are adapted to Shader Graph 14.0.12 through the local
Fraxinus-ShaderGraph package; advanced targets and VFX/DOTS remain separate work.
See [ShaderGraph.md](ShaderGraph.md).

## Core and runtime integration

The existing Core fork supplies typed RenderGraph, APV and STP infrastructure.
It stays at version 14.0.99; no Unity 6 Core package is installed. This port adds
friend access in Core's Runtime, Editor and two PrivateShared `AssemblyInfo.cs`
files. `Runtime/RenderPipeline/RenderPipelineGraphicsSettingsCompatibility.cs`
now resolves resource paths against the package declaring each settings type,
and loads resource arrays using `ResourcePaths` / `ResourceFormattedPaths`.
The URP fork's `Runtime/UniversalRenderPipeline.cs` re-registers its settings
when a URP instance is constructed after another SRP.

UltraRP registers and serializes its own settings through that bridge. Shadow
culling uses the Unity 2022 culling/split-data APIs. Screen/MSAA, format queries,
debug textures, volumes and renderer submission use the 2022 implementations.
Graph assets can load while a different SRP is active: blit materials acquire
their pipeline resources when they are first rendered.

ScriptableNode's `ProcessGraphProcessor` executes all valid nodes in computed
dependency order, and clearing a graph also clears an existing processor's
execution list. `JobGraphProcessor` delegates to that processor synchronously
and exposes `usesSynchronousFallback`; it does not schedule parallel jobs.
Copied edges rebind their graph owner, and node removal clears incident edges
and lookup tables. Custom `ITypeAdapter` implementations must be concrete,
closed types with a public parameterless constructor. Discovery isolates bad
constructors and invalid conversions without partially registering an adapter. IL2CPP
port transfers avoid expression compilation. UnityLinker receives the package
preservation rules through an editor build processor; custom extensions need
their own preservation rules. See [IL2CPP.md](IL2CPP.md).

APV, PRT baking, full virtual texturing and native render-pass compilation
retain implementation code but require separate visual and platform validation.
Historical STP/TAA, object motion, camera stacking, MSAA and clustered lighting
results and graphics API scope are recorded in [Validation.md](Validation.md).
Compilation alone is not a claim that those complete
paths work. PRT now uses `Task` with main-thread yielding and cancellation in
place of the source project's UniTask dependency.

External assets from the Unity 6 project are not rewritten automatically.
Copy their dependencies and apply the entries in `GuidRemapping.json` to those
copies. Runtime node type identities are retained. Serialized UltraRP editor
type names from `Unity.RenderPipelines.Universal.Editor` resolve through a
namespace-restricted alias to `Fraxinus.UltraRP.Editor`.

## Generic context data

`ContextStateManager.Get<T>()` replaces `GetCurrentResourceData()`,
`GetCurrentCameraData()` and `GetCurrentRenderingData()`. `ContextInfo.data`
replaces its three fixed data fields. External context producers should cache a
`ContextData`, bind existing references with `Set<T>()`, and use `Get<T>()` to
read them. An unset type or an empty manager returns null.

Each ContextBegin node reuses separate main/sub containers. The sub scope has
its own ResourceData, CameraData and RenderingData. Render-pass consumers use
`frameData.Get<T>()` to read the active scope, falling back to frame data for
shared types. Core's ContextContainer owns this lookup; Begin/End only switch
its borrowed ScopeData binding. Contains/GetOrCreate use the same resolution,
and newly created items remain owned by the frame container. Begin/End record
camera/global-state changes in render order;
the End output publishes the child target after restoring its parent. See
[ScriptableNode.md](ScriptableNode.md) for connections and scope filters. `Clear()`
removes references without resetting borrowed objects; manager Reset clears
its stack, registry and current scope binding. Containers remain owned by their
nodes or callers. Rebinding a container updates all ContextInfo copies that
refer to it; these are not immutable snapshots.

ContextData inherits Core's ContextContainer.ScopeData. Both use the same cached
generic type IDs to index arrays without reflection or boxing on each access.
Frame disposal clears the scope binding before resetting only its owned items.
This follow-up requires Core commit `038ad6278a1f92195b654724a78d92865a6017d5`
containing ScopeData; the original 2026-10-21 checkpoint predates that container
API. Use the paired revisions in [Installation.md](Installation.md).
Creating a container and growing its storage allocate; warmed reads and reuse
with sufficient capacity do not. See [Validation.md](Validation.md) for measured
allocation counts and their scope.

## Editor compatibility matrix

Compilation establishes API compatibility only. The interactive and graphics
checks below remain separate acceptance work; this matrix does not mark a
rendering or Player milestone as passed.

| Area | Unity 2022 implementation | Remaining boundary or verification |
| --- | --- | --- |
| ScriptableNode / RendererGraph | Original serialized type identities are retained. GraphView uses the 2022 mouse handlers; RendererGraph Save writes graph data and saves the asset. | Automated validation covers graph edits, pointer-helper routing, Undo/Redo and actual domain reload. Manual UX checks remain useful for layout, mixed selection and platform-specific pointer behavior; execution results are recorded separately. |
| Camera, light and pipeline inspectors | Camera/light selection uses `CustomEditorForRenderPipeline`. Rendering-layer masks use the existing Core editor helper. Settings lookups use the Core compatibility registry. | Inspector selection while switching pipelines, mixed multi-object editing, camera stacks, and Undo/Redo have not been interactively verified. |
| Hand-written material inspectors | Lit, Unlit, Simple Lit, Baked Lit, particle Lit/Simple Lit/Unlit, and Terrain Lit inspector classes use the matching Shader Graph 14 property drawer where applicable. Render-queue overrides use the 2022 serialized `m_CustomRenderQueue` field. | Lit/Unlit and clustered Simple Lit have rendered fixtures. Interactive inspector controls, queue override persistence, multi-object edits and other material families remain unverified. Terrain uses the 2022 rule that diffuse alpha supplies smoothness when present; Unity 6's selectable terrain smoothness source is unavailable. |
| ShaderGraph | Lit/Unlit targets, inspectors, material refresh and supporting templates are adapted to Shader Graph 14.0.12. The local SG fork only grants friend access to `Fraxinus.UltraRP.Editor`. | Forward and Deferred forward-only fallback, depth/normals (including packed Deferred normals), shadows, selection/picking and LOD variants are implemented. Meta, MotionVectors and rendering-layer MRT output are implemented. GBuffer, custom interpolators and the advanced targets remain outside this target implementation. Custom Forward draw lists must include `UltraForwardOnly`. The nine inherited advanced graphs remain optional. See [ShaderGraph.md](ShaderGraph.md). |
| VFX Graph / DOTS | VFX editor registration is retained in `Optional~`; active editor assemblies depend on Shader Graph 14 but have no VFX or Entities dependency. | No VFX output or DOTS integration is supported or validated. Restoring these requires version-specific API, assembly, shader-template and include-path adaptation. |
| GPU Resident Drawer (GRD) | The 2022 inspector displays Disabled and explains the native-backend limitation. The Core 2022 compatibility implementation provides no native GRD backend. | GRD and GPU occlusion acceleration are unavailable. Standard renderer submission and SRP Batcher behavior must be validated through the normal rendering acceptance checks. |
| XR | Non-XR compilation is the current baseline. UltraRP XR branches require `FRAXINUSRP_ENABLE_XR` as well as Unity XR symbols. | XR has not been ported or device-tested. Enabling the symbol alone is not a supported configuration; stereo rendering, motion vectors, mirror view and XR post processing need a separate adaptation pass. |
| Decal / PRT editor tools | Decal gizmo LOD uses a SceneView/HandleUtility approximation on 2022. PRT baking uses `Task` and linked cancellation/timeout handling in place of UniTask. | Decal gizmo interaction and PRT bake, timeout, cancellation and GPU-resource behavior have not been manually verified. No compatible default Decal material is supplied while the Decal Shader Graph target is unavailable. |

The Shader Graph property drawer is active in `BaseShaderGUI`. Restoring the
remaining `Optional~` targets still requires adapting their version-specific
APIs, templates and resource references; moving the sources alone is insufficient. See `Editor/UltraRPEditor/Optional~/README.md` for that integration
boundary.

The Arnold and 3ds Max physical-material import processors that directly load
the unsupported Autodesk graphs are also retained under
`Editor/UltraRPEditor/Optional~/AssetPostProcessors`. Their source files and
`.meta` files are unchanged. Before isolation, the nine graph GUIDs were checked
against active package, project asset and project-setting text: no material or
serialized resource depended on them. Terrain detail, grass and billboard grass
defaults now load the package's hand-written shaders. SpeedTree 7 and 8 defaults
also use the hand-written shaders, and the SpeedTree 8 name/GUID table points to
that active asset. Their GBuffer pass tags match the renderer's draw list.
Editor default-shader lookup uses UltraRP's registered global settings even when
another pipeline is active, without creating or registering settings in a getter.
SpeedTree 9 remains unavailable: its compatibility getter returns null and its
Unity 6 graph stays optional. These defaults do not restore the excluded advanced
Shader Graph targets or add a Unity 2022 SpeedTree 9 importer.

## Editor and resource changes

The active UltraRP editor assembly is `Fraxinus.UltraRP.Editor`. It retains the
original editor namespaces while avoiding the existing URP assembly name.
ShaderGraph detection now reads the shader tag, and metadata lookup reads the
shader's metadata subassets. Target generation and graph inspector integration
use the friend access declared by the local Shader Graph fork.
Editor analytics and asset reimport hooks use the Unity 2022 APIs.

Visible material emission follows Unity 2022's `EmissiveIsBlack` flag semantics:
GI set to None can still enable `_EMISSION`. Merely setting `_EmissionColor`
does not enable a material whose emission checkbox is still disabled.

HDR render targets preserve emissive values above one before Bloom thresholding.
The final Uber pass declares its Bloom texture dependency
and binds per-camera Bloom parameters immediately before drawing; mip-count
one uses the initialized downsample texture.

Historical Bloom A/B captures checked emission, HDR input and a localized
brightness increase outside emissive geometry; see [Validation.md](Validation.md).

Global-settings postprocessing only registers UltraRP when an UltraRP asset is
active. The Player preprocessing hook asks the global settings to prepare
runtime graphics resources before building. Resource validation checks the
declared package path of every postprocess shader and texture in both graphics
settings and the serialized `PostProcessData` asset. The serialized asset's 16
shader and 44 texture references now match their target `.meta` GUIDs; blue
noise and film-grain arrays follow the declared order. Five default materials
(Lit, particle Unlit, Terrain Lit, sprite Unlit and sprite mask) reference this
package's hand-written shaders.

The first milestone's serialized-reference audit is recorded in the project at
`Logs/FraxinusRP/AssetReferenceAudit.json`. On 2026-10-19 it checked 94 reference
occurrences across 33 active asset/importer files: 69 package-internal, 24 Unity
built-ins, and one Core `VolumeProfile` script. It found no unresolved references, source-project-only
references, unintended URP package references, or duplicate active package
GUIDs. The audit excludes `~` directories, C# GUID strings, shader include paths,
and graph JSON contents. The nine optional graph assets and two optional import
processors are explicitly listed as excluded in the report; they are not active
ShaderGraph importer dependencies. The current audit, including generated
Shader Graph and advanced-scene fixtures, is `Phase2AssetReferenceAudit.json`;
its results and the current include/GUID audits are recorded in
[Validation.md](Validation.md). Reference audits do not establish shader
compilation or visual correctness.

## Optional postprocess variant stripping

`UltraRenderPipelineShaderStrippingSetting.stripUnusedPostProcessingVariants`
remains false by default. Enabling it opts a Player build into the following
contract: every UltraRP effect created or activated at runtime must also appear
as a component in an imported `VolumeProfile` asset. A declaration profile may
be unused by scenes. Leave the setting disabled when runtime-only effects
cannot be declared this way.

The adapter uses Core's `UnityEditor.Rendering.IShaderVariantStripper`. Its
postprocess rules run only while building an UltraRP Player with this setting
enabled. They union component types across all discoverable profiles, including package
profiles, independently of scene usage, component activity and current
intensity. This preserves effects that are temporarily disabled or enabled by
scripts later. When no profiles are found, a profile cannot be loaded, or a
component script is missing, it keeps all postprocess variants and reports why.

Only shaders with both the exact UltraRP shader name and the matching path in
this package are eligible. In UberPost, enabled keywords for undeclared lens
distortion, chromatic aberration, Bloom, tonemapping and film grain can be
removed. Entire Bloom, Gaussian/Bokeh depth-of-field, and camera-motion-blur
shaders can be removed when their corresponding components are absent.
Panini and shaders outside this whitelist are retained. A declared Bloom keeps
all four quality/dirt modes, and a declared depth of field keeps both modes.

Effects-off variants, internal HDR grading (`_HDR_GRADING`), alpha output,
sRGB conversion and dithering remain available. The same opt-in adapter also
honors explicit build settings for its shader whitelist, which includes
FinalPost and ScalingSetup:

- `stripScreenCoordOverrideVariants=true` removes `SCREEN_COORD_OVERRIDE`.
  Leave this setting false when Player cameras need screen-coordinate overrides.
- Core `ShaderStrippingSetting.stripRuntimeDebugShaders=true` removes
  `DEBUG_DISPLAY`. Setting it false preserves runtime debug variants. Missing
  Core or UltraRP settings conservatively preserve their corresponding variants;
  this adapter does not infer the debug policy from `Debug.isDebugBuild`.
- `PlayerSettings.allowHDRDisplaySupport=false` removes the HDR display-input,
  encoding and color-space-conversion keywords, including the HDR-specific
  scaling variants. It does not remove internal HDR grading or ordinary SDR
  scaling. Enabling HDR display support retains these output variants.

The screen-coordinate and debug keywords use global keyword lookup; HDR output
keywords use the owning shader's local keyword space. These rules only inspect
fragment variants of shaders with the exact package path and shader name.
They do not inspect the current camera, effect value or currently connected
display to predict future runtime behavior. The build log records the collected
effect union, these build settings and each shader's removed variant count.
Apart from the narrow deferred rule below, general UltraRP feature stripping
requires additional feature-specific rules beyond the limited prefilter collector below.

Historical stripping checks covered profile unions, inactive components,
effect-mode retention, effects-off/internal-HDR variants, all eight combinations
of the three build settings and shader ownership; see [Validation.md](Validation.md).

## Shader keyword prefiltering

The asset filter now matches the actual `RenderPipeline=UltraPipeline` shader
tag. Previously its `UltraRenderPipeline` tag matched no hand-written shader,
so the serialized default filter values were never applied. Build preparation
now initializes every derived field before native shader enumeration. With
`stripUnusedVariants` disabled, these fields retain both OFF and ON states.
The former direct capability attributes have moved to derived fields so that
they also obey this switch. Platform-specific unsupported-keyword rules remain.

With the switch enabled, disabled asset capabilities remove their corresponding
keywords while retaining OFF. Enabled shadows and lights also retain OFF;
camera visibility and quality changes do not require a new build. Explicit Core
debug stripping, UltraRP screen-coordinate stripping and Player HDR display
support control those keyword groups. Internal HDR rendering and HDR grading
are independent of HDR display output. Reflection-probe filtering only removes
disabled features; it does not force the ON variant. Soft-shadow quality modes
remain together when soft shadows are supported.

Only graphs accepted by the conservative built-in graph proof permit removal
of unused Forward+, SSAO and DBuffer keywords. The stricter forward-only proof
also permits removal of Deferred keywords; reviewed Deferred graphs retain them. Unknown, derived,
missing or custom graphs retain graph-controlled variants. Native render-pass
and legacy-lightmap states are retained. Asset capabilities that can be enabled
at runtime must be declared in included pipeline assets, or unused-variant
stripping must remain disabled. This is a limited collector, not a replacement
for every Unity 6 feature-specific stripping rule.

`UltraRPShaderPrefiltering` prepares fields through the normal Player preprocess
callback. The validation Player also prepares and checks them before calling
`BuildPipeline.BuildPlayer`. Its rule checks cover disabled-light OFF variants,
opt-out retention, unknown graph retention, output settings, and field-level
recovery. AssetBundle prefilter collection is not part of this Player workflow.

Preparation writes `Library/FraxinusRP/ShaderPrefilteringRecovery.json` before
changing assets. Build completion, validation cleanup and a subsequent Editor
process restore the original in-memory fields and dirty state. If Unity saved
an asset during the build, recovery restores only its original prefilter YAML
lines, preserving other fields, line endings and the UTF-8 BOM. Package assets
resolve through `PackageInfo.resolvedPath`. Pipeline assets must use text
serialization, be main assets, and contain one serialized object per file for
this recovery; unsupported files fail before mutation. Recovery requires the
original asset GUID and refuses to write a replacement asset at the old path.
Every preparation restores any previous journal and recomputes the fields,
including synchronous build retries before the next Editor update. An assembly
reload also recovers pending fields once the build is no longer running.

## Deferred shader stripping

`UltraRenderPipelineShaderStrippingSetting.stripUnusedVariants` controls narrow
rules for the package-owned `Hidden/Ultra Render Pipeline/StencilDeferred` shader
and `Shaders/Lighting/DeferredLit.compute`. The compute rule also requires the
exact asset name `DeferredLit` and uses Core's compute-stripper interface.
Only an UltraRP Player build can use this rule. With the setting enabled, the
analysis combines every pipeline asset reported by CoreBuildData and every
entry in its `rendererGraphList`, including non-default renderers.

| Declared rendering paths | StencilDeferred | DeferredLit compute |
| --- | --- | --- |
| Forward only | Remove | Remove |
| Stencil (optionally with Forward) | Retain | Remove |
| Clustered (optionally with Forward) | Remove | Retain |
| Both deferred modes, or uncertain usage | Retain | Retain |

The proof accepts only the exact `UltraRendererGraph` type and a reviewed set of
built-in node types. The actual `DeferredObjectPass.cluster` field declares its
lighting mode; multiple nodes can declare both modes in one graph. A GBuffer
node without a known lighting consumer, custom or unknown graph/node type, null
entry, empty graph, missing managed reference, custom blit material or custom
post-pass extension retains both shaders.
DrawObjectsPass material/shader overrides, deferred/GBuffer shader tags, and
unknown Uber effect components or processors also retain them. Ordinary custom
forward shader tags remain allowed.
Disabled and disconnected nodes are still inspected. The allowlist must be
reviewed when new built-in node types are introduced. TextureBlitNode's serialized
user material override is checked; its runtime default CoreBlit material cache
does not prevent this proof.

Declare all graphs that Player code may select in the included pipeline assets.
Disable `stripUnusedVariants` when scripts create graphs, switch to undeclared
assets/graphs, or change `DeferredObjectPass.cluster` to a mode not represented
by the declarations. The build cannot infer those runtime uses.

This rule is independent of the postprocess opt-in and VolumeProfile discovery.
Turning off postprocess stripping still preserves all postprocess variants.
Shaders with another name or package path are unaffected. Other UltraRP feature
stripping and general prefilter-data collection remain outside this rule.

Historical deferred-stripping checks covered Forward, Stencil and Clustered
assets, unions, non-default graphs, disabled/disconnected nodes, both modes in
one graph, unknown/derived types and shader ownership boundaries. The temporary
validation entry points have been removed; production build analysis remains.

DeferredLights resolves the DeferredLit kernels only on the first actual
clustered-deferred dispatch, and creates its owned Stencil material/pass lookup
only when Stencil executes. Forward and Clustered construction therefore do not
instantiate a stripped Stencil shader. Each draw validates its requested pass;
unused passes such as Fog can legitimately be stripped. Missing resources or
requested passes produce an explicit error. The original Deferred fixture uses stencil lighting. A separate Clustered
fixture exercises compute lighting, main/additional shadows and Simple Lit;
see [Validation.md](Validation.md) for actual acceptance results. Clustered
passes declare depth, shadow and GPU-buffer dependencies, dispatch every
lighting model, guard edge tiles and reset per-camera shadow keywords.

Overlay cameras currently force Forward before GBuffer/Deferred nodes execute.
The GBuffer overlay stencil-clear branch is therefore not used by the reviewed
built-in graphs. This stripping proof does not add Deferred/Clustered overlay
support; custom graph/node paths retain both shaders conservatively.

Clustered cookies share Forward's light-type-specific projection and atlas
sampling. CPU light buffers derive the additional-light index from the current
visible-light order, excluding the main-light slot; the cookie manager's CPU
mapping is populated later during RenderGraph execution and must not be read
while recording the graph. GPU enable bits handle cookies rejected by the atlas.
Main and additional directional cookies are sampled as well, with explicit mip 0
for main-cookie sampling in compute. See the lighting validator's scope in
[Validation.md](Validation.md); this does not establish every lighting model.

## Ordinary Player shader diagnostics

The UltraRP build processor collects retained compiler inputs and shaders listed
in the packed-asset report, including cached shaders and Shader Graph subassets.
It checks Shader/ComputeShader compiler errors and Shader Graph importer errors,
and writes `Library/FraxinusRP/ShaderBuildDiagnostics.json`. Errors raise a
`BuildFailedException`; the settings/prefilter cleanup still runs. This applies
to ordinary Player builds.

Only included asset paths are checked; unrelated erroneous project shaders are
not scanned. Compiler messages are filtered by the build's configured graphics
APIs. Platform-independent and unrecognized native platform messages are kept.
Shader Graph copies of compiler errors use that same platform filtering, while
graph-generation errors and the error fallback shader fail validation.
Diagnostics have asset/API granularity: Unity's stored messages do not identify
the complete variant that produced an error, so the check conservatively retains
errors on included assets even if that particular variant may have been stripped.
This is an error check, not an exhaustive compilation of every possible keyword
combination, an AssetBundle validator or coverage of every graphics backend.

## Player-specific compatibility

The first Shader Graph IL2CPP build exposed an existing integration issue in
`Fraxinus-URPExtensions/Plugins/source`: twelve C/C++ source/header importers
selected every platform, although the retained plugin has no D3D11 backend
and the Windows rendering path does not use it. Unity 2022 copied those files
into a flat IL2CPP directory, breaking the `Unity/IUnityGraphics.h` include.
The companion fix excludes only Windows x86/x64 in these twelve importers.
It preserves all GUIDs, native source, other platform settings and Apple binary
importers. This is a Windows build-input correction in the existing URP fork;
it does not add native-plugin support to UltraRP. The subsequent IL2CPP build
and Player runs establish acceptance separately.

Unity 2022 uses `CameraTarget` for the screen depth attachment; the separate
Unity 6 `BuiltinRenderTextureType.Depth` target is unavailable there. Explicit
RenderTexture cameras use their own color/depth attachments.

The virtual-texturing feedback compute uses Core's standard 2D texture macros
and an independent utility include guard. This avoids the inherited collision
with UnityInput and implicit XR array declarations. Compiling its two kernels
does not establish complete virtual-texturing or XR support.

## Material ownership

`MaterialLibrary` records shader resources during initialization and creates a
material only on its first request. Reload and cleanup destroy only materials
owned by the library. Unused effects stripped from a Player no longer cause
eager unsupported-shader warnings; requesting an unavailable effect reports one
explicit error per resource. Historical resource checks covered allocation,
caching, reload and cleanup against actual Unity material objects.

## Temporal, flare and depth targets

STP is selected through the pipeline asset's upscaling filter and requires a
supported compute device, postprocessing and temporal AA. The temporal resolve
runs once even if the renderer graph's ordinary TAA entry is disabled. TAA/STP
jitter and history belong to the main camera; subcontexts do not reuse that
history. Opaque object motion uses the renderer graph's opaque layer mask and
the material's MotionVectors pass. Newly generated UltraRP Shader Graph Lit and
Unlit shaders provide that pass, including time-based evaluation and custom vertex displacement. Transparent object motion remains outside this path.

A build that uses STP must declare at least one pipeline asset configured for
STP in its Graphics/Quality pipeline set (or Core additional-pipeline assets). Core's
STP resource stripper uses that declaration to retain the compute resources;
changing only the filter at runtime, or keeping an unreferenced STP asset in
the project, is insufficient when no declared pipeline asset enables STP. Runtime Volume profiles also follow the existing postprocess
resource declaration rules below.

Enable the LensFlare entry on the PostProcess node, the desired pipeline flare
support flag, and camera postprocessing. Data-driven flares use LensFlareDataSRP
and scene LensFlareComponentSRP components, with camera depth for occlusion.
Screen-space flares use the ScreenSpaceLensFlare Volume override; their bright
source uses Bloom threshold/filter settings but works with Bloom intensity zero
or the Bloom entry disabled. Their composition preserves destination alpha.
A declared ScreenSpaceLensFlare profile retains the Bloom filter shader during
postprocess stripping. The current camera integration is mono. Use the paired RPCore fixes as well:
flare projection must treat local positions with homogeneous W=1, orthographic
occlusion must compare eye-space depth, and integer shader constants must use
SetGlobalInteger on Unity 2022. Those corrections live in Core's
`LensFlareCommonSRP.cs` and `LensFlareCommon.hlsl`.

A target RenderTexture with no color format and a valid depth/stencil format
uses a dedicated depth-only branch. It renders opaque and transparent queues
with their materials' clipping and ZWrite states, camera culling and graph layer
masks. It uses the target viewport directly, independently of renderScale, and
preserves depth for CameraClearFlags.Nothing. It bypasses color attachments,
lighting, temporal history and postprocessing. Custom color graph nodes are not
executed for this target type.
