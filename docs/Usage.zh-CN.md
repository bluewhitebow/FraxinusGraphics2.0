# FraxinusGraphics 2.0 使用指南

[English](Usage.md) | [简体中文](Usage.zh-CN.md) · [仓库首页](../README.zh-CN.md)

使用 **Unity 2022.3**，其中 **2022.3.62f3** 与源工程一致。本仓库是一组 UPM 包，使用它们的 Unity 工程自行提供 `Assets`、`ProjectSettings` 和 `Packages/manifest.json`。

本仓库包含 **ScriptableNode / UltraRP** 的四个配套包：Core、Light Transport、Shader Graph 和 FraxinusRP，下方配置安装这四个包。UltraRP 不依赖 URP。

各包保留自己的版本号，`2.0` 表示套件版本。请使用同一提交中的四个配套包。

## 使用本地 checkout

将本仓库与使用它的工程放在同一级目录：

```text
workspace/
├── FraxinusGraphics2.0/
│   └── Packages/
└── MyUnityProject/
    └── Packages/
        └── manifest.json
```

把以下条目合并到工程 `Packages/manifest.json` 的 `dependencies` 中，保留工程其他依赖。[manifest.local.json](manifest.local.json) 提供同样的 JSON 依赖配置。

```json
{
  "com.unity.render-pipelines.core": "file:../../FraxinusGraphics2.0/Packages/Fraxinus-RPCore",
  "com.fraxinus.rendering.light-transport": "file:../../FraxinusGraphics2.0/Packages/Fraxinus-Light-Transport",
  "com.unity.shadergraph": "file:../../FraxinusGraphics2.0/Packages/Fraxinus-ShaderGraph",
  "com.bluewhitebow.fraxinus-rp": "file:../../FraxinusGraphics2.0/Packages/FraxinusRP"
}
```

`file:` 路径相对于使用方工程的 **Packages 目录**。目录不同级时需相应调整；工程若与 `D:/Unity/FraxinusGraphics2.0` 同级，可直接使用上述相对路径。

其他官方及内置依赖由 Unity 按包声明解析。源工程已解析的共享依赖包括 Burst **1.8.28**、Collections **2.6.6** 和 Mathematics **1.3.2**。若使用方 manifest 显式锁定了更旧版本，接入时应检查这些固定版本。

## 使用 Git 安装

将以下条目合并到 `dependencies`，也可使用 [manifest.git.json](manifest.git.json)。`?path=` 用于选择仓库中的单个 UPM 包；仓库根目录本身不是一个 UPM 包。

```json
{
  "com.unity.render-pipelines.core": "https://github.com/bluewhitebow/FraxinusGraphics2.0.git?path=/Packages/Fraxinus-RPCore#main",
  "com.fraxinus.rendering.light-transport": "https://github.com/bluewhitebow/FraxinusGraphics2.0.git?path=/Packages/Fraxinus-Light-Transport#main",
  "com.unity.shadergraph": "https://github.com/bluewhitebow/FraxinusGraphics2.0.git?path=/Packages/Fraxinus-ShaderGraph#main",
  "com.bluewhitebow.fraxinus-rp": "https://github.com/bluewhitebow/FraxinusGraphics2.0.git?path=/Packages/FraxinusRP#main"
}
```

Unity 需要能够调用 Git 并访问该仓库。`#main` 跟随开发；需要可复现版本时，将**所有** `#main` 替换为同一个已发布提交哈希或标签，并将使用方工程的 lockfile 纳入版本控制。

使用方工程 `Packages/` 下同 ID 的 embedded 包优先于 manifest 引用。切换到 checkout 或 Git 依赖时，先备份本地改动，将过期的 embedded 副本移出该目录；等待 Unity 解析后，在 Package Manager 确认实际包来源。

## 配置 ScriptableNode / UltraRP

Graphics 资产与当前 Quality 覆盖决定实际运行的管线。安装 UltraRP 不会自动启用它，也不会转换已有材质或 Renderer 资产。

1. 创建 **Assets > Create > FraxinusEngine > UltraRP > UltraRP Asset**，同时会创建并关联 Renderer Graph；同级 **Ultra Renderer** 菜单可单独创建图。
2. 打开 Renderer Graph 配置节点。基础 Forward 路径使用 `RenderDrivenPass → DrawObjectsPass → PostProcessPass` 的 Camera Target 连接；绘制节点使用 `UltraForward` / `UltraForwardOnly` 标签。透明物体需要对应的透明绘制节点。
3. 将 `ColorGradingLutPass.colorLut` 接到 `PostProcessPass.colorLut` 以使用后处理。主光阴影还需连接阴影输出并配置阴影节点调度，参见[图配置说明](../Packages/FraxinusRP/Documentation~/Installation.md)及 [ScriptableNode 指南](../Packages/FraxinusRP/Documentation~/ScriptableNode.md)。
4. 在 Graphics / Quality 指定 UltraRP 资产。通过 **Assets > Create > Shader Graph > UltraRP** 创建 Lit / Unlit 图，或使用包内手写 UltraRP Shader。
5. 按功能文档配置 [Hybrid 光照](../Packages/FraxinusRP/Documentation~/HybridLighting.md)、[GTAO](../Packages/FraxinusRP/Documentation~/AmbientOcclusion.md)、[Contact Shadows / 自动曝光](../Packages/FraxinusRP/Documentation~/ContactShadowsAndExposure.md)及[超分辨率](../Packages/FraxinusRP/Documentation~/Upscaling.md)。

UltraRP 的 Target 内部 API 需要配套 Shader Graph 分支。它的版本仍为 `14.0.12`，安装同版本的官方包不会提供相同的程序集访问权限。FraxinusRP 当前没有可导入示例场景或示例生成器。

## 常见配置问题

| 现象 | 检查项 |
| --- | --- |
| 缺少 `ScopeData` 或类型化 RenderGraph API | 确认 UltraRP 与 Core 来自本仓库同一提交，并检查旧 embedded 副本是否优先生效。 |
| UltraRP Shader Graph 内部访问或 Target 编译错误 | 确认 `com.unity.shadergraph` 解析到 `Fraxinus-ShaderGraph`，而不是官方包。 |
| 找不到包或 Git 路径无效 | 检查 GitHub 连接、`?path=/Packages/...` 是否与目录名称一致、所有引用的修订是否存在。 |
| 安装后仍使用原来的管线 | 检查 Graphics 与当前 Quality 的管线资产；安装包本身不会选择管线。 |
| 材质变粉或 UltraRP 画面为空 | 检查材质对应的管线、Renderer Graph 连接及绘制标签。URP Renderer Feature 不是 UltraRP 图节点。 |
| 包内安装文档指向其他仓库或检查点 | 使用本页的 2.0 配套依赖配置。 |

[UltraRP 验证记录](../Packages/FraxinusRP/Documentation~/Validation.md)列出了测试环境和功能范围。接入时请验证项目场景、目标平台及 Player 构建。
