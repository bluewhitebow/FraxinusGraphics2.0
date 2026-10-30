# Changes from FraxinusGraphics 1.x / 相对 1.x 的变化

[English home](../README.md) · [中文首页](../README.zh-CN.md)

Compared with [FraxinusGraphics 1.x](https://github.com/bluewhitebow/FraxinusGraphics/tree/311a15a4dd6645a4443e67f0d7c9c5cad3b7e9f2), Unity-FraxinusRP centers on **ScriptableNode / UltraRP** and distributes four companion packages: FraxinusRP, Core, Shader Graph and Light Transport.

相较于 [FraxinusGraphics 1.x](https://github.com/bluewhitebow/FraxinusGraphics/tree/311a15a4dd6645a4443e67f0d7c9c5cad3b7e9f2)，Unity-FraxinusRP 以 **ScriptableNode / UltraRP** 为核心，提供 FraxinusRP、Core、Shader Graph 与 Light Transport 四个配套包。

| Area / 方向 | FraxinusGraphics 1.x | Unity-FraxinusRP |
| --- | --- | --- |
| Pipeline / 管线 | Fraxinus URP and its Core / Fraxinus URP 及其配套 Core。 | Independent UltraRP with editable Renderer Graphs / 独立 UltraRP 与可编辑 Renderer Graph。[Graph workflow / 图工作流](../Packages/FraxinusRP/Documentation~/ScriptableNode.md)。 |
| Rendering foundation / 渲染基础 | RenderGraph, STP, probe baking and Compute ray queries / RenderGraph、STP、探针烘焙与 Compute 射线查询。 | Reuses these foundations and adds scoped context lookup, pipeline settings integration and shared scene-light tracking / 延续这些基础，增加作用域上下文读取、管线设置集成及共享场景灯光追踪。[Migration / 迁移说明](../Packages/FraxinusRP/Documentation~/Migration.md)。 |
| Material authoring / 材质创作 | Shader Graph is not distributed in the repository / 仓库未收录 Shader Graph。 | Matching Shader Graph 14.0.12 fork with UltraRP Lit / Unlit targets / 配套 Shader Graph 14.0.12 分支及 UltraRP Lit / Unlit Target。[Shader Graph](../Packages/FraxinusRP/Documentation~/ShaderGraph.md)。 |
| UltraRP lighting / UltraRP 光照 | UltraRP is outside the repository / 仓库不包含 UltraRP。 | Hybrid Lit, unshadowed rectangle lights, GTAO, Contact Shadows and Auto Exposure, with documented path support / 提供 Hybrid Lit、无阴影矩形面光、GTAO、Contact Shadows 与自动曝光，支持路径见文档。[Updates / 更新记录](../Packages/FraxinusRP/Documentation~/Updates.md)。 |
| Probe lighting / 探针光照 | Core probe workflows integrated with Fraxinus URP / Core 探针工作流接入 Fraxinus URP。 | APV / APRTV backends and relighting integration in UltraRP / UltraRP 接入 APV / APRTV 后端与重光照。[APRTV validation / APRTV 验证](../Packages/FraxinusRP/Documentation~/Validation.md#aprtv-integration-2026-10-23-unreleased)。 |
| Virtual texturing / 虚拟纹理 | No UltraRP package / 不包含 UltraRP 包。 | Native Streaming Virtual Texturing integration on documented UltraRP paths / 在文档所列 UltraRP 路径接入原生流式虚拟纹理。[SVT](../Packages/FraxinusRP/Documentation~/VirtualTexturing.md)。 |

UltraRP uses the matching Core and Shader Graph forks and does not require URP. Each package retains its own version. See [installation and usage](Usage.md) for dependency configuration.

UltraRP 使用配套 Core 与 Shader Graph 分支，无需安装 URP。各包保留独立版本号。依赖配置见[安装与使用](Usage.zh-CN.md)。
