# 原神角色展示案例 · 聊天记录

- 导出于：2026-09-19 10:46（本地时间）
- 用户发言：246 条
- 助手回复：350 条
- 已去掉工具调用和内部过程，只保留对话正文。

---

## 1. 你（Wednesday, Aug 26, 2026, 12:55 AM (UTC+8)）

现在这个工程有报错，帮我修复一下；

### 助手

先看工程状态和现有报错，再定位该改哪里。

### 助手

接下来会从 Unity 编辑器日志里把具体报错捞出来。

### 助手

报错集中在 URP Asset 的几个属性变成只读。我先对照工程代码和 URP 17 的新 API 再改。

### 助手

原因已经明确：URP 17 里这几个属性改成了 `internal set`，编辑器脚本不能再直接赋值。接下来改成用 `SerializedObject` 写入对应序列化字段。

### 助手

编译错误已经消掉了。接下来处理 `OutlineRendererFeature` 在构造阶段调用 `Shader.TagToID` 的运行时异常。

### 助手

`Create()` 会在序列化时被调用，所以 `ShaderTagId` 不能在那里创建。改成在真正录制 Render Graph 时再初始化。

### 助手

工程里的报错已经修好，Unity 也重新编译通过了。

**原因有两处：**

1. **编译错误（CS0200）**  
   Unity 6.3 / URP 17 里，`UniversalRenderPipelineAsset` 的阴影、附加光、反射探针相关属性改成了只能内部赋值。`RenderingLabProjectSetup.cs` 还在直接写这些属性，编辑器脚本编不过。

2. **运行时异常（TagToID）**  
   `OutlineRendererFeature` 在类型初始化时就创建了 `ShaderTagId`。Unity 6 不允许在 ScriptableObject 构造/序列化阶段调用 `Shader.TagToID`，第一次初始化项目时会炸掉。

**改动：**
- 初始化管线时，改为通过 `SerializedObject` 写对应序列化字段
- 描边 Pass 改到真正录制 Render Graph 时再创建 `ShaderTagId`

当前 Console 里原先的编译错误和 `TagToID` 异常都已经没有了。如果编辑器还停在 Safe Mode，点 **Exit Safe Mode** 即可。之后可以走菜单 **Rendering Lab > Initialize Project**，再打开 `00_Hub` 进 Play。

## 2. 你（Wednesday, Aug 26, 2026, 1:08 AM (UTC+8)）

StylizedLit这个shader还有报错

### 助手

先看 `StylizedLit` 的 shader 报错和相关文件。

### 助手

报错是 `ShiftTangent` 和 URP 重名，以及 `_FORWARD_PLUS` 关键字过时。我对照 URP 17 的定义改这两个点。

### 助手

URP 已经带了同名的 `ShiftTangent`，本地再定义会冲突。我会删掉重复定义，并把过时的 `_FORWARD_PLUS` 换成 `_CLUSTER_LIGHT_LOOP`。

### 助手

`StylizedLit` 的报错已经改好了。

**错误：** `redefinition of 'ShiftTangent'`  
URP 的 `BSDF.hlsl` 里已经有同名函数，`StylizedLitForward.hlsl` 又定义了一次，冲突了。本地那份已经删掉，头发各向异性高光直接用 URP 自带的。

**警告：** `_FORWARD_PLUS` 在 URP 17 里过时了，已换成 `_CLUSTER_LIGHT_LOOP`（`WetGround` 一并改了）。

Unity 这边如果还在 Play Mode（而且关了 Domain Reload），Console 里可能还是旧报错。退出 Play，或在 Project 窗口对 `StylizedLit.shader` 右键 **Reimport**，就会按新代码重新编译。

## 3. 你（Wednesday, Aug 26, 2026, 1:14 AM (UTC+8)）

StylizedLit，这个shader还是有报错；不过先不用管他；现在这个工程你已经帮我把角色的shader以及一些renderfeature写好是吗？但是我不想这样，我只要先有几个针对PC和Mobile的不同的配置，剩下的关于光照，场景shader，角色shader，renderfeature等，等我需要的时候再帮我写吧，这样更利于我学习；

### 助手

明白了：先只保留 PC / Mobile 分档，角色 shader、场景 shader、光照和 Renderer Feature 先拿掉，等你需要时再写。我先看现在工程里哪些是分档相关、哪些可以先撤。

### 助手

接下来会把工程收成「六档 URP 配置 + 一个默认材质测试场」，自定义 shader / Feature / 灯光先拿掉。

### 助手

删除会走单文件接口，避免一次清太多。

### 助手

对，之前是把角色 Shader、灯光、场景 Shader、Renderer Feature 一次性写进去了。按你的学习节奏，这些都先拿掉了。

**现在工程只保留六档质量配置：**

| 档 | 路径 | 阴影 | HDR | 附加灯 | STP |
| --- | --- | --- | --- | --- | --- |
| PC High / Mid / Low | Forward+ / Forward+ / Forward | 4096 / 2048 / 1024 | 高档开 | 高档开 | 仅 PC High |
| Mobile High / Mid / Low | Forward+ / Forward / Forward | 2048 / 1024 / 关 | 低档关 | 仅 High 开 | 关 |

Play `00_Hub` 后，左上角切档。场景里是默认 URP Lit 的地面和几个几何体，用来对照阴影和清晰度，不是最终美术。

**先拿掉、等你开口再写的：**
- 角色 Shader（StylizedLit）
- 场景 Shader（湿地面等）
- 灯光 Rig
- Renderer Feature（描边、Planar、SSR、Debug）
- 后处理 Volume、GI / APV

请再点一次菜单 **Rendering Lab > Initialize Project**，清掉旧 Renderer 上残留的 Feature，然后打开 `00_Hub` 进 Play。

你准备学哪一块（灯光、GI、反射、Feature、后处理、角色 Shader），直接说就行。

## 4. 你（Tuesday, Sep 1, 2026, 8:01 PM (UTC+8)）

我在这个Unity工程里新建了一个名为@Assets/RenderingLab/Cha/Yuanshen/YuanShenChara.unity 的场景，我准备用这个场景来还原原神的选角界面的角色shader，你可以先帮我整理一下思路，写不用写代码，等我准备好模型；

### 助手

先看你新建的场景和工程现状，再按「原神选角界面」把角色 shader 的思路整理成可执行的学习顺序，先不写代码。

### 助手

场景和资源我看过了。`YuanShenChara` 里已经有相机、平行光、地面，以及 `Pole` 的 Face / Hair / Body 三套贴图。材质现在还是 **URP Lit**，这没问题——选角界面那套观感，本来就不是 Lit 能调出来的。

下面只整理思路，不写代码。目标也先收窄：**还原「原神选角界面」里角色怎么被照、怎么上色**，不是还原大世界（那套要吃环境光、APV、地面反弹）。

---

## 1. 先把目标说清楚

选角界面本质是一座**小型摄影棚**：

- 背景简单、几乎不贡献光照
- 角色自己转，灯相对稳定
- 脸的明暗形状要「画出来」的，不能跟头转乱飘
- 头发要有那两道扫过的高光
- 衣服有分层的阴影色，金属/丝绸有一点高光，但整体仍是卡通

所以这课要拆成两块，不要搅在一个 Shader 里：

| 块 | 管什么 | 现在场景里 |
| --- | --- | --- |
| 摄影棚 | 相机、Key / Fill / Rim、背景、角色朝向 | 已有 Camera + 一盏 Directional Light，够起步 |
| 角色 Shader | 怎么解释「这束光打在脸上/头发/衣服上」 | 还没有，三套材质仍是 Lit |

大世界 GI、湿地面、Bloom、档位拆 Feature，都先别进这个场景。

---

## 2. 你这套资源其实已经是「原神打包法」

贴图命名已经把分工写死了，后面写 Shader 就按这个读，不要再按 PBR 去猜：

**脸**

- `*_Face_Diffuse`：颜色（sRGB）
- `*_Face_Shadow`：面部 SDF / 阴影梯度（**数据图，必须线性**）

**身体 / 头发**

- `*_Diffuse`：颜色（sRGB）
- `*_Lightmap`：不是 Unity 烘焙 Lightmap，是 **ILM 通道包**（AO、高光强度、高光锐度、Ramp 分区等）

当前这两张数据图的导入还是 `sRGBTexture: 1`。等你准备动手时，第一件小事就是：Diffuse 保持 sRGB，`Face_Shadow` 和两张 `Lightmap` 改成 **sRGB 关掉**。通道看错，后面脸和衣服怎么调都不对。

材质也已经按原神习惯拆好了：`Pole_Face` / `Pole_Hair` / `Pole_Body`。后面用**同一个 Shader、三个 Keyword/开关**即可，不必一上来写三个文件。

动手前用 Channel 视图自己核对一遍 Lightmap 的 R/G/B/A 各干什么（不同时期资源会略有差别），不要死记某篇教程的通道表。

---

## 3. 选角界面在 GPU 上实际在算什么

角色不吃「场景里随便一盏灯的 Lambert」，而是：

1. **主光方向**（选角里通常就是那盏角色灯，甚至可以是一个空物体的 forward）
2. **身体**：半兰伯特或阈值 → 乘 Lightmap 里的 AO/阈值 → 用 Ramp 或冷色阴影色，得到「亮部 / 暗部」两块，而不是光滑的灰阶
3. **脸**：不用模型法线当主阴影。用光在「脸前方/右方」平面上的投影，去采样 `Face_Shadow`，得到鼻子、脸颊那块画好的阴影
4. **头发**：切线空间各向异性高光（通常两层，错开），再被 Diffuse/Lightmap 压住
5. **描边**：Cull Front 的外壳外扩，颜色偏暗；选角近景对描边宽度很敏感
6. **Rim**：背光侧一层，强度要受阴影控制，否则暗部也会「发光」

URP Lit 做的是金属度/光滑度的微表面反射。原神角色做的是 **「阴影形状 + 材质分区 + 少量高光」**。这是两套语言。

---

## 4. 建议的学习顺序（一次只加一样）

按这个顺序，每一步都能单独对照截图，不会一上来就被通道和描边淹没。

**第 0 步：当摄影棚用（现在就能做）**

- 角色放在相机正前方，地面只当脚底参考，不要当环境光
- 平行光当 Key：略侧、略俯，先把脸的大明暗摆对
- 先关环境反射、先不要烘焙
- 转一圈角色，用 Lit 看清楚：鼻子、锁骨、头发体积现在是「灰」的——后面 Shader 就是要把这些变成「块」

**第 1 步：身体卡通底**

- 半兰伯特 + 阴影染色 + 软硬阈值
- 还不管脸、不管头发、不管描边
- 验收：衣服是两块颜色，不是 PBR 过渡

**第 2 步：接 Lightmap**

- 把 AO / 高光遮罩 / Ramp 分区接上
- 验收：缝、褶皱、金属边应和原画贴图对得上，而不是整片一样暗

**第 3 步：脸 SDF**

- 单独开关，只打在 Face 材质
- 要一个「脸朝前」方向（绑在头骨上，不要用世界 Z）
- 验收：转头或转灯时，鼻侧阴影跟着「画好的形状」走，而不是球状 Lambert

**第 4 步：头发各向异性**

- 只打在 Hair
- 先一层高光，稳定后再加第二层
- 验收：绕角色转一圈，高光沿发丝扫过，不是一个白点贴在头上

**第 5 步：描边**

- 近景选角对描边最挑：硬边会裂、远近粗细会跳
- 先用网格法线外扩能看；要干净再考虑平滑法线进 UV/切线
- 验收：剪影完整，手、脸不被描边吃掉

**第 6 步：Rim + 选角灯**

- Shader 里的 Rim 和场景里的 Rim 灯分开想：一个是公式，一个是光
- Fill 很弱，只把暗部抬到能看清衣服颜色
- 验收：和游戏里选角一样「亮、干净、暗部仍是冷色」，而不是曝光的 PBR 人偶

到第 6 步，这个场景作为「选角界面还原」就可以停。金属丝袜、眼睛、描边平滑法线、表情、UI 背景，都算下一层。

---

## 5. 模型这边你可以先核对的清单

你说模型还要再准备，准备时按这个查就行：

- 网格已按 Face / Hair / Body（或 Cloth）拆好，和三套材质对应
- 法线是平滑过的（尤其头、头发）。硬边多，描边和头发高光都会碎
- 脸朝前方向明确：角色 T-pose/A-pose 时 +Z 或 +X 朝哪，后面 SDF 要用
- 头发有可用的切线（各向异性依赖 Tangent）
- 不要把 `Face_Shadow`、`Lightmap` 当颜色图开 sRGB
- 选角场景里角色 **Receive GI 先关掉或只留 Light Probes 但场景不烘**，避免环境光把卡通阴影洗灰

`Pole` 这套已经能当第一只试验体。第二只模型进来时，应是换贴图和朝向，而不是换一套 Shader。

---

## 6. 刻意先不要做的

- 不要在这个场景里上 APV / 场景反射 / 湿地面
- 不要指望调 URP Lit 的 Metallic/Smoothness 靠近原神
- 不要一上来写完整米哈游 Shader Graph 百科全书；通道没对上，功能越多越难查
- 不要把「选角」和「大世界」做成同一个光照假设——大世界是后面另一课

---

你这边模型/朝向确认好了告诉我。下一步我建议从 **第 1 步身体卡通底** 开始写，Face / Hair 先仍用颜色，方便你对照 Lightmap 和 SDF 是哪一步加上去的。

## 5. 你（Tuesday, Sep 1, 2026, 8:19 PM (UTC+8)）

我已经准备好了，现在角色是朝向+x，头部目前没有骨骼，所以用SDF做脸部阴影的时候，可以认为是朝向+x的轴；我希望身体，头发和脸分辨做成不同的shader，还新增了一个Avatar_Girl_Pole_Hutao_Tex_Body_Shadow_Ramp的贴图；你先不用帮我直接在Unity里面写shader，你可以一步步告诉我怎么写，我会按照你给你的教程，自己手动写shader；我这里还没有脸部的sdf，元神的脸应该是有一张黑白渐变的sdf吧

### 助手

先核对你现在的贴图（尤其 Ramp 和那张 Face_Shadow），再按身体 / 头发 / 脸三套 Shader 给你一份能自己动手写的步骤。

### 助手

先把贴图说清楚，再按 **身体 → 头发 → 脸** 给你一份能自己敲的教程。代码只作为「你照着写」的参考，我不会在工程里建 Shader。

---

## 先回答：脸部 SDF 是什么，你现在有没有

原神脸上**确实有一张灰度图**，用来决定「这块皮肤多容易进阴影」。常见叫法是 Face Shadow / Face Lightmap / SDF，看起来大致是：

- 正面看一张脸的灰度
- 额头、颊前偏亮，鼻侧、下颌、眼窝偏暗
- 是**连续渐变**，不是纯黑白剪影

你文件夹里的 `Avatar_Girl_Pole_Prototype_Tex_Face_Shadow.png` **不是这张图**。它是两个白色眼形贴在黑底上，更像 **眼睛遮罩**（阴影不要画进眼球）。脸部卡通阴影暂时做不出来，不是 Shader 写错了，是缺图。

后面脸 Shader 可以先搭好骨架：没 SDF 时用普通半兰伯特顶上；有图再换成采样。临时 SDF 怎么画，放在最后一节。

---

## 贴图在 Inspector 里先改对

写代码之前改导入，否则通道全是脏的。

| 贴图 | sRGB | 用途 |
| --- | --- | --- |
| `*_Diffuse` | **勾选** | 颜色 |
| `*_Lightmap` | **关掉** | 数据（AO / 高光 / 分区） |
| `*_Shadow_Ramp` | **勾选** | 阴影颜色条（是颜色） |
| `*_Face_Shadow`（等你有真 SDF） | **关掉** | 数据 |
| 现在这张眼睛白块 | 可先关 sRGB | 以后当「眼睛不受脸阴影」 |

Lightmap、Ramp 用 **Point 或 Bilinear** 都可以；Ramp 的 Wrap 用 **Clamp**，避免 U=0/1 时绕到另一头。

Ramp 你这张胡桃的是 **横向 4 条**：上→下大约是皮肤暖影、中性灰、冷紫、粉紫。采样时：

- **U**：亮→暗（光照结果）
- **V**：选第几条（0.125 / 0.375 / 0.625 / 0.875）

Lightmap **不要背公式**。在 Inspector 里分别只看 R、G、B、A，自己记一版：

- 哪张像缝里的脏污/AO → 多半乘在漫反射或用来推阴影阈值  
- 哪张像亮边、金属、花纹高光 → 多半是高光强度  
- 哪张大块平涂不同灰 → 可能是 Ramp 行号  

下面公式里我用社区里较常见的记法：`R=AO/阈值，G=高光强度，B=高光锐度，A=Ramp行`。你对完通道如果对不上，只改 `.r/.g/.b/.a`，别改整套结构。

角色朝向 **+X**，没头骨：脸前向就当成世界空间 `(1,0,0)`，上方向 `(0,1,0)`。

---

## 三个 Shader 怎么拆

建三个文件即可，例如：

- `GenshinBody.shader` → `RenderingLab/Genshin/Body`
- `GenshinHair.shader` → `RenderingLab/Genshin/Hair`
- `GenshinFace.shader` → `RenderingLab/Genshin/Face`

都只写 **一个 Forward 通道**（`LightMode = UniversalForward`），选角场景一盏主方向光就够，先不要 `_ADDITIONAL_LIGHTS`、不要 GI。

每个文件骨架相同，你先抄这一段结构，再往 Frag 里填公式：

```hlsl
Shader "RenderingLab/Genshin/Body"
{
    Properties { /* 贴图和参数 */ }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            // Vert / Frag
            ENDHLSL
        }
    }
}
```

Vert 三个 Shader 都可以一样：算出 `positionCS、positionWS、normalWS、uv`。用 `GetVertexPositionInputs` / `GetVertexNormalInputs`。头发稍后会用到 `tangentWS`，Body/Face 第一版可以不要切线。

主光这样拿（URP 6）：

```hlsl
Light mainLight = GetMainLight();
float3 L = normalize(mainLight.direction);
float3 N = normalize(i.normalWS);
float3 V = normalize(GetWorldSpaceViewDir(i.positionWS));
```

---

## 身体 Shader：分 4 小步，每步都能看图

Properties 建议：`_BaseMap`、`_LightMap`、`_RampMap`，再加 `_RampY`（0~1，先手调选第几行）、`_ShadowThreshold`、`_ShadowSmooth`。

### 第 1 步：颜色能亮起来

Frag 里只做：

```hlsl
half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
half ndotl = dot(N, L) * 0.5h + 0.5h;   // 半兰伯特，0~1
return half4(albedo.rgb * ndotl * mainLight.color, 1);
```

验收：转灯，衣服是圆滚滚的灰阶，还完全不像原神。这一步只证明 URP 主光和 UV 是通的。

### 第 2 步：用 Ramp 把灰阶变成「两块颜色」

```hlsl
float2 rampUV = float2(ndotl, _RampY); // _RampY 先试 0.125、0.375、0.625、0.875
half3 ramp = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, rampUV).rgb;
return half4(albedo.rgb * ramp * mainLight.color, 1);
```

验收：暗部应是 Ramp 右侧那种染色（皮肤偏暖、布料偏冷紫），不是脏灰。U 仍是 ndotl 的话，过渡可能偏软，下一步用阈值收硬。

### 第 3 步：阈值 + Lightmap.R，阴影形状跟贴图走

```hlsl
half4 ilm = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, i.uv);
half ao = ilm.r;                         // 对完通道再改
half lambert = dot(N, L) * 0.5h + 0.5h;
half shadow = smoothstep(
    _ShadowThreshold - _ShadowSmooth,
    _ShadowThreshold + _ShadowSmooth,
    lambert * ao);
float2 rampUV = float2(shadow, _RampY);
half3 ramp = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, rampUV).rgb;
half3 color = albedo.rgb * ramp * mainLight.color;
```

验收：褶皱、夹缝应比周围先变暗；转角色时是「块」在挪，不是塑料球。若整件衣服同时黑/同时亮，多半是 R 不是 AO，换 `.g` 试。

`_RampY` 稳定后，可改成 `ilm.a * 0.45 + 0.02` 这类自动选行（4 条 Ramp 时，A 通道常是分区）。先手调，再自动化。

### 第 4 步：衣服高光（可选，身体有了再加）

```hlsl
half specMask = ilm.g;
half specPower = lerp(8.h, 64.h, ilm.b);
half3 H = normalize(V + L);
half spec = pow(saturate(dot(N, H)), specPower) * specMask;
color += spec * mainLight.color;
```

验收：金属扣、花纹边有一小条高光；皮肤行不应爆白。爆白就把 mask 换通道或乘小系数。

身体先停在这里。描边以后单独做，不要和 Ramp 搅在一起。

---

## 头发 Shader：身体抄过来，换高光

第一版可以 **同样 Ramp 漫反射**（头发也有 Lightmap），高光改成各向异性。

Vert 加上 `tangentOS`，传 `tangentWS`。Frag 里：

```hlsl
float3 T = normalize(i.tangentWS.xyz);
float3 B = normalize(cross(N, T)); // 用不用 B 看你高光方向；发丝高光常用 T
```

**不要自己再写一个叫 `ShiftTangent` 的函数**——URP 的 `Lighting.hlsl` 里已经有同名函数，重名会编不过。直接用，或自己改名叫 `ShiftHairTangent`。

两层高光（原神头发那两道扫光）：

```hlsl
float3 t1 = normalize(T + N * _AnisoShift1);
float3 t2 = normalize(T + N * _AnisoShift2);
float3 H = normalize(V + L);

half spec1 = pow(sqrt(saturate(1 - dot(t1, H) * dot(t1, H))), _AnisoPower1);
half spec2 = pow(sqrt(saturate(1 - dot(t2, H) * dot(t2, H))), _AnisoPower2);

half specMask = ilm.g; // 同样以你对的通道为准
half3 spec = (spec1 + spec2 * _AnisoSpec2) * specMask * _SpecColor.rgb;
```

`sqrt(1 - TdotH^2)` 就是 Kajiya-Kay 里的 `sin`，高光会沿发丝滑动。

验收：绕角色走一圈，高光是一条带走过头发，不是脑门上一个白点。第一层 `_AnisoShift1` 建议从 `0.1` 试，第二层 `-0.08` 左右。切线是坏的（导入没切线）时，高光会碎——那是模型问题。

漫反射部分直接复用身体第 2～3 步。头发 Ramp 行常常和身体不是同一条，单独给 `_RampY`。

---

## 脸 Shader：没 SDF 也能先写，有图再换一行

脸 **不要用身体那套 NdotL 当主阴影**。法线在鼻子、眼窝上太碎，卡通脸会花。

### 没 SDF 时（你现在）

先让脸「能亮、能染色」：

```hlsl
half ndotl = dot(N, L) * 0.5h + 0.5h;
half3 ramp = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, float2(ndotl, _RampY)).rgb;
half3 color = albedo.rgb * ramp * mainLight.color;
```

`_RampY` 用 Ramp **第一行**（皮肤那条，大约 0.125）。眼睛那张白块可以当 mask：白的地方强制 `ndotl = 1`，避免眼珠进脸阴影。

验收：脸是皮肤色，灯转时还是球状明暗——这是预期，等 SDF。

### 有 SDF 之后怎么接（朝向 +X）

原神做法的核心就一句：**用「光在脸平面上偏左还是偏右」，去和 SDF 灰度比大小**，不用鼻子的法线。

脸空间（没骨骼，写死）：

```hlsl
float3 headF = float3(1, 0, 0);          // 脸朝 +X
float3 headU = float3(0, 1, 0);
float3 headR = normalize(cross(headU, headF)); // 脸右
```

光在脸左右/前后上的分量：

```hlsl
float2 Lxz = normalize(float2(dot(L, headR), dot(L, headF)));
```

- `Lxz.y > 0`：光在脸前  
- `Lxz.x > 0`：光从脸右侧来  

SDF 一般只画「光从一侧来」的梯度，另一侧把 UV.x 翻过来再采一次：

```hlsl
half sdf  = SAMPLE_TEXTURE2D(_FaceSDF, sampler_FaceSDF, i.uv).r;
half sdfM = SAMPLE_TEXTURE2D(_FaceSDF, sampler_FaceSDF, float2(1 - i.uv.x, i.uv.y)).r;

half sdfUse = (Lxz.x > 0) ? sdfM : sdf;
half faceShadow = smoothstep(
    -_FaceSoft,
     _FaceSoft,
    sdfUse * 2 - 1 + Lxz.y * _FaceFront); 
// _FaceFront 用来控制「正前方有多不容易进阴影」
```

然后 `faceShadow` 当 Ramp 的 U，和身体一样去采皮肤那一行 Ramp。

验收：转灯时，阴影贴着鼻翼、颊侧「画好的形状」挪，而不是一个圆盘在脸上滚。眼睛 mask 为 1 的像素不要乘 faceShadow。

公式里的加减、是否 `1-sdf`，以你那张 SDF 的亮暗约定为准：若灯在右、阴影却跑反了，对调 `sdf` / `sdfM`，或把 `Lxz.x > 0` 反过来。

---

## 没有官方 SDF 时，怎么自己糊一张能用的

不需要完美还原米哈游那张。选角能看，一张 512 的灰度就够：

1. 用脸 Diffuse 当底，新建灰度层。  
2. 当「光从脸正面偏左 30° 打过来」去画：受光的额、左颊偏亮（白），鼻梁右侧、右颊、下颌偏暗（灰）。  
3. 做成平滑渐变，不要硬边。鼻梁做成一条中间亮、两侧暗的脊。  
4. 眼睛、嘴巴可以涂中灰，真阴影用眼睛 mask 抠掉。  
5. 导入：sRGB 关，用这张当 `_FaceSDF`。

你有正版/拆包资源后，用官方 Face Shadow 替换即可，Shader 不用改结构。

---

## 建议你今天实际敲的顺序

1. 改 Lightmap / Ramp 导入。  
2. 只写 `GenshinBody`，走到第 3 步（Ramp + 阈值），Body 材质换过去。  
3. 复制成 Hair，加切线高光。  
4. 复制成 Face，先半兰伯特 + 皮肤 Ramp；SDF 有了再换采样。  

每一步只改一个现象。身体已经像「块」了再动头发，否则分不清是 Ramp 错了还是切线错了。

你写完 Body 第 3 步如果阴影形状不对，把 Lightmap 分通道截图发我，我们按你的图把 `.r/.g/.b/.a` 定死。SDF 你画好或找到官方图之后，再说一声，我按你那张的亮暗方向帮你对「左/右翻转」要不要对调。

## 6. 你（Tuesday, Sep 1, 2026, 8:37 PM (UTC+8)）

你叫我写shader的时候，最好把shader的每一个模块都告诉我，比如说vert模块，frag模块，上面你告诉我了基本的Pass模块

### 助手

你现在的文件已经有外壳了。下面按 **从上到下每一个模块** 说明：它干什么、你要写什么。函数名跟你现在的 `vert` / `frag` 一致。

先改一个小点：第 9 行 Tags 里用 `"RenderPipeline"="UniversalPipeline"`（不要 `UniversalRenderPipeline`），URP 认的是这个名字。

---

## 整份 Shader 的地图

```
Shader "名字"
{
    Properties          ← Inspector 里能看到的参数
    SubShader
    {
        Tags            ← 告诉 URP：不透明、走哪条管线
        Pass
        {
            Tags        ← 告诉 URP：这是 Forward 那一刀
            HLSLPROGRAM
                pragma  ← 指定 vert / frag 函数名
                include ← 借用 URP 的矩阵、灯光
                CBUFFER ← 材质参数（给 SRP Batcher）
                贴图声明
                Attributes  ← 网格输入（vert 的入参）
                Varyings    ← vert 传给 frag 的包
                vert        ← 每个顶点跑一次
                frag        ← 每个像素跑一次
            ENDHLSL
        }
    }
}
```

选角身体 Shader **暂时只要这一个 Pass**。ShadowCaster / DepthOnly 以后再说。

---

## 1. Properties（给美术调的）

写在 `Shader` 大括号最上面。名字是 **显示名**，真正进 GPU 的是左边那个变量名。

身体第一版先放这些：

```hlsl
Properties
{
    [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
    [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
    _LightMap("ILM LightMap", 2D) = "white" {}
    _RampMap("Shadow Ramp", 2D) = "white" {}
    _RampY("Ramp Row (0.125/0.375/0.625/0.875)", Range(0,1)) = 0.125
    _ShadowThreshold("Shadow Threshold", Range(0,1)) = 0.5
    _ShadowSmooth("Shadow Smooth", Range(0.001,0.5)) = 0.05
}
```

现在可以先只留 `_BaseMap` 和 `_BaseColor`，后面步骤再往里加。Properties **不会自动进 GPU**，必须在下面的 CBUFFER / TEXTURE2D 再声明一次。

---

## 2. SubShader Tags

```hlsl
Tags
{
    "RenderPipeline"="UniversalPipeline"
    "RenderType"="Opaque"
    "Queue"="Geometry"
}
```

- `RenderPipeline`：只给 URP 用  
- `RenderType` / `Queue`：当普通不透明物体画  

`LOD 100` 可留可删，URP 角色一般不靠 LOD 切换这套。

---

## 3. Pass 模块（你已经写了）

```hlsl
Pass
{
    Name "ForwardLit"
    Tags { "LightMode"="UniversalForward" }
    Cull Back
    ZWrite On
    ZTest LEqual
```

- `LightMode=UniversalForward`：URP 画不透明正向光照时会跑这个 Pass  
- `Cull Back`：背面剔除  
- `ZWrite On`：写深度  

`HLSLPROGRAM` … `ENDHLSL` 之间才是真正的 GPU 代码。

---

## 4. pragma（指定入口）

```hlsl
#pragma vertex vert
#pragma fragment frag
```

意思是：顶点入口叫 `vert`，像素入口叫 `frag`。下面必须有这两个函数，名字要完全一致。

---

## 5. include（借 URP 的工具）

你已经写了，保留：

```hlsl
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
```

- `Core.hlsl`：物体→世界→裁剪空间、UV 变换  
- `Lighting.hlsl`：`GetMainLight()`、`GetVertexPositionInputs()` 等  

选角暂时不要加一堆 `multi_compile`。

---

## 6. CBUFFER（材质参数进 GPU）

Properties 里的 Float/Color/Vector 都要进这里，名字必须和 Properties 里一样。

```hlsl
CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;   // 自动带上的 tiling/offset，TRANSFORM_TEX 会用
    half4  _BaseColor;
    float  _RampY;
    half   _ShadowThreshold;
    half   _ShadowSmooth;
CBUFFER_END
```

贴图本身 **不进 CBUFFER**，进下一节。`_BaseMap_ST` 即使 Properties 没写，只要有 `_BaseMap`，Unity 也会生成。

---

## 7. 贴图声明

```hlsl
TEXTURE2D(_BaseMap);  SAMPLER(sampler_BaseMap);
TEXTURE2D(_LightMap); SAMPLER(sampler_LightMap);
TEXTURE2D(_RampMap);  SAMPLER(sampler_RampMap);
```

采样时写：

```hlsl
SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv)
```

第一版可以只声明 `_BaseMap`。

---

## 8. Attributes 模块（网格给 vert 的数据）

每个 **顶点** 从模型里读这些：

```hlsl
struct Attributes
{
    float4 positionOS : POSITION;  // 物体空间位置
    float3 normalOS   : NORMAL;    // 物体空间法线
    float2 uv         : TEXCOORD0; // 第一套 UV
};
```

冒号后面是 **语义**，告诉 GPU 这根线接网格的哪一项。身体第一版这三个够用。头发以后要加 `float4 tangentOS : TANGENT`。

---

## 9. Varyings 模块（vert 打包给 frag）

光栅化会按像素插值这些量：

```hlsl
struct Varyings
{
    float4 positionCS : SV_POSITION; // 裁剪空间，必须有
    float2 uv         : TEXCOORD0;
    float3 positionWS : TEXCOORD1;   // 世界坐标，算视线用
    float3 normalWS   : TEXCOORD2;   // 世界法线，算光照用
};
```

`SV_POSITION` 是系统语义，不能省。`TEXCOORDn` 只要互不重复即可。

---

## 10. vert 模块（每个顶点一次）

职责：**只做空间变换，不算光照**（卡通灯放 frag，避免插值把阴影块插糊）。

```hlsl
Varyings vert(Attributes input)
{
    Varyings o = (Varyings)0;

    VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs  nrm = GetVertexNormalInputs(input.normalOS);

    o.positionCS = pos.positionCS;
    o.positionWS = pos.positionWS;
    o.normalWS   = nrm.normalWS;
    o.uv         = TRANSFORM_TEX(input.uv, _BaseMap);

    return o;
}
```

对应关系：

| 你要的 | 用哪个 URP 函数 |
| --- | --- |
| 裁剪空间坐标（屏幕位置） | `pos.positionCS` |
| 世界坐标 | `pos.positionWS` |
| 世界法线 | `nrm.normalWS` |
| UV 带 Tiling | `TRANSFORM_TEX` |

---

## 11. frag 模块（每个像素一次）

职责：采样贴图、算主光、输出颜色。返回类型 `half4`，语义 `SV_Target` 表示画到当前颜色缓冲。

**先写第 1 步（只验证通了）：**

```hlsl
half4 frag(Varyings i) : SV_Target
{
    float3 N = normalize(i.normalWS);
    Light mainLight = GetMainLight();
    float3 L = normalize(mainLight.direction);

    half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
    half ndotl = saturate(dot(N, L) * 0.5h + 0.5h);

    return half4(albedo.rgb * ndotl * mainLight.color, 1);
}
```

这里三件事：

1. `normalize(normalWS)`：插值后法线长度会变，必须再归一化  
2. `GetMainLight()`：场景里那盏 Directional Light  
3. 半兰伯特 `dot * 0.5 + 0.5`：暗部不会全黑，方便下一步接 Ramp  

---

## 你现在该敲进去的完整顺序

对着你已有的文件，在 `#include` 两行 **之后**、`ENDHLSL` **之前**，按这个顺序补：

1. `CBUFFER_START` … `CBUFFER_END`  
2. `TEXTURE2D` / `SAMPLER`  
3. `struct Attributes`  
4. `struct Varyings`  
5. `vert` 函数  
6. `frag` 函数  

补完后：`Pole_Body` 换成这个 Shader，Diffuse 拖进 `_BaseMap`，进 Play。转灯应看到灰阶明暗。没有报错、能转灯，再进入之前说的第 2 步 Ramp。

如果 Console 有红字，把完整报错贴过来（哪一行、什么 `error`）。下一步我再按同样方式拆 **Ramp 采样** 该写在 frag 的哪几行。

## 7. 你（Tuesday, Sep 1, 2026, 10:28 PM (UTC+8)）

OK, 这一步完成了，符合预期，继续下一步吧。

### 助手

第 2 步只动 **frag 模块**。Properties、CBUFFER、`TEXTURE2D(_RampMap)` 你已经写好了，vert 也不用改。

---

## 这一步在算什么

第 1 步用 `NdotL` 当灰度去乘颜色，所以暗部发脏。

现在用 `NdotL` 去 **Ramp 上查颜色**：

- **U（横）**：亮暗。`0` 是 Ramp 最左，`1` 是最右  
- **V（竖）**：第几条。你这张胡桃 Ramp 有 4 行，用 `_RampY`

查到的 `ramp.rgb` 代替原来的 `NdotL` 去乘 albedo。

---

## Inspector 先挂图

1. `Pole_Body` 的 **Ramp Map** 拖入 `Avatar_Girl_Pole_Hutao_Tex_Body_Shadow_Ramp`  
2. 这张 Ramp 的 Wrap 设成 **Clamp**（不要 Repeat）  
3. `_RampY` 先拨到 **0.125**（第一行，皮肤/暖色）。`0.5` 容易卡在两行中间  

Light Map、Threshold 这步先不用管。

---

## 改 frag 模块

把原来的：

```hlsl
half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
half NdotL = saturate(dot(N, L) * 0.5 + 0.5);

return half4(albedo.rgb * NdotL * mainLight.color,1);
```

换成：

```hlsl
half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
half NdotL = saturate(dot(N, L) * 0.5h + 0.5h);

float2 rampUV = float2(NdotL, _RampY);
half3 ramp = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, rampUV).rgb;

return half4(albedo.rgb * ramp * mainLight.color, 1);
```

三行对应三件事：

| 行 | 模块里的角色 |
| --- | --- |
| `NdotL` | 还是半兰伯特，当作 Ramp 的 U |
| `rampUV` | 拼采样坐标：横=光照，竖=哪一条 Ramp |
| `ramp` | 从贴图取出「亮部色 / 暗部染色」 |

---

## 验收

- 转灯：暗部应是 Ramp 里的染色（暖橙或冷紫），不是脏灰  
- 拧 `_RampY`：0.125 / 0.375 / 0.625 / 0.875 应换到不同条，阴影色会变  
- 明暗过渡可能仍偏软——下一步才用 Threshold 收硬  

如果 **亮部和暗部对调了**（灯打正面反而变暗），把 U 改成 `1.0h - NdotL` 再试，说明这张 Ramp 是左暗右亮。

这一步 OK 了说一声，下一步把 **Shadow Threshold + LightMap.R** 接进 frag。

## 8. 你（Tuesday, Sep 1, 2026, 10:39 PM (UTC+8)）

据我了解的，原神的这个Ramp图在Y轴上分层了两部分，每一部分分层了5条颜色，上半部分可能是白天的颜色，下半部分是晚上的 颜色？ 只是现在这张图上上下部分的第四和第五个颜色都是白色而已；你看看是不是这样的；

### 助手

我对着你这张 Ramp 贴图看一下 Y 轴怎么分层，以及是不是「上昼下夜、每半边 5 条」。

### 助手

你的理解是对的，我上次按「4 条」来写 `_RampY` 不准确。对着这张图量过了：

**`256 × 20`，刚好 10 条，每条 2 像素高。**

```
上半 5 条（白天）
  1  冷紫灰 → 暖肤色
  2  鲑鱼粉 → 浅桃
  3  暖灰   → 浅桃
  4  纯白
  5  纯白
─────────
下半 5 条（夜晚）
  1  冷蓝灰 → 暖白
  2  粉     → 浅桃
  3  紫     → 暖灰
  4  纯白
  5  纯白
```

第 4、5 条上下都是白的，所以乘到衣服上等于「不再额外染阴影色」，不是贴图坏了，是这张胡桃 Ramp 没用到那两档材质。

---

## 原神一般怎么用这 10 条

| 轴 | 含义 |
| --- | --- |
| **U（横）** | 光照。这张图是 **左暗右亮**（左边彩色阴影，右边接近受光色）。你现在 `U = NdotL` 方向是对的，不用 `1-NdotL`。 |
| **V 上半 / 下半** | 白天 / 晚上。游戏里用时间或一个 `DayNight` 参数，把 V 整体偏移 `0.5`。 |
| **每一半里的 5 条** | 不同材质（皮肤、布、金属等）。常用 Lightmap 的某个通道当「第几条」。 |

白条 = Ramp 贡献为 1，阴影只靠漫反射本身，看起来会「平」。选角先用有颜色的 1～3 条就行。

---

## 采样时注意 Unity 的 V

Unity 里 **V=0 是贴图底边，V=1 是顶边**。美术说的「上面是白天」在 Shader 里是 **V 比较大**。

10 条的中心大约是：

| 你想采的条 | 白天 `_RampY` | 夜晚 `_RampY` |
| --- | --- | --- |
| 第 1 条 | 0.95 | 0.45 |
| 第 2 条 | 0.85 | 0.35 |
| 第 3 条 | 0.75 | 0.25 |
| 第 4 条（白） | 0.65 | 0.15 |
| 第 5 条（白） | 0.55 | 0.05 |

公式（行号 `0~4`，`isNight` 为 0 或 1）：

```hlsl
// row: 0=第一条(有颜色), 4=第五条(白)
// isNight: 0=白天(贴图上半), 1=晚上(贴图下半)
float rampV = 1.0 - (isNight * 5.0 + row + 0.5) / 10.0;
```

选角界面可以先不管昼夜，`isNight = 0`，白天拧 `_RampY`：`0.95 / 0.85 / 0.75` 看三条彩色。

---

Inspector 里 `_RampY` 先别停在 `0.5`——那正好在昼夜分界附近，容易采到白条或两条掺在一起。下一步接 Lightmap 时，再把「第几条」从手拧改成贴图通道。

## 9. 你（Tuesday, Sep 1, 2026, 10:51 PM (UTC+8)）

这一段的代码该如何写？

### 助手

这一段要改 **三个模块**：Properties、CBUFFER、frag。vert 不用动。另外你 frag 里现在有一个 `return half4(ramp,1)` 会提前退出，真正乘 albedo 的那行走不到，改完后把它删掉。

---

## 1. Properties 模块

把原来的 `_RampY` 换成「第几条」和「白天/晚上」：

```hlsl
_RampRow("Ramp Row (0-4)", Range(0, 4)) = 0
[Toggle] _IsNight("Night Ramp", Float) = 0
```

`_RampRow = 0` 是每一半的第 1 条（有颜色），`3` 和 `4` 是白条。

---

## 2. CBUFFER 模块

`_RampY` 改成这两个，名字必须和 Properties 一致：

```hlsl
float _RampRow;
float _IsNight;
```

`_ShadowThreshold` / `_ShadowSmooth` 先留着，下一步才用。

---

## 3. frag 模块

NdotL 算完之后，**不要再写** `float2 rampUV = float2(NdotL, _RampY)`，改成：

```hlsl
half NdotL = saturate(dot(N, L) * 0.5h + 0.5h);

float row = clamp(_RampRow, 0.0, 4.0);
float night = step(0.5, _IsNight); // 勾选=1，不勾=0
float rampV = 1.0 - (night * 5.0 + row + 0.5) / 10.0;

float2 rampUV = float2(NdotL, rampV);
half3 ramp = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, rampUV).rgb;

return half4(albedo.rgb * ramp * mainLight.color, 1);
```

三行在干什么：

| 代码 | 作用 |
| --- | --- |
| `clamp(_RampRow, 0, 4)` | 只允许 5 条材质 |
| `night * 5` | 晚上整段往贴图下半挪 5 条 |
| `+ 0.5) / 10` | 采每一条的中心，避免卡在分界线上 |
| `1.0 - ...` | Unity 的 V=0 在贴图底，所以要反过来才能让「第 1 条」对上美术说的最上面那条 |

然后 **删掉** 这两行，只留上面那个 `return`：

```hlsl
return half4(ramp,1);
return half4(albedo.rgb * NdotL * mainLight.color,1);
```

---

## 验收

- `_IsNight` 不勾、`_RampRow = 0/1/2`：白天三条彩色阴影  
- `_RampRow = 3` 或 `4`：几乎不染色（白条）  
- 勾上 `_IsNight`：同样 row，阴影变成下半那套冷色  

对了再说，下一步把 Lightmap 接进 frag，用通道自动选 row，并加上 Threshold。

## 10. 你（Tuesday, Sep 1, 2026, 11:11 PM (UTC+8)）

我新增了一个ramp贴图用来做测试，我把第一条改成了纯绿色，第二条改成了纯紫色，第三条改成了深蓝色，测试一下看是否采样了正确的颜色，我实验下来发现，在Ramp row中填0和1都能识别正确的颜色，就是填2的时候没有显示正确的蓝色，更像是蓝色和紫色的混合；我看了一下贴图，是不是Unity把这个ramp图原来的256*16，改成了256*256，导致颜色和颜色之间混合了？

## 11. 你

<dynamic_tools>
You have access to tools through dynamic namespaces, e.g. MCP servers, using `GetDynamicTools` and `CallDynamicTool`.

## Dynamic Tool Discovery and Invocation

Use `GetDynamicTools` to discover tool schemas, then `CallDynamicTool` to invoke one tool. Aim to minimize round-trips: ideally one discovery call followed by one invocation.

If the user mentions a product or service represented by an available namespace, and the request likely depends on it, proactively inspect that namespace before answering. If you are unsure which namespace matches, search with a relevant pattern.

`GetDynamicTools` supports these modes:

1. `{"namespace":"<id>"}`: returns schemas and full descriptions for every tool in that namespace.
2. `{"namespace":"<id>","toolName":"<name>"}`: returns one tool schema with its full description.
3. `{"pattern":"<regex>"}`: searches namespace and tool names.
4. `{"namespace":"<id>","pattern":"<regex>"}`: searches tools within one namespace.
5. No arguments: returns the full catalog.

Pattern-search and catalog results shorten long descriptions, marked by a trailing "... [truncated]"; namespace and single-tool lookups always return the complete description.

Always inspect a tool's schema before invoking it with `CallDynamicTool`.

If the available dynamic tools do not fully support what the user asked you to do, complete the work you can with the current tool set. In your work summary, include what you were unable to do and why. Do not use browser automation to work around missing tools unless the user explicitly asks you to use the browser.

Available dynamic tool namespaces:

<dynamic_tool_namespaces>
<namespace name="plugin-gmail-gmail" source="mcp" />
<namespace name="plugin-google-drive-google-drive" source="mcp" />
<namespace name="cursor" tools="CreateGoal, GenerateImage, UpdateGoal" namespaceUseInstructions="Native Cursor tools for this session. These are highly recommended and useful tools that you should use when the right situation arises. Don't be afraid to look at one if it seems relevant, even if you don't end up using it. You MUST read the tool schemas before calling them." source="cursor" />
</dynamic_tool_namespaces>

## MCP Resource Access

You also have access to MCP resources via `FetchMcpResource`.
If an MCP-backed namespace requires authentication, call `mcp_auth` through `CallDynamicTool` for that namespace, then inspect it again and retry if appropriate. Do not authenticate namespaces preemptively or repeatedly.
</dynamic_tools>

## 12. 你（Tuesday, Sep 1, 2026, 11:11 PM (UTC+8)）

我新增了一个ramp贴图用来做测试，我把第一条改成了纯绿色，第二条改成了纯紫色，第三条改成了深蓝色，测试一下看是否采样了正确的颜色，我实验下来发现，在Ramp row中填0和1都能识别正确的颜色，就是填2的时候没有显示正确的蓝色，更像是蓝色和紫色的混合；我看了一下贴图，是不是Unity把这个ramp图原来的256*16，改成了256*256，导致颜色和颜色之间混合了？

### 助手

先看你这张测试 Ramp 的真实尺寸和导入设置，再判断是公式按 10 行采错了，还是 Unity 缩放/滤波把相邻行混在一起。

### 助手

不完全是「被拉成 256×256」。你这张测试图源文件是 **256×20**，和原版 Ramp 一样。Unity 预览窗经常显示成方图，看起来像 256×256，**GPU 上实际导入尺寸通常不是正方形**。

真正会把第 2 行蓝和紫混在一起的，是下面几件事叠在一起。

## 1. NPOT 把 20 高缩成了 16

20 不是 2 的幂。你的 `.meta` 里是：

- `nPOTScale: 1` → Inspector 里 **Non-Power of 2 = To nearest**
- 离 20 最近的 2 的幂是 **16**（差 4），32 差 12

所以导入后经常是 **256×20 → 256×16**，不是 256×256。10 行原来每行 **2 像素**，挤成 16 高后每行只有 **1.6 像素**，行与行之间本来就会糊。

在 Inspector 顶上看导入后的 **Width × Height**（不要看中间那个方预览）。

## 2. 双线性 + Mipmap 会专门坑「中间那一行」

当前导入还是默认：

- **Filter Mode = Bilinear**
- **Generate Mip Maps = On**
- **Compression = Compressed**

采样公式仍按 10 行、采行中心：

```hlsl
float rampV = 1.0 - (night * 5.0 + row + 0.5) / 10.0;
```

- Row **0** → V ≈ **0.95**（贴图最顶，旁边几乎没有别的色）
- Row **1** → V ≈ **0.85**
- Row **2** → V ≈ **0.75**（夹在紫和蓝之间）

Bilinear 会采 2×2 邻域。行高只剩 1.6px 时，第 2 行中心离紫/蓝交界非常近，所以看起来像蓝紫混合；0、1 更靠边缘，还能「碰巧」对上。

更坑的是这句：

```hlsl
SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, rampUV)
```

`rampUV.x = NdotL`，在角色表面上从 0 变到 1，GPU 会以为贴图被缩得很小，**自动选很糊的 mip**。中间那一行最先变成泥色。Toon Ramp 必须锁 0 级 mip。

## 3. 建议改法（导入 + 一行采样）

选中 Ramp，改完点 Apply：

| 项 | 改成 |
|---|---|
| Non-Power of 2 | **None**（保持 256×20） |
| Filter Mode | **Point** |
| Generate Mip Maps | **关** |
| Wrap Mode | **Clamp** |
| Compression | **None** |

Shader 里把采样改成锁 mip 0：

```hlsl
half3 ramp = SAMPLE_TEXTURE2D_LOD(_RampMap, sampler_RampMap, rampUV, 0).rgb;
```

改完再测 Row = 2，应该就是纯深蓝，不再掺紫。

**结论：** 颜色串行主要是 **20→16 的缩放 + 双线性/mip 在行边界上插值**，不是方图 256×256。预览是方的可以忽略；以 Inspector 里的导入分辨率和 Filter/Mip 设置为准。

## 13. 你（Tuesday, Sep 1, 2026, 11:36 PM (UTC+8)）

OK,  一切正常了，但是我发现一个问题，我截图给你；第一张图是我目前实现的效果，第二张图是原神的效果；我现在发现原神的阴影面积比较小，这个是不是靠Shadow Threshold的这个来控制的？

### 助手

先看你现在的 Lambert 有没有真正用上 Threshold，再对照原神那种「阴影很窄」的做法。

### 助手

是的，**阴影面积主要就是这个量在管**，但你现在的 shader 里 `_ShadowThreshold` / `_ShadowSmooth` 还没参与计算。Ramp 的 U 直接吃的是半兰伯特：

```87:93:Assets/RenderingLab/Cha/Yuanshen/Shader/Genshin_Body.shader
                half NdotL = saturate(dot(N, L) * 0.5 + 0.5);
                ...
                float2 rampUV = float2(NdotL, rampV);
```

半兰伯特在 **0.5** 处分界。侧面、裙褶背光那一侧 `NdotL` 会掉到 0.5 以下，Ramp 采到左边的阴影色，所以你会看到一大块薰衣草阴影。原神把这条分界**往更暗的一侧推**，只有很背光的地方才进阴影，亮部面积更大。

## 怎么接

用 Threshold 重映射 U，不要把半兰伯特原样当 Ramp U。把 `rampUV` 那一行改成：

```hlsl
half NdotL = saturate(dot(N, L) * 0.5 + 0.5);

float rampU = smoothstep(
    _ShadowThreshold - _ShadowSmooth,
    _ShadowThreshold + _ShadowSmooth,
    NdotL
);

float2 rampUV = float2(rampU, rampV);
```

含义：

- `NdotL < Threshold` → 靠近 Ramp **左边**（阴影色）
- `NdotL > Threshold` → 靠近 Ramp **右边**（亮部色）
- `_ShadowSmooth` 是交界宽度，原神那种硬边可以先留 **0.03～0.08**

拉 slider 时：

| 值 | 效果 |
|---|---|
| **0.5**（默认） | 接近你现在这张图，阴影偏大 |
| **往小调（0.15～0.3）** | 阴影变窄，更接近原神 |
| 往大调 | 阴影更大 |

对照原神，先把 **Shadow Threshold 降到约 0.2**，再微调 Smooth。

## 别指望只靠 Threshold 完全对上

Threshold 只移动**整块明暗交界**。原神裙子褶上那条又细又干净的暗缝，很大一部分是 **LightMap 的 R（AO / 阴影偏移）** 画进去的：大面积布料保持亮，只有夹缝被压进阴影。Threshold 调好面积之后，下一步才是把 LightMap 乘进 Lambert。

先改采样、把 Threshold 拉低，看大块阴影是否收成原神那种窄带。对上了再说 LightMap。

## 14. 你（Tuesday, Sep 1, 2026, 11:51 PM (UTC+8)）

OK，那先接入shadow Threshold 和 ShadowSmooth的代码吧，之前都还没有接入；

### 助手

已经接到 Ramp 的 U 上了：半兰伯特先经过 `smoothstep`，再去采 Ramp。

```87:99:Assets/RenderingLab/Cha/Yuanshen/Shader/Genshin_Body.shader
                half NdotL = saturate(dot(N, L) * 0.5 + 0.5);
                float rampU = smoothstep(
                    _ShadowThreshold - _ShadowSmooth,
                    _ShadowThreshold + _ShadowSmooth,
                    NdotL
                );
                ...
                float2 rampUV = float2(rampU, rampV);
```

材质里这两个 slider 现在会生效。对照原神可以把 **Shadow Threshold 降到 0.15～0.3**，阴影会明显变窄；**Shadow Smooth** 控制交界软硬，硬边可以先留在 0.05 附近。

## 15. 你（Wednesday, Sep 2, 2026, 12:10 AM (UTC+8)）

Avatar_Girl_Catalyst_Barbara_Tex_Body_Lightmap 这个图的G通道感觉是不是弥补没阴影的地方？

### 助手

先把这张 LightMap 的各通道拆开看，再对照原神常见的 ILM 用途判断 G 到底是不是在补阴影。

### 助手

不是。G 看起来像在「补阴影」，其实是 **高光强度**；真正用来补几何光打不出来的阴影的是 **R**。

Barbara 这张 Body LightMap 拆开之后是这样的：

| 通道 | 画面长什么样 | 用途 |
|---|---|---|
| **R** | 有灰阶变化：褶皱、夹缝偏暗，装饰偏亮 | **AO / 阴影偏移**，按像素把 Lambert 推进或拉出阴影 |
| **G** | 衣服几乎是一条平的中灰（约 **128**），部分饰品是 **0** | **高光强度**。布料半高光，黑的地方没高光 |
| **B** | 几档离散灰 | 高光形状 / 阈值（shininess） |
| **A** | 几块接近 0.5 或 1 的色块 | Ramp 行 / 材质 ID |

G 之所以容易看错：整件衣服都是同一档灰，RGB 合成时整张图发绿；饰品、花纹上 G=0 变成黑块，看起来像「这里要强制出阴影」。那些黑块是 **关掉高光**，不是往 Lambert 里加阴影。G 的最大值只有约 150，从来没有纯白，也不像一张 AO。

「没阴影的地方补上」对应的是 **R**：

- R 暗 → 同等光照下更早进 Ramp 左边（腋下、裙褶、布料夹缝）
- R 亮 → 更不容易进阴影（十字、花纹等要保持干净的细节）

后面接到 Lambert 时，典型是用 R 去偏 Threshold，而不是乘 G：

```hlsl
half4 ilm = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, IN.uv);
half ao = ilm.r;
float rampU = smoothstep(
    _ShadowThreshold - _ShadowSmooth,
    _ShadowThreshold + _ShadowSmooth,
    NdotL * ao
);
```

`NdotL * ao` 会让 R 暗的地方即使朝向灯光也更容易进阴影，大面积衣服仍由 Threshold 控制亮面大小。G 留到做身体高光再用。

另外这张图是数据贴图，Inspector 里把 **sRGB 关掉**，否则 R/G 的 128 会被当成颜色做伽马，阈值会漂。Channel 下拉选 R / G 可以自己对一下上面的判断。

## 16. 你（Wednesday, Sep 2, 2026, 12:19 AM (UTC+8)）

不是的，我可以确定，这张图的G通道就是补阴影的，可以从裙子的褶皱处可以判断出来；

### 助手

你说得对，我按常见教程把 G 当成高光了。先对着你这张通道图，把裙褶上的像素值对一下，再改结论。

### 助手

你说得对，是我搞错了。裙褶那几条放射状黑线就是画进去的褶皱阴影，这张图的 **G 就是用来补 N·L 打不出来的阴影**。

上次按通用教程把 G 当成高光，和这张 Barbara LightMap 对不上。布料大面积是 **G ≈ 128**（Unity 的 G 通道预览会显示成实心绿，看起来像 1，实际是中灰），褶沟、裙摆内侧、部分饰品是 **G = 0**。R 在衣服上反而是大面积接近 0，所以这张图里补阴影的是 G，不是 R。

褶之所以要画：裙褶法线常常是平滑插值的，灯光一转，沟里也会被算成亮部。G=0 把这些像素钉在 Ramp 左边，亮部再大，褶沟也还在。

接入时不要直接 `NdotL * ilm.g`。128 是「正常受光」，直接乘会把整件衣服压暗。先把 0.5 当成 1：

```hlsl
half4 ilm = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, IN.uv);
half foldShadow = saturate(ilm.g * 2.0); // 128 → 1，0 → 0

float rampU = smoothstep(
    _ShadowThreshold - _ShadowSmooth,
    _ShadowThreshold + _ShadowSmooth,
    NdotL
) * foldShadow;
```

`foldShadow = 1` 时只走 Threshold；褶沟 `= 0` 时 `rampU` 被乘掉，强制采阴影色。

这就是「补阴影」：Threshold 管大块明暗，G 管几何光照顾不到的死缝。要的话我可以直接改进 shader。

## 17. 你（Wednesday, Sep 2, 2026, 12:23 AM (UTC+8)）

先不改进shader，等我先明白原理；我现在不理解的是，G通道在改加阴影的地方标记成黑色，他是怎么和Ramp图的阴影的颜色统一的？

### 助手

G 通道**不带颜色**。它只决定「去 Ramp 的哪一列采样」，阴影长什么样仍然完全来自 Ramp。

整条链路是：

```
NdotL + Threshold  →  算出一个 0~1 的 rampU
G 通道（褶沟=0）  →  再把这个 U 往左边压
                    ↓
              用这个 U 去采 Ramp
                    ↓
              albedo × ramp颜色
```

Ramp 从左到右是：**阴影色 → 亮部色**。  
动态光造成的大块阴影，是 `NdotL` 低，U 靠近 0，采到左边。  
G 画成黑的褶沟，是把 U **强制变成 0**，同样采到左边。

所以两处阴影是同一块像素、同一行 Ramp：

| 来源 | 怎么进阴影 | 采到的颜色 |
|---|---|---|
| 侧光、背光 | Threshold 把半兰伯特压到 U≈0 | Ramp 左边（例如裙子那条薰衣草） |
| 裙褶（G=0） | 不管朝不朝光，U 被乘成 0 | **还是** Ramp 左边同一条色 |

颜色能统一，是因为 **没有「G 的阴影色」这种东西**。G 只是开关：黑 = 请用 Ramp 的阴影端；绿/128 = 请用光照算出来的 U。

白裙子乘上 Ramp 左边的淡紫，褶沟和侧影都会变成同一种薰衣草；蓝饰边乘同一列 Ramp，阴影就是更深的蓝紫。换 Ramp 行（以后用 A 通道）只换「这是哪套材质的阴影色」，不会让 G 单独再调一种颜色。

可以把它想成：Threshold 决定「光从哪开始变暗」，G 决定「这些像素能不能变亮」。两者最后都只改采样坐标，颜色字典只有 Ramp 一本。

## 18. 你（Wednesday, Sep 2, 2026, 12:31 AM (UTC+8)）

绿/128 这里为什么要除以128？ 直接用rampUV的U乘以_LightMap.G不就可以了吗？

### 助手

可以乘，但要乘对地方；G 的 128 在 shader 里是 **0.5**，不是 1。

Unity 把 G 通道显示成实心绿，看起来像填满了。贴图字节是 0–255，shader 读到的是除以 255 之后的 0–1：

| 贴图像素 | shader 里的 `ilm.g` |
|---|---|
| 0（褶沟黑） | **0** |
| **128（衣服上那片绿）** | **≈ 0.5** |
| 255（这张图几乎没有） | 1 |

`* 2`（也就是按 128 当满分）只是把作者的「满分」还原成 1：`128 → 1`，`0 → 0`。

若在 **smoothstep 之后** 写 `rampU * ilm.g`：

- 亮部 `rampU = 1`，再乘 0.5 → **U 永远是 0.5**
- Ramp 右半段（真正的亮部色）采不到，整件衣服会像半阴影

褶沟倒是对的：`1 * 0 = 0`，还是阴影。问题出在大面积绿区被当成半开，而不是全开。

若乘在 **Threshold 之前**：

```hlsl
smoothstep(..., NdotL * ilm.g)
```

Threshold 设得很低（比如 0.2）时，亮部 `1 * 0.5 = 0.5` 仍高于 0.2，看起来还能过。Threshold 回到 0.5 时，整个身体都贴在明暗交界上，阴影会炸开。作者把衣服涂成 128，意思是「这里正常受光」，不是「亮度打五折」。

所以：

- **褶沟要吃阴影**：乘 G 可以（G=0 把 U 打到 Ramp 左边）
- **衣服要采到 Ramp 最右边**：128 必须先当成 1，也就是 `saturate(ilm.g * 2)`，再去乘

`* 2` 不是什么特殊公式，只是这张图画布用 128 当 100%，不是 255。

## 19. 你（Wednesday, Sep 2, 2026, 12:41 AM (UTC+8)）

哦，确实，G通道的没有阴影的区域是128的颜色；那先把_LightMap的G通道的shader先完成把，你不用直接修改我的代码；你先列出代码，并做一些注释就可以了，代码我自己加入到shader中；

### 助手

`_LightMap` 贴图和 sampler 你已经有了，只改 `frag`。插在算完 `NdotL` / `rampU` 的地方。

```hlsl
// LightMap.g：128 = 正常受光，0 = 强制进阴影（裙褶等）
half4 ilm = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, IN.uv);

// 128/255 ≈ 0.5，乘 2 把「满分」还原成 1；褶沟仍是 0
half foldShadow = saturate(ilm.g * 2.0);

half NdotL = saturate(dot(N, L) * 0.5 + 0.5);

float rampU = smoothstep(
    _ShadowThreshold - _ShadowSmooth,
    _ShadowThreshold + _ShadowSmooth,
    NdotL
);

// 乘在 smoothstep 之后：亮部仍是 1，褶沟变成 0，采 Ramp 最左边
rampU *= foldShadow;
```

`float2 rampUV = float2(rampU, rampV);` 不用改，继续用乘完之后的 `rampU`。

验证时可以临时：

```hlsl
return half4(foldShadow.xxx, 1);   // 衣服白、褶沟黑
return half4(rampU.xxx, 1);        // 侧影 + 褶沟都应该是黑
```

看完把最后的 `return` 换回 `albedo * ramp * light`。LightMap 是数据图，Inspector 里 **sRGB 关掉**。

## 20. 你（Wednesday, Sep 2, 2026, 9:32 PM (UTC+8)）

ilm.g 这里要乘以2，是把通道从128的还原到256吗？256最终在shader采样的图里是1？

### 助手

不是还原到 256。8 位贴图像素是 **0～255**，没有 256；shader 里采样结果已经是 **0～1**，也没有 256 这个单位。

对应关系：

| 贴图里你看到的 | shader 里的 `ilm.g` |
|---|---|
| 0 | **0** |
| 128 | **128 / 255 ≈ 0.5** |
| 255 | **1** |

GPU 不会给你 128 或 256，一采样就是除过 255 的小数。`* 2` 只是在这个 0～1 上做缩放：

```
0.5 * 2 = 1.0
0   * 2 = 0
```

作者把「正常受光」画在 128，不是 255。乘 2 等于把他们的满分 0.5 拉成 shader 的满分 1，褶沟的 0 还是 0。

容易混的两点：

- **256** 是 8 位一共有 256 档（0 到 255），不是颜色最大值。
- 最大值是 **255 → 1**，不是 256 → 1。

## 21. 你（Wednesday, Sep 2, 2026, 9:34 PM (UTC+8)）

那如果直接在ilm.g通道里颜色填充255，是不是就不用乘以2了？

### 助手

对。若「正常受光」画成 **255**、「褶沟」画成 **0**，shader 里就是 `1` 和 `0`，直接 `rampU *= ilm.g` 即可，不用 `* 2`。

这张官方图把满分约定在 **128**，所以要乘 2。不要为了少写一个乘法去改贴图；`* 2` 就是在迁就这张图的画法。

## 22. 你（Wednesday, Sep 2, 2026, 9:36 PM (UTC+8)）

OK，现在我的身体shader已经完成了颜色贴图和ramp贴图和ilm.g的阴影区域的功能；

### 助手

身体这条链路已经齐了：**Diffuse → 半兰伯特 + Threshold → LightMap.G 补褶沟 → Ramp U/V → albedo × ramp**。

你现在 Scene 里身体是纯白/纯黑，是因为 `frag` 里先 `return` 了调试输出：

```113:115:Assets/RenderingLab/Cha/Yuanshen/Shader/Genshin_Body.shader
                return half4(rampUV.xxx,1);

                return half4(albedo.rgb * ramp * mainLight.color,1);
```

前面那行把 **Ramp 的 U**（亮=1、影=0）直接画出来了，后面真正的颜色乘算不到。确认褶沟和侧影位置无误之后，注释掉第一行，就会回到白裙子 + Ramp 阴影色。

另外 `foldShadow` 用 `half` 即可，不必 `half4`。`rampU *= foldShadow` 只需要一个标量。

下一步（仍只动 Body）比较自然的是 **LightMap.A 自动选 Ramp 行**，把现在手填的 `_RampRow` 换成贴图里的材质分区。你要继续的话再说一声。

## 23. 你（Wednesday, Sep 2, 2026, 9:43 PM (UTC+8)）

好的，我发现lightmap.a只有3个灰色，刚好对应ramp的3条颜色吧；如果是的，就做这个功能吧；还是老样子，不要直接改我代码，你呈现出来，我自己增加代码；

### 助手

先核对 LightMap.A 实际有几档灰度、和 Ramp 行怎么对应，再给你一段可直接贴进 shader 的代码。

### 助手

是的。这张 Body LightMap 的 A 不是连续灰，而是三档，对应 Ramp **白天 5 行里的第 0 / 2 / 4 行**（1、3 这张角色没用）：

| 贴图 A | shader 里 | 白天 Ramp 行 |
|---|---|---|
| ≈ 0 | 0 | 第 0 条 |
| ≈ 126～128 | ≈ 0.5 | 第 2 条 |
| ≈ 255 | 1 | 第 4 条 |

所以用 `round(ilm.a * 4)`，不要 `* 2` 去对应 0/1/2。`* 4` 才是「5 个槽位、下标 0～4」；A=1 时 `* 5` 会采到夜间第一行。A 经常是 126 不是精确 128，要用 **round**，`floor` 会落到空的第 1 行。

`ilm` 你已经采过，把原来的 `_RampRow` 换成 A 即可：

```hlsl
// LightMap.a：材质 ID，0 / 0.5 / 1 → 白天 Ramp 第 0 / 2 / 4 行
float row = round(ilm.a * 4.0);
row = clamp(row, 0.0, 4.0);

float night = step(0.5, _IsNight);
float rampV = 1.0 - (night * 5.0 + row + 0.5) / 10.0;
```

原来的

```hlsl
float row = clamp(_RampRow, 0.0, 4.0);
```

删掉或注释掉。`_RampRow` 属性可以留着，要对某行时临时改成：

```hlsl
float row = _RampRow; // 调试用，覆盖 A
```

验证：裙子、饰边、金属如果影子色不同，说明三档对上了。Night 打开后应整段换到下半 5 行，相对行号不变。

## 24. 你（Wednesday, Sep 2, 2026, 9:47 PM (UTC+8)）

应该不对吧，lightmap.a三个灰度，不是应该刚好对应ramp图上的0,1,2吗；3和4都是白色，不就没有阴影效果了吗？

### 助手

你这个判断对，是我套了「5 个材质槽」的通用公式，和这张 Ramp、这张 A 对不上。

A 只有 **0 / 0.5 / 1** 三档，就是三种材质。这张 Ramp 白天真正有阴影色的是 **第 0、1、2 行**，3、4 是白的，乘上去等于没阴影。若按我上次的 `* 4`，A=1 会落到第 4 行，那部分会「永远乘白」。

三档应对前三行：

| LightMap.A | 白天 Ramp 行 |
|---|---|
| 0 | 0 |
| 0.5 | 1 |
| 1 | 2 |

```hlsl
// 三档灰 → 前三行；3、4 行是白的，不要用
float row = round(ilm.a * 2.0);
row = clamp(row, 0.0, 2.0);

float night = step(0.5, _IsNight);
float rampV = 1.0 - (night * 5.0 + row + 0.5) / 10.0;
```

`* 2` 因为满分是 1、只要 3 个下标（0、1、2）。夜间仍然 `+ 5`，用下半张对应的 0、1、2 行。

以后若某张 LightMap 的 A 是 0 / 0.25 / 0.5 / 0.75 / 1 五档，并且 Ramp 五行都有颜色，再改回 `round(ilm.a * 4)`。这张 Barbara 用 `* 2` 即可。

## 25. 你（Wednesday, Sep 2, 2026, 9:50 PM (UTC+8)）

OK，但是这个round(ilm.a * 2.0)，如果是5行的话乘以4这个我不是很懂，可以再详细的解释一下这个算法吗？

### 助手

这是把 **0～1 的编号** 映射成 **第几行（整数下标）**，乘数永远是「行数 - 1」。

贴图 A 只能存 0～1，不能存「第 2 行」这种整数。美术用灰度当编号：最黑 = 第 0 种材质，最白 = 最后一种。

行的下标是 `0, 1, 2, …, N-1`，一共 N 行。从 0 均匀铺到 1，相邻两档之间隔着：

```
1 / (N - 1)
```

所以：

```
row = round(ilm.a * (N - 1))
```

---

**3 行**（下标 0、1、2）→ 乘 **2**：

```
0.0 * 2 = 0  → 第 0 行
0.5 * 2 = 1  → 第 1 行
1.0 * 2 = 2  → 第 2 行
```

三档灰刚好落在 0、1、2。

**5 行**（下标 0、1、2、3、4）→ 乘 **4**，不是乘 5：

```
0.00 * 4 = 0  → 第 0 行
0.25 * 4 = 1  → 第 1 行
0.50 * 4 = 2  → 第 2 行
0.75 * 4 = 3  → 第 3 行
1.00 * 4 = 4  → 第 4 行
```

若乘 5：`1.0 * 5 = 5`，会跑出第 5 行。白天只有 0～4，第 5 行已经是夜间第一行。

---

`round` 是因为像素不是精确的 128。例如 126/255 ≈ 0.494：

```
0.494 * 2 = 0.988  → round → 1   （对）
0.494 * 2 = 0.988  → floor → 0   （错，整片掉到第 0 行）
```

---

可以记成：

| 要用几行 | 乘多少 | 原因 |
|---|---|---|
| 3 行（0～2） | `* 2` | 2 = 3 - 1 |
| 5 行（0～4） | `* 4` | 4 = 5 - 1 |

A 是「在 0 和 1 之间的位置」，乘 `(行数 - 1)` 得到「第几行」。Barbara 只有三档灰、Ramp 有效阴影也在前三行，所以用 `* 2`。

## 26. 你（Wednesday, Sep 2, 2026, 9:57 PM (UTC+8)）

@Assets/RenderingLab/Cha/Yuanshen/Shader/Genshin_Body.shader:104 这一行代码是什么意思？

### 助手

`_RampRow` 是材质面板上那个 **Ramp Row (0-4)** 滑条，整件衣服共用一个数字，用来手动选 Ramp 第几行。

`clamp(x, 0, 4)` 的意思是：把 `x` 限制在 0 和 4 之间。小于 0 当成 0，大于 4 当成 4，中间不变。滑条本身已经是 0～4，这里多包一层，防止数值写出范围，采到贴图外面。

所以这一行就是：**当前手动选中的行号，保证是 0～4 的合法下标**。后面用它去算 `rampV`：

```106:106:Assets/RenderingLab/Cha/Yuanshen/Shader/Genshin_Body.shader
                float rampV = 1.0 - (night * 5.0 + row +0.5) / 10;
```

接入 LightMap.A 之后，这一行应换成按像素读 A，例如 `round(ilm.a * 2.0)`，不再整件衣服共用一个 `_RampRow`。滑条可以留着当调试覆盖。

## 27. 你（Wednesday, Sep 2, 2026, 10:28 PM (UTC+8)）

@Assets/RenderingLab/Cha/Yuanshen/Shader/Genshin_Body.shader:105 这一行呢？ step函数的功能是什么？

### 助手

`step(edge, x)` 是一个阶跃：拿 `x` 和 `edge` 比大小，得到 **0 或 1**，没有中间值。

```
step(edge, x)  =  0   当 x < edge
                  1   当 x >= edge
```

这里：

```hlsl
float night = step(0.5, _IsNight);
```

`_IsNight` 是面板上的 Toggle：关 ≈ 0，开 ≈ 1。和 0.5 比：

| Toggle | `_IsNight` | `night` |
|---|---|---|
| 关 | 0 | **0**（白天） |
| 开 | 1 | **1**（夜间） |

下一行用它当开关：

```hlsl
float rampV = 1.0 - (night * 5.0 + row + 0.5) / 10.0;
```

- `night = 0` → 不加 5，采上半 5 行（白天）
- `night = 1` → 加 5，采下半 5 行（夜间）

用 `step` 而不是直接用 `_IsNight`，是为了把 Toggle 收成干净的 0/1，避免出现 0.3 这种半开着、V 采在两行之间的情况。等价写法是 `night = _IsNight >= 0.5 ? 1 : 0`。

## 28. 你（Wednesday, Sep 2, 2026, 11:12 PM (UTC+8)）

好了，这个已经实现了；接下来实现什么？

### 助手

Body 的 **漫反射卡通光** 已经齐了：颜色、Ramp、Threshold、G 补阴影、A 选行。LightMap 还剩 **R 和 B** 没进 shader，下一步就做 **身体高光**。

原神衣服上金属扣、丝带、绸缎那一圈亮边，不是 Ramp 来的，是 Blinn 高光，再用 ILM 的 R/B 控制「哪里亮、亮斑多大」：

| 通道 | 接下来的角色 |
|---|---|
| **R** | 高光强度（这张图衣服大面积接近 0，饰品更亮） |
| **B** | 高光范围 / 锐利度（几档离散灰） |
| G、A | 已经在用，先不动 |

建议仍只改 Body，先加一个能开关的高光，用 `return` 单独看高光再叠回 `albedo * ramp`。R/B 具体怎么乘，等你说开始再列代码。

再往后的顺序：

1. **Body 高光**（现在）
2. 边缘光（Fresnel / 逆光描边感）
3. **Hair** shader（各向异性高光，和身体不是同一套）
4. **Face** shader（SDF 阴影，不能用这套 N·L + Ramp）

描边可以放在三个材质都稳定之后。准备好了就说一声，还是只列代码、不改你的文件。

## 29. 你（Wednesday, Sep 2, 2026, 11:51 PM (UTC+8)）

先帮我看一下项目，Camera上的Post processing无法使用，是怎么回事

### 助手

先查场景里的 Camera、URP Renderer 和 Volume，看 Post Processing 是被关掉了还是链路没接上。

### 助手

原因已经定位：Camera 和 Volume 都开着，但 6 个 URP Renderer 的 `postProcessData` 是空的，后处理 Pass 根本不会跑。正在把默认资源接回去，并让 Initialize 以后不会再漏掉。

### 助手

Camera 上的勾是开着的，Volume 里也有 Bloom / Tonemapping。后处理没效果，是因为 **URP Renderer 没有后处理资源**。

六个质量档的 Renderer（`PCHigh_Renderer` 等）里都是：

```
postProcessData: None
```

这是用 `CreateInstance` 建 Renderer 时漏赋的。URP 没有这份 `PostProcessData`（Bloom 等 shader）就不会跑 Post Process Pass，所以 Camera 勾了也等于没勾。Inspector 里 Renderer 上 Post Processing 那栏会是空的。

已经把 URP 自带的默认 `PostProcessData` 接到六个 Renderer 上，并让 **Rendering Lab > Initialize Project** 以后不会再漏。回 Unity 等资源刷新，Game 视图里 Bloom 会非常明显（当前 Intensity 约 17）。

Scene 视图要单独打开右上角 **Post Processing** 叠加层，和 Camera 勾选是两回事。

## 30. 你（Thursday, Sep 3, 2026, 1:02 AM (UTC+8)）

为什么Unity6.3会出现默认没有接入postprocess的情况？

### 助手

这不是 Unity 6.3 的默认行为，是这个项目创建 Renderer 的方式漏了赋值。对照一下官方创建路径和你们的初始化代码。

### 助手

Unity 6.3 **没有**把后处理从默认配置里拿掉。用官方模板或菜单新建 URP 工程时，Renderer 会自动挂上 `PostProcessData`。你这边空着，是初始化脚本只建了对象、没走官方那第二步。

官方创建 Renderer 时是这样的：

```769:771:Library/PackageCache/com.unity.render-pipelines.universal@97f77d4dda9a/Runtime/Data/UniversalRenderPipelineAsset.cs
                    var rendererData = CreateInstance<UniversalRendererData>();
                    rendererData.postProcessData = PostProcessData.GetDefaultPostProcessData();
                    return rendererData;
```

`CreateInstance` 只得到一个空的 Renderer，`postProcessData` 默认就是 `null`。真正让 Bloom 等能跑的，是后面那次赋值。

这个项目的 `RenderingLabProjectSetup` 当时只做了第一句：

```76:79:Assets/RenderingLab/Editor/RenderingLabProjectSetup.cs
                data = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(data, path);
```

没有赋 `PostProcessData`。所以六个档位的 Renderer 都是空的，Camera 勾了也没用。

6.3 / URP 17 还把这件事藏进了 Renderer Inspector：Post Processing 那个勾，本质就是「有没有引用 `PostProcessData`」。勾上 = 填默认资源，取消 = 设成 `None`。资源是空的时候，勾看起来像开着，Pass 其实不会进。

另外 `GetDefaultPostProcessData()` 是 `internal` 的，外面的初始化脚本不能直接调，更容易漏。所以不是 6.3 默认关闭后处理，而是 **脚本创建 Renderer 时没补官方那一行**。现在已经在 Initialize 里补上了，菜单新建的 URP 工程本来就不会有这个问题。

## 31. 你（Thursday, Sep 3, 2026, 9:13 PM (UTC+8)）

好的，明白了； 接下来是做金属高光了吗？

### 助手

对，接下来就是 **Body 高光**。衣服大面积没有，金属扣、金边、丝带那些亮斑才走这套。还是 Blinn（`N·H`），不是头发那种各向异性。

这张 LightMap 上：

| 通道 | 作用 |
|---|---|
| **R** | 高光强度。布料≈0，饰品偏亮 |
| **B** | 高光锐度。越高斑越小、越「金属」 |
| G / A | 已经在用，别改 |

`ilm` 采过了。还要视角：用已有的 `positionWS` 算 `V` 和半角向量 `H`。

**Properties 里加：**

```hlsl
_SpecColor("Spec Color", Color) = (1, 1, 1, 1)
_SpecIntensity("Spec Intensity", Range(0, 5)) = 1
```

**CBUFFER 里加：**

```hlsl
half4 _SpecColor;
half _SpecIntensity;
```

**frag 里，采完 Ramp 之后、最后 `return` 之前：**

```hlsl
// 视角和半角：高光跟着眼睛走，不是贴在表面上
float3 V = GetWorldSpaceNormalizeViewDir(IN.positionWS);
float3 H = normalize(L + V);
float NdotH = saturate(dot(N, H));

// R：哪里能亮；B：pow 的指数，越大越尖
half specMask = ilm.r;
half specGloss = ilm.b;
float spec = pow(NdotH, lerp(8.0, 128.0, specGloss));
spec *= specMask;
spec *= rampU; // 阴影里关掉，避免背光金属还在闪

half3 specCol = spec * _SpecColor.rgb * _SpecIntensity * mainLight.color;

// 先只看高光：
// return half4(spec.xxx, 1);

return half4(albedo.rgb * ramp * mainLight.color + specCol, 1);
```

先 `return spec`：应该只有扣子、金边是白点，裙子是黑的。对了再改成相加那行。

`lerp(8, 128, B)` 是起点：B=0 的布即使 R 漏一点也不会尖；金属 B 高就是小亮点。范围不对再改这两个数，先不要动 R/B 的用法。

## 32. 你（Thursday, Sep 3, 2026, 9:28 PM (UTC+8)）

帮我解释一下 float3 V 和 float3 H的算法，最好能带示意图解释；

## 33. 你（Saturday, Sep 5, 2026, 12:07 AM (UTC+8)）

我加另一个CharaDebugView.cs的枚举文件，我要在Genshin_Body.shader的属性栏里来查看不同的颜色，但是现在这个CS脚本有报错，你帮我看一下

### 助手

脚本里是拼写错误：`pubic` 应写成 `public`，类型没编出来，材质上的 `[Enum]` 也找不到它。

### 助手

报错是因为写成了 `pubic enum`，C# 关键字是 `public`。改完后枚举才能编译，材质上的 `[Enum(RenderLab.CharaDebugView)]` 才能认出下拉项。

另外 `_DebugView` 默认值是 **9**，枚举目前只有 0～8。编译通过后把默认改成 `3`（`FinalColor`），否则一打开材质会落在一个不存在的项上。

## 34. 你（Saturday, Sep 5, 2026, 1:55 PM (UTC+8)）

OK,一切正常了，我在材质球面板加了一个_DebugView, 方便查看各种计算结果和贴图通道的显示是否正常； 高光已经做好了，下一步做什么？

### 助手

Body 的漫反射 + Ramp + ILM + 高光这条已经齐了。下一步仍留在 Body 上做 **边缘光（Rim）**。

原神衣服在逆光、侧光时，轮廓上会有一圈跟着视线走的亮边，用你已经算过的 `V`：

```
rim = saturate(1 - N·V)
```

再乘一个「只在背光侧出现」的项（例如 `1 - NdotL` 或 `1 - rampU`），避免正脸也一圈白边。颜色和宽度用两个材质参数即可，算完加到 `finalColor` 上。Debug 枚举可以再加一项 `rim`。

另外现在 `finalColor` 还没把高光加进去：

```hlsl
half3 finalColor = albedo.rgb * ramp * mainLight.color + specCol;
```

`FinalColor` 调试项才会是「底色 + Ramp + 高光」。Rim 叠在这后面。

再往后：

1. **Body 边缘光**（现在）
2. **Hair** shader（切线各向异性高光，和身体不是同一套）
3. **Face** shader（SDF，不能用这套 N·L）
4. 描边 Pass

要做 Rim 的话还是只列代码，不改你的文件。

## 35. 你（Saturday, Sep 5, 2026, 2:00 PM (UTC+8)）

先解释一下rim的做法，这个rim不是应该要做基于深度的rim吗？这样才能保证rim的宽度是一致的，不用帮我改代码

### 助手

对，要宽度稳定，应该用 **屏幕空间深度 Rim**，不是 `1 - N·V`。

## Fresnel 为什么会忽宽忽窄

`rim = 1 - N·V` 看的是「法线离开视线有多快」。

圆肩膀、大腿这种法线转得慢，要转过很大一块表面 `N·V` 才掉下来，边会 **又宽又糊**。裙褶、金属锐边法线一下子翻过去，边 **又细又硬**。宽度跟几何曲率绑在一起，没法在屏幕上画出「一圈一样宽的光」。

它也不是轮廓检测：正对相机的球面边缘也会亮，即使根本没有和背景断开。

## 深度 Rim 在比什么

不看曲率，看 **往轮廓外挪几个像素，后面是不是更远**。

```
屏幕上当前像素 P
       ·  → 沿「视线空间法线的 XY」挪 _RimWidth 个像素 →  · Q

深度(P) = 角色自己
深度(Q) = 背景 / 身后另一块身体  →  差很大  →  这是轮廓，亮 Rim
深度(Q) ≈ 深度(P)                 →  还在同一块表面上  →  不亮
```

`_RimWidth` 是 **屏幕像素**（再乘 `1/屏幕分辨率` 变成 UV 偏移）。肩和褶用同一套像素宽度，所以看起来是一圈匀的边。

和描边 Pass 不是一回事：描边是扩一圈网格；Rim 仍画在角色表面上，只是轮廓附近被点亮。

## 和光的关系

原神那种「逆光才一圈亮」是深度 Rim **再乘背光**：

- 轮廓：深度差
- 哪一侧亮：`1 - NdotL` 或 `1 - rampU`（阴影那边）

正对主光的那一侧即使是轮廓，也往往压暗，避免整圈白边。

手臂挡在身体前面时，中间那条缝深度差也会亮——原神经常 **要** 这种叠层亮边，不是 bug。

## 工程上要注意

- URP 要开 **Depth Texture**（你们 PC 档一般已经开了）
- 用 `SampleSceneDepth` 采当前和偏移后的深度，转成 Linear Eye Depth 再相减
- 偏移方向用 **view space 法线的 xy**，才会沿着可见轮廓往外探，而不是随便往屏幕右下
- 远近：用「差了多少米」当强度时，远处会变弱；若只判断「差是否超过一个阈值」，宽度仍由像素偏移决定，更稳

下一步写 Body 时按这个做即可：深度差出轮廓，背光做遮罩，再加到 `finalColor`。Fresnel 只适合当很便宜的近似，不适合当「宽度一致」的角色 Rim。

## 36. 你（Saturday, Sep 5, 2026, 2:03 PM (UTC+8)）

好的，先帮我做深度的算法，我要把这个深度的显示效果放到_DebugView里面检查；

### 助手

先把深度采样和深度差 Rim 接到 shader 与 DebugView，方便你在材质里单独检查。

## 37. 你（Saturday, Sep 5, 2026, 2:04 PM (UTC+8)）

不用帮我写代码，只要一步步列出来即可；

### 助手

按这个顺序加，先能在 Debug 里看到深度，再做偏移比较。先不要叠到 `finalColor`。

## 1. 确认深度图有在出

URP Asset 里 **Depth Texture** 打开。Camera 的 Depth Texture 用 Pipeline 设置即可。没有这张图，后面采到的全是 0 或 1。

## 2. 头文件

`Lighting.hlsl` 后面再 include：

```hlsl
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
```

## 3. 属性（先给 Debug 用）

- `_RimWidth`：沿轮廓往外挪多少 **像素**，先 2～8
- `_RimScale`：深度差放大多少，方便看清，先 1～5

CBUFFER 里各加一个对应变量。

## 4. 采当前像素深度

frag 里用已经有的 `positonCS`：

```hlsl
float2 screenUV = GetNormalizedScreenSpaceUV(IN.positonCS);
float rawDepth = SampleSceneDepth(screenUV);
float linear01 = Linear01Depth(rawDepth, _ZBufferParams);
```

`linear01` 是 0～1 灰度：近黑远白（或反过来，看你怎么看）。这就是「深度是否采对了」。

## 5. 深度差（Rim 算法本体）

法线转到视线空间，用它的 **xy** 当屏幕上的外探方向：

```hlsl
float3 normalVS = TransformWorldToViewDir(N, true);
float2 offsetDir = normalize(normalVS.xy);
float2 offsetUV = offsetDir * _RimWidth / _ScreenParams.xy;
```

再采一次偏移后的深度，转成 **线性眼睛深度**（单位是米，才能相减）：

```hlsl
float depthP = LinearEyeDepth(rawDepth, _ZBufferParams);
float depthQ = LinearEyeDepth(SampleSceneDepth(screenUV + offsetUV), _ZBufferParams);
float depthRim = saturate((depthQ - depthP) * _RimScale);
```

轮廓上 Q 更远，差为正，`depthRim` 接近 1；平面上两次采到同一块表面，差≈0。

`normalVS.xy` 接近 0 时（正对相机）不要 normalize，直接让 `depthRim = 0`。

## 6. 接到 `_DebugView`

枚举在 `specular = 8` 后面加两项，例如：

- `9` = `sceneDepth` → 显示 `linear01`
- `10` = `depthRim` → 显示 `depthRim`

`CharaLightingDebugColor` 末尾加两个参数、两个 `if`。Body 的调用处把这两个数传进去。

材质默认 `_DebugView` 现在是 9，加完会正好进 `sceneDepth`。

## 7. 怎么判断对了

| Debug | 正常 |
|---|---|
| `sceneDepth` | 角色身上有近深远浅的灰阶，不是整片洋红/纯黑 |
| `depthRim` | 只有轮廓一圈白，中间黑；拉大 `_RimWidth` 圈变宽 |

`sceneDepth` 全黑/全白：深度图没进来。`depthRim` 整身花、没有一圈：偏移方向或用了 `Linear01` 去相减（要用 `LinearEyeDepth`）。

先做到 Debug 里这两项对，下一步再乘背光、加到 `finalColor`。

## 38. 你（Saturday, Sep 5, 2026, 2:44 PM (UTC+8)）

我想把screenuv放到_DebugView,现在有个报错，帮我看一下

### 助手

报错是因为 `CharaLightingDebugColor` 返回 `half3`，`screenSpaceUV` 那支却 `return` 了 `float2`，类型对不上。`rawDepth` / `linear01` 是标量，HLSL 能扩成三通道；`float2` 不能。

### 助手

已改成 `half3(screenSpaceUV, 0)`：U 走红、V 走绿，B 为 0。Debug 选 `screenSpaceUV` 时，角色上应是从左下（黑）到右上（黄）的渐变，这是屏幕 UV，不是模型 UV。

## 39. 你（Saturday, Sep 5, 2026, 3:42 PM (UTC+8)）

float2 screenUV = GetNormalizedScreenSpaceUV(IN.positonCS);
float rawDepth = SampleSceneDepth(screenUV);
float linear01 = Linear01Depth(rawDepth, _ZBufferParams);  这几行代码_DebugView,rawDpeth显示的是全黑，liear01显示的是全白；是不是不对？

### 助手

`rawDepth` 全黑、`linear01` 全白正好对得上「采到了空深度」。先核对 URP 深度图拷贝时机和你们 Renderer 的设置。

### 助手

对，这对组合说明 **没采到角色自己的深度**，不是 `Linear01Depth` 写反了。

Unity 用 **Reversed Z**：近处 ≈ 1（白），远处 / 清空 ≈ 0（黑）。

| 你看到的 | 实际采样值 | 含义 |
|---|---|---|
| `rawDepth` 全黑 | ≈ **0** | 当成无穷远，或深度图是空的 |
| `linear01` 全白 | ≈ **1** | `Linear01Depth(0)` = 远平面，整身在「无穷远」 |

要是采到角色，近处角色 `rawDepth` 应接近白，`linear01` 应是很深的灰（几乎黑，因为 2～5 米相对 Far 1000 仍然很近）。现在正好反了。

原因在 URP 拷贝时机，不在这三行。Body 是 **Opaque + UniversalForward**。你们 Renderer 现在是：

- **Depth Texture Mode = After Transparents**（`CopyDepthMode = 1`）
- **Depth Priming = Disabled**

`_CameraDepthTexture` 要等透明都画完才拷。你在不透明 Forward 里 `SampleSceneDepth`，读的是还没填的图（清成 0），所以黑 + 白。

就算改成 After Opaques，**同一个不透明 Pass 里仍然读不到本帧深度**——拷贝发生在所有不透明结束之后。深度 Rim 要拿邻居像素比深度，必须在 Forward **之前**就有一张完整深度图。

做法（Inspector，先不用改 shader）：

1. 打开当前用的 Renderer（例如 `PCHigh_Renderer`）
2. **Depth Priming** 改成 **Forced**，或 **Depth Texture Mode** 改成 **Force Prepass**
3. 确认 URP Asset 里 **Depth Texture** 仍是开的

这样会先做 Depth Prepass → 拷到 `_CameraDepthTexture` → 再画角色颜色。这时再看 Debug：

- `rawDepth`：角色偏白（近），背景/远处偏黑  
- `linear01`：角色很暗的灰，不是一片白

想先确认公式、暂时不碰 Renderer，可以用顶点自己的深度对比（这不是场景深度，不能做 Rim）：

```hlsl
float ownLinear01 = Linear01Depth(IN.positonCS.z / IN.positonCS.w, _ZBufferParams);
```

`screenUV` 渐变若是对的，UV 没问题，就是深度图在这个 Pass 里还是空的。

## 40. 你（Saturday, Sep 5, 2026, 3:46 PM (UTC+8)）

没看明白，是我工程设置有问题，还是怎么回事？

### 助手

是 **工程设置和这个算法不匹配**，不是那三行采样写错了，项目也没坏。

可以想成两条时间线：

1. **你的角色 shader** 在画不透明颜色的时候，就要去读 `_CameraDepthTexture`
2. **URP 真正把深度拷进这张图**，发生在「所有不透明都画完」甚至「透明都画完」之后

你现在的 Renderer 是：深度图 **等透明画完再拷**，而且 **没有 Depth Prepass**。所以角色在画自己的时候，那张深度图还是空的。空深度在 Reversed Z 里是 0 → Debug 里 `rawDepth` 全黑、`linear01` 全白。

这不是 Bloom、质量档那种坏了，是：**默认设置够后处理用（后处理在最后，那时深度已经有了），不够在不透明 Pass 里做深度 Rim。**

要让 Rim 能读到本帧角色深度，改当前正在用的那个 Renderer（一般是 `PCHigh_Renderer`）：

- **Depth Texture Mode** 改成 **Force Prepass**  
  或  
- **Depth Priming** 改成 **Forced**

意思是：先单独画一遍只有深度 → 拷进 `_CameraDepthTexture` → 再画角色颜色。这时你再采，才是角色自己的深度。

改完用 Debug 看：`rawDepth` 角色应该偏白，`linear01` 应该是很暗的灰，不会再是「黑 + 白」一整片。

## 41. 你（Saturday, Sep 5, 2026, 3:48 PM (UTC+8)）

除了这个改法，还有其他的修改方案吗？ 是不是单独写一个Depth Prepass也可以？

### 助手

可以，不一定要改全局的 Depth Priming。核心就一件事：**颜色 Pass 开始之前，必须已经有一张能采的深度图。** 官方 Force Prepass 只是其中一种做法。

## 方案对比

**1. 改 Renderer（上次说的）**  
URP 帮你做全场景 Depth Prepass，再拷到 `_CameraDepthTexture`。角色、地面、道具都会进这张图。改动最小，但整个场景都多画一遍深度。

**2. 自己做角色 Depth Prepass（可以）**  
单独一张 RT，只画角色（按 Layer）。颜色 Pass 采这张图，不采 `_CameraDepthTexture`。  
这样 **不用改** Copy Depth / Priming。轮廓相对「空像素」（清成远平面）就会亮，和背景是墙还是天空无关，更像角色 Rim。手臂挡身体那种叠层亮边也还在。

这才是「单独写 Prepass」。一般用 Renderer Feature：先画角色深度 → Forward 里采自定义 `_CharDepthTexture`。

**3. 只在 shader 里加 `DepthOnly` Pass —— 不够**  
URP 的 Depth Prepass 会调用 `"LightMode" = "DepthOnly"`。你只加这个 Pass、却不让管线跑 Prepass，这个 Pass **根本不会执行**。它是给方案 1 用的，不是独立方案。

**4. 把 Rim 挪到不透明之后**  
等 URP 按 After Opaques 拷完 `_CameraDepthTexture`，再用一个 AfterRenderingOpaques 的 Pass / Feature 去画 Rim。颜色 Pass 不必采深度。多一次绘制，设置可以维持默认。

**5. 全屏后处理描边**  
不透明结束后对深度做 Sobel。宽度也稳，但作用范围是整屏，要用模板/Layer 才只套角色，和「画在衣服上的 Rim」不是同一套。

---

学习阶段建议仍用 **方案 1**：先确认采样和 Debug 是对的。  
以后要角色和场景深度脱钩、又不想全场景 Prepass，再做成 **方案 2**。

自己写 Prepass 时记住：必须在 Forward **之前**画进一张你绑定好的深度图，并且 shader 采的是那张图。只加 DepthOnly、不管线执行它，深度 Debug 还会是黑 + 白。

## 42. 你（Saturday, Sep 5, 2026, 3:51 PM (UTC+8)）

比较成熟的方案是哪一种？

### 助手

分两层看：**引擎里最稳的**，和 **原神这种角色 Rim 最常用的**，不是同一套。

**在 Unity URP 里最成熟的，是官方 Depth Prepass（Force Prepass / Depth Priming Forced）。**  
管线已经支持，Frame Debugger 能看，SSAO、软粒子也能共用这张 `_CameraDepthTexture`。商业 URP 项目做「不透明 shader 里采场景深度」，几乎都走这条，而不是自己再写一份全场景深度。

**原神那种角色边缘光，更成熟的是「只画角色的深度 Prepass」（上次的方案 2）。**  
角色单独一张深度图，Forward 只采它。轮廓相对空像素发光，不会因为身后一厘米有墙就亮一圈；手臂挡身体仍有叠层亮边。角色是特殊前向、和场景光照拆开的项目（包括米哈游风格）基本都是这个思路。代价是要维护一个 Renderer Feature + Layer。

不推荐当「成熟方案」的：

- **只加 DepthOnly、不让管线跑 Prepass**：等于没做  
- **不透明之后再画一次 Rim**：能用，但拆成两段，调试和合批都别扭  
- **全屏 Sobel**：成熟的是描边/后处理，不是衣服上那层带背光的 Rim

你现在在 URP 里学、先把 Debug 采对：用 **官方 Prepass**。  
以后要角色和场景脱钩、Rim 只跟身体轮廓走：再做成 **角色专用深度图**。那才是这套美术效果的成品形态，不是另起一套引擎深度。

## 43. 你（Saturday, Sep 5, 2026, 3:58 PM (UTC+8)）

我把Depth priming mode改成Forced以后，提示不能开msaa抗锯齿，同时还有一个提示: On Android, ios,appletv, Unity performs depth priming only in forced mode. 角色模型现在也不显示了

### 助手

角色消失和那两条提示是同一件事。先核对 shader 有没有 DepthOnly，以及当前 MSAA / Priming 设置。

### 助手

三条是分开的，角色消失才是真正的问题。

## 角色为什么没了

`Genshin_Body` **只有 `UniversalForward`，没有 `DepthOnly` Pass。**

Depth Priming = Forced 时 URP 会：

1. 先用 `DepthOnly` 画一遍深度  
2. 再画颜色，并把深度测试改成 **Equal**（必须和 Prepass 里的深度完全一致才着色）

你的 shader 不参加第 1 步，深度缓冲里没有这个角色。第 2 步 Equal 对不上 → **一个像素都不画**。地面如果还在，多半是 Lit 自带 DepthOnly。

所以不是模型坏了，是 Priming 和当前这个 shader 不配套。先把 **Depth Priming 改回 Disabled**，角色会回来。

## 那两条提示

**不能开 MSAA：** URP 规定 Priming 和 MSAA 互斥。`PCHigh_Pipeline` 里 MSAA 是 4x，所以会警告。这是引擎限制，不是报错。

**Android / iOS 那段：** 说明文档。移动端就算设 Auto 也不会做 Priming，只有 Forced 才会。和 PC 上模型消失无关，可以当没看见。

## 建议怎么走

不要用 Forced Priming 来给 Rim 采深度。改回 Disabled 之后，用 **Depth Texture Mode = Force Prepass**（拷深度用 Prepass，颜色 Pass 仍是 LEqual，角色还在）。

同时给 Body shader **补一个只写深度的 DepthOnly Pass**，角色才会进 `_CameraDepthTexture`，Rim 才能采到自己。没有这个 Pass，即使用 Force Prepass，深度图里也没有她。

DepthOnly 很短：同一个 vert，frag 只 `return 0`，Tags 用 `"LightMode" = "DepthOnly"`，`ZWrite On`，`ColorMask 0`。

顺序：

1. Priming 改回 **Disabled**（人先出现）  
2. MSAA 可以继续开  
3. Copy Depth 改成 **Force Prepass**  
4. shader 加上 **DepthOnly**  
5. 再看 `rawDepth` / `linear01`

Forced Priming 是为了 Early-Z 省填充，不是为了 Rim；你这个 shader 现在也用不了它。

## 44. 你（Saturday, Sep 5, 2026, 4:08 PM (UTC+8)）

copy depth在哪里设置？

### 助手

Inspector 里不叫 Copy Depth，叫 **Depth Texture Mode**。

1. Project 窗口打开当前质量档用的 Renderer，例如 `Assets/RenderingLab/Settings/PCHigh_Renderer`
2. Inspector 里找到 **Depth Texture Mode**
3. 改成 **Force Prepass**（另外两项是 After Opaques / After Transparents）

这是 **Renderer** 上的项，不是 Camera，也不是 URP Pipeline Asset。Pipeline 里那个 **Depth Texture** 勾选只表示要不要这张图；**何时生成** 由 Renderer 的 Depth Texture Mode 决定。

Depth Priming 请保持 **Disabled**，避免角色再次消失。改完再看 `rawDepth` / `linear01`。角色要进深度图，Body shader 还需要 `DepthOnly` Pass。

## 45. 你（Saturday, Sep 5, 2026, 4:10 PM (UTC+8)）

depthOnly pass 这个pass的代码怎么加？把代码呈现一下；

### 助手

贴在 **ForwardLit 那个 Pass 的右花括号后面**、`SubShader` 结束之前。DepthOnly 是独立 Pass，和颜色 Pass 分开编译，所以 `CBUFFER` 要再写一份（和上面保持一致，SRP Batcher 才不会报不兼容）。

```hlsl
Pass
{
    Name "DepthOnly"
    Tags { "LightMode" = "DepthOnly" }

    Cull Back
    ZWrite On
    ColorMask R
    ZTest LEqual

    HLSLPROGRAM
    #pragma vertex DepthOnlyVertex
    #pragma fragment DepthOnlyFragment

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        half4 _BaseColor;
        half4 _SpecColor;
        half _SpecIntensity;
        float _IsNight;
        half _ShadowThreshold;
        half _ShadowSmooth;
        float _RimWidth, _RimScale;
        float _DebugView;
    CBUFFER_END

    struct DepthAttributes
    {
        float4 positionOS : POSITION;
    };

    struct DepthVaryings
    {
        float4 positionCS : SV_POSITION;
    };

    DepthVaryings DepthOnlyVertex(DepthAttributes IN)
    {
        DepthVaryings o;
        o.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
        return o;
    }

    half DepthOnlyFragment(DepthVaryings IN) : SV_TARGET
    {
        return IN.positionCS.z;
    }
    ENDHLSL
}
```

要点：

- `"LightMode" = "DepthOnly"`：URP Prepass 只找这个 Tag，名字必须一字不差
- `ColorMask R`：只写深度，不写颜色
- frag 返回 `positionCS.z`：把本片元深度写入，给后面的 `_CameraDepthTexture` 用

Renderer 保持 **Depth Priming = Disabled**，**Depth Texture Mode = Force Prepass**。加完再看 `rawDepth`：角色应偏白，不再整片黑。

## 46. 你（Saturday, Sep 5, 2026, 4:13 PM (UTC+8)）

这里CBUFFER每个pass里面都要写一份相同的，有点麻烦； 可以把CBUFFER放到pass之外，Subshader里面，也就是 LOD 100下面的位置？

### 助手

不能把 `CBUFFER` 直接写在 `LOD 100` 下面。那一块是 **ShaderLab**，只认 `Tags`、`Pass`、`UsePass` 这些，不认 HLSL。写在那儿不会进任何 Pass，还会报错。

每个 `HLSLPROGRAM` 都是单独编译的，所以 Pass 里的 HLSL 互相看不见。要共用，用 Unity 的 **`HLSLINCLUDE`**：写在 SubShader 里、所有 Pass 之前，会自动插进每一个 `HLSLPROGRAM`。

```hlsl
SubShader
{
    Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
    LOD 100

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        half4 _BaseColor;
        half4 _SpecColor;
        half _SpecIntensity;
        float _IsNight;
        half _ShadowThreshold;
        half _ShadowSmooth;
        float _RimWidth, _RimScale;
        float _DebugView;
    CBUFFER_END
    ENDHLSL

    Pass
    {
        Name "ForwardLit"
        ...
        HLSLPROGRAM
        // 这里不要再写一份 CBUFFER
        ...
        ENDHLSL
    }

    Pass
    {
        Name "DepthOnly"
        ...
        HLSLPROGRAM
        // 这里也不要写 CBUFFER，HLSLINCLUDE 已经带进来了
        ...
        ENDHLSL
    }
}
```

注意：

- 各 Pass 里原来的 `CBUFFER` **删掉**，否则重复定义
- `HLSLINCLUDE` 里不要放 `#pragma vertex / fragment`
- Forward 里已经 `#include Core.hlsl` 也没关系，有 include guard

另一个做法是把 CBUFFER 放到 `GenShin_Shared.hlsl`，每个 Pass `#include` 一次。`HLSLINCLUDE` 更省事，DepthOnly、以后的 ShadowCaster 都会自动带上同一份。

## 47. 你（Saturday, Sep 5, 2026, 4:21 PM (UTC+8)）

@Assets/RenderingLab/Cha/Yuanshen/Shader/Genshin_Body.shader:76-78我把CBUFFER_START移动到pass之外，HLSLINCLUDE和ENDHLSL里面之后，这些贴图采样要从pass里面移动出去吗？ Onlydepth这个pass需要这些贴图采样器吗？

### 助手

**DepthOnly 不需要这些贴图。** 那个 Pass 只做 `物体空间 → 裁剪空间` 并写出深度，不采 `_BaseMap` / `_LightMap` / `_RampMap`。采样器留在 ForwardLit 里即可，**不必**挪到 `HLSLINCLUDE`。

和 CBUFFER 不是同一回事：

| | 要不要每 Pass 一份 |
|---|---|
| **CBUFFER（材质数值）** | 要。SRP Batcher 要求各 Pass 的 `UnityPerMaterial` 布局一样，所以放进 `HLSLINCLUDE` |
| **TEXTURE2D / SAMPLER** | 谁采样谁声明。不是 CBUFFER，DepthOnly 不声明也没问题 |

ForwardLit 继续留着你标的 76–78 行。以后 ShadowCaster、第二个颜色 Pass 如果也不采贴图，同样不用带。只有某个 Pass 真的 `SAMPLE_TEXTURE2D` 了，才在那个 Pass（或以后真有多个 Pass 都采，再考虑放进 `HLSLINCLUDE`）里声明。

挪出去也可以编译，DepthOnly 只是多几个用不到的声明，没有好处。现在这样拆是对的。

## 48. 你（Saturday, Sep 5, 2026, 4:27 PM (UTC+8)）

ok，现在在_DebugView里调试linear01 随着角色模型离摄像机距离的远近有黑白变化了，rawDepth仍然是黑色的；

### 助手

`linear01` 能随距离变，说明深度已经采对了。`rawDepth` 看起来一片黑，多半是深度缓冲的非线性，不是又采空了。

### 助手

这是正常现象，说明 **Force Prepass + DepthOnly 已经生效**。判断标准是 `linear01`，不是 `rawDepth`。

`SampleSceneDepth` 拿到的是 **GPU 非线性深度**。精度几乎全堆在近平面：角色在 2 米还是 20 米，raw 可能都是 `0.001` 这种数。显示成 0～1 的颜色，就是一片黑，走远走近也看不出差别。

`Linear01Depth` 就是把这个数 **解成「近 0、远 1」的线性距离**。所以只有它会随摄像机远近变灰变亮。你现在能看到这个变化，就是采到角色自己的深度了。

可以记：

| Debug | 用来干什么 |
|---|---|
| `rawDepth` | 看「有没有采到纹理」。接近 0 或接近 1 的一整片都可以，**不要**用它看远近 |
| `linear01` | 看远近是否对。近黑远白（或反过来随距离变）就对了 |

下一步做 Rim 时，相减要用 **`LinearEyeDepth`（米）**，不要用 raw，也不要用 `linear01`。`linear01` 只适合 Debug；两米和二十米的差在 0～1 里太小，相减几乎是 0，轮廓会出不来。

`rawDepth` 不用再纠结。继续做偏移采样，用两次 `LinearEyeDepth` 的差当 `depthRim` 即可。

## 49. 你（Saturday, Sep 5, 2026, 5:20 PM (UTC+8)）

现在基于深度的Rim通过调试看以及正常了，我截图给你看

### 助手

对，这就是深度 Rim 该有的样子：中间是黑的（`depthRim ≈ 0`），轮廓一圈宽度比较匀的白边（外探到更远的像素）。比 Fresnel 那种肩宽褶窄要稳。

脸和头发还是别的材质，所以没有整片变黑，只有 Body 在走 Debug。手臂贴在裙子上如果也有细白缝，那是叠层深度差，原神经常要留着。

下一步是把它当成光，而不是 Debug 贴图：

1. **背光遮罩**：乘 `1 - rampU`（或 `1 - NdotL`），让亮边主要出现在背光一侧，避免正对相机一圈白描边。
2. **颜色 / 强度**：`_RimColor * _RimIntensity * depthRim * 遮罩`。
3. **叠进最终色**：  
   `finalColor += rimCol`  
   Debug 选 `FinalColor` 时应该是衣服颜色 + 这一圈。

深度差和偏移已经不用再改。你要叠颜色的话还是只列代码，不直接改文件。

## 50. 你（Saturday, Sep 5, 2026, 5:32 PM (UTC+8)）

是的，只列代码，不直接改文件；

### 助手

深度差那几行不用动。在后面加背光遮罩和颜色，再叠进 `finalColor`。

**Properties**（`_RimScale` 下面）：

```hlsl
_RimColor("Rim Color", Color) = (1, 1, 1, 1)
_RimIntensity("Rim Intensity", Range(0, 5)) = 1
```

`[DepthRim]` 请改成 `[Header(Depth Rim)]`，否则不是分组标题。

**HLSLINCLUDE 的 CBUFFER** 里补：

```hlsl
half4 _RimColor;
half _RimIntensity;
```

**frag 里，`depthRim` 算完之后、`finalColor` 改成：**

```hlsl
// 只在背光侧亮，避免正脸一圈白描边
float rimMask = saturate(depthRim) * (1.0 - rampU);
half3 rimCol = rimMask * _RimColor.rgb * _RimIntensity * mainLight.color;

half3 finalColor = albedo.rgb * ramp * mainLight.color + specCol + rimCol;
```

Debug 不用改签名：选 `depthRim` 仍看纯轮廓；选 `FinalColor` 应是衣服 + 高光 + 这一圈。若要单独看「乘过背光之后」的 Rim，枚举再加一项 `rimMask = 13`，debug 函数里 `return rimMask`，调用处把 `rimMask` 传进去。

先把 `_RimIntensity` 放在 0.5～1，颜色偏浅蓝或跟随主光。正对光那一侧边应变暗，侧光/逆光轮廓才亮。

## 51. 你（Saturday, Sep 5, 2026, 5:47 PM (UTC+8)）

OK, 已经实现了，接下来是做哪个模块了？

### 助手

Body 这条已经齐了：漫反射、Ramp、ILM、高光、深度 Rim。接下来开 **Hair shader**（新文件，不要再往 Body 里堆）。

头发和身体主要差在高光：身体是 `N·H` 的点状高光，头发要 **沿发丝方向的条状高光**（切线 / 各向异性，ShiftTangent 那种）。漫反射仍可以走半兰伯特 + Ramp + LightMap，和 Body 类似，能复用 `GenShin_Shared.hlsl` 里的 Debug。

再往后：

1. **Hair**（现在）
2. **Face**（SDF 脸阴影，不能用这套 N·L）
3. **描边 Pass**（三个材质都稳定之后）

Hair 用独立 shader：`RenderingLab/Yuanshen/Hair`。准备好了还是只列 Properties / 结构，不直接改文件。

## 52. 你（Saturday, Sep 5, 2026, 5:48 PM (UTC+8)）

是的，只列代码结构和功能实现思路的解释，不改文件

### 助手

先对照 Body 的骨架，说明 Hair 要复用什么、高光为什么要换成切线各向异性。

### 助手

新建 `Genshin_Hair.shader`，路径 `RenderingLab/Yuanshen/Hair`。从 Body **复制整份骨架**（HLSLINCLUDE、ForwardLit、DepthOnly、Debug），不要在 Body 里加 keyword。贴图用头发那套：`Hair_Diffuse`、`Hair_Lightmap`、`Hair_Shadow_Ramp`。LightMap 通道先用 Debug 对一遍，不要默认和身体同一套。

## 和 Body 的关系

| 模块 | Hair |
|---|---|
| 半兰伯特 + Threshold + Ramp | **同样做**，先保证头发明暗对 |
| LightMap.G 补阴影、A 选行 | 先 Debug 确认，再按头发图接 |
| 深度 Rim | 可原样搬，DepthOnly 也要有 |
| 高光 | **整段换掉**，不要再用 `pow(NdotH, …)` |

原神头发高光是 **沿发丝的一条亮带**，不是金属那种点。方向来自网格的 **切线 T**（发丝走向），不是法线 N。

## 建议实现顺序

**1. 只出漫反射**  
vert 暂时可以和 Body 一样。头发材质换上新 shader，Debug 看 albedo / ramp / ilm。这一步对了再动高光。

**2. 把切线送进片元**  
`Attributes` 加 `float4 tangentOS : TANGENT;`  
`GetVertexNormalInputs` 已经能算出 `tangentWS`。`Varyings` 加 `tangentWS`。  
Debug 先把 `tangentWS` 编码成颜色（和 normal 一样 `*0.5+0.5`）：发片上应是平滑的方向色，乱掉说明模型切线不对。

**3. Kajiya-Kay 条状高光**  
`H` 仍是 `normalize(L+V)`。不要用 `N·H`，用 **T 和 H 的夹角正弦**：

```hlsl
float3 T = normalize(IN.tangentWS.xyz);
float TdotH = dot(T, H);
float sinTH = sqrt(saturate(1.0 - TdotH * TdotH));
float spec = pow(sinTH, _HairShininess);
```

`T·H = 0`（光线垂直发丝）时 `sinTH = 1`，高光最亮，所以是一条带。`_HairShininess` 越大条越细。

**4. 双层高光 + 偏移**  
原神一般两层：一条细白主高光、一条稍宽、略偏色的副高光。把 T 沿 N 拧一点再算 spec：

```hlsl
float3 ShiftHairTangent(float3 T, float3 N, float shift)
{
    return normalize(T + N * shift);
}
```

**不要**把函数叫 `ShiftTangent`：URP 的 `Lighting.hlsl` 里已经有同名函数，重定义会编不过。

两层示例思路：

```hlsl
float3 T1 = ShiftHairTangent(T, N, _SpecShift1);
float3 T2 = ShiftHairTangent(T, N, _SpecShift2);
// 各自 pow(sinTH, exp)，乘颜色和强度，再相加
specHair *= rampU; // 阴影里关掉，和身体一样
```

UV 的 V 或 LightMap 某一通道可以当 per-pixel shift，用来把高光拆成一缕一缕，先用常数 shift 出条状，再加贴图。

**5. 最后再接 Rim**  
漫反射 + 头发高光稳定后再拷 Body 的深度 Rim。

## 文件结构（逻辑顺序）

```
Shader "RenderingLab/Yuanshen/Hair"
  Properties     // 与 Body 类似，高光改成 Shift1/2、Shininess1/2、两套颜色
  HLSLINCLUDE    // 同一份 CBUFFER
  Pass ForwardLit
    vert: position / normal / tangent / uv
    frag: 漫反射(先) → 头发高光(后) → Rim
  Pass DepthOnly // 与 Body 相同，给深度 Rim 用
```

`GenShin_Shared.hlsl` 的 Debug 直接 include。枚举可加 `tangentWS`、`hairSpec`。

第一周只做到：**头发网格用新 shader，Ramp 明暗正确，切线 Debug 能看。** 条状高光是下一刀。需要某一段的具体公式时再说，我仍只列代码。

## 53. 你（Saturday, Sep 5, 2026, 6:32 PM (UTC+8)）

头发这里很奇怪，我在_DebugView里做了一个Rim mask的检查，是1.0-rampU, 身体部分是好的，但是头发部分是灰色的，还有杂色；帮我看看是怎么回事；

### 助手

头发 `1-rampU` 发灰带噪，多半是 `rampU` 没到 0/1。先对一下 Hair 的 `rampU` 算法和 LightMap 通道。

### 助手

不是 Rim 公式坏了，是头发这张 LightMap **还在按颜色贴图导入**，再乘进 `rampU` 之后，亮部到不了 1。

你现在头发里还是 Body 那套：

```hlsl
half foldShadow = saturate(ilm.g * 2);
rampU *= foldShadow;
```

身体 LightMap 的 G 大面积是 **128**，关 sRGB 时 `128/255 ≈ 0.5`，再 `* 2` 得到 **1**，朝光一面 `rampU = 1`，`1 - rampU` 就是纯黑。

头发这张 `Hair_Lightmap` 的 `.meta` 仍是：

- **sRGB 开着**
- **Generate Mip Maps 开着**
- **Filter = Bilinear**

sRGB 下像素 128 会被解成大约 **0.22**，不是 0.5。于是：

`foldShadow ≈ 0.44` → 朝光处 `rampU` 最高只有 0.4 左右 → `1 - rampU` 卡在中灰。

杂色是同一张图的 G 里有很多中间灰（发缝、高光遮罩），再经过 mip + 双线性，一缕一缕抖出来。身体 G 几乎是平的 128，所以看不出来。

先改头发 LightMap 的 Inspector（和当时 Ramp / 身体 ILM 一样），Apply：

| 项 | 改成 |
|---|---|
| sRGB | **关** |
| Generate Mip Maps | **关** |
| Filter | **Point** |
| Compression | **None** |

然后 Debug 分开看：

- `ilmG`：关 sRGB 后大面积应是中灰（128），不该花成一堆脏点
- `rampU`：朝光应接近白
- `1 - rampU`：朝光应接近黑，只留轮廓/背光白

头发法线比衣服碎，`smoothstep(NdotL)` 本身也会有一点灰，但不应再是现在这种一片脏灰。先把导入改对，不要先改 `* 2` 的公式。

另外：头发 G 不一定还是「补阴影」。这张图 G 里 0 和 128、255 都很多。导入修好后若 `ilmG` 仍不像身体那样的褶沟遮罩，就不要再 `rampU *= foldShadow`，改用 R 或先关掉这一乘，用 Debug 对通道。

## 54. 你（Saturday, Sep 5, 2026, 8:34 PM (UTC+8)）

我在_DebugView里加入了tangent的调试，现在感觉显示不对，你帮我一下我的代码写得是否有问题；

### 助手

问题在 vert，不在 Debug 的 `* 0.5 + 0.5`。

你写的是：

```hlsl
VertexNormalInputs normal = GetVertexNormalInputs(IN.normalOS.xyz);
o.tangentWS.xyz = normal.tangentWS.xyz;
```

只传了法线。URP 这个重载会把切线 **写死成世界空间 (1, 0, 0)**：

```22:28:Library/PackageCache/com.unity.render-pipelines.universal@97f77d4dda9a/ShaderLibrary/ShaderVariablesFunctions.hlsl
VertexNormalInputs GetVertexNormalInputs(float3 normalOS)
{
    VertexNormalInputs tbn;
    tbn.tangentWS = real3(1.0, 0.0, 0.0);
    tbn.bitangentWS = real3(0.0, 1.0, 0.0);
    tbn.normalWS = TransformObjectToWorldNormal(normalOS);
    return tbn;
}
```

`tangentOS` 根本没进计算。Debug 里整头会接近同一色（世界 +X → 编码后大约粉红 `(1, 0.5, 0.5)`），转角色时切线也不跟着发丝走。

要用带切线的重载：

```hlsl
VertexNormalInputs normal = GetVertexNormalInputs(IN.normalOS.xyz, IN.tangentOS);
o.normalWS = normal.normalWS;
o.tangentWS.xyz = normal.tangentWS;
o.tangentWS.w = IN.tangentOS.w; // 以后算 bitangent 用
```

frag 里先归一化再给 Debug / 高光：

```hlsl
float3 T = normalize(IN.tangentWS.xyz);
```

Debug 继续 `CharaEncodeDireToRGB(T)`。对了以后：同一缕头发颜色应顺着发丝连续变化，转模型时颜色跟着头转，不是整头一块粉红。

FBX 导入勾上 **Tangents**（Calculate 或 Import）。没有切线数据时，即使换了重载也会是 0 或乱的。

## 55. 你（Saturday, Sep 5, 2026, 9:37 PM (UTC+8)）

OK, tangent正常显示了，可以继续做tangent高光了

### 助手

函数已经有了。片元里有几处要改，否则条状高光出不来或会编不过。

**1. `TdotH` 不要 `saturate`**

`T·H` 可以为负。`saturate` 之后 `sinTH` 是错的，高光条会糊掉或偏一边。

```hlsl
float TdotH = dot(T, H); // 不要 saturate
float sinTH = sqrt(saturate(1.0 - TdotH * TdotH));
```

**2. `_HairShininess` 现在是 0～1，不能当 pow 指数**

`pow(sinTH, 0.5)` 是开方，条会更宽。指数要用 **8～128** 这种数。Properties 改成：

```hlsl
_SpecExp1("Spec Exp 1 (narrow)", Range(8, 128)) = 64
_SpecExp2("Spec Exp 2 (wide)", Range(8, 128)) = 16
_SpecShift1("Spec Shift 1", Range(-0.5, 0.5)) = 0.1
_SpecShift2("Spec Shift 2", Range(-0.5, 0.5)) = -0.1
_SpecColor2("Spec Color 2", Color) = (0.8, 0.6, 0.5, 1)
```

CBUFFER 里补 `_SpecExp1`、`_SpecExp2`、`_SpecColor2`。Shift 要能 **为负**，两层才会上下错开。

**3. 用 T1 / T2 算两层，删掉未偏移的那条 `spec`**

可在 `GenShin_Shared.hlsl` 里加（不要叫 `ShiftTangent`）：

```hlsl
float HairStrandSpecular(float3 T, float3 H, float exponent)
{
    float TdotH = dot(T, H);
    float sinTH = sqrt(saturate(1.0 - TdotH * TdotH));
    return pow(sinTH, exponent);
}
```

frag 里 `V`、`H` 之后：

```hlsl
float3 T1 = ShiftHairTangent(T, N, _SpecShift1);
float3 T2 = ShiftHairTangent(T, N, _SpecShift2);

float spec1 = HairStrandSpecular(T1, H, _SpecExp1);
float spec2 = HairStrandSpecular(T2, H, _SpecExp2);

half specMask = ilm.r; // 先 Debug 看 ilmR，不对再改通道
half3 specCol = (spec1 * _SpecColor.rgb + spec2 * _SpecColor2.rgb)
                * _SpecIntensity * specMask * rampU * mainLight.color;

half3 finalColor = albedo.rgb * ramp * mainLight.color + specCol + rimCol;
```

Debug 把现在的 `0.0` 换成 `specCol`，选 `specular` 应看到沿发丝的亮带，阴影里没有。

**4. 先不要用未偏移的 T 算 `spec`**

你现在的 `pow(sinTH, _HairShininess)` 和 `T1/T2` 是两套，只留 T1/T2。`NdotH` 头发用不到，可删。

调参：Exp1 大、Shift 小 → 细白主高光；Exp2 小、Shift 为负 → 宽一点的副高光。条是竖的而不是沿发丝，把 Shift 接近 0 再看切线 Debug 是否沿发丝。

## 56. 你（Saturday, Sep 5, 2026, 9:56 PM (UTC+8)）

_SpecShift1,_SpecShift2这两个已经没有任何计算了，我现在的shader高光好像有错误，调参数没变化

### 助手

高光没变化，是因为 **Shift 没用上，而且 `HairStandSpecular` 的参数传错了**。

你现在写的是：

```hlsl
float spec1 = HairStandSpecular(T, N, _SpecExp1);
float spec2 = HairStandSpecular(T, N, _SpecExp2);
```

函数签名是 `(T, H, exponent)`，第二个参数必须是 **半角 H**。你传进去的是 **法线 N**。

头发切线和法线几乎垂直，`T·N ≈ 0` → `sinTH ≈ 1` → `pow(1, 任何指数) = 1`。两层高光处处都是 1，所以：

- `_SpecExp1 / 2` 怎么拉都一样  
- `_SpecShift1 / 2` 根本没参与计算  

`GenShin_Shared.hlsl` 里也没有 `ShiftHairTangent` 了，需要加回去。

**Shared 里保留 / 补上：**

```hlsl
float3 ShiftHairTangent(float3 T, float3 N, float shift)
{
    return normalize(T + N * shift);
}

float HairStandSpecular(float3 T, float3 H, float exponent)
{
    float TdotH = dot(T, H); // 不要 saturate
    float sinTH = sqrt(saturate(1.0 - TdotH * TdotH));
    return pow(sinTH, exponent);
}
```

**frag 里 `H` 算完之后改成：**

```hlsl
float3 T1 = ShiftHairTangent(T, N, _SpecShift1);
float3 T2 = ShiftHairTangent(T, N, _SpecShift2);

float spec1 = HairStandSpecular(T1, H, _SpecExp1);
float spec2 = HairStandSpecular(T2, H, _SpecExp2);
```

后面 `specCol` 那行不用改。Debug 选 **`specular`** 再拉 Shift：亮带应沿发丝上下移动。选 `FinalColor` 时若 `_DebugView` 还停在 9（screenUV），看起来也会像「调了没反应」。

## 57. 你（Saturday, Sep 5, 2026, 10:07 PM (UTC+8)）

已经实现了，但是我发现我现在的这个原神模型的头发的UV并没有按照规则来排布，头发的uv都应该是统一朝向的吧

### 助手

游戏里的头发 **UV 不会整张图朝同一个方向**。原神也一样：一张 Diffuse 上是很多岛屿，旋转、镜像、挤在一起，只为省贴图。

各向异性高光跟的不是「UV 朝哪」，而是 **模型切线 T 在三维里是否顺着发丝**。UV 只是用来采样颜色 / LightMap。岛屿在 UV 里横着摆、倒着摆都可以，只要这张卡片上的切线沿着那缕头发。

所以：

| 看什么 | 正常情况 |
|---|---|
| UV 展开 | 一块块碎片，方向不统一 |
| tangentWS Debug | 同一缕头发颜色连续，并顺着 3D 发丝 |

你这套模型（尤其 Pole / 拆出来的资源）经常是：UV 乱摆，切线也没严格按发丝做。这时 `HairStandSpecular` 会对，但亮带会横着扫、或者一卡一条方向。

可以先确认，不必先改 UV：

1. 切线 Debug 和 3D 发丝是否同向。若亮带和发丝差 90°，把 `T` 换成副切线再试：

```hlsl
float3 B = normalize(cross(N, T) * IN.tangentWS.w);
// HairStandSpecular 用 B 代替 T
```

2. 不要用 `uv.y` 当 shift，除非你确认 V 沿发丝。这张图的 UV 不统一，用 UV 当流向会更花。

3. 官方做法往往是 **切线已经沿发丝**，再用 LightMap 某一通道做 per-pixel 微偏移，而不是靠 UV 朝向。

结论：UV 不统一 **不等于** 做错了；要统一的是发丝方向（切线或副切线）。亮带和发缕对不上，再换 `B` 或检查 FBX 的 Tangents，而不是去重排整张 UV。

## 58. 你（Saturday, Sep 5, 2026, 10:14 PM (UTC+8)）

这是用T算出来高光，感觉辫子是正确的，头顶和刘海感觉翻转了90度

### 助手

这是 **模型切线方向不统一**，不是 Kajiya 公式把刘海算反了。

MikkT 切线跟 **UV 的 U 方向**走。辫子卡片的 U 往往沿发丝，所以 T 顺着辫子，高光是垂直发丝的横带——你看到的「对」。头顶 / 刘海那几块岛屿在 UV 里转了 90°，U 变成横穿额头，T 也横着走，高光就变成竖条，看起来像翻了 90°。

同一套 `T` 没法两头都对，除非用遮罩或启发式在 **T 和 B 里选顺着发丝的那根**。

你已经算了 `B`。Debug 把 `tangentWS` 那项改成显示 `B`：刘海若变成「顺着刘海」，就说明刘海该用副切线。

学习阶段可用「更接近世界下方」当发丝方向（辫子、刘海大多往下长）：

```hlsl
float3 flowHint = float3(0.0, -1.0, 0.0);
float3 hairT = (abs(dot(T, flowHint)) > abs(dot(B, flowHint))) ? T : B;

float3 T1 = shiftHairTangent(hairT, N, _SpecShift1);
float3 T2 = shiftHairTangent(hairT, N, _SpecShift2);
float spec1 = HairStandSpecular(T1, H, _SpecExp1);
float spec2 = HairStandSpecular(T2, H, _SpecExp2);
```

辫子上 T 已接近向下会继续用 T；刘海上 T 是横的、B 更向下就会改用 B。侧发、卷度大的卡可能仍会错，那是启发式的极限。

成品做法是画一张流向（或 LightMap 里一通道）去 lerp(T, B)，不要靠世界 Y。这套拆出来的模型 UV/切线本来就不整齐，shader 只能补，补不干净。

另外：你算了 `T1/T2`，高光仍传的是未偏移的 `T`。接上 `hairT` 之后，`HairStandSpecular` 请用 `T1/T2`，Shift 才会作用。

## 59. 你（Saturday, Sep 5, 2026, 10:20 PM (UTC+8)）

所以，如果用T的话，UV岛需要朝向基本一致的吧

### 助手

对，**如果全模型只用网格切线 T**，每块头发岛屿在展开时必须遵守同一套约定：**U 方向（切线方向）在三维里顺着发丝**。

更精确一点：

- 不要求所有岛在 2D 贴图上「箭头都朝屏幕上方」
- 要求每块岛的 **+U 对应到模型上的发根→发梢**（或你们统一的那根轴）
- 平移、紧凑打包可以；**在 UV 里转 90°** 会让这块的 T 在 3D 里也转 90°，刘海那种竖条就是这样来的
- 转 180° / 镜像通常还能用（高光对 T 和 −T 往往差不多），`tangent.w` 会处理镜像

所以：用 T 做头发高光 = 美术在拆 UV 时就把「U = 发丝」定死。这张模型岛方向不统一，单靠 T 就会有的对、有的差 90°。要嘛重展 UV，要嘛 shader 里在 T/B 之间选，或画流向图。

## 60. 你（Saturday, Sep 5, 2026, 10:22 PM (UTC+8)）

OK，我知道了，但是我发现原神的角色展示效果并没有用这个高光的算法，我发现他的高光就是画死在Lightmap的B通道，

### 助手

有可能。角色**展示/立绘**经常把高光画进贴图，不一定走你现在这套 Kajiya。进战斗、大世界里头发高光一般仍会随灯光和镜头在发丝上滑动，那就是各向异性，LightMap 多半只当 **遮罩或偏移**，不是整条亮带的形状。

怎么区分：

| | 画死在 B | 切线高光 |
|---|---|---|
| 转灯 / 转镜头 | 亮斑粘在头发上不动 | 亮带沿发丝移动 |
| 形状 | 和 B 通道图案一致 | 随 T 和 H 变 |

这张头发 LightMap 的 **B 几乎全是 0**，只有少量灰，很像「只在几缕上画了高光」。你截图里那种死白、锯齿、辫子横条、刘海竖条，也更像贴图像素，而不是光滑的 `sin(T·H)`。

若要对齐**展示效果**，高光可以先改成用 B（仍只列代码）：

```hlsl
half3 specCol = ilm.b * _SpecColor1.rgb * _SpecIntensity * rampU * mainLight.color;
```

`rampU` 还在，背光里可以关掉。Debug 选 `ilmB` 应和现在看到的白斑对上。

若以后要「画死的形状 + 少量随光滑动」，再：

```hlsl
specCol = ilm.b * (spec1 * _SpecColor1.rgb + ...) * ...
```

B 当强度，Kajiya 当会动的一层。展示向可以先只用 B，切线高光当进战斗再加。UV 岛朝向乱，也解释了为什么官方展示更愿意画死，而不是信这套模型的 T。

## 61. 你（Saturday, Sep 5, 2026, 10:25 PM (UTC+8)）

另外：你算了 T1/T2，高光仍传的是未偏移的 T。接上 hairT 之后，HairStandSpecular 请用 T1/T2，Shift 才会作用。你之前说的这个是改哪里的代码；我现在确实调整Shift没有效果

### 助手

改 **Hair shader 的 frag**，就是算完 `T1/T2` 紧接着的那两行，不是 Shared 里的函数。

你现在是：

```166:169:Assets/RenderingLab/Cha/Yuanshen/Shader/Genshin_Hair.shader
                float3 T1 = shiftHairTangent(T, N, _SpecShift1);
                float3 T2 = shiftHairTangent(T, N, _SpecShift2);
                float spec1 = HairStandSpecular(T, H, _SpecExp1);
                float spec2 = HairStandSpecular(T, H, _SpecExp2);
```

`T1`、`T2` 算了但没传进去，高光仍用原来的 `T`，所以 Shift 滑条无效。

改成：

```hlsl
float3 T1 = shiftHairTangent(T, N, _SpecShift1);
float3 T2 = shiftHairTangent(T, N, _SpecShift2);
float spec1 = HairStandSpecular(T1, H, _SpecExp1);
float spec2 = HairStandSpecular(T2, H, _SpecExp2);
```

只动 `HairStandSpecular` 的 **第一个参数**：`T` → `T1` / `T2`。后面 `specCol` 不用改。

若要用「T 或 B 里更顺着发丝的那根」，先得到 `hairT`，再：

```hlsl
float3 T1 = shiftHairTangent(hairT, N, _SpecShift1);
float3 T2 = shiftHairTangent(hairT, N, _SpecShift2);
```

Debug 选 **`specular`**，拉 `_SpecShift1` 到两端（-0.5 / 0.5），亮带应沿发丝上下挪。仍没变化就看 `_DebugView` 是不是还停在 screenUV，而不是 `specular` / `FinalColor`。

## 62. 你（Saturday, Sep 5, 2026, 10:27 PM (UTC+8)）

OK，基本上实现了，但是发现这个高光的算法还挺难的；你有什么办法用更简单的方法教会我这个算法的逻辑吗？目前我暂时会用画死的高光，就是lightmap里的B通道；

### 助手

可以先把 Kajiya 放下，展示用 B 通道完全没问题。下面用一个物理图像，不靠公式堆砌。

## 头发高光在干什么

金属高光问的是：**这块表面朝没朝向「反射光线刚好进眼睛」的方向？** 所以用 `N·H`，亮的是一个点。

头发不是一块平板，是很多**细管子**。管子侧面一圈都能反光，于是亮的是**一条带子**，垂直于发丝。

```
发丝方向 T  →→→→→→→
高光带      ████████   ← 横着箍在头发上，不是沿发丝拉长
```

辫子上那条横带，就是这个意思。刘海竖条，是那块的 T 转了 90°，带子跟着转了。

## 三个向量只记一件事

- **T**：发丝朝哪（沿头发）
- **L / V**：光和眼睛
- **H**：光和眼睛的「折中方向」，`normalize(L+V)`  
  「折中」对准发丝侧面时，带子最亮。

`T·H`：发丝和这个折中方向有多平行。

- 平行（点积接近 ±1）→ 你顺着管子看，侧面反光进不了眼睛 → **暗**
- 垂直（点积接近 0）→ 你在看管子侧面 → **亮**

所以用的是 **正弦** 不是余弦：

```
sin = sqrt(1 - (T·H)²)
```

垂直时 sin=1 最亮，平行时 sin=0 最暗。`pow(sin, 很大的数)` 只是把带子收细。

## Shift 在干什么

`T + N * shift` 再归一化：把发丝方向往法线拧一点点，等于把「最亮的那条箍」沿头发**往上或往下挪**。两层 Shift 一正一负，就是一细一宽两条带。你没把 T1/T2 传进高光时，拧了也没用。

## 和画死的 B 怎么对应

| | B 通道 | T 高光 |
|---|---|---|
| 形状 | 美术画在贴图上 | 数学算出来的箍 |
| 转灯 | 不动 | 带子在头发上滑 |
| 难度 | 采样相加 | 要 T 方向对、UV 岛约定 |

展示页灯光几乎固定，画死最省事，也避开这套模型切线不统一。

以后要捡回算法，只记三句：

1. 头发是管子，高光是**箍**，不是点  
2. 管子侧面最亮 ⇒ 用 `sin(T 和 H 的夹角)`  
3. Shift = 把箍沿发丝挪位置  

B 通道可以当成美术已经帮你画好的「箍画在哪」。公式是同一张画，改成会跟着灯走。

## 63. 你（Saturday, Sep 5, 2026, 10:33 PM (UTC+8)）

对了，我发现原神的现在版本里高光，是不是一个简单的bilnn高光乘以Lighmap.b的高光mask，因为我发现他头发高光只显示了一部分，暗部没有显示，感觉是根据光照的变化，bilnn高光也会动，然后乘以Lightmap.b

### 助手

你的观察和「**会动的 Blinn × 画出来的 B 遮罩 × 暗部关掉**」是对得上的。

三件事叠在一起，就会是你在游戏里看到的那样：

1. **Blinn（`N·H`）** — 灯或镜头一转，亮斑在头发上滑，不是贴死的。  
2. **`LightMap.B`** — 只有画过的那几缕能亮，所以「只显示一部分」。B 大面积是 0，本来就不是满头高光。  
3. **乘 `rampU`（或 N·L）** — 背光、阴影里高光被乘掉，暗部没有。

画死只有第 2 条，灯转了斑不动。纯 Kajiya 没有 B 的话，切线对的地方整圈都会亮，不会只有美术画过的形状。

展示 / 简化版完全可能就是：

```hlsl
float NdotH = saturate(dot(N, H));
float spec = pow(NdotH, _SpecExp1);   // 先 32～64
half3 specCol = spec * ilm.b * _SpecColor1.rgb * _SpecIntensity * rampU * mainLight.color;
```

`H` 仍是 `normalize(L + V)`。不用 T、不用 Shift。Debug 先只 `return spec * ilm.b`：应看到会动的亮斑，形状被 B 剪过，阴影里是黑的。

和 Kajiya 的差别（两种都可以再乘 B 和 `rampU`）：

| | Blinn × B | Kajiya × B |
|---|---|---|
| 斑在遮罩里的样子 | 偏圆、跟着「朝向眼睛的反射」 | 仍是垂直发丝的箍 |
| 要不要切线 / UV 朝向 | 不要 | 要 |

原神大世界完整头发 shader 仍有可能是各向异性再乘遮罩；你现在看的展示效果，用 Blinn × B 解释更干净，也更适合这套切线不统一的模型。

先用上面三行替换现在的 `T1/T2` 高光即可。B 当形状，Blinn 当「会不会跟灯走」，`rampU` 当暗部开关。

## 64. 你（Sunday, Sep 6, 2026, 12:36 AM (UTC+8)）

OK,效果可以了，现在好像头发的Ramp还没有正确显示吧

### 助手

先对照头发 Ramp 贴图尺寸、LightMap.A，以及 shader 里现在的采样，看是行号、U，还是贴图导入的问题。

### 助手

问题主要在 **V（行号）**，不是 Ramp 图坏了。头发这张图同样是 **256×20、10 行**，但有颜色的只有：

| | 行（从上往下） | 内容 |
|---|---|---|
| 白天 | 0、1 | 暖棕 / 略冷的发色 |
| 白天 | 2、3、4 | **纯白** |
| 夜间 | 5、6 | 紫 |
| 夜间 | 7、8、9 | **纯白** |

你还在用身体那套：

```hlsl
float row = round(ilm.a * 2.0); // 0 / 0.5 / 1 → 0 / 1 / 2
rampV = 1.0 - (night * 5.0 + row + 0.5) / 10.0;
```

头发 LightMap 的 A 基本是 **0 和 255**。A=1 时 `row = 2`，采到白天第 3 行 → **乘白 = 没有阴影色**，看起来就像 Ramp 没起作用。身体可以 `* 2` 是因为身体真有三档材质；头发只有两档有效行。

改成采 0、1 行（夜间仍 `+5`）：

```hlsl
float row = round(ilm.a);      // 0 或 1
row = clamp(row, 0.0, 1.0);
float night = step(0.5, _IsNight);
float rampV = 1.0 - (night * 5.0 + row + 0.5) / 10.0;
```

临时验证：`return half4(ramp, 1);`  
朝光应是 Ramp 右边浅色，背光是左边棕/紫，不该是纯白乘上去的平涂。

另外头发 Ramp 的导入仍建议：**Wrap Clamp、Filter Point、关 Mipmap**（和身体 Ramp 一样），否则 2 像素一行会和邻行（白）混在一起。

Debug：`ilmA` 应是两块黑/白；白的那块以前在采白色行，改完应走到第 1 行有颜色的那条。

## 65. 你（Sunday, Sep 6, 2026, 1:03 AM (UTC+8)）

头发的ramp 显示有问题，你帮看看，不像身体看上去很正常

### 助手

行号那步你已经改对了（`round(ilm.a)` → 0/1）。头发 Ramp 看起来脏、不像身体，主要是 **U 被 LightMap.G 乘坏了**，不是 V。

身体 G 大面积是 **128**，`* 2` 得到 1，朝光处 `rampU` 能到 1，Ramp 采最右边，明暗是干净两档。

你材质上的头发 LightMap（`Hair_Lightmap.tga`）G 不是 128：

| G | 含义 |
|---|---|
| **115～117** | 头发主体（约 0.45，`* 2 ≈ 0.9`） |
| **0** | 很大一块，`rampU` 被乘成 0，永远采 Ramp 最左边 |
| 中间灰 | 发缝细节，再双线性就变成杂色 |

所以 `rampU *= saturate(ilm.g * 2)` 在头发上会：朝光也到不了 Ramp 最右，还有整片被 G=0 钉在阴影色里。身体没有这个问题。

先只改这一行，**头发不要乘 G**：

```hlsl
float rampU = smoothstep(
    _ShadowThreshold - _ShadowSmooth,
    _ShadowThreshold + _ShadowSmooth,
    NdotL
);
// 头发先不要：rampU *= foldShadow;
```

Debug 看 `ramp`：应接近身体那种硬边两档（头发法线碎，会略软一点）。再把 **Shadow Threshold 降到 0.15～0.3**（材质里现在是 0.5，头发更容易停在 Ramp 中间灰）。

Ramp 采样锁 mip 0，避免 `NdotL` 当 U 时糊行：

```hlsl
half3 ramp = SAMPLE_TEXTURE2D_LOD(_RampMap, sampler_RampMap, rampUV, 0).rgb;
```

头发 Ramp 已 Clamp、关了 Mip，Filter 仍建议 **Point**。

对照 Debug：`ilmG` 若是一团 0.45 的脏灰而不是身体那种平 128 + 黑褶，就证明 G 不是头发的「补阴影」，不能再乘进 `rampU`。G 以后若要接，先用 Debug 对过再决定，不要照搬 Body。

## 66. 你（Sunday, Sep 6, 2026, 1:13 AM (UTC+8)）

Lightmap.g 可以确定还是补阴影，只是Lightmap.g的颜色值不对，我现在已经把Lightmap.g的颜色值修正了，现在显示正常了

### 助手

那就说通了：头发 G 仍然是补阴影，之前是数值没按「128 = 满分」来，`* 2` 之后到不了 1，Ramp 的 U 一直被压在中间。改成和身体一样的 0 / 128 之后，朝光就能采到 Ramp 右边，褶沟 G=0 仍会钉在左边。

Hair 这条（漫反射 + Ramp + G/A + 展示用 Blinn×B + Rim）可以收了。下一步是 **Face shader**：脸不能用这套 N·L，要用 SDF（你这套资源里的 Face Shadow 还要再确认是不是真正的 SDF）。准备好了还是只列结构和思路。

## 67. 你（Sunday, Sep 6, 2026, 12:47 PM (UTC+8)）

好的，开始做脸；我发现我这边的原神贴图关于脸的有好几种黑白图，我截图给你，你看看能不能大概推测一下他们的用途；

### 助手

先对着工程里的原图拆通道看，再对照原神脸阴影常见用法，避免只凭预览猜。

## 68. 你

<available_subagent_types>
Available subagent_types and a quick description of what they do:
- generalPurpose: General-purpose agent for researching complex questions, searching for code, and executing multi-step tasks. Use when searching for a keyword or file and not confident you'll find the match quickly.
- explore: Fast agent specialized for exploring codebases. Use this when you need to quickly find files by patterns (eg. "src/components/**/*.tsx"), search code for keywords (eg. "API endpoints"), or answer questions about the codebase (eg. "how do API endpoints work?"). When calling this agent, specify the desired thoroughness level: "quick" for basic searches, "medium" for moderate exploration, or "very thorough" for comprehensive analysis across multiple locations and naming conventions.
- cursor-guide: Read Cursor product documentation to answer questions about how Cursor Desktop, IDE, CLI, Cloud Agents, Bugbot, and other features work. Use when the user asks 'In Cursor, how do I...?' or similar questions about Cursor products.
- ci-investigator: Investigate a single failing PR CI check and return a short root-cause summary. Use when the user asks to summarize, explain, diagnose, or investigate a specific failed check from a pull request.
- bugbot: Use only when the user *explicitly* asks for a Bugbot-like review of local code changes. When launching this subagent, set the Task description to exactly "Bugbot". Launch exactly one Bugbot subagent with `run_in_background: false` unless the user explicitly asks to run in background. Use this fixed prompt form: "Full Repository Path: ...\nDiff: <one of: \"branch changes\", \"uncommitted changes\", \"natural language\">\nChange Description: ...\nCustom Instructions: ..."; default to `Diff: branch changes`; include `Change Description` only when `Diff` is `natural language`, formatting it as one block per changed file (a `<path> (added|modified|deleted|renamed)` header followed by bullets of what changed, mentioning line numbers or ranges inline where helpful), and only use `natural language` as a last resort after a regular diff-based review failed because the diff could not be computed; include `Custom Instructions` only when the user gave specific review instructions. This subagent is single-shot and does not support `resume`; always launch a fresh subagent instead.
- security-review: Use only when the user *explicitly* asks for a security review of local code changes. When launching this subagent, set the Task description to exactly "Security Review". Launch exactly one security-review subagent with `run_in_background: false` unless the user explicitly asks to run in background. Use this fixed prompt form: "Full Repository Path: ...\nDiff: <one of: \"branch changes\", \"uncommitted changes\">\nCustom Instructions: ..."; default to `Diff: branch changes`; include `Custom Instructions` only when the user gave specific review instructions. This subagent is single-shot and does not support `resume`; always launch a fresh subagent instead.
- best-of-n-runner: Run a task in an isolated git worktree. Each best-of-n-runner gets its own branch and working directory. Use for best-of-N parallel attempts or isolated experiments.
</available_subagent_types>

<available_subagent_models>
If the user explicitly asks for the model of a subagent/task, you may ONLY use model slugs from this list:
- inherit (default; required unless the user explicitly requested another model)
- composer-2.5-fast
- cursor-grok-4.6-high-fast

If the user isn't asking for a specific version, prefer the latest version of the model family. As an example, if the user just says "gpt" or "claude", use the latest available version of GPT or Claude.

IMPORTANT: If the user requests a model that is NOT in the list above, do NOT substitute a different model or guess. Instead, skip launching the subagent with that model and tell the user which model was unavailable and which models are available.

When speaking to the USER about which model you selected for a subagent, do NOT use the kebab-case model names unless the user requested the model using that format. Ue the same naming scheme the user used to discuss the model when they requested it.
</available_subagent_models>

<dynamic_tools>
You have access to tools through dynamic namespaces, e.g. MCP servers, using `GetDynamicTools` and `CallDynamicTool`.

## Dynamic Tool Discovery and Invocation

Use `GetDynamicTools` to discover tool schemas, then `CallDynamicTool` to invoke one tool. Aim to minimize round-trips: ideally one discovery call followed by one invocation.

If the user mentions a product or service represented by an available namespace, and the request likely depends on it, proactively inspect that namespace before answering. If you are unsure which namespace matches, search with a relevant pattern.

`GetDynamicTools` supports these modes:

1. `{"namespace":"<id>"}`: returns schemas and full descriptions for every tool in that namespace.
2. `{"namespace":"<id>","toolName":"<name>"}`: returns one tool schema with its full description.
3. `{"pattern":"<regex>"}`: searches namespace and tool names.
4. `{"namespace":"<id>","pattern":"<regex>"}`: searches tools within one namespace.
5. No arguments: returns the full catalog.

Pattern-search and catalog results shorten long descriptions, marked by a trailing "... [truncated]"; namespace and single-tool lookups always return the complete description.

Always inspect a tool's schema before invoking it with `CallDynamicTool`.

If the available dynamic tools do not fully support what the user asked you to do, complete the work you can with the current tool set. In your work summary, include what you were unable to do and why. Do not use browser automation to work around missing tools unless the user explicitly asks you to use the browser.

Available dynamic tool namespaces:

<dynamic_tool_namespaces>
<namespace name="plugin-gmail-gmail" source="mcp" />
<namespace name="plugin-google-drive-google-drive" source="mcp" />
<namespace name="cursor" tools="CreateGoal, GenerateImage, UpdateGoal" namespaceUseInstructions="Native Cursor tools for this session. These are highly recommended and useful tools that you should use when the right situation arises. Don't be afraid to look at one if it seems relevant, even if you don't end up using it. You MUST read the tool schemas before calling them." source="cursor" />
</dynamic_tool_namespaces>

## MCP Resource Access

You also have access to MCP resources via `FetchMcpResource`.
If an MCP-backed namespace requires authentication, call `mcp_auth` through `CallDynamicTool` for that namespace, then inspect it again and retry if appropriate. Do not authenticate namespaces preemptively or repeatedly.
</dynamic_tools>

## 69. 你（Sunday, Sep 6, 2026, 3:34 PM (UTC+8)）

那后面这个length(Lxz)的目的和作用是什么？

### 助手

`length(Lxz)` 是水平分量的 **长度**（有多长），不是方向。

`L` 已经是单位向量，但去掉 y 之后，`(x,z)` 一般不再是长度 1：

- 灯几乎水平：y≈0，`length(L.xz)` ≈ 1  
- 灯斜 45°：xz 变短，长度大约 0.7  
- 灯几乎正上/正下：x、z 都接近 0，长度 ≈ 0  

后面这句要用它当「安全的 normalize」：

```hlsl
Lxz = lxzLen > 1e-4 ? Lxz / lxzLen : float2(1.0, 0.0);
```

`Lxz / lxzLen` 把水平方向拉回长度 1，才能和脸朝前做点积。若长度已经接近 0，再除会得到无穷大或 NaN，所以小于 `1e-4` 时强行当成正前方 `(1,0)`。

Debug 里 `lxzLen`：灯越水平越接近白（1），越顶光越接近黑（0）。顶光时脸 SDF 本来也不稳定，这个保护就是为这种情况准备的。

## 70. 你（Sunday, Sep 6, 2026, 3:37 PM (UTC+8)）

接近顶光的时候，我发现脸变黑了，这个变黑的目的是什么？

### 助手

先看你现在脸 shader 里顶光时实际走的是哪条路径，确认变黑是 debug、数值保护，还是光照算法本身。

### 助手

你现在看到的黑，**不是**「顶光要把脸画暗」的设计，而是你这行 debug 把 `lxzLen` 当成灰度画出来了：

```93:93:Assets/RenderingLab/Cha/Yuanshen/Shader/Genshin_Face.shader
return half4(lxzLen.xxx,1);
```

`lxzLen` 表示灯在水平面里还剩多少分量。顶光时灯几乎只有 y，水平几乎为 0，所以整张脸变成黑。它只是在告诉你：**这时水平方向已经测不准了**，不是在给脸做阴影。

真正的脸算法里，顶光也**不会故意变黑**。下一行是保护，不是压暗：

```hlsl
Lxz = lxzLen > 1e-4 ? Lxz / lxzLen : float2(1.0, 0.0);
```

水平太短时，强行当成正前方 `(1,0)`，后面 `front` / SDF 还能算下去，避免除零。原神脸也不用 `NdotL`，就是因为顶光时法线会把脸颊、鼻子算成大片暗部，和美术要的「按朝向转的脸阴影」对不上。

把那行 `return` 注释掉之后，顶光脸应该接近「灯在正前」的效果（大部分受光），而不是黑。若仍觉得顶光不该当正前，那是后面才要做的策略（例如淡出 SDF、改成全亮），和现在这个 length 无关。

## 71. 你（Sunday, Sep 6, 2026, 3:48 PM (UTC+8)）

还有一个灯光旋转的问题，灯光左右水平的旋转的时候，其实是在Transform面的Y轴旋转的，这样SDF能正常的随着灯光的在Y轴上旋转，SDF阴影显示正常；我可以理解成，沿Y轴旋转和之前你说的只计算xz屏幕的方向是什么关系？

### 助手

可以这么理解：**灯光绕 Transform 的 Y 轴转，就是在改水平朝向；shader 里只取 `L.xz`，就是只看这个水平朝向。** 两者是同一件事的两边。

世界坐标里：

- **Y** = 上下（竖直轴）
- **XZ** = 地面这个水平面

绕 Y 转灯光，灯的方向在水平面上打转（东南西北），**不改变俯仰**。这时变的是 `L.x` 和 `L.z`，`L.y` 几乎不变。

脸 SDF 问的不是「法线对着灯吗」，而是「灯从脸的哪一侧水平打过来」：正前 / 侧 / 后。所以丢掉 `L.y`，只用 `L.xz` 做 2D 方向，正好对应「绕 Y 的水平旋转」。

关系可以写成：

| 你在 Scene 里做的 | 向量上发生的事 | 脸 shader 用的 |
|---|---|---|
| 灯光绕 **Y** 转 | `L.x`、`L.z` 变 | `L.xz` → `front`、左右翻转 UV |
| 灯光绕 **X/Z** 抬高、压低 | 主要变 `L.y` | 被丢掉；只影响 `length(L.xz)` 会不会太短 |

所以你看到：水平转 Y，SDF 跟着走，是预期行为。把灯往上抬成顶光，水平分量变短，SDF 的「东南西北」就越来越测不准——那是俯仰，不是绕 Y。

再补一句：`Rxz = (0,1)` 是世界 **+Z**（水平右转），不是 Transform 的 Y。注释里的「+y」容易和灯光的 Y 轴搞混。

## 72. 你（Sunday, Sep 6, 2026, 3:53 PM (UTC+8)）

那意思是我旋转y轴其实影响的是灯光xz平面的灯光向量？旋转x轴影响的是灯光yz平面的灯光向量？旋转z轴影响的是灯光xy平面的向量？

### 助手

对，**在绕世界轴转一个方向向量**时，就是这个关系：转哪根轴，那根轴上的分量不变，另外两个分量在对应平面里转。

- 绕 **世界 Y**：`L.y` 不变，`L.x` 和 `L.z` 在 **XZ** 平面里转（水平转圈）
- 绕 **世界 X**：`L.x` 不变，`L.y` 和 `L.z` 在 **YZ** 平面里转（前后俯仰）
- 绕 **世界 Z**：`L.z` 不变，`L.x` 和 `L.y` 在 **XY** 平面里转（左右侧倾）

脸 SDF 只用 `L.xz`，所以你绕世界 Y 转灯，阴影会跟着走；绕 X/Z 主要改俯仰/侧倾，水平朝向几乎不动（直到变成顶光，`L.xz` 短到测不准）。

有两个容易混的地方：

1. 这说的是 **世界轴**。Scene 里旋转工具若是 Local，物体已经歪了之后，再转它的「本地 Y」就不是世界 XZ 了。
2. Inspector 里的 X/Y/Z 是欧拉角，**三个都非零时**不是三组互不影响的平面旋转（有旋转顺序）。你之前那种「只拧 Y、X/Z 差不多为 0」才和上表完全对得上。

对现在这张脸，记住一句就够：SDF 要的是灯在水平面上的东南西北，对应的就是绕世界 Y、改 `L.xz`。

## 73. 你（Sunday, Sep 6, 2026, 3:58 PM (UTC+8)）

所以，当灯光沿着x轴旋转的时候，这个的时候灯光会慢慢的接近到脸部的顶部，尤其是灯光旋转x轴灯光90度的时候，如果不加这个保护，脸就形成了一半是有阴影，一半是没有阴影的，这样就和头发身体的光影明暗对不上了，所以，这个时候我们就采取脸是白的，完全没有阴影？

### 助手

你的观察对了一半：**顶光时不该出现「半边阴、半边阳」**；那是把灯误当成侧光。至于「脸全白、完全没阴影」，和当前这行保护的关系要拆开看。

绕世界 **X** 转到约 90°，灯接近正上，`L ≈ (0, 1, 0)`，于是 `L.xz ≈ (0, 0)`。

这时如果还拿这个 `(0,0)` 去算：

- `front = dot((0,0), 脸朝前) = 0` → 被当成 **正侧方**
- `threshold = 0.5` → SDF 画出大约一半阴影

灯其实在头顶，身体/头发是「从上往下照」，脸却出现一张侧光脸，所以对不上。这不是「没保护就会除零」那么简单，而是 **水平方向已经消失，还在用 SDF 假装有左右**。

`length` 保护有两层：

1. **数值**：`L.xz` 太短时不能除，否则方向乱跳。
2. **美术选择**：太短时改用 `(1, 0)`，当成正前方。正前时 `threshold = 0`，SDF 几乎全过，脸 **没有这张 SDF 阴影**（接近全亮）。

所以你说的「这时候脸当白的、不要 SDF 阴影」——**和现在这句 fallback 的效果是一致的**。目的不是让脸和身体一样亮，而是：**顶光没有东南西北，就不要画出假的左右脸影。**

身体/头发顶光也不会整片死白，它们还有自己的 ramp / `NdotL`。脸只是关掉「按朝向切的那张脸阴影」。比半边阴半边阳更接近「灯在上面、人是亮的」。

若以后要更顺，可以用 `lxzLen` 在「还有水平朝向」和「全亮」之间 lerp，避免到 90° 突然啪一下变全亮。现在这句硬切已经是同一思路。

## 74. 你（Sunday, Sep 6, 2026, 4:04 PM (UTC+8)）

OK，明白了；我现在实验了一下，我旋转灯光从80度旋转到91度，在超过90度的时候脸就会突然变成没有sdf的效果，因为在小于90度的时候，灯光在角色的正背面，这个时候脸是全受sdf的影响是吧；

### 助手

对，你看到的跳变主要不是那句 `1e-4` 保护，而是：**丢掉俯仰、再把剩下的 `xz` 拉回单位长度之后，灯只要还在「后半球」，就会被当成正后方。**

绕 X 转到快 90° 时，`L.xz` 已经很短，但还在。`Lxz / lxzLen` 会把这点残渣放大成长度为 1 的水平方向：

- **不到 90°**（你这套灯光还在角色背后）：残渣仍朝后 → `front ≈ -1` → `threshold = 1` → `sdf` 几乎都过不了 → **整张脸是阴影色**。不是 SDF 轮廓「更明显」，而是阈值顶满，鼻子、脸颊那条影线都被吞掉，变成一片暗。
- **刚过 90°**：灯翻过头顶，残渣改朝前 → `front ≈ 1` → `threshold = 0` → 整张脸受光，看起来像「没有 SDF」。
- **正好 90°** 附近：`xz` 短到保护启动，才会强行当成正前。这个窗口极窄，你从 80° 拧到 91° 的那一下，主要是 **背后 ↔ 面前** 换半球。

所以可以记：

| 灯相对角色 | `front` | 脸上实际效果 |
|---|---|---|
| 正前 | 1 | 几乎全亮，看不出 SDF 形状 |
| 正侧 | 0 | SDF 轮廓出来（半边阴） |
| 正后 | -1 | 几乎全暗，也看不出 SDF 形状 |

SDF 那张图真正在「画形」的，是侧光附近。正后全暗、正前全亮，都是阈值被推到头，不是 SDF 坏了。

这也解释了为什么 80° 还全黑、一过 90° 突然全亮：水平方向被归一化后只有「前或后」，没有「有一点点在后面所以阴影淡一点」。若以后要顶光过渡更顺，才需要用 `lxzLen` 把阴影往全亮 lerp，而不是只靠过 90° 翻符号。

## 75. 你（Sunday, Sep 6, 2026, 4:08 PM (UTC+8)）

Lxz = lxzLen > 1e-4 ? Lxz / lxzLen : float2(1.0,0.0); 这句话的具体意思是什么？ lxzLen = length(Lxz)的意思是把灯光的xz值贵1吗？

### 助手

`length` **不算归 1**，它只量长度。真正把 `L.xz` 拉回长度 1 的，是后面那句除法。

拆开看：

**第一句** `lxzLen = length(Lxz)`

`Lxz` 是 `(L.x, L.z)`。`length` 就是：

\[
\sqrt{L.x^2 + L.z^2}
\]

得到一个数：水平分量有多长。向量本身没被改掉。

- 灯几乎水平：大约 `1`
- 灯斜着：`0.x`
- 灯几乎顶光：接近 `0`

**第二句** 是三元运算，等价于：

```hlsl
if (lxzLen > 0.0001)
    Lxz = Lxz / lxzLen;      // 归 1：方向不变，长度变成 1
else
    Lxz = float2(1.0, 0.0);  // 太短，没法除，当成正前（角色 +X）
```

`Lxz / lxzLen` 才是归一化。例如灯在水平面上朝 `(0.3, 0.4)`，长度是 `0.5`，除完变成 `(0.6, 0.8)`，方向一样，长度变成 1。后面才能和脸朝前 `(1,0)` 做点积，得到干净的 `-1 ~ 1`。

`1e-4` 就是 `0.0001`。比它还短就认为水平方向没了，再除会爆，所以改用 `(1, 0)`。

对应关系：

| 写法 | 做什么 |
|---|---|
| `length(Lxz)` | 量长度，不改方向 |
| `Lxz / lxzLen` | 归 1（normalize） |
| `float2(1, 0)` | 顶光时的备用方向 |

## 76. 你（Sunday, Sep 6, 2026, 4:15 PM (UTC+8)）

那其实就是：Lxz=L.xz 得到的值其实还不是向量？通过Length这个函数以后，才能得到灯光的真正的向量？这个是三角函数吗？ 已xz平面的两条边，算出斜边的长度？

### 助手

`L.xz` **已经是向量**，只是少了 y 的二维向量 `(L.x, L.z)`。`length` 不会算出「真正的灯光向量」，它只算出这个二维向量有多长，结果是一个 **数**。

可以看成直角三角形：

- 一条直角边 = `L.x`（东西）
- 另一条直角边 = `L.z`（南北）
- 斜边 = `length(L.xz)` = \(\sqrt{x^2 + z^2}\)

这就是勾股定理。和三角函数是一家：斜边长度、再除过去，等价于把方向变成单位向量。`cos`/`sin` 也可以表示同一个方向，这里用向量除法更直接。

例子：灯水平朝向大约 `(0.6, 0, 0.8)`，丢掉 y 后：

| 步骤 | 得到什么 | 是向量吗 |
|---|---|---|
| `L.xz` → `(0.6, 0.8)` | 水平方向，长度已经是 1 | 是 |
| 若灯是斜的 `(0.3, 0.8, 0.4)`，`L.xz` → `(0.3, 0.4)` | 方向对，但长度只有 0.5 | 是 |
| `length` → `0.5` | 斜边有多长 | **不是**，是标量 |
| `(0.3, 0.4) / 0.5` → `(0.6, 0.8)` | 方向不变，长度归 1 | 是（单位向量） |

灯光的完整方向一直是三维的 `L`。`L.xz` 是「投影到地面」的那一段；`length` 量这段有多长；除完才是「地面上的单位方向」，给后面和脸朝前做点积用。

## 77. 你（Sunday, Sep 6, 2026, 4:17 PM (UTC+8)）

这个还不是那么完美的理解，你可以通过写一个C#脚本，把灯光xyz的向量长度画出来，这样是不是能更好的理解？

### 助手

可以。先看项目里现有的调试脚本和灯光用法，再写一个在 Scene 里画出灯光三维向量、XZ 投影和斜边长度的可视化。

### 助手

可以，已经加了一个 Scene / Game 对照脚本：`Assets/RenderingLab/Cha/Scripts/LightXzGizmo.cs`。

挂在角色头附近的空物体上（或角色根上）。`Target Light` 可空，会用场景里的太阳灯。在 **Scene** 窗口里转灯光的 X/Y，箭头会跟着动。

颜色对应关系：

| 颜色 | 是什么 | 对应 shader |
|---|---|---|
| 黄 | 完整 `L`（三维，长度 1） | `normalize(mainLight.direction)` |
| 橙 | 从黄箭头尖端落到地面 | 丢掉的 `Y` |
| 红 / 蓝 | 地面上的两条直角边 | `L.x`、`L.z` |
| 青 | 红蓝构成的斜边 | `L.xz`，`length` 就是这条的长度 |
| 绿 | 青线拉到长度 1 | `L.xz / lxzLen` |
| 品红 | 青线太短时的备用 | `float2(1, 0)` |
| 白 | 脸朝前 | `Fxz = (1, 0)` |

Game 视图左上角会打出同一组数字。转 Y 时青线在地面上打转；转 X 接近 90° 时黄箭头竖起来，青线变短，绿线仍是长度 1（只是把很短的青线放大）。

`length` 量的是青色斜边有多长，不是灯光向量本身。灯光向量一开始就是黄的那条。

## 78. 你（Sunday, Sep 6, 2026, 4:22 PM (UTC+8)）

在这个脚本上加一个功能开关，关掉没有归一化的开关，我想看一下没有归一化的灯光xyz他们的样子是什么样的

### 助手

在脚本上加开关：打开时画出未归一化的 X/Y/Z 分量，关掉后只留归一化后的方向。

### 助手

Inspector 里现在有两个开关：

- **Show Unnormalized Xyz**：未归一化的 X/Y/Z。从原点沿世界轴画出三条分量，**长度就是 `L.x` / `L.y` / `L.z` 的值**，不是 1。
- **Show Normalized Xz**：归一化后的水平方向（绿线，`L.xz / length`）。

想只看未归一化，把 **Show Normalized Xz** 关掉即可。

未归一化开着时：

| 颜色 | 分量 | 你会看到 |
|---|---|---|
| 红 | `L.x` | 东西方向有多长 |
| 偏绿 | `L.y` | 上下有多长 |
| 蓝 | `L.z` | 南北方向有多长 |
| 青 | `L.xz` | 红+蓝在地面上的斜边，长度一般 **不是** 1 |

转灯光 X 接近 90°：绿的 Y 变长，红/蓝变短，青线也变短。这就是「丢掉 Y 之前」三条轴的样子。黄线仍是完整的 `L`（合起来长度仍是 1）。

绿线（归一化）开着时，即使青线已经很短，绿线仍会被拉回长度 1——这就是 shader 里那次除法。

## 79. 你（Sunday, Sep 6, 2026, 4:28 PM (UTC+8)）

@Genshin_Face.shader (96-97)  这两句是确定脸朝前和朝右，这个是根据世界坐标的位置吗？

### 助手

对，这两句用的是 **世界坐标的轴向**，而且是写死的单位方向，不是从模型顶点、也不是从 Transform 读出来的。

`float2` 存的是世界 **XZ** 两个分量 `(x, z)`，不是 `(x, y)`：

| 变量 | 值 | 世界方向 | 含义 |
|---|---|---|---|
| `Fxz` | `(1, 0)` | 世界 **+X** | 假定脸朝前是 +X |
| `Rxz` | `(0, 1)` | 世界 **+Z** | 假定脸朝右是 +Z |

注释里的「朝右 +y」容易误导：这里的第二个数是 **z**，不是世界 Y（Y 是上下，已经被丢掉了）。

它们能用，是因为这套角色约定 **脸朝世界 +X**。`L.xz` 也是世界水平方向，两边同一套坐标系，点积才有「前/后、左/右」。

限制：角色在 Scene 里绕 Y 转了，这两句仍指向世界 +X / +Z，不会跟着转。要跟角色走，得用物体矩阵取出真正的前/右，再取 `.xz`，例如：

```hlsl
float3 forwardWS = normalize(TransformObjectToWorldDir(float3(1, 0, 0)));
float3 rightWS   = normalize(TransformObjectToWorldDir(float3(0, 0, 1)));
float2 Fxz = normalize(forwardWS.xz);
float2 Rxz = normalize(rightWS.xz);
```

现在这版是「人一直朝世界 +X」的实验室写法。

## 80. 你（Sunday, Sep 6, 2026, 4:34 PM (UTC+8)）

是的，现在这个sdf阴影是死的，如果我通过Bip001Head这个骨骼旋转头部SDF阴影也应该要动的，这个应该怎么修改我的shader代码；还是先帮我呈现代码和原理，我自己手动修改shader代码

### 助手

先确认脸网格和 `Bip001Head` 的层级关系：蒙皮网格的物体矩阵通常不会跟着头骨转，这决定 shader 里该读矩阵还是由脚本传入朝向。

### 助手

SDF 贴图本身不会跟着骨骼转。要跟着 `Bip001Head` 动的，是 **「灯相对脸」的前/右**，不是 UV。

现在 `Fxz`、`Rxz` 写死成世界 `+X` / `+Z`。头骨转了，蒙皮会让鼻子、脸颊在世界里动，但 shader 仍按「世界正前方」判灯，阴影就钉在世界上。

另外：**不要**指望 `TransformObjectToWorldDir`。脸一般是 `SkinnedMeshRenderer`，物体矩阵是角色根，不是 `Bip001Head`。只转头时这个矩阵不变。

---

**做法：** C# 每帧读头骨世界朝向，传给 shader；shader 用这两条水平方向替换写死的 `(1,0)`、`(0,1)`。

这套模型约定脸朝 **局部 +X**。Unity 里：

- `transform.right` = 局部 +X → 脸朝前  
- `transform.forward` = 局部 +Z → 脸朝右  

在 Scene 里点 `Bip001Head`：红轴应出鼻子，蓝轴应朝脸右侧。若对不上，只换这两行映射。

---

**1. Properties + CBUFFER 各加两个向量**（默认值和现在一样，没脚本时行为不变）：

```hlsl
_FaceForwardWS("Face Forward WS", Vector) = (1, 0, 0, 0)
_FaceRightWS("Face Right WS", Vector) = (0, 0, 1, 0)
```

```hlsl
float4 _FaceForwardWS;
float4 _FaceRightWS;
```

**2. 用头骨朝向替换 96–97 行：**

```hlsl
float2 Fxz = _FaceForwardWS.xz;
float fLen = length(Fxz);
Fxz = fLen > 1e-4 ? Fxz / fLen : float2(1.0, 0.0);

float2 Rxz = _FaceRightWS.xz;
float rLen = length(Rxz);
Rxz = rLen > 1e-4 ? Rxz / rLen : float2(0.0, 1.0);
```

和灯一样：只取 XZ（点头的俯仰丢掉），太短就 fallback。后面的 `front`、`isRight`、翻 UV **不用改**。

**3. 挂在角色上的脚本**（对着脸那张 Renderer 写 PropertyBlock，避免改到共享材质）：

```csharp
using UnityEngine;

[ExecuteAlways]
public class FaceHeadOrientation : MonoBehaviour
{
    public Transform headBone;                 // 拖 Bip001Head
    public Renderer faceRenderer;              // 拖脸的 SkinnedMeshRenderer

    static readonly int FwdId = Shader.PropertyToID("_FaceForwardWS");
    static readonly int RgtId = Shader.PropertyToID("_FaceRightWS");
    MaterialPropertyBlock _block;

    void LateUpdate()
    {
        if (headBone == null || faceRenderer == null)
            return;

        if (_block == null)
            _block = new MaterialPropertyBlock();

        faceRenderer.GetPropertyBlock(_block);
        // 局部 +X = 朝前，局部 +Z = 朝右（先在 Scene 里核对头骨红/蓝轴）
        _block.SetVector(FwdId, headBone.right);
        _block.SetVector(RgtId, headBone.forward);
        faceRenderer.SetPropertyBlock(_block);
    }
}
```

用 `LateUpdate`，让动画先转头，再把朝向写进材质。

---

转头时相当于灯在脸的局部水平面里转：头向左转，相对灯就更像从右侧来，SDF 会翻 UV、改 `threshold`。只转头、灯不动，阴影也会在脸上挪。

若转头时左右反了，把 `SetVector(RgtId, …)` 改成 `-headBone.forward`。若前后反了，用 `-headBone.right`。先对轴，再改符号。

## 81. 你（Sunday, Sep 6, 2026, 4:49 PM (UTC+8)）

这些我都已经加好了，但是运行Unity 旋转灯光的时候，脸部的SDF阴影不转动，应该是有问题，帮我看一下

### 助手

先对照你改过的脸 shader、朝向脚本和材质赋值，看灯光转的时候 `Fxz`/`Lxz` 有没有真正参与计算。

### 助手

问题在 fragment 里：**灯光方向 `L` 算了，但没有拿去和脸做点积。** 转灯当然不会动。

你现在写成了：

```103:106:Assets/RenderingLab/Cha/Yuanshen/Shader/Genshin_Face.shader
                float front = dot(Fxz, Rxz);; //1=正前，-1=正后
                float threshold = saturate(0.5 - front * 0.5);

                float isRight = step(0.0, dot(Fxz, Rxz));
```

`Fxz` 和 `Rxz` 是脸的前、右，两者几乎总垂直，`dot` 恒约 0。`threshold` 就锁在 0.5，`isRight` 也不随灯变。头骨朝向只提供坐标系，**阴影跟着转的是灯在这个坐标系里的水平方向 `Lxz`。**

把丢掉的 `L.xz` 加回去，点积改回和灯做：

```hlsl
float2 Lxz = L.xz;
float lxzLen = length(Lxz);
Lxz = lxzLen > 1e-4 ? Lxz / lxzLen : float2(1.0, 0.0);

float2 Fxz = _FaceForwardWS.xz;
float fLen = length(Fxz);
Fxz = fLen > 1e-4 ? Fxz / fLen : float2(1.0, 0.0);

float2 Rxz = _FaceRightWS.xz;
float rLen = length(Rxz);
Rxz = rLen > 1e-4 ? Rxz / rLen : float2(0.0, 1.0);

float front = dot(Lxz, Fxz);   // 灯 vs 脸朝前
float threshold = saturate(0.5 - front * 0.5);

float isRight = step(0.0, dot(Lxz, Rxz));  // 灯 vs 脸朝右
```

另外 Properties 里 `_FaceRightWS = (0,1,0,0)` 是世界 **+Y**，`.xz` 是 `(0,0)`，会走 fallback。默认应写成 `(0,0,1,0)`（世界 +Z）。脚本在跑的话会被 `headBone.forward` 覆盖，但默认值仍建议改掉。

改完后：转灯，SDF 应再动；转 `Bip001Head`，阴影相对脸也应挪。

## 82. 你（Sunday, Sep 6, 2026, 4:55 PM (UTC+8)）

我现在转动Bip001Head的时候，sdf阴影是突然之间跳变的，不是像旋转灯光一样，慢慢的渐变的

### 助手

先看你现在的点积、左右翻转和头骨朝向是怎么算的；头转时跳变通常是 XZ 投影后前/右不再垂直，左右 UV 在侧光时被 `step` 硬切。

### 助手

这是预期里的硬切，不是 Unity 没刷新。转灯看起来顺，是因为那时前/右是世界 `+X` / `+Z`，在水平面里永远垂直；转头骨则不是。

`isRight = step(dot(Lxz, Rxz))` 会在某一瞬间把 SDF 的 U **整张镜像**。这个跳变要藏起来，必须发生在「正前全亮」或「正后全暗」——也就是 `front` 接近 `±1` 的时候。那要求 **水平面上的前、右始终差 90°**。

转灯时满足这一点。转 `Bip001Head` 时你把骨头的三维 `right` / `forward` 各自丢掉 Y 再分别归一化：三维里它们垂直，**投影到 XZ 后常常不再垂直**。于是 `L·R = 0`（翻 UV）出现在侧光、`threshold ≈ 0.5` 时，半边阴影突然左右对调，就是你看到的跳。

做法：只取头的水平朝前，**在 XZ 里用垂直向量当朝右**，不要再用 `_FaceRightWS.xz`。

把 98–104 行换成：

```hlsl
float2 Fxz = _FaceForwardWS.xz;
float fLen = length(Fxz);
Fxz = fLen > 1e-4 ? Fxz / fLen : float2(1.0, 0.0);

// 水平面里前的垂线 = 朝右。F=(1,0) 时得到 (0,1)，和世界 +Z 一致
float2 Rxz = float2(-Fxz.y, Fxz.x);
```

`front`、`isRight`、翻 UV 后面不用改。左右反了就把 `Rxz` 改成 `float2(Fxz.y, -Fxz.x)`。

脚本里建议把头朝前也压到水平面（点头时 `.xz` 变短，不会突然走 fallback）：

```csharp
Vector3 fwd = headBone.right;
fwd.y = 0f;
if (fwd.sqrMagnitude < 1e-6f)
    fwd = Vector3.right;
fwd.Normalize();

_block.SetVector(FwdId, fwd);
```

`_FaceRightWS` 可以不再传。这样转头只改变水平偏航，SDF 会像转灯一样滑过去；翻 UV 仍在全亮/全暗时发生，肉眼就不再跳。

## 83. 你（Sunday, Sep 6, 2026, 5:01 PM (UTC+8)）

脚本里的你帮我修改吧，shader里的代码我已经改好了

### 助手

已经改好 `FaceHeadOrientation.cs`。

现在只把头骨局部 **+X**（`headBone.right`）压到水平面：丢掉 Y，长度太短时当成世界 `+X`，再归一化后写入 `_FaceForwardWS`。朝右不再传，shader 里用 `float2(-Fxz.y, Fxz.x)` 即可。

转 `Bip001Head` 时 SDF 应能平滑跟着偏航走。若左右反了，把 shader 里那句改成 `float2(Fxz.y, -Fxz.x)`。

## 84. 你（Sunday, Sep 6, 2026, 5:03 PM (UTC+8)）

好奇怪，我转动Bip001 Head的时候，sdf阴影完全不动？

### 助手

阴影完全不动，多半是头骨轴向或 PropertyBlock 没写进 shader。先查场景里脚本引用、头骨轴向，以及材质是不是没收到 `_FaceForwardWS`。

### 助手

根因是压平 Y 之后：`Bip001 Head` 的局部 +X 多半沿脖子朝上，水平分量接近 0，脚本就会永远 fallback 成世界 `+X`，转头 SDF 当然不动。正在改脚本：自动选水平分量最大的头骨轴，并画一条调试箭头方便核对。

### 助手

上次把 `headBone.right` 的 Y 清掉，正好踩在这套 **Bip001 Head** 的轴向上：局部 +X 多半是沿脖子朝上，水平分量几乎是 0，脚本就一直 fallback 成世界 `+X`。灯还在跟世界前方比，头怎么转 SDF 都不会动。

脚本已改成默认 **AutoHorizontal**：在头骨的 ±X/±Y/±Z 里选 **水平分量最大** 的那根（一般是出鼻子的那根），再压到 XZ 写给 shader。

Scene 里从头骨会画出一条 **青色箭头**，转头时它应在水平面里跟着转。箭头动、阴影不动，再看脸材质是不是这张 Renderer；箭头也不动，把 `Face Forward Axis` 改成 LocalY 或 LocalZ 试。左右反了勾 `Flip Forward`。

## 85. 你（Sunday, Sep 6, 2026, 5:07 PM (UTC+8)）

可以了，但是我现在脸部分层了两个模型，所以脚本上的Face Renderer需要支持拖拽多个Skinned Mesh Renderer

### 助手

Inspector 里现在是 **Face Renderers** 数组，可以把分层后的两张脸 `SkinnedMeshRenderer` 都拖进去。脚本会对每一张写入同一套 `_FaceForwardWS`。

之前拖在单个槽里的那张会自动迁进数组；如果没出现，把两张脸重新拖进列表即可。

## 86. 你（Sunday, Sep 6, 2026, 5:14 PM (UTC+8)）

OK，一切正常了；

### 助手

那就好。脸 SDF 现在会跟主光水平朝向走，也会跟 `Bip001 Head` 的偏航走，两张脸网格共用同一套朝前方向。接下来要做脸部 Lightmap、描边或别的，直接说就行。

## 87. 你（Sunday, Sep 6, 2026, 5:17 PM (UTC+8)）

好的，脸部的贴图我发现也包含了头饰，头饰上也有金属高光，是不是脸部的shander里也需要有和身体高光的算法，然后通过脸部的Lightmap里的通道来区分？

### 助手

先对照你现在的脸贴图、Face Lightmap 通道，以及身体高光是怎么用 ILM 的，再判断头饰金属该走哪条。

### 助手

对照 Barbara 的 Face Lightmap 和 MetalMap 通道，确认头饰金属是跟身体 ILM 同一套，还是另有一张金属图。

### 助手

对，头饰和皮肤在同一张脸 Diffuse 上，脸 shader **需要金属高光**，但不要整张脸套身体那套 Blinn。皮肤走 SDF，头饰走高光；用 Face Lightmap **分区**，不是把身体 ILM 原样搬过来。

两套图职责不同：

| 贴图 | 干什么 |
|---|---|
| `FaceLightmap.tga` | 脸/眼/头饰分区（R/G/B/A 和身体 ILM 不是同一套含义） |
| `Avatar_Tex_MetalMap.png` | 全角色共用的 **金属 Matcap**（用视线空间法线去采样），不是按脸 UV 画的遮罩 |
| 身体 LightMap R/B | 布料上画好的高光形状和锐度 |

身体高光是：`pow(NdotH, 由 B 控制) * R * rampU`。皮肤用这个会发油、还会和 SDF 抢。原神头饰金属更常见的是：

1. Face Lightmap 某通道标出「这是饰品」  
2. 饰品 **不要吃 SDF**（否则脸阴影会抹到金属上）  
3. 用视角法线采样 MetalMap，再乘金属遮罩  

建议顺序：先把 `FaceLightmap.tga` 接到 shader 上，Debug 分别看 R/G/B/A（你已有 `ilmR`～`ilmA`）。对照脸 UV：皮肤、眼睛、底边那块金属饰品分别落在哪。

大致会是这种分工（以你拆开的通道为准，不要先背身体那套）：

- **皮肤**：SDF 阴影  
- **眼睛**：常年受光（避免 SDF 把眼白涂暗）  
- **头饰**：`faceShadow = 1`（或不乘 SDF），再加金属  

金属项可以先写成（和身体一样要 `V`、`N`）：

```hlsl
float2 matcapUV = TransformWorldToViewDir(N, true).xy * 0.5 + 0.5;
half metal = SAMPLE_TEXTURE2D(_MetalMap, sampler_MetalMap, matcapUV).r;
half3 specCol = metal * metalMask * _SpecColor.rgb * _SpecIntensity * mainLight.color;
```

`metalMask` 来自 Face Lightmap 里真正盖住头饰的那一层。皮肤处这个 mask 应接近 0。

先把四个通道 debug 出来，看清头饰在哪条通道，再写高光。通道对上了告诉我，再给你对应的 lerp 写法。

## 88. 你（Sunday, Sep 6, 2026, 5:28 PM (UTC+8)）

不对，刚刚说错了，脸部确实没有金属；我说的那个头饰是在头发Hair的里的；

### 助手

那就对了：金属在头发网格上。先看你现在的 Hair shader 和 Hair Lightmap 通道，确认头饰该用哪一层、怎么接身体那套高光。

### 助手

对，金属在 **Hair** 这张网格上（头饰和头发同一套 UV），改 Hair shader，脸继续只做 SDF，不用上金属。

你这套 Hair Lightmap 已经把区域画开了，不必再接 Face Lightmap：

| 通道 | 头发上大致是什么 |
|---|---|
| **R** | 大片浅灰是头发；头饰区偏黑；其中一块纯白几何 = **金属遮罩** |
| **G** | 折叠阴影（你已经在用 `*2`） |
| **B** | 发丝高光形状；头饰上也有灰阶，给锐度/高光用 |
| **A** | ramp 行（头发 0/1）；右边白块是头饰材质行，不是金属开关 |

现在 Hair 高光是：

```hlsl
spec = pow(NdotH, _SpecExp1) * ilm.b * ...
```

`ilm.r` 采了但没用。发丝走 **B** 没问题；头饰金属的开关在 **R 的高亮**，和身体「R=有没有高光、B=多尖」是同一类 ILM，但头发还多了各向异性，不要整颗头套同一种。

建议拆成两路：

- **头发**（`ilm.r` 不是那块金属白）：保持现在的 Blinn（或以后的 Kajiya），乘 `ilm.b`
- **金属头饰**（`ilm.r` 接近 1）：不要用发线切线；用视角法线采样共用的 `Avatar_Tex_MetalMap`，再乘金属遮罩

```hlsl
half metalMask = step(0.9, ilm.r);          // 先用 debug 看阈值对不对
half hairSpec  = pow(NdotH, _SpecExp1) * ilm.b * (1.0 - metalMask);

float2 matcapUV = TransformWorldToViewDir(N, true).xy * 0.5 + 0.5;
half metal = SAMPLE_TEXTURE2D(_MetalMap, sampler_MetalMap, matcapUV).r * metalMask;

half3 specCol = (hairSpec + metal) * _SpecColor1.rgb * _SpecIntensity * rampU * mainLight.color;
```

先把 Debug 切到 `ilmR`：头饰金属应是白点，头发应是灰。阈值对了再接 MetalMap。脸那套 `headilm*` 可以先放着，和头发无关。

## 89. 你（Sunday, Sep 6, 2026, 5:35 PM (UTC+8)）

如果R通道是金属mask的话，那头发部分是灰色的，岂不是也会有金属效果？

### 助手

会，**如果你把 `ilm.r` 直接当金属强度去乘**，头发那片灰就会变成「一半金属」。所以不能写成 `metal = ilm.r`。

这张 R 里其实是 **三档灰度，不是 0/1 一张金属贴图**：

| R 的值 | 区域 | 含义 |
|---|---|---|
| 接近 **1**（纯白） | 头饰上那块几何 | 金属 |
| **中间灰** | 头发 | 发丝高光遮罩（有没有、有多强），不是金属 |
| 接近 **0**（黑） | 头饰布/塑料 | 不要发丝高光，也不要金属 |

`step(0.9, ilm.r)` 的意思是：只有够白才进金属，灰头发过不了这道门，金属为 0。

发丝高光继续用 **B**（你现在就是 `* ilm.b`）。若也把灰 R 乘进金属，两套会叠在头发上。

可以记：

```hlsl
half metalMask = step(0.9, ilm.r);           // 只有纯白
half hairSpec  = pow(NdotH, _SpecExp1) * ilm.b * (1.0 - metalMask);
half metal     = matcap * metalMask;         // 不要再乘 ilm.r 的灰
```

Debug 看 `ilmR`：调阈值（0.8～0.95）直到只有头饰白点亮、头发全黑，再接 MetalMap。阈值太低，灰头发就会开始「发金属」。

## 90. 你（Sunday, Sep 6, 2026, 5:37 PM (UTC+8)）

OK明白了，反正ilmR里的值不符合Ramp对应的灰度值是吧

### 助手

对，**R 不参与选 ramp 行**，它的灰、白也不是 ramp 那几档材质灰。

你这边分工是：

- **Ramp 的 V（哪一行）** ← LightMap **A** 的离散灰度（身体 `round(a*2)` 三档，头发 `round(a)` 两档）
- **Ramp 的 U（亮暗）** ← 光照 + **G** 折叠阴影
- **R** ← 高光/金属遮罩（头发灰 = 发丝高光区，接近 1 的白 = 金属）
- **B** ← 高光锐度或发丝高光形状

所以 ilmR 是灰还是白，只说明「这里高光怎么画」，不会去对 ramp 上某一行的颜色。选错行是 A 的事，不是 R。

## 91. 你（Sunday, Sep 6, 2026, 8:38 PM (UTC+8)）

好的，把头发的shader的最终的代码展示出来

### 助手

下面是把金属头饰接进去之后的完整 Hair shader。发丝仍用 Blinn × `ilm.b`；`ilm.r ≥ 0.9` 才走 MetalMap。材质上把 `Avatar_Tex_MetalMap` 拖到 **Metal Map**，阈值不对就调 **Metal Threshold**。

```hlsl
Shader "RenderingLab/Yuanshen/Hair"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _LightMap("Light Map", 2D) = "white" {}
        _RampMap("Ramp Map", 2D) = "white" {}
        _MetalMap("Metal Map", 2D) = "black" {}
        [Toggle] _IsNight("Night Ramp", float) = 0

        _ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSmooth("Shadow Smooth", Range(0.001, 0.5)) = 0.05

        [Header(Specular)]
        _SpecExp1("Spec Exp 1", Range(8, 128)) = 64
        _SpecColor1("Specular Color1", Color) = (1, 1, 1, 1)
        _SpecIntensity("Specular Intensity", Range(0, 5)) = 1
        _MetalThreshold("Metal Threshold", Range(0.5, 1)) = 0.9

        [Header(Rim)]
        _RimWidth("Rim Width", Range(0, 20)) = 4
        _RimScale("Rim Scale", Range(1, 5)) = 1
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimIntensity("Rim Intensity", Range(0, 10)) = 1

        [Header(Debug)]
        [Enum(RenderLab.CharaDebugView)] _DebugView("Debug View", float) = 3
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _SpecColor1, _RimColor;
            half _SpecIntensity;
            float _IsNight;
            half _ShadowThreshold;
            half _ShadowSmooth;
            float _RimWidth, _RimScale, _RimIntensity;
            float _SpecExp1;
            float _MetalThreshold;
            float _DebugView;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "GenShin_Shared.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_LightMap); SAMPLER(sampler_LightMap);
            TEXTURE2D(_RampMap); SAMPLER(sampler_RampMap);
            TEXTURE2D(_MetalMap); SAMPLER(sampler_MetalMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positonCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 tangentWS : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings o = (Varyings)0;
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normal = GetVertexNormalInputs(IN.normalOS.xyz, IN.tangentOS);
                o.positonCS = pos.positionCS;
                o.positionWS = pos.positionWS;
                o.normalWS = normal.normalWS;
                o.tangentWS.xyz = normal.tangentWS.xyz;
                o.tangentWS.w = IN.tangentOS.w;
                o.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return o;
            }

            half4 frag(Varyings IN) : SV_TARGET
            {
                float3 T = normalize(IN.tangentWS.xyz);
                float3 N = normalize(IN.normalWS);
                Light mainLight = GetMainLight();
                float3 L = normalize(mainLight.direction);

                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                half4 ilm = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, IN.uv);

                half foldShadow = saturate(ilm.g * 2.0);
                half NdotL = saturate(dot(N, L) * 0.5 + 0.5);
                float rampU = smoothstep(
                    _ShadowThreshold - _ShadowSmooth,
                    _ShadowThreshold + _ShadowSmooth,
                    NdotL
                );
                rampU *= foldShadow;

                // 头发 A 是 0/255 两档，不要 *2
                float row = round(ilm.a);
                row = clamp(row, 0.0, 1.0);
                float night = step(0.5, _IsNight);
                float rampV = 1.0 - (night * 5.0 + row + 0.5) / 10.0;
                half3 ramp = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, float2(rampU, rampV)).rgb;

                float3 V = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                float3 H = normalize(L + V);
                float NdotH = saturate(dot(N, H));

                // R 接近 1 = 金属；头发中间灰过不了阈值
                half metalMask = step(_MetalThreshold, ilm.r);
                float hairSpec = pow(NdotH, _SpecExp1) * ilm.b * (1.0 - metalMask);

                float2 matcapUV = TransformWorldToViewDir(N, true).xy * 0.5 + 0.5;
                half metal = SAMPLE_TEXTURE2D(_MetalMap, sampler_MetalMap, matcapUV).r * metalMask;

                half3 specCol = (hairSpec + metal) * _SpecColor1.rgb * _SpecIntensity * rampU * mainLight.color;

                float2 screenUV = GetNormalizedScreenSpaceUV(IN.positonCS);
                float rawDepth = SampleSceneDepth(screenUV);
                float linear01 = Linear01Depth(rawDepth, _ZBufferParams);
                float3 normalVS = TransformWorldToViewDir(N, true);
                float2 offsetDir = normalize(normalVS.xy);
                float2 offsetUV = offsetDir * _RimWidth / _ScreenParams.xy;
                float depthP = LinearEyeDepth(rawDepth, _ZBufferParams);
                float depthQ = LinearEyeDepth(SampleSceneDepth(screenUV + offsetUV), _ZBufferParams);
                float depthRim = saturate(depthQ - depthP) * _RimScale;
                float rimMask = saturate(depthRim) * (1.0 - rampU);
                half3 rimCol = _RimColor.rgb * _RimIntensity * rimMask * mainLight.color;

                half3 finalColor = albedo.rgb * ramp * mainLight.color + specCol + rimCol;

                half3 debugColor = CharaLightingDebugColor(
                    _DebugView, albedo.rgb, N, NdotL,
                    finalColor, ilm.r, ilm.g, ilm.b, ilm.a, specCol,
                    screenUV, rawDepth, linear01, depthRim, rimMask, rampU, T, ramp, 0.0,
                    0.0, 0.0, 0.0, 0.0);

                return half4(debugColor, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            struct DepthAttributes
            {
                float4 positionOS : POSITION;
            };

            struct DepthVaryings
            {
                float4 positonCS : SV_POSITION;
            };

            DepthVaryings DepthOnlyVertex(DepthAttributes IN)
            {
                DepthVaryings o = (Varyings)0;
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                o.positonCS = pos.positionCS;
                return o;
            }

            half4 DepthOnlyFragment(DepthVaryings IN) : SV_TARGET
            {
                return half4(IN.positonCS.z, 0, 0, 1);
            }
            ENDHLSL
        }
    }
}
```

有一处笔误，DepthOnly 顶点里请改成：

```hlsl
Varyings o = (Varyings)0;
```

应写成：

```hlsl
DepthVaryings o = (DepthVaryings)0;
```

和你原来 DepthOnly 那一段一样即可。粘进去后 Debug 用 `ilmR` 核对金属只有头饰是白，再用 `specular` 看金属高光。

## 92. 你（Monday, Sep 7, 2026, 12:08 AM (UTC+8)）

OK，好像都已经实现了，

### 助手

那就好。现在三套分工是清楚的：

- **身体**：ramp + ILM 折叠阴影 / 高光  
- **头发**：同样的 ramp，发丝 Blinn 用 B，头饰金属用 R 阈值 + MetalMap  
- **脸**：SDF 跟灯和头骨偏航走，不走金属  

接下来如果要做描边、脸部 Lightmap（眼睛常亮）或把头发各向异性捡回来，直接说就行。

## 93. 你（Monday, Sep 7, 2026, 12:26 AM (UTC+8)）

脸部还没有完全做完，脸部只实现了SDF的效果，还有Lightmap上的 一些通道还没有实现

## 94. 你（Monday, Sep 7, 2026, 12:26 AM (UTC+8)）

刚刚网卡了，现在继续~

### 助手

接着做脸的 Lightmap 通道。先对照你现在的 Face shader 和 FaceLightmap 各通道实际画了什么。

### 助手

继续。`_HeadLightMap` 已经采样进 `headilm` 了，Debug 也能看四条通道，只是 **还没拿去改光照**。按这张 TGA 的统计，通道是这样分的：

| 通道 | 贴图里长什么样 | 用途 |
|---|---|---|
| **R** | 约 72% 纯白、27% 纯黑 | 皮肤区域。白 = 吃 SDF，黑 = 不吃（UV 岛外头、非脸） |
| **G** | 几乎全白 | 基本不用 |
| **B** | 中间有柔和暗斑，其余较亮 | AO（鼻底、眼窝一类） |
| **A** | 约 83% 黑，只有一点点白 | 眼睛。白 = 常亮，不被脸阴影涂暗 |

和身体 ILM 不是一套：脸的 R 不是金属，A 不是 ramp 行。

在算出 `faceShadow` 之后、写 `finalColor` 之前加上：

```hlsl
// R：只有皮肤走 SDF；非脸当成不受脸阴影
faceShadow = lerp(1.0, faceShadow, headilm.r);

// A：眼睛抬到全亮
faceShadow = lerp(faceShadow, 1.0, headilm.a);

half3 shadowLit = lerp(_FaceShadowColor.rgb, 1.0, faceShadow);
half3 finalColor = albedo * shadowLit * headilm.b * mainLight.color;
```

顺序是先 R 再 A：眼睛若也在皮肤白区里，必须最后用 A 打掉阴影。

先 Debug `headilmR`：脸白、周围黑。`headilmA`：只有眼白。`headilmB`：鼻子附近暗一圈。对上了再改 `finalColor`。G 可以忽略。

## 95. 你（Monday, Sep 7, 2026, 12:43 AM (UTC+8)）

OK,基本上以及实现了；你可以把这几天做的关于原色的这个角色渲染知识和实现逻辑，以及关键功能的代码做出一个ppt吗？

### 助手

会做一份可打开的 PPT，先对照三套 shader 的最终实现，把这几天的原神角色渲染逻辑和关键代码整理进去。

### 助手

PPT 已生成，共 20 页：

`Docs/GenshinCharacterShading.pptx`

直接用 PowerPoint 或 WPS 打开即可。内容按这几天的实现来排：

1. **架构** — Body / Hair / Face 为什么要拆开  
2. **贴图约定** — Diffuse、ILM、Ramp、SDF、MetalMap 和导入注意  
3. **通道对照表** — 同一张 LightMap 在三套 shader 里含义不同  
4. **身体** — 半兰伯特、G 折叠阴影、A 选 Ramp 行、Blinn、深度边缘光  
5. **头发** — A 不要 `*2`、金属用 R 的 `step(0.9)` + Matcap  
6. **脸** — 不用 NdotL、`L.xz` 归一化、SDF 阈值与翻 UV、头骨朝向、Head Lightmap  
7. **Debug 和踩坑** — 后处理、Depth Priming、NPOT、点积写错、头骨轴向等  

需要改页或加截图，可以说一下要补哪一节。

## 96. 你（Monday, Sep 7, 2026, 12:59 AM (UTC+8)）

OK，后面还剩下哪些功能？

### 助手

对照三套 shader 和工程里还没接上的模块，列一份还剩什么。

### 助手

主光照三件套已经能看了。还没做、或只做了一半的，按优先级大概是这些：

**脸（还差一口气）**
- **AO**：`headilm.b` 已经采了，但 `finalColor` 还没乘，鼻底/眼窝不会暗下去。
- **脸没有 Ramp / 昼夜**，暗部现在是纯色 `_FaceShadowColor`，和身体夜晚切换还对不齐。
- **脸没有深度边缘光**，身体和头发有。

**描边（三套都还没接）**
- 工程里有 `OutlinePass.hlsl`，角色 Body / Hair / Face **都还没有** 外扩描边 Pass（法线外扩、描边色、脸要细一点）。这是原神角色观感差最大的一块。

**头发（可选）**
- 各向异性（Kajiya）写过又拿掉了：刘海 UV 不沿线，切线要自己带。展示用 Blinn 可以先留着。
- 金属现在乘了 `rampU`，头饰进阴影时高光会一起灭，若要饰品更「硬」可以金属不乘 `rampU`。

**身体（收尾）**
- 金属 Matcap 只在头发上；身体 ILM 若也有高 R 金属区，可以同样接 `MetalMap`。
- 多光源 / 环境光目前只有主方向光。

**工程向（不必马上做）**
- 昼夜 `_IsNight` 还是材质开关，没有跟场景时间走。
- 脸顶光过 90° 仍是硬切，可以用 `lxzLen` 把 SDF 淡出到全亮。
- 描边用的平滑法线需要模型/导入侧准备。

建议下一块做 **描边**，角色会立刻更像原神；脸只要补一行 `* headilm.b` 就能把 AO 收完。你想先描边还是先把脸收尾，说一声就行。

## 97. 你（Monday, Sep 7, 2026, 1:12 AM (UTC+8)）

OK，我现在发现的问题是，鞋子的鞋头黑色部分应该是matcap实现的；然后腰带上的魔法书上有蓝色的自发光；

### 助手

鞋头金属和腰带魔法书自发光都在身体上。先看 Body 的 Diffuse / LightMap 通道里这两块是怎么标的。

### 助手

对，这两块都在 **身体** 上，不是头发。现在 Body 只有 Blinn（`ilm.r × ilm.b`），所以：

- **鞋头黑块**：固有色本来就很暗，Blinn 只会在高光点闪一下白，没有 Matcap 那种随视角扫过的漆皮/金属感。
- **腰带魔法书**：蓝色图案画在 Diffuse 上，发光遮罩在单独的 **`Diffuse_A`**（大约只有 0.2% 的白，就在书上那个水元素圆标），shader 完全没用 Alpha，所以不会自发光。

和头发金属同一套阈值：**`ilm.r` 接近 1 才是金属**（鞋头、金饰），中间灰仍走原来的 Blinn。

---

**1. 金属 Matcap（鞋头）**

Properties 里加上 `_MetalMap`、`_MetalThreshold`（可和头发共用 `Avatar_Tex_MetalMap`）。高光那段改成：

```hlsl
half metalMask = step(_MetalThreshold, ilm.r);

float spec = pow(NdotH, lerp(8.0, 128.0, ilm.b));
spec *= ilm.r * rampU * (1.0 - metalMask);

float2 matcapUV = TransformWorldToViewDir(N, true).xy * 0.5 + 0.5;
half3 metal = SAMPLE_TEXTURE2D(_MetalMap, sampler_MetalMap, matcapUV).rgb;
half3 metalCol = albedo.rgb * metal * _SpecIntensity;

half3 specCol = spec * _SpecColor.rgb * _SpecIntensity * mainLight.color;
half3 finalColor = albedo.rgb * ramp * mainLight.color + specCol + rimCol;
finalColor = lerp(finalColor, metalCol * mainLight.color + rimCol, metalMask);
```

黑鞋头：albedo 暗 × matcap 高光 = 发亮的黑漆。金扣同理，只是 albedo 是金色。

---

**2. 魔法书自发光**

Diffuse PNG 基本没有可用 Alpha，要用拆出来的 `Avatar_Girl_Catalyst_Barbara_Tex_Body_Diffuse_A.png`：

```hlsl
_EmissionMap("Emission Mask (Diffuse A)", 2D) = "black" {}
_EmissionColor("Emission Color", Color) = (1, 1, 1, 1)
_EmissionIntensity("Emission Intensity", Range(0, 8)) = 2
```

```hlsl
half emitMask = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv).r;
half3 emission = albedo.rgb * emitMask * _EmissionColor.rgb * _EmissionIntensity;
finalColor += emission;   // 不乘 ramp / 灯光，阴影里也会亮
```

`_EmissionColor` 先留白，蓝从 albedo 来。不够蓝再把 Emission Color 拧成浅蓝。

`EmissionMap` 当遮罩：**sRGB 关掉**。

---

先 Debug `ilmR`：鞋头、金饰应接近白。再看 Emission Mask：只有书上那个蓝标是白。对上了再贴上面两段。CBUFFER 记得同步加 `_MetalThreshold`、`_EmissionColor`、`_EmissionIntensity`。

## 98. 你（Monday, Sep 7, 2026, 9:23 PM (UTC+8)）

好像有点不对，half metalMask = step(_MetalThreshold,ilm.r);这一行的意思是把鞋头黑色部分单独提取出来吗？

### 助手

不是。这行 **不看鞋是不是黑色**，也不看 Diffuse。它只看 LightMap 的 **R** 够不够大。

```hlsl
step(_MetalThreshold, ilm.r)
```

等价于：

```hlsl
ilm.r >= _MetalThreshold ? 1 : 0
```

`ilm.r` 是美术画在 LightMap 上的「这里是不是金属/硬高光」。阈值默认 0.9，只有 **接近纯白** 的像素才为 1。

所以 mask 里会包括：

- 鞋头黑块（固有色黑，但 R 是白 → 要 Matcap）
- 金扣、金属边（固有色金，R 也是白）

**不是**「把黑色鞋头单独抠出来」。黑只是 albedo 的颜色；Matcap 乘上去之后，暗底 + 随视角扫过的高光，看起来才像漆皮。

对照：

| 你看到的 | 实际依据 |
|---|---|
| 鞋头是黑的 | Diffuse RGB |
| 要不要走 Matcap | LightMap R ≥ 阈值 |

Debug 切到 `ilmR`：该走金属的地方应是白点，包括鞋头和金饰，不是「只有黑的地方」。头发那块灰 R 过不了 0.9，所以不会进金属。

## 99. 你（Monday, Sep 7, 2026, 9:40 PM (UTC+8)）

但是鞋头是黑色的，这一行算出来的

### 助手

你这张图说明鞋头的 LightMap R 并没有过阈值，所以 `metalMask` 是黑的。正在对照鞋头 UV 上 R/A 的实际灰度，看金属该用哪条通道。

### 助手

对，**你这张图已经把事情说清楚了**：`step(_MetalThreshold, ilm.r)` 算出来的白，是领口、裙扣、魔法书纹样那些 **R 接近 1 的装饰金属**；**鞋头在 LightMap R 里就是 0**，所以 mask 是黑的。

之前说「鞋头也会在 R 里被选中」说错了。R 不认「看起来黑不黑」，只认美术有没有把这块画成高亮。芭芭拉这块：

| 部位 | Diffuse | LightMap R | 这行 `metalMask` |
|---|---|---|---|
| 金扣 / 书上花纹 | 金、蓝纹 | ≈ 1 | 白 → 会走 Matcap |
| 鞋头、深色靴 | 很暗的蓝黑 | ≈ **0** | 黑 → **不会**走 Matcap |
| 裙子、袜子 | 白/布 | ≈ 0 | 黑 |

鞋头黑，是 **固有色黑**，再加上 **没被标成金属**。它和裙子一样，A 大约在 148（`round(a*2)` 落在布料那一行），不是 A=255 的金属行。

所以：

- 要 Matcap 的是 debug 里那些白点（扣子、书），不是鞋。
- 游戏里鞋头那点「漆皮质感」，更多是 **Ramp 亮部那一抹** + 很暗的 albedo，不是这行 mask。
- 若你仍想强行给鞋头 Matcap，得改 LightMap（把鞋头 R 画白，或 A 画成金属档），或另做一张遮罩。**单靠改 `step` 阈值没用**，R 已经是 0，再低也选不出来。

先把 Matcap 只用在现在这张白 mask 上，看扣子和书对不对。鞋头是另一套材质，不要和这行绑在一起。

## 100. 你（Monday, Sep 7, 2026, 9:55 PM (UTC+8)）

我发现一个解决方案，默认ilm.r都采用matcap，然后half metalMask = step(_MetalThreshold,ilm.r)； 这一行_MetalThreshold=0.9的时候，刚好只有非常明显的金属部分，把这个算出来的白的区域再加上NdotH的高光，这样你觉得如何？

### 助手

这个分层是对的：**Matcap 管「随视角扫过的质感」，`R ≥ 0.9` 再叠一层更尖的 NdotH。** 但不要理解成「全身默认都乘 Matcap」。

`ilm.r` 大部分是 **0**（布、皮肤、鞋头）。若对整身采样 Matcap，白裙子和皮肤会像涂了一层球状反光。Matcap 仍然要用 **R 当权重**，只是不要 `step` 成 0/1，让中间灰也能有一点：

| `ilm.r` | 怎么走 |
|---|---|
| 0 | 只有 Ramp，没有 Matcap |
| 中间灰 | `matcap × ilm.r`，弱一层扫光 |
| ≥ 0.9 纯白 | 满 Matcap，**再加** `pow(NdotH, …)` |

鞋头 R 仍是 0，这样还是吃不到 Matcap，和上次那张黑白图一致。鞋要另说；扣子、书上的金/纹可以按你这套做。

```hlsl
float2 matcapUV = TransformWorldToViewDir(N, true).xy * 0.5 + 0.5;
half3 matcap = SAMPLE_TEXTURE2D(_MetalMap, sampler_MetalMap, matcapUV).rgb;

// 连续：R 有多少，Matcap 就有多强（不是只抠纯白）
half3 matcapCol = albedo.rgb * matcap * ilm.r * _SpecIntensity;

// 只有非常硬的金属再加 Blinn
half hardMetal = step(0.9, ilm.r);
float spec = pow(NdotH, lerp(8.0, 128.0, ilm.b));
spec *= hardMetal * rampU;
half3 specCol = spec * _SpecColor.rgb * _SpecIntensity * mainLight.color;

half3 finalColor = albedo.rgb * ramp * mainLight.color
                 + matcapCol * mainLight.color
                 + specCol + rimCol;
```

`_MetalThreshold` 若仍要留，就只给 `hardMetal` 用，别拿去关 Matcap。Debug 可以分别看 `ilm.r`、`matcap * ilm.r`、`hardMetal`。若 Matcap 把整块金扣洗成一片亮，再改成 `lerp(ramp光照, albedo*matcap, ilm.r)` 用 Matcap **替换** 暗部，而不是叠加。

## 101. 你（Monday, Sep 7, 2026, 10:03 PM (UTC+8)）

鞋子黑色部分R是灰色的，可以吃到matcap的，我截图给你看，意思是，ilm.r 当做matcap的分层，然后ilm.r通过_MetaThreshold以后，当做NdotH的高光分层，第二张图是@Genshin_Body.shader (161-162) 计算后的结果；

### 助手

对，两张图就是这两层：

- **图一 `ilm.r`**：连续灰度。鞋头灰 → Matcap 乘一个中等权重；扣子/书纹接近白 → Matcap 最强；裙子黑 → 没有 Matcap。
- **图二 `step(0.9, ilm.r)`**：只留下「非常硬」的金属。鞋头被切掉，NdotH 只打在这些白点上。

`ilm.r` 当 Matcap 分层，阈值之后当 NdotH 分层，这样分是对的。把 159–170 换成下面，并删掉 `return half4(metalMask.xxx,1);`：

```hlsl
float2 matcapUV = TransformWorldToViewDir(N, true).xy * 0.5 + 0.5;
half3 matcap = SAMPLE_TEXTURE2D(_MetalMap, sampler_MetalMap, matcapUV).rgb;
half3 matcapCol = albedo.rgb * matcap * ilm.r * _SpecIntensity * mainLight.color;

half hardMetal = step(_MetalThreshold, ilm.r);   // 0.9：只有图二那些白
float spec = pow(NdotH, lerp(8.0, 128.0, ilm.b));
spec *= hardMetal * rampU;
half3 specCol = spec * _SpecColor.rgb * _SpecIntensity * mainLight.color;
```

`finalColor` 改成：

```hlsl
half3 finalColor = albedo.rgb * ramp * mainLight.color + matcapCol + specCol + rimCol;
```

`_MetalThreshold` 只控制第二层，不要再用它去关 Matcap。鞋头只会在图一里吃 Matcap，图二里本来就不该有 NdotH。

## 102. 你（Monday, Sep 7, 2026, 10:58 PM (UTC+8)）

OK, 这下效果对了，也好很多好，和官方原版效果比较接近；emission的功能我也已经做好了；接下来应该做什么了？

### 助手

主光照这条线已经齐了：身体 Ramp + Matcap/`ilm.r` + 硬金属 NdotH + 自发光，头发、脸 SDF 也能看。接下来按「观感提升」排：

**1. 描边（最值得先做）**  
三套 Shader 都还没有外扩 Pass。加上之后会立刻更接近原神：法线外扩、描边色、脸细一点、头发可以略粗。工程里已有 `OutlinePass.hlsl`，可以接到 Body / Hair / Face 上。

**2. 脸收尾（小改）**  
- `headilm.b` 还没乘进颜色，鼻底 AO 不会出来。  
- 脸没有深度边缘光，身体/头发有，侧光时头体会「断」。  
- 暗部还是纯 `_FaceShadowColor`，没有 Ramp/昼夜。

**3. 对齐和抛光**  
- 头发硬金属也可以叠一层 NdotH，和身体同一套逻辑。  
- `_IsNight` 现在是材质开关，三套可以一起切。  
- 顶光过 90° 脸 SDF 仍会硬切，可用 `lxzLen` 淡出。

建议下一刀做 **描边**。你要先讲原理再自己写 Pass，还是我按你现在的三个 shader 列一版可粘贴的 Outline，说一声就行。

## 103. 你（Monday, Sep 7, 2026, 11:04 PM (UTC+8)）

先做描边，先讲原理；另外，项目的outlinepass.hlsl我已经删除了，这个我会重新建立一个.hlsl专门给描边的

### 助手

先把描边原理讲清楚。你之后新建的 `.hlsl` 只要包这套顶点外扩；三套 shader 各加一个 Pass 去调用即可。

### 助手

原神角色描边不是后处理描边（Sobel），而是 **反向外壳（inverted hull）**：把模型沿法线略微撑大，只画「外壳的背面」，正面角色再盖住中间，屏幕上就只剩一圈轮廓。衣服上那些内线，很多已经画在 Diffuse 里，外壳只管剪影。

---

**Pass 怎么设**

| 项 | 值 | 原因 |
|---|---|---|
| Cull | **Front** | 扔掉外壳朝外的面，留下朝内的背面 |
| ZWrite | On | 和角色写进同一份深度 |
| ZTest | LEqual | 不要穿到角色前面 |
| LightMode | 自定，例如 `SRPDefaultUnlit` | 不必进光照；URP 会画这个 Pass |
| 顺序 | 描边 Pass 放在 ForwardLit **后面** 或前面都可以 | 常见：先画外壳再画角色，角色盖住内部 |

顶点：沿法线外扩。片元：输出描边色，几乎不做光照。

---

**外扩要在哪一层做**

物体空间 `pos += normal * width` 最简单，但远近粗细会变（透视）。原神要的是 **屏幕上差不多一样宽**，所以在 **裁剪空间 / NDC** 里沿「法线的屏幕 XY」推：

```hlsl
float4 posCS = TransformObjectToHClip(positionOS);

float3 normalWS = TransformObjectToWorldNormal(normalOS);
float3 normalVS = TransformWorldToViewDir(normalWS);
// z 压一下，避免朝向相机的法线在屏幕上缩成一个点，轮廓断裂
normalVS.z = -0.5;
float2 offset = normalize(normalVS.xy);

// * posCS.w：抵消透视，宽度按像素走
posCS.xy += offset * _OutlineWidth * posCS.w / _ScreenParams.y;
```

`_OutlineWidth` 单位大约是「占屏幕高度的比例」。脸 0.3、身体 0.8、头发 1.0 这种分材质调。

可加一点 `posCS.z` 偏置，减轻和角色表面的 z-fighting。

---

**法线必须「光滑」**

外壳跟的是 **顶点法线**。硬边（鞋、书、裙褶）法线不连续，描边会裂开、漏白。原神用的是 **平滑后的描边法线**（常烘进 UV3 / 顶点色 / 第二套法线），不是高模那套硬法线。

第一版可以先用网格自带 `NORMAL`。裂开了再烘平滑法线，顶点改读那一套。

---

**颜色**

纯黑会死、会脏。常见：

- 固定 `_OutlineColor`（深褐 / 深紫，跟衣服色相）
- 或 `albedo * _OutlineColor`，让描边跟着固有色走一点

脸比身体更浅、更细，避免「炭笔框脸」。

---

**建议你的 `Genshin_Outline.hlsl` 只暴露两件事**

```hlsl
float4 GenshinOutlinePositionCS(float3 positionOS, float3 normalOS, float width);
half3  GenshinOutlineColor(half3 albedo, half3 outlineColor, half tint);
```

三个 shader 各加一个 Pass：`#include` 这份 hlsl，vert 里调 `GenshinOutlinePositionCS`，frag 里输出颜色。Properties / CBUFFER 里加 `_OutlineWidth`、`_OutlineColor`。脸的 width 默认更小。

**先不要做：** 后处理描边、模板描边、按深度的全屏描边。外壳法和现在的 Forward + DepthOnly 最贴。

你建好空的 `.hlsl` 后，可以把文件路径发我，我按上面两个函数把实现列给你粘。三套 Pass 也可以等 hlsl 过了再贴。

## 104. 你（Monday, Sep 7, 2026, 11:12 PM (UTC+8)）

hlsl文件我已经建立好了，Genshin_Outline.hlsl; 还是老样子，你一步步来，把描边的知识点分几个步骤讲解同时附带代码，你不用给我写代码，我自己边看你的讲解边写代码

### 助手

按「一步一函数」来。先看整条路，这一轮只做第 1 步。

**路线**

1. hlsl：只把顶点变到裁剪空间，**先不外扩**  
2. 三个 shader 各加一个 `Cull Front` Pass，片元输出纯色（确认能画）  
3. 视线空间法线的 **XY** 外扩  
4. `* posCS.w / _ScreenParams.y` 做成屏幕近似等宽  
5. 描边色（可乘一点 albedo）  
6. 脸 / 身体 / 头发用不同 `_OutlineWidth`

---

**第 1 步：hlsl 只做坐标变换**

`Genshin_Outline.hlsl` 先不要撑模型。函数只做：物体空间位置 → 裁剪空间。三个 shader 的描边顶点都会调它，后面外扩只改这一个函数。

需要 `Core.hlsl` 里的 `TransformObjectToHClip`。这份 hlsl 会被 shader 在已 include Core 之后再 include，**这里不要再 include 一次**，避免重复定义。

```hlsl
#ifndef GENSHIN_OUTLINE_INCLUDED
#define GENSHIN_OUTLINE_INCLUDED

float4 GenshinOutlinePositionCS(float3 positionOS, float3 normalOS, float width)
{
    // 第 1 步：先原样变换。width / normalOS 后面才用，现在不用。
    return TransformObjectToHClip(positionOS);
}

#endif
```

`width` 和 `normalOS` 先空着，是为了第 3 步改函数内部时，Pass 的 vert **不用改签名**。

写好后告诉我，再做第 2 步：在 Body 上加描边 Pass（先只动身体，头发脸照抄）。第 2 步之前 **不要** 在 vert 里自己 `+= normal`。

## 105. 你（Tuesday, Sep 8, 2026, 9:03 PM (UTC+8)）

开始第二步

### 助手

第 2 步只动 **Body**。目标：描边 Pass 能编译、能被 URP 画出来。这一步 **还没有外扩**，所以屏幕上很可能 **还是看不见线**，这是正常的。

---

**为什么这步可能看不见**

`Cull Front` 画的是背面。模型和 ForwardLit 一样大，正面已经写下深度，背面更深，ZTest 失败，线被挡住。第 3 步沿法线撑大之后，多出来的一圈才会露出来。现在用 Frame Debugger 能看到名为 Outline 的 Pass 就算成功。

---

**1. Properties / CBUFFER**

Properties：

```hlsl
[Header(Outline)]
_OutlineWidth("Outline Width", Range(0, 5)) = 1
_OutlineColor("Outline Color", Color) = (0.2, 0.15, 0.22, 1)
```

`UnityPerMaterial` 里跟上（SRP Batcher 要求一致）：

```hlsl
float _OutlineWidth;
half4 _OutlineColor;
```

---

**2. 插在 ForwardLit 和 DepthOnly 之间的 Pass**

`HLSLINCLUDE` 已经有 `Core.hlsl`，每个 Pass 都能用。描边 Pass 里再 include 你的 hlsl。`LightMode` 用 `SRPDefaultUnlit`，默认 URP 会画，又不必走光照。

```hlsl
Pass
{
    Name "Outline"
    Tags { "LightMode" = "SRPDefaultUnlit" }
    Cull Front
    ZWrite On
    ZTest LEqual

    HLSLPROGRAM
    #pragma vertex OutlineVert
    #pragma fragment OutlineFrag
    #include "Genshin_Outline.hlsl"

    struct OutlineAttributes
    {
        float4 positionOS : POSITION;
        float3 normalOS : NORMAL;
    };

    struct OutlineVaryings
    {
        float4 positionCS : SV_POSITION;
    };

    OutlineVaryings OutlineVert(OutlineAttributes IN)
    {
        OutlineVaryings o = (OutlineVaryings)0;
        o.positionCS = GenshinOutlinePositionCS(IN.positionOS.xyz, IN.normalOS, _OutlineWidth);
        return o;
    }

    half4 OutlineFrag(OutlineVaryings IN) : SV_TARGET
    {
        return half4(_OutlineColor.rgb, 1);
    }
    ENDHLSL
}
```

---

**核对**

- Shader 无报错。  
- Frame Debugger 里角色有 **Outline**。  
- 场景里暂时没有黑圈。

不要在 vert 里自己 `+= normal`。写好后说一声，第 3 步只改 hlsl：用法线的视线 XY 去撑 `positionCS`。头发和脸先别加 Pass，等外扩在身体上看到线再复制。

## 106. 你（Tuesday, Sep 8, 2026, 9:12 PM (UTC+8)）

怎么在frame debugger 里看是否有Outline？

### 助手

你现在这个 Pass 的 `LightMode` 写成了 `"Outline"`。URP **不会画它不认识的 LightMode**，所以 Frame Debugger 里往往 **根本没有 Outline**，不是你不会看。

先改成默认就会画的：

```hlsl
Tags { "LightMode" = "SRPDefaultUnlit" }
```

另外你 `#pragma fragment OutlineFragment` 了，文件里好像还没写这个函数。没有片元程序 Pass 会编不过。补上：

```hlsl
half4 OutlineFragment(OutlineVaryings IN) : SV_TARGET
{
    return half4(_OutlineColor.rgb, 1);
}
```

顶点里若报 `positionCS` 找不到，你结构体写的是 `positonCS`（少了一个 i），两边名字要一致。

---

**Frame Debugger 怎么开**

1. 菜单 **Window → Analysis → Frame Debugger**  
2. 点左上角 **Enable**（游戏视图会停在这一帧）  
3. 左边是这一帧所有 Draw Call 的树  

在树顶的搜索框输入：

- `SRPDefaultUnlit`（改 LightMode 之后）  
- 或物体名，例如 `Barbara`  

点中某一条，右边会显示 Shader Pass Name。成功的话能看到 **Pass: Outline**，Event 类似 `Draw Mesh`。

改 LightMode 之前搜 `Outline` 是空的，这是 URP 跳过了这个 Pass，不是 Debugger 坏了。

点进那条 Draw 后，游戏视图只画到这一步：没外扩时可能是一整块背面色，或几乎看不见。能列出这一条就算第 2 步过了。用完点 **Disable**。

## 107. 你（Tuesday, Sep 8, 2026, 9:22 PM (UTC+8)）

OK,看到了；我有一个疑问，我在fragment debug里发现，先绘制的Outline的这个物体，然后再绘制的原问题；这个是怎么决定两个pass，都是不透明的问题的情况下出现的渲染先后次序？

### 助手

对，你看到的就是 URP 的规则：**两个都是不透明、Queue 也一样时，不是谁写在 shader 前面谁先画，而是 Renderer 里 LightMode 的登记顺序。**

不透明物体都进同一段 **Draw Opaque Objects**。对同一个 Renderer，URP 会按内部这份标签列表去对 Pass（顺序大致如此）：

1. `SRPDefaultUnlit` ← 你的 Outline  
2. `UniversalForward` ← ForwardLit  
3. 还有 `UniversalForwardOnly` 等  

所以 Frame Debugger 里一定是：**先 Outline，再本体**。你把 Outline Pass 写在 ForwardLit 后面也一样，URP **不按文件里 Pass 的先后**。

内置管线才是「一个 shader 里 Pass 从上到下」。URP 是「先按 LightMode 分桶，再按桶的顺序画」。

这个顺序对反向外壳刚好合适：先画撑大的背面外壳，再画正面角色把中间盖住，只剩一圈线。若 Outline 反而在 Forward 之后，还没外扩时更容易被深度挡住（你第 2 步看不见线，也有这个原因）。

同一段不透明里，**不同物体**之间还有：Render Queue → 大致从近到远（不透明提前 Z）→ 材质。你现在是 **同一个物体的两个 Pass**，只看 LightMode 列表。

第 3 步外扩之后，先 Outline 再 Forward 的顺序不用改。准备好了就说，只改 `Genshin_Outline.hlsl` 里那一个函数。

## 108. 你（Tuesday, Sep 8, 2026, 9:26 PM (UTC+8)）

SRPDefaultUnlit  这个标签决定了比UniversalForward 先绘制吗？

### 助手

是，**在默认 URP 的不透明绘制里**，就是这样：Pass 的 `LightMode` 对上列表里的哪一项，就按那一项的**下标**排先后。

`SRPDefaultUnlit` 被登记在 `UniversalForward` **前面**，所以 Outline 会先画。不是这个名字自带「优先」的含义，只是 URP 源码里先 `SetShaderPassName(0, SRPDefaultUnlit)`，再登记 Forward。

换一个 URP 不认识的标签（比如之前的 `"Outline"`），不会插入到 Forward 前面，而是**整段被跳过**。若自己写 Renderer Feature，把 Forward 登记在 Unlit 前面，顺序就会反过来。

你现在用 `SRPDefaultUnlit`：能被画到，而且会先于角色本体，正好符合外壳描边。

## 109. 你（Tuesday, Sep 8, 2026, 9:27 PM (UTC+8)）

好的，现在开始第三步吧

### 助手

第 3 步只改 `GenshinOutlinePositionCS`：**在裁剪空间里，沿「法线在屏幕上的方向」把顶点推出去。** Body 的 Pass 不用动。

外壳要露出来，必须比角色本体稍大。沿物体空间 `positionOS + normalOS` 会随透视远近乱变。先取法线在 **视线空间** 的 XY（差不多就是屏幕上往外指的方向），加到 `positionCS.xy` 上。

`normalVS.z = -0.5`：正对相机时法线几乎是 `(0,0,-1)`，XY 接近 0，`normalize(xy)` 会炸。先把 z 压成一个常数，再对 **三维** 归一化，XY 仍有稳定方向。

```hlsl
float4 GenshinOutlinePositionCS(float3 positionOS, float3 normalOS, float width)
{
    float4 posCS = TransformObjectToHClip(positionOS);

    float3 normalWS = TransformObjectToWorldNormal(normalOS);
    float3 normalVS = TransformWorldToViewDir(normalWS);
    normalVS.z = -0.5;
    float2 offset = normalize(normalVS).xy;

    posCS.xy += offset * width;
    return posCS;
}
```

材质上把 `_OutlineWidth` 拧到 **0.5～2** 应能看到一圈 `_OutlineColor`。裂开、硬边漏色先别管（平滑法线以后再说）。

现在宽度还是「裁剪空间里加一个数」，近大远小、和分辨率无关。第 4 步才用 `posCS.w` 和 `_ScreenParams.y` 做成屏幕上近似等宽。

能看到圈再说，不要提前乘 `w`。

## 110. 你（Tuesday, Sep 8, 2026, 9:32 PM (UTC+8)）

normalVS.z = -0.5;这一句为什么要加？ 原来normalVS.z 是通过TransformWorldToViewDir(normalWS)来的是吧，原来应该是多少？

### 助手

对。`normalVS` 整根向量都来自 `TransformWorldToViewDir(normalWS)`，**x、y、z 本来都有**。`z = -0.5` 是事后改掉原来的 z，不是变换函数算出来的。

视线空间（相机在原点，看向自己的前方）：

| 这个顶点的法线朝哪 | 原来的 `normalVS` 大概 | XY |
|---|---|---|
| 正对相机（鼻子、脑门） | `(很小, 很小, ±1)`，z 占满 | ≈ 0 |
| 侧轮廓（脸颊边缘） | z 接近 0，xy 较大 | 正好能当「往屏幕外推」的方向 |
| 背对相机 | z 符号相反，接近 ∓1 | 又很小 |

原来的 z **没有一个固定数**，每个顶点从大约 **-1 到 1** 都有。正对相机时最接近 `±1`。

外扩用的是 `normalize(...).xy`。若还拿原来的 z：

- 正对相机：`xy ≈ 0`，只对 `xy` 做 `normalize` 等于除以接近 0，方向变成噪声，描边会闪、裂。
- 即使对 **三维** 归一化再取 xy：`(0,0,1)` 归一化后 xy 仍是 0，这些点在屏幕上几乎不外扩。

写成 `normalVS.z = -0.5` 再 `normalize(normalVS)`：

- 强行给一个不太极端的 z，**tiny 的 xy 不会被当成单位向量放大**，方向更稳。
- `-0.5` 是常用经验值（有人写 `0.01` / `-0.1`），不是物理上的「正确 z」，只是避免 z 的绝对值太大、xy 被吃掉。

所以：**原来的 z 是变换算出来的朝向深度分量；这一句是故意丢掉它，换成常数，专门稳定屏幕 XY 外扩。** 可以暂时注释掉对比：正对相机转一圈，描边是否在脸前闪或断。第 4 步和这句无关，先搞懂再继续。

## 111. 你（Tuesday, Sep 8, 2026, 9:38 PM (UTC+8)）

描边正常显示了，开始下一步；

### 助手

第 4 步只改外扩那一行：**抵消透视，并按屏幕高度来标宽度。**

现在是 `posCS.xy += offset * width`。GPU 会做 `ndc = posCS.xy / posCS.w`。远处 `w` 更大，同样的加法在屏幕上更短 → **近粗远细**。分辨率一变，粗细也不一样。

要屏幕上差不多一样宽：

- 先 **乘 `posCS.w`**：NDC 偏移变成 `width`，与远近无关。  
- 再 **除 `_ScreenParams.y`**（屏幕像素高）：`width` 跟分辨率挂钩，1080 和 4K 不会差一截。

把第 13 行换成：

```hlsl
posCS.xy += offset * width * posCS.w / _ScreenParams.y;
```

`width` 的含义变了，数字会和刚才差很多。从 **1～8** 再拧 `_OutlineWidth`，看剪影是否远近一样粗。

拉远相机对比：第 3 步线会变细，这一步应基本稳住。稳了再说第 5 步描边色（可乘 albedo）。Pass 还是不用改。

## 112. 你（Tuesday, Sep 8, 2026, 9:47 PM (UTC+8)）

衣服袖子这里靠外的那一侧和靠内身体的这一侧描边不一致是什么原因？

### 助手

这是反向外壳描边的正常现象，**不是第 4 步远近等宽算错了。**

外壳描边稳的是 **剪影**：模型和背景（或空像素）交界的那一圈。你截图里袖子靠外那侧贴着灰背景，前面没有别的面把线盖住，所以又粗又完整。

靠身体那一侧是 **袖子和躯干叠在一起的缝**：

1. **绘制顺序**  
   先画整身 Outline，再画 `UniversalForward`。缝里那条线已经画上了，但躯干/袖子正面接着写颜色和深度，把缝里的线盖掉。外侧没有这层面，线就留着。

2. **法线方向**  
   袖内侧法线多半朝身体。屏幕 XY 外扩会把顶点往「身体里」推，露在屏幕上的像素更少；外侧法线朝外，推完正好多出一圈。

3. **深度**  
   缝里外壳和身体几乎贴在一起，ZTest 很容易让线输给身体表面。

所以：**外圈是剪影，内圈是两块网格之间的缝。** 外壳法几乎只保证前者。原神衣服内侧、袖笼那些线，很多是 **画在 Diffuse 上的内线**，不是这层 Pass 挤出来的。

若以后想让内侧也略明显一点，可以给描边一个很小的 **朝相机的 Z 偏置**（线稍微浮在表面前），缝里会多露一点，但也可能从身体上「浮出来」。那是后面再调的，和现在的 `w / ScreenParams` 无关。

第 5 步仍是描边色。内缝不齐先当预期，不用为了它改外扩公式。

## 113. 你（Tuesday, Sep 8, 2026, 9:51 PM (UTC+8)）

我说的是我标记的这两个位置

### 助手

之前说的是「贴着身体那条没圈到的缝」。你标的是 **同一条外侧剪影上的两处**：上面泡袖外沿，下面深蓝袖口外沿。这两处粗细不一样，原因也不同。

外扩用的是 `normalize(normalVS).xy`。这是三维归一化之后的 **xy，长度不是 1**，大约是 \(\sqrt{1-z^2}\)，法线越正对相机，屏幕上推得越短。

- **上面那圈**：泡袖鼓起来，剪影处法线更「立在屏幕上」，xy 大，挤出来就粗。  
- **下面那圈**：袖口是更小的圆管，还往深处伸，法线更侧向相机，xy 小，同样的 `width` 屏幕上更窄。  
  再叠一层：线是深色，画在深蓝布上，没有白袖和灰背景对比强，看起来更像「变细、快没了」。

第 4 步只保证 **远近** 差不多宽，**不保证** 同一条剪影上每个点一样粗。这是 `normalize(三维).xy` 的副作用，不是 Pass 顺序把袖口盖住了（你标的两处前面都是空背景）。

若希望整条外沿一样粗，第 3 步改成 **二维** 归一化（xy 接近 0 时要保护）：

```hlsl
float2 offset = normalVS.xy;
offset = (length(offset) > 1e-5) ? normalize(offset) : float2(1.0, 0.0);
```

正对相机的面可能又会闪，和现在「稳但粗细随朝向变」是一对取舍。袖口硬边处法线不连续，线还可能再细一截，那是平滑法线的问题，以后再说。

## 114. 你（Tuesday, Sep 8, 2026, 10:00 PM (UTC+8)）

感觉还是不对，我用一个胶囊体做了一个实验，你看截图，理论上应该这个胶囊体的一圈的描边都是要一样粗细的；

### 助手

你的预期对：**光滑胶囊的剪影，屏幕上应近似一样粗。** 现在两头圆粗、长边细，是外扩「方向」和「像素」还不是同一套空间，不是胶囊网格坏了。

你已经 `normalize(offset.xy)`，裁剪空间里位移长度是 1，但：

**1. 只除了 `_ScreenParams.y`**  
NDC 里 x、y 都是 -1～1，屏幕却是宽×高。同样一段 clip 偏移，横着和竖着对应的 **像素数不一样**（宽屏上横线更粗、竖线更细）。胶囊一斜，长边法线偏一种方向、圆头偏另一种，看起来就像「边细、头粗」。

**2. `normalVS.xy` 是视线空间，加在 `posCS.xy` 上是裁剪空间**  
投影矩阵对 x、y 的缩放不同（FOV × 宽高比）。视线里的单位向量，不能直接当裁剪空间里的单位向量用。

所以：**剪影法线在屏幕上该往哪推，和推 1 个像素，要对齐到 NDC/像素。**

把外扩改成：先用投影作用在 `nVS.xy` 上，再二维归一化，再 **按 `ScreenParams.xy` 分量** 加回去：

```hlsl
float4 posCS = TransformObjectToHClip(positionOS);

float3 normalWS = TransformObjectToWorldNormal(normalOS);
float3 normalVS = TransformWorldToViewDir(normalWS);

float2 clipDir = mul((float2x2)UNITY_MATRIX_P, normalVS.xy);
clipDir = (length(clipDir) > 1e-5) ? normalize(clipDir) : float2(1.0, 0.0);

posCS.xy += clipDir * width * posCS.w * 2.0 / _ScreenParams.xy;
return posCS;
```

`normalVS.z = -0.5` 在只用 `.xy` 时已经不起作用，可以先拿掉，避免干扰。

`width` 现在更接近「像素」，大约 **2～8** 再拧。横着放、竖着放、斜着放这颗胶囊，一圈应接近均宽。仍可能有一点点透视误差，那是外壳法线 ≈ 剪影法线，不是再除一个 `y` 能消掉的。

改完用同一颗胶囊对比截图。匀了再回头看袖子。

## 115. 你（Tuesday, Sep 8, 2026, 10:11 PM (UTC+8)）

float2 clipDir = mul((float2x2)UNITY_MATRIX_P, normalVS.xy);这一行代码的意思是什么？

### 助手

这行是在问：**视线空间里法线的 XY，经过投影之后，在裁剪空间 XY 上指向哪。**

`UNITY_MATRIX_P` 是投影矩阵，把「相机前方的点」变成裁剪坐标。完整是 4×4。`(float2x2)` 只取左上角 2×2，也就是 **只作用在 x、y 上** 的那一块（缩放 / FOV / 宽高比），不管平移，也不管 z、w。

透视投影里这块大致是：

- \(P_{00}\)：x 方向，里面含 **FOV 和宽高比**
- \(P_{11}\)：y 方向，主要是 **FOV**

`mul(那块 2×2, normalVS.xy)` 就是：

```text
clipDir.x = P00 * normalVS.x
clipDir.y = P11 * normalVS.y
```

`normalVS.xy` 还是「相机里往左/往上」的单位方向；乘完之后变成「裁剪空间里横竖各被拉了多少」。宽屏上 \(P_{00}\) 和 \(P_{11}\) 不相等，所以横竖不能当同样的长度用。后面再 `normalize(clipDir)`，得到的才是 **裁剪/NDC 里长度均匀的方向**，加到 `posCS.xy` 上才和像素对得上。

对比：

| 写法 | 空间 | 问题 |
|---|---|---|
| `normalize(normalVS.xy)` | 视线 | 横竖到屏幕的像素不一样 |
| `mul((float2x2)P, normalVS.xy)` 再归一化 | 投影后的 XY | 把 FOV、宽高比算进去 |

它不是把顶点做一次投影（那是 `TransformObjectToHClip`），只是把 **一根方向** 用投影的 XY 缩放变过去。方向没有「位置」，所以用 2×2 就够了。

## 116. 你（Tuesday, Sep 8, 2026, 10:14 PM (UTC+8)）

这样描边效果就正常了，胶囊体一圈都是一样的粗细，之前出现不一致的粗细，是忽略了屏幕的长宽比吗？

### 助手

对，**主要就是宽高比没进外扩**，但可以再拆成两件同一类的事：

1. **只除 `_ScreenParams.y`**  
   NDC 的 x、y 都是 -1～1，屏幕却是宽 × 高。横着推和竖着推，落到像素上的长度不一样。

2. **视线空间的 `normalVS.xy` 直接加到 `posCS.xy`**  
   投影对 x、y 的缩放本来就不同（里面已经含宽高比和 FOV）。视线里的「单位长度」≠ 裁剪空间里的「单位长度」。

`mul((float2x2)UNITY_MATRIX_P, …)` 把投影的 XY 缩放算进去，再 `* 2 / _ScreenParams.xy` 按像素宽、高分别去除，横竖才是同一套「屏幕长度」。胶囊一斜，剪影法线什么方向都有，以前就会头粗边细；现在一圈均宽，就是这两处对齐了。

和「近大远小」不是同一件事：那是漏乘 `posCS.w`。宽高比是 **同一距离下，横线和竖线不一样粗**。

## 117. 你（Tuesday, Sep 8, 2026, 10:19 PM (UTC+8)）

这个描边的感觉有点复杂，各种空间转来转去，有什么通俗易懂的方法理解吗？

### 助手

可以把它想成一件事：**在屏幕上，把剪影往外推几个像素。** 中间那些矩阵，只是「从模型上的法线」换算成「屏幕上往哪推、推多远」。

---

角色是 3D 的，描边要的是 2D 的：像用笔沿着人的外轮廓描一圈，而且 **整圈一样粗**。

法线是 3D 里「表面朝外」。剪影上，这个「朝外」投影到屏幕上，就是「轮廓该往哪胀」。外壳法就是：每个顶点按这个方向挪一点，再只画胀大那层的背面。

---

为什么不能在模型空间直接 `位置 + 法线`？  
那是在 **房间里** 把人吹大。离相机近的看起来胀得多，远的胀得少；屏幕是扁的，横竖像素还不相等。看起来就是近粗远细、横竖不一样。

所以要换到 **纸上（屏幕）** 再推。换算分三步，对应你写过的代码：

1. **法线先跟相机坐在一起**（`TransformWorldToViewDir`）  
   问：从相机看，这条法线偏左还是偏上？这是 `normalVS.xy`。

2. **乘投影的 2×2**（`mul((float2x2)UNITY_MATRIX_P, …)`）  
   相机镜头把「左右」和「上下」拉得不一样（宽屏、FOV）。不乘的话，你以为推了 1 厘米，纸上横竖已经不是 1 厘米。乘完再 `normalize`，得到的是 **纸面上的方向**，长度先不管。

3. **乘 `w`、除 `ScreenParams.xy`**  
   - `w`：透视除法会除以 w，乘回去抵消，远近在纸上推的像素一样多。  
   - 除宽、除高：NDC 的 -1～1 对应整屏像素，横竖分开除，1 个 `width` 才是「大约几个像素」。

---

一条线串起来：

**模型朝外 → 相机里朝哪 → 投影到画面上朝哪 → 按像素推开。**

不是四个「神秘空间」，是同一根箭头换了三次尺子，最后用的尺子是屏幕像素。胶囊一圈均宽，就是这把尺子横竖、远近都对准了。

## 118. 你（Tuesday, Sep 8, 2026, 10:26 PM (UTC+8)）

OK，那继续下一步吧

### 助手

第 5 步改 **颜色**：线不要纯黑/纯白，要带一点衣服固有色。外扩函数不用动，改 hlsl 加一个着色函数，再改 Body 的 Outline 片元。

纯黑会脏、像剪纸；你现在 `_OutlineColor` 是白的，线会像一圈高光。原神是 **深色 × albedo**：白袖子线偏暖灰，蓝袖口线偏深蓝。

---

**1. `Genshin_Outline.hlsl` 末尾、`#endif` 前加上：**

```hlsl
half3 GenshinOutlineColor(half3 albedo, half3 outlineColor, half tint)
{
    // tint=0：只用描边色；tint=1：完全跟着固有色乘描边色
    return lerp(outlineColor, albedo * outlineColor, tint);
}
```

---

**2. Outline Pass 要带 UV、采样 BaseMap**

贴图声明在 ForwardLit 里，Outline Pass **看不到**，要在这个 Pass 里再写一次（或以后挪进 `HLSLINCLUDE`）。

顶点输入补 `uv`，varyings 补 `uv`，vert 里 `TRANSFORM_TEX`。片元：

```hlsl
TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

// Attributes 增加：float2 uv : TEXCOORD0;
// Varyings 增加：float2 uv : TEXCOORD0;
// vert 里：o.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

half4 OutlineFragment(OutlineVaryings IN) : SV_TARGET
{
    half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb * _BaseColor.rgb;
    half3 col = GenshinOutlineColor(albedo, _OutlineColor.rgb, 0.5);
    return half4(col, 1);
}
```

`0.5` 可先写死。以后要做滑条再加 `_OutlineAlbedoTint`。

材质上把 **Outline Color 改成深色**，例如 `(0.25, 0.18, 0.22)`，不要留白。白袖外沿应比蓝袖口略浅，仍是线而不是第二层衣服。

调好看了再说第 6 步：Hair / Face 同样加 Pass，脸的 `_OutlineWidth` 更小。

## 119. 你（Tuesday, Sep 8, 2026, 11:28 PM (UTC+8)）

OK，身体上的描边可以了，继续下一步吧

### 助手

第 6 步：把 Body 那份 Outline Pass **原样搬到 Hair 和 Face**。外扩还是 `Genshin_Outline.hlsl`，只是材质上宽度不同。脸要细，避免「炭笔框脸」；头发可以和身体接近或略粗。

---

**Hair**（贴图名和 Body 一样是 `_BaseMap`）

Properties / CBUFFER 加上：

```hlsl
[Header(Outline)]
_OutlineWidth("Outline Width", Range(0, 5)) = 1.2
_OutlineColor("Outline Color", Color) = (0.2, 0.15, 0.12, 1)
```

```hlsl
float _OutlineWidth;
half4 _OutlineColor;
```

ForwardLit 和 DepthOnly 之间粘 Body 的整个 Outline Pass，`TRANSFORM_TEX(..., _BaseMap)`、`TEXTURE2D(_BaseMap)` 不用改。

---

**Face**（贴图叫 `_BaseTex`，这里必须换名）

Properties / CBUFFER 同样加 `_OutlineWidth`、`_OutlineColor`，默认宽度 **更小**，例如 `0.6`。颜色偏皮肤，如 `(0.35, 0.22, 0.22)`。

Pass 里三处和 Body 不同：

```hlsl
o.uv = TRANSFORM_TEX(IN.uv, _BaseTex);

TEXTURE2D(_BaseTex); SAMPLER(sampler_BaseTex);

half3 albedo = SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, IN.uv).rgb * _BaseColor.rgb;
```

脸分两层网格的话，两张都用这个 Face shader，都会描边。

---

**怎么调**

| 材质 | `_OutlineWidth` 起点 |
|---|---|
| Body | 你现在已经满意的值 |
| Hair | 接近 Body，或略大 |
| Face | 约为 Body 的一半 |

Inspector 里三个是 **各自的** 材质参数，不会共用一个滑条。

头发碎发、硬边可能裂线，那是法线不光滑，先当预期。三套都能看到线，描边这条线就齐了。

## 120. 你（Wednesday, Sep 9, 2026, 9:30 PM (UTC+8)）

OK，可以了,都可以看到描边了

### 助手

那就好。描边这条线齐了：屏幕近似等宽的外壳、带一点固有色，身体 / 头发 / 脸各调各的宽度。

后面如果要打磨，优先是这些（都不是必须立刻做的）：

- 硬边裂线：烘平滑描边法线  
- 脸 AO：`headilm.b` 乘进颜色  
- 脸的深度边缘光，和身体对齐  

先玩角色、调三个 `_OutlineWidth` 也行。下一步想做哪块直接说。

## 121. 你（Wednesday, Sep 9, 2026, 10:41 PM (UTC+8)）

先做硬边平滑吧

### 助手

硬边裂线是描边在用法线硬边，需要把「按位置平均过的平滑法线」烘进网格再给 Outline 用。先看现有描边顶点和模型导入方式。

### 助手

裂线是因为描边跟的是硬边法线。平滑法线要单独烘进网格（光照仍用硬法线）。我先加一个烘焙工具，再告诉你 shader 怎么读。

### 助手

硬边裂线是因为 **UV 缝把同一个空间点拆成多个顶点**，各自法线不同；外壳按各写法线往外推，线会撕开。光照仍要硬边（褶、金属），所以不能改网格自带 `NORMAL`，要另存一套 **按位置平均过的平滑法线**。

已加菜单：**RenderingLab → Bake Smooth Outline Normals**。选中角色根物体执行：按位置平均法线，写入 **UV2（TEXCOORD2）**，另存 `_SmoothOutline.asset` 再赋回 Renderer，不改 FBX。

---

**然后改描边（你自己贴）**

`GenshinOutlinePositionCS` 增加参数，用法线前先用平滑的：

```hlsl
float4 GenshinOutlinePositionCS(float3 positionOS, float3 normalOS, float3 smoothNormalOS, float width)
{
    float3 n = (dot(smoothNormalOS, smoothNormalOS) > 0.01) ? smoothNormalOS : normalOS;
    // 后面 TransformObjectToWorldNormal 改用 n，不要用 normalOS
    ...
}
```

三个 Outline Pass 的 Attributes 增加：

```hlsl
float3 smoothNormalOS : TEXCOORD2;
```

调用改成：

```hlsl
o.positionCS = GenshinOutlinePositionCS(
    IN.positionOS.xyz, IN.normalOS.xyz, IN.smoothNormalOS, _OutlineWidth);
```

没烘焙时 UV2 是 0，会自动退回硬法线。

---

先烘焙再改 shader。鞋、书、袖口硬边应变连续。注意：UV2 **不会随骨骼蒙皮**，T 姿/转头小动作一般够用；以后要跟动画走，再把平滑法线烘进 `TANGENT`（身体/脸可以，头发切线另说）。

烘完若裂线还在，看 Renderer 是否已经换成 `_SmoothOutline` 那张网格。

## 122. 你（Wednesday, Sep 9, 2026, 10:46 PM (UTC+8)）

你这个方案是 新增了一个网格，不是很实用，用起来也很麻烦，有没有更好的方案？

### 助手

复制网格确实别扭。改成 FBX 导入时直接把平滑法线写进原网格的 UV2，场景里不用换 Mesh。

## 123. 你

<available_subagent_types>
Available subagent_types and a quick description of what they do:
- generalPurpose: General-purpose agent for researching complex questions, searching for code, and executing multi-step tasks. Use when searching for a keyword or file and not confident you'll find the match quickly.
- explore: Fast agent specialized for exploring codebases. Use this when you need to quickly find files by patterns (eg. "src/components/**/*.tsx"), search code for keywords (eg. "API endpoints"), or answer questions about the codebase (eg. "how do API endpoints work?"). When calling this agent, specify the desired thoroughness level: "quick" for basic searches, "medium" for moderate exploration, or "very thorough" for comprehensive analysis across multiple locations and naming conventions.
- cursor-guide: Read Cursor product documentation to answer questions about how Cursor Desktop, IDE, CLI, Cloud Agents, Bugbot, and other features work. Use when the user asks 'In Cursor, how do I...?' or similar questions about Cursor products.
- ci-investigator: Investigate a single failing PR CI check and return a short root-cause summary. Use when the user asks to summarize, explain, diagnose, or investigate a specific failed check from a pull request.
- bugbot: Use only when the user *explicitly* asks for a Bugbot-like review of local code changes. When launching this subagent, set the Task description to exactly "Bugbot". Launch exactly one Bugbot subagent with `run_in_background: false` unless the user explicitly asks to run in background. Use this fixed prompt form: "Full Repository Path: ...\nDiff: <one of: \"branch changes\", \"uncommitted changes\", \"natural language\">\nChange Description: ...\nCustom Instructions: ..."; default to `Diff: branch changes`; include `Change Description` only when `Diff` is `natural language`, formatting it as one block per changed file (a `<path> (added|modified|deleted|renamed)` header followed by bullets of what changed, mentioning line numbers or ranges inline where helpful), and only use `natural language` as a last resort after a regular diff-based review failed because the diff could not be computed; include `Custom Instructions` only when the user gave specific review instructions. This subagent is single-shot and does not support `resume`; always launch a fresh subagent instead.
- security-review: Use only when the user *explicitly* asks for a security review of local code changes. When launching this subagent, set the Task description to exactly "Security Review". Launch exactly one security-review subagent with `run_in_background: false` unless the user explicitly asks to run in background. Use this fixed prompt form: "Full Repository Path: ...\nDiff: <one of: \"branch changes\", \"uncommitted changes\">\nCustom Instructions: ..."; default to `Diff: branch changes`; include `Custom Instructions` only when the user gave specific review instructions. This subagent is single-shot and does not support `resume`; always launch a fresh subagent instead.
- best-of-n-runner: Run a task in an isolated git worktree. Each best-of-n-runner gets its own branch and working directory. Use for best-of-N parallel attempts or isolated experiments.
</available_subagent_types>

<available_subagent_models>
If the user explicitly asks for the model of a subagent/task, you may ONLY use model slugs from this list:
- inherit (default; required unless the user explicitly requested another model)
- composer-2.5-fast
- cursor-grok-4.6-high-fast

If the user isn't asking for a specific version, prefer the latest version of the model family. As an example, if the user just says "gpt" or "claude", use the latest available version of GPT or Claude.

IMPORTANT: If the user requests a model that is NOT in the list above, do NOT substitute a different model or guess. Instead, skip launching the subagent with that model and tell the user which model was unavailable and which models are available.

When speaking to the USER about which model you selected for a subagent, do NOT use the kebab-case model names unless the user requested the model using that format. Ue the same naming scheme the user used to discuss the model when they requested it.
</available_subagent_models>

<dynamic_tools>
You have access to tools through dynamic namespaces, e.g. MCP servers, using `GetDynamicTools` and `CallDynamicTool`.

## Dynamic Tool Discovery and Invocation

Use `GetDynamicTools` to discover tool schemas, then `CallDynamicTool` to invoke one tool. Aim to minimize round-trips: ideally one discovery call followed by one invocation.

If the user mentions a product or service represented by an available namespace, and the request likely depends on it, proactively inspect that namespace before answering. If you are unsure which namespace matches, search with a relevant pattern.

`GetDynamicTools` supports these modes:

1. `{"namespace":"<id>"}`: returns schemas and full descriptions for every tool in that namespace.
2. `{"namespace":"<id>","toolName":"<name>"}`: returns one tool schema with its full description.
3. `{"pattern":"<regex>"}`: searches namespace and tool names.
4. `{"namespace":"<id>","pattern":"<regex>"}`: searches tools within one namespace.
5. No arguments: returns the full catalog.

Pattern-search and catalog results shorten long descriptions, marked by a trailing "... [truncated]"; namespace and single-tool lookups always return the complete description.

Always inspect a tool's schema before invoking it with `CallDynamicTool`.

If the available dynamic tools do not fully support what the user asked you to do, complete the work you can with the current tool set. In your work summary, include what you were unable to do and why. Do not use browser automation to work around missing tools unless the user explicitly asks you to use the browser.

Available dynamic tool namespaces:

<dynamic_tool_namespaces>
<namespace name="plugin-gmail-gmail" source="mcp" />
<namespace name="plugin-google-drive-google-drive" source="mcp" />
<namespace name="cursor" tools="AwaitShell, CreateGoal, Delete, EditNotebook, FetchMcpResource, GenerateImage, ReadLints, SwitchMode, Task, TodoWrite, UpdateGoal, WebFetch, WebSearch" namespaceUseInstructions="Native Cursor tools for this session. These are highly recommended and useful tools that you should use when the right situation arises. Don't be afraid to look at one if it seems relevant, even if you don't end up using it. You MUST read the tool schemas before calling them.

Here are some crucial instructions:
- AwaitShell: Use to sleep and check shell progress. Never sleep using shell.
- ReadLints: Check for linter errors after substantive edits.
- SwitchMode: Switch between modes: Agent and Plan. Proactively consider switching modes for relevant requests.
- TodoWrite: Use this tool to manage complex multi-step tasks." source="cursor" />
</dynamic_tool_namespaces>

If an MCP-backed namespace requires authentication, call `mcp_auth` through `CallDynamicTool` for that namespace, then inspect it again and retry if appropriate. Do not authenticate namespaces preemptively or repeatedly.
</dynamic_tools>

## 124. 你（Wednesday, Sep 9, 2026, 10:46 PM (UTC+8)）

你这个方案是 新增了一个网格，不是很实用，用起来也很麻烦，有没有更好的方案？

### 助手

更好的做法是 **导入时写进 FBX 自带的网格**，不再复制 `.asset`，也不用换 Renderer 上的 Mesh。

Unity 在 `OnPostprocessModel` 里改的就是导入结果：同一位置的法线平均后写入 **UV2**。场景里继续用原来的 SkinnedMesh，光照还是硬法线，描边读 `TEXCOORD2`。

**你怎么用**

1. 若已经赋过 `_SmoothOutline` 网格，把 Renderer 的 Mesh **改回 FBX 里原来那张**。  
2. 选中角色（或 `Barbara.fbx`）→ **RenderingLab → Reimport Models (Smooth Outline Normals)**。  
3. 只有 `Assets/RenderingLab/Cha` 下的模型会处理。以后重新导入 FBX 也会自动带上 UV2。

Shader 还是上一套：Outline 顶点加 `float3 smoothNormalOS : TEXCOORD2`，`GenshinOutlinePositionCS` 优先用它。没烘过时 UV2 为 0，会退回硬法线。

多出来的 `_SmoothOutline.asset` 可以删。UV2 不会跟骨骼蒙皮，T 姿/小幅度转头一般没问题。

## 125. 你（Wednesday, Sep 9, 2026, 11:10 PM (UTC+8)）

放到UV2以后，有的地方描边不顺滑了，除了这个方案还有更好的方案吗？要能支持动作的；

### 助手

UV2 不跟骨骼蒙皮，动作一变形描边就会歪。我先看你现在的烘培和描边顶点怎么读这条数据，再给你一个能跟着动画走的方案。

### 助手

UV2 不会被蒙皮，关节一弯描边方向还停在绑定姿势。原神管线是把平滑法线写进 **TANGENT**——Unity 会跟骨骼一起转。我先把烘培改成写切线，Shader 那边你按下面改。

### 助手

能跟动作走的方案，就是把平滑法线写进 **TANGENT**，不要写 UV2。

Unity 蒙皮只会变换这三条：`POSITION`、`NORMAL`、`TANGENT`。UV 只是插值，骨骼弯了它还停在绑定姿势。所以 UV2 里哪怕烘得再对，胳膊一抬、头一转，描边方向就和变形后的表面对不上，看起来就是「有的地方不顺」。另外 Unity 经常把 UV 存成 `float2`，法线的 Z 会丢，正面/背面更容易裂。

切线通道是 `float4`，而且会被蒙皮。这就是原神 / lilToon 这类描边的做法：光照仍用硬 `NORMAL`，挤轮廓用切线里的平滑法线。你的 Body / Face 本来就不用切线空间法线；Hair 里的 TBN 是以前各向异性留下的，当前 Blinn 没用到，可以先占掉。

烘培已经改成写 `TANGENT.xyz`（`w` 保留）。选中角色 → **RenderingLab → Reimport Models (Smooth Outline Normals)**。

Shader 你自己改这三处：

**1. `Genshin_Outline.hlsl` 用平滑法线挤**（你现在这行是注释掉的，所以一直在用硬法线）

```hlsl
float3 n = (dot(smoothNormalOS, smoothNormalOS) > 0.01) ? smoothNormalOS : normalOS;
float3 normalWS = TransformObjectToWorldNormal(n);
```

**2. Outline 顶点输入改切线，去掉 TEXCOORD2**

```hlsl
struct OutlineAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
};
```

调用改成：

```hlsl
o.positionCS = GenshinOutlinePositionCS(
    IN.positionOS.xyz, IN.normalOS, IN.tangentOS.xyz, _OutlineWidth);
```

Body / Hair / Face 三个 Outline Pass 都一样。

**3. 验证**：Debug 把 Hair 的 tangent 视图打开，应看到平滑的方向场，关节动的时候它会跟着转。

以后如果头发要做真正的各向异性，切线通道会打架——那时头发描边用硬法线即可（头发通常本来就是一组平滑组）。身体继续用切线里的平滑法线。

## 126. 你（Wednesday, Sep 9, 2026, 11:19 PM (UTC+8)）

那如果我的模型有用到normal map，tangent这个就不能用了是吧？

### 助手

对，**如果要把物体空间的平滑法线直接写进 `TANGENT.xyz`，和 normal map 会抢同一条通道。**

法线贴图在切线空间，片元里要靠网格自带的 TBN 变到世界空间：

`worldN = T * map.x + B * map.y + N * map.z`

`TANGENT.xyz` 是真正的切线，`w` 是副切线朝向。你把它换成平滑法线，TBN 就坏了，normal map 会花、会翻。

但这不代表「用了 normal map 就不能做带动画的平滑描边」。物体空间方向必须跟着骨骼转，所以只能搭在**会被蒙皮的坐标系**上。切线本身会被蒙皮，于是可以改存法：

**平滑法线存成切线空间，放 UV2；描边时用蒙皮后的 TBN 重建。**

烘的时候（绑定姿势）：

```hlsl
ts.x = dot(smoothOS, T)
ts.y = dot(smoothOS, B)
ts.z = dot(smoothOS, N)
```

描边顶点里（动画之后，TBN 已经被蒙皮）：

```hlsl
float3 smoothWS = normalize(T * ts.x + B * ts.y + N * ts.z);
```

UV 不用蒙皮。道理和 normal map 一样：贴图/数据停在切线空间，TBN 一转，方向就跟着表面走。UV2 只当 `float2` 也行，八面体压成两份量即可。

另外两条备选，一般不如这条干净：

| 做法 | 结果 |
|---|---|
| 片元里用 `ddx/ddy` 重建 TBN | `TANGENT` 让给描边，normal map 不读网格切线；接缝和 MikkTSpace 烘出来的图可能对不齐 |
| 硬法线挤轮廓 | 通道不打架；有硬边的衣服/装备仍会锯齿 |

你现在这套原神角色 shader 没有 normal map，Body/Face 占用 `TANGENT` 是对的。以后某套装备要上法线贴图，再把平滑法线改成切线空间塞 UV2，正向 Pass 继续用真正的切线解 normal map。

## 127. 你（Wednesday, Sep 9, 2026, 11:25 PM (UTC+8)）

这个的意思是，把切线空间塞进uv2，然后切线还不影响normal map？

### 助手

对。**切线通道原样留给 normal map，UV2 只多存一份「用切线空间表示的平滑法线」。** 两条数据各干各的。

正向光照还是老样子，完全不碰 UV2：

```text
TANGENT  → 真正的切线 T（蒙皮）
NORMAL   → 硬法线 N（蒙皮）
B        → cross(N, T) * tangent.w
法线贴图 → TBN * map  → 世界空间法线
```

描边 Pass 多读一个 UV2：

```text
UV2      → 绑定姿势下的平滑法线，已经点到 TBN 上
           ts = (dot(smooth, T), dot(smooth, B), dot(smooth, N))

动画后：
smoothWS = normalize(T * ts.x + B * ts.y + N * ts.z)
```

UV2 本身不会被蒙皮，但它存的是**相对 TBN 的坐标**。骨骼一转，T、B、N 一起转，乘出来的 `smoothWS` 也就跟着转。这和法线贴图能跟着动作走是同一件事。

所以不是「切线塞进 UV2」，而是：

| 通道 | 存什么 | 谁用 |
|---|---|---|
| `TANGENT` | 真正的切线 | normal map **和** 重建描边法线 |
| `NORMAL` | 硬法线 | 光照（硬边）+ 重建描边法线 |
| `UV2` | 平滑法线的切线空间坐标 | 只给描边 |

你现在这套角色没有 normal map，所以直接把平滑法线写进 `TANGENT` 更简单，少一次重建。以后某张图要上法线贴图，再改成「切线留给 TBN，平滑法线改存 UV2」这一套。

## 128. 你（Wednesday, Sep 9, 2026, 11:27 PM (UTC+8)）

这套方案出来的描边效果和直接用切线的描边效果是一样的？

### 助手

**静止、均匀缩放时，肉眼上可以当成一样。** 不是同一条被蒙皮的向量，但是同一个方向，描边挤出结果几乎重合。

直接写切线：

```text
绑定姿势的 smoothOS
    → 塞进 TANGENT
    → Unity 按切线去蒙皮
    → 得到动画后的平滑法线
```

UV2 + TBN 重建：

```text
绑定姿势：ts = TBN⁻¹ * smoothOS   （点到切线空间）
动画后：  smooth = TBN * ts         （TBN 已被蒙皮）
```

如果 TBN 是正交的，骨骼又只是旋转，两边就是同一个 `R * smooth`。

会有一点点差的情况：

- 骨骼带**非均匀缩放**时，法线走逆转置，切线走普通 3×3，TBN 不再正交，重建会略歪。
- 烘的时候用的 B，和 shader 里 `cross(N, T) * w` 不一致（尤其是镜像 UV）。
- UV2 压成 `float2` 时精度比切线里的 `float3` 低。

描边只用到视空间法线的 XY，差几个像素才看得出来。角色没有非均匀缩放、切线按 Unity/MikkT 那套烘，就可以当效果相同。

你现在没有 normal map，继续直接用切线更干净，少一次重建，也没有这点误差。UV2 方案是「切线要留给法线贴图」时的替代，不是画质升级。

## 129. 你（Wednesday, Sep 9, 2026, 11:32 PM (UTC+8)）

OK，那角色的shader部分基本上完成了；现在我想做一个倒影，

### 助手

角色正向已经能进反射相机，接下来按参考图做平面倒影。我先看场景和 URP 现在怎么搭，再定相机、地板 shader 和淡出怎么接。

### 助手

参考图是镜面平面倒影，不是 SSR。角色 shader 不用改：另开一台镜像相机把角色（含描边）画进 RT，地板再采样、上色、淡出。我先把相机脚本和地板 shader 接上。

### 助手

参考图是**平面倒影**：地板当镜子，角色用同一套 Body / Hair / Face / Outline 再画一遍，不是屏幕空间反射。

已经接进 `YuanShenChara` 场景里的 Plane：脚本负责镜像相机，地板 shader 负责采样、染色、模糊、淡出。角色 shader 不用改。

**怎么工作**

1. 主相机要画之前，绕地板法线把相机镜像过去。  
2. 用斜近裁剪切掉地板以下，避免画到镜子背面。  
3. `GL.invertCulling = true`，镜像后绕组反了，正面还是正面，描边的 Cull Front 也还在。  
4. 清成透明黑，只留下角色。  
5. 地板用屏幕 UV 采样这张 RT。点在平面上时，主相机和镜像相机投到同一像素，所以对得上。

**材质上可以调**

| 参数 | 作用 |
|---|---|
| Floor Color | 深蓝底 |
| Reflection Tint / Intensity | 倒影偏暗、偏青 |
| Blur | 参考图那种发软 |
| Fade Distance / Power | 离脚下越远越淡（站立角色看起来很像按高度淡） |

RT 默认半分辨率，本身也会软一点。场景里如果还有 Capsule 占位，倒影里也会出现，关掉即可。

回 Unity 看 Game 视图。如果整张倒影上下或左右反了，说一声，那是 UV 翻转，改采样那一行就行。后面如果要「按真实离地高度淡出」，再加反射深度，现在这套先对齐参考图。

## 130. 你（Wednesday, Sep 9, 2026, 11:46 PM (UTC+8)）

这个是等于新建了一相机渲染了一遍角色是吧？ 但是我在场景中并没有看到另外一个相机？

### 助手

对，就是**临时建了一台相机，把角色又画了一遍**到一张 RenderTexture 上，不是把模型复制到地板下面。

Hierarchy 里看不到，是因为创建时写了：

```153:156:Assets/RenderingLab/Cha/Scripts/PlanarReflection.cs
            var go = new GameObject("Planar Reflection Camera");
            go.hideFlags = HideFlags.HideAndDontSave;
            _reflectionCamera = go.AddComponent<Camera>();
            _reflectionCamera.enabled = false;
```

`HideAndDontSave`：不出现在层级、不存进场景，停 Play / 关脚本就毁掉。  
`enabled = false`：不会自己往 Game 视图画，只在主相机开始渲染前被手动 `Render()` 一次，输出进 `_PlanarReflectionTex`。

镜像后的位置每帧按主相机算，所以也不需要你在场景里摆一台看得见的相机。想在 Hierarchy 里看到它的话，把 `hideFlags` 去掉就行，调试用，平时藏着更干净。

## 131. 你（Wednesday, Sep 9, 2026, 11:51 PM (UTC+8)）

你看原神的角色展示界面，他的是不是场景也是反射的？他这个是不是ssr反射？

### 助手

先对照你这张展示界面，看倒影里到底有什么、没有什么，再判断是平面镜还是 SSR。

### 助手

**不是 SSR。** 展示界面里真正被当镜子用的，几乎只有角色；后面那团青绿「场景」看起来像倒进去了，其实是地板颜色和雾，跟天空对上了。

这张法露珊可以对上几件事：

**1. 倒影里只有角色**  
脚、裙摆、鞋底是干净的竖直镜像，接地点对得上。空中的金火花、青粒子在地板上没有一一对应的倒影。要是 SSR，屏幕上半的粒子会糊到地板上。

**2. 天并没有被镜像**  
上面是亮青，下面是暗蓝。如果天空盒被平面反射进来，靠近镜头的地板应该更亮（倒过来的天顶）。现在是越往下越暗，说明空地板就是一块深色材质，再叠一点氛围色。

**3. 为什么看起来像「整个场景都在反」**  
展示场本来就没有房间、柱子、地面细节，只有角色 + 渐变 + 粒子。镜子里能看见的 3D 物体本来就只有人，所以整张图会像「场景都在反」，其实只是角色倒影铺在同色地板上。

| | 展示界面 | 开放世界水面 |
|---|---|---|
| 做法 | 平面倒影（多画一次角色） | SSR / 屏幕空间平面反射一类 |
| 反什么 | 角色（粒子、天空通常不进这台相机） | 屏幕里已有的东西 |
| 特征 | 接地点稳、无屏幕边缘缺一块 | 出屏、被挡住的会缺、会拉伸 |

SSR 的硬伤是：**屏幕上没有的东西反不出来**。镜头推到脸的时候，身体下半可能已经出画，SSR 地板上的腿会烂掉或消失。角色展示要绕着人转、拉近，用平面反射最稳，也就是我们现在这套（反射相机清成透明，只留下角色）。

开放世界走路、水面、湿地才是 SSR 的场；那套和展示界面不是同一条。想更像原神，下一步是粒子不要进反射相机，地板颜色对上背景雾，而不是改成 SSR。

## 132. 你（Thursday, Sep 10, 2026, 12:00 AM (UTC+8)）

单是我看他地面更像是倒影了天空，除了前景飘的粒子的光点以外；我分析是，地面倒影了天空，而且地面倒影的天空被模糊了，天空的贴图自带一些星光斑点，从地面倒影上可以看出一些对称的地方，看我给你图片中标记的一处天空和一处地面，这看是去就是一样的

### 助手

你标的那两处是对的：地面下半张就是**同一张天空**倒过来，再被糊过一遍。上一句说「地板只是涂了氛围色」说浅了，星点能对上，不是巧合。

那仍然**不是 SSR**。前景粒子你已经排除了：真用屏幕空间反射，那些飘着的光点也会进地板。星点在、粒子不在，说明天空和粒子不在同一条反射路径上。

更像两层叠在一起：

**1. 天空 → 模糊倒影（无限远）**  
星光是天空贴图/天空盒上自带的，不是场景里的灯。水平镜子照无限远的天，看起来就是绕地平线上下对称。你圈的那组三点，就是这个。糊，是地板当粗糙镜面：降分辨率、mip、高斯都可以，整片天用同一套模糊，所以倒影里的星比天上的大、软。

中间那条亮青「地平线」也符合：渐变最亮的一环在天边，一镜像，亮带正好落在分界上。

**2. 角色 → 另一条清晰的平面倒影**  
脚、裙还是之前那套镜像相机，清透明、不画粒子、通常也不靠这张 RT 去画天。所以人是清楚的，天是糊的，粒子没有倒影。

**3. 粒子 → 最后叠上去**  
加法特效，反射相机剔除这一层。

实现上，地板 shader 可以是：

```text
floor = 模糊的天空反射（cubemap 或同一张天图，用反射视线去采）
      + 角色平面 RT（现在这张）
      + 地板自身颜色
```

天空那层不必再开一台相机：天是无限远，`reflect(-V, 地板法线)` 去采样就行，糊直接用 cubemap 的高 mip。角色才需要那次额外渲染。

所以：地面**确实在倒天空**，而且倒的是贴图上的星，不是 3D 场景；角色是另一路；整体不是 SSR。你这张对比已经够把这三层拆开了。

## 133. 你（Thursday, Sep 10, 2026, 12:05 AM (UTC+8)）

那天空可能是一张cubemap，然后地面是一个平面，这个平面反射天空的cubemap，这种cubemap也很有可能地平线的位置就结束了，就是亮青色的位置结束，亮青色一下的cubemap有可能是黑色或者暗色的，这样分析对吗？

### 助手

大方向对：**天很可能是 cubemap（或等效的天空盒），地是平面，用反射向量去采这张天。** 角色还是另外那次平面渲染。

有一处容易推反：地面看起来暗，**不是因为 cubemap 在亮青下面涂黑了。**

水平镜子采的是**天上那一半**，不是 cubemap 的底：

```text
视线 V 打到地板（法线 +Y）
R = reflect(-V, +Y)   → 把视线的 Y 一翻，朝上指

屏幕下方、靠近镜头的地板  →  R 更朝天顶    → 采到 cubemap 上面（暗青 + 星）
接近地平线的地板        →  R 更水平      → 采到 cubemap 赤道（亮青）
```

所以你看到的是：

| 屏幕位置 | 实际在看 cubemap 哪 | 观感 |
|---|---|---|
| 最上 | 天顶 | 暗 |
| 中间亮带 | 赤道 / 地平线 | 亮青 |
| 地板近处（画面底） | **还是天顶**（倒过来） | 又暗，星点能和天上对上 |

下面那块暗，和上面那块暗是**同一圈天顶**，不是「地平线以下的贴图」。你圈的对称星点，正好说明地板在采上半球。

Cubemap 底下涂黑，这件事**可以做**（展示场反正有地板挡住，往下看也用不到），但它几乎不影响这张构图。要是底下是黑、地板又去采下半球，倒影里不该有那些星。

亮青更像赤道上一圈最亮的渐变，不是贴图在这儿裁切。真裁成「上半有图、下半纯黑」，没地板的缝、斜视边缘会硬一刀；原神那条亮带是糊的、上下连着的，更像整球都画了：天顶暗 → 地平亮 → 底也暗。底下画暗只是为了填满立方体，不是地面暗的原因。

地板 shader 里天空那层就是：

```hlsl
float3 R = reflect(-V, float3(0, 1, 0));
half3 sky = SAMPLE_TEXTURECUBE_LOD(_SkyCube, sampler_SkyCube, R, blurMip);
```

`blurMip` 一大，星就和参考图一样发糊。再叠角色那张 RT。

## 134. 你（Thursday, Sep 10, 2026, 12:11 AM (UTC+8)）

看我画的图，加入地面的面片模型的边在这个位置，上面的是天空盒，下面蓝色标记是地面的面片，当地面反射天空的时候，我红线画的地面的边缘刚好也能反射天空我绿色标记的地方的亮青色吗？ 可以把地面面片的接缝隐藏掉？

### 助手

颜色上对：**擦着地平线的地板，反射的就是天空盒那圈亮青。** 绿和蓝采的是 cubemap 同一条赤道，红线两边可以接到看不出颜色接缝。

擦地视线几乎水平：

```text
V ≈ 水平（略朝上指相机）
R = reflect(-V, +Y) ≈ 水平朝远方
→ 地板边缘采到 cubemap 地平线（亮青）
→ 红线上面的天空盒也是朝水平看，同样采到亮青
```

再加一层 Fresnel：越擦边反射越强，地板在红线附近几乎变成纯天空，自身那点深蓝会被盖住。这就是「接缝藏进亮带」的原因。

但红线不该是**面片模型的几何边**。

面片如果真的在红线裁切，那是一条 3D 多边形边。镜头一转、一俯仰，这条边不会钉在屏幕地平线上，天空和地板会错开，接缝立刻露出来。原神那条线是**很大一块地面向远方灭掉**的视觉地平线，网格边在视锥外面，你永远看不到。

做法：

- 平面做很大（或者一个大圆盘），把角色放中间，边远到画面里切不到。
- 红线让它自己出现：擦地反射 + 天空盒地平线亮青对上。
- 地板 cubemap 用更高 mip（糊），天上用清晰采样。接缝处一边略糊一边略清，反而像大气，不必强行一样锐。

所以：**亮青对上，能把「天和地」藏住；藏不住的是「网格裁在红线上」。** 面片要铺过地平线，不是停在地平线。

## 135. 你（Thursday, Sep 10, 2026, 12:14 AM (UTC+8)）

我现在在场景中放置了一个skybox，这个skybox刚好也是上暗，地平线位置亮， 做一个实验，把地面的片加入skybaox的反射，看看效果

### 助手

地面加上天空盒 cubemap 反射。我先看你现在的地板 shader 和场景里的 skybox 怎么接的。

### 助手

场景里环境反射强度是 0，不能走 Unity 的 GI。地板会直接采天空盒进 `unity_SpecCube0` 的那张图，并加一个可选的自定义 Cubemap 槽。

### 助手

已经加到地板上：先用反射向量采天空，角色倒影再叠上去。

```hlsl
R = reflect(-V, N)          // 水平镜，采的是天上那一半
sky = cubemap.Sample(R, mip)
col = lerp(地板色, sky, fresnel)   // 擦地平线 fresnel→1，亮青接上
col = lerp(col, 角色RT, mask)
```

**怎么看这次实验**

默认关着 **Use Custom Cubemap**，采的是场景天空盒烘焙进 `unity_SpecCube0` 的那张图，**不走** Lighting 里那个被你设成 0 的 Reflection Intensity。

- 天空盒是 **Cubemap 材质**：勾上 Use Custom Cubemap，把同一张 Cube 拖进 `Sky Cubemap`。
- 天空盒是 **Procedural**（默认那套）：菜单 **Window → Rendering → Lighting → Generate Lighting** 一次，探针才会带上你调的「上暗、地平线亮」。
- `Sky Mip` 拉到 4～6：星点发糊。  
- `Sky Fresnel` 大约 4：脚下偏地板色，远处亮青和天接上。

镜头水平看着角色，地平线附近应该和天空亮带对上。对不上或纯黑，把 Cube 拖进材质槽再看。

## 136. 你（Thursday, Sep 10, 2026, 12:40 AM (UTC+8)）

好像真的可以诶

### 助手

对，这就是那套几何在干活。

地平线那圈最亮的青白，天空盒正着看是它，地板 `reflect(-V, +Y)` 擦边采到的也是它，所以红线两边颜色能接上。云也被倒过来了，说明采的是同一张 cubemap，不是地板自己画了个渐变。

还能看见一条细线，正常：天上是清晰天空盒，地上是带 mip 的糊反射，锐度差一点点。原神也是倒影更糊，靠亮带把接缝「吃」进雾里，不是数学上无缝。

角色倒影是另一路（那张平面 RT）。天空亮的时候容易把人冲掉，把 `Sky Fresnel` 或脚下 `Floor Color` 略压一点，人的倒影会重新出来。天空这层你已经验证过了。

## 137. 你（Thursday, Sep 10, 2026, 12:45 AM (UTC+8)）

后面我要做一些粒子效果的飘动的云，我也可以把这些飘动的云也反射到地面的面片上吗？

### 助手

可以，但不要塞进 cubemap。飘着的云是场景里会动的东西，跟角色走同一条路：**平面反射那台相机把它画进 RT**。

Cubemap 是无穷远、基本不更新的天。云放进去会没有远近、不会从脚边飘过，还得每帧重烘整张 cube，不划算。

| | 放哪 | 地面怎么采 |
|---|---|---|
| 天空、地平线亮带、远处死星 | cubemap | `reflect(-V, N)` |
| 角色、近处飘的云 | 平面 RT | 屏幕 UV |
| 前景火星、闪光 | 图层剔除，不进反射相机 | 不倒 |

粒子默认就会被那台相机关进来（现在只关了地板自己）。要单独控制的话，给云一个层，反射相机 `cullingMask` 勾上；火星那层去掉。

有一个坑：地板现在是用 **RT 的 alpha** 当角色遮罩的。加法粒子往往 `alpha = 0`，只加 RGB，这样云在 RT 里有颜色，叠到地上会被 mask 乘没。云用 **Alpha Blend**（带不透明度），或者地板改成 RGB 也参与混合，例如：

```hlsl
half mask = saturate(max(planar.a, luma(planar.rgb))) * fade;
col = lerp(sky, planar.rgb * _ReflectionTint.rgb, mask);
```

广告牌粒子会对着**正在画它的那台相机**转，反射相机里会自动朝镜像镜头，倒影方向是对的。软粒子如果用了场景深度，镜像相机里深度是倒过来的世界，接地点可能要再调。

所以：远天继续 cubemap；飘的云进平面反射，和角色一张 RT。做云的时候记得层和混合模式。

## 138. 你（Thursday, Sep 10, 2026, 8:39 PM (UTC+8)）

OK，这个后面再做，现在角色基本上接近渲染完成了，还差一个后处理

### 助手

角色主体可以先收口，接下来补后处理。我先看场景 Volume 和相机现在开了哪些，再对上原神展示那种bloom、调色。

### 助手

场景里后处理**管道已经在跑**：相机开了 Post Processing，也有全局 Volume。缺的是按展示界面把效果配齐——现在 Profile 里只有 Bloom + Tonemapping，而且 Tonemapping 的 Mode 是 **None**，等于几乎没做色调映射。

原神角色展示一般就这几层，全是 URP 自带 Override，不用新写 Shader：

| 效果 | 干什么 | 建议起点 |
|---|---|---|
| **Bloom** | 只让书本发光、金属、粒子冒光，衣服不要糊成一片 | Threshold `0.9`～`1.2`，Intensity `0.25`～`0.6`，Scatter `0.7` |
| **Tonemapping** | 把 HDR 收进显示器 | **Neutral**（别开 ACES，二次元会被压脏） |
| **Color Adjustments** | 对比、饱和、一点曝光 | Contrast `10`～`20`，Saturation `10`～`20`，Post Exposure `0`～`0.3` |
| **Vignette** | 可选，压四角把人推到中间 | Intensity `0.15` 左右，能看出来就减 |
| **Depth of Field** | 全身展示不要开 | 关 |

选中 **Global Volume** → Profile 里 **Add Override → Post-processing**，把上面几项加上。Bloom 现在 Threshold/Intensity 都是 1，要么爆要么几乎看不见，先按表改。

和角色 Shader 的衔接：后处理吃的是 **HDR 值 > 1** 的像素。书本 emission、金属 Matcap 拉高才会进 Bloom；ramp 里的衣服是 0～1，本来就不该Bloom。

粒子云还没做，先把这一套调到人好看即可。若要自己写一层全屏 Bloom/调色（Renderer Feature），再说，那是另一课。

## 139. 你（Thursday, Sep 10, 2026, 9:12 PM (UTC+8)）

OK，原神的角色shader可以做完一个完整的ppt吗？或者如果你不擅长的话，可以把关于原神的shader的对话内容导出来，我让codex去做

### 助手

可以做完整 PPT。仓库里已有一版，但停在描边和倒影之前。我对照现在的 Body / Hair / Face / 描边 / 倒影把内容补全。

### 助手

旧 PPT 停在脸部，没写描边、平滑法线、倒影和后处理。我按现在这套管线整份重做。

### 助手

已经做成完整 PPT，直接打开：

`Docs/GenshinCharacterShading.pptx`

上一版只写到脸。这一版 32 页，按你们实际落地的代码收口：

1. **架构** — 三套 Shader、贴图、ILM 通道对照（身体 / 头发 / 脸不要混）  
2. **身体** — 半兰伯特 Ramp、两档金属（灰 Matcap + 金 Blinn）、深度 Rim、自发光  
3. **头发** — 两行 Ramp、发丝高光 vs 头饰金属  
4. **脸** — 水平 SDF、L.xz、头骨朝向、Head Lightmap  
5. **描边** — `SRPDefaultUnlit`、裁剪空间外扩、平滑法线进 **TANGENT**（以及以后有 Normal Map 怎么改）  
6. **展示场** — 平面反射相机、Cubemap 地板、为什么不是 SSR  
7. **后处理** — Bloom + Neutral，ACES 不要开  
8. **坑 / 文件 / 待补** — 脸 AO 还没乘、Volume 还要调、粒子云以后进平面 RT  

改 Shader 以后要更新幻灯片，在仓库根目录跑：

```bash
python Docs/build_genshin_ppt.py
```

不用再导对话给 Codex。脚本就是这份 PPT 的源。

## 140. 你（Friday, Sep 11, 2026, 8:17 PM (UTC+8)）

我想在这个角色材质球里面集成做ramp贴图的工具，这样我就可以实时的在材质球面板实时的调整ramp条的颜色，不用到photoshop里面区制作了，你先看一下这个工具的实现思路，先不用给我写代码；

### 助手

先对照你现在的 Ramp 采样，再拆这套材质面板是怎么把渐变烘成贴图的。不写代码。

### 助手

这套面板的本质不是「在材质里采样 Photoshop」，而是：**Inspector 里编若干条渐变 → CPU 烘成一张 Ramp 图 → 赋给现在的 `_RampMap`**。Shader 仍然是 `SAMPLE_TEXTURE2D`，不用改采样模型。

**面板在干什么**

| UI | 实际数据 |
|---|---|
| Day / Night | 两套渐变。下拉只决定你在编哪一套；运行时仍由 `_IsNight` 选 V |
| 行 0、1、2… | 一条 `Gradient`（左暗右亮 = rampU 0→1） |
| `+` / `-` | 增删行，改图高度 |
| 条带预览 | 把 Gradient 画成 256 宽的一行像素 |
| RampOffset | 多半是 UV 微移（U=明暗分界、V=换行），不重烘也能拧 |

拖条上的颜色时：改 Gradient → `SetPixels` 写进一张 `Texture2D` → 材质立刻看到。这就是「不用 PS、还能实时看」。

**和你现在这张图怎么对齐**

你的 Shader 已经把图当成 **10 行、每行中心采样**：

```text
rampV = 1 - (night * 5 + row + 0.5) / 10
身体 row = 0/1/2    头发 row = 0/1
白天占上 5 行，夜晚占下 5 行（Unity V=0 在底）
```

所以工具不要做成「随便加到 6 行、高度跟着变」就完了，除非连 `/10` 和 `night*5` 一起改。稳妥约定：

- 白天 5 行 + 夜晚 5 行，烘 **256 × 20**（每行 2 像素），和现有图一模一样  
- 身体只用 0～2，头发只用 0～1，多出来的行留空即可  
- 过滤保持 **Clamp + Point + 关 Mip**，否则行会串

截图里 Day 有 0～5 共 6 行，比你的 5 行多一档。接进来时要么 UI 锁成 5，要么改 Shader 的分母，不要两套各说各话。

**数据放哪（比 UI 更关键）**

1. **渐变本身**：存在材质隐藏字段（JSON / 一串 Color）或旁路一个小 ScriptableObject。只靠贴图像素反推 Gradient 不稳。  
2. **贴图**：内存里那张图只负责预览；要进工程、给 Git，还得写出 `png` 再赋回 `_RampMap`。不写盘，重开 Unity 渐变还在、图可能丢。  
3. **sRGB**：当颜色图烘，和现在 Barbara 的 Ramp 导入一致，否则面板里对的色，游戏里会偏。

**实时的范围**

- 改条带颜色、Day/Night 切换编辑：每帧烘 256×20，开销可以忽略。  
- 不要用 Shader 里塞 8 个 color key 去替代贴图——和原神 ILM 选行不是同一套，Debug 看 `ramp` 也会对不上。

**RampOffset** 建议先当调试用：X 微移 U（阴影分界左右挪），不必改 Gradient。你现在的明暗分界已经有 `_ShadowThreshold`，功能会有一点重叠，集成时想清楚谁说了算。

**和 PS 流程的关系**

官方 Ramp 仍是「一张图、多角色共用」。这个工具适合**按材质调自己的行**；调完可以导出 png，当共享 Ramp 用。不要每个材质球各烘一张又去改 ILM 的 A 行号，行语义会乱。

下一步写的话，就是一个 Body/Hair 共用的 `ShaderGUI`：画这些条、烘 10 行图、写回 `_RampMap`。Shader 采样可以不动。你要开始写再说。

## 141. 你（Friday, Sep 11, 2026, 8:22 PM (UTC+8)）

我发现用了这个ShaderGUI以后，他的材质球下会新生成两个文件，这两个文件就是用来保存Ramp图的吗？

### 助手

对，**真正存 Ramp 像素的是 `GM_RampTex`**。它一般不是旁边另存的 png，而是挂在材质球下面的 **子资源**（和 `.mat` 写在同一个资源里）。

Unity 工程窗口里，材质能展开出子物体，就是 `AssetDatabase.AddObjectToAsset` 这种做法：主资源是材质，下面再塞一张 `Texture2D`。Shader 的 `_RampMap` 指的就是这张图。你在面板上改条带，GUI 改的是渐变数据，再 `SetPixels` 写进 `GM_RampTex`，所以能实时预览，也不用开 PS。

**`GM_Folder` 通常不是第二张 Ramp。** 它多半是一个空的 `ScriptableObject`，只为了在工程里看起来像文件夹、把 Ramp 收进材质下面，有的实现还会把 Gradient 的 JSON/色标存在它上面。你截图里它带警告，很像 **脚本丢了**（GUI 那套程序集没进工程，或 class 改名了）。文件夹对象坏了，Ramp 贴图作为 `Texture2D` 往往还能用；但渐变编辑数据如果存在 Folder 上，重开会丢，条带对不上像素。

可以这么记：

| 对象 | 干什么 |
|---|---|
| 材质球 | 参数、引用 `_RampMap` |
| `GM_RampTex` | 烘好的 Ramp 图（Shader 真正采样的） |
| `GM_Folder` | 层级/容器，有时存渐变；不是给 GPU 采样的 |

和「一张独立 png」的差别：拷贝材质会连 Ramp 一起走，不用找贴图路径；`.mat` 会变大，Git 也不好看图 diff。官方角色仍是共享 png；这套是 **每颗材质自带一份烘好的 Ramp**。

## 142. 你（Friday, Sep 11, 2026, 8:25 PM (UTC+8)）

OK，那我准备开始制作这个工具了，就在之前我们好的身体脸头发的材质球上做吧；我也顺便学习一下ShaderGUI的做法，我们学习的过程还是按之前写shader的方法，你说逻辑和实现以及代码的解释，我来编写代码；最好是步骤尽量清晰，一步步来，不要一开始就给我一段很长功能很多的代码；

### 助手

先只做一件事：**让材质球用上你自己的 Inspector**。不烘 Ramp、不画条带。脸没有 Ramp，这一步只改 **Body**（跑通后再给 Hair 加同一行）。

Unity 默认用内置面板画 Properties。你在 Shader 末尾写 `CustomEditor "类名"`，它就会改去调这个类的 `OnGUI`。类必须放在 **Editor** 目录，否则进游戏包会编不过。

**1. 新建** `Assets/RenderingLab/Cha/Scripts/Editor/GenshinCharacterGui.cs`

```csharp
using UnityEditor;
using UnityEngine;

namespace RenderLab.Editor
{
    public class GenshinCharacterGui : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            EditorGUILayout.HelpBox("GenshinCharacterGui 已接上", MessageType.Info);
            base.OnGUI(materialEditor, properties);
        }
    }
}
```

`base.OnGUI` = 原来那一长串属性原样画出来。`HelpBox` 只是证明走的是你的代码，不是默认面板。

**2. 打开 `Genshin_Body.shader`，在最后一个 `}` 之前加一行**（和 `FallBack` 同级，在 Shader 块里面）

```csharp
CustomEditor "RenderLab.Editor.GenshinCharacterGui"
```

有 namespace 就要写全名，只写类名会找不到，材质球会变粉、属性全没。

选中身体材质：面板**最上面**应出现那句 HelpBox，下面仍是 BaseMap、Ramp、描边那些。没有这句话 = CustomEditor 没对上，先看 Console 的 ShaderGUI 报错。

这一步通了再做下一步：在默认属性上面，自己画出 `_RampMap` 那一张。

## 143. 你（Friday, Sep 11, 2026, 8:36 PM (UTC+8)）

已经显示了，把我单做一个C#的新手，你看怎么教我理解这个GUI代码的知识和原理；

### 助手

先建立一张地图：这段代码**不进游戏、不跑在角色身上**，只在你选中材质球时，替 Unity 画 Inspector。

**两套程序**

| | 游戏运行时 | 编辑器 |
|---|---|---|
| 例子 | `FaceHeadOrientation.cs` | `GenshinCharacterGui.cs` |
| 谁调用 | Play 之后每帧 `Update` | 你盯着 Inspector 时 Unity 反复画界面 |
| 命名空间 | `UnityEngine` | 还要 `UnityEditor` |
| 放哪 | `Scripts/` | 必须 `Editor/` 文件夹 |

`Editor` 文件夹 = 这份代码打进游戏包时会被丢掉。所以 GUI 不能写在角色脚本里。

**调用链（选中身体材质时）**

```text
Shader 末尾 CustomEditor "RenderLab.Editor.GenshinCharacterGui"
        ↓ Unity 用这个字符串去找 C# 类
GenshinCharacterGui.OnGUI(...)
        ↓ 你先画 HelpBox
        ↓ 再 base.OnGUI → Unity 按 Properties 把贴图、滑条全部画出来
```

字符串必须和 **命名空间 + 类名** 一字不差。对不上就粉材质、属性消失。

**对着你的文件逐行**

`using UnityEditor`  
编辑器 API：`ShaderGUI`、`MaterialEditor`、`EditorGUILayout` 都在这里。缺了这行，下面的类型会报找不到。

`using UnityEngine`  
`MessageType`、以后的 `Material`、`Texture2D` 在这里。编辑器脚本两边都要用。

`namespace RenderLab.Editor`  
只是文件夹式的名字，避免和别人的 `GenshinCharacterGui` 撞车。Shader 里才要写全名。

`public class GenshinCharacterGui : ShaderGUI`  
`:` 在 C# 里是「继承」。Unity 规定：想接管材质面板，必须继承 `ShaderGUI`。你不是从零发明 Inspector，是填 Unity 留好的格子。

`public override void OnGUI(...)`  
`override` = 覆盖父类的同名方法。Unity **不会**调你随便起的函数名，它只调 `OnGUI`。你不写 override，它还是走默认面板，你的 HelpBox 永远不会出现。

两个参数（Unity 塞给你的，不用自己造）：

| 参数 | 是什么 |
|---|---|
| `materialEditor` | 「当前这颗材质的画笔」：画贴图槽、记撤销、多选材质 |
| `properties[]` | Shader 里每一条 Property 对应一个 `MaterialProperty`（`_RampMap`、`_IsNight`…） |

`OnGUI` 和 `Update` 一样会**反复执行**：拖滑条、切窗口、Undo，都会再画一遍。不要在这里 `new` 一堆东西却不判断「有没有变」，后面烘 Ramp 时会踩这个坑。现在两行没问题。

`EditorGUILayout.HelpBox(...)`  
`EditorGUILayout` = 编辑器里从上往下自动排的控件。`HelpBox` 就是黄/蓝那条提示。`Layout` 表示你不用自己算像素坐标。

`base.OnGUI(materialEditor, properties)`  
`base` = 父类 `ShaderGUI`。这句话等于：「剩下的按 Unity 默认材质面板画。」没有它，你只能看到 HelpBox，BaseMap、Ramp 会全没。以后我们会改成：Ramp 自己画，其它属性仍交给 `base` 或逐条 `ShaderProperty`。

**和 Shader 的关系**

Shader 只声明数据（有一张 `_RampMap`）。  
GUI 决定**人怎么改这些数据**（默认贴图槽，或以后的颜色条）。  
真正采样仍在 HLSL。GUI 再花，GPU 看到的还是一张图。

**自己试一下（加深 OnGUI 会被反复调用）**

把 HelpBox 那行暂时改成：

```csharp
EditorGUILayout.LabelField("draw at " + System.DateTime.Now.ToString("HH:mm:ss"));
```

盯着材质面板不动，秒数会跳。说明不是点一次才运行，而是**面板可见就在刷**。试完改回去即可。

下一步才是：用 `FindProperty("_RampMap", properties)` 拿到 Ramp，自己画那一张槽。等你说开始再写。

## 144. 你（Friday, Sep 11, 2026, 8:44 PM (UTC+8)）

OK, 开始吧，还是按之前的方法啊，你说原理和代码，我来编写；

### 助手

这一步只做一件事：**Ramp 由你画，其它属性你自己循环画，不再调用 `base.OnGUI`。** 还是不烘贴图。

`base.OnGUI` 会把 Properties 里每一项都画一遍。你若再画一次 `_RampMap`，槽会重复。所以改成：先找到 Ramp → 你画 → 循环时跳过它。

把 `OnGUI` 换成下面这一段。

```csharp
public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
{
    EditorGUILayout.HelpBox("GenshinCharacterGui 已接上", MessageType.Info);

    MaterialProperty rampMap = FindProperty("_RampMap", properties, false);
    if (rampMap != null)
    {
        EditorGUILayout.LabelField("Ramp", EditorStyles.boldLabel);
        materialEditor.ShaderProperty(rampMap, rampMap.displayName);
    }

    foreach (MaterialProperty prop in properties)
    {
        if (prop.name == "_RampMap")
            continue;
        if ((prop.flags & MaterialProperty.PropFlags.HideInInspector) != 0)
            continue;

        materialEditor.ShaderProperty(prop, prop.displayName);
    }

    materialEditor.RenderQueueField();
}
```

**`FindProperty("_RampMap", properties, false)`**  
在数组里按 **Shader 变量名** 找，不是面板上的显示名。第三个 `false`：找不到就返回 `null`，不抛错。脸没有 Ramp，以后共用这个 GUI 才不会炸。默认 `true` 时找不到会报错。

**`if (rampMap != null)`**  
C# 里引用类型没找到就是 `null`。先判断再用不然后面 `.displayName` 会空引用。

**`ShaderProperty(rampMap, …)`**  
请 `materialEditor` 按类型画控件：贴图就出槽，Float 就出滑条。第二个参数是左边的字，用 `displayName` 就是 Properties 里括号里那串（`Ramp Map`）。

**`foreach`**  
`properties` 里每一个 Property 走一遍。`continue` = 跳过这次，进入下一个。  
先跳 `_RampMap`（你已经画过），再跳 `HideInInspector`（隐藏属性，默认面板也不画）。

**`RenderQueueField`**  
以前 `base.OnGUI` 末尾会画 Render Queue。不用 `base` 了就要自己补，否则 Queue 那一行会消失。

选中身体材质：最上面 HelpBox，接着加粗 **Ramp** 和贴图槽，下面仍是 BaseMap、LightMap、描边……Ramp **只出现一次**。拖一张 Ramp 图，Scene 里角色应和以前一样变。

通了告诉我，下一步用 `Gradient` 烘一行像素，还是不写盘。

## 145. 你（Friday, Sep 11, 2026, 8:50 PM (UTC+8)）

这个还需要脱一张ramp图到槽上面吗？ 这个工具是想自己生成ramp，同时可以在生成的ramp条上加锚点，调颜色；你看我给你的截图的功能，我是点击了RampTex 0槽就会弹出一个可以调颜色的界面；

### 助手

**不需要往槽里拖图。** 上一句让你画贴图槽，只是为了认 `_RampMap`。你要的工具是：点色条 → Unity 弹出 **HDR Gradient Editor**（截图那个窗口，加锚点、调颜色）→ 脚本把渐变烘成图，写进 `_RampMap`。槽里那张图是生成结果，不是手工拖进去的。

那个弹窗不用自己做。`EditorGUILayout.GradientField` 点一下就是它。

这一步只做 **一条** 渐变：改颜色，角色立刻变。先不管 10 行、白天夜晚、存盘。整张 Ramp 会被这一条临时顶掉，所以皮肤/衣服会暂时共用同一条，这是正常的。

在类里加一个字段（记当前这条渐变）：

```csharp
Gradient _row0;
```

`OnGUI` 改成：

```csharp
public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
{
    EditorGUILayout.HelpBox("点下面的色条，会弹出 Gradient 编辑器", MessageType.Info);

    if (_row0 == null)
    {
        _row0 = new Gradient();
        _row0.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.35f, 0.28f, 0.28f), 0f),
                new GradientColorKey(Color.white, 0.55f),
                new GradientColorKey(Color.white, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            });
    }

    MaterialProperty rampMap = FindProperty("_RampMap", properties, false);

    EditorGUI.BeginChangeCheck();
    _row0 = EditorGUILayout.GradientField("Ramp 行 0", _row0);
    if (EditorGUI.EndChangeCheck() && rampMap != null)
        BakeRow0(rampMap);

    base.OnGUI(materialEditor, properties);
}
```

类里再加烘培方法：

```csharp
void BakeRow0(MaterialProperty rampMap)
{
    var tex = new Texture2D(256, 2, TextureFormat.RGBA32, false);
    tex.wrapMode = TextureWrapMode.Clamp;
    tex.filterMode = FilterMode.Point;
    tex.hideFlags = HideFlags.DontSave;

    var pixels = new Color[256 * 2];
    for (int x = 0; x < 256; x++)
    {
        Color c = _row0.Evaluate(x / 255f);
        pixels[x] = c;
        pixels[x + 256] = c;
    }

    tex.SetPixels(pixels);
    tex.Apply();
    rampMap.textureValue = tex;
}
```

**字段 `_row0`**  
GUI 类上的变量，用来记住锚点和颜色。还没存进材质，换选一个材质或重开 Unity，条可能回到初始值，但足够验证点击弹窗。

**`BeginChangeCheck` / `EndChangeCheck`**  
中间的控件有没有被改。只有改了才烘，避免 `OnGUI` 每刷一次就 `new Texture2D`。

**`GradientField`**  
画出那条左暗右亮的预览。点击 → 你截图里的编辑器。`Location %` 就是锚点在 U 上的位置（0=阴影，1=受光）。

**`Evaluate(x/255f)`**  
U 从 0 扫到 1，对应 Shader 里的 `rampU`。256 宽和原神 Ramp 一样。

**`rampMap.textureValue = tex`**  
给 `_RampMap` 赋值。下面 `base.OnGUI` 里那个贴图槽会自己出现这张图，不用拖。`DontSave`：先只活在内存里，关 Unity 会丢，下一步再写进材质子资源。

点色条、挪锚点、改暗部颜色，Game 视图里角色阴影色应跟着变。槽里会多出一张 256×2 的图。通了再说：拆成 10 行、以及怎么点「第 0 条」而不是用默认的 GradientField 样式。

## 146. 你（Friday, Sep 11, 2026, 8:52 PM (UTC+8)）

OK，你明白就行，那我还是先回到上一步，你教我的进度吧，等我完成你上一步教我的；

### 助手

好，进度停在这一步：**自己画 Ramp 槽，循环画其余属性，先不要 `base.OnGUI`，也不要烘渐变。**

把 `OnGUI` 换成：

```csharp
public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
{
    EditorGUILayout.HelpBox("GenshinCharacterGui 已接上", MessageType.Info);

    MaterialProperty rampMap = FindProperty("_RampMap", properties, false);
    if (rampMap != null)
    {
        EditorGUILayout.LabelField("Ramp", EditorStyles.boldLabel);
        materialEditor.ShaderProperty(rampMap, rampMap.displayName);
    }

    foreach (MaterialProperty prop in properties)
    {
        if (prop.name == "_RampMap")
            continue;
        if ((prop.flags & MaterialProperty.PropFlags.HideInInspector) != 0)
            continue;

        materialEditor.ShaderProperty(prop, prop.displayName);
    }

    materialEditor.RenderQueueField();
}
```

通过标准：身体材质上 Ramp **只出现一次**（上面加粗那块），下面仍是其它贴图和滑条，Render Queue 还在最底。还能往 Ramp 槽里拖图，角色显示正常。

做完再说，再往「点色条生成 Ramp」走。

## 147. 你（Friday, Sep 11, 2026, 9:09 PM (UTC+8)）

刚刚那一步的目的是什么？现在材质球界面上并没有改变；

### 助手

界面几乎没变，有两个原因，第二个才是这次没生效。

**这一步的目的不是换皮肤，是把画笔从 Unity 手里拿过来。**  
`base.OnGUI` 等于「全部默认画法」。改成自己 `Find` + `foreach` 之后，看起来可以几乎一样，但 **Ramp 先画谁、画几次、以后换成色条** 都由你说了算。外观大变是下一步；这一步通了，只应看到：Ramp 被提到上面，并且只出现一次。

**你现在没提到上面，是因为没找到这个属性。**

`FindProperty` 要的是 Shader 里的**变量名**，不是槽里那张图的文件名。

```text
Properties 里：     _RampMap ("Ramp Map", 2D) = "white" {}
                      ↑ 这个

工程里的贴图：     Avatar_Girl_..._Shadow_Ramp_Gs.png
                      ↑ 只是赋给 _RampMap 的值，不是属性名
```

你写的是贴图名，`FindProperty` 得到 `null`，`if (rampMap != null)` 整段跳过；`foreach` 里 `continue` 也对不上，`_RampMap` 仍按默认画在原来位置。所以和 `base.OnGUI` 看起来一样。

把两处字符串都改成 `"_RampMap"`：

```csharp
MaterialProperty rampMap = FindProperty("_RampMap", properties, false);
// ...
if (prop.name == "_RampMap")
    continue;
```

改完应看到：HelpBox 下面先是加粗 **Rampss** 和那张 Ramp 槽，下面列表里 **不再出现第二次** Ramp。这就是「控制权在你这」的证据。

`prop.name` 永远是 `_RampMap` 这种下划线名；`displayName` 才是面板上的 `Ramp Map`。槽里拖哪张 png 都不会改 `name`。

## 148. 你（Friday, Sep 11, 2026, 9:15 PM (UTC+8)）

@Assets/RenderingLab/Cha/Scripts/Editor/GenshinCharacterGui.cs:12 这一行意思是，去找材质球面板上的shader属性里的"_RampMap"名字，存到变量rampMap里面吗？

### 助手

对，意思就是这样，再精确一点：

不是去「已经画出来的面板」上找控件，而是在 Unity 传进 `OnGUI` 的 **`properties` 数组**里找。这个数组和 Shader 里 `Properties { ... }` 一一对应，每一项是一个 `MaterialProperty`。

```text
Shader:  _RampMap ("Ramp Map", 2D) = "white" {}
              ↑ name              ↑ displayName     ↑ 槽里的贴图是 textureValue
```

`FindProperty("_RampMap", properties, false)`：在数组里找 `name == "_RampMap"` 的那一项，放到 `rampMap` 里。找到的是**属性对象**（能读名字、能改贴图），不是贴图文件本身。

第三个 `false`：找不到就返回 `null`（比如脸的 Shader 没有 `_RampMap`），不要报错。

后面 `rampMap.textureValue` 才是槽里那张图；`ShaderProperty(rampMap, …)` 才是把这一项画到面板上。

## 149. 你（Friday, Sep 11, 2026, 9:18 PM (UTC+8)）

@Assets/RenderingLab/Cha/Scripts/Editor/GenshinCharacterGui.cs:14-27 这两个函数的目的是什么？

### 助手

这不是两个函数，是 `OnGUI` 里前后两段逻辑，都在**往 Inspector 上画控件**。真正去画的那句都是 `ShaderProperty`。

**上面那段 `if (rampMap != null)`：单独把 Ramp 画在最上面。**

- `rampMap != null`：Shader 里真有 `_RampMap` 才画（身体有，脸没有）。
- `LabelField("Rampss")`：自己加的标题，和 Shader 无关。
- `ShaderProperty(rampMap, …)`：画出 Ramp 那个贴图槽。  
有了这一段，你才能以后把它换成色条，而不动下面其它属性。

**下面那段 `foreach`：把它余属性按默认方式画完。**

- `properties` 里每一项走一遍。
- `prop.name == "_RampMap"` 就 `continue`：Ramp 上面已经画过，避免出现第二次。
- `HideInInspector` 也跳过：和 Unity 默认面板一样，隐藏属性不画。
- 其余的 `ShaderProperty`：BaseMap、LightMap、描边宽度……该滑条滑条、该贴图贴图。

合起来就是：

```text
先画 Ramp（你控制的位置）
再画「除了 Ramp 以外」的全部可见属性
```

以前 `base.OnGUI` 是 Unity 按 Properties 从上到下全画一遍。现在拆成「Ramp 一块 + 其它一块」，外观几乎一样，但 Ramp 已经从默认流水线里抽出来了。

## 150. 你（Friday, Sep 11, 2026, 10:40 PM (UTC+8)）

那这个foreach就是把材质球属性上除了_RampMap的属性之外的都再显示，如果没有foreach，材质球面板就只会有ramp这个槽吗？因为我们已经override了原本的绘制，是吗？

### 助手

对，就是这个意思。

你 `override OnGUI` 之后，Unity **只跑你写的这些**，不会再自动把 Properties 全画一遍。`base.OnGUI` 你已经去掉了，默认绘制等于关掉。

所以现在面板上有什么，完全取决于你调用了哪些绘制：

| 你写了 | 面板上会出现 |
|---|---|
| `HelpBox` | 顶上那句提示 |
| `if` 里的 `ShaderProperty(rampMap)` | Ramp 槽 |
| `foreach` | 除 `_RampMap` 以外的可见属性 |
| `RenderQueueField` | 最底下的 Queue |

**没有 `foreach`：** 就只剩提示 + Ramp 槽 + Queue，BaseMap、光照、描边都会消失。不是坏了，是你没画它们。

`foreach` 的作用就是把「剩下的」补回去，只把 Ramp 留在你自己那段里。

## 151. 你（Friday, Sep 11, 2026, 10:41 PM (UTC+8)）

OK，明白了，接下来继续吧

### 助手

Ramp 槽已经由你画了。这一步把它换成 **一条可点的渐变**：点开就是 Unity 的 Gradient 编辑器，改完烘成图写进 `_RampMap`。不用拖 png。`foreach` 不用动。

先在类里加一个字段，用来记住这条渐变的锚点和颜色：

```csharp
Gradient _row0;
```

把 `if (rampMap != null) { ... }` 整段换成：

```csharp
if (rampMap != null)
{
    if (_row0 == null)
    {
        _row0 = new Gradient();
        _row0.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.35f, 0.28f, 0.28f), 0f),
                new GradientColorKey(Color.white, 0.55f),
                new GradientColorKey(Color.white, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            });
    }

    EditorGUILayout.LabelField("Ramp 行 0", EditorStyles.boldLabel);

    EditorGUI.BeginChangeCheck();
    _row0 = EditorGUILayout.GradientField(_row0);
    if (EditorGUI.EndChangeCheck())
        BakeRow0(rampMap);
}
```

在类里、`OnGUI` 外面加烘培：

```csharp
void BakeRow0(MaterialProperty rampMap)
{
    var tex = new Texture2D(256, 2, TextureFormat.RGBA32, false);
    tex.wrapMode = TextureWrapMode.Clamp;
    tex.filterMode = FilterMode.Point;
    tex.hideFlags = HideFlags.DontSave;

    var pixels = new Color[256 * 2];
    for (int x = 0; x < 256; x++)
    {
        Color c = _row0.Evaluate(x / 255f);
        pixels[x] = c;
        pixels[x + 256] = c;
    }

    tex.SetPixels(pixels);
    tex.Apply();
    rampMap.textureValue = tex;
}
```

**`_row0`**：GUI 上的记忆。还没存进材质，换选材质或重开 Unity 可能回到初始三条色。

**`SetKeys`**：第一次打开时给一条「左暗右亮」，和卡通 Ramp 习惯一样。两个数组分别是颜色锚点、透明度锚点。

**`GradientField`**：画出那条预览。点击弹出编辑器，`Location %` 就是 U（0=阴影，1=受光）。

**`BeginChangeCheck / EndChangeCheck`**：只有你改了渐变才烘。`OnGUI` 会不停刷新，没有这组判断会每帧 `new Texture2D`。

**`Evaluate(x/255f)`**：从左到右 256 个像素，对应 Shader 的 `rampU`。

**`textureValue = tex`**：写进 `_RampMap`。`foreach` 仍跳过它，所以面板上不再出现贴图槽，只剩色条。`DontSave`：先只活在内存里。

现在整张 Ramp 被这一条 256×2 顶掉，身体各材质行会暂时共用这一条，阴影色应跟着锚点变。通了下一步再拆成 10 行。

## 152. 你（Friday, Sep 11, 2026, 11:06 PM (UTC+8)）

这个有报错，你帮我看一下，顺便你先帮我把整个工具都写完吧，我现在看这个有点吃力；

### 助手

先看报错和你现在的 GUI 代码，再按现有 Body/Hair 的 10 行 Ramp 把整套工具补完。

### 助手

报错是三处：`_row0` 没声明、`StePixels`/`Alpha` 是 `SetPixels`/`Apply` 的拼写。Inspector 被掏空是因为 `foreach` 被注释掉了。接下来按你们 Shader 的 10 行约定把整套 Ramp 工具写完。

### 助手

你那几处报错是：

- `_row0` 写了但类里没有这个字段  
- `StePixels` → `SetPixels`  
- `Alpha()` → `Apply()`  
- `foreach` 被注释掉，所以面板上其它属性全没了  

工具已经按你们 Shader 的约定写完：**256×20、白天 5 行 + 夜晚 5 行、每行 2 像素**，V 的算法和现在的 `rampV` 一致。Body / Hair 都挂了这个 GUI。

**怎么用**

1. 选中身体或头发材质（必须是 Project 里的 `.mat`）。  
2. 点 **创建可编辑 Ramp（写进材质子资源）**。材质下会出现 `RampData`（存渐变）和 `RampTex`（给 GPU 采样）。  
3. 点某一行色条，弹出 Gradient 编辑器，加锚点、改颜色，角色会跟着变。  
4. 上面的 Day/Night 只决定**编辑哪 5 行**；场景里走哪一半看下面原来的 **Night Ramp** 开关。

没有点创建之前，还是用你拖的那张 Barbara png，不会一打开就把官方 Ramp 盖掉。脸没有 `_RampMap`，这个编辑器不会出现。

行数锁死 5+5，没有 +/- ，改行数会和 Shader 里的 `/ 10` 对不上。Ctrl+S 保存材质，渐变和贴图都在 `.mat` 里。

## 153. 你（Friday, Sep 11, 2026, 11:12 PM (UTC+8)）

好像有个报错

### 助手

报错是第 63 行：`GradientField` 这个重载不吃 `bool`，`true` 被当成了 `GUILayoutOption`。改成用 `EditorGUI.GradientField` 画 HDR 色条。

### 助手

Console 里是：

`Argument 2: cannot convert from 'bool' to 'GUILayoutOption'`

`EditorGUILayout.GradientField(渐变, true)` 在你这版 URP/编辑器 API 里没有「第二个参数是 HDR」这个重载，`true` 被当成布局选项，所以编不过。已经改成 `EditorGUI.GradientField(矩形, 渐变, true)`，点色条仍会弹出 HDR Gradient 编辑器。

回 Unity 等编译结束，材质面板应能正常打开。再点 **创建可编辑 Ramp** 即可。

## 154. 你（Friday, Sep 11, 2026, 11:17 PM (UTC+8)）

还是有报错

### 助手

这次是 `EditorGUI.GradientField` 的参数对不上：没有 `(矩形, 渐变, true)` 这一档，编译器把第二参数当成了标题字符串。

已改成带空标题的写法：

```csharp
EditorGUI.GradientField(rect, GUIContent.none, rows[i], true);
```

回 Unity 等编译。若 Console 还有红字，把完整那一行贴过来。

## 155. 你（Friday, Sep 11, 2026, 11:20 PM (UTC+8)）

Ok,没有红字了，但是还有两条警告；我现在材质球面板上没有原来的那个ramp了，但是角色身上还是有Ramp图的效果，这是为什么？

### 助手

两条警告是 Unity 6 把 `flags` 标成过时了。Ramp 槽被我们藏起来了，但材质上 `_RampMap` 仍指着原来那张图，Shader 照样在采，所以角色看起来没变。我先改掉警告，并把当前 Ramp 贴图槽重新显示出来。

### 助手

**角色还有 Ramp 效果，是因为槽藏了，赋值没清。**

Inspector 只是给人看的。`foreach` 里跳过了 `_RampMap`，所以面板上没有那一行。材质数据里 `_RampMap` 仍指向 Barbara 那张 png，片元着色器照样 `SAMPLE_TEXTURE2D(_RampMap, …)`。没画 ≠ 没贴图。

两条警告是 Unity 6 的过时 API：`prop.flags` / `PropFlags` 要改成 `propertyFlags` / `ShaderPropertyFlags`，已经换掉了。

面板顶部加回了 **当前采样的图**：没点创建之前，这里就是原来的 Ramp png；点创建并改色条之后，槽里会换成烘出来的 `RampTex`。

## 156. 你（Friday, Sep 11, 2026, 11:27 PM (UTC+8)）

有多了一个报错；ramp条我点了创建，之前老的ramp条就没有，这个符合预期；

## 157. 你

<available_subagent_types>
Available subagent_types and a quick description of what they do:
- generalPurpose: General-purpose agent for researching complex questions, searching for code, and executing multi-step tasks. Use when searching for a keyword or file and not confident you'll find the match quickly.
- explore: Fast agent specialized for exploring codebases. Use this when you need to quickly find files by patterns (eg. "src/components/**/*.tsx"), search code for keywords (eg. "API endpoints"), or answer questions about the codebase (eg. "how do API endpoints work?"). When calling this agent, specify the desired thoroughness level: "quick" for basic searches, "medium" for moderate exploration, or "very thorough" for comprehensive analysis across multiple locations and naming conventions.
- cursor-guide: Read Cursor product documentation to answer questions about how Cursor Desktop, IDE, CLI, Cloud Agents, Bugbot, and other features work. Use when the user asks 'In Cursor, how do I...?' or similar questions about Cursor products.
- ci-investigator: Investigate a single failing PR CI check and return a short root-cause summary. Use when the user asks to summarize, explain, diagnose, or investigate a specific failed check from a pull request.
- bugbot: Use only when the user *explicitly* asks for a Bugbot-like review of local code changes. When launching this subagent, set the Task description to exactly "Bugbot". Launch exactly one Bugbot subagent with `run_in_background: false` unless the user explicitly asks to run in background. Use this fixed prompt form: "Full Repository Path: ...\nDiff: <one of: \"branch changes\", \"uncommitted changes\", \"natural language\">\nChange Description: ...\nCustom Instructions: ..."; default to `Diff: branch changes`; include `Change Description` only when `Diff` is `natural language`, formatting it as one block per changed file (a `<path> (added|modified|deleted|renamed)` header followed by bullets of what changed, mentioning line numbers or ranges inline where helpful), and only use `natural language` as a last resort after a regular diff-based review failed because the diff could not be computed; include `Custom Instructions` only when the user gave specific review instructions. This subagent is single-shot and does not support `resume`; always launch a fresh subagent instead.
- security-review: Use only when the user *explicitly* asks for a security review of local code changes. When launching this subagent, set the Task description to exactly "Security Review". Launch exactly one security-review subagent with `run_in_background: false` unless the user explicitly asks to run in background. Use this fixed prompt form: "Full Repository Path: ...\nDiff: <one of: \"branch changes\", \"uncommitted changes\">\nCustom Instructions: ..."; default to `Diff: branch changes`; include `Custom Instructions` only when the user gave specific review instructions. This subagent is single-shot and does not support `resume`; always launch a fresh subagent instead.
- best-of-n-runner: Run a task in an isolated git worktree. Each best-of-n-runner gets its own branch and working directory. Use for best-of-N parallel attempts or isolated experiments.
</available_subagent_types>

<available_subagent_models>
If the user explicitly asks for the model of a subagent/task, you may ONLY use model slugs from this list:
- inherit (default; required unless the user explicitly requested another model)
- composer-2.5-fast
- cursor-grok-4.6-high-fast

If the user isn't asking for a specific version, prefer the latest version of the model family. As an example, if the user just says "gpt" or "claude", use the latest available version of GPT or Claude.

IMPORTANT: If the user requests a model that is NOT in the list above, do NOT substitute a different model or guess. Instead, skip launching the subagent with that model and tell the user which model was unavailable and which models are available.

When speaking to the USER about which model you selected for a subagent, do NOT use the kebab-case model names unless the user requested the model using that format. Ue the same naming scheme the user used to discuss the model when they requested it.
</available_subagent_models>

<dynamic_tools>
You have access to tools through dynamic namespaces, e.g. MCP servers, using `GetDynamicTools` and `CallDynamicTool`.

## Dynamic Tool Discovery and Invocation

Use `GetDynamicTools` to discover tool schemas, then `CallDynamicTool` to invoke one tool. Aim to minimize round-trips: ideally one discovery call followed by one invocation.

If the user mentions a product or service represented by an available namespace, and the request likely depends on it, proactively inspect that namespace before answering. If you are unsure which namespace matches, search with a relevant pattern.

`GetDynamicTools` supports these modes:

1. `{"namespace":"<id>"}`: returns schemas and full descriptions for every tool in that namespace.
2. `{"namespace":"<id>","toolName":"<name>"}`: returns one tool schema with its full description.
3. `{"pattern":"<regex>"}`: searches namespace and tool names.
4. `{"namespace":"<id>","pattern":"<regex>"}`: searches tools within one namespace.
5. No arguments: returns the full catalog.

Pattern-search and catalog results shorten long descriptions, marked by a trailing "... [truncated]"; namespace and single-tool lookups always return the complete description.

Always inspect a tool's schema before invoking it with `CallDynamicTool`.

If the available dynamic tools do not fully support what the user asked you to do, complete the work you can with the current tool set. In your work summary, include what you were unable to do and why. Do not use browser automation to work around missing tools unless the user explicitly asks you to use the browser.

Available dynamic tool namespaces:

<dynamic_tool_namespaces>
<namespace name="plugin-google-drive-google-drive" source="mcp" />
<namespace name="plugin-gmail-gmail" source="mcp" />
<namespace name="cursor" tools="AwaitShell, CreateGoal, Delete, EditNotebook, FetchMcpResource, GenerateImage, ReadLints, SwitchMode, Task, TodoWrite, UpdateGoal, WebFetch, WebSearch" namespaceUseInstructions="Native Cursor tools for this session. These are highly recommended and useful tools that you should use when the right situation arises. Don't be afraid to look at one if it seems relevant, even if you don't end up using it. You MUST read the tool schemas before calling them.

Here are some crucial instructions:
- AwaitShell: Use to sleep and check shell progress. Never sleep using shell.
- ReadLints: Check for linter errors after substantive edits.
- SwitchMode: Switch between modes: Agent and Plan. Proactively consider switching modes for relevant requests.
- TodoWrite: Use this tool to manage complex multi-step tasks." source="cursor" />
</dynamic_tool_namespaces>

If an MCP-backed namespace requires authentication, call `mcp_auth` through `CallDynamicTool` for that namespace, then inspect it again and retry if appropriate. Do not authenticate namespaces preemptively or repeatedly.
</dynamic_tools>

## 158. 你（Friday, Sep 11, 2026, 11:27 PM (UTC+8)）

有多了一个报错；ramp条我点了创建，之前老的ramp条就没有，这个符合预期；

### 助手

老 Ramp 槽被换成烘出来的图，**这是预期**。创建就是让 `_RampMap` 指向新的 `RampTex`，不再用 Barbara 那张 png。

多出来的报错是上一改漏了：`DrawRampEditor` 里写了 `materialEditor.TexturePropertySingleLine`，但这个函数拿不到 `OnGUI` 里的 `materialEditor`，会变成「当前上下文不存在该名称」。已经把编辑器当参数传进去了。

另外创建时不再在面板刷新途中 `ImportAsset`（容易把贴图变成不可读，`SetPixels` 再报错），改成点按钮后下一帧再写资源。

回 Unity 等编译。若材质下已经有 `RampData` / `RampTex`，色条应能改、角色会跟着变。老 png 不会自动回来，这是对的。

## 159. 你（Friday, Sep 11, 2026, 11:51 PM (UTC+8)）

OK；这个工具可以了；我想继续再完善角色界面关于背景的内容；之前的天空球蓝色的渐变我我已经做好了；但是我发现他还有一层类似这样的星光点，前面还有一层粒子系统动态的星光点；我截图给你的那个我能看出来类似一张不满星星点的图片，因为我截图给你的星点都是一起移动的，不像另外一层粒子系统发射的比较随机；我想知道的是，这层比较单一的星光点，也是做成天空球还是怎么做？

### 助手

先看你现在的天空球和场景是怎么搭的，再对照原神试玩界面那层一起跟着动的星点该怎么做。

### 助手

判断对了：截图里那些一起平移的点，是贴在「无限远」上的一张星点图，不是粒子。做法和你已经做好的渐变天空球是同一类，不要做成粒子。

角色界面通常是三层叠在一起：

| 层 | 是什么 | 为什么动起来不一样 |
|---|---|---|
| 最远 | 蓝色渐变天空球 | 你已经做好了 |
| 中间 | 稀疏星点贴图（黑底白点，Additive） | 整张图跟摄像机一起转，点与点的相对位置不变 |
| 最近 | 粒子星光 | 各自闪、各自飘，有远近差，所以看起来随机 |

星点层「一起动」，是因为它们没有视差：每个点都钉在同一个方向上。摄像机一转，整张星图跟着转。粒子会出生、死亡、近大远小，所以不会整片平移。

**推荐：还是天空球，不要单独做 Cubemap。**

更干净的两种接法：

1. **同一颗天空球、同一套 shader 多贴一张星图**  
   渐变还是现在的颜色，再 `viewDir`（或球 UV）去采一张黑底星点图，Additive 叠上去。一个 mesh，亮度、滚动、地平线淡出都能独立调。

2. **再套一颗稍小一点的球**  
   材质只画星点，`Queue` 比渐变靠前，`ZWrite Off`，Blend Additive。和现在的球完全解耦，改起来最省事。

Cubemap 更适合你地板已经在采的那层环境色，不适合这种「几颗亮点」。星很稀，六张面又浪费又容易在接缝处露馅。原神大世界夜空也是球 + 平铺 2D 星图，不是 Cubemap；天顶那圈同心圆，就是球 UV 在极点挤在一起。

贴图怎么做：

- 黑底 + 少量偏青白的点，**不要带蓝天**，不然会和渐变叠两次蓝。
- 导入关 sRGB 或当 Mask 用都行，shader 里再乘颜色和强度。
- 采样用 Clamp 或 Repeat 看你要不要平铺。平铺省图，但天顶更容易出现那圈「假天空」花纹；展示场景里这其实很原神。

和粒子的分界：

- 星点层：无限远，几乎不闪，最多整球慢慢转，或 shader 里用时间让整张图一起呼吸。
- 粒子层：挂在角色/相机附近，Random Between Two Constants，有寿命、闪烁、轻微飘。

地板反射：星点如果只在主相机的天空球上，反射相机不一定看得到。要让地面也有这层星，要么星点画在场景球上并让反射相机渲染它，要么把星点写进地板正在采的那张 Cubemap。粒子云之前说的一样，想进倒影就要进反射相机的层。

先做第 1 或第 2 种天空球星点即可；粒子那层可以后加，互不干扰。

## 160. 你（Friday, Sep 11, 2026, 11:57 PM (UTC+8)）

应该是一张黑底+星光点的偏白色的图片，我然后我新建一个很大的半球体，把这张图放到半球体上，然后让这个球体慢慢旋转；还有一个功能，球体旋转的时候，贴图上我绘制的偏白色的星点，会慢慢变色，这个应该是需要shader支持对吧；

### 助手

对，星点本身用黑底 + 偏白点的图贴在大半球上、再慢慢转，这条路是对的。**旋转只会让点在天上挪位置，不会让已经画好的白色点自己变色**；要变色，必须 shader 在采样之后乘一个随时间（或随旋转角度）变化的颜色。

分层可以这样理解：

1. **贴图**：只负责「哪里有星」。黑 = 没有，白/灰 = 有多亮。黑不要带蓝，后面用 Additive，黑会完全隐掉，露出你已经做好的渐变天空。
2. **半球**：只负责「星在无限远的哪些方向上」。转 Transform（或 shader 里滚 UV）= 整张星图一起转，这就是你看到的「所有点一起移动」。
3. **Shader**：负责混合、地平线淡出，以及你说的变色。

变色常见两种，别混：

| 你想要的 | 要不要额外逻辑 |
|---|---|
| 图上有的星本来就是青/白/暖白，一转只是换一批进画面 | 贴图里画好就行，shader 只采样 |
| 同一颗画出的白点，转的过程中慢慢从白变青再变一点品红 | 必须 shader：采样后乘/lerp 一个 tint |

第二种可以跟旋转绑在一起，例如用转过的角度或 `_Time` 做 `lerp(_TintA, _TintB, 0.5 + 0.5 * sin(...))`。想每颗星不同步变色，用这颗星的 UV 当相位：`sin(_Time + uv.x * 10 + uv.y * 7)`，看起来会像有的偏青、有的偏白，而不是整片一起换色。

半球注意这几件：

- 人在球里面：`Cull Front`，`ZWrite Off`，Queue 比渐变天空靠前一点。
- Blend 用 Additive（`Src One, Dst One`），黑底才不会盖住蓝天。
- 地平线附近用 `viewDir.y` 把星淡掉，不然半球切口会露一圈星点。
- 球跟着相机走（或足够大），否则一平移会穿帮。转的是绕 Y，速度很慢即可；也可以不转 mesh，只在 shader 里 `uv.x += _Time.y * _RotateSpeed`，球就能钉在相机上。

贴图当亮度遮罩用就够了，颜色全部交给 shader 的 tint，以后调色不用重画图。

你要是准备写这层 shader，可以说一下：变色是整片一起换，还是每颗星错开换。我按你的选择列要写的属性和片元顺序。

## 161. 你（Saturday, Sep 12, 2026, 3:40 PM (UTC+8)）

OK, 我已经准备好了星星的贴图和模型，现在开始写shader，还是按照之前的方法，你说原理和实现思路，代码呈现，我来跟你说的方案和代码写代码，也是按照一个个功能来；

### 助手

先对齐你现有 shader 的写法，再只讲第一块：能把星点贴图正确画在半球内侧。

### 助手

第一块只做一件事：**人在半球里面，能看见星点贴图，黑底不挡后面的蓝天，星点也不要画到角色脸上。** 旋转、变色、地平线淡出下一轮再加。

原理：

- 这层不参与光照，Unlit 即可。
- 相机在球内，看到的是内壁，所以 `Cull Front`（剔外留内）。默认 `Cull Back` 会把你正在看的那一面剔掉，球会消失。
- `ZWrite Off`：不写深度，避免和渐变天空球抢 Z、也避免挡住后面可能要画的透明东西。
- `ZTest LEqual`：角色、地板已经写过深度，星点在更远的地方，那些像素测不过，星就不会盖在芭芭拉脸上。
- `Blend One One`（Additive）：输出纯黑 = 加 0，蓝天还在；白点 = 往上加亮。所以贴图必须是黑底星点，不要带蓝。
- `Queue = Transparent`：等不透明的角色和地板画完再画星，深度测试才有东西可比。

半球要比渐变天空球**略小一圈**（比如 0.98 倍），否则两球贴在一起会闪。材质把星图拖到 `_StarMap`。

把下面整份建成例如 `Genshin_Stars.shader`：

```hlsl
Shader "RenderingLab/Genshin/Stars"
{
    Properties
    {
        _StarMap("Star Map", 2D) = "black" {}
        _StarColor("Star Color", Color) = (1, 1, 1, 1)
        _StarIntensity("Star Intensity", Range(0, 5)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "Stars"
            Tags { "LightMode" = "UniversalForward" }

            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend One One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _StarMap_ST;
                half4 _StarColor;
                half _StarIntensity;
            CBUFFER_END

            TEXTURE2D(_StarMap);
            SAMPLER(sampler_StarMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings o = (Varyings)0;
                o.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                o.uv = TRANSFORM_TEX(IN.uv, _StarMap);
                return o;
            }

            half4 frag(Varyings IN) : SV_TARGET
            {
                half3 star = SAMPLE_TEXTURE2D(_StarMap, sampler_StarMap, IN.uv).rgb;
                half3 col = star * _StarColor.rgb * _StarIntensity;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
```

片元里 `star` 是亮度遮罩：黑是 0，点是 >0，再乘颜色和强度。Additive 下 alpha 不参与混合，写 `1` 即可。

贴图导入：Wrap 用 Repeat 或 Clamp 看 UV 是否平铺；Filter 用 Bilinear；不要当 Normal。黑底星点当颜色图即可。

写好后场景里应能透过球看到蓝天，星点叠在天上，转摄像机星跟着球的 UV 走，角色身上没有星。若整球看不见，先查是不是还在 `Cull Back`；若星挡住蓝天，查是不是没开 Additive，或贴图底不是纯黑。

这一块过了告诉我画面怎样，下一块做绕 Y 慢慢转 UV。

## 162. 你（Saturday, Sep 12, 2026, 5:21 PM (UTC+8)）

功能正常实现了，但是我新增的这个星星的半球体，现在也反射到地板的Plane里了，但是他是跟随着角色的平面反射的参数走的，这个星星的半球体的反射是不是要再Plane上额外做一个参数设置？

### 助手

先看地板的平面反射是怎么采图、星星半球有没有被那台反射相机画进去。

### 助手

**不要给星星单独加一套平面反射参数。** 它不该走角色那条平面反射，所以才会跟 `_FadeDistance`、`_ReflectionTint` 绑在一起。

地板现在是两层叠的：

1. **天空**：`reflect(-V, N)` 采 Cubemap，靠 Fresnel 铺满整块地板。
2. **角色**：反射相机画进 `_PlanarReflectionTex`，再用中心衰减 `fade` 叠上去。

反射相机只排除了地板本身，场景里别的都会画进去。星星半球是一张很大的透明网格，于是进了这张 RT。地板片元又不区分「这是角色还是天」，整张图共用：

```hlsl
mask = saturate(planar.a) * fade * _ReflectionIntensity;
col = lerp(col, planar.rgb * _ReflectionTint.rgb, mask);
```

星 shader 还写了 `alpha = 1`，连黑底像素在 RT 里也是不透明。结果是：半球盖住的那一片倒影，全部按角色倒影的淡出/染色走，远处没有角色的地方星也会跟着 fade 消失。这是角色倒影的逻辑，不是天空该有的样子。

星星是无限远的天，倒影应该像 Cubemap 那样铺在整块地板上，靠 Fresnel 在贴角处更亮，**不该**围着角色做一个圆斑。平面反射是给近处、有视差的东西用的（角色、以后的近景粒子）。给星再加一组 Plane 参数，还是同一张 RT，仍然分不开「角色像素」和「星像素」。

正确拆法：

| 东西 | 怎么进地板 |
|---|---|
| 角色 | 继续平面反射 + 现在的 fade/tint |
| 渐变天空、星半球 | **不要进反射相机**（放到单独 Layer，culling mask 去掉它） |
| 地板上的星 | 跟 Cubemap 走同一条天空，不要跟角色 fade |

地板上要不要看见旋转的星，是下一步才需要选的：

- 倒影里不要星：排除 Layer 就结束，Cubemap 保持现在的蓝天。
- 倒影里要星、且跟着上面那层一起转：以后在地板 shader 里用同一个反射向量 `R` 再采星图（和 Cubemap 一样是「方向」，不是平面 RT）。那是天空参数，不是再做一套平面反射。

先把星（以及渐变天空球，如果也进了倒影）挪到例如 `Sky` 层，反射相机别画这一层。排除之后，角色倒影应恢复成只有人，星只出现在真天空里。这一步做完告诉我地板上还要不要星，再决定要不要在 Floor 里加星图采样。

## 163. 你（Saturday, Sep 12, 2026, 5:26 PM (UTC+8)）

我已经把星半球加入到SkyStar层了，但是地板还是有星半球的反射；

### 助手

主相机还要看见星，`CopyFrom` 会把主相机的层遮罩原样拷给反射相机，所以只改物体 Layer 不够。给反射相机加一层排除。

### 助手

只改物体 Layer 不够。反射相机每帧 `CopyFrom` 主相机，**主相机还要看见星，遮罩就会原样拷过去**，星半球照样会画进地板那张 RT。

已经在 `PlanarReflection` 里，拷完之后把天空层从反射遮罩里扣掉。层名正好是 `SkyStar` 的会自动排除。编译后选中地板，看倒影里是不是只剩角色。

若还在：确认星半球 Inspector 上改的是 **Layer**，不是 Tag；渐变天空球如果也进了倒影，把它和 `SkyStar` 一起勾进地板组件上新出现的 **Exclude Layers**。主相机 Culling Mask 仍要包含这一层，天上的星才会在。

## 164. 你（Saturday, Sep 12, 2026, 5:28 PM (UTC+8)）

现在星半球层没有了；skybox是怎么反射到地板上的？应该和角色的反射不一样吧？

### 助手

对，**和角色倒影不是同一条路**。星半球从平面反射里拿掉之后，地板上剩的那层天，走的是 Cubemap，不是反射相机。

角色：再架一台镜像相机，把附近的网格画进 `_PlanarReflectionTex`，屏幕 UV 采回来，再乘中心 `fade`。有视差、有远近，所以人会倒过来，也会在脚边浓、远处淡。

天空：每个地板像素只算一个方向，**没有第二台相机**。

```hlsl
float3 V = GetWorldSpaceNormalizeViewDir(IN.positionWS);
float3 N = normalize(IN.normalWS);          // 地板朝上
float3 R = reflect(-V, N);                  // 视线打到地板后弹向哪里
half3 sky = SampleSkyCubemap(R, _SkyMip);  // 用这个方向去采天
half fresnel = pow(1 - saturate(dot(N, V)), _SkyFresnel);
col = lerp(_FloorColor, sky, fresnel);
```

`R` 只表示「朝天上哪个方向看」，Cubemap 是无限远的环境，所以：

- 没有角色那种脚边圆斑，整块地板都能看到天
- 贴得越斜（掠射），Fresnel 越大，天越清楚；往下看更像地板颜色
- `_SkyMip` 把天弄糊，像湿地面，不是平面反射那套 3×3 `_Blur`
- 星半球、渐变天空球是场景网格，进的是反射相机；Cubemap 是贴图，和 Layer 无关

你现在地板采的是材质上的 `_SkyCube`（开了 Use Custom Cubemap），一般就是场景那张 `CubeMap_BG01`，**不是**星半球那张 2D 图。所以排除 `SkyStar` 之后：天上仍有星（主相机在画网格），地板倒影里没有星，只剩 Cubemap 里的蓝。这是预期。

以后若要地板上也有会转的星，是在这段 Cubemap 采样旁边，用同一个 `R` 再采星图，仍然走天空这条，不要走 `_PlanarReflectionTex`。

## 165. 你（Saturday, Sep 12, 2026, 5:30 PM (UTC+8)）

但是我还的需求是，星半球也需要像skybox一样，反射到地板上；

### 助手

这条要加在**地板 shader**里，不是星半球、也不是平面反射。和 Cubemap 用同一个弹射方向 `R`，把星点加进「天」里，再走现在的 Fresnel。

### 助手

星半球继续 **不要** 进反射相机。地板上的星和 Cubemap 是同一条：用已经算好的 `R` 当方向，把方向收成和半球一样的球 UV，采同一张星图，**加在 `sky` 上**，然后再 Fresnel。这样星会铺满地板、贴角更亮，不会跟角色的 fade 走。

半球 shader 用的是网格 UV；地板没有这张网格，所以要用方向自己生成 UV（经纬）：

- `u`：绕 Y 转一圈 → `atan2(R.x, R.z)`
- `v`：从天顶到地平 → `asin(R.y)`

和 Unity 默认球的 UV 同一类。若倒影里的星和天上左右反了或缝错了，再改 `atan2` 两个分量或 `u = 1 - u`。

在 `Genshin_Floor.shader` 里加这些（星材质用同一张图、同一套颜色/强度；Yaw 先填半球 Transform 的 Y，没转就 0）：

**Properties**（放在 Sky Cubemap 那段下面）：

```hlsl
[Header(Star Field)]
_StarMap("Star Map", 2D) = "black" {}
_StarColor("Star Color", Color) = (1, 1, 1, 1)
_StarIntensity("Star Intensity", Range(0, 5)) = 1
_StarYaw("Star Yaw (deg)", Float) = 0
```

**CBUFFER** 里补：

```hlsl
float4 _StarMap_ST;
half4 _StarColor;
half _StarIntensity;
float _StarYaw;
```

Cubemap 那两行下面加：

```hlsl
TEXTURE2D(_StarMap);
SAMPLER(sampler_StarMap);
```

`SampleSkyCubemap` 上面加两个函数：

```hlsl
float3 RotateYaw(float3 dir, float yawDeg)
{
    float rad = yawDeg * PI / 180.0;
    float s, c;
    sincos(rad, s, c);
    return float3(dir.x * c - dir.z * s, dir.y, dir.x * s + dir.z * c);
}

float2 DirToStarUV(float3 dir)
{
    dir = normalize(dir);
    float u = atan2(dir.x, dir.z) / (2.0 * PI) + 0.5;
    float v = asin(clamp(dir.y, -1.0, 1.0)) / PI + 0.5;
    return TRANSFORM_TEX(float2(u, v), _StarMap);
}
```

`frag` 里在采完 Cubemap 之后、算 fresnel **之前** 加上星（和半球一样 Additive）：

```hlsl
half3 sky = SampleSkyCubemap(R, _SkyMip) * _SkyTint.rgb * _SkyIntensity;

float3 starDir = RotateYaw(R, _StarYaw);
float2 starUV = DirToStarUV(starDir);
half3 star = SAMPLE_TEXTURE2D(_StarMap, sampler_StarMap, starUV).rgb;
sky += star * _StarColor.rgb * _StarIntensity * saturate(starDir.y);

half fresnel = pow(saturate(1.0 - saturate(dot(N, V))), _SkyFresnel);
half3 col = lerp(_FloorColor.rgb, sky, fresnel);
```

`saturate(starDir.y)` 让地平线以下不采星。角色那段 `SamplePlanar` 不要动。

材质上把星图拖进新槽。对完之后：天上的星仍来自半球网格；地板上的星来自这段 `R` 采样，会跟 Cubemap 一起受 Fresnel，不受 `_FadeDistance` 影响。

若左右反、接缝对不上，或 Tiling 和半球不一致，把现象说一下再改 UV。转半球时先把 `_StarYaw` 调成同一个 Y，两边才会一起转。

## 166. 你（Saturday, Sep 12, 2026, 5:42 PM (UTC+8)）

地板上已经能看到反射的星贴图了，但是这个不是真的反射我场景里Sky_Star_Mesh这个半球体，等于说如果我让这个半球体渲染或则UV流动，地板上的星并不会动；

### 助手

地板现在是自己按方向采星图，和场景里的半球网格没有共用旋转/UV。先看你两份 shader 现在怎么采，再把它们绑到同一套参数上。

### 助手

判断对了：地板现在是自己按方向采同一张图，和 `Sky_Star_Mesh` 无关，所以转网格或滚 UV 都不会带动倒影。

要让倒影跟半球是同一个物体，用**第二张平面反射 RT，只画 `SkyStar`**，合成时走天空 Fresnel，不走角色 fade。星 shader 以后加旋转、UV 流动、变色，地板会一起变。正在加这张天空倒影 RT。

### 助手

判断没错：地板那套 `R → UV → 采星图` 只是**仿一张天**，并不画 `Sky_Star_Mesh`。网格旋转、UV 流动都发生在半球材质上，地板不知情。

已经改了反射脚本：同一台镜像相机画两张 RT。

| RT | 画什么 | 地板怎么用 |
|---|---|---|
| `_PlanarReflectionTex` | 角色 | 原来的 fade / tint |
| `_PlanarSkyTex` | 只画 `SkyStar` 层的半球 | **加进 `sky`，走 Fresnel**，不乘 fade |

半球 shader 里以后做的旋转、滚 UV、变色，都会进第二张 RT，地板跟着动。

请你改 `Genshin_Floor.shader`：

1. **删掉**地板上整段 Star Field（`_StarMap`、`_StarYaw`、`RotateYaw`、`DirToStarUV` 和 frag 里那三行采星）。否则会叠两套星。
2. 和 `_PlanarReflectionTex` 一样声明全局图（不用进 Properties）：

```hlsl
TEXTURE2D(_PlanarSkyTex);
SAMPLER(sampler_PlanarSkyTex);
```

3. `frag` 里先算屏幕 UV，Cubemap 之后、Fresnel 之前把半球倒影 **加** 上去（只用 rgb，不要 `planar.a`，不要 fade）：

```hlsl
half3 sky = SampleSkyCubemap(R, _SkyMip) * _SkyTint.rgb * _SkyIntensity;

float2 uv = GetNormalizedScreenSpaceUV(IN.positionCS);
half3 starRefl = SAMPLE_TEXTURE2D(_PlanarSkyTex, sampler_PlanarSkyTex, uv).rgb;
sky += starRefl;

half fresnel = pow(saturate(1.0 - saturate(dot(N, V))), _SkyFresnel);
half3 col = lerp(_FloorColor.rgb, sky, fresnel);
```

角色那段 `SamplePlanar(uv)` 保持原样，`uv` 已经算过就别再算一次。

编译后：转 `Sky_Star_Mesh` 或以后给星 shader 加 UV 流动，地板上的星应一起动；角色倒影仍是脚边那圈 fade。若天空倒影是黑的，看半球 Layer 是不是正好叫 `SkyStar`。

## 167. 你（Saturday, Sep 12, 2026, 5:50 PM (UTC+8)）

最终的Floorshader列一下；

### 助手

你现在这份还不完整：`starRefl` 采了但没加进 `sky`，`col` 没定义，`RotateYaw` / `DirToStarUV` 还在却已经没有 `_StarMap`。用下面这一份整文件替换。

```hlsl
Shader "RenderingLab/Genshin/Floor"
{
    Properties
    {
        _FloorColor("Floor Color", Color) = (0.02, 0.05, 0.14, 1)

        [Header(Sky Cubemap)]
        [Toggle(_USE_SKYCUBE)] _UseSkyCube("Use Custom Cubemap", Float) = 0
        [NoScaleOffset] _SkyCube("Sky Cubemap", Cube) = "" {}
        _SkyTint("Sky Tint", Color) = (1, 1, 1, 1)
        _SkyIntensity("Sky Intensity", Range(0, 2)) = 1
        _SkyMip("Sky Blur (Mip)", Range(0, 8)) = 4
        _SkyFresnel("Sky Fresnel", Range(0.5, 8)) = 4

        [Header(Character Planar)]
        _ReflectionTint("Reflection Tint", Color) = (0.55, 0.7, 0.95, 1)
        _ReflectionIntensity("Reflection Intensity", Range(0, 1)) = 0.65
        _Blur("Blur", Range(0, 8)) = 1.5
        _FadeDistance("Fade Distance", Range(0.1, 20)) = 3.5
        _FadePower("Fade Power", Range(0.2, 8)) = 1.8
        _FadeCenter("Fade Center (XZ)", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            Name "Floor"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _USE_SKYCUBE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _FloorColor;
                half4 _SkyTint;
                half _SkyIntensity;
                float _SkyMip;
                float _SkyFresnel;
                half4 _ReflectionTint;
                half _ReflectionIntensity;
                float _Blur;
                float _FadeDistance;
                float _FadePower;
                float4 _FadeCenter;
            CBUFFER_END

            TEXTURE2D(_PlanarReflectionTex);
            SAMPLER(sampler_PlanarReflectionTex);
            float4 _PlanarReflectionTex_TexelSize;

            TEXTURE2D(_PlanarSkyTex);
            SAMPLER(sampler_PlanarSkyTex);

            TEXTURECUBE(_SkyCube);
            SAMPLER(sampler_SkyCube);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings o = (Varyings)0;
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                o.positionCS = pos.positionCS;
                o.positionWS = pos.positionWS;
                o.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return o;
            }

            half3 SampleSkyCubemap(float3 R, float mip)
            {
                #if defined(_USE_SKYCUBE)
                    return SAMPLE_TEXTURECUBE_LOD(_SkyCube, sampler_SkyCube, R, mip).rgb;
                #else
                    half4 encoded = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, R, mip);
                    return DecodeHDREnvironment(encoded, unity_SpecCube0_HDR);
                #endif
            }

            half4 SamplePlanar(float2 uv)
            {
                float2 texel = _PlanarReflectionTex_TexelSize.xy * _Blur;
                half4 sum = 0;
                [unroll]
                for (int y = -1; y <= 1; y++)
                {
                    [unroll]
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 offset = float2(x, y) * texel;
                        sum += SAMPLE_TEXTURE2D(_PlanarReflectionTex, sampler_PlanarReflectionTex, uv + offset);
                    }
                }
                return sum / 9.0;
            }

            half4 frag(Varyings IN) : SV_TARGET
            {
                float3 N = normalize(IN.normalWS);
                float3 V = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                float3 R = reflect(-V, N);

                half3 sky = SampleSkyCubemap(R, _SkyMip) * _SkyTint.rgb * _SkyIntensity;

                float2 uv = GetNormalizedScreenSpaceUV(IN.positionCS);
                half3 starRefl = SAMPLE_TEXTURE2D(_PlanarSkyTex, sampler_PlanarSkyTex, uv).rgb;
                sky += starRefl;

                half fresnel = pow(saturate(1.0 - saturate(dot(N, V))), _SkyFresnel);
                half3 col = lerp(_FloorColor.rgb, sky, fresnel);

                half4 planar = SamplePlanar(uv);
                float2 delta = IN.positionWS.xz - _FadeCenter.xz;
                float fade = saturate(1.0 - length(delta) / max(_FadeDistance, 1e-4));
                fade = pow(fade, _FadePower);

                half mask = saturate(planar.a) * fade * _ReflectionIntensity;
                col = lerp(col, planar.rgb * _ReflectionTint.rgb, mask);
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
```

片元顺序是：Cubemap → 加上 `_PlanarSkyTex`（星半球倒影）→ Fresnel 混进地板 → 再叠角色平面反射。`_PlanarSkyTex` / `_PlanarReflectionTex` 都是脚本设的全局图，不用在材质上拖槽。

## 168. 你（Saturday, Sep 12, 2026, 5:54 PM (UTC+8)）

我现在想让地板反射的星半球和角色一样，可以调整模糊度；

### 助手

角色那套模糊是 3×3 盒式模糊，星半球现在是单次采样，所以没法调。加上独立的 `_StarBlur`，算法和角色相同，强度分开调。反射 RT 同分辨率，还要把天空图的 texel 大小设成全局，和角色那张一样。

### 助手

原理和角色倒影相同：邻域 3×3 平均，步长 = 纹素大小 × 模糊强度。星用单独的 `_StarBlur`，不要和角色的 `_Blur` 绑在一起，这样天上可以更糊、人还可以保持稍清楚。

脚本已经会写 `_PlanarSkyTex_TexelSize`。你在 Floor 里改这几处：

**Properties**，放在 Sky Cubemap 那段：

```hlsl
_StarBlur("Star Blur", Range(0, 8)) = 1.5
```

**CBUFFER** 里加：

```hlsl
float _StarBlur;
```

纹理声明处加（和角色那张 texel 一样）：

```hlsl
float4 _PlanarSkyTex_TexelSize;
```

把 `SamplePlanar` 下面加上星的同款函数：

```hlsl
half3 SamplePlanarSky(float2 uv)
{
    float2 texel = _PlanarSkyTex_TexelSize.xy * _StarBlur;
    half3 sum = 0;
    [unroll]
    for (int y = -1; y <= 1; y++)
    {
        [unroll]
        for (int x = -1; x <= 1; x++)
        {
            float2 offset = float2(x, y) * texel;
            sum += SAMPLE_TEXTURE2D(_PlanarSkyTex, sampler_PlanarSkyTex, uv + offset).rgb;
        }
    }
    return sum / 9.0;
}
```

**frag** 里把单次采样换成：

```hlsl
half3 starRefl = SamplePlanarSky(uv);
sky += starRefl;
```

`_StarBlur = 0` 时 9 个采样点叠在一起，等于不糊；加大后星点在地板上会散开。角色 `_Blur` 不用动。

## 169. 你（Saturday, Sep 12, 2026, 6:01 PM (UTC+8)）

OK，功能实现了； 地板Plane模型上挂的脚本上有Exclude Layers和Sky Reflection Layers这个层的功能是什么？

### 助手

这两个都是 **只给反射相机用的层开关**，主相机不受影响。天上的星该看见还是会看见。

反射每帧画两张图：

| 字段 | 进哪张 RT | 地板怎么用 |
|---|---|---|
| **Exclude Layers** | **不**画进 `_PlanarReflectionTex`（角色倒影） | 角色那段 fade / tint |
| **Sky Reflection Layers** | **只**画进 `_PlanarSkyTex`（天空倒影） | `sky +=` 再走 Fresnel |

`CopyFrom` 会把主相机的 Culling Mask 原样拷过来，主相机必须包含 `SkyStar` 你才能看见天。所以要在拷完之后再拆：角色图扣掉天空，天空图只留天空。

**Exclude Layers**：额外从角色倒影里拿掉的层。渐变天空球、以后不想跟角色 fade 走的东西勾在这里。脚本还会自动扣掉 `SkyStar` 和 Sky Reflection Layers 里的层，所以星半球即使你忘了勾 Exclude，也不会进角色那张图。

**Sky Reflection Layers**：谁算「天空倒影」。星半球、以后要跟着转/糊在整块地板上的云，勾在这里。图层名正好是 `SkyStar` 时会自动并进去，所以你现在留空，星倒影也在。

以后加东西可以按「近处物体 vs 无限远的天」来勾：

- 角色、武器、近处粒子 → 哪边都不勾，走角色倒影
- 星半球、天空球 → Sky Reflection Layers（不要进 Exclude 也行，脚本会从角色图里扣掉天空层）
- 只要主相机看见、地板完全不要倒影 → 只勾 Exclude，Sky 也不勾

主相机 Culling Mask 仍然要包含这些层，否则真天空也会消失。

## 170. 你（Saturday, Sep 12, 2026, 6:03 PM (UTC+8)）

意思是通过这两个层的选项，可以实现地板上不反射skybox和 Sky_Star_Mesh？

### 助手

**星半球可以靠这两层关掉倒影；Unity 的 Skybox 不行**，它根本不走这两张 RT。

`Sky_Star_Mesh` 是场景里的网格，有 Layer。不进 `_PlanarSkyTex`、也不进角色那张 RT，地板上就不会有这层星。注意：脚本里图层名是 `SkyStar` 时会**自动**加进天空倒影，所以 Sky Reflection Layers 留空，星倒影仍在。真要地板上没星，需要把物体换到别的层、或改脚本别自动包含 `SkyStar`，并且不要勾进 Sky Reflection Layers。Exclude 只保证它不进**角色**倒影。

**Skybox**（场景里那张 Cubemap / Camera Skybox）不是带 Layer 的物体。反射相机清屏是纯透明，不会把 Skybox 画进 RT。地板上的天来自 Floor shader 里这段：

```hlsl
sky = SampleSkyCubemap(R) ...
col = lerp(_FloorColor, sky, fresnel)
```

要关掉，是材质上把 `_SkyIntensity` 打到 0，或关掉 Use Custom Cubemap，**不是** Exclude / Sky Reflection Layers。

如果渐变天也是你放的那颗**天空球网格**，那和星半球一样，可以用层从倒影里拿掉；那也不是 Skybox 资源本身。

## 171. 你（Saturday, Sep 12, 2026, 6:05 PM (UTC+8)）

关于地面反射角色，Sky_Star_Mesh和Skybox这一章节的内容我不是太了解；可以先把原理详细的讲解一下吗？

### 助手

地面上你看见的「倒影」其实是 **三种完全不同的东西叠在一起**，只是最后都画在同一块 Plane 上。分清楚「无限远的天」和「近处的人」，后面那两个 Layer 开关就好懂了。

## 1. 镜子在算什么

眼睛看向地板上某一点，光线打到镜面后弹开，沿着反射方向 `R` 再去「看」世界：

```text
相机 → 地板像素 P → 弹射方向 R = reflect(-V, N)
```

`V` 是这一点指向相机，`N` 是地板朝上。  
**倒影要像什么，取决于 R 指向的东西有多远：**

| 东西 | 远近 | 用方向 R 够不够 |
|---|---|---|
| Skybox / Cubemap | 无限远，没有远近差 | 够。只问「朝哪看」，不问「在哪」 |
| `Sky_Star_Mesh` | 很大的半球，近似无限远 | 方向采样能骗；要跟网格一起转/滚 UV，就得真的去画这张网格 |
| 角色 | 就在地板附近，有视差 | 不够。必须像照镜子一样再拍一遍 |

所以不是「地板有一种反射」，而是三种采样，最后在片元里按顺序混。

## 2. Skybox：没有物体，只有方向

Unity 的 Skybox 不是场景里的 Mesh，没有 Layer。它是相机清屏时，用视线方向去采一张 Cubemap。

地板 **不经过反射相机**，自己在 shader 里做同一件事：

```text
R = reflect(-V, 地板法线)
sky = Cubemap.Sample(R)     // 你材质上的 _SkyCube
col = lerp(地板色, sky, fresnel)
```

Fresnel：越斜着看地板（掠射），`dot(N,V)` 越小，天越清楚；往下看更像地板本色。这就是湿地面「天映在地上」的那一层。

要点：

- 反射相机清的是透明色，**根本不画 Skybox**。
- Exclude Layers / Sky Reflection Layers **管不到** 它。
- 关掉它只能调 Floor 的 `_SkyIntensity`，或不要采 Cubemap。
- Cubemap 不会跟着 `Sky_Star_Mesh` 转，那是另一套资源。

你主相机天上看到的蓝，和地板上 Cubemap 那层蓝，可以是同一张图，但是 **两条独立的采样**。

## 3. 角色：再架一台镜子相机

角色有厚度、有脚和头，相机一转，倒影里的相对位置会变。只拿一个方向去采贴图，人会「贴死在天上」，没有近大远小。

做法：把主相机绕地板平面翻过去，再 `Camera.Render()` 到 `_PlanarReflectionTex`。

```text
主相机在地板上方
    ↓ 镜像
反射相机在地板下方（倒着看）
    ↓ 画出角色
RT：屏幕空间的一张倒影照片
    ↓ 地板用屏幕 UV 去采
乘 fade（脚边浓、远处淡）、乘 tint，叠到 col 上
```

这就是「真·平面反射」：拍到网格怎么转、怎么播动画，倒影就怎么变。

代价：主相机看见的东西，`CopyFrom` 之后反射相机默认也会画。星半球如果也在里面，就会和角色 **共用** fade——星不该围着人做一个圆斑。所以才把层拆开。

## 4. Sky_Star_Mesh：网格天，近似无限远

星是场景里的半球 Mesh，Additive 画在真天空上。主相机能看见它，是因为它在画面里被画出来了。

它和 Skybox 的差别：Skybox 是贴图方向查找；星是 **物体**，有 Transform、有 UV、有自己的 shader。你转半球或滚 UV，只有「去画这个 Mesh」的路径才会跟着动。

地板上要它，有过两条路：

**A. 用 R 再采同一张星图**（之前那版）  
和 Skybox 同类：便宜，但和半球物体无关，转 Mesh 倒影不动。

**B. 现在：第二张平面反射 RT**  
同一台镜像相机再画一次，Culling Mask **只留** 星那一层，画到 `_PlanarSkyTex`。地板：

```text
sky = Cubemap(R)
sky += _PlanarSkyTex（屏幕 UV，可模糊）
col = lerp(地板色, sky, fresnel)    // 星跟天走 Fresnel
然后再叠角色倒影（fade）
```

这样倒影里的星就是 `Sky_Star_Mesh` 本人：转、UV 流动、以后变色都会跟上。  
合成却走 **天空**（Fresnel、铺满地板），不走角色 fade。

## 5. 三层在地板上怎么叠

你现在的片元顺序就是这张图：

```text
① Cubemap(R) + Fresnel     → Skybox 那层天（无限远，无网格）
② += 星半球的平面 RT        → Sky_Star_Mesh（真网格，当天空叠）
③ lerp 角色平面 RT × fade   → 芭芭拉（近处，脚边一圈）
```

①② 先合成「整块地板上的天」，③ 再把人贴在这层天上。人会挡住/盖住一部分天，这和真镜子一样。

## 6. 那两个 Layer 在切什么

它们 **只改反射相机画谁**，不改主相机。

```text
主相机：什么都画（人 + 星 + Skybox 清屏）
                │
                ├─ 角色 RT：画人，扣掉天空层     → Exclude + 自动去掉 SkyStar
                └─ 天空 RT：只画星半球           → Sky Reflection Layers
```

- 地板上不要 **星网格** 的倒影：别让它进天空 RT（现在名字叫 `SkyStar` 会自动进，要关得换层或改脚本）。
- 地板上不要 **Skybox** 的天：Layer 无效，去调 Cubemap / `_SkyIntensity`。
- 地板上不要 **角色**：那是角色 RT 的 intensity / fade，也不是 Skybox。

一句话：**Skybox 是方向查表；角色是近处再拍一张；星半球是物体，你选择把它当「天」再拍一张，而不是塞进角色那张图。**

## 172. 你（Saturday, Sep 12, 2026, 6:10 PM (UTC+8)）

OK，那先这样，具体原理之后再研究；现在已有的功能都很好的实现了；接下来继续实现Sky_Star_Mesh的自动旋转；

### 助手

这一块只做 **绕 Y 慢慢转**。不要转 Mesh 的 Transform：半球以后多半要钉在相机上，转物体会和跟随打架。球 UV 的 `u` 本来就是绕 Y 一圈，所以在 shader 里 `uv.x += 时间 × 速度`，看起来就是整片星在转。地板采的是半球本人的倒影 RT，UV 一滚，地上的星会一起动。

`_Time.y` 是秒。`u` 从 0 到 1 等于转 360°，所以速度用「每秒滚多少圈」：`0.01` 大约 100 秒转一圈，适合天空。贴图 Wrap 用 **Repeat**，否则 `u` 超出 0–1 星会卡住或消失。

在 `Genshin_Star.shader` 里加：

**Properties**

```hlsl
_RotateSpeed("Rotate Speed (turns/sec)", Float) = 0.01
```

**CBUFFER**（和 `_StarIntensity` 放一起）

```hlsl
float _RotateSpeed;
```

**vert** 里，`TRANSFORM_TEX` 之后：

```hlsl
o.uv = TRANSFORM_TEX(IN.uv, _StarMap);
o.uv.x += _Time.y * _RotateSpeed;
```

片元不用改。进 Play 才能看到 `_Time` 在走（编辑模式默认不推进）。速度太大先改小；方向反了就给 `_RotateSpeed` 写负数。

转起来并且地板倒影跟着动了，再说下一块：星点随时间变色。

## 173. 你（Saturday, Sep 12, 2026, 6:13 PM (UTC+8)）

OK, 接下来实现旋转的过程中白色星点变色，可以从材质球面板上选择在两个色相之间过度吧；

### 助手

星图仍当亮度遮罩（黑=没有，白=有多亮），颜色全部用两个 Tint 去乘。时间让它们在 A、B 之间来回；用已经在滚的 `uv.x` 当相位，这样不是整片一起闪，有的偏 A、有的偏 B，旋转时看起来会换色。地板倒影会跟着走，不用改 Floor。

中间不要走 RGB 的脏灰色：两个颜色都选亮的（青白、粉白都可以）。`_HueSpeed` 是每秒来回大约几轮，`0.2` 比较慢。

把原来的 `_StarColor` 换成下面两个（强度还用 `_StarIntensity`）：

**Properties**

```hlsl
_TintA("Tint A", Color) = (0.55, 0.85, 1, 1)
_TintB("Tint B", Color) = (1, 0.75, 0.95, 1)
_HueSpeed("Hue Speed", Float) = 0.2
```

**CBUFFER** 里删掉 `_StarColor`，改成：

```hlsl
half4 _TintA;
half4 _TintB;
float _HueSpeed;
```

**frag**

```hlsl
half4 frag(Varyings IN) : SV_TARGET
{
    half3 star = SAMPLE_TEXTURE2D(_StarMap, sampler_StarMap, IN.uv).rgb;

    float t = 0.5 + 0.5 * sin(_Time.y * _HueSpeed * 2.0 * PI + IN.uv.x * 2.0 * PI);
    half3 tint = lerp(_TintA.rgb, _TintB.rgb, t);

    half3 col = star * tint * _StarIntensity;
    return half4(col, 1);
}
```

`sin` 在 -1～1，收成 0～1 再 `lerp`。`IN.uv.x` 已经含旋转，每颗星相位不同。Play 模式下改两个颜色就能看过渡；觉得整片还是太齐，把 `_HueSpeed` 再降一点。

## 174. 你（Saturday, Sep 12, 2026, 9:41 PM (UTC+8)）

我在场景中加了粒子烟雾，现在粒子烟雾和地面有硬的穿插，这是要做软粒子是吧，软粒子是要在地板的shader上做 还是在粒子的shader上做呢？

### 助手

先确认你现在的粒子用的是哪套 shader，以及场景有没有开深度图——软粒子要靠这两样。

### 助手

对，这就是软粒子要解决的问题。**做在粒子 shader 上，不要改地板。**

地板是不透明的，先画、先写深度。烟雾是透明的，后画，每个片元拿自己的深度去和已经写进深度缓冲的地板比：比地板更远的像素直接被丢掉，所以交界是一条硬线。地板不知道后面会有粒子，没法替烟雾做淡出。

软粒子是粒子片元自己干这件事：采场景深度图，用「地板深度 − 粒子深度」当距离，距离接近 0 时把 alpha（或 Additive 的亮度）收掉，看起来像溶进地面。

```text
场景深度（地板已经写好） − 粒子自己的深度
        ↓
距离很小 → 烟雾变透明
距离够大 → 烟雾保持原样
```

你现在的 `FX_Cloud` 用的就是 URP Particles，材质里已经有 Soft Particles，只是关着（`_SoftParticlesEnabled: 0`）。在粒子材质上打开 **Soft Particles**，把 Near / Far 调到大约贴地那几米的厚度。URP Asset 和相机要开 **Depth Texture**，否则采不到场景深度，开关等于没开。

这和地板倒影不是一条路：软粒子只修「烟雾切地面」的硬边。若还要烟雾进倒影，那是反射相机的 Layer，以后再说。

## 175. 你（Saturday, Sep 12, 2026, 9:44 PM (UTC+8)）

粒子的材质球上没有看到soft particles选项

### 助手

选项在，只是 **不在 Surface Options 里**。URP Particles/Unlit 把 Soft Particles 画在 **Advanced Options** 里，而且必须 `_ZWrite` 为 0（你已经是 Transparent，这个是满足的）。

你截图里 Advanced 只看到黄字 `MaterialPropertyBlock is used...`，那是粒子系统改了材质块的提示。**Soft Particles 和 Flip-Book Blending 应该在这行黄字上面。** 先确认 Advanced 三角是展开的，往上看有没有 `Flip-Book Blending` / `Soft Particles`。

如果还是没有，多半是 Unity 6 材质面板把「额外属性」藏了：

1. 在 **Project** 窗口点选 `FX_Cloud` 材质（不要只点场景里粒子组件上的材质槽）。
2. Inspector 右上角 **⋮** → 打开 **Debug**，找到 `_SoftParticlesEnabled`，改成 **1**。
3. 再切回 Normal，Advanced 里应出现 **Surface Fade** 的 Near / Far。Near `0`、Far `1`～`2` 先试，穿插会变软。
4. URP Asset（以及场景相机）打开 **Depth Texture**，不开软粒子没有深度可采，等于没开。

黄字可以忽略，粒子系统本来就会用 MaterialPropertyBlock。改完进 Play 看贴地那条硬边是否化开。

## 176. 你（Saturday, Sep 12, 2026, 9:57 PM (UTC+8)）

是这么吗？但是我调参数，没有起作用，要么就是粒子不显示了

### 助手

开关找对了。粒子消失或看起来没变化，是 **Near / Far 的含义和数值** 的问题，不是没生效。

这两项不是模糊半径，而是「离不透明表面多远才开始显形」，单位是米：

```text
粒子相对地板（或角色）的深度差
        │
 < Near  → 完全透明（等于看不见）
 Near～Far → 慢慢显现
 ≥ Far   → 完全不透明
```

你现在是 **Near 0.69、Far 20.7**：离地面不到 0.69m 的烟雾全没了；要离任何实心物体 20m 才完全看见。角色展示场景里烟雾就贴在地板上，整片都会被收掉，所以你会觉得「一调就没了」。Far 特别大时，贴地那一层永远在淡出区间里，硬边好像还在、又像没调。

先改成：

- **Near = 0**
- **Far = 0.5～1.5**（软边大概半米到一米厚）

Near 必须小于 Far。Far 只控制贴地那一条交界有多宽，不要开到几十。

它会贴着 **所有写了深度的东西** 淡，不只是地板。烟雾擦过芭芭拉身体也会变淡，这是正常的。

若 Near=0、Far=1 仍然整片消失：当前 Quality 的 URP Asset 要开 **Depth Texture**（`PCLow` / `MobileLow` 是关的）。相机 Depth Texture 不要设成 Disabled。以前 Depth Priming = Forced 也会搞坏深度，软粒子会跟着整片没。

## 177. 你（Saturday, Sep 12, 2026, 10:01 PM (UTC+8)）

那我应该怎么设置才能保留角色的深度边缘光，同时软粒子能起效果？

### 助手

深度边缘光和软粒子都采场景深度图，先看角色 rim 和 URP/相机现在怎么拿深度，再对一下会互相踩的设置。

### 助手

两套效果都采 `_CameraDepthTexture`，**可以同时开**，不是二选一。之前角色 rim 出事，是 Depth Priming = Forced 把人裁掉了，和软粒子不是同一件事。

按这个配：

**1. URP Asset（当前 Quality 用的那份，不要用 PCLow / MobileLow）**
- Depth Texture：**开**
- Opaque Texture：可关，软粒子不用它

**2. URP Renderer（和上面那份 Asset 绑在一起的）**
- Depth Priming Mode：**Disabled**（不要 Forced，人会消失）
- Depth Texture Mode / Copy Depth：**After Opaques**

你这边 PCHigh 的 Copy Depth 现在是 **After Transparents**。深度要等透明画完才拷，烟雾在透明阶段采的是空的或过期的深度，结果就是：一开软粒子整片没，或完全看不出淡出。改成 After Opaques 之后：

- 不透明（地板、角色）画完 → 拷一份深度  
- 角色 rim 用的是更早的 depth prepass（Forward+ 本来就有），边缘光还在  
- 软粒子用这份「刚拷好的不透明深度」，贴地才会溶

**3. 场景相机**
- Depth Texture：**Use Pipeline Settings**（你现在就是 2，不用改成 Off）

**4. 粒子材质**
- Soft Particles：开  
- Near **0**，Far **0.5～1.5**

不要为了 rim 去开 Forced Priming，也不要为了粒子把相机 Depth 关掉。深度图开着、拷贝放在不透明之后，两边共用同一张图、只是采样时机不同。改完 Renderer 进 Play 看：人身上 rim 还在，烟只在贴地/贴人那一圈变软。

## 178. 你（Saturday, Sep 12, 2026, 10:05 PM (UTC+8)）

我发现只有把Depth Texture mode改成after Opaques之后软粒子起作用了，但是角色上的深度边缘光没有了

### 助手

After Opaques 时角色前向阶段还采不到当帧深度，所以 rim 会没。接着核对角色 shader 有没有 DepthOnly，以及 Renderer 用 Force Prepass 能不能让两边同时采到深度。

### 助手

这是 **同一张深度图、两个采样时机对不上**，不是 rim 坏了。

角色边缘光写在 **不透明前向** 里，片元当时就要 `SampleSceneDepth`。  
After Opaques 是 **角色已经画完** 才把深度拷进 `_CameraDepthTexture`。所以：

| Copy Depth | 角色着色时有没有当帧深度 | 粒子（透明）时有没有 |
|---|---|---|
| After Transparents | 有时有（Forward+ 预通道），rim 还在 | 拷贝还没发生，软粒子失效 |
| After Opaques | 还没有，rim 没了 | 已经拷好，软粒子有效 |
| **Force Prepass** | 颜色之前先画一遍 DepthOnly | 透明也能采这张预通道 |

两边要同时有，把 Renderer 的 **Depth Texture Mode 改成 Force Prepass**。Depth Priming 继续 **Disabled**，不要 Forced。

Force Prepass 只画带 `LightMode = DepthOnly` 的物体。Body / Hair / Face 已经有这个 Pass，地板 `Genshin_Floor` **没有**。不补的话：人 rim 会回来，烟贴角色会软，**贴地仍是硬切**。

在 `Genshin_Floor.shader` 里，主 Pass 的 `ENDHLSL` / `}` 后面加（和 Body 同一套）：

```hlsl
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            struct DepthAttributes
            {
                float4 positionOS : POSITION;
            };

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            DepthVaryings DepthOnlyVertex(DepthAttributes IN)
            {
                DepthVaryings o = (Varyings)0;
                o.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return o;
            }

            half4 DepthOnlyFragment(DepthVaryings IN) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
```

顶点里我写成了 `(Varyings)0`，你这边改成 `(DepthVaryings)0`，不要拷错。更稳妥直接用：

```hlsl
DepthVaryings o = (DepthVaryings)0;
o.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
```

地板 shader 要 `#include` 已经有的 `Core.hlsl`（主 Pass 里有即可，这个 Pass 也要 include 一次，或放在 SubShader 的 HLSLINCLUDE 里）。Floor 现在 include 只在主 Pass 里，**DepthOnly 的 HLSLPROGRAM 里也要加那行 Core.hlsl**。

改完：Force Prepass + 地板 DepthOnly + 粒子 Near 0 / Far 0.5～1.5。 rim 用预通道里「自己和旁边」的深度差，粒子用同一张图里的地板和角色。

## 179. 你（Saturday, Sep 12, 2026, 10:09 PM (UTC+8)）

OK，可以了； 现在我想让这个粒子云也能反射到地板里，应该怎么做？

### 助手

粒子进倒影取决于层进哪张 RT，以及地板是用 alpha 还是 rgb 去叠。先看反射脚本和粒子材质现在怎么画。

### 助手

粒子云要进倒影，走 **角色那张平面 RT**，不要放进 `SkyStar`。

星那张天空 RT **只画透明、不画地板和角色**，深度是空的。你云材质开了软粒子，进那张图会被当成「贴着物体」整片 fade 掉，倒影里等于没有云。角色 RT 里有人（地板会关掉以免套娃），软粒子才有深度可采。

已经给反射相机打开了 Depth Texture，软粒子在倒影里才不会被空深度收掉。

你这边场景这样放：

1. 粒子物体 Layer 用 **Default**（或任何不在 Sky Reflection / Exclude 里的层）。
2. 不要勾进 **Sky Reflection Layers**，也不要进 **Exclude Layers**。
3. 材质保持 **Transparent + Alpha**（你现在的 Surface Blending 就是）。地板用 `planar.a` 当遮罩，Alpha 混合才能留下 alpha；如果改成 Additive `One One`，alpha 经常是 0，人倒影在、云仍没有。
4. Color Mode Additive 可以留着，那只是粒子颜色怎么乘贴图，不是混合模式。

Floor shader 不用改：云会和角色一起进 `_PlanarReflectionTex`，再走同一套 fade / 模糊。贴地的烟会倒在脚边那一圈里，这是预期。

若你要的是天上那层大云、希望像星一样铺满整块地板、走 Fresnel：把那套粒子的 **Soft Particles 关掉**，再放进 Sky 层。开着软粒子的云不能进只有透明的天空 RT。

编译后进 Play：倒影里应能看到烟；没有的话先看 Layer 是不是误进了 `SkyStar`。

## 180. 你（Saturday, Sep 12, 2026, 10:13 PM (UTC+8)）

粒子云层好像没有进地板反射

### 助手

先查粒子在场景里的 Layer、反射相机的裁剪，以及地板是不是用 alpha 把云丢掉了。

### 助手

云其实已经在 **FX 层** 上，反射相机也会画这一层。没进地板是另外两件事把它弄没了：

1. **镜像相机会 `invertCulling`**。角色网格必须翻面，粒子广告牌还是 `Cull Back`，正面被剔掉，倒影里云直接没了。
2. **软粒子在反射相机里**。反射时地板是关掉的，深度不对；你材质 Far 还是 17.68，贴着人/地的烟会被 fade 成全透明。主相机看起来正常，倒影里没有。

已经在反射那一帧临时把 FX 粒子改成 **双面 + 关掉软粒子**，画完立刻改回去，主相机软粒子不受影响。地板遮罩也改成 `max(alpha, 亮度)`，避免粒子 alpha 偏低时被 `planar.a` 丢掉。

编译后进 Play 看倒影。粒子物体保持 **FX** 层，不要放进 SkyStar。主相机软粒子仍建议 Near 0、Far 0.5～1.5，17 会把贴地的烟收得很狠。

## 181. 你（Saturday, Sep 12, 2026, 10:18 PM (UTC+8)）

好像还是没有反射出来

### 助手

云在 `(-5, 2.92, 12.14)`，离角色 fade 中心大约 13m，而地板 `_FadeDistance` 只有 3.5m。即便画进角色 RT，合成时也会被 fade 乘没。粒子云应走星那条路：单独一张 RT，整块地板 Fresnel，并且 **不要 invertCulling**（广告牌会被剔掉）。

### 助手

上次没出来，主要不是层没画上，是 **合成时被角色倒影的 fade 乘没了**。

粒子在大约 `(-5, 3, 12)`，离角色中心十多米。地板角色倒影只用 `_FadeDistance`（你现在是 3.5m）那一圈，圈外即使 RT 里有云，`fade` 也是 0。星能看见是因为走另一张图，不乘这个 fade。

这次改成和星同类：

- 单独一张 `_PlanarFxTex`，只画 **FX** 层
- **不翻面剔除**（广告牌再 `invertCulling` 会被剔光）
- 反射那一帧关掉软粒子，画完恢复
- 地板用 `sky +=` 叠上去，走 Fresnel，铺整块地，不再跟脚边 fade

粒子物体保持 **FX** 层。编译后进 Play，倒影里的云会像天上的星一样铺在地板上，贴角更明显。

## 182. 你（Sunday, Sep 13, 2026, 12:11 PM (UTC+8)）

OK, 接下来我还想做一个功能，我在YuanShenChara的场景里后面会放好几个角色，我想通过键盘的左右箭头键来回切换不同的角色；比如说，按左键角色A向左移动一段距离以后淡出，同时角色B从右边淡入；你先分析一下这个功能怎么做，分析出思路以后，我们再来一步步来实现功能，先不用帮我写代码

### 助手

先看场景里角色现在怎么摆、有没有现成的切换脚本，再按你的左右切换+淡入淡出需求把思路拆开。

### 助手

这个功能本质是 **舞台中央只展示一个角色，左右切页时两人同时在滑、同时在隐**。相机、灯、地板都不用动。先把模型想清楚，再谈淡出怎么接到你现在这套不透明角色 shader 上。

## 舞台怎么摆

场景里每个角色一个根节点（芭芭拉、胡桃…），挂在一个切换器的列表里，顺序就是左右键的顺序。

平时：

- 只有 **当前角色** 在舞台中心 `(0, 站姿)`，`SetActive(true)`，Idle 照常播。
- 其他人 `SetActive(false)`，省 Skinning、倒影、脸朝向。

不要用切相机、也不要所有人叠在原点靠透明度硬切，那样倒影和深度边缘光会搅在一起。

## 一次切换在干什么

以 **右箭头** 为例（看下一位）：

| | 退场的 A | 登场的 B |
|---|---|---|
| 起始 | 中心，完全可见 | 在右侧屏外（或刚入画），完全透明 |
| 过程 | 向左滑 + 淡出 | 从右滑向中心 + 淡入 |
| 结束 | 关掉 | 停在中心，完全可见，成为「当前」 |

**左箭头** 只是方向对调：A 向右退，B 从左边进。列表到头就循环。

动画期间 **丢掉新的按键**（或只记一次），避免连按把坐标拧乱。时长大约 0.4～0.8 秒，位置和透明度用同一条 0～1 曲线（Ease In Out）。

## 为什么淡出不能只改材质颜色的 Alpha

Body / Hair / Face / Outline 都是 **不透明 Queue**。改 `_BaseColor.a` 不会让人变透明，深度、描边、深度边缘光也还在。

三条路：

1. **Dither / clip（推荐）**  
   shader 里用屏幕格子或噪声，`clip` 掉一部分像素。不透明管线不用改 Queue，描边、深度、平面倒影都会跟着「变稀」。用 MaterialPropertyBlock 写 `_Fade`（1 全显示，0 全 clip），不要改共享材质，否则所有用这张 mat 的角色一起淡。

2. **Dissolve 贴图**  
   更花，适合技能消失，切换角色略重。

3. **改成 Transparent**  
   排序、描边、深度 rim、倒影 alpha 全要重做，不适合现在这套。

所以淡入淡出是 **角色 shader 加一个 `_Fade` + 切换脚本改 MPB**，不是只写 C# 位移。

Body / Hair / Face / Outline 四个 Pass 都要读同一个 `_Fade`，否则身体淡了描边还在。

## 和现有系统怎么相处

- **FaceHeadOrientation**：继续挂在每个角色自己根上，关掉的人本来就不跑。
- **平面倒影**：反射相机会把激活的网格画进去。切换那一瞬间 A、B 都激活，倒影里两人一起滑，这是对的。地板 `_FadeCenter` 仍对准舞台中心，人滑出 3.5m 后倒影自己变淡，和角色淡出是两回事，可以并存。
- **深度边缘光**：dither 造成的镂空边缘会多一点 rim，一般能接受；不要为了切换去改 Depth Texture。
- **Animator**：退场/登场过程保持 Idle，切完再 `SetActive(false)` 退场的那个，避免 T-pose 闪一下。

## 建议的拆步（之后按这个写）

1. **只切人、不播动画**：左右键改 `currentIndex`，只激活一个根节点。确认列表、循环、输入。
2. **只滑位置**：按你说的左右位移，先不做透明。确认距离（大约 2～4 米，能滑出镜头即可）。
3. **shader `_Fade` + dither clip**：四个角色 Pass 加上，脚本对当前两人写 MPB。
4. **收尾**：切换中锁输入、切完关掉退场角色、可选把倒影 `_FadeCenter` 仍钉在舞台。

第一步不碰 shader，你可以先把几个角色放进场景列好。准备好了就说，从「列表 + 左右键只显示一个」开始写。

## 183. 你（Monday, Sep 14, 2026, 1:15 AM (UTC+8)）

腿上金属这种大面积的高光是怎么实现的？

### 助手

先对照你 Body 里金属/Matcap 现在怎么写的，再对着截图讲这种大面积腿部高光是哪一层。

### 助手

截图里膝盖金片、腿甲那一块发白的大高光，**不是 Blinn 那种小亮点**，是 **MatCap（球体贴图）铺在金属区域上**。你 Body 里已经在采 `_MetalMap`，差的是怎么合成、以及这张图本身长什么样。

## 为什么会「一大片」

Blinn：`pow(NdotH, 很高)`，只有法线几乎对准半程向量的地方亮，所以是小斑。

MatCap：用 **视角空间法线** 去采一张「金属球的照片」：

```hlsl
float2 matcapUV = TransformWorldToViewDir(N, true).xy * 0.5 + 0.5;
half3 matcap = SAMPLE_TEXTURE2D(_MetalMap, sampler_MetalMap, matcapUV).rgb;
```

膝盖金饰是一块比较平滑的凸面，邻近像素的 `N` 在相机里差不多，就会采到 MatCap 上 **同一块亮区**。亮区有多大，完全取决于这张图：原神的 MetalMap 中间/偏上是一条很宽的亮带，不是一个白点，所以整块膝甲会一起亮。转相机时，高光贴着屏幕走，像棚拍反光，不像太阳在模型上爬。

`ilm.r` 只回答 **哪里是金属**（金边、菱形甲），不负责高光形状。形状全在 `_MetalMap` 上。

## 两层金属（和你之前的分工）

| | 谁 | 看起来像什么 |
|---|---|---|
| 大面积反光 | MatCap × `ilm.r`（或 `step` 出硬遮罩） | 膝甲、金饰那片白 |
| 更尖的一点 | `ilm.r` 很高时再加 Blinn | 边缘上的碎高光 |

截图那种「整块金子在发光」，走第一层。第二层可有可无，不要用 Blinn 去撑面积。

合成一般是 **叠在 ramp 上**，不要让 MatCap 把整个人换成球光照：

```text
base   = albedo × ramp × 灯
metal  = albedo × matcap × 金属遮罩 × 强度
final  = base + metal
```

乘 `albedo` 金子才是金的；MatCap 多半是灰度亮暗。你现在 `matcapCol = matcap * matcapMask` 再 `return` 遮罩，看到的是黑白块，不是腿上那层油亮。

## 图和遮罩

- **MetalMap**：一张金属球，中间大亮、四周暗。Bilinear，当颜色图。若是平白或噪声，不会出现「一整片高光」。
- **LightMap.R**：金饰涂高（接近 1），布料/皮肤接近 0。`step` 会整块开/关；用连续的 `ilm.r` 当权重，灰金属可以半亮。
- 高光再乘一点 `(0.5 + 0.5 * rampU)` 也可以，背光里金属稍暗，但原神展示里常常背光也亮，因为 MatCap 跟灯方向关系不大。

转一圈角色：金饰上的亮斑应 **跟着相机** 在表面滑动，而不是钉在 UV 上。若亮斑画在贴图里跟着腿的 UV 走，那就不是 MatCap，是 albedo 画死的。

你 shader 里 MatCap UV 这行是对的；下一步是去掉 debug `return`，改成 `base + albedo * matcap * mask`，并把真正的金属球图拖进 `_MetalMap`。若高光还是碎、或整身都在闪，把 `ilm.r` 和 MetalMap 的画面说一下就能对。

## 184. 你（Monday, Sep 14, 2026, 1:21 AM (UTC+8)）

那帮我看一下我的body的shader该怎么优化；

### 助手

先把 Body 现在的片元整段读完，对照 MatCap 金属和高光该怎么叠。

### 助手

核心问题不在 MatCap UV，而在 **把球体贴图塞进了 Blinn 的标量高光里**。膝甲那种大面积反光会被乘上 `_SpecColor`、丢掉 albedo 的金色，布料高光也和金属抢同一个 `step`。

你现在是：

```hlsl
half3 matcapCol = matcap * matcapMask; // 没乘 albedo
spec += matcapCol;                     // half3 加进 float，等于只用了 .x
specCol = spec * _SpecColor * _SpecIntensity * light;
```

应拆成两层，最后都加到 `finalColor` 上。

把高光那段换成下面（`NdotH` 算完之后、depth rim 之前）。`normalVS` 给 MatCap 和 rim 共用，少做一次视角法线。

```hlsl
float3 V = GetWorldSpaceNormalizeViewDir(IN.positionWS);
float3 H = normalize(L + V);
float NdotH = saturate(dot(N, H));
float3 normalVS = TransformWorldToViewDir(N, true);

float2 matcapUV = normalVS.xy * 0.5 + 0.5;
half3 matcap = SAMPLE_TEXTURE2D(_MetalMap, sampler_MetalMap, matcapUV).rgb;
half metalMask = saturate(ilm.r);
half3 matcapCol = albedo.rgb * matcap * metalMask;

half hardMetal = step(_MetalThreshold, ilm.r);
float spec = pow(NdotH, lerp(8.0, 128.0, ilm.b));
spec *= ilm.b * (1.0 - metalMask) * rampU;
spec += pow(NdotH, 64.0) * hardMetal * rampU;
half3 specCol = spec * _SpecColor.rgb * _SpecIntensity * mainLight.color;
```

`finalColor` 改成：

```hlsl
half3 finalColor = albedo.rgb * ramp * mainLight.color + specCol + matcapCol + rimCol;
finalColor += emission;
```

rim 里删掉再算一遍的 `TransformWorldToViewDir`，直接用上面的 `normalVS`。

对照：

| | 你现在 | 改完 |
|---|---|---|
| 膝甲大高光 | 当 Blinn 强度，丢金色 | `albedo × MetalMap × ilm.r` 直接加 |
| 布料微高光 | 和金属同一个 `step`，容易全开或全关 | `(1 - metalMask) × ilm.b` |
| 硬金属闪点 | 和 MatCap 糊在一起 | `ilm.r` 过 `_MetalThreshold` 再加一粒 Blinn |
| Debug | 默认 `_DebugView = 9` 是屏幕 UV | 看最终图把 Debug View 调成 **FinalColor** |

MatCap 用连续的 `ilm.r` 当权重，不要 `step(_MatCapTHreshold, …)`，灰金属才能半亮；硬切留给 `_MetalThreshold` 那粒 Blinn。`_MatCapTHreshold` 可以先不用。

`_MetalMap` 必须是金属球（中间大亮带），白图不会出现那片油光。Debug 选 **ilmR** 确认金饰是白的、皮肤是黑的。

改完膝甲应跟着相机滑一大片金亮，布料只有暗部以外的软高光。若整身发白，是 `ilm.r` 涂太满；若金饰完全不亮，查 LightMap R 和 MetalMap 球图。

## 185. 你（Monday, Sep 14, 2026, 1:49 AM (UTC+8)）

第一张图是现在shader里的效果，第二张图是原色原版的效果，感觉金属暗部不够暗，高光也没有形成大面积的高光；

### 助手

两张图的差别不是 UV 写错了，是 **金属还在用卡通 ramp 打底，MatCap 只是往上加了一层**。底已经是中等金色，暗部抬不下去，亮部也跳不出来，膝甲就不会变成「中间一大块高光、四周暗金」。

你现在的 MetalMap 其实是对的：中间一条很宽的亮带，两边灰、边缘近黑。膝甲正对相机时整块应采到亮带（大面积高光），金边法线更侧，应采到暗环。

但合成是：

```text
final = 金色albedo × ramp  +  albedo × matcap
         ↑ 暗部也有一份金     ↑ 再加一点
```

MatCap 再暗，ramp 那份金还在，所以第一张图金饰几乎一样亮。原神是 **金属区用 MatCap 当光照，换掉 ramp**：

```text
布料 = albedo × ramp × 灯
金属 = albedo × matcap          ← 暗就是暗，亮就是一大片
final = lerp(布料, 金属, ilm.r)
```

把这段：

```hlsl
half3 matcapCol = matcap * matcapMask * albedo.rgb;
...
half3 finalColor = albedo.rgb * ramp * mainLight.color + specCol + matcapCol + rimCol;
```

换成：

```hlsl
half metalMask = saturate(ilm.r);
half3 matcap = SAMPLE_TEXTURE2D(_MetalMap, sampler_MetalMap, matcapUV).rgb;
matcap = pow(matcap, _MatCapContrast);

half3 baseCol = albedo.rgb * ramp * mainLight.color;
half3 metalCol = albedo.rgb * matcap * _MatCapIntensity;
half3 lit = lerp(baseCol, metalCol, metalMask);

half3 finalColor = lit + specCol + rimCol;
finalColor += emission;
```

Properties 里加：

```hlsl
_MatCapIntensity("MatCap Intensity", Range(0, 4)) = 1.5
_MatCapContrast("MatCap Contrast", Range(0.5, 3)) = 1.4
```

CBUFFER 同步加上。Blinn 那段不用改，继续只给非金属和硬金属闪点。

贴图：Wrap 改成 **Clamp**（现在是 Repeat，边缘会卷到亮带）；关掉 Mipmap，否则侧看金属发灰。Debug 先看 **ilmR**：金饰应接近白，皮肤近黑。若金饰 R 只有 0.3，lerp 仍会掺大量 ramp，对比依然平。

改完膝甲正对镜头应整块偏亮黄白，金框凹处明显更暗；转相机时亮斑在甲面上滑动。若亮斑在脚底一侧，把 `matcapUV.y = 1.0 - matcapUV.y` 试一下。

## 186. 你（Monday, Sep 14, 2026, 1:54 AM (UTC+8)）

原来的NdotH高光不要了吗？

### 助手

**要留。** 去掉的只是「把 MatCap 加进 `spec` 那个标量」；`NdotH` 的 Blinn 还在，只是别再当膝甲那一大片高光用。

两层分工：

| | 用什么 | 给谁 |
|---|---|---|
| 大面积金属 | MatCap，**换掉** 金属区的 ramp | 膝甲、金饰 |
| 小高光 | `pow(NdotH, …)` | 布料微闪 + 金属上更尖的一点 |

片元里仍是先算 Blinn，再和 MatCap 合成：

```hlsl
// 布料：非金属才走 Blinn
float spec = pow(NdotH, lerp(8.0, 128.0, ilm.b));
spec *= ilm.b * (1.0 - metalMask) * rampU;

// 硬金属额外一粒（ilm.r 很高才有）
spec += pow(NdotH, 64.0) * hardMetal * rampU;
half3 specCol = spec * _SpecColor.rgb * _SpecIntensity * mainLight.color;

half3 baseCol = albedo.rgb * ramp * mainLight.color;
half3 metalCol = albedo.rgb * matcap * _MatCapIntensity;
half3 lit = lerp(baseCol, metalCol, metalMask);

half3 finalColor = lit + specCol + rimCol;
```

`lerp` 只动 **漫反射那一层**（ramp vs 球体贴图）。`specCol` 仍然加在最后，所以丝绸高光和金属碎光都在。膝甲的大亮斑来自 MatCap，不要再用 `NdotH` 去撑面积。

## 187. 你（Monday, Sep 14, 2026, 1:55 AM (UTC+8)）

给一个完整的shader吧，现在修改得有点乱了

### 助手

先对一下 Outline、GUI 还依赖哪些属性，再给你一份能直接整文件替换的 Body。

### 助手

整份替换 `Genshin_Body.shader`。MatCap 用 lerp 换掉金属区的 ramp，Blinn 仍加在最后。

### 助手

已经整份写进 `Genshin_Body.shader`，编译即可。材质上会多出 **MatCap Intensity**（先 1.5）和 **MatCap Contrast**（先 1.4）。Debug View 默认是 **FinalColor**。

金属区用 MatCap **换掉** ramp；`NdotH` 仍加在最后：布料走 `ilm.b`，`ilm.r` 超过 Hard Metal Threshold（默认 0.9）再加一粒硬闪。

`_MetalMap` 用那张中间亮带的球图，Wrap 设 Clamp、关掉 Mipmap。金饰仍平、不够暗，把 Contrast 提到 1.6～2，或 Debug 看 **ilmR** 金饰是不是够白。

## 188. 你（Monday, Sep 14, 2026, 2:01 AM (UTC+8)）

高光是不是还是要乘以ramp，要不然灯光转到ramp的暗部的时候，高光还是亮的？

### 助手

Blinn **已经乘了** `rampU`，灯转到阴影里碎高光会跟着灭。你看到「暗部还是亮」，是 **MatCap**：它按视角法线采球图，本来就不吃主光方向，所以会换掉 ramp，灯转过去膝甲仍然亮。

若希望金属也跟卡通阴影走，只压 MatCap，不要动 `metalMask`（否则暗部金子会变回布料 ramp）。

```hlsl
half3 metalCol = albedo * matcap * _MatCapIntensity * lerp(0.3, 1.0, rampU);
half3 lit = lerp(baseCol, metalCol, metalMask);
```

`0.3` 是阴影里还留一点金属形状，不会全黑。要更跟灯，把 `0.3` 降到 `0.1`；要更像原神展示（背光金饰仍亮），升到 `0.5～0.8` 或干脆不乘。

`finalColor = lit + specCol + …` 不用改：`specCol` 里已经有 `* rampU`。`foldShadow` 已经进了 `rampU`，衣缝里的金属也会一起暗。

## 189. 你（Tuesday, Sep 15, 2026, 12:16 AM (UTC+8)）

裙子里面灰紫色的是开了双面渲染吗？ 我确定这个裙子是单面的，外面是有贴图的，里面是一个灰紫色；这个实现的逻辑是什么？不用给我写代码

### 助手

先看芭芭拉裙子网格有几个材质槽、法线朝哪，再对照截图里内侧灰紫是怎么来的。

### 助手

**不是双面渲染。** 单面 `Cull Back` 的一张三角，从背面看会直接被剔掉，你不可能看见「里面一块灰紫」。能看见内侧，是因为模型里 **另外做了朝内的面**，每张面仍然是单面。

常见做法（原神裙子基本是这个）：

裙子在 DCC 里复制一层外裙面片，法线翻过来，稍微往里收一点（避免和外面 z-fighting）。于是：

- 朝外的面：UV 走裙子花纹，Body 那套 albedo / ILM / ramp  
- 朝内的面：UV 指到贴图上一块涂死的灰紫，或换一个很便宜的第二材质，就是一个灰紫 `_BaseColor`

渲染时两边都是 `Cull Back`。相机从下往上「钻进裙摆」，看到的是那组朝内的三角，不是外裙的背面。

截图里内侧不是死平的色块，还有一点明暗，说明里层多半 **还在走卡通 ramp**（同一套 Body，或简化版只乘颜色），不是 Unlit 填色。

和「一张面 Cull Off、用 `VFACE` 正面贴图背面改色」的区别：

| | 翻面补几何（原神这类） | 双面 + VFACE |
|---|---|---|
| 三角数量 | 裙子相关面大约 ×2 | 不增加 |
| Cull | 仍 Back，单面 | Off |
| 里外材质 | 可完全两套（贴图 vs 纯色） | 同一 Pass 里分支 |
| 描边 | 里外可分别控制 | 背面也容易描出一圈 |

你在 DCC / Unity 里看到裙子是单面材质、没有 Cull Off，和「里面是灰紫」不矛盾：灰紫是 **另一组朝里的面**，不是外裙贴图的反面。选中 Body 网格看材质槽，往往除了身体还有一个 Inner / 第二个 slot，或同一张 Diffuse 边上有一块没细节的紫灰。

## 190. 你（Tuesday, Sep 15, 2026, 12:26 AM (UTC+8)）

OK，那需要把body 的shader的 Cull放到材质球属性面板，代码应该怎么写？

### 助手

`Cull` 是渲染状态，写在 Pass 上，用材质属性去驱动，**不必在片元里读**。描边那一趟必须继续剔正面，不要跟身体共用这个开关。

**Properties**（和其他选项放一起即可）：

```hlsl
[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
```

`0 = Off` 双面，`1 = Front`，`2 = Back`（默认，和现在一样）。GUI 会显示成下拉框。

**CBUFFER** 里加一行（给 SRP Batcher 凑齐属性，shader 里可以不用）：

```hlsl
float _Cull;
```

**ForwardLit** 把写死的 `Cull Back` 换成：

```hlsl
Cull [_Cull]
```

**DepthOnly** 同样改成 `Cull [_Cull]`，不然双面时颜色有背面、深度没有，软粒子和深度 rim 会对不上。

**Outline 不要改**，保持：

```hlsl
Cull Front
```

里外两层几何仍然是两套单面三角的话，外裙、里裙各用一个材质球，两边都 `Back` 即可。只有「一张面、正反都画」才把该材质的 Cull 调成 **Off**。

## 191. 你（Tuesday, Sep 15, 2026, 12:35 AM (UTC+8)）

我可以在材质球上控制描边pass的开启和关闭吗？

### 助手

可以。有两种做法，差别是 **Pass 还画不画**。

**推荐：关掉整个 Outline Pass**（不占填充率）

材质上加开关：

```hlsl
[Header(Outline)]
[Toggle] _EnableOutline("Enable Outline", Float) = 1
```

你已经有 `GenshinCharacterGui`，在 `OnGUI` 里画完属性之后加：

```csharp
Material mat = materialEditor.target as Material;
if (mat.HasProperty("_EnableOutline"))
    mat.SetShaderPassEnabled("Outline", mat.GetFloat("_EnableOutline") > 0.5f);
```

`SetShaderPassEnabled` 的名字对应 Pass 上的 `Name "Outline"`，不是 `LightMode`。关了之后 URP 不会再调度这一趟。Hair / Face 若也是 `Name "Outline"`，同一套 GUI 能共用。

**只改 Shader、不改 C#**：Toggle 照样加，Outline 顶点里宽度乘上开关——Pass 还在跑，只是挤出为 0，几乎看不见，但仍有一次绘制：

```hlsl
float width = _OutlineWidth * step(0.5, _EnableOutline);
o.positionCS = GenshinOutlinePositionCS(..., width);
```

里裙如果本来就不该描边，用第一种，给里层材质关掉 `_EnableOutline` 即可。宽度滑到 0 也能藏描边，但 Pass 不会被关掉。

## 192. 你（Tuesday, Sep 15, 2026, 12:45 AM (UTC+8)）

我加上了，但是Body_UV1这个模型上的描边还在，我已经把这个模型上的开关 关掉了

### 助手

开关关了描边还在，先对一下 GUI 有没有真正关掉 Pass，以及 Body_UV1 是不是还有别的材质在画 Outline。

### 助手

关不掉，是因为 URP **不认 Pass 的 `Name "Outline"`**，它只按 `LightMode` 抽通道。你 GUI 里写的 `SetShaderPassEnabled("Outline", …)` 等于没关，描边还在用 `SRPDefaultUnlit` 画。

已经改成同时关 `"SRPDefaultUnlit"`，并且 Outline 顶点里乘了 `_EnableOutline`，开关关掉时挤出宽度为 0。

再看一眼 **Body_UV1** 的 Renderer：一个网格经常有 **多个材质槽**（身体 / 里裙）。只关其中一个球，别的槽仍会描边。每个用 Body shader 的材质都要关，或在 Project 里改共享的那张 `.mat`。

编译后重新点一下材质上的开关（GUI 要跑一次才会写入 Pass 开关）。若还在，把该 Renderer 上所有材质的 Enable Outline 都关掉。

## 193. 你（Tuesday, Sep 15, 2026, 12:50 AM (UTC+8)）

Body_UV1这个mesh的描边关掉了，现在裙子内侧还是有黑色描边，是Body这个裙子外层的描边穿过来； 看图片，这有什么解决方案吗？

### 助手

这是 **外裙倒挤描边穿到镜头里**，不是里裙自己又画了一遍。

描边是 `Cull Front` 把外裙沿法线（和平滑法线）往外推一层。裙摆法线有朝下、朝里的分量，挤出来的黑边会落到 **里裙和相机之间**。深度测试谁近谁赢，所以你看见紫灰内侧上还有一圈黑线。里裙关掉 Outline 也没用，线是外层 Body 画的。

可行做法，按原神常用程度：

**1. 顶点色压描边宽度（最对症，你 shader 已经接了）**  
大纲顶点是 `_OutlineWidth * IN.color.g`。在 DCC 里把 **外裙内侧 / 下摆** 的顶点色 G 涂成 0，那些地方挤出为 0，就不会穿进来。身体、袖口、裙外轮廓继续留 G=1。原神角色包几乎都是这样控描边的。

**2. 外裙拆成独立材质**  
若 Body 网格把身体和裙子画在同一个材质槽里，关描边会连身体一起没。把外裙拆成单独 SubMesh / 材质，那张球 `Enable Outline` 关掉。里外两层就都没描边，穿帮自然消失；外轮廓会少一圈线，裙摆边缘可能变“平”。

**3. 不要用改 Queue / ZTest 硬盖**  
里裙往后画、描边关 ZWrite，都挡不住「描边顶点已经顶到相机更近」；Stencil 能抠，但为这个做一套模板测试偏重。

优先做 **1**：只涂外裙朝里的那一圈顶点。涂完不用改进 shader，转镜头看内侧黑边应消失，角色外轮廓还在。若 Body 没有顶点色（G 全是 1），需要在 Blender / Maya 里画完再进 Unity，平滑法线仍放 TANGENT。

## 194. 你（Tuesday, Sep 15, 2026, 9:08 PM (UTC+8)）

OK，一切都正常了，现在准备做角色切换功能，就是之前聊过的，你先看一下，思路该怎么做；先不用写代码；

### 助手

先看场景里现在有几个角色、根节点怎么挂，再按现状把切换思路对齐一遍。

### 助手

场景里已经有 **Barbara** 和 **Lumine**，各挂了 `FaceHeadOrientation`。切换器只要管这两个根节点的列表，相机、灯、地板都不动。

## 舞台约定

列表顺序 = 左右键顺序。平时只有 **当前角色** 在舞台中心、`SetActive(true)`，Idle 照常播；另一个关掉，省蒙皮和倒影。

位移用 **相机水平右方向**（屏幕的左/右），不要用角色本地 X。你们角色根节点 Y=90°，用本地轴会和镜头左右对不上。

## 一次切换（右箭头 = 下一位）

| | 退场 A | 登场 B |
|---|---|---|
| 开始 | 中心，完全可见 | 屏幕右侧外，完全透明，打开 |
| 过程 | 向屏幕左滑 + 淡出 | 从右滑到中心 + 淡入 |
| 结束 | 关掉 | 钉在中心，成为当前 |

左箭头方向对调。列表到头循环。动画期间丢掉新按键（或只记一次），避免连按把坐标拧乱。时长大约 0.4～0.8 秒，位置和淡出共用一条 0～1 曲线。

## 淡出为什么必须进 shader

Body / Hair / Face / Outline / 里裙都是不透明。改材质颜色 Alpha 不会变透明。要用 **MPB 写 `_Fade`（1→0）**，片元里 dither `clip`。每个角色自己的 Renderer 写 MPB，不要改共享 `.mat`。

根节点下 **所有** SkinnedMesh（身体、里裙 Body_UV1、头发、脸）都要写同一个 `_Fade`，描边 Pass 也要 clip，否则人没了线还在。

## 和现有系统

- **FaceHeadOrientation**：继续挂在各自根上，关掉的人不跑。
- **平面倒影**：切换那几帧两人同时激活，倒影里一起滑，这是对的。`_FadeCenter` 仍对准舞台中心。
- **Animator**：滑动过程保持 Idle，切完再 `SetActive(false)` 退场的那个。
- **描边顶点色**：不用动，跟着 mesh 走。

## 建议拆四步写

1. 列表 + 左右键，只 `SetActive` 一个根，不播动画。确认循环、输入。
2. 按屏幕左右滑位置，先不做透明。
3. 四个角色 shader 加 `_Fade` dither；脚本对当前两人写 MPB。
4. 锁输入、切完关掉退场角色。

第一步不碰 shader。角色根拖进切换器列表即可。准备好了就从「只显示一个」开始写。

## 195. 你（Tuesday, Sep 15, 2026, 9:13 PM (UTC+8)）

好的，开始第一步吧，也是你说逻辑和功能，我来写；MPB是什么？

### 助手

**MPB** 是 **Material Property Block**（材质属性块）。同一张 `.mat` 被很多人共用时，如果 `material.SetFloat("_Fade", 0)`，所有人都变透明。MPB 是挂在 **某一个 Renderer** 上的临时覆盖：只改这个人的 `_Fade`，磁盘上的材质不动。第三步淡入淡出才用它，现在不用写。

---

第一步只做一件事：**左右键切换「谁在台上」**，没有滑动、没有淡出。

逻辑：

- 一个 `Transform[] characters`，顺序就是按键顺序（0 芭芭拉、1 荧）。
- `int _index` 记当前是谁。
- 右箭头：`_index + 1`，超过最后一个回到 0。
- 左箭头：`_index - 1`，小于 0 回到最后一个。
- 每次切完：循环列表，只有 `_index` 那个 `SetActive(true)`，其余 `false`。
- 用 `Input.GetKeyDown`（项目里 Quality 面板也是这套）。
- `Start` 里先刷一次，避免两个角色开场叠在一起。

挂在空物体上（比如场景里的 `Cha`），把 Barbara、Lumine 的 **根** 拖进数组。

```csharp
using UnityEngine;

namespace RenderLab
{
    public class CharacterSwitcher : MonoBehaviour
    {
        public Transform[] characters;
        public int startIndex;

        int _index;

        void Start()
        {
            _index = characters != null && characters.Length > 0
                ? Mathf.Clamp(startIndex, 0, characters.Length - 1)
                : 0;
            Apply();
        }

        void Update()
        {
            if (characters == null || characters.Length == 0)
                return;

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                _index = (_index + 1) % characters.Length;
                Apply();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _index = (_index - 1 + characters.Length) % characters.Length;
                Apply();
            }
        }

        void Apply()
        {
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i] == null)
                    continue;
                characters[i].gameObject.SetActive(i == _index);
            }
        }
    }
}
```

负数取模在 C# 里会得到负数，左边要用 `(_index - 1 + Length) % Length`，不要写成 `(_index - 1) % Length`。

进 Play：右键只剩下一位，左键回去，同时只有一个人。Hierarchy 里关掉的那个应是灰的。这一步过了再做滑动。

## 196. 你（Tuesday, Sep 15, 2026, 9:27 PM (UTC+8)）

功能已经实现了，角色在原地切换；

## 197. 你

<available_subagent_types>
Available subagent_types and a quick description of what they do:
- generalPurpose: General-purpose agent for researching complex questions, searching for code, and executing multi-step tasks. Use when searching for a keyword or file and not confident you'll find the match quickly.
- explore: Fast agent specialized for exploring codebases. Use this when you need to quickly find files by patterns (eg. "src/components/**/*.tsx"), search code for keywords (eg. "API endpoints"), or answer questions about the codebase (eg. "how do API endpoints work?"). When calling this agent, specify the desired thoroughness level: "quick" for basic searches, "medium" for moderate exploration, or "very thorough" for comprehensive analysis across multiple locations and naming conventions.
- cursor-guide: Read Cursor product documentation to answer questions about how Cursor Desktop, IDE, CLI, Cloud Agents, Bugbot, and other features work. Use when the user asks 'In Cursor, how do I...?' or similar questions about Cursor products.
- ci-investigator: Investigate a single failing PR CI check and return a short root-cause summary. Use when the user asks to summarize, explain, diagnose, or investigate a specific failed check from a pull request.
- bugbot: Use only when the user *explicitly* asks for a Bugbot-like review of local code changes. When launching this subagent, set the Task description to exactly "Bugbot". Launch exactly one Bugbot subagent with `run_in_background: false` unless the user explicitly asks to run in background. Use this fixed prompt form: "Full Repository Path: ...\nDiff: <one of: \"branch changes\", \"uncommitted changes\", \"natural language\">\nChange Description: ...\nCustom Instructions: ..."; default to `Diff: branch changes`; include `Change Description` only when `Diff` is `natural language`, formatting it as one block per changed file (a `<path> (added|modified|deleted|renamed)` header followed by bullets of what changed, mentioning line numbers or ranges inline where helpful), and only use `natural language` as a last resort after a regular diff-based review failed because the diff could not be computed; include `Custom Instructions` only when the user gave specific review instructions. This subagent is single-shot and does not support `resume`; always launch a fresh subagent instead.
- security-review: Use only when the user *explicitly* asks for a security review of local code changes. When launching this subagent, set the Task description to exactly "Security Review". Launch exactly one security-review subagent with `run_in_background: false` unless the user explicitly asks to run in background. Use this fixed prompt form: "Full Repository Path: ...\nDiff: <one of: \"branch changes\", \"uncommitted changes\">\nCustom Instructions: ..."; default to `Diff: branch changes`; include `Custom Instructions` only when the user gave specific review instructions. This subagent is single-shot and does not support `resume`; always launch a fresh subagent instead.
- best-of-n-runner: Run a task in an isolated git worktree. Each best-of-n-runner gets its own branch and working directory. Use for best-of-N parallel attempts or isolated experiments.
</available_subagent_types>

<available_subagent_models>
If the user explicitly asks for the model of a subagent/task, you may ONLY use model slugs from this list:
- inherit (default; required unless the user explicitly requested another model)
- composer-2.5-fast
- cursor-grok-4.6-high-fast

If the user isn't asking for a specific version, prefer the latest version of the model family. As an example, if the user just says "gpt" or "claude", use the latest available version of GPT or Claude.

IMPORTANT: If the user requests a model that is NOT in the list above, do NOT substitute a different model or guess. Instead, skip launching the subagent with that model and tell the user which model was unavailable and which models are available.

When speaking to the USER about which model you selected for a subagent, do NOT use the kebab-case model names unless the user requested the model using that format. Ue the same naming scheme the user used to discuss the model when they requested it.
</available_subagent_models>

<dynamic_tools>
You have access to tools through dynamic namespaces, e.g. MCP servers, using `GetDynamicTools` and `CallDynamicTool`.

## Dynamic Tool Discovery and Invocation

Use `GetDynamicTools` to discover tool schemas, then `CallDynamicTool` to invoke one tool. Aim to minimize round-trips: ideally one discovery call followed by one invocation.

If the user mentions a product or service represented by an available namespace, and the request likely depends on it, proactively inspect that namespace before answering. If you are unsure which namespace matches, search with a relevant pattern.

`GetDynamicTools` supports these modes:

1. `{"namespace":"<id>"}`: returns schemas and full descriptions for every tool in that namespace.
2. `{"namespace":"<id>","toolName":"<name>"}`: returns one tool schema with its full description.
3. `{"pattern":"<regex>"}`: searches namespace and tool names.
4. `{"namespace":"<id>","pattern":"<regex>"}`: searches tools within one namespace.
5. No arguments: returns the full catalog.

Pattern-search and catalog results shorten long descriptions, marked by a trailing "... [truncated]"; namespace and single-tool lookups always return the complete description.

Always inspect a tool's schema before invoking it with `CallDynamicTool`.

If the available dynamic tools do not fully support what the user asked you to do, complete the work you can with the current tool set. In your work summary, include what you were unable to do and why. Do not use browser automation to work around missing tools unless the user explicitly asks you to use the browser.

Available dynamic tool namespaces:

<dynamic_tool_namespaces>
<namespace name="cursor-ide-browser" tools="browser_navigate, browser_snapshot, browser_click, browser_mouse_click_xy, browser_type, browser_fill, browser_select_option, browser_press_key, browser_scroll, browser_drag, browser_get_bounding_box, browser_highlight, browser_tabs, browser_cdp, browser_take_screenshot, browser_lock" namespaceUseInstructions="The cursor-ide-browser MCP server provides a Cursor-owned browser tab plus a raw Chrome DevTools Protocol command tool.

CORE WORKFLOW:
1. Start by understanding the user's goal and what success looks like on the page.
2. Use browser_tabs with action "list" to inspect open tabs and URLs before acting.
3. Use browser_navigate to create or navigate the target tab. Omit the position parameter for background automation so focus is preserved.
4. Use browser_lock before longer automation on an existing tab, then browser_lock with action "unlock" when finished.
5. Use browser_snapshot for accessibility context and browser_take_screenshot for visual verification.
6. Use browser_click, browser_type, browser_fill, browser_select_option, browser_press_key, browser_scroll, and browser_drag for page interactions.
7. Use browser_highlight and browser_get_bounding_box for visual grounding and coordinate diagnostics.
8. Use browser_cdp for page inspection, profiling, runtime evaluation, DOM/CSS queries, and performance data.

AVOID RABBIT HOLES:
1. Do not repeat the same failing action more than once without new evidence such as a fresh snapshot, a different ref, a changed page state, or a clear new hypothesis.
2. IMPORTANT: If four attempts fail or progress stalls, stop acting and report what you observed, what blocked progress, and the most likely next step.
3. Prefer gathering evidence over brute force. If the page is confusing, use browser_snapshot, browser_take_screenshot, or CDP inspection before trying more actions.
4. If you encounter a blocker such as login, passkey/manual user interaction, permissions, captchas, destructive confirmations, missing data, or an unexpected state, stop and report it instead of improvising repeated actions.
5. Do not get stuck in wait-action-wait loops. Every retry should be justified by something newly observed.

CRITICAL - Lock/unlock workflow:
1. browser_lock requires an existing browser tab - you CANNOT call browser_lock with action: "lock" before browser_navigate
2. Correct order: browser_navigate -> browser_lock({ action: "lock" }) -> (interactions) -> browser_lock({ action: "unlock" })
3. If a browser tab already exists (check with browser_tabs list), call browser_lock with action: "lock" FIRST before any interactions
4. Only call browser_lock with action: "unlock" when completely done with ALL browser operations for this turn

IMPORTANT - Waiting strategy:
When waiting for page changes, prefer short CDP polling loops with Runtime.evaluate, DOM queries, Page lifecycle signals, or browser_snapshot checks rather than a single long wait.

CDP USAGE:
- Use browser_cdp with a DevTools Protocol method and params object, for example Runtime.evaluate, DOM.getDocument, CSS.getComputedStyleForNode, Profiler.start/stop, Performance.getMetrics, Log.enable, and Network.enable.
- Do not use browser_cdp with CDP Input.* methods. They are denied because they are focus-sensitive in Electron webviews and can route input to Cursor UI instead of the browser page.
- Use browser_click, browser_type, browser_fill, browser_select_option, browser_press_key, browser_scroll, and browser_drag for clicks, typing, filling inputs, selecting options, keyboard actions, scrolling, and drag-and-drop.
- Use Runtime.evaluate for advanced DOM-scoped interactions that the dedicated browser tools do not cover.
- For profiling, call Profiler.enable, Profiler.start, reproduce the behavior, then Profiler.stop. The profile is saved to a file and returned as a log_file; read that file only when you need to inspect details.
- For JavaScript evaluation, prefer Runtime.evaluate with returnByValue when possible.
- Some browser-wide or sensitive CDP methods are denied, especially cookie, storage, permission, download, target-management, filesystem-backed file-input commands, system-level commands, and CDP navigation/history navigation commands.
- Large CDP responses are saved to files instead of being inlined. Prefer using the returned file path over immediately stuffing large payloads into context; read focused sections only when needed.

VISION:
- browser_take_screenshot attaches an image result that the model can inspect. CDP Page.captureScreenshot returns data inside JSON and should not replace browser_take_screenshot when visual verification is needed.

NOTES:
- browser_snapshot returns snapshot YAML and is the main source of truth for page structure.
- Refs are opaque handles tied to the latest browser_snapshot for that tab.
- Iframe content is not accessible - only elements outside iframes can be interacted with.
- When you stop to report a blocker, include the current page, the target you were trying to reach, the blocker you observed, and the best next action. If the blocker requires manual user interaction, ask the user to take over at that point rather than assuming it in advance." source="mcp" />
<namespace name="plugin-gmail-gmail" source="mcp" />
<namespace name="plugin-google-drive-google-drive" source="mcp" />
<namespace name="cursor" tools="AwaitShell, CreateGoal, Delete, EditNotebook, FetchMcpResource, GenerateImage, ReadLints, SwitchMode, Task, TodoWrite, UpdateGoal, WebFetch, WebSearch" namespaceUseInstructions="Native Cursor tools for this session. These are highly recommended and useful tools that you should use when the right situation arises. Don't be afraid to look at one if it seems relevant, even if you don't end up using it. You MUST read the tool schemas before calling them.

Here are some crucial instructions:
- AwaitShell: Use to sleep and check shell progress. Never sleep using shell.
- ReadLints: Check for linter errors after substantive edits.
- SwitchMode: Switch between modes: Agent and Plan. Proactively consider switching modes for relevant requests.
- TodoWrite: Use this tool to manage complex multi-step tasks." source="cursor" />
</dynamic_tool_namespaces>

If an MCP-backed namespace requires authentication, call `mcp_auth` through `CallDynamicTool` for that namespace, then inspect it again and retry if appropriate. Do not authenticate namespaces preemptively or repeatedly.
</dynamic_tools>

## 198. 你（Tuesday, Sep 15, 2026, 9:32 PM (UTC+8)）

我想在切换成功角色以后，我的Sky_Star_Mesh的星星的颜色TintA和Tint设置成另外的颜色，另外，天空盒的现在的蓝色贴图换成其他颜色的贴图，这个功能也能集成在这个脚本中吗？

### 助手

可以，就放在 `CharacterSwitcher` 里。主题和角色列表 **下标对齐**（0 跟芭芭拉、1 跟荧），滑动结束和开场 `ApplyInstant` 各调一次即可。

星的 `_TintA` / `_TintB` 用 **MPB** 写在 `Sky_Star_Mesh` 的 Renderer 上，不要 `sharedMaterial.SetColor`，否则会改到磁盘上的 `.mat`。

天空盒是场景里的 `CubeMap_BG01`（`Skybox/Cubemap`），贴图槽叫 `_Tex`。换 Cubemap：

```csharp
RenderSettings.skybox.SetTexture("_Tex", cube);
DynamicGI.UpdateEnvironment();
```

地板若开了 `_SkyCube`，倒影里的天 **不会** 自动跟着天空盒变，要在主题里再给地板赋一次同一张图。

加一个和角色数组等长的主题：

```csharp
[System.Serializable]
public class CharacterTheme
{
    public Color tintA = new Color(0.55f, 0.85f, 1f);
    public Color tintB = new Color(1f, 0.75f, 0.95f);
    public Cubemap skyCube;
}

public CharacterTheme[] themes;
public Renderer starRenderer;
public Material floorMaterial;
```

再加应用函数（可复用同一个 MPB）：

```csharp
static readonly int TintAId = Shader.PropertyToID("_TintA");
static readonly int TintBId = Shader.PropertyToID("_TintB");
static readonly int SkyTexId = Shader.PropertyToID("_Tex");
static readonly int FloorCubeId = Shader.PropertyToID("_SkyCube");

MaterialPropertyBlock _starBlock;

void ApplyTheme(int index)
{
    if (themes == null || index < 0 || index >= themes.Length)
        return;
    CharacterTheme t = themes[index];

    if (starRenderer != null)
    {
        if (_starBlock == null)
            _starBlock = new MaterialPropertyBlock();
        starRenderer.GetPropertyBlock(_starBlock);
        _starBlock.SetColor(TintAId, t.tintA);
        _starBlock.SetColor(TintBId, t.tintB);
        starRenderer.SetPropertyBlock(_starBlock);
    }

    if (t.skyCube != null && RenderSettings.skybox != null)
    {
        RenderSettings.skybox.SetTexture(SkyTexId, t.skyCube);
        DynamicGI.UpdateEnvironment();
    }

    if (floorMaterial != null && t.skyCube != null)
        floorMaterial.SetTexture(FloorCubeId, t.skyCube);
}
```

调用：`ApplyInstant()` 末尾、`Slide` 里 `_index = next` 之后各写一行 `ApplyTheme(_index)`。

Inspector：`themes` Size 和角色数量一样，每个元素填 TintA、TintB、对应 Cubemap；Star Renderer 拖 `Sky_Star_Mesh`；地板材质可选。

先换贴图、瞬间变色即可。以后要和滑动一起渐变，再把颜色/Cubemap 做成 lerp，现在不必。

## 199. 你（Tuesday, Sep 15, 2026, 9:35 PM (UTC+8)）

这个脚本功能你直接在CharacterSwitcher.cs这个脚本上帮我修改吧；但是如果要修改shader就让我来修改；

### 助手

我直接改 `CharacterSwitcher.cs`：给每个角色配 Tint 和天空盒 Cubemap，切换成功后立刻套上。Shader 不动。

### 助手

只改 C#：切角色后用 MPB 改星星 Tint，运行时克隆天空盒再换 Cubemap，避免写回 `.mat`。

### 助手

已经加进 `CharacterSwitcher.cs`，Shader 没动。切换结束和开场都会套当前角色的主题。

在挂了这个脚本的物体上填：

1. **Themes** Size 和 Characters 一样（0 芭芭拉、1 荧）。
2. 每个元素设 **Tint A**、**Tint B**，以及要换成的 **Sky Cube**。
3. **Star Renderer** 可以空着，运行时会找名为 `Sky_Star_Mesh` 的物体；想稳妥就手动拖上去。
4. 地板倒影也要跟天空盒一起换时，把地板 Mesh 拖到 **Floor Renderer**。

星星颜色走 MPB，天空盒会克隆一份再改 `_Tex`，不会写回 `CubeMap_BG01.mat`。没填 Cubemap 的那一项只改星星，天空盒保持原样。

## 200. 你（Tuesday, Sep 15, 2026, 9:49 PM (UTC+8)）

FLoor地板上的材质球换了Sky Cubemap以后，Floor Color这个值也需要换，这个值也加入到脚本上吧；

### 助手

把 `_FloorColor` 加进每个角色主题，和 Cubemap 一起用 MPB 写到地板上。

### 助手

已经加进每个 Theme 的 **Floor Color**，切角色时和 Cubemap 一起写到地板的 `_FloorColor`。

Inspector 里每个 Theme 现在都能单独填这块颜色。记得 **Floor Renderer** 拖上地板 Mesh，否则颜色和 Cubemap 都不会改。没有 Cubemap 时仍会换地板颜色。

## 201. 你（Tuesday, Sep 15, 2026, 9:54 PM (UTC+8)）

我还想吧场景里名为Cloud的粒子系统里的Star Color也放到脚本里，对应不同角色的不同Cloud的颜色；

### 助手

场景里 Cloud 用的是 URP 粒子材质。把每个角色的 Cloud 颜色加进主题，切角色时同时改粒子 Start Color 和材质着色。

### 助手

已经加进每个 Theme 的 **Cloud Color**。切角色时会改名为 `Cloud` 的粒子：既写 Start Color（新粒子），也用 MPB 改 `_StarColor` / `_BaseColor`（场上已有的云立刻变色）。

Inspector 里每个 Theme 填 **Cloud Color** 即可。物体必须叫 `Cloud`，或者把粒子系统拖到 **Cloud Particles**。颜色开了 HDR，可以拉过 1 去喂 Bloom。

## 202. 你（Tuesday, Sep 15, 2026, 9:58 PM (UTC+8)）

Start Color 可以像粒子系统我设置的一样，在两个颜色之间变化吗？

### 助手

可以。Cloud 粒子本身就是 Random Between Two Colors；改成每个主题两个颜色，只写 `startColor`，不再用材质单色盖掉变化。

### 助手

可以，已经改成粒子系统里那种 **Random Between Two Colors**。

每个 Theme 现在有 **Cloud Color A** 和 **Cloud Color B**。新生成的粒子会在这两个颜色之间随机取值，和 Inspector 里 Start Color 选两个颜色时一样。

场上已经活着的粒子不会立刻改色，要等它们消掉再出生。材质上的单色覆盖去掉了，否则会把双色冲掉。

原先那个单独的 Cloud Color 字段拆成了两个，Inspector 里需要重新填一遍 A / B。

## 203. 你（Tuesday, Sep 15, 2026, 10:04 PM (UTC+8)）

还有一个颜色，就是Cloud材质球上的BaseColor的颜色也加入到脚本中

### 助手

把材质上的 `_BaseColor` 加进每个主题，用 MPB 写到 Cloud 的 Renderer，不改 `.mat` 文件。

### 助手

已经加进每个 Theme 的 **Cloud Base Color**，切角色时用 MPB 写 `_BaseColor`（URP 粒子还会同步 `_Color`），不会改 `FX_Cloud.mat`。

它和 Start Color A/B 是两层：材质 Base Color 是整片云的底色，粒子 Start Color 仍在两个颜色之间随机。最终颜色是两者相乘。Inspector 里每个角色单独填这个 HDR 颜色即可。

## 204. 你（Tuesday, Sep 15, 2026, 10:26 PM (UTC+8)）

切换角色的时候，我发现Cloud粒子的颜色没有及时刷新，我重新激活一下就刷新了，是不是可以按下箭头切换的角色的时候重新激活一下Cloud的粒子？

### 助手

Start Color 在出生时写进粒子，场上旧粒子不会跟着改。切角色后先改颜色，再开关一次 Cloud，让粒子用新颜色重新生成。

### 助手

已经加上。左右键切完角色、套上新颜色之后，会立刻把 Cloud 关再开一次，粒子按新的 Start Color / Base Color 重新生成。

原因是 Start Color 只在粒子出生时写入，场上还活着的那批不会自己改色。开场 `ApplyInstant` 也会走同一套，避免第一次主题对不上。

## 205. 你（Tuesday, Sep 15, 2026, 10:33 PM (UTC+8)）

OK，效果都正常了，现在还差淡入淡出？

### 助手

淡入淡出是切换里还没做的那一步。先看角色 Shader 里有没有 `_Fade`，再把滑动时的淡入淡出接到 `CharacterSwitcher`。

### 助手

对，角色滑动时的 dither 淡入淡出还没接。C# 我直接改脚本；Shader 里还没有 `_Fade`，片段你自己加。

### 助手

对，切换这边还差这一步。脚本已经在滑动时用 MPB 写 `_Fade`：出场 `1 → 0`，入场 `0 → 1`。Shader 里还没有这个属性，所以你现在按箭头还看不出 dither。

不透明卡通角色不要改 Alpha，用屏幕空间 dither + `clip`，描边才不会变成半透明壳。

在 `GenShin_Shared.hlsl` 里加：

```hlsl
void CharaDitherClip(float2 posCS, float fade)
{
    clip(fade - InterleavedGradientNoise(posCS, 0) - 1e-4);
}
```

Body / Hair / Face 各自：

1. Properties：`_Fade("Fade", Range(0, 1)) = 1`
2. CBUFFER：`float _Fade;`
3. Forward、Outline、DepthOnly 三个 fragment **开头**调用：

```hlsl
CharaDitherClip(IN.positionCS.xy, _Fade);
```

DepthOnly 也要 clip，否则淡出时深度 Rim 和软粒子仍会当人是实心的。三个 Shader 都加完，滑动淡入淡出就会和位移一起走。

## 206. 你（Tuesday, Sep 15, 2026, 11:35 PM (UTC+8)）

OK，角色半透明消失已经实现了； 现在就是天空球在切换的时候还没有过渡

### 助手

天空盒现在是切完瞬间换贴图。先看场景里天空球怎么画，再在滑动过程里把颜色和贴图做成过渡。

### 助手

Cubemap 不能在 C# 里直接插值。脚本会在滑动时把星星、地板颜色和两张天空盒做成过渡；天空盒混合需要一个很小的 Shader，我写成新文件，不改你现有的角色/地板 Shader。

### 助手

滑动过程里，星星 Tint、地板颜色、天空盒两张 Cubemap、Cloud Base Color 都会跟着 `u` 插值。Cloud 的 Start Color 仍在结束时重启一次。

Cubemap 没办法在 C# 里直接 lerp，所以新建了 `Genshin_SkyboxBlend.shader`（没改你现有的角色 Shader）。运行时会把天空盒换成它，用 `_Tex` / `_TexB` / `_SkyBlend` 做交叉淡化。

地板倒影还是只采一张 `_SkyCube`，要一起过渡的话在 `Genshin_Floor.shader` 里自己加：

Properties / CBUFFER：

```hlsl
[NoScaleOffset] _SkyCubeB("Sky Cubemap B", Cube) = "" {}
_SkyBlend("Sky Blend", Range(0, 1)) = 0
```

变量：`TEXTURECUBE(_SkyCubeB);`，以及 `float _SkyBlend;`

`SampleSkyCubemap` 改成：

```hlsl
half3 a = SAMPLE_TEXTURECUBE_LOD(_SkyCube, sampler_SkyCube, R, mip).rgb;
half3 b = SAMPLE_TEXTURECUBE_LOD(_SkyCubeB, sampler_SkyCube, R, mip).rgb;
return lerp(a, b, saturate(_SkyBlend));
```

星星和地板颜色现在就会跟角色一起过渡。地板 Cubemap 要等上面这几行加上之后，倒影才会跟着混。

## 207. 你（Tuesday, Sep 15, 2026, 11:41 PM (UTC+8)）

这个天空盒新的shader好像有报错

### 助手

问题多半是 HLSL 里 `float3(float2, float)` 这种写法，官方天空盒用的是 CG，这边会编不过。改成和 Unity 自带 Cubemap 天空盒同一套写法。

### 助手

已经改好了。之前用 URP 的 HLSL 去写天空盒，`float3(float2, float)` 这种拼向量的方式和 `Core.hlsl` 对天空盒都不合适，所以会报错。

现在改成和 Unity 自带 `Skybox/Cubemap` 同一套 **CGPROGRAM + UnityCG.cginc**，再在上面加 `_TexB` / `_SkyBlend`。让 Unity 重新编一下这个 Shader，红色报错应会消失，滑动时两张 Cubemap 仍会交叉过渡。

## 208. 你（Tuesday, Sep 15, 2026, 11:46 PM (UTC+8)）

切换的时候地板上的反射天空的颜色有点延迟，导致天空已经换了贴图，地板慢一拍再换的反射；这个有办法解决吗？

## 209. 你（Tuesday, Sep 15, 2026, 11:53 PM (UTC+8)）

@Assets/RenderingLab/Cha/Yuanshen/Shader/Genshin_Floor.shader  这个shader有个报错，帮我看一下

### 助手

DepthOnly 通道多了一个 `}`，而且缺了 `DepthAttributes` / `DepthVaryings` 结构体，所以编不过。正在补回去。

### 助手

报错是 DepthOnly 通道里用了 `DepthVaryings`，但结构体当时没声明。把公共结构放到 `HLSLINCLUDE`，并修掉 `half4` 赋给 `half3` 的截断警告。

### 助手

报错是 **`unrecognized identifier 'DepthVaryings'`**，出在 DepthOnly 通道：函数已经在用这个类型，但结构体当时还没写，Unity 就编不过。

已经把 `DepthAttributes` / `DepthVaryings` 放到 SubShader 的 `HLSLINCLUDE` 里，两个 Pass 都能看见。顺手修了 `SamplePlanarSky` 里 `half4` 赋给 `half3` 的截断警告。

让 Unity 重新编一下这个 Shader，这条红字应会消失。Console 里如果还挂着旧报错，点一下 Clear 再看。

## 210. 你（Tuesday, Sep 15, 2026, 11:57 PM (UTC+8)）

OK，现在完美了； 但是我还是没了解Genshin_SkyboxBlend这个shader你是怎么使用的？不是替换CubeMap_BG01这个材质球的shader吗？

### 助手

不是改磁盘上的 `CubeMap_BG01.mat`。那个材质球一直还是 Unity 自带的 `Skybox/Cubemap`，只有一张 `_Tex`，没法两张图交叉淡化。

真正换 Shader 的是 **运行时克隆出来的那一份**，过程在 `EnsureSkyboxInstance`：

1. 记下场景当前天空盒：`RenderSettings.skybox`，一般就是 `CubeMap_BG01`。
2. `new Material(...)` 复制一份，**不写回** `.mat` 文件。
3. 把这份克隆的 Shader 改成 `RenderingLab/Genshin/SkyboxBlend`（多了 `_TexB` 和 `_SkyBlend`）。
4. 把原来的 Cubemap 拷到 `_Tex` / `_TexB`，`_SkyBlend = 0`。
5. `RenderSettings.skybox = 克隆`，之后相机画的是这份运行时材质。

退出 Play 时 `OnDestroy` 会把 `RenderSettings.skybox` 设回原来的 `CubeMap_BG01`，所以 Project 里那个材质球不会脏。

滑动时脚本只改克隆：

| 属性 | 含义 |
|---|---|
| `_Tex` | 当前角色的 Cubemap |
| `_TexB` | 下一个角色的 Cubemap |
| `_SkyBlend` | `0 → 1`，片元里 `lerp(A, B, blend)` |

所以：`CubeMap_BG01` 是编辑器里的默认天空盒；`Genshin_SkyboxBlend` 只挂在 Play 模式那份克隆上。若你想在 Inspector 里直接看到 `_TexB` / `Sky Blend`，可以把 `CubeMap_BG01` 的 Shader 手动换成 `RenderingLab/Genshin/SkyboxBlend`，效果一样，只是资源文件会被改掉。现在这套是为了不碰那份资产。

## 211. 你（Wednesday, Sep 16, 2026, 12:04 AM (UTC+8)）

我想在角色切换好以后，在角色身上做一个这样的效果，现在先不做；你先分析一下这个效果制作的思路；file:///G:/MinionsArt/LowPolyGlitch%20Effect/Low%20Poly%20Glitch%20Effect%20%EF%BD%9C%20Minions%20Art%20on%20Patreon%20(2026_9_13%2015%EF%BC%9A08%EF%BC%9A29).html这个页面是实现这个效果的参考资料；

### 助手

先看你给的参考页和现有角色 Shader，再对照 GIF 拆这个效果怎么接到切换之后。

### 助手

这个 GIF 不是后处理描边，也不是粒子。它是 **在角色 Shader 里把网格“量化成大块面”，再给每个面边缘画一条 HDR 发光线**。MinionsArt 的 `LowpolyGlitch.shadergraph` 就是这条路。

参考图里三块是一回事：

| 图里写的 | 实际在干什么 |
|---|---|
| **Main Texture** | 原来的贴图 / 你现在的卡通着色 |
| **Glitchy Colors Glow** | 每个“大三角”一块高饱和色 + 边上发光 |
| **Lerp … glow** | 用一个权重在「正常角色」和「故障块面」之间混合 |

图里沙滩球面很少、边很粗，是因为顶点位置被 **Round 到格子**（图里的 `_PolySize`），平滑球被收成几块大面。边上的线来自 **Custom Interpolator 传重心坐标**，片元里 `min(bary)` 接近 0 就是边，再用 `_GlowWidth` 控制粗细。每块面的颜色是量化后的位置做 hash，所以会跳出青 / 粉 / 黄这种 HDR 色，喂 Bloom。

还有一组 **Glitch Segment**（`_SectionMoving`、`_SegmentWidth`、`_SectionSmoothness`）：用一条会动的世界空间“切片”，让故障只出现在一条带子上，而不是整身一直闪。

接到你现在的展示场，建议这样拆，先别改现有 Forward：

**1. 时机**  
滑动 + dither 结束后再播，大约 0.3～0.8 秒。`CharacterSwitcher` 用 MPB 写一个 `_Glitch`（0→1→0）。不要跟 `_Fade` 叠在同一帧里抢着 clip。

**2. 画在哪**  
Body / Hair / Face 加一条 **Additive 叠加 Pass**（或现有 Pass 里 `lerp(卡通, 故障, _Glitch)`）。描边 Pass 先不要进故障，否则外扩壳会多一套线。平面反射相机会把这层一起画进去，落地倒影是对的。

**3. 大块面**  
原神网格很密，若用真实三角边，会变成一张渔网，不像 GIF。故障 Pass 里在 **蒙皮之后** 把 `positionWS` 按格子 Round（`_PolySize` 大约角色身高的 1/8～1/20）。只动这一 Pass 的顶点，主着色保持现在的轮廓。

**4. 发光边**  
两条路，以后做的时候选一条：

- 和参考一样：重心坐标（Custom Interpolator / `SV_Barycentrics` / 不共享顶点）。共享顶点的蒙皮网格上 `VertexID % 3` 会错。
- 更省事：量化后的位置在面交界处不连续，用 `fwidth` 找跳变当边。不用拆网格。

边写成 HDR（`_Color * 强度 > 1`），你场景里已有 Bloom，看起来就会像 GIF 那种辉光。

**5. 和现有系统怎么配**  
Ramp / MatCap / Rim 继续画正常角色。`_Glitch` 只混合或叠那一层。MPB 和现在的 Tint、`_Fade` 一样，不要改共享材质。故障 Pass 也要走你已经加的 `CharaDitherClip`，避免淡入没完线已经亮着。

以后动手时先只做 Body 一条叠加 Pass + 切完角色播一次 `_Glitch`，沙滩球那种大面 + 亮边对上了，再接到 Hair / Face。现在不用改代码。

## 212. 你（Wednesday, Sep 16, 2026, 12:12 AM (UTC+8)）

等于说是在现有角色比如body上新加一个pass，这个pass里面做好这个Glitch效果，然后Glitch这个pass和body的ForwardLit Pass做过度吗？

### 助手

对，可以加一条 Glitch Pass，但 **两条 Pass 之间不能像颜色那样 lerp**。GPU 是先画 ForwardLit，再画 Glitch；过渡靠你自己控制 **Glitch 画多亮、混多少**，不是引擎把两个 Pass 交叉淡化。

更清楚的是两种接法：

**A. ForwardLit 里混合（一条 Pass）**

```hlsl
col = lerp(卡通结果, 故障色, _Glitch);
```

`_Glitch` 从 0→1→0，看起来就是两套外观在过渡。实现简单，轮廓、深度、倒影都还是同一次绘制。缺点是故障的「块面挤顶点」会动到这同一 Pass，角色剪影可能跟着抖。

**B. 另开一条 Additive Pass（上次说的）**

ForwardLit 始终画正常芭芭拉。Glitch Pass `Blend One One`，输出 `故障 * _Glitch`。`_Glitch = 0` 时等于没画。这不是两 Pass 对半混合，而是 **正常角色一直在，故障当一层光叠上去**。GIF 那种亮边很适合这种加法。顶点量化也只发生在 Glitch Pass，主身体轮廓稳住。

切换完角色时，脚本只写 MPB 的 `_Glitch`，和现在写 `_Fade` 一样。

建议先走 **B**：主着色不动，故障是一层可开关的特效。等块面和亮边对了，再决定要不要把一部分混进 ForwardLit。Hair / Face 同一套参数即可，描边 Pass 先别加。

## 213. 你（Wednesday, Sep 16, 2026, 12:14 AM (UTC+8)）

效果应该是更接近B，就是角色A淡出，角色B进入，等角色B滑入到正确的位置以后，再在角色身上叠加_Glitch这个pass；

### 助手

对，就是这个顺序。和现在的切换对齐的话是：

1. **滑动中**：A `_Fade 1→0` 滑出，B `_Fade 0→1` 滑入。这时 `_Glitch = 0`，Glitch Pass 相当于没画。
2. **滑到 `_home` 停下**：B 已经在台上，主题（天、地、云）也套完。
3. **再播故障**：只给 **当前角色 B** 的 Renderer 写 `_Glitch`，`0 → 1 → 0`（大概零点几秒）。Additive Pass 叠在已经画好的 ForwardLit 上。
4. **播完**：`_Glitch` 回到 0，身上只剩正常卡通。

不要在滑动途中开 Glitch：淡入还在 dither，再叠亮边会糊成一片。也不要给正在滑走的 A 开，那是退场不是登场。

脚本侧以后加在 `Slide` 里 `_index = next`、`ApplyTheme` **之后** 再开一条短协程即可；Shader 侧 Glitch Pass 用 `_Glitch` 乘输出，为 0 就什么都不加。现在还不用写，等你要做 Body 那条 Pass 再说。

## 214. 你（Wednesday, Sep 16, 2026, 12:16 AM (UTC+8)）

对，现在不开始做；你之前说的大块面，我看教程里面好像就是把原模型给减免，然后从上到下依次膨胀，同时膨胀的区域加GLow效果吗？

### 助手

方向接近，但有两处容易看走眼。

**不是把模型减面。**  
网格三角形数量没变。Shader 在顶点里把位置 **Round 到格子**（`_PolySize`）。很多顶点被吸到同一个格子角上，看起来像低模大块面，其实还是原来那套拓扑。沙滩球那种几块大面，是格子够大，不是 Blender 减面。

**「从上到下依次膨胀」是切片，不是整身缩放。**  
`_SectionMoving` 是一条沿高度走的世界空间带子（再加 `_SegmentWidth` / `_SectionSmoothness`）。带子扫过的地方切到故障外观，没扫到的仍是原贴图。看起来像从下往上（或从上往下）长出来。

你看到的「膨胀」多半是：顶点被吸到格子上时，表面会微微外凸成一块块，像鼓起来。一般 **不是** `position += normal * 膨胀` 那种充气；只是量化带来的外形变钝。

**Glow 加在带子扫过的区域。**  
那一带已经是大块面，再用重心/边宽画出 HDR 亮边。所以观感是：一条光带往下 sweep → 经过的地方变成色块 + 发光线，不是整身一直闪。

接到你的切换上可以更简单：角色 B 站定后，不必真的做减面。Glitch Pass 里量化顶点做出块面，用一条从上到下的 `_Section`（或整身 `_Glitch` 闪一下）乘发光。主 ForwardLit 不动。以后做的时候先只做「站定后扫一条带子」，减面和真膨胀都可以先不做。

## 215. 你（Wednesday, Sep 16, 2026, 12:21 AM (UTC+8)）

OK，那我大概明白了，现在先不做；之后做的时候我再问你； 现在我想再优化一下之前做的角色切换效果；我想除了角色滑入的效果之外我们做的所有效果都稍微延迟一点点；比如说，当我按下箭头键切换角色的时候，除了角色的淡出效果其他效果都先不动，等滑入的角色滑入到一半的时候，再切我挂在Cha脚本里的那些效果；其实就是我想让角色滑入淡入淡出和其他效果有一个层次的错开；

### 助手

把环境主题（天、地、星、云）推迟到角色滑到一半再开始过渡，滑入和淡入淡出仍立刻走。

### 助手

已经错开了。按箭头后，**角色滑动和 `_Fade` 立刻走**；星星、天空盒、地板、Cloud Base Color 会等到滑入进度到一半才开始过渡。

Inspector 里多了 **Theme Delay**（默认 `0.5`）。`0.5` 就是滑到一半再切环境；调成 `0.3` 会更早，`0.7` 更晚。后半段环境过渡仍用 SmoothStep，不会突然跳。

Cloud 的 Start Color 重启还是在滑动结束时，比环境色更靠后一点。要改这个时机再说。

## 216. 你（Wednesday, Sep 16, 2026, 12:24 AM (UTC+8)）

这个延迟的时间可以显示在@Assets/RenderingLab/Cha/Scripts/CharacterSwitcher.cs 这个脚本上吗？

### 助手

把延迟改成 Inspector 上能直接填的秒数，不再用 0–1 比例。

### 助手

可以。`Theme Delay` 现在是 **秒**，挂在 `CharacterSwitcher` 上就能改。

默认 `0.25`：滑动总长 `Duration` 是 `0.5` 秒时，角色先滑 0.25 秒，天空/地板/星星/云再开始过渡。想更晚就加大这个值，最大不要超过 `Duration`。

如果你之前把旧的 0–1 滑条留在 `0.5`，Unity 会把它当成 0.5 秒，环境几乎要滑完才动。看一下 Inspector，需要半程错开就填 `0.25`。

## 217. 你（Wednesday, Sep 16, 2026, 12:30 AM (UTC+8)）

效果已经有了；但是感觉还有一个地方不对；我现在的Duration时间是0.35秒，themeDelay是0.3秒的时候；现在感觉Themes里的切换效果时间是不是只有0.05秒的时间，并不是Duration的0.35时间？

### 助手

现在主题被挤在 `Duration - ThemeDelay` 里（0.35−0.3=0.05 秒）。延迟结束后，主题过渡应单独走满 `Duration`，即使角色已经滑到位。

### 助手

判断对了。之前主题被塞进滑动剩下的时间里：`0.35 - 0.3 = 0.05` 秒，所以会一闪而过。

现在两条时间线分开：

- **Duration 0.35s**：角色滑入 + 淡入淡出
- **Theme Delay 0.3s**：环境先不动
- **再花 Duration 0.35s**：天空 / 地板 / 星星 / 云做完整过渡

总长大约 `0.3 + 0.35 = 0.65` 秒。角色可以先站定，主题继续慢慢换。这段时间里箭头会锁住，避免中途再切。

## 218. 你（Wednesday, Sep 16, 2026, 12:34 AM (UTC+8)）

Themes过度时间让我在脚本上来调整吧；我想要的效果可能是，角色淡入淡出的duration时间是0.35s，然后ThemeDelay0.3s, 然后Theme的过度时间可能是只需要0.3秒或者0.25秒，这个时间我想自己来填；

### 助手

已经拆开了，Inspector 上三个时间各自填：

| 字段 | 作用 |
|---|---|
| **Duration** | 角色滑动 + 淡入淡出 |
| **Theme Delay** | 按下箭头后，环境先停多久 |
| **Theme Duration** | 天空 / 地板 / 星星 / 云交叉过渡要多久 |

例如 Duration `0.35`、Theme Delay `0.3`、Theme Duration `0.25`：角色 0.35 秒站定，环境 0.3 秒后开始换，再花 0.25 秒换完。

## 219. 你（Wednesday, Sep 16, 2026, 12:45 AM (UTC+8)）

OK，效果非常完美；就是还有一个地方可以优化；现在Cloud在切换的时候有点闪，应该是做了一次激活操作；之前是因为切换以后，粒子的颜色没有刷新，所以做了一次激活，现在看来这个方案不是很好；还有其他方案吗？

### 助手

闪是因为 `SetActive` 会把粒子清掉再重生。改成改 `startColor`，并用 `GetParticles` / `SetParticles` 按稳定随机把场上已有粒子立刻重上色，不再开关物体。

### 助手

闪是因为关开 Cloud 会让粒子全部死掉再重生，中间会空一帧。

现在不再激活物体，而是：

1. 改 `Start Color`，新生粒子颜色是对的。
2. `GetParticles` 改场上已有粒子的 `startColor`（用每颗粒子自己的 `randomSeed`，还是两色之间随机），再 `SetParticles` 写回去。
3. 材质 **Base Color** 继续跟着主题 lerp。

云还在原来的位置飘，只是颜色跟着过渡。粒子特别多时主题那零点几秒会每帧刷一遍颜色，一般云层这个量没问题。

## 220. 你（Wednesday, Sep 16, 2026, 1:00 AM (UTC+8)）

Directional Light我给灯光做一个顺时针沿y轴旋转360度，模拟角色在不动光照下的效果；按下1键就开始旋转，旋转时间可以在脚本上填写；

### 助手

已经加在 `CharacterSwitcher` 上。按 **1**，平行光绕世界 **Y 轴顺时针转一圈**，角色原地不动，看起来就像光在转。

Inspector：

- **Directional Light**：可空着，会找名为 `Directional Light` 的物体
- **Light Orbit Duration**：转一圈要多少秒，默认 4

转完会回到起始朝向，避免精度漂一点。转圈途中再按 1 不会叠第二次。左右切角色互不影响。若方向和你想的顺时针反了，说一声改成逆时针即可。

## 221. 你（Wednesday, Sep 16, 2026, 1:03 AM (UTC+8)）

改成逆时针方向吧；

### 助手

已经改成绕世界 Y 轴**逆时针**转一圈。按 1 再看一下方向。

## 222. 你（Wednesday, Sep 16, 2026, 11:26 PM (UTC+8)）

开始研究怎么做那个角色身上的扫光效果，还是按照之前的方法，把整体方案拆分成详细的步骤，你说逻辑和实现方法，以及代码，我来写；

### 助手

先对照你现在的 Body Pass 和切换时机，把扫光拆成你可以按步粘贴的实现顺序。

### 助手

先只做 **Body**。Hair / Face 等这条 Pass 在身上能看见再复制。描边不要加。

整体仍是方案 B：ForwardLit 不动。多一条 Additive Pass，角色 **滑到位之后** 再播。下面按步做，做完一步再说下一步。

---

**第 1 步：空 Pass，确认能叠在身上**

目标：Inspector 里把 `_Glitch` 拉到 1，全身叠一层青色，拉回 0 完全消失。不扫、不量化。

为什么用 `UniversalForwardOnly`：Outline 已经占用 `SRPDefaultUnlit`，URP 对同一 LightMode 往往只认第一条。`UniversalForwardOnly` 在默认 URP 里会画，又不会和 ForwardLit 抢。

Properties 和 **CBUFFER 都要加**（少一个 MPB 写了也不生效）：

```hlsl
_Glitch("Glitch", Range(0, 1)) = 0
[HDR] _GlitchColor("Glitch Color", Color) = (0.3, 1.2, 1.4, 1)
```

```hlsl
float _Glitch;
half4 _GlitchColor;
```

插在 Outline 和 DepthOnly **之间**：

```hlsl
Pass
{
    Name "Glitch"
    Tags { "LightMode" = "UniversalForwardOnly" }
    Cull [_CullMode]
    ZWrite Off
    ZTest LEqual
    Blend One One

    HLSLPROGRAM
    #pragma vertex GlitchVert
    #pragma fragment GlitchFrag
    #include "GenShin_Shared.hlsl"

    struct Attributes
    {
        float4 positionOS : POSITION;
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
    };

    Varyings GlitchVert(Attributes IN)
    {
        Varyings o = (Varyings)0;
        o.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
        return o;
    }

    half4 GlitchFrag(Varyings IN) : SV_TARGET
    {
        CharaDitherClip(IN.positionCS.xy, _Fade);
        return half4(_GlitchColor.rgb * _Glitch, 1);
    }
    ENDHLSL
}
```

`ZWrite Off` + `ZTest LEqual`：贴在已写深度的身体上，不打孔。`Blend One One`：往上加光。

Play 里选中 Body 材质，`_Glitch = 0` 应看不出变化；拉到 1 应整身发青。若完全看不见：Frame Debugger 里找 `Glitch` / `UniversalForwardOnly`。没有这条，再说。

这一步 **先不要改 CharacterSwitcher**，用滑动条确认 Pass 活着。

---

**第 2 步：从上到下一条带子**

顶点把 `positionWS` 传进片元。世界空间 Y 和一条会动的高度比：

```hlsl
float band = 1.0 - saturate(abs(IN.positionWS.y - _GlitchHeight) / max(_GlitchWidth, 1e-4));
band = smoothstep(0.0, 1.0, band);
return half4(_GlitchColor.rgb * _Glitch * band, 1);
```

再加：

```hlsl
_GlitchHeight("Glitch Height", Float) = 0
_GlitchWidth("Glitch Width", Range(0.05, 2)) = 0.35
```

手拖 `_GlitchHeight`（大概从 2 拖到 0，按角色身高来），应看到一条光带往下走，带外仍是正常卡通。带宽用 `_GlitchWidth`。

---

**第 3 步：大块面（仍不减面）**

蒙皮之后（`GetVertexPositionInputs` 拿到的 WS）再量化，只动 Glitch Pass：

```hlsl
float3 SnapWS(float3 p, float size)
{
    size = max(size, 1e-4);
    return round(p / size) * size;
}
```

`o.positionCS = TransformWorldToHClip(SnapWS(pos.positionWS, _PolySize));`

`_PolySize` 从 `0.08` 试到 `0.2`。太大块会破轮廓，太小像渔网。主身体剪影应不变。

---

**第 4 步：块面边缘发光**

量化后的 WS 在面交界处不连续，用 `fwidth` 找跳变（蒙皮网格先不必拆顶点做重心坐标）：

```hlsl
float3 snap = IN.snapWS;
float3 fw = fwidth(snap);
float edge = 1.0 - saturate(min(min(fw.x, fw.y), fw.z) / max(_GlowWidth, 1e-4));
```

更稳的常见写法是对 `frac(snap / _PolySize)` 做边：格子内部 `frac` 在 0～1，靠近 0 或 1 就是边。第 3 步块面对了再调这条。边用 HDR 色，Bloom 会bloom出去。

---

**第 5 步：切完角色再播（脚本，你写）**

`Slide` 里 `characterSettled = true` 之后（B 已在 `_home`）再开协程，只给 **当前角色** 的 Renderer 写 MPB。

`_Glitch`：0→1→0 包络（或一直为 1，只靠高度扫）。  
`_GlitchHeight`：从 `renderer.bounds.max.y` 扫到 `min.y`。  
时长单独一个 `glitchDuration`。  
滑的时候保持 `_Glitch = 0`。  
`ApplyInstant` 开场也要清成 0。

和 `_Fade` 一样用 `PropertyToID("_Glitch")` / `"_GlitchHeight"`。

---

先做第 1 步。青光能叠在 ForwardLit 上、`_Glitch = 0` 能关掉，把现象发我，再做第 2 步带子。

## 223. 你（Wednesday, Sep 16, 2026, 11:54 PM (UTC+8)）

_Glitch = 1以后没有效果，在Frame Debugger里没有看到_Glithch pass，好像也没看到描边的pass，但是描边效果有的

### 助手

Frame Debugger 看不到 Pass，多半是 URP 没画这条 LightMode，或 ShaderGUI 把 Pass 关掉了。先对一下 Body 的 Glitch Pass 和 `GenshinCharacterGui`。

### 助手

Glitch 这条 Pass **没编过**，所以 Frame Debugger 里不会出现。Console 里应有这条 Shader 的 error。你这段里有三处笔误：

```hlsl
Cull [CullMode]          // 应写成 Cull [_CullMode]
#pragme vertex GlitchVert // 应写成 #pragma
CharaditherClip(...)     // 应写成 CharaDitherClip
```

`#pragme` 一错，顶点函数没挂上，整条 Pass 会被丢掉。改完保存，Console 红字应消失，Frame Debugger 里找 **UniversalForwardOnly**（不是 Pass 名 `Glitch`）。

描边同理：URP 按 **LightMode** 列事件，不按 `Name "Outline"`。描边在 Debugger 里叫 **SRPDefaultUnlit**，效果在是因为这条编过、也在画。

改完后再把 `_Glitch` 拉到 1。Debugger 里应多一次 `UniversalForwardOnly`。还没有的话把 Console 报错贴过来。

## 224. 你（Thursday, Sep 17, 2026, 12:25 AM (UTC+8)）

OK，看到了；开始第二步吗？你之前说的第二步我还没有做；

### 助手

开始第 2 步：光从全身变成一条沿高度走的带子。第 1 步的加法 Pass 不动，只改 Glitch 的顶点和片元。

顺手把 `Cull [CullMode]` 改成 `Cull [_CullMode]`，少了下划线 Unity 不会用你的剔除模式。

**逻辑**

片元只知道自己的世界坐标 Y。和一条「光带中心高度」`_GlitchHeight` 比距离，落在 `_GlitchWidth` 里才亮，外面乘 0。拖 Height，带子就在身上上下走。这一步还是手拖 Inspector，先不接脚本。

**Properties + CBUFFER 都加：**

```hlsl
_GlitchHeight("Glitch Height", Float) = 1
_GlitchWidth("Glitch Width", Range(0.05, 2)) = 0.35
```

```hlsl
float _GlitchHeight;
float _GlitchWidth;
```

**Glitch Pass 改成：**

```hlsl
struct Attributes
{
    float4 positionOS : POSITION;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : TEXCOORD0;
};

Varyings GlitchVert(Attributes IN)
{
    Varyings o = (Varyings)0;
    VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
    o.positionCS = pos.positionCS;
    o.positionWS = pos.positionWS;
    return o;
}

half4 GlitchFrag(Varyings IN) : SV_TARGET
{
    CharaDitherClip(IN.positionCS.xy, _Fade);
    float band = 1.0 - saturate(abs(IN.positionWS.y - _GlitchHeight) / max(_GlitchWidth, 1e-4));
    band = smoothstep(0.0, 1.0, band);
    return half4(_GlitchColor.rgb * _Glitch * band, 1);
}
```

`GetVertexPositionInputs` 已经在 HLSLINCLUDE 的 Core.hlsl 里。

**怎么验**

1. `_Glitch = 1`
2. `_GlitchWidth = 0.35`
3. 拖 `_GlitchHeight`：芭芭拉大概从 `2` 拖到 `0`（脚底附近）

应看到一条横带贴在身上走，带外是正常卡通。带宽不对就改 Width。高度范围对不上，把角色放进 Scene 看 Transform Y，或看 Renderer Bounds 的 min/max Y。

这一步对了再说第 3 步量化块面。脚本还是先别动。

## 225. 你（Thursday, Sep 17, 2026, 12:27 AM (UTC+8)）

OK，Glitchpass有效果了；接下来做什么？

### 助手

第 3 步：**只在 Glitch Pass 里量化顶点**，ForwardLit 轮廓不动。带子继续用原来的世界 Y 去扫。

**逻辑**

蒙皮之后得到 `positionWS`，再 `round` 到格子上。很多顶点吸到同一个角，叠加那一层看起来像大块面。三角形数量没变。格子只拿去算 **裁剪空间位置**；扫光高度仍用 **没量化的 Y**，带子才不会一格一格跳。

**Properties + CBUFFER：**

```hlsl
_PolySize("Poly Size", Range(0.02, 0.5)) = 0.12
```

```hlsl
float _PolySize;
```

可以丢在 `GenShin_Shared.hlsl` 里，或直接写在 Pass 里：

```hlsl
float3 SnapWS(float3 p, float size)
{
    size = max(size, 1e-4);
    return round(p / size) * size;
}
```

**顶点改成：**

```hlsl
struct Varyings
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : TEXCOORD0;
};

Varyings GlitchVert(Attributes IN)
{
    Varyings o = (Varyings)0;
    VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
    float3 snapped = SnapWS(pos.positionWS, _PolySize);
    o.positionCS = TransformWorldToHClip(snapped);
    o.positionWS = pos.positionWS;
    return o;
}
```

片元还是第 2 步那条 `band`（用 `IN.positionWS.y`），不要改。

**怎么验**

`_Glitch = 1`，拖 `_GlitchHeight`，带子还在。同时叠加层应发「钝」、一块一块的，主身体（衣服褶皱、脸）保持光滑。

`_PolySize`：`0.08`～`0.2` 先试。太大块会穿出身体；太小看不出量化。

`Cull` 仍写成 `Cull [_CullMode]`。

这一步块面对了，下一步才是用 `fwidth` 画块面发光边。脚本还是先别动。

## 226. 你（Thursday, Sep 17, 2026, 12:37 AM (UTC+8)）

第二步已经实现了；但是我对float band = 1.0 - saturate(abs(IN.positionWS.y - _GlitchHeight) / max(_GlitchWidth, 1e-4));这个算法不是很理解，这个公式是怎么算出来一条横线的；

### 助手

这条带子不是画出来的线，是每个像素自己算「离中心有多远」，近的亮、远的灭，连起来看起来像一条横带。

把身体想成很多点，每个点只有一个世界高度 `y`。`_GlitchHeight` 是带子中心的高度（比如 1.2）。先算竖直距离：

```text
d = abs(y - _GlitchHeight)
```

脚在 `y = 0`、中心在 `1.2`，这个点的 `d = 1.2`。胸口刚好在 `1.2`，`d = 0`。

再除以半宽 `_GlitchWidth`（比如 0.35）：

```text
t = d / Width
```

`t = 0` 在中心，`t = 1` 刚好在带子边缘，`t > 1` 在带子外面。

`saturate(t)` 把大于 1 的都收成 1。再 `1 -` 反过来：

| 位置 | t | band |
|---|---|---|
| 正好在 Height 上 | 0 | **1**（最亮） |
| 离中心 Width 处 | 1 | **0**（灭） |
| 更远 | >1 → saturate 成 1 | **0** |

所有 `band > 0` 的点，高度都落在：

```text
[Height - Width, Height + Width]
```

这是一条沿水平方向铺开、沿 Y 有厚度的环。角色左右的点只要 `y` 接近 Height，都会亮，所以看起来是 **横着的一条光**，不是竖线。

`_GlitchWidth` 是半宽：0.35 大约总共 0.7 米厚。拖 `_GlitchHeight` 就是把这个区间整体上移或下移。

后面的 `smoothstep(0, 1, band)` 只是把从 1 掉到 0 的边修软一点，不改变「一条横带」这件事。

## 227. 你（Thursday, Sep 17, 2026, 9:46 PM (UTC+8)）

y-LGlitchHeight以后，除以max(_GlitchWidth, 1e-4)这个目的是什么？

### 助手

除以 `_GlitchWidth` 是把「差了多少米」换成「占带宽的几成」，这样 Width 才能真正控制带子有多厚。

`abs(y - Height)` 单位是世界空间的米。胸口离中心 0.1 米，脚离中心可能 1.2 米，数字不能直接当亮度用。除掉半宽之后：

- 距离 = Width → 结果是 **1**（刚好在边缘）
- 距离 = 0 → **0**（中心）
- Width 改成 0.2 或 0.5，同一套 `1 - saturate(...)` 仍然是「中心 1、边缘 0」

若不除，`1 - saturate(距离)` 里距离很容易大于 1，带子会瘦成一条几乎看不见的缝，而且和 Inspector 上的 Width **脱钩**。

`max(..., 1e-4)` 是防止 Width 填 0 时除以零。用一个极小的正数代替，Shader 不会炸。

## 228. 你（Thursday, Sep 17, 2026, 9:49 PM (UTC+8)）

如果不除以的话就会导致这条光带在角色身上不同的位置，可能亮度会不一样吗？

### 助手

不会。光带 **挪到头还是挪到脚，中心都一样亮**。公式里只有 `y` 和 `Height` 的差，没有「这是头还是腿」。

中心永远是 `abs(...) = 0` → `band = 1`。不除 Width 时也一样。

身上亮度不一样，是因为 **同一条带子里，有的点离中心近、有的远**。胸口在中心就最亮，往上往下逐渐变暗。这是带子该有的衰减，除不除都这样。

除与不除的差别只有 **衰减有多快、带子有多厚**：

- **除掉 Width**：离中心 `Width` 米处灭掉，厚度你能调。
- **不除**：`saturate` 在距离 ≥ 1 米时就把亮度掐死，等于半宽被写死成 1 米，和滑条无关。头上那条和脚上那条，中心亮度仍然相同。

所以除法是在标定「多厚算一带」，不是在修「身体不同部位亮度不均」。

## 229. 你（Thursday, Sep 17, 2026, 9:51 PM (UTC+8)）

可以用数学公式和简单的画图帮我解释一下吗？

### 助手

这条带子就是一个以 `H` 为中心、半宽为 `W` 的**帐篷函数**：只跟「离中心多远」有关，跟头还是脚无关。

\[
\mathrm{band}(y)=\mathrm{saturate}\!\left(1-\frac{|y-H|}{W}\right)
=\max\!\left(0,\;1-\frac{|y-H|}{W}\right)
\]

其中 \(H=\) `_GlitchHeight`，\(W=\) `_GlitchWidth`。下面用 \(H=1.2\,\mathrm{m}\)、\(W=0.35\,\mathrm{m}\) 画出来。

<div class="vis-section-stack">
  <div class="vis-column">
    <figure class="vis-chart-figure">
      <div class="vis-chart-meta">
        <div class="vis-chart-meta-copy">
          <h3>band vs world y</h3>
          <span>H 1.2 m, W 0.35 m</span>
        </div>
      </div>
      <svg class="vis-chart" role="img" aria-label="Tent function of band versus world height"></svg>
    </figure>
    <script data-runtime="d3"></script>
    <script>
      (() => {
        const root = document.currentScript.previousElementSibling.previousElementSibling;
        const svg = root.querySelector("svg.vis-chart");
        const H = 1.2;
        const W = 0.35;
        const samples = [];
        for (let i = 0; i <= 80; i++) {
          const y = (i / 80) * 2;
          samples.push({ y, band: Math.max(0, 1 - Math.abs(y - H) / W) });
        }
        window.chart.draw(svg, ({ width, height }) => {
          const frame = window.chart.frame(svg, {
            width,
            height,
            xMin: 0,
            xMax: 2,
            yMin: 0,
            yMax: 1,
            xLabel: "world y (m)",
            yLabel: "band",
            xTicks: 5,
            yTicks: 5
          });
          if (!frame) return;
          const line = d3.line()
            .x((d) => frame.x(d.y))
            .y((d) => frame.y(d.band));
          const g = d3.select(svg).append("g");
          g.append("path")
            .attr("class", "vis-line vis-data-series vis-orange")
            .attr("d", line(samples));
          const marks = [
            { x: H - W, label: "H − W" },
            { x: H, label: "H" },
            { x: H + W, label: "H + W" }
          ];
          marks.forEach((m) => {
            g.append("line")
              .attr("class", "vis-reference-guide")
              .attr("x1", frame.x(m.x))
              .attr("x2", frame.x(m.x))
              .attr("y1", frame.y0)
              .attr("y2", frame.y1);
          });
          g.append("text")
            .attr("class", "vis-label vis-chart-text")
            .attr("x", frame.x(H))
            .attr("y", frame.y(1) - 6)
            .attr("text-anchor", "middle")
            .text("1 at center");
        });
      })();
    </script>
  </div>
  <div class="vis-column">
    <div class="vis-chart-meta">
      <div class="vis-chart-meta-copy">
        <h3>Same band on the body</h3>
        <span>Bright only between H − W and H + W</span>
      </div>
      <div class="vis-legend">
        <span class="vis-legend-item"><span class="vis-swatch vis-orange"></span>band</span>
        <span class="vis-legend-item"><span class="vis-swatch vis-fg"></span>body</span>
      </div>
    </div>
    <svg viewBox="0 0 736 260" width="100%" role="img" aria-label="Character side view with a horizontal light band">
      <g fill="none" stroke="var(--border)" stroke-width="1" vector-effect="non-scaling-stroke">
        <line x1="88" y1="24" x2="88" y2="236"/>
      </g>
      <text class="vis-chart-text" x="80" y="28" text-anchor="end">y</text>
      <text class="vis-chart-text" x="80" y="78" text-anchor="end">H+W</text>
      <text class="vis-chart-text" x="80" y="128" text-anchor="end">H</text>
      <text class="vis-chart-text" x="80" y="178" text-anchor="end">H−W</text>
      <text class="vis-chart-text" x="80" y="240" text-anchor="end">feet</text>
      <line x1="88" y1="72" x2="420" y2="72" stroke="var(--border)" stroke-width="1" stroke-dasharray="4 4" vector-effect="non-scaling-stroke"/>
      <line x1="88" y1="172" x2="420" y2="172" stroke="var(--border)" stroke-width="1" stroke-dasharray="4 4" vector-effect="non-scaling-stroke"/>
      <rect x="168" y="72" width="140" height="100" class="vis-mark vis-orange vis-fill-20"/>
      <ellipse cx="238" cy="44" rx="22" ry="24" fill="var(--muted)" stroke="var(--foreground)" stroke-width="1.5" vector-effect="non-scaling-stroke"/>
      <rect x="206" y="70" width="64" height="118" rx="8" fill="var(--card)" stroke="var(--foreground)" stroke-width="1.5" vector-effect="non-scaling-stroke"/>
      <line x1="88" y1="122" x2="420" y2="122" stroke="var(--orange)" stroke-width="2" vector-effect="non-scaling-stroke"/>
      <text class="vis-chart-text" x="448" y="126">center, band = 1</text>
      <text class="vis-chart-text" x="448" y="76">edge, band = 0</text>
      <text class="vis-chart-text" x="448" y="176">edge, band = 0</text>
    </svg>
  </div>
</div>

除以 \(W\) 之后，横轴从「米」变成「占半宽的比例」：

- \(|y-H|=0\) → \(\mathrm{band}=1\)
- \(|y-H|=W\) → \(\mathrm{band}=0\)
- \(|y-H|>W\) → \(\mathrm{saturate}\) 掐成 0

所以亮的像素高度都挤在 \([H-W,\,H+W]\) 这一段里，左右只要 \(y\) 接近 \(H\) 都会亮，看起来就是一条**横带**。把 \(H\) 从 2 拖到 0，只是把帐篷整体沿着角色上下平移，帐篷形状不变。

## 230. 你（Friday, Sep 18, 2026, 8:25 PM (UTC+8)）

好的，这一步完成了，下一步做什么？

### 助手

下一步是 **第 3 步：只在 Glitch Pass 里量化顶点**，让叠加层看起来像大块面。扫光公式不用改。

**逻辑**

蒙皮之后得到 `positionWS`，再 `round` 到格子上。很多顶点吸到同一个角，叠加层变钝；三角形数量没变。  
**裁剪位置**用量化后的点，**扫光的 Y** 仍用原来的 `positionWS.y`，带子才不会一格一格跳。ForwardLit 完全不动。

**Properties + CBUFFER：**

```hlsl
_PolySize("Poly Size", Range(0.02, 0.5)) = 0.12
```

```hlsl
float _PolySize;
```

**顶点改成：**

```hlsl
float3 SnapWS(float3 p, float size)
{
    size = max(size, 1e-4);
    return round(p / size) * size;
}

Varyings GlitchVert(Attributes IN)
{
    Varyings o = (Varyings)0;
    VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
    float3 snapped = SnapWS(pos.positionWS, _PolySize);
    o.positionCS = TransformWorldToHClip(snapped);
    o.positionWS = pos.positionWS;
    return o;
}
```

片元还是现在的 `band`，不要动。

**怎么验**

`_Glitch = 1`，拖 Height：带子还在，叠加层应一块一块的，衣服和脸仍光滑。`_PolySize` 先试 `0.08`～`0.2`。太大块会穿出身体，太小看不出量化。

块面对了再做第 4 步：用 `fwidth` 画块面发光边。脚本还是先别动。

## 231. 你（Friday, Sep 18, 2026, 8:37 PM (UTC+8)）

好像没效果，连光带都不见了；

### 助手

光带消失是因为量化后的点已经是 **世界坐标**，却拿去走了 **物体空间** 的裁剪变换。

你现在写的是：

```hlsl
o.positionCS = TransformObjectToHClip(snapped);
```

`snapped` 来自 `pos.positionWS`，已经在世界里。`TransformObjectToHClip` 会再乘一遍模型矩阵，顶点飞到错误位置，对不上身体写好的深度，`ZTest LEqual` 全部测不过，带子就像没了。

改成：

```hlsl
o.positionCS = TransformWorldToHClip(snapped);
```

ForwardLit 仍然用原来的 `pos.positionCS`，只有 Glitch 这一层用量化后的世界点。

改完若带子只剩碎点、大块变黑：量化会把一部分顶点推到身体 **后面**，深度还是测不过。可以先临时 `ZTest Always` 确认块面在；要贴在身上就改回 `LEqual`，并加：

```hlsl
Offset -1, -1
```

让这一 Pass 略微拉近相机。

`_PolySize` 先用 `0.08` 左右，太大更容易穿到身体后面。先改 `TransformWorldToHClip` 再看。

## 232. 你（Friday, Sep 18, 2026, 8:43 PM (UTC+8)）

OK, 效果看上去正常；

## 233. 你

<available_subagent_types>
Available subagent_types and a quick description of what they do:
- generalPurpose: General-purpose agent for researching complex questions, searching for code, and executing multi-step tasks. Use when searching for a keyword or file and not confident you'll find the match quickly.
- explore: Fast agent specialized for exploring codebases. Use this when you need to quickly find files by patterns (eg. "src/components/**/*.tsx"), search code for keywords (eg. "API endpoints"), or answer questions about the codebase (eg. "how do API endpoints work?"). When calling this agent, specify the desired thoroughness level: "quick" for basic searches, "medium" for moderate exploration, or "very thorough" for comprehensive analysis across multiple locations and naming conventions.
- cursor-guide: Read Cursor product documentation to answer questions about how Cursor Desktop, IDE, CLI, Cloud Agents, Bugbot, and other features work. Use when the user asks 'In Cursor, how do I...?' or similar questions about Cursor products.
- ci-investigator: Investigate a single failing PR CI check and return a short root-cause summary. Use when the user asks to summarize, explain, diagnose, or investigate a specific failed check from a pull request.
- bugbot: Use only when the user *explicitly* asks for a Bugbot-like review of local code changes. When launching this subagent, set the Task description to exactly "Bugbot". Launch exactly one Bugbot subagent with `run_in_background: false` unless the user explicitly asks to run in background. Use this fixed prompt form: "Full Repository Path: ...\nDiff: <one of: \"branch changes\", \"uncommitted changes\", \"natural language\">\nChange Description: ...\nCustom Instructions: ..."; default to `Diff: branch changes`; include `Change Description` only when `Diff` is `natural language`, formatting it as one block per changed file (a `<path> (added|modified|deleted|renamed)` header followed by bullets of what changed, mentioning line numbers or ranges inline where helpful), and only use `natural language` as a last resort after a regular diff-based review failed because the diff could not be computed; include `Custom Instructions` only when the user gave specific review instructions. This subagent is single-shot and does not support `resume`; always launch a fresh subagent instead.
- security-review: Use only when the user *explicitly* asks for a security review of local code changes. When launching this subagent, set the Task description to exactly "Security Review". Launch exactly one security-review subagent with `run_in_background: false` unless the user explicitly asks to run in background. Use this fixed prompt form: "Full Repository Path: ...\nDiff: <one of: \"branch changes\", \"uncommitted changes\">\nCustom Instructions: ..."; default to `Diff: branch changes`; include `Custom Instructions` only when the user gave specific review instructions. This subagent is single-shot and does not support `resume`; always launch a fresh subagent instead.
- best-of-n-runner: Run a task in an isolated git worktree. Each best-of-n-runner gets its own branch and working directory. Use for best-of-N parallel attempts or isolated experiments.
</available_subagent_types>

<available_subagent_models>
If the user explicitly asks for the model of a subagent/task, you may ONLY use model slugs from this list:
- inherit (default; required unless the user explicitly requested another model)
- composer-2.5-fast
- cursor-grok-4.6-high-fast

If the user isn't asking for a specific version, prefer the latest version of the model family. As an example, if the user just says "gpt" or "claude", use the latest available version of GPT or Claude.

IMPORTANT: If the user requests a model that is NOT in the list above, do NOT substitute a different model or guess. Instead, skip launching the subagent with that model and tell the user which model was unavailable and which models are available.

When speaking to the USER about which model you selected for a subagent, do NOT use the kebab-case model names unless the user requested the model using that format. Ue the same naming scheme the user used to discuss the model when they requested it.
</available_subagent_models>

<dynamic_tools>
You have access to tools through dynamic namespaces, e.g. MCP servers, using `GetDynamicTools` and `CallDynamicTool`.

## Dynamic Tool Discovery and Invocation

Use `GetDynamicTools` to discover tool schemas, then `CallDynamicTool` to invoke one tool. Aim to minimize round-trips: ideally one discovery call followed by one invocation.

If the user mentions a product or service represented by an available namespace, and the request likely depends on it, proactively inspect that namespace before answering. If you are unsure which namespace matches, search with a relevant pattern.

`GetDynamicTools` supports these modes:

1. `{"namespace":"<id>"}`: returns schemas and full descriptions for every tool in that namespace.
2. `{"namespace":"<id>","toolName":"<name>"}`: returns one tool schema with its full description.
3. `{"pattern":"<regex>"}`: searches namespace and tool names.
4. `{"namespace":"<id>","pattern":"<regex>"}`: searches tools within one namespace.
5. No arguments: returns the full catalog.

Pattern-search and catalog results shorten long descriptions, marked by a trailing "... [truncated]"; namespace and single-tool lookups always return the complete description.

Always inspect a tool's schema before invoking it with `CallDynamicTool`.

If the available dynamic tools do not fully support what the user asked you to do, complete the work you can with the current tool set. In your work summary, include what you were unable to do and why. Do not use browser automation to work around missing tools unless the user explicitly asks you to use the browser.

Available dynamic tool namespaces:

<dynamic_tool_namespaces>
<namespace name="cursor-ide-browser" tools="browser_navigate, browser_snapshot, browser_click, browser_mouse_click_xy, browser_type, browser_fill, browser_select_option, browser_press_key, browser_scroll, browser_drag, browser_get_bounding_box, browser_highlight, browser_tabs, browser_cdp, browser_take_screenshot, browser_lock" namespaceUseInstructions="The cursor-ide-browser MCP server provides a Cursor-owned browser tab plus a raw Chrome DevTools Protocol command tool.

CORE WORKFLOW:
1. Start by understanding the user's goal and what success looks like on the page.
2. Use browser_tabs with action "list" to inspect open tabs and URLs before acting.
3. Use browser_navigate to create or navigate the target tab. Omit the position parameter for background automation so focus is preserved.
4. Use browser_lock before longer automation on an existing tab, then browser_lock with action "unlock" when finished.
5. Use browser_snapshot for accessibility context and browser_take_screenshot for visual verification.
6. Use browser_click, browser_type, browser_fill, browser_select_option, browser_press_key, browser_scroll, and browser_drag for page interactions.
7. Use browser_highlight and browser_get_bounding_box for visual grounding and coordinate diagnostics.
8. Use browser_cdp for page inspection, profiling, runtime evaluation, DOM/CSS queries, and performance data.

AVOID RABBIT HOLES:
1. Do not repeat the same failing action more than once without new evidence such as a fresh snapshot, a different ref, a changed page state, or a clear new hypothesis.
2. IMPORTANT: If four attempts fail or progress stalls, stop acting and report what you observed, what blocked progress, and the most likely next step.
3. Prefer gathering evidence over brute force. If the page is confusing, use browser_snapshot, browser_take_screenshot, or CDP inspection before trying more actions.
4. If you encounter a blocker such as login, passkey/manual user interaction, permissions, captchas, destructive confirmations, missing data, or an unexpected state, stop and report it instead of improvising repeated actions.
5. Do not get stuck in wait-action-wait loops. Every retry should be justified by something newly observed.

CRITICAL - Lock/unlock workflow:
1. browser_lock requires an existing browser tab - you CANNOT call browser_lock with action: "lock" before browser_navigate
2. Correct order: browser_navigate -> browser_lock({ action: "lock" }) -> (interactions) -> browser_lock({ action: "unlock" })
3. If a browser tab already exists (check with browser_tabs list), call browser_lock with action: "lock" FIRST before any interactions
4. Only call browser_lock with action: "unlock" when completely done with ALL browser operations for this turn

IMPORTANT - Waiting strategy:
When waiting for page changes, prefer short CDP polling loops with Runtime.evaluate, DOM queries, Page lifecycle signals, or browser_snapshot checks rather than a single long wait.

CDP USAGE:
- Use browser_cdp with a DevTools Protocol method and params object, for example Runtime.evaluate, DOM.getDocument, CSS.getComputedStyleForNode, Profiler.start/stop, Performance.getMetrics, Log.enable, and Network.enable.
- Do not use browser_cdp with CDP Input.* methods. They are denied because they are focus-sensitive in Electron webviews and can route input to Cursor UI instead of the browser page.
- Use browser_click, browser_type, browser_fill, browser_select_option, browser_press_key, browser_scroll, and browser_drag for clicks, typing, filling inputs, selecting options, keyboard actions, scrolling, and drag-and-drop.
- Use Runtime.evaluate for advanced DOM-scoped interactions that the dedicated browser tools do not cover.
- For profiling, call Profiler.enable, Profiler.start, reproduce the behavior, then Profiler.stop. The profile is saved to a file and returned as a log_file; read that file only when you need to inspect details.
- For JavaScript evaluation, prefer Runtime.evaluate with returnByValue when possible.
- Some browser-wide or sensitive CDP methods are denied, especially cookie, storage, permission, download, target-management, filesystem-backed file-input commands, system-level commands, and CDP navigation/history navigation commands.
- Large CDP responses are saved to files instead of being inlined. Prefer using the returned file path over immediately stuffing large payloads into context; read focused sections only when needed.

VISION:
- browser_take_screenshot attaches an image result that the model can inspect. CDP Page.captureScreenshot returns data inside JSON and should not replace browser_take_screenshot when visual verification is needed.

NOTES:
- browser_snapshot returns snapshot YAML and is the main source of truth for page structure.
- Refs are opaque handles tied to the latest browser_snapshot for that tab.
- Iframe content is not accessible - only elements outside iframes can be interacted with.
- When you stop to report a blocker, include the current page, the target you were trying to reach, the blocker you observed, and the best next action. If the blocker requires manual user interaction, ask the user to take over at that point rather than assuming it in advance." source="mcp" />
<namespace name="plugin-gmail-gmail" source="mcp" />
<namespace name="plugin-google-drive-google-drive" source="mcp" />
<namespace name="cursor" tools="AwaitShell, CreateGoal, Delete, EditNotebook, FetchMcpResource, GenerateImage, ReadLints, SwitchMode, Task, TodoWrite, UpdateGoal, WebFetch, WebSearch" namespaceUseInstructions="Native Cursor tools for this session. These are highly recommended and useful tools that you should use when the right situation arises. Don't be afraid to look at one if it seems relevant, even if you don't end up using it. You MUST read the tool schemas before calling them.

Here are some crucial instructions:
- AwaitShell: Use to sleep and check shell progress. Never sleep using shell.
- ReadLints: Check for linter errors after substantive edits.
- SwitchMode: Switch between modes: Agent and Plan. Proactively consider switching modes for relevant requests.
- TodoWrite: Use this tool to manage complex multi-step tasks." source="cursor" />
</dynamic_tool_namespaces>

If an MCP-backed namespace requires authentication, call `mcp_auth` through `CallDynamicTool` for that namespace, then inspect it again and retry if appropriate. Do not authenticate namespaces preemptively or repeatedly.
</dynamic_tools>

## 234. 你（Friday, Sep 18, 2026, 8:56 PM (UTC+8)）

我看实例效果图里，那个扫光是有颜色变化的，他那个颜色变化是通过什么实现的？

### 助手

参考图里那种青 / 粉 / 黄一块一块的，**不是扫光带子自己在变色**，也不是贴图。是 **每个量化格子有一个固定的随机色**，带子只是决定哪些格子被照亮。

格子 ID 来自你已经在做的量化：

```text
cell = round(positionWS / PolySize)
```

同一块面上的像素 `cell` 相同，颜色就平涂；隔壁格子 `cell` 差 1，颜色就跳开。扫光从上往下走时，陆续亮起不同格子，看起来就像「光在变色」，其实是不同块面的色露出来了。

MinionsArt 图里那组 **Glitchy Colors Glow**，就是把 Round 后的位置再 `frac` / 打散成 RGB，再乘一个 HDR `_Color` 去喂 Bloom。图里还有 **Lerp between main texture and glitchy glow**：故障色和原贴图混合，不是只用单色叠加。

以后要做成那样，片元里把现在的 `_GlitchColor` 换成「格子色」，例如：

```hlsl
float3 cell = round(IN.positionWS / max(_PolySize, 1e-4));
float3 cellCol = frac(cell * float3(0.1031, 0.11369, 0.13787));
cellCol = lerp(cellCol, _GlitchColor.rgb, 0.25);
return half4(cellCol * _Glitch * band * glow, 1);
```

`frac(cell * 小数)` 把整数格子折进 0～1，当 RGB。系数不要用整数，否则很多格子会撞成同色。

现在的 `_GlitchColor` 是整条带子一个色。先确认亮边和块面对，再加这一段。要做的时候再说，我按你当前片元写完整替换。

## 235. 你（Friday, Sep 18, 2026, 9:50 PM (UTC+8)）

我加上了这段，但是还是没有颜色的变化；

### 助手

先看你现在的 Glitch 片元是怎么写的，再对上为什么格子色没跳出来。

### 助手

原因有两层，代码其实已经跑了，只是**几乎看不出来**。

**1. `lerp` 把格子色冲掉了**  
`cellCol` 只有 0～1，而 `_GlitchColor` 是 HDR。`lerp(格子色, HDR, 0.25)` 等于往一个很亮的单色靠，0.25 的 HDR 就能盖住整段 hash。默认再是白 `(1,1,1)` 还会把颜色洗成灰白。加法混合叠在原色衣服上，更像一条单色扫光。

**2. 只亮了格子边，相邻格子色又太像**  
`frac(cell * 0.1031)` 隔壁格子只差约 0.1，是淡粉彩不是青/品红那种跳色。再乘 `glow`（内部只有 0.12），你看到的基本是细线，细线上那点差别会被加法混合吃掉。

另外颜色是按**没量化的** `positionWS` 算的，和顶点 Snap 后的大面不是同一套格子，也不会出现参考图那种「一块面一个色」。

先临时在 `return` 前加这一行，确认 hash 本身有色块：

```hlsl
return half4(cellCol, 1);
```

若整身已经是马赛克彩块，说明只是被 HDR 和亮边盖住了。然后把顶点 / 片元换成下面这段：颜色跟 Snap 走，hash 用跳得开的那种，`_GlitchColor` 只当亮度。

```hlsl
struct Varyings
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : TEXCOORD0;
    float3 cellCol    : TEXCOORD1;
};

float3 GlitchHash(float3 cell)
{
    float3 p = frac(cell * 0.1031);
    p += dot(p, p.yzx + 33.33);
    return frac((p.xxy + p.yzz) * p.zyx);
}

Varyings GlitchVert(Attributes IN)
{
    Varyings o = (Varyings)0;
    VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
    float3 snapped = SnapWS(pos.positionWS, _PolySize);
    o.positionCS = TransformWorldToHClip(snapped);
    o.positionWS = pos.positionWS;
    o.cellCol = GlitchHash(round(snapped / max(_PolySize, 1e-4)));
    return o;
}

half4 GlitchFrag(Varyings IN) : SV_TARGET
{
    CharaDitherClip(IN.positionCS.xy, _Fade);

    float band = 1.0 - saturate(abs(IN.positionWS.y - _GlitchHeight) / max(_GlitchWidth, 1e-4));
    band = smoothstep(0.0, 1.0, band);

    float3 p = IN.positionWS / max(_PolySize, 1e-4);
    float3 d = min(frac(p), 1.0 - frac(p));
    float grid = min(min(d.x, d.y), d.z);
    float edge = 1.0 - saturate(grid / max(_GlowWidth, 1e-4));
    edge = smoothstep(0.0, 1.0, edge);

    float glow = lerp(0.35, 1.0, edge);
    float amp = max(max(_GlitchColor.r, _GlitchColor.g), _GlitchColor.b);

    return half4(IN.cellCol * amp * _Glitch * band * glow, 1);
}
```

Inspector 里把 `_GlitchColor` 先设回白、Intensity 1～2，不要拉成很亮的单色 HDR。`PolySize` 可以先 0.12～0.2，色块才大。确认色块对了，再把那行 debug `return` 删掉。

## 236. 你（Friday, Sep 18, 2026, 10:01 PM (UTC+8)）

有颜色了，但是hdr还是GlitchColor的颜色，不是算出来的格子上的颜色，我想HDR的GLow效果是格子上算出来的颜色；

### 助手

先看你现在片元怎么乘 `_GlitchColor`，再改成亮度跟 HDR、色相跟格子色。

### 助手

格子色现在是 0～1 的灰粉彩，再整体乘一个亮度，Bloom 阈值以上就变成**一条发白/发灰的光**；你在 Inspector 里把 `_GlitchColor` 拉成带色 HDR 时，眼睛会觉得「辉光还是取色器那一色」。加法 Pass 也不会按格子把某一路 RGB 单独打爆。

辉光要跟格子走：先做成**饱和色**（一格一个纯色相），再用 `_GlitchColor` 的**强度**把这个色抬到 HDR（>1），Bloom 才会按格子色开花。色相不要乘 `_GlitchColor.rgb`。

顶点把 **snapped** 传下去，颜色改到片元里按格子算（不要插值，插值会冲成灰）：

```hlsl
struct Varyings
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : TEXCOORD0;
    float3 snappedWS  : TEXCOORD1;
};

float3 CellHDR(float3 cell)
{
    float h = frac(dot(cell, float3(0.137, 0.269, 0.421)));
    float3 k = float3(1.0, 2.0 / 3.0, 1.0 / 3.0);
    return saturate(abs(frac(h + k) * 6.0 - 3.0) - 1.0);
}

Varyings GlitchVert(Attributes IN)
{
    Varyings o = (Varyings)0;
    VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
    float3 snapped = SnapWS(pos.positionWS, _PolySize);
    o.positionCS = TransformWorldToHClip(snapped);
    o.positionWS = pos.positionWS;
    o.snappedWS = snapped;
    return o;
}

half4 GlitchFrag(Varyings IN) : SV_TARGET
{
    CharaDitherClip(IN.positionCS.xy, _Fade);

    float band = 1.0 - saturate(abs(IN.positionWS.y - _GlitchHeight) / max(_GlitchWidth, 1e-4));
    band = smoothstep(0.0, 1.0, band);

    float3 p = IN.positionWS / max(_PolySize, 1e-4);
    float3 d = min(frac(p), 1.0 - frac(p));
    float grid = min(min(d.x, d.y), d.z);
    float edge = 1.0 - saturate(grid / max(_GlowWidth, 1e-4));
    edge = smoothstep(0.0, 1.0, edge);

    float3 cell = round(IN.snappedWS / max(_PolySize, 1e-4));
    float3 cellCol = CellHDR(cell);

    float glow = lerp(0.35, 1.0, edge);
    float hdr = max(max(_GlitchColor.r, _GlitchColor.g), _GlitchColor.b);

    return half4(cellCol * hdr * _Glitch * band * glow, 1);
}
```

`CellHDR` 是按格子 ID 取色相，饱和度拉满，所以会是青 / 品红 / 黄这种，而不是 `frac` 出来的脏灰。`hdr` 只取 `_GlitchColor` 三个通道里最大的那个数当曝光。

Inspector 里把 **Glitch Color 改成纯白 `(1,1,1)`，只拖 Intensity**（比如 4～8）。不要把取色器调成青或粉，那个色相会被丢掉，只留下亮度。确认 Bloom 开着，格子亮部才会按格子色泛光。

## 237. 你（Friday, Sep 18, 2026, 10:14 PM (UTC+8)）

OK, 基本上实现了，接下来做什么？

### 助手

扫光本体已经齐了，下一步是切角色时自动播，而不是靠 Inspector 手拖。先对一下切换脚本里角色到位的时机。

### 助手

Body 扫光已经齐了。按之前的方案 B，下一步是 **B 滑到 `_home` 之后由脚本播一遍**：`_GlitchHeight` 从上扫到下，`_Glitch` 做淡入淡出。脚本我直接改。

### 助手

Body 这一侧不用再改。已经接到 `CharacterSwitcher` 上了：B 滑到 `_home` 之后自动播一遍扫光。

脚本只推两个数，和你在材质上拖的是同一套：

- `_GlitchHeight`：按角色 Renderer 的世界包围盒，从头顶扫到脚底
- `_Glitch`：前 12% 淡入、中间保持、后 18% 淡出，播完归零

Inspector 上多了 **Glitch Duration**（默认 0.7 秒）。再切角色时不要挡着，中途再切会停掉上一轮。

**先看这几件：**

1. Body 材质把 `_Glitch` 设回 **0**。运行时由 MPB 驱动，留着 1 会一直亮着。
2. 进 Play，切角色，B 到位后应出现从上到下的彩带，然后消失。
3. 现在只有 Body 有 Glitch Pass，头发和脸暂时不会扫。确认 Body 节奏对了再拷。

时长不对就改 `glitchDuration`。带子太窄/太宽仍是材质上的 `_GlitchWidth`。

## 238. 你（Friday, Sep 18, 2026, 10:25 PM (UTC+8)）

我想再优化一下，当我在切换角色的时候，进入的角色移动到位置停止的时候先来一点时间的fade，随着扫光的节奏的同时变成正常的颜色；

### 助手

到位后先淡出、再跟扫光一起露出原色。先看 `_Fade` 和各 Pass 的裁剪是怎么接的，再改脚本并把 shader 要贴的部分给你。

### 助手

到位后先 dither 淡出，再让原色跟着扫光从上露出来。脚本我改；Body 的 ForwardLit 要加一行高度裁剪，否则身体会整块淡入，对不上扫光线。

### 助手

脚本已经按这个时间轴改好了：

1. B **滑入时保持实色**（不再边走边淡入）
2. 停住后 **`glitchPreFade`（默认 0.2s）dither 淡出**
3. 扫光从上到下时，`_Fade` 跟着同一条 `u` 回到 1，原色和扫光一起回来

Inspector 上多了 **Glitch Pre Fade**。

现在整身还是「整块」淡入。要让**扫过的地方才露出原色**，Body 的 ForwardLit 要用高度裁剪（Glitch Pass 不要裁，光带才能压在交界上）。

`GenShin_Shared.hlsl` 里 `CharaDitherClip` 下面加：

```hlsl
void CharaSweepClip(float2 posCS, float fade, float posY, float glitch, float height, float width)
{
    float reveal = 1.0;
    if (glitch > 1e-4)
        reveal = saturate((posY - height) / max(width, 1e-4) + 0.5);
    clip(fade * reveal - InterleavedGradientNoise(posCS, 0) - 1e-4);
}
```

Body `ForwardLit` 片元把原来的 `CharaDitherClip` 换成：

```hlsl
CharaSweepClip(IN.positionCS.xy, _Fade, IN.positionWS.y, _Glitch, _GlitchHeight, _GlitchWidth);
```

Outline / DepthOnly 也要同一行，否则描边和深度还是整身。这两个 Pass 目前没有 `positionWS`，顶点里加：

```hlsl
o.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
```

头发、脸暂时只有 `_Fade`，会整块跟着淡入。看起来分家了再把上面三个 float 和 `CharaSweepClip` 拷过去。先改 Body ForwardLit 就能对上扫光节奏。

## 239. 你（Saturday, Sep 19, 2026, 10:15 AM (UTC+8)）

先分析一下现在的流程：A角色在场景里正常显示，我按下箭头左键，B角色移动f同时ade值从0-1，然后角色fade值又从0到1同时加上Glitch；我想要的是：滑入到正确的位置的时候fade值只有0.4，然后0.4开始结合Glitch效果，从0.4到1；避免出现角色滑入到正确的位置以后fade值是1，又接一个fade值从0-1的效果；这个你能理解我的意思吗？

### 助手

先对一下脚本里现在的 fade / Glitch 时间轴，再按你说的改：到位时停在 0.4，再跟扫光一起从 0.4 拉到 1。

### 助手

能理解。问题就是 **Fade 走了两遍**：到位时已经到 1，扫光又从 0 拉到 1，中间还被我改成先 1→0 再 0→1，所以会闪一下。

**之前实际在做的：**
- 滑入：B 直接 Fade=1（或你记得的 0→1）
- 停住：再 Fade 1→0，然后 0→1 + Glitch

**你要的一条曲线：**
- 滑入：B 的 Fade **0 → 0.4**，停在半透
- 扫光：Fade **0.4 → 1**，和 Glitch 同一段时间，中间不掉回 0

已经按这个改了。A 仍是 1→0 滑出。Inspector 上原来的 Pre Fade 换成 **Glitch Arrive Fade**（默认 0.4），想更实或更虚就改这个数。

## 240. 你（Saturday, Sep 19, 2026, 10:20 AM (UTC+8)）

OK了，这个对了；你帮我把脸和头发的shader 也按身体的逻辑修改一下吧；，这次你可以直接改头发和脸的shader；

### 助手

这次直接改头发和脸，先对上 Body 现在的 Glitch Pass、裁剪和属性。

### 助手

头发和脸会接上和 Body 相同的扫光裁剪、Glitch Pass，以及 Outline / Depth 的世界坐标。

### 助手

头发和脸已经按 Body 同一套接上了。`CharacterSwitcher` 本来就会给子 Renderer 写 `_Fade` / `_Glitch` / `_GlitchHeight`，不用再改脚本。

两边都加了：

- 同样的 Glitch 属性（高度、带宽、PolySize、格子 HDR）
- ForwardLit / Outline / DepthOnly 用 `CharaSweepClip`，扫过的高度才露出原色和描边
- Additive 的 Glitch Pass（量化、亮边、格子色），光带本身仍只按 `_Fade` dither，避免被扫光线裁掉

把头发、脸材质上的 `_Glitch` 留在 **0**。`Poly Size` / `Glow Width` 尽量和 Body 调成一样，格子才对齐。切一次角色，头、发、身体应同一条扫光从上往下露出来。

## 241. 你（Saturday, Sep 19, 2026, 10:24 AM (UTC+8)）

滑入：B 的 Fade 0 → 0.4，停在半透
扫光：Fade 0.4 → 1，和 Glitch 同一段时间，中间不掉回 0； 这里滑入：B 的 Fade 0 → 0.4，改成0-0.2；

### 助手

已经改了：滑入到位时 Fade 是 **0 → 0.2**，扫光再 **0.2 → 1**。Inspector 里 **Glitch Arrive Fade** 默认现在是 0.2；如果场景里还显示 0.4，手动改一下即可。

## 242. 你（Saturday, Sep 19, 2026, 10:27 AM (UTC+8)）

滑入：B 的 Fade 0 → 0.2，停在半透,：这里稍微停个0.2秒，再播放下一步的扫光：Fade 0.4 → 1，和 Glitch 同一段时间，中间不掉回 0

### 助手

到位后会在 Fade **0.2** 停 **0.2 秒**，再开扫光。扫光仍是 **0.2 → 1**（和到位值接上），不会先跳到 0.4。

Inspector 上多了 **Glitch Hold**，默认 0.2。停顿长短改这个。

## 243. 你（Saturday, Sep 19, 2026, 10:31 AM (UTC+8)）

最后，给扫光从上到下的速度加一个节奏变化，开始比较慢，慢慢的加速；

### 助手

扫光高度改成 **ease-in**：开头慢、越往下越快。Fade 跟着同一条曲线，和光带对齐。

Inspector 上多了 **Glitch Ease**，默认 2。1 是匀速，3～4 开头更拖、后半更冲。

## 244. 你（Saturday, Sep 19, 2026, 10:35 AM (UTC+8)）

再帮我加一个功能，按A键顺时针旋转角色，D键逆时钟旋转角色；W键顺时针旋转灯光，S键逆时钟旋转灯光；

### 助手

先看现有按键和灯光绕转是怎么写的，再接上 A/D 转角色、W/S 转灯。

### 助手

已经接到 `CharacterSwitcher` 上，**按住**就会转（绕世界 Y）：

- **A** 角色顺时针，**D** 逆时针  
- **W** 灯顺时针，**S** 逆时针  

速度默认 90°/秒，Inspector 里是 **Character Rotate Speed** / **Light Rotate Speed**。切角色滑动时不转人，避免和滑入抢；灯在按 **1** 自动绕圈时也不用手转。脸部朝向仍由头骨脚本写 `_FaceForwardWS`，跟着转。

## 245. 你（Saturday, Sep 19, 2026, 10:44 AM (UTC+8)）

OK，完美了；这个案例算结束了；

### 助手

这个展示案例就收在这里。

切角色、主题过渡、扫光节奏、头发脸同步，以及 A/D 转人、W/S 转灯都已经能跑通。之后要复盘或加别的场景，直接开新任务就行。

## 246. 你（Saturday, Sep 19, 2026, 10:45 AM (UTC+8)）

可以把这个案例的所有聊天记录导出吗？
