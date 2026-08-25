# 6. PC / 移动 高中低配

场景：`06_QualityTiers`

## 你在编辑器里点哪里

- `Project Settings > Quality`：六档名字必须和 `QualityTier` 枚举顺序一致（0=PC High … 5=Mobile Low）。
- 每档 Custom Render Pipeline 指向 `Settings/*_Pipeline.asset`。
- 运行时 HUD 六个按钮调用 `QualityTierController.Apply`。
- 目录：`QualityTierCatalog`（`Resources/QualityTierCatalog.asset`）记录 Feature 开关，给 HUD 显示，并驱动 `_LabPlanarEnabled` / `_LabOutlineEnabled`。

拆档两类手段：

1. **关 Feature / 降分辨率 / 降阴影**（TA 当天就能做，本 Demo 的主路径）。
2. **Shader 变体 + Strip Unused Variants**（包体和手机编译时间）。URP Graphics 设置里的 Strip Unused Variants 在你稳定之后再开。

## GPU 上实际发生了什么

换 URP Asset = 换 Renderer Data = 换 Rendering Path、Shadowmap 尺寸、附加灯模式、Probe 混合、是否要 Depth/Opaque 纹理。Planar 第二相机在低档直接不 Render。

移动低档附加灯 Disabled：霓虹只存在于烘焙 GI。所以 **低配必须 Bake**，否则场景会变成只有一盏方向光的灰坑。

## 和绝区零观感的对应

高配：倒影、软影、多灯、STP 抗锯齿。低配：剪影、脸的明暗、招牌颜色还在。可读性优先于倒影。

建议预算（经验，不是硬指标）：

- PC High：4K 影、Forward+、8 附加灯、Planar 1.0、SSAO、Bloom、STP
- Mobile High：2K 影、1 盏角色实时灯 + 烘焙霓虹、无 Planar、无 STP
- Mobile Low：无影或硬影、无 Bloom、无描边、Render Scale 0.6

## 换自己模型时的检查表

- 六档都能进，无粉红 Shader。
- 同机位三档截图，脸的明暗形状一致，只是软硬和反射不同。
- 包体：Android 打一次 Development Build，看 Frame Debugger 里还有没有不该存在的 SSAO。
- 不要用 `QualitySettings.pixelLightCount` 当 URP 主控，URP 看 Asset 的 Additional Lights。
