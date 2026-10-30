# Historical package integration validation / 历史包整合验证

This page preserves validation of the **original eight-package export**, before Fraxinus URP, URP Config, GPU Batcher and GPU Terrain were removed. The current repository contains four packages and centers on UltraRP. **Unity package import and C# compilation have not been rerun for that four-package layout.** The results below apply to the earlier eight-package snapshot and do not validate the current package set.

本页保留 **初次八包导出** 的验证记录，发生在移除 Fraxinus URP、URP Config、GPU Batcher 和 GPU Terrain 之前。当前仓库以 UltraRP 为中心，包含四个包，**尚未针对四包布局重新运行 Unity 包导入和 C# 编译**。下方结果仅适用于此前的八包快照，不能视为当前包集合的验收。

The historical run took place on **2026-10-30**, using **Unity 2022.3.62f3 on Windows** and the working-tree snapshot recorded in [PackageSources.json](PackageSources.json). That file remains the original source inventory, including the four removed packages; its counts and hashes describe the earlier export.

历史验证日期：**2026-10-30**。环境：**Windows / Unity 2022.3.62f3**。对应源码为 [PackageSources.json](PackageSources.json) 记录的初始工作区快照。该文件继续保留原始来源清单，包含已移除的四个包；文件数量和哈希描述的是此前导出结果。

## Original eight-package results / 原八包验证结果

| Check / 检查项 | Result / 结果 |
| --- | --- |
| Package export / 包导出 | All 13,009 package files match their source bytes, including 6,248 `.meta` files. Source working-tree edits are included. / 13,009 个包文件与源文件字节一致，包含 6,248 个 `.meta`，纳入源工作区未提交改动。 |
| Repository layout / 仓库布局 | Eight ordinary package directories; nested Git metadata and ignored build output excluded. / 八个普通包目录，已排除嵌套 Git 元数据和被忽略的编译产物。 |
| Package metadata / 包元数据 | Eight package manifests and 39 assembly definitions parse; all declared sample paths exist. / 八份包清单和 39 份程序集定义可解析，声明的示例路径均存在。 |
| Active resource GUIDs / 生效资源 GUID | No duplicate GUIDs among 4,736 active asset metadata entries; Unity-ignored directories such as `Samples~` and `Optional~` excluded. / 4,736 个生效资源 GUID 无重复，统计不含 `Samples~`、`Optional~` 等 Unity 忽略目录。 |
| UPM resolution / 包解析 | An isolated empty host project resolved all eight package IDs as local dependencies from this repository. / 独立空白宿主工程将八个包 ID 全部解析到本仓库的本地依赖。 |
| Editor compilation / 编辑器编译 | Batch-mode import and C# compilation passed; validation completed with exit code **0**. / 批处理导入及 C# 编译通过，验证完成，退出码 **0**。 |
| Source integrity after import / 导入后完整性 | All eight package content hashes still match the initial snapshot. / 八个包的内容哈希仍与初始快照一致。 |

The host project explicitly pinned Burst **1.8.28**, Collections **2.6.6** and Mathematics **1.3.2**, matching the source project's resolved versions. Verification used `-batchmode -nographics` and checked each package's actual resolved path. Unity emitted headless shader-support warnings, existing shader pragma warnings and mixed-line-ending warnings; the run was not warning-free.

Unity also reconstructed four empty directories from their existing `.meta` files (three in FraxinusRP and one in URP). No package file contents changed. Git does not store empty directories.

宿主工程显式固定 Burst **1.8.28**、Collections **2.6.6** 和 Mathematics **1.3.2**，与源工程已解析版本一致。验证使用 `-batchmode -nographics`，检查各包实际解析路径。日志包含无图形设备下的 Shader 支持警告，以及已有的 Shader pragma 和混合换行警告，并非零警告运行。

Unity 还根据已有 `.meta` 重建了四个空目录（FraxinusRP 三个、URP 一个），包文件内容没有变化。Git 不保存空目录。

That run validated local installation and C# compilation of the original eight-package snapshot. It did not validate rendered output, sample gameplay, Player builds, other platforms, or GitHub package downloads. Git installation requires the repository contents to be published first. Historical feature tests retained in individual packages keep their original scope. See the current [usage guide](Usage.md) for UltraRP's four-package dependency set.

该次验证覆盖原八包快照的本地安装与 C# 编译，未覆盖渲染画面、示例运行、Player 构建、其他平台及 GitHub 包下载。Git 安装须先发布仓库内容。各包保留的历史功能测试仍以其原始验证范围为准。当前 UltraRP 的四包依赖配置见[使用指南](Usage.zh-CN.md)。
