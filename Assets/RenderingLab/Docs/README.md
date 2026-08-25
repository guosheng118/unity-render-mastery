# 课程目录（美术 TA）

目标：能在 Unity 6.3 URP 上独立完成「角色能上屏、环境能烘焙、档位能拆、Feature 能写」的 TA 工作，而不是只会调材质球。

品质参照《绝区零》：风格化 PBR + NPR、角色分离光照、霓虹反弹、湿地面反射、Bloom 控制、跨端拆档。不是像素级还原。

## 怎么学

1. 先 Play `07_NeonShowcase`，用左上角六档来回切，记住「关什么会变平」。
2. 按课打开对应场景，对照本文。
3. 作业统一：导入你自己的角色，三档截图（PC High / Mobile High / Mobile Low）。

## 课表

| 课 | 文件 | 场景 |
| --- | --- | --- |
| 0 总览与工程 | [00_overview.md](00_overview.md) | `00_Hub` |
| 1 灯光搭建 | [lessons/01_lighting.md](lessons/01_lighting.md) | `01_Lighting` |
| 2 Unity 6.3 GI / APV | [lessons/02_gi_apv.md](lessons/02_gi_apv.md) | `02_GI_APV` |
| 3 反射 | [lessons/03_reflections.md](lessons/03_reflections.md) | `03_Reflections` |
| 4 Renderer Feature + Render Graph | [lessons/04_renderer_features.md](lessons/04_renderer_features.md) | `04_RendererFeatures` |
| 5 后处理 Volume | [lessons/05_postprocess.md](lessons/05_postprocess.md) | `05_PostProcess` |
| 6 跨端高中低配 | [lessons/06_quality_tiers.md](lessons/06_quality_tiers.md) | `06_QualityTiers` |
| 7 角色 Shader 规范 | [lessons/07_character_shader.md](lessons/07_character_shader.md) | `07_NeonShowcase` |
| 8 换模型作业 | [lessons/08_swap_your_fbx.md](lessons/08_swap_your_fbx.md) | 任意 |

## 官方菜单速查（Unity 6.3）

- Quality / URP Asset：`Edit > Project Settings > Quality`，双击当前 Pipeline Asset
- Graphics：`Edit > Project Settings > Graphics`
- 烘焙：`Window > Rendering > Lighting`
- 帧调试：`Window > Analysis > Rendering Debugger` 与 `Frame Debugger`
- Volume：场景里 `GlobalVolume`，Inspector 里 Add Override

## 不要走的弯路

- 不要为了「更像电影」去开 HDRP，移动档会直接断掉。
- 不要在 6.3 上死等官方 SSR（预计更后面的版本）。湿地面用 Planar，室内用 Box Probe。
- 不要用旧 Light Probe Group 当主 GI。角色吃 **APV**。
- 不要把描边做成全屏后处理描边当唯一方案；角色用反向外壳 / 法线外扩，便宜且稳。
