# Unity Render Mastery

Unity 6.3 URP 渲染实验室。当前只做一件事：**PC / Mobile 六档质量配置**。灯光、场景 Shader、角色 Shader、Renderer Feature 等后面按课再加。

管线是 **URP 17.x + Render Graph**。

## 需要的环境

- **Unity 6.3 LTS**（`ProjectSettings/ProjectVersion.txt` 为 `6000.3.x`）
- 用 Hub 打开本仓库根目录

## 第一次打开

1. Unity Hub → Open → 本仓库根目录。
2. 等 Package Manager 拉完 `com.unity.render-pipelines.universal`。
3. 菜单 **Rendering Lab > Initialize Project**。生成 6 套 URP Asset / Renderer，写进 Quality Settings。
4. 打开 `Assets/RenderingLab/Scenes/00_Hub.unity`，按 Play。
5. 左上角切档。右键拖视角，滚轮拉近。`H` 隐藏 HUD，`N` 下一档。

场景里只有默认 URP Lit 的占位体，用来看出阴影、Render Scale、附加灯开关的差别。

## 档位（Quality）

| 档 | 路径 | 阴影 | HDR | 附加灯 | STP |
| --- | --- | --- | --- | --- | --- |
| PC High | Forward+ | 4096 软影 4 cascade | 开 | 开 | 开（Inspector 核对 Upscaling Filter） |
| PC Mid | Forward+ | 2048 | 开 | 开 | 关 |
| PC Low | Forward | 1024 硬影 | 关 | 关 | 关 |
| Mobile High | Forward+ | 2048 | 开 | 开 | 关 |
| Mobile Mid | Forward | 1024 | 开 | 关 | 关 |
| Mobile Low | Forward | 关 | 关 | 关 | 关 |

运行时切档走 `QualityTierController`：换 Quality Level + URP Asset。

## 以后再写（你开口即可）

- 灯光 Rig（Key / Fill / Rim / 霓虹）
- GI / APV
- 反射（Probe / Planar）
- Renderer Feature（描边、Debug、教学 SSR）
- 后处理 Volume
- 角色 Shader（StylizedLit）
- 场景 Shader（湿地面等）

## 文档

[`Assets/RenderingLab/Docs`](Assets/RenderingLab/Docs/README.md)
