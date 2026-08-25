# 7. 角色 Shader 规范（StylizedLit）

场景：`07_NeonShowcase`  
文件：`Assets/RenderingLab/Shaders/StylizedLit.shader`

## 你在编辑器里点哪里

占位角色四套材质逻辑（运行时 `LabMaterials.Stylized`）：

- 衣服：Ramp + 冷阴影
- 头：`_UseFaceShadow`
- 头发：`_UseHairAniso`
- 描边宽 `_OutlineWidth`

Debug HUD：Albedo / NdotL / Shadow / GI / Rim。查问题时先看这些，再开 Rendering Debugger。

## GPU 上实际发生了什么

半兰伯特 `ndotl * 0.5 + 0.5` 把暗部抬起来，再用 `smoothstep(threshold±smooth)` 切卡通阶。阴影色乘在暗部，形成「冷调阴影、暖调受光」。

面部教学 SDF：用主光在「脸向右轴」上的投影去和一张灰度图比较。量产要：

1. DCC 里画一张沿 U 的 SDF（左黑右白或反过来）。
2. 角色头骨骼明确 Forward。
3. 用头的切线空间，而不是世界 Up 叉乘（本 Demo 为占位球简化了）。

头发：Kajiya-Kay 双高光，切线偏移 `_AnisoShift`。需要发片切线顺着发流方向。球没有发流向，高光会「围着圆转」，换模型才有意义。

GI：`SAMPLE_GI`。Rim：`1 - N·V`。附加灯：Forward+ 循环。

描边 Pass `LightMode=Outline`：沿法线在 clip 空间外扩。TAA/STP 下比后处理描边稳。

## 和绝区零观感的对应

- 脸：硬而干净的明暗交界，跟着头转，不跟着世界太阳乱跳。
- 发：两条高光带，不是 Blinn 圆点。
- 衣服：暗部有颜色，不是灰。
- 线：贴着形体，厚度按屏幕走，不随距离爆炸（本 Demo 用 clip 空间系数，仍需按角色包络微调）。

## 换自己模型时的检查表

- 头的 Pivot / Forward 对，脸阴影不会「糊在鼻子上」。
- 头发模型切线：DCC 里 Align tangent。
- 法线外扩裂开：平滑法线存 UV2。
- 角色 Layer=Character，否则没描边。
- Receive GI=Light Probes。
- 不要把 URP Lit 和 StylizedLit 混在同一套身体上还指望阴影色一致。
