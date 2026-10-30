# 安装 FraxinusRP

## 最新 main（2026-10-28）

本次 main 新增 Hybrid Lit 与实时无阴影矩形面光。此前的 Contact Shadows、直方图自动曝光、阴影修复、InjectPass 调度、Renderer Graph 界面、GC 优化、APRTV、Native Pass 和 Bloom 修复也包含在内。安装这一版时，将下方历史 manifest 示例中的对应依赖替换为：

```json
{
  "com.bluewhitebow.fraxinus-rp": "https://github.com/bluewhitebow/FraxinusRP.git#main",
  "com.unity.render-pipelines.core": "https://github.com/bluewhitebow/Fraxinus-RPCore.git#5a7a0c68efae9e2c527332c15349227077326679",
  "com.unity.render-pipelines.universal": "https://github.com/bluewhitebow/Fraxinus-URPExtensions.git#162e7059c38ae0e47bcf6ce92e75889ea08c28fb"
}
```

其余依赖沿用下方固定版本。Core 现在提供共享场景灯光追踪程序集；如果项目安装了 URP，必须同步更新到上述 URP 提交，避免旧 URP 包中的同名程序集冲突。FraxinusRP 本身不要求安装 URP，未安装时无需添加该依赖。`main` 会随开发推进；此前 renderer 更新对应 FraxinusRP 提交 `64b4c7575895d27e850b178548f0496820963bad`，该提交不包含本次 Hybrid 光照。

Hybrid 光照在 Unity 2022.3 / D3D11 下通过 Editor 编译、40 组 Editor GPU 采样，以及 Windows Mono Development Player 构建和 11 组 Player GPU 采样。范围和限制见 [HybridLighting.md](HybridLighting.md)。这不包含普通 Lit 的 Deferred 面光、面光 Cookie 或阴影，也不代表 IL2CPP、XR 或性能预算验收。功能摘要见 [Updates.md](Updates.md)；此前效果设置见 [ContactShadowsAndExposure.md](ContactShadowsAndExposure.md)。

## SGSR 检查点（历史版本）

此前的 SGSR 检查点为 `unity2022-checkpoint-2026-10-23`，适用于 Unity **2022.3.62f3**，新增 SGSR1／SGSR2。该标签保持原有源码，不包含上方 main 的后续更新。包版本仍为 **0.1.0**；使用方式和该轮验收范围见 [Upscaling.md](Upscaling.md)，历史测试记录见 [Validation.md](Validation.md)。

## 配套依赖

下方固定标签包含 [2026-10-23 SGSR 更新](Updates.md#2026-10-23-sgsr1-and-sgsr2)及此前的开发改动。该历史检查点配合下列 Core 提交使用，其中包含 `ContextContainer.ScopeData`、作用域读取支持和此前的耀斑修复。该轮 Core、Shader Graph 等配套依赖沿用原固定提交。

这个组合包含 FraxinusRP、Core、Shader Graph 和 URP 的配套修改。Shader Graph 的版本号与官方包同为 14.0.12，但增加了 UltraRP Target 所需的内部程序集访问权限，因此应使用下列 fork。Git 必须能够访问账号下的私有仓库。

将以下条目合并进项目 `Packages/manifest.json` 的 `dependencies`，保留项目自身的其他依赖：

```json
{
  "com.bluewhitebow.fraxinus-rp": "https://github.com/bluewhitebow/FraxinusRP.git#unity2022-checkpoint-2026-10-23",
  "com.unity.render-pipelines.core": "https://github.com/bluewhitebow/Fraxinus-RPCore.git#038ad6278a1f92195b654724a78d92865a6017d5",
  "com.unity.shadergraph": "https://github.com/bluewhitebow/Fraxinus-ShaderGraph.git#ac5b57a251cf2bf170baca4c8a08c6bc08ccc87d",
  "com.fraxinus.rendering.light-transport": "https://github.com/bluewhitebow/Fraxinus-Light-Transport.git#43aa6bdedbe20762defd2b97cdc49aad035d11f5",
  "com.unity.render-pipelines.universal": "https://github.com/bluewhitebow/Fraxinus-URPExtensions.git#4293593086615bd5b5c461674a6d22cfc38fa0d9",
  "com.unity.render-pipelines.universal-config": "https://github.com/bluewhitebow/Fraxinus-URPGlobal.git#fe230634bcc018cf309105ad0f4d5ee2cf69e8ff",
  "com.unity.burst": "1.8.28",
  "com.unity.collections": "2.6.6"
}
```

这是与现有 URP 共存的安装组合。主项目验收使用本地包；上面的 Git 地址固定到该检查点源码，尚未在全新项目中另跑一次完整验收。

如果 `Packages/` 下已有相同标识的 embedded 包，它会优先于 manifest 中的 Git 依赖。继续本地开发时，可以保留本地包并检出这些提交，沿用现有的 `file:` 引用。

## 创建管线与渲染图

1. 使用 **Assets → Create → FraxinusEngine → UltraRP → UltraRP Asset** 创建管线资产。同目录会同时创建并关联 Renderer Graph；同级 **Ultra Renderer** 菜单可单独创建渲染图。
2. 打开 Renderer Graph 配置绘制队列、Layer Mask、Shader Tags 和节点连接。基础 Forward 路径使用 `RenderDrivenPass → DrawObjectsPass → PostProcessPass` 的 Camera Target 连接；绘制节点使用 `UltraForward` / `UltraForwardOnly` Shader Tags。透明物体需要对应的透明绘制节点。
3. 后处理需要连接 `ColorGradingLutPass.colorLut` 到 `PostProcessPass.colorLut`。主光阴影需要连接 `MainLightShadowCasterPass.mainLightShadow` 到接收阴影的绘制节点，并将阴影节点设为 `BeforeDrivenPass`；同时启用管线及灯光的阴影设置。详见 [HybridLighting.md](HybridLighting.md)。
4. 将管线资产指定到 **Project Settings → Graphics** 及当前 **Quality** 等级，再为场景设置灯光、相机和材质。
5. 使用 **Assets → Create → Shader Graph → UltraRP → Lit Shader Graph / Unlit Shader Graph** 创建着色器图，再为图创建材质并应用到场景物体。

当前包不发布示例场景、Package Manager 示例入口或示例生成菜单。历史测试所用的示例和工具保留在对应检查点源码中。

## 验收记录

SGSR 检查点通过 43 帧 Editor Play Mode GPU 检查、Windows Mono Player 构建及 20 帧离屏渲染，覆盖 SGSR1／SGSR2、Alpha、历史重置、运动矢量和回退配置。测试使用 Unity 2022.3.62f3／D3D11，48 份宿主和隔离项目设置已恢复。详细结果及报告哈希见 [Upscaling.md](Upscaling.md)；该轮未验证窗口呈现、IL2CPP 或其他图形 API。

2026-10-22 检查点通过 Unity 编译、六种高级示例生成、Basic Forward 的 Lit／Unlit 像素检查及四种 Inspector 配置的修复前后对照。该轮未重新构建 Player；记录见 [Validation.md](Validation.md)。

[UnifiedContextGetSummary.json](UnifiedContextGetSummary.json) 保存 `-get` 检查点的容器、渲染、GC.Alloc 正控制及管线切换结果和证据 SHA-256。该轮未重新构建 Player，预热循环的分配事件结果也不代表整帧或 IL2CPP 无分配。

[FeatureCompletionSummary.json](FeatureCompletionSummary.json) 保存前一检查点的专项渲染测试、Windows Mono 构建及离屏 Player 烟测结果。[AcceptanceSummary.json](AcceptanceSummary.json) 保留首次检查点的 12 个分组、8 个构建配置及 16 次 Player 运行统计。它们对应各自源码快照，不能作为后续改动的 Player 验收；历史 0 B 读数的更正见 [Validation.md](Validation.md)。完整日志、截图和 Player 二进制保留在原宿主项目的 `Logs/FraxinusRP` 与 `Builds`，不包含在本源码仓库中。移植验收脚本已移除，记录及对应源码仍可从历史检查点查阅。
