# 1. 灯光搭建

场景：`01_Lighting`

## 你在编辑器里点哪里

Play 后 Hierarchy 会出现 `_LightingRig`：

- `Key_Directional`：主光，Mixed，软影。决定面部受光方向。
- `Fill_Spot`：冷色补光，压掉死黑。
- `Rim_Spot`：背光/轮廓，服务剪影。
- `Neon_*`：烘焙向点光（Mixed）。真正「染环境」靠 GI，实时只负责近处高光。
- `CharacterOnly_Point`：演示「角色专用灯」。量产时用 **Light Layer / Rendering Layer**，让这盏灯只打到 Character。

改光：选中灯，看 `Light` 组件。URP 额外数据在 `Universal Additional Light Data`（温度、Cookie、Layer）。

Rendering Layer 在 Unity 6：灯的 `Rendering Layer Mask` 与 Renderer 的 `Rendering Layer Mask` 求交。角色网格默认 Default+Character 层，见 `Project Settings > Tags and Layers > Rendering Layers`。

## GPU 上实际发生了什么

主方向光：一次全屏光照 + Shadowmap 采样（cascade）。Forward+ 附加灯：tile light list，像素循环 `LIGHT_LOOP_BEGIN`。阴影是单独的 ShadowCaster pass。

Mixed 模式：静态物体间接光进 Lightmap/APV，直接光仍可实时。动态角色没有 Lightmap UV，只能 `Receive GI = Light Probes`。

## 和绝区零观感的对应

角色「永远好看」通常是：

1. 主光方向锁在脸的 30–45°，不跟场景太阳死绑。
2. 阴影染色偏冷（本 Demo `_ShadowColor`）。
3. 轮廓光独立，强度随镜头变化。
4. 霓虹是环境，不是主光。主光一脏，脸立刻花。

## 换自己模型时的检查表

- 脸在主光下左脸亮右脸暗（或反过来），中间过渡短。
- 金属/光滑材质不要被 Fill 洗成白。
- 角色投影落在地面，没有 peter-panning（调 Bias）。
- 关掉 Rim，剪影是否还能读出形体；能，说明 Key 对了。
