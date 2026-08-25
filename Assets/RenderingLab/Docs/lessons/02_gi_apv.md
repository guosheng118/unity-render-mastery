# 2. Unity 6.3 GI：Lightmap + Adaptive Probe Volumes

场景：`02_GI_APV`

云端交的工程 **没有烘焙数据**。你必须在本机 Bake。

## 你在编辑器里点哪里

1. URP Asset → Lighting → Light Probe Lighting → **Adaptive Probe Volumes**（Initialize 会尽量写成 APV）。
2. `Window > Rendering > Lighting`：
   - Mixed Lighting：开 Baked Global Illumination
   - Lighting Mode：Baked Indirect 或 Shadowmask（本 Demo 建议 Baked Indirect）
   - Adaptive Probe Volumes 页：Baking = Single Scene，调 Min/Max Probe Spacing
3. 菜单 `Light > Adaptive Probe Volume`，Mode = Global（或包住街道的 Local）。
4. 环境网格：Static + Contribute Global Illumination；Receive = Lightmaps。
5. 角色：Non-static；Receive = **Light Probes**。
6. 点 Generate Lighting。

场景里的 `AdaptiveProbeVolume_Placeholder` 只是 Gizmo，告诉你体积该有多大。真正的 APV 对象必须用菜单创建。

漏光：加 `Probe Adjustment Volume` 把墙后探针 Invalidate。天空不进室内：开 Sky Occlusion。角色发灰：探针太稀或没 Bake，或角色仍在吃 Lightmap。

## GPU 上实际发生了什么

静态：纹素存间接辐照，顶点/像素采 Lightmap。动态：世界空间位置去 APV 3D 纹理里取 SH（Unity 6 用 probe atlas）。`SAMPLE_GI` 在 StylizedLit 里把这块加到漫反射。

APV 比旧 Probe Group 密、能局部加密、能 Scenario Blend。旧 Group 只在文档里当历史，不要当新项目主路径。

## 和绝区零观感的对应

霓虹巷：静态墙壁要有间接红/青，角色走过时皮肤也要带一点环境色。这是探针，不是后处理。后处理只能统一染色，分不出「走到招牌底下」。

## 换自己模型时的检查表

- Lighting 窗口没有红色错误。
- 角色移动时亮度连续，没有一格一格跳（加密探针或开 APV 环绕）。
- 关掉所有实时附加灯，角色仍能被环境微微照亮。
- 墙缝没有亮探针喷出来。
