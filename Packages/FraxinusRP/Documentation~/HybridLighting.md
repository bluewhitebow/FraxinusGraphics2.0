# Hybrid Lit 与矩形面光

支持 `Ultra Render Pipeline/Hybrid Lit` 和无阴影的实时 Rectangle 面光。普通 Ultra Lit、Complex Lit 与 Hybrid Lit 的 Forward 光照，以及普通 Lit 的 Clustered Deferred，共用 LTC 面光计算；Hybrid 另外使用 rough diffuse、GGX、微阴影、FGD 环境反射与 AO 多次反弹近似。

## 使用

1. 使用配套 Fraxinus Core 与 Ultra Pipeline。安装版本见 [Installation.md](Installation.md)。
2. 创建材质，将 Shader 设为 **Ultra Render Pipeline/Hybrid Lit**。沿用 Lit 的金属/高光工作流、贴图、透明和 Alpha Clip，支持 Clear Coat。
3. 在 Light Inspector 中将 Type 设为 **Area / Rectangle**、Mode 设为 **Realtime**，设置 Size、Range、Intensity。现有 Ultra Additional Light Data 保存运行时宽高，以及 Diffuse / Specular Dimmer。
4. 矩形沿局部 **+Z** 单面发光，旋转灯光使蓝色轴朝向接收物体。尺寸使用世界单位，与 Transform Scale 无关。面光没有遮挡阴影，墙体不会阻止光照。
5. 若要限制接收物体，在 Pipeline Asset 启用 Rendering Layers，并设置灯光和 Mesh Renderer 的 Rendering Layer Mask。Light 的旧 Culling Mask 只参与相机粗筛，不能替代逐物体 Rendering Layers。

运行时创建灯光时，先将 Light.type 设为 Rectangle，再添加 `UltraAdditionalLightData`，通过其 `areaSize`、`areaDiffuseDimmer`、`areaSpecularDimmer` 调整参数。Unity 2022 的 `Light.areaSize` 是 Editor API，Player 使用附加数据内保存的尺寸。编辑器和场景构建处理会保存 authored Realtime/Baked 模式；Mixed Rectangle 暂不支持。

## 渲染路径

| 路径 / 材质 | 当前支持 |
| --- | --- |
| 普通 Ultra Lit / Complex Lit Forward | 实时矩形面光 |
| Hybrid Lit Forward / Forward+ | 矩形面光及增强直接光、FGD 环境光 |
| Deferred 图中的 Hybrid Lit | 通过 UltraForwardOnly 节点照明；自定义图需包含此绘制入口 |
| 普通 Lit 的 Clustered Deferred | 实时矩形面光，支持金属/高光工作流、Rendering Layers 和关闭直接高光 |
| 普通 Lit 的 Stencil Deferred | 尚未接入面光循环 |
| Shader Graph | 默认 Forward target 为 2.0，本阶段不列为面光支持范围；部分条件变体提高 target，不代表整个图已支持 |
| 自定义 Shader | 必须使用共享 PBR 入口且编译 target 至少 3.5；需要自行验证相关变体和资源消费 |
| Simple Lit / 自定义光照 | 没有自动接入 LTC 面光 |

Hybrid 使用本地 Complex Lit 的 ShadowCaster、辅助 GBuffer、深度法线、Meta 和运动矢量 Pass。其材质分类沿用 ComplexLit，防止 Deferred 与 ForwardOnly 重复照明。已有 APRT、Contact Shadows 和后处理曝光入口保留。

Scene View 使用同样的面光输入。启用后处理的 Renderer Graph 必须将 `ColorGradingLutPass.colorLut` 接到 `PostProcessPass.colorLut`；即使没有启用 Volume 效果，UberPost 也会采样该表。缺少这条连接可能导致 Scene View 开启后处理时整屏变灰。

方向光阴影需要同时启用 Pipeline Asset 的阴影设置并接通渲染节点：将 `MainLightShadowCasterPass` 设为 `BeforeDrivenPass`，把其 `mainLightShadow` 输出连接到需要接收阴影的 `DrawObjectsPass.mainShadowMap`，并在方向光上启用 Hard 或 Soft Shadows。可从 2048 阴影图、2 级联和 50 米距离开始调整。未连接的阴影节点不会进入渲染执行集合。Contact Shadows 只补充近距离接触细节，不能替代主光阴影图。

本次没有迁入上游的 ForwardGBuffer、SSR/SSGI、OIT、WetSurface、路径追踪，也没有复制其材质曝光乘法；这些属于独立功能。

## 数量与资源

每相机最多选择 **16** 盏可见实时矩形灯，超出时采用距离相机最近的灯，并报告一次提示。范围剔除包含矩形自身的尺寸。已启用的附加灯光组件维护候选列表，因此不依赖 Unity 2022 原生剔除是否返回 Rectangle，也不在绘制中搜索场景或创建组件。

每相机将灯光快照上传为固定数组，复用本地 LTC / FGD 查表。RenderGraph 显式声明纹理读取；无灯相机重置灯数，同视角子 Context 继承只读灯光输入。原生可见灯列表若出现 Rectangle，会保留普通灯槽位并清零其普通点光贡献，避免破坏 Cookie/阴影索引；这些槽位仍占原生可见灯预算。

Clustered Deferred 按 kernel 显式绑定 LTC / FGD 和当前相机灯数，只为普通 Lit 执行共享面光循环。矩形灯仍遍历上述最多 16 盏灯，尚未加入 punctual light 的 tile/cluster 剔除列表；Hybrid 继续通过 ForwardOnly 着色。

普通 Lit/Complex Lit 的 Forward shader target 提高到 3.5，Hybrid 为 4.5。实时面光要求 Texture2DArray；低端 Shader、XR 与 Preview 相机不在本阶段面光支持范围。Cookie、面光阴影、barn door 和按 tile/cluster 筛选面光属于后续阶段。

LTC 七张数值表与项目原有数据相同。源码来源及许可证见 [光照库 README](../ShaderLibrary/Lighting/README.md)。详细架构与后续计划见 [迁移设计](HybridLitAndAreaLightsMigration.md)。

## 验证

下列记录描述当时的测试源码与宿主场景；示例和生成器已从当前发布包移除，其源码可查看清理前提交 `02a80e8`。

2026-10-28 Scene View / Clustered 修复追加验证（Unity 2022.3.62f3 / Windows / GTX 1650 / D3D11 / Linear）：

- 隔离项目先在提交 `199ed9b` 复现普通 Lit 在 Clustered 与 Scene View 中没有面光贡献；Hybrid ForwardOnly 正常。
- 修复后 35 张 Editor GPU 采样通过，无非有限值、粉色错误像素或运行时错误。普通 Lit 的金属/粗糙度及高光工作流与 Forward 的均值差小于 0.5%；关闭高光时保留漫反射，金属不泄漏漫反射。覆盖 Rendering Layers、单面发光、普通点光、Simple Lit / Unlit 排除及真实 Scene View 相机。
- 另外 4 张 Stencil 点光采样确认 GBuffer 保留反射率后，关闭高光仍保留非金属漫反射、消除金属直接高光。Stencil 的移动端 / Switch 分支也统一读取材质标志；这些设备没有进行运行验证。
- 实际宿主 Scene View 的最终输出已恢复球体、面光着色和 Gizmos。仅补齐现有 Demo renderer graph 的 LUT，不保存或重建用户正在编辑的场景。
- 新生成 Demo 在默认非 Native 模式下，Scene View 后处理开关均正常。追加检查发现 Unity 2022 的 Native RenderGraph + 后处理 + Gizmos 组合仍会偏色并出现 LUT 彩条；回退 `199ed9b` 的 Renderer 也能逐值复现。该既有问题尚未修复，Demo 默认关闭 Native Pass；Native + 后处理测试未验收通过。对照和人工复核见下述 `Reports/DemoSceneView`、`Reports/DemoSceneView-RendererBaseline`。
- 证据：`D:/Unity/FraxinusRPEvidence/2026-10-28/ClusterAreaValidation/Reports/{Baseline,Fixed}/Report.json` 与 `SceneViewState8.SceneEnd.png`。本次 Clustered 增量未重新构建 Player；以下 Player 结果属于此前 Forward / Hybrid 验证。

2026-10-28，在 Unity 2022.3.62f3 / Windows / NVIDIA GTX 1650 / D3D11 / Gamma 下完成：

- 干净基线和实际宿主项目 Editor 脚本编译通过。恢复配套 Core `5a7a0c6`、URPGlobal `fe23063`，宿主 manifest / lock 已解析本地 Core。
- 40 组 Editor GPU 采样通过：两种材质的面光开关和背面、金属/粗糙度、Clear Coat、透明/Alpha Clip、Rendering Layers、多相机、正交 Forward+、两种 Deferred 图中的 Hybrid ForwardOnly、普通灯与 Cookie、FGD 环境、子 Context 继承/恢复及生成示例。
- Windows Mono Development Player 构建通过，启用 unused-variant stripping；11 组 Player GPU 采样通过，验证普通 Lit / Hybrid、Realtime/Baked、序列化尺寸及运行时尺寸变化、零灯和多相机恢复。所有图像检查均无非有限值、粉色错误像素和运行时错误。
- Editor 确认 Hybrid 全部 9 个 Pass；当前无 Enlighten Realtime GI 的 Player 配置正常裁掉 Meta，8 个运行时 Pass 均保留。
- 示例生成器保留测试项目的 Graphics、Quality 和已加载场景；未命名场景入口在资产创建前返回。原生 Rectangle 未出现在本次 Unity 2022 可见灯列表，附加组件登记路径实际参与渲染。

Player 构建保留 5 条已有公共着色代码警告：三种材质的 `SampleContactShadow`、AutoExposure 的 isnan 优化提示、DebugViewTiles 的字体采样提示。新增 Hybrid GI 和附加数据字段的警告已修复。没有执行 IL2CPP、XR、其他图形 API、Linear 亮度校准或性能预算验收；本次没有重新验证 APRT/Contact Shadows 的全部效果组合。

本地证据目录为宿主项目 `Logs/FraxinusRP/HybridAreaLights-2026-10-28/`：`Gpu/Report.json`、`GpuFinal/Report.json`、`Player/Build.json`、`Player/Run.json` 与对应 PNG / 日志。测试 harness 仅放在该隔离验证项目，未纳入发布包。源码和报告哈希由该目录的 `FinalSourceHashes.json` / `ValidationSummary.json` 记录。

宿主编译期间 Unity 自动重写了 `ProjectSettings/GraphicsSettings.asset` 和 `ShaderGraphSettings.asset`；没有主动切换项目管线，当前 Graphics 仍指向已有 DeferredPipeline 资产，Quality 文件未变化。没有本轮编译前这两份设置的逐字快照，因此不声称它们内容完全未变。
