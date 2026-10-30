# FraxinusGraphics 2.0

[English](README.md) | [简体中文](README.zh-CN.md)

**A rendering suite for Unity 2022.3 that makes the rendering workflow an editable graph.**

FraxinusGraphics 2.0 centers on **ScriptableNode / UltraRP**, combining render-node authoring, typed RenderGraph execution, material tools and probe lighting in a coordinated source distribution. Connect drawing, shadows, post-processing and intermediate textures in a Renderer Graph, then extend the workflow with custom nodes and shaders.

UltraRP is an independent Scriptable Render Pipeline ported from the Fraxinus Engine Unity 6 branch to **Unity 2022.3.62f3**. This repository contains **four companion packages: FraxinusRP, Fraxinus Core, Shader Graph and Light Transport**. Installing and running UltraRP does not require URP.

[Installation and usage](docs/Usage.md) · [Changes from 1.x](docs/ChangesFrom1.x.md) · [Local dependencies](docs/manifest.local.json) · [Git dependencies](docs/manifest.git.json)

## Core features

### Editable rendering workflows

**Renderer Graph** expresses the rendering workflow through nodes and resource connections, with Forward, Forward+, Stencil Deferred and Clustered Deferred paths. The graph editor supports saving, copy/paste, Undo/Redo and window reloads. Custom nodes and pass injection let projects extend their rendering logic.

**Nested render contexts** provide separate color, depth and mutable frame data for the same camera view, including intermediate targets at different resolutions. Explicitly composite child results back into the parent to organize rendering stages and effects. Child contexts reuse the camera view and culling results and have no independent TAA / STP history.

Renderer Graph organizes rendering nodes; the underlying **RenderGraph** manages passes and resource dependencies. Together they connect visual authoring to rendering execution. [Graph workflow](Packages/FraxinusRP/Documentation~/ScriptableNode.md)

### Materials and realtime lighting

**Hybrid Lit** extends the Lit workflow with rough diffuse, GGX, micro-shadowing, FGD environment reflections and an AO multibounce approximation, with Clear Coat support. **LTC rectangle lights** provide realtime area lighting with size, diffuse/specular intensity and Rendering Layer controls. The current limit is 16 unshadowed rectangle lights per camera.

Volume effects include **GTAO**, main-directional-light **Contact Shadows** and histogram-based **Auto Exposure**, adding occlusion, nearby contact detail and brightness adaptation. See [Hybrid lighting](Packages/FraxinusRP/Documentation~/HybridLighting.md), [GTAO](Packages/FraxinusRP/Documentation~/AmbientOcclusion.md) and [contact shadows / exposure](Packages/FraxinusRP/Documentation~/ContactShadowsAndExposure.md) for material and rendering-path support.

### Probe lighting and precomputed relighting

**APV** provides adaptive probe placement, standalone baking, sky occlusion and probe-data streaming. **APRTV** uses precomputed radiance-transfer data to respond to directional-light and HDR sky changes at runtime, with lighting-scenario blending. UltraRP integrates the corresponding backends, material sampling and relighting scheduling.

Core's probe baker uses Light Transport's **Compute ray queries**, without requiring DXR. APRTV operates on precomputed static scenes: geometry and material changes require rebaking, and dynamic occlusion and runtime multibounce feedback are outside its current scope. APRT data loads in bulk for loaded scenes. [APV baking](Packages/Fraxinus-RPCore/Documentation~/StandaloneAPVBaking.md) · [APRTV](Packages/Fraxinus-RPCore/Documentation~/APRT.md)

### Shader Graph and streaming virtual textures

FraxinusRP provides **UltraRP Lit / Unlit Shader Graph targets** using internal APIs exposed by the companion Shader Graph fork, with transparency, alpha clipping and auxiliary passes for depth, normals, shadows, Meta and motion vectors. Projects can use visual material authoring alongside handwritten shaders.

**Streaming Virtual Texturing (SVT)** integrates Unity's native Texture Stacks, tile feedback and cache backend, with configurable cache budgets and coarse-mip preloading. Enable project VT and author compatible materials explicitly; ordinary Texture2D samples do not become virtual automatically. [Shader Graph](Packages/FraxinusRP/Documentation~/ShaderGraph.md) · [SVT](Packages/FraxinusRP/Documentation~/VirtualTexturing.md)

### Upscaling and temporal processing

**STP, SGSR1 spatial upscaling and SGSR2 temporal upscaling** work with Render Scale to adjust rendering resolution. Temporal paths integrate motion vectors, camera history and history resets, with fallbacks for unsupported configurations. Select a path for the intended image quality and target hardware. [Upscaling configuration](Packages/FraxinusRP/Documentation~/Upscaling.md)

### Companion rendering infrastructure

Fraxinus Core provides **Raster / Compute / Unsafe RenderGraph passes, RTHandle, Volume, Blitter and Rendering Debugger**, adapted to Unity 2022 execution APIs. Light Transport supplies Compute-based BVH construction and traversal, hit/occlusion queries and sampling tools for probe baking and custom computations. [Core](Packages/Fraxinus-RPCore/README.md) · [Light Transport](Packages/Fraxinus-Light-Transport/README.md)

## How does it differ from other pipelines?

The focus is on **combining editable rendering workflows, companion lighting features and modifiable rendering infrastructure on Unity 2022.3**. This comparison describes workflows and integration; there is no common benchmark establishing an image-quality or performance ranking.

| Compared with | Main distinction | Adoption considerations |
| --- | --- | --- |
| Built-in Render Pipeline | UltraRP organizes rendering through an independent SRP, Renderer Graph and explicit resource dependencies, with its own materials and lighting paths. | Configure the pipeline asset, graph and matching materials; existing Built-in shaders need migration. |
| Official URP 14 (Unity 2022.3) | URP is commonly extended through Renderers and Renderer Features. UltraRP exposes the rendering workflow as an editable node graph, with nested contexts, Hybrid Lit, LTC area lights and companion probe workflows. | Projects maintain graph connections and custom code; URP Renderer Features and materials need adaptation for UltraRP. |
| Official HDRP | UltraRP focuses on composing features within an editable rendering workflow, including selected lighting implementations derived from HDRP algorithms and its own Core / Shader Graph integration. | Use the documented feature scope; this does not establish full HDRP coverage or HDRP material compatibility. |
| Unity 6 SRP workflows | Selected newer rendering infrastructure and Fraxinus Engine workflows are brought to Unity 2022.3 for projects that still need that engine version. | Backports remain limited by engine APIs; Unity 6's native GPU Resident Drawer is unavailable on Unity 2022. |

The source is intended for rendering research, project-specific effects and developers who want a modifiable node-based SRP on Unity 2022.3. When adopting it in an existing project, assess material migration, target-platform behavior and frame budgets with representative scenes.

## The four packages

| Package | UPM identifier | Version | Role |
| --- | --- | --- | --- |
| [FraxinusRP](Packages/FraxinusRP/README.md) | `com.bluewhitebow.fraxinus-rp` | `0.1.0` | ScriptableNode, UltraRP, render nodes and material / lighting integration. |
| [Fraxinus-RPCore](Packages/Fraxinus-RPCore/README.md) | `com.unity.render-pipelines.core` | `14.0.99` | RenderGraph, shared rendering tools, probe baking and data management. |
| [Fraxinus-ShaderGraph](Packages/Fraxinus-ShaderGraph/FraxinusChanges.md) | `com.unity.shadergraph` | `14.0.12` | Shader Graph fork exposing the internal APIs needed by UltraRP targets. |
| [Fraxinus-Light-Transport](Packages/Fraxinus-Light-Transport/README.md) | `com.fraxinus.rendering.light-transport` | `1.0.1` | Compute ray queries, acceleration structures and sampling. |

**2.0 names the suite; each UPM package retains its own version.** Use all four packages from one checkout or Git revision. Matching version numbers do not make official packages interchangeable with these forks. Fraxinus URP, URP Config, GPU Batcher and GPU Terrain have been removed from the current distribution; see [Changes from 1.x](docs/ChangesFrom1.x.md) for the scope changes.

## Getting started

1. Put this checkout beside your Unity project and merge the `dependencies` from [manifest.local.json](docs/manifest.local.json) into the project's `Packages/manifest.json`, preserving other dependencies. The [usage guide](docs/Usage.md) also covers Git installation after publication.
2. Open the consuming project with **Unity 2022.3.62f3** and let Package Manager resolve dependencies.
3. Create a pipeline asset and its associated Renderer Graph through **Assets > Create > FraxinusEngine > UltraRP > UltraRP Asset**. Connect drawing, shadow and post-processing nodes using the [graph configuration guide](Packages/FraxinusRP/Documentation~/Installation.md).
4. Assign the UltraRP asset in **Project Settings > Graphics** and check the active **Quality** override. Use the included UltraRP shaders or create Lit / Unlit graphs through **Assets > Create > Shader Graph > UltraRP**.

This repository contains packages and documentation. Open a consuming Unity project in the Editor. FraxinusRP currently does not ship a sample scene or sample generator.

## Support and validation

- **Dependencies:** Core and Shader Graph retain Unity package IDs and replace the corresponding official packages. UltraRP needs the companion forks; official Shader Graph `14.0.12` does not provide the required internal API access.
- **Lighting paths:** Hybrid Lit and Shader Graph Lit / Unlit render through Forward / ForwardOnly. Rectangle lights support the documented Forward paths and ordinary Lit Clustered Deferred; Stencil Deferred and Shader Graph area-light receivers are outside that scope.
- **Graph and material extensions:** `JobGraphProcessor` currently uses a synchronous fallback. Advanced Shader Graph targets and VFX integration still have isolated components. Project-defined nodes need reflection-preservation rules from the [IL2CPP guide](Packages/FraxinusRP/Documentation~/IL2CPP.md).
- **Platforms and textures:** Unity labels SVT experimental. XR, hardware dynamic resolution and child contexts are among the paths that do not collect feedback. Other features have their own camera and platform limits, documented individually.

The retained [integration validation](docs/Validation.md) covers local import and C# compilation of the earlier eight-package snapshot. **Unity validation has not been rerun for the current four-package layout.** Package-level GPU / Player results likewise apply to their historical source revisions and environments. See [UltraRP validation](Packages/FraxinusRP/Documentation~/Validation.md), [Core tests](Packages/Fraxinus-RPCore/Tests~/Unity2022APV/README.md) and [Light Transport smoke checks](Packages/Fraxinus-Light-Transport/Tests~/GpuSmoke/README.md).

## Source and licensing

The layout follows [FraxinusGraphics](https://github.com/bluewhitebow/FraxinusGraphics), with the retained packages drawn from the Fraxinus Unity 2022.3 development project. [PackageSources.json](docs/PackageSources.json) preserves the source revisions and content inventory from the initial eight-package import for historical traceability. This page and the [usage guide](docs/Usage.md) define the current installation scope.

Licenses and third-party notices remain with their respective packages; no new common license is assigned. See the [Core license](Packages/Fraxinus-RPCore/LICENSE.md), [Shader Graph license](Packages/Fraxinus-ShaderGraph/LICENSE.md), [Light Transport license](Packages/Fraxinus-Light-Transport/LICENSE.md) and [Hybrid lighting attribution](Packages/FraxinusRP/ShaderLibrary/Lighting/README.md). Parts of Hybrid lighting are adapted from IllusionRP, including algorithms originating in Unity HDRP.
