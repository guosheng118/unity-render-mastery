# 0. 总览：这个工程在干什么

## 你在编辑器里点哪里

1. `Rendering Lab > Initialize Project`（第一次必点）。
2. 打开 `Assets/RenderingLab/Scenes/00_Hub.unity`，Play。
3. 看 `Assets/RenderingLab/Settings/` 是否出现 `PCHigh_Pipeline.asset` 等 6 套资源。
4. `Project Settings > Quality` 应有 PC High … Mobile Low，每档的 Custom Render Pipeline 指向对应 Asset。

## GPU 上实际发生了什么

URP 用 **Render Graph** 编一帧：ShadowCaster → Depth/DepthNormals（可选）→ Forward+ 不透明（含角色 StylizedLit）→ Outline Feature 再画一遍 Character → 透明 / 后处理 Volume → 最终到 Backbuffer。

Forward+ 先把屏幕切成 tile，再给每个 tile 分配灯光索引。所以 PC 高档可以挂很多霓虹点光，而移动低档直接 `Additional Lights = Disabled`，只留主方向光。

## 和绝区零观感的对应

绝区零是高度定制的 Unity 分支。你能在官方 URP 里对齐的是 **决策** 而不是每一行自定义 SRP：

- 角色不吃环境灯的全部随机性 → Rendering Layer + 角色专用灯 + Ramp 阴影色
- 霓虹渗到皮肤 → APV / 探针，而不是把点光强度拉爆
- 湿街道 → Planar Reflection，不是指望全屏 SSR
- 高配华丽、低配可读 → 关 Feature / 降阴影 / 降 Render Scale，而不是换一套完全不同的美术

## 换自己模型时的检查表

- 工程能进 Play，HUD 能切 6 档。
- Character 层存在（`Tags & Layers`）。
- Initialize 之后 Graphics 的 Scriptable Render Pipeline 不为空。
