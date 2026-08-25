# Unity Render Mastery

Unity 6.3 URP「渲染实验室」：给 **3D 美术转 TA** 用的可运行 Demo。对照《绝区零》的风格化品质（角色卡通光影 + 霓虹环境 + 湿地面），系统过一遍灯光、APV GI、反射、Renderer Feature、后处理，以及 **PC / 移动端 高中低** 六档配置。

这不是绝区零引擎复刻，也不是 HDRP 电影向项目。管线是 **URP 17.x + Render Graph**，因为你同时要 PC 和手机档。

## 需要的环境

- **Unity 6.3 LTS**（编辑器版本 `6000.3.x`，本仓库 `ProjectSettings/ProjectVersion.txt` 写的是 `6000.3.0f1`）
- Windows / macOS 均可。用 Hub 打开本仓库根目录。
- 第一次打开会编译 URP 包，并自动跑菜单 **Rendering Lab > Initialize Project**（若 Graphics 里还没有 Pipeline Asset）。也可手动点一次。

## 第一次打开

1. Unity Hub → Open → 选择本仓库根目录。
2. 等 Package Manager 拉完 `com.unity.render-pipelines.universal`。
3. 菜单 **Rendering Lab > Initialize Project**。它会生成 6 套 URP Asset / Renderer，写进 Quality Settings，并挂上 Outline / Planar / Debug Feature。
4. 打开 `Assets/RenderingLab/Scenes/00_Hub.unity`，按 Play。
5. 左上角切档位和场景。右键拖视角，滚轮拉近。`H` 隐藏 HUD，`N` 下一档。

场景几何是运行时搭的占位体（角色胶囊 + 霓虹街道）。**把你自己的 FBX 拖到 `CharacterSlot` / `EnvironmentSlot` 即可**，不必用仓库里的盒子。

## 场景地图

| 场景 | 学什么 |
| --- | --- |
| `00_Hub` | 导航、档位、占位角色 |
| `01_Lighting` | Key / Fill / Rim / 霓虹 / 角色专用灯 |
| `02_GI_APV` | Lightmap + Adaptive Probe Volumes 工作流（需在 Editor 里 Bake） |
| `03_Reflections` | Box Projection Probe + 湿地面 Planar |
| `04_RendererFeatures` | Render Graph：描边、Planar 槽位、Debug、教学 SSR |
| `05_PostProcess` | Volume：Bloom / Tonemap / CA / Grain |
| `06_QualityTiers` | 六档对照 |
| `07_NeonShowcase` | 主切片，全部叠在一起 |

课程正文在 [`Assets/RenderingLab/Docs`](Assets/RenderingLab/Docs/README.md)。

## 档位（Quality）

| 档 | 路径 | 阴影 | Planar | SSAO | Bloom | STP |
| --- | --- | --- | --- | --- | --- | --- |
| PC High | Forward+ | 4096 软影 4 cascade | 全分辨率 | 开 | 开 | 开（Inspector 核对 Upscaling Filter） |
| PC Mid | Forward+ | 2048 | 半分辨率 | 开 | 开 | 关 |
| PC Low | Forward | 1024 硬影 | 关 | 关 | 弱 | 关 |
| Mobile High | Forward+ | 2048 | 关 | 开 | 低阈值 | 关 |
| Mobile Mid | Forward | 1024 | 关 | 关 | 极简 | 关 |
| Mobile Low | Forward | 关 | 关 | 关 | 关 | 关 |

运行时切档走 `QualityTierController`：换 Quality Level + URP Asset + 全局 Shader 开关。

## 角色材质（你来换模型时）

Shader：`RenderingLab/StylizedLit`

- 身体：半兰伯特 + 冷色阴影 `_ShadowColor` + Ramp
- 头：勾 `_UseFaceShadow`，塞面部 SDF/梯度图（教学版，不是完整米哈游方案）
- 头发：勾 `_UseHairAniso`
- 描边：Outline Pass，由 `OutlineRendererFeature` 画 Character 层
- Receive GI：**Light Probes**（吃 APV）
- Layer：`Character`

平滑法线做描边：把平滑后的法线写入 UV2 或 Tangent，再改 `OutlinePass.hlsl` 去读。占位球用的是网格法线，硬边会裂，这是作业而不是 bug。

## 本环境做不到的事

云端 Agent **没有 Unity Editor**，因此：

- 打开工程后必须在本机 **Bake GI**（Lighting 窗口，Mixed + APV）
- URP Asset 由 Initialize 菜单生成，不要指望手写 YAML 里已经有 6 套完整 Renderer Data
- STP 枚举值请在 Pipeline Asset 的 Upscaling Filter 里确认选中 **STP**（仅 PC High）

## 文档

从 [`Assets/RenderingLab/Docs/README.md`](Assets/RenderingLab/Docs/README.md) 按课往下看。每课四段：编辑器点哪里、GPU 上发生什么、和绝区零观感的对应、换自己模型的检查表。
