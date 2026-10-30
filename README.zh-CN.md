# FraxinusGraphics 2.0

[English](README.md) | [简体中文](README.zh-CN.md)

**面向 Unity 2022.3 的可编排渲染套件，让渲染流程本身成为可编辑的图。**

FraxinusGraphics 2.0 以 **ScriptableNode / UltraRP** 为核心，将渲染节点编辑、类型化 RenderGraph、材质创作与探针光照整合到一套配套源码中。你可以在 Renderer Graph 中连接绘制、阴影、后处理和中间纹理，按项目需要组织渲染流程，也可以编写自定义节点与 Shader。

UltraRP 是独立的 Scriptable Render Pipeline，从 Fraxinus Engine 的 Unity 6 分支移植到 **Unity 2022.3.62f3**。当前仓库包含 **FraxinusRP、Fraxinus Core、Shader Graph 和 Light Transport 四个包**，安装与运行 UltraRP 无需 URP。

[安装与使用](docs/Usage.zh-CN.md) · [相对 1.x 的变化](docs/ChangesFrom1.x.md) · [本地依赖配置](docs/manifest.local.json) · [Git 依赖配置](docs/manifest.git.json)

## 核心特性

### 可编辑的渲染流程

**Renderer Graph** 用节点与资源连接表达渲染流程，覆盖 Forward、Forward+、Stencil Deferred 与 Clustered Deferred。图编辑器支持保存、复制粘贴、Undo/Redo 和窗口重载；自定义节点与 Pass 注入用于扩展项目自己的渲染逻辑。

**嵌套渲染 Context** 可以为同一相机视角建立独立的颜色、深度与可变帧数据，并使用不同分辨率的中间目标。子结果可显式合成回父目标，适合组织局部渲染流程和多阶段合成。子 Context 复用相机视角与剔除结果，不拥有独立的 TAA / STP 历史。

Renderer Graph 负责组织渲染节点；底层 **RenderGraph** 负责 Pass 与资源依赖。两者共同构成从可视化编排到渲染执行的工作流。[图工作流](Packages/FraxinusRP/Documentation~/ScriptableNode.md)

### 材质与实时光照

**Hybrid Lit** 在 Lit 工作流上加入 rough diffuse、GGX、微阴影、FGD 环境反射与 AO 多次反弹近似，并支持 Clear Coat。**LTC 矩形面光**提供具有面积感的实时照明，可设置尺寸、漫反射／高光强度及 Rendering Layers；当前每相机最多 16 盏，无面光阴影。

Volume 效果包括 **GTAO**、主方向光 **Contact Shadows** 和基于直方图的**自动曝光**，分别用于补充遮蔽、近距离接触细节与亮度适应。具体材质和渲染路径支持见 [Hybrid 光照](Packages/FraxinusRP/Documentation~/HybridLighting.md)、[GTAO](Packages/FraxinusRP/Documentation~/AmbientOcclusion.md)及[接触阴影与曝光](Packages/FraxinusRP/Documentation~/ContactShadowsAndExposure.md)。

### 探针光照与预计算重光照

**APV** 提供自适应探针布点、独立烘焙、天空遮蔽与探针数据流式加载。**APRTV** 使用预计算的辐射传输数据，在运行时响应方向光与 HDR 天空变化，并支持光照场景混合。UltraRP 已接入对应后端、材质采样与重光照调度。

Core 的探针烘焙使用 Light Transport 提供的 **Compute 射线查询**，无需 DXR。APRTV 面向预计算的静态场景：几何和材质变化需要重烘，当前不处理动态遮挡或运行时多反弹反馈；APRT 数据按已加载场景整批加载。[APV 烘焙](Packages/Fraxinus-RPCore/Documentation~/StandaloneAPVBaking.md) · [APRTV](Packages/Fraxinus-RPCore/Documentation~/APRT.md)

### Shader Graph 与流式虚拟纹理

FraxinusRP 提供 **UltraRP Lit / Unlit Shader Graph Target**，使用配套 Shader Graph 分支开放的内部 API，支持透明、Alpha Clip，以及深度、法线、阴影、Meta 和运动矢量等辅助 Pass，方便在可视化材质创作和手写 Shader 之间选择。

**Streaming Virtual Texturing（SVT）**接入 Unity 原生 Texture Stack、纹理页反馈与缓存后端，并开放缓存预算和粗 mip 预加载配置。需要显式开启项目 VT 并创作对应材质；普通 Texture2D 不会自动转换为虚拟纹理。[Shader Graph](Packages/FraxinusRP/Documentation~/ShaderGraph.md) · [SVT](Packages/FraxinusRP/Documentation~/VirtualTexturing.md)

### 超分辨率与时序处理

提供 **STP、SGSR1 空间超分辨率和 SGSR2 时序超分辨率**，配合 Render Scale 调整渲染分辨率。时序路径接入运动矢量、相机历史与历史重置，并针对不支持的配置提供回退；可按画质目标与目标设备选择。[超分辨率配置](Packages/FraxinusRP/Documentation~/Upscaling.md)

### 配套渲染基础设施

Fraxinus Core 提供 **Raster / Compute / Unsafe RenderGraph Pass、RTHandle、Volume、Blitter 和 Rendering Debugger**，并适配 Unity 2022 执行接口。Light Transport 提供基于 Compute 的 BVH 构建与遍历、射线命中／遮挡查询和采样工具，供探针烘焙及自定义计算使用。[Core](Packages/Fraxinus-RPCore/README.zh-CN.md) · [Light Transport](Packages/Fraxinus-Light-Transport/README.zh-CN.md)

## 与其他渲染管线有什么不同？

本项目的重点是**在 Unity 2022.3 上，把可编辑渲染流程、配套光照功能和可修改的渲染底层放在一起**。下面比较的是工作流与集成方式；没有统一基准测试支持画质或性能排名。

| 对照对象 | 主要区别 | 选择时需要考虑 |
| --- | --- | --- |
| Built-in Render Pipeline | UltraRP 使用独立 SRP、Renderer Graph 和显式资源依赖来组织渲染，并提供自己的材质与光照路径。 | 需要配置管线资产、图和对应材质；已有 Built-in Shader 需要迁移。 |
| 官方 URP 14（Unity 2022.3） | URP 通常通过 Renderer 与 Renderer Feature 扩展；UltraRP 将渲染流程暴露为可编辑节点图，并提供嵌套 Context、Hybrid Lit、LTC 面光及配套探针工作流。 | 需要自己维护图连接与定制代码；URP Renderer Feature 和材质需要针对 UltraRP 适配。 |
| 官方 HDRP | UltraRP 强调在可编排流程中组合所需功能，包含部分来自 HDRP 算法体系的光照实现，以及自己的 Core / Shader Graph 集成。 | 功能范围以本仓库文档为准；不能据此假定完整 HDRP 功能覆盖或 HDRP 材质兼容。 |
| Unity 6 的 SRP 工作流 | 将部分新版渲染基础与 Fraxinus Engine 工作流带到 Unity 2022.3，适合仍需使用该引擎版本的项目。 | 回移范围受引擎 API 限制；Unity 6 原生 GPU Resident Drawer 在 Unity 2022 中不可用。 |

如果你需要研究渲染流程、开发项目专用效果，或希望在 Unity 2022.3 上使用可修改的节点化 SRP，本项目提供了相应的源码基础。接入现有项目时，应先用代表性场景检查材质迁移、目标平台和帧预算。

## 四个包的分工

| 包 | UPM 标识 | 版本 | 职责 |
| --- | --- | --- | --- |
| [FraxinusRP](Packages/FraxinusRP/README.md) | `com.bluewhitebow.fraxinus-rp` | `0.1.0` | ScriptableNode、UltraRP、渲染节点与材质／光照集成。 |
| [Fraxinus-RPCore](Packages/Fraxinus-RPCore/README.zh-CN.md) | `com.unity.render-pipelines.core` | `14.0.99` | RenderGraph、共享渲染工具、探针烘焙与数据管理。 |
| [Fraxinus-ShaderGraph](Packages/Fraxinus-ShaderGraph/FraxinusChanges.md) | `com.unity.shadergraph` | `14.0.12` | 为 UltraRP Target 开放所需内部 API 的 Shader Graph 分支。 |
| [Fraxinus-Light-Transport](Packages/Fraxinus-Light-Transport/README.zh-CN.md) | `com.fraxinus.rendering.light-transport` | `1.0.1` | Compute 射线查询、加速结构与采样。 |

**2.0 是套件名称，各 UPM 包保留自己的版本号。** 四个包应来自同一份 checkout 或同一个 Git 提交；版本号相同不代表官方包与定制分支可以互换。当前发行范围已移除 Fraxinus URP、URP Config、GPU Batcher 和 GPU Terrain，调整原因见[相对 1.x 的变化](docs/ChangesFrom1.x.md)。

## 开始使用

1. 将本仓库放在 Unity 工程的同级目录，将 [manifest.local.json](docs/manifest.local.json) 的 `dependencies` 合并到工程的 `Packages/manifest.json`，保留工程其他依赖。发布后的 Git 安装方式见[使用指南](docs/Usage.zh-CN.md)。
2. 用 **Unity 2022.3.62f3** 打开使用这些包的工程，等待 Package Manager 完成依赖解析。
3. 通过 **Assets > Create > FraxinusEngine > UltraRP > UltraRP Asset** 创建管线资产及关联的 Renderer Graph，按[图配置说明](Packages/FraxinusRP/Documentation~/Installation.md)连接绘制、阴影与后处理节点。
4. 在 **Project Settings > Graphics** 指定 UltraRP 资产，检查当前 **Quality** 等级的覆盖设置。使用包内 UltraRP Shader，或通过 **Assets > Create > Shader Graph > UltraRP** 创建 Lit / Unlit 图。

本仓库提供包和文档；Editor 应打开引用这些包的 Unity 工程。FraxinusRP 当前不发布示例场景或示例生成器。

## 支持范围与验证

- **依赖**：Core 与 Shader Graph 沿用 Unity 包 ID，会替换相应官方包。UltraRP 需要配套分支；官方 Shader Graph 即使同为 `14.0.12`，也不包含所需内部 API 访问权限。
- **光照路径**：Hybrid Lit 与 Shader Graph Lit / Unlit 通过 Forward / ForwardOnly 绘制。矩形面光支持文档所列 Forward 路径及普通 Lit 的 Clustered Deferred；不包含 Stencil Deferred 和 Shader Graph 面光接收路径。
- **图与材质扩展**：`JobGraphProcessor` 当前使用同步后备实现；高级 Shader Graph Target 和 VFX 集成仍有隔离项。项目自定义节点需要按 [IL2CPP 指南](Packages/FraxinusRP/Documentation~/IL2CPP.md)配置反射保留。
- **平台与纹理**：SVT 属于 Unity 实验性功能，XR、硬件动态分辨率和子 Context 等路径不收集反馈。其他功能也有各自的相机与平台范围，请以功能文档为准。

仓库保留的[整合验证记录](docs/Validation.md)对应移除前的八包快照，覆盖本地导入与 C# 编译，**当前四包布局尚未重新执行 Unity 验证**。包内 GPU / Player 测试同样对应各自历史源码与环境；可查看 [UltraRP 验证记录](Packages/FraxinusRP/Documentation~/Validation.md)、[Core 测试](Packages/Fraxinus-RPCore/Tests~/Unity2022APV/README.md)和 [Light Transport 烟测](Packages/Fraxinus-Light-Transport/Tests~/GpuSmoke/README.md)。

## 来源与许可证

仓库结构参考 [FraxinusGraphics](https://github.com/bluewhitebow/FraxinusGraphics)，现有包来自 Fraxinus Unity 2022.3 开发工程。[PackageSources.json](docs/PackageSources.json) 保留初始八包导入时的来源提交与内容清单，用于追溯历史；当前安装范围以本页及[使用指南](docs/Usage.zh-CN.md)为准。

许可证与第三方声明保留在各自包中，未重新指定统一许可证。参见 [Core 许可证](Packages/Fraxinus-RPCore/LICENSE.md)、[Shader Graph 许可证](Packages/Fraxinus-ShaderGraph/LICENSE.md)、[Light Transport 许可证](Packages/Fraxinus-Light-Transport/LICENSE.md)及 [Hybrid 光照来源说明](Packages/FraxinusRP/ShaderLibrary/Lighting/README.md)。Hybrid 光照部分算法适配自 IllusionRP，其中包含源于 Unity HDRP 的算法。
