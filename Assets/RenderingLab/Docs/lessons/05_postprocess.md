# 5. 后处理 Volume

场景：`05_PostProcess`

## 你在编辑器里点哪里

Play 后会有 `GlobalVolume`（`LabVolumeFactory` 动态建 Profile，避免手写 YAML 坏掉）。

Override：

- Bloom：霓虹阈值，tint 偏品红。阈值太低会糊成一张粉纸。
- Tonemapping：Neutral（风格化更稳）。对比 ACES：ACES 更电影、更压高光，卡通皮肤容易脏。
- Color Adjustments：轻微对比和饱和。
- Vignette / Chromatic Aberration / Film Grain：电影感调料，移动低档应关。

本机可把 Profile 存成资产：选中 Volume → 把 Profile 拖进 `Assets/RenderingLab/Settings/`。

PC High 的抗锯齿/上采样：Pipeline Asset → Quality → Upscaling Filter → **STP**（Spatial-Temporal Post-processing）。移动档不要开 STP。

URP 后处理走 Uber post pass，吃 `Camera` HDR。LDR 手机档 Bloom 会很难看，Initialize 已把 Mobile Low 的 HDR 关掉。

## GPU 上实际发生了什么

不透明 + 透明画完，颜色 RT 进后处理栈。Bloom：亮点下采样 → 高斯/dual filter → 加回。Tonemap：HDR 映射到显示。CA：按色差扭曲 UV。Grain：噪声纹理。

Volume 有 Global 和 Local。Local 用碰撞盒做室内/室外混合。本 Demo 主场景是 Global；作业里给「走进招牌底下」加一个 Local，加重 Bloom。

## 和绝区零观感的对应

霓虹要「灯管自己发光」，Bloom 只负责晕。先把 Unlit/自发光材质亮度做对，再开 Bloom。反过来用 Bloom 造灯，一切低配灯就没了。

## 换自己模型时的检查表

- 皮肤高光不要被 Bloom 开花。
- ACES vs Neutral 各截一张，选更像你项目的。
- Mobile Low 关 Bloom 后，招牌几何本身仍是亮的。
- Rendering Debugger → Lighting 里能看到 Bloom 前后。
