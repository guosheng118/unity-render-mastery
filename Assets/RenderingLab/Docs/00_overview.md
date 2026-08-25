# 0. 总览：现在工程在干什么

## 你在编辑器里点哪里

1. `Rendering Lab > Initialize Project`（第一次必点）。
2. 打开 `Assets/RenderingLab/Scenes/00_Hub.unity`，Play。
3. 看 `Assets/RenderingLab/Settings/` 是否出现 `PCHigh_Pipeline.asset` 等 6 套资源。
4. `Project Settings > Quality` 应有 PC High … Mobile Low，每档的 Custom Render Pipeline 指向对应 Asset。
5. 左上角六个按钮切档，看阴影软硬、清晰度和整体锐度的变化。

## GPU 上实际发生了什么

切档 = 换一套 URP Asset。Asset 决定 Rendering Path（Forward+ / Forward）、Shadowmap 尺寸、cascade、软影、附加灯、HDR、Render Scale、是否 STP。

场景几何只是默认 `Universal Render Pipeline/Lit` 占位体，用来对照档位，不是最终美术。

## 接下来

灯光、GI、反射、Renderer Feature、后处理、角色/场景 Shader 都还没写。你准备学哪一块，再说一声即可。
