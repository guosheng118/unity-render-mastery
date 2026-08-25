# 4. Renderer Feature 与 Render Graph

场景：`04_RendererFeatures`

## 你在编辑器里点哪里

1. 选中 `Assets/RenderingLab/Settings/PCHigh_Renderer.asset`。
2. Inspector 里 Renderer Features 列表应有：
   - `OutlineRendererFeature`
   - `PlanarReflectionRendererFeature`（文档槽位）
   - `DebugBufferRendererFeature`
   - `SimpleSsrRendererFeature`（默认关）
   - SSAO（Initialize 时若类型存在会加）
3. 代码在 `Assets/RenderingLab/RendererFeatures/`。
4. Play 后 HUD 的 Debug Buffer 按钮：Albedo / NdotL / Shadow / GI / Rim。这些颜色来自 `StylizedLit` 的 `_LabDebugMode`，Feature 只画顶栏色条，演示如何挂一帧 RG pass。

## GPU 上实际发生了什么

Unity 6.3 默认 Render Graph。你的 Feature `AddRenderPasses` 只是排队；真正录命令在 `ScriptableRenderPass.RecordRenderGraph`。

Outline pass：`CreateRendererList` + `DrawRendererList`，ShaderTag `Outline`，过滤 Character 层。不要再实现已弃用的 `Execute(ScriptableRenderContext)` 当作新项目模板。

Planar 为什么不完全在 RG 里画第二相机：第二视图需要独立 culling / 翻转 / 斜裁剪。6.3 上稳妥做法仍是镜像 `Camera.Render()`，Feature 负责档位开关。等官方 SSR 成熟后再把屏幕空间反射收进 RG。

复制模板时记住：

- `builder.UseRendererList`
- `SetRenderAttachment` / `SetRenderAttachmentDepth`
- `SetRenderFunc` 里只执行绘制，不要在 lambda 里分配托管内存

## 和绝区零观感的对应

描边是角色可读性，不是后处理滤镜。全屏 Sobel 在运动模糊和 TAA 下会抖、会描到场景电线。角色第二 Pass 外扩便宜，移动端也能留。

## 换自己模型时的检查表

- Character 层对，Outline 才看得到。
- 硬边模型描边断裂 → 导出平滑法线到 UV2，改 `OutlinePass.hlsl`。
- Rendering Debugger 能看到 Outline pass 名称 `Lab Outline`。
- 低档 `_LabOutlineEnabled=0` 时描边消失。
