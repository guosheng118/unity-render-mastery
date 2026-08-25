# 6. PC / 移动 高中低配

场景：`00_Hub`（当前唯一落地的课）

## 你在编辑器里点哪里

- `Project Settings > Quality`：六档名字必须和 `QualityTier` 枚举顺序一致（0=PC High … 5=Mobile Low）。
- 每档 Custom Render Pipeline 指向 `Settings/*_Pipeline.asset`。
- 运行时 HUD 六个按钮调用 `QualityTierController.Apply`。
- 目录：`QualityTierCatalog`（`Resources/QualityTierCatalog.asset`）给 HUD 显示用。

拆档现在只用 URP Asset：

- 关附加灯 / 降阴影 / 降 Render Scale / 关 HDR / 关软影
- PC High 的 Upscaling Filter 选 STP（在 Pipeline Asset Inspector 里核对）

Shader 变体剥离、自定义 Feature 开关以后再加。

## GPU 上实际发生了什么

换 URP Asset = 换 Renderer Data = 换 Rendering Path、Shadowmap 尺寸、附加灯模式、Probe 混合、是否要 Depth/Opaque 纹理。

## 和绝区零观感的对应（先记决策）

高配：软影、多 cascade、Forward+ 能挂更多灯。低配：硬影或没影、分辨率更低、只留主方向光。可读性优先。

## 检查表

- 六档都能进，无粉红材质（现在用的是 URP 自带 Lit）。
- 同机位切档，影子边缘和整体锐度应能看出差别。
- 不要用 `QualitySettings.pixelLightCount` 当 URP 主控，URP 看 Asset 的 Additional Lights。
