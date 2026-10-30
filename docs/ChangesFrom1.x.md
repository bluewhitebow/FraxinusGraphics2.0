# Changes from FraxinusGraphics 1.x / 相对 1.x 的变化

[English home](../README.md) · [中文首页](../README.zh-CN.md)

The reference is [FraxinusGraphics](https://github.com/bluewhitebow/FraxinusGraphics/tree/311a15a4dd6645a4443e67f0d7c9c5cad3b7e9f2), whose repository contains Core, URP, URP Config and Light Transport. The current 2.0 repository centers on **ScriptableNode / UltraRP** and contains **four packages**: FraxinusRP, Core, Shader Graph and Light Transport. URP, URP Config, GPU Batcher and GPU Terrain are no longer included. This comparison describes repository scope and documented capabilities, not a performance benchmark.

对照对象为 [FraxinusGraphics](https://github.com/bluewhitebow/FraxinusGraphics/tree/311a15a4dd6645a4443e67f0d7c9c5cad3b7e9f2)，其中包含 Core、URP、URP Config 和 Light Transport 四个包。当前 2.0 以 **ScriptableNode / UltraRP** 为中心，保留 **四个包**：FraxinusRP、Core、Shader Graph 与 Light Transport，不再收录 URP、URP Config、GPU Batcher 和 GPU Terrain。本页比较仓库范围与已记录的能力，不是性能测试。

| Area / 方向 | Reference repository / 参考仓库 | Current 2.0 repository / 当前 2.0 仓库 |
| --- | --- | --- |
| Package set / 包集合 | Four companion packages / 四个配套包。 | Four packages: Core and Light Transport are retained; FraxinusRP and Shader Graph replace URP and Config in the distributed package set / 四个包：保留 Core 与 Light Transport，发行包集合中的 URP 和 Config 改为 FraxinusRP 与 Shader Graph。 |
| Rendering workflow / 渲染工作流 | Fraxinus URP with Core / Fraxinus URP 与 Core。 | ScriptableNode / UltraRP Renderer Graph authoring, with no URP installation required for UltraRP / 以 ScriptableNode / UltraRP Renderer Graph 编辑为主，UltraRP 本身无需安装 URP。[Source / 来源](../Packages/FraxinusRP/README.md)。 |
| Shared Core / 共享 Core | RenderGraph, probe baking, STP and common rendering utilities / RenderGraph、探针烘焙、STP 与通用渲染工具。 | Supplies UltraRP with scoped context lookup, pipeline settings integration, shared scene-light tracking and APV / APRTV backends / 为 UltraRP 提供作用域上下文读取、管线设置集成、共享场景灯光追踪及 APV / APRTV 后端。[Migration / 迁移说明](../Packages/FraxinusRP/Documentation~/Migration.md)。 |
| Shader Graph | Not a package in the reference monorepo / 未作为独立包收录。 | Includes the 14.0.12 fork granting UltraRP's editor assembly access to internal custom-target APIs / 纳入 14.0.12 分支，为 UltraRP 编辑器开放内部 Target API。[Fork changes / 分支差异](../Packages/Fraxinus-ShaderGraph/FraxinusChanges.md)。 |
| UltraRP lighting / UltraRP 光照 | UltraRP is outside that repository / 仓库不包含 UltraRP。 | Hybrid Lit, unshadowed rectangle lights, GTAO, Contact Shadows and Auto Exposure, with per-path limits / 包含 Hybrid Lit、无阴影矩形面光、GTAO、Contact Shadows 与自动曝光，各有路径限制。[Updates / 更新记录](../Packages/FraxinusRP/Documentation~/Updates.md)。 |
| GPU scene rendering / GPU 场景渲染 | URP provides external drawing hooks / URP 提供外部绘制接口。 | GPU Batcher and GPU Terrain are outside the current package set; neither has been connected to the UltraRP Renderer Graph / 当前包集合不含 GPU Batcher 与 GPU Terrain，两者均未接入 UltraRP Renderer Graph。 |
| Virtual texturing / 虚拟纹理 | No UltraRP package / 不包含 UltraRP 包。 | UltraRP native Streaming Virtual Texturing integration on documented paths; terrain RVT from the removed GPU Terrain package is outside this release / 提供文档所列路径的 UltraRP 原生流式虚拟纹理集成；已移出的 GPU Terrain 地形 RVT 不属于当前发行范围。[SVT](../Packages/FraxinusRP/Documentation~/VirtualTexturing.md)。 |
| Distribution / 分发 | Four packages from one repository / 同仓库四个包。 | Coordinated local / Git dependencies for FraxinusRP, Core, Shader Graph and Light Transport / 为 FraxinusRP、Core、Shader Graph 与 Light Transport 提供配套本地 / Git 依赖配置。[Usage / 使用指南](Usage.zh-CN.md)。 |

Typed RenderGraph, STP, APV / APRT and Compute ray queries build on the earlier Fraxinus foundation; their inclusion does not make them new 2.0 implementations. UltraRP adds its own graph workflow and rendering integrations. Features of the removed URP package, such as its cached cascaded-shadow path, should not be read as current UltraRP capabilities. Package versions retain their source values rather than all becoming `2.0.0`.

类型化 RenderGraph、STP、APV / APRT 与 Compute 射线查询延续已有 Fraxinus 基础，收录这些能力不代表在 2.0 中重新实现。UltraRP 提供自己的图编排工作流和渲染集成。已移除 URP 包中的缓存级联阴影等实现，不能视作当前 UltraRP 的功能，各包版本也不会统一改成 `2.0.0`。

GPU Batcher and GPU Terrain were removed from this checkout because their rendering paths have not been integrated into UltraRP. GPU Terrain has hard dependencies on the removed Fraxinus URP APIs, and Batcher's shaders also depend on URP integration. Removing them keeps the distributed dependency set aligned with the active pipeline.

GPU Batcher 与 GPU Terrain 的渲染路径尚未接入 UltraRP，因此已从当前仓库移出。GPU Terrain 硬依赖已移出的 Fraxinus URP API，Batcher 着色器也依赖 URP 集成。当前发行依赖随之收敛为与 UltraRP 配套的四个包。

The [source inventory](PackageSources.json) preserves the original eight-package export, including the four packages removed from the current checkout. The [integration validation](Validation.md) also records that earlier layout; Unity import and compilation have not been rerun for the current four-package repository. Existing feature tests retain their original pipeline and snapshot scope. The previous repository's screenshots and standalone demo do not validate the current repository.

[来源清单](PackageSources.json)保留初次导出的八包记录，包含当前仓库已移出的四个包。[整合验证](Validation.md)也对应此前的八包布局；当前四包仓库尚未重新运行 Unity 导入和编译。已有功能测试仍以其原始管线、源码快照为准，旧仓库截图与独立 Demo 不作为当前仓库的验收证据。
