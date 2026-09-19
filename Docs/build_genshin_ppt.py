# -*- coding: utf-8 -*-
from pptx import Presentation
from pptx.util import Inches, Pt
from pptx.dml.color import RGBColor
from pptx.enum.text import PP_ALIGN
from pptx.enum.shapes import MSO_SHAPE
from pptx.oxml.ns import qn
from lxml import etree
from pathlib import Path

BG = RGBColor(0x12, 0x16, 0x22)
CARD = RGBColor(0x1C, 0x24, 0x38)
ACCENT = RGBColor(0x5B, 0xC0, 0xBE)
GOLD = RGBColor(0xE8, 0xC5, 0x47)
WHITE = RGBColor(0xF4, 0xF1, 0xDE)
MUTED = RGBColor(0xA8, 0xB2, 0xC8)
CODE_BG = RGBColor(0x0E, 0x14, 0x20)

W = Inches(13.333)
H = Inches(7.5)
FONT = "Microsoft YaHei"
CODE_FONT = "Consolas"
TOTAL = 32


def set_run(run, size=16, color=WHITE, bold=False, font=FONT):
    run.font.size = Pt(size)
    run.font.bold = bold
    run.font.color.rgb = color
    run.font.name = font
    rPr = run._r.get_or_add_rPr()
    ea = rPr.find(qn("a:ea"))
    if ea is None:
        ea = etree.SubElement(rPr, qn("a:ea"))
    ea.set("typeface", FONT)


def fill_slide(slide, color=BG):
    fill = slide.background.fill
    fill.solid()
    fill.fore_color.rgb = color


def add_rect(slide, l, t, w, h, color):
    sh = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, l, t, w, h)
    sh.fill.solid()
    sh.fill.fore_color.rgb = color
    sh.line.fill.background()
    sh.shadow.inherit = False
    return sh


def add_round(slide, l, t, w, h, color):
    sh = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, l, t, w, h)
    sh.fill.solid()
    sh.fill.fore_color.rgb = color
    sh.line.fill.background()
    sh.adjustments[0] = 0.08
    sh.shadow.inherit = False
    return sh


def add_text(slide, l, t, w, h, text, size=16, color=WHITE, bold=False, align=PP_ALIGN.LEFT, font=FONT):
    box = slide.shapes.add_textbox(l, t, w, h)
    tf = box.text_frame
    tf.word_wrap = True
    p = tf.paragraphs[0]
    p.alignment = align
    run = p.add_run()
    run.text = text
    set_run(run, size, color, bold, font)
    return box


def add_bar(slide):
    add_rect(slide, Inches(0), Inches(0), Inches(0.12), H, ACCENT)


def add_footer(slide, page):
    add_text(slide, Inches(0.4), Inches(7.15), Inches(10), Inches(0.28),
             "Unity 6.3 URP  ·  RenderingLab  ·  原神风格角色渲染（完整管线）", 11, MUTED)
    add_text(slide, Inches(11.6), Inches(7.15), Inches(1.4), Inches(0.28),
             f"{page} / {TOTAL}", 11, MUTED, align=PP_ALIGN.RIGHT)


def bullets(slide, l, t, w, h, items, size=16, color=WHITE):
    box = slide.shapes.add_textbox(l, t, w, h)
    tf = box.text_frame
    tf.word_wrap = True
    for i, item in enumerate(items):
        p = tf.paragraphs[0] if i == 0 else tf.add_paragraph()
        p.space_after = Pt(7)
        run = p.add_run()
        run.text = "•  " + item
        set_run(run, size, color)
    return box


def code_block(slide, l, t, w, h, code, size=13):
    add_round(slide, l, t, w, h, CODE_BG)
    box = slide.shapes.add_textbox(l + Inches(0.18), t + Inches(0.12), w - Inches(0.3), h - Inches(0.18))
    tf = box.text_frame
    tf.word_wrap = True
    for i, line in enumerate(code.strip("\n").split("\n")):
        p = tf.paragraphs[0] if i == 0 else tf.add_paragraph()
        p.space_after = Pt(2)
        run = p.add_run()
        run.text = line
        set_run(run, size, ACCENT, False, CODE_FONT)
    return box


def title_slide(prs, kicker, title, subtitle):
    slide = prs.slides.add_slide(prs.slide_layouts[6])
    fill_slide(slide)
    add_rect(slide, Inches(0), Inches(0), Inches(0.18), H, ACCENT)
    add_rect(slide, Inches(0), Inches(6.85), W, Inches(0.65), CARD)
    add_text(slide, Inches(0.7), Inches(1.7), Inches(12), Inches(0.4), kicker, 16, GOLD, True)
    add_text(slide, Inches(0.7), Inches(2.2), Inches(12), Inches(1.5), title, 34, WHITE, True)
    add_text(slide, Inches(0.7), Inches(3.9), Inches(12), Inches(1.4), subtitle, 17, MUTED)
    add_text(slide, Inches(0.7), Inches(7.0), Inches(12), Inches(0.3),
             "Unity 6.3 URP 17  ·  Barbara  ·  Body / Hair / Face / Outline / Floor / Volume", 13, MUTED)
    return slide


def section_slide(prs, num, title, desc, page):
    slide = prs.slides.add_slide(prs.slide_layouts[6])
    fill_slide(slide)
    add_bar(slide)
    add_text(slide, Inches(0.7), Inches(2.4), Inches(12), Inches(0.4), num, 16, GOLD, True)
    add_text(slide, Inches(0.7), Inches(2.9), Inches(12), Inches(1.0), title, 32, WHITE, True)
    add_text(slide, Inches(0.7), Inches(4.1), Inches(11), Inches(1.2), desc, 18, MUTED)
    add_footer(slide, page)
    return slide


def content_slide(prs, kicker, title, page):
    slide = prs.slides.add_slide(prs.slide_layouts[6])
    fill_slide(slide)
    add_bar(slide)
    add_text(slide, Inches(0.5), Inches(0.22), Inches(12), Inches(0.3), kicker, 12, GOLD, True)
    add_text(slide, Inches(0.5), Inches(0.48), Inches(12), Inches(0.5), title, 24, WHITE, True)
    add_rect(slide, Inches(0.5), Inches(1.05), Inches(1.4), Inches(0.06), ACCENT)
    add_footer(slide, page)
    return slide


def add_table(slide, l, t, w, h, rows, col_w=None, header=True):
    cols = len(rows[0])
    table_shape = slide.shapes.add_table(len(rows), cols, l, t, w, h)
    table = table_shape.table
    if col_w:
        for i, cw in enumerate(col_w):
            table.columns[i].width = cw
    for r, row in enumerate(rows):
        for c, val in enumerate(row):
            cell = table.cell(r, c)
            cell.text = ""
            p = cell.text_frame.paragraphs[0]
            p.alignment = PP_ALIGN.LEFT
            run = p.add_run()
            run.text = val
            is_header = header and r == 0
            set_run(run, 12 if not is_header else 13, GOLD if is_header else WHITE, is_header)
            fill = cell.fill
            fill.solid()
            fill.fore_color.rgb = CARD if r == 0 else (
                RGBColor(0x16, 0x1C, 0x2C) if r % 2 else RGBColor(0x14, 0x1A, 0x28)
            )
    return table_shape


def cards(slide, items, y=1.3, h=4.9, w=2.95, gap=3.15, x0=0.5):
    for i, (title, desc) in enumerate(items):
        x = Inches(x0 + i * gap)
        add_round(slide, x, Inches(y), Inches(w), Inches(h), CARD)
        add_text(slide, x + Inches(0.18), Inches(y + 0.28), Inches(w - 0.36), Inches(0.7), title, 16, ACCENT, True)
        add_text(slide, x + Inches(0.18), Inches(y + 1.15), Inches(w - 0.36), Inches(h - 1.5), desc, 14, MUTED)


def build():
    prs = Presentation()
    prs.slide_width = W
    prs.slide_height = H

    title_slide(
        prs,
        "RENDERING LAB  /  GENSHIN CHARACTER",
        "原神风格角色渲染\n完整管线笔记",
        "身体 Ramp · 头发金属 · 脸 SDF · 倒插描边 · 平滑法线\n平面倒影 + Cubemap 地板 · URP Volume 后处理",
    )

    s = content_slide(prs, "CONTENTS", "目录", 2)
    cards(s, [
        ("01  架构", "三套独立 Shader\nILM 通道约定\n贴图导入"),
        ("02  身体 / 头发 / 脸", "半兰伯特 Ramp\n两档金属\n水平 SDF"),
        ("03  描边", "SRPDefaultUnlit\n裁剪空间外扩\n平滑法线进切线"),
        ("04  展示场", "平面反射相机\n天空 Cubemap\nBloom / Neutral"),
    ], y=1.35, h=5.1)

    s = content_slide(prs, "01  ARCHITECTURE", "三套 Shader，不要合成一张万能材质", 3)
    bullets(s, Inches(0.5), Inches(1.25), Inches(12.3), Inches(1.5), [
        "角色约定：世界 +X 为脸朝前。灯光用 GetMainLight().direction（指向灯）。",
        "Body / Hair / Face 分开：身体用法线阴影，脸用 SDF，头发还要切开丝高光和头饰金属。",
        "共用 GenShin_Shared.hlsl 与 RenderLab.CharaDebugView。描边 Pass 三套都挂。",
    ], 16)
    add_table(s, Inches(0.5), Inches(3.05), Inches(12.3), Inches(3.5), [
        ["Shader", "路径", "阴影", "高光 / 其它"],
        ["Body", "RenderingLab/Yuanshen/Body", "半兰伯特 → rampU × G", "ilm.r 连续 Matcap；≥阈值再加 Blinn"],
        ["Hair", "RenderingLab/Yuanshen/Hair", "同身体，A 只有两行", "发丝 Blinn×B；金属 step(R)+Matcap"],
        ["Face", "RenderingLab/Genshin/Face", "水平 SDF，不用 NdotL", "无金属；Head Lightmap 分区"],
    ], col_w=[Inches(1.5), Inches(3.5), Inches(3.5), Inches(3.8)])

    s = content_slide(prs, "01  ARCHITECTURE", "贴图各管什么", 4)
    add_table(s, Inches(0.5), Inches(1.25), Inches(12.3), Inches(5.3), [
        ["贴图", "作用", "导入要点"],
        ["Diffuse / Base", "固有色", "sRGB 开"],
        ["LightMap / ILM", "R 高光权重  G 折叠阴影  B 锐度  A 选 Ramp 行", "sRGB 关"],
        ["Ramp 256×20（10 行×2px）", "卡通明暗查表；Unity V=0 在底", "Clamp、Point、关 Mip；防 NPOT 20→16"],
        ["Face SDF", "脸阴影形状（灰度距离场）", "sRGB 关，Clamp、Point、关 Mip"],
        ["Head Lightmap TGA", "R 皮肤吃 SDF；A 眼睛常亮；B=AO", "sRGB 关"],
        ["MetalMap", "共用 Matcap，用视角法线 XY 采样", "不是按角色 UV 画的遮罩"],
        ["Diffuse_A / Emission", "书本神之眼等自发光遮罩", "sRGB 关；加算，不乘 Ramp"],
    ], col_w=[Inches(3.6), Inches(5.0), Inches(3.7)])

    s = content_slide(prs, "01  ARCHITECTURE", "同一张 LightMap，三套语义不要混用", 5)
    add_table(s, Inches(0.5), Inches(1.25), Inches(12.3), Inches(5.3), [
        ["通道", "身体 Body", "头发 Hair", "脸 Head Lightmap"],
        ["R", "Matcap 连续权重；≥阈值再加 Blinn", "中间灰=发丝区；≥0.9 白=金属", "皮肤：白吃 SDF，黑不吃"],
        ["G", "折叠阴影，128=全受光，*2", "同左", "几乎全白，忽略"],
        ["B", "Blinn 指数，越大越尖", "发丝高光形状", "AO（鼻底），当前未乘进 final"],
        ["A", "Ramp 行 round(a*2) → 0/1/2", "Ramp 行 round(a)→0/1，不要当身体 *2 用", "眼睛常亮"],
    ], col_w=[Inches(1.4), Inches(3.7), Inches(3.8), Inches(3.4)])

    s = content_slide(prs, "02  BODY", "半兰伯特 → 阈值 → Ramp 的 U", 6)
    bullets(s, Inches(0.5), Inches(1.25), Inches(6.2), Inches(5.3), [
        "N·L 做成 0~1 半兰伯特，暗部不会死黑。",
        "smoothstep(阈值±平滑) 得到 rampU：0 暗、1 亮。",
        "再乘 G：褶皱、腋下可强制进暗。",
        "贴图里 128 = 正常受光，所以 saturate(g*2) 把 128 拉成 1。",
        "漫反射：albedo × ramp × 灯光色。",
    ], 16)
    code_block(s, Inches(6.9), Inches(1.3), Inches(5.9), Inches(5.3), """
half foldShadow = saturate(ilm.g * 2);
half NdotL = saturate(dot(N, L) * 0.5 + 0.5);

float rampU = smoothstep(
    _ShadowThreshold - _ShadowSmooth,
    _ShadowThreshold + _ShadowSmooth,
    NdotL);
rampU *= foldShadow;
""", 14)

    s = content_slide(prs, "02  BODY", "A 通道选 Ramp 行，昼夜上下各 5 行", 7)
    bullets(s, Inches(0.5), Inches(1.25), Inches(6.2), Inches(5.3), [
        "Ramp 10 行：白天 0~4，夜晚 5~9。V=0 是底部。",
        "身体 A 三档灰：round(a*2) → 行号 0/1/2。",
        "必须 LOD 0。Mip 会把相邻行混色。",
        "高度 20 若被 NPOT 缩成 16，行会串——Point + 关 Mip。",
        "_IsNight 目前是材质开关，不是全局 TOD。",
    ], 16)
    code_block(s, Inches(6.9), Inches(1.3), Inches(5.9), Inches(5.3), """
float row = round(ilm.a * 2.0);
row = clamp(row, 0.0, 2.0);
float night = step(0.5, _IsNight);
float rampV = 1.0
    - (night * 5.0 + row + 0.5) / 10.0;

half3 ramp = SAMPLE_TEXTURE2D(
    _RampMap, sampler_RampMap,
    float2(rampU, rampV)).rgb;
""", 14)

    s = content_slide(prs, "02  BODY", "两档金属：灰用 Matcap，金再用 Blinn", 8)
    bullets(s, Inches(0.5), Inches(1.22), Inches(12.3), Inches(1.7), [
        "ilm.r 当连续权重：鞋头等灰金属也有 Matcap，黑布没有。",
        "hardMetal = step(阈值, ilm.r)（约 0.9）只给金饰/书页额外 NdotH。",
        "Matcap UV = 视角法线 XY * 0.5 + 0.5，不是模型 UV。",
    ], 15)
    code_block(s, Inches(0.5), Inches(3.15), Inches(12.3), Inches(3.45), """
float2 matcapUV = TransformWorldToViewDir(N, true).xy * 0.5 + 0.5;
half3 matcapCol = albedo * MetalMap(matcapUV) * ilm.r * intensity * light;

half hardMetal = step(_MetalThreshold, ilm.r);
float spec = pow(NdotH, lerp(8, 128, ilm.b)) * hardMetal * rampU;

final = albedo * ramp * light + matcapCol + specCol + rimCol + emission;
""", 14)

    s = content_slide(prs, "02  BODY", "深度边缘光 + 自发光", 9)
    bullets(s, Inches(0.5), Inches(1.22), Inches(6.2), Inches(2.0), [
        "沿视空间法线 XY 偏移采样深度，差分为 Rim，只乘 (1-rampU)。",
        "Emission 用 Diffuse_A 小白点（书本水神印），加算，不乘 Ramp。",
    ], 15)
    code_block(s, Inches(0.5), Inches(3.4), Inches(6.2), Inches(3.2), """
offsetUV = nVS.xy * _RimWidth / _ScreenParams.xy;
depthRim = saturate(depthQ - depthP) * _RimScale;
rimMask  = depthRim * (1 - rampU);
""", 13)
    code_block(s, Inches(6.9), Inches(1.3), Inches(5.9), Inches(5.3), """
half emitMask = SAMPLE(_EmissionMap).r;
half3 emission = albedo * emitMask
               * _EmissionCol * _EmissionIntensity;

finalColor += emission;   // 加，不是乘
""", 14)

    section_slide(prs, "03", "头发 Hair",
                  "Ramp 几乎照搬身体，但 A 只有两档。头饰金属用 R 的高阈值，不能把头发的灰当成金属。", 10)

    s = content_slide(prs, "03  HAIR", "头发 Ramp 行：两档，不要按身体 *2 去打空白行", 11)
    bullets(s, Inches(0.5), Inches(1.25), Inches(12.3), Inches(2.0), [
        "A 是 0 或 255。clamp(round(a*2), 0, 1) 或 round(a) 都可以，目标是只打 0/1 两行。",
        "按身体 round(a*2) 且不 clamp 到 1，会打到空行。",
        "Kajiya 各向异性试过又丢掉：UV 岛不是沿发丝走 U。",
    ], 16)
    code_block(s, Inches(0.5), Inches(3.5), Inches(12.3), Inches(3.1), """
float row = round(ilm.a * 2.0);
row = clamp(row, 0.0, 1.0);   // 头发只有两行
float rampV = 1.0 - (night * 5.0 + row + 0.5) / 10.0;
""", 16)

    s = content_slide(prs, "03  HAIR", "发丝 Blinn + 头饰 Matcap，R 分三档", 12)
    add_table(s, Inches(0.5), Inches(1.22), Inches(12.3), Inches(2.15), [
        ["ilm.r", "区域", "怎么用"],
        ["接近 1", "头饰金属", "step(阈值) → MetalMap"],
        ["中间灰", "头发", "不是金属；高光走 ilm.b"],
        ["接近 0", "头饰布/塑料", "两套高光都不要"],
    ], col_w=[Inches(2.4), Inches(3.2), Inches(6.7)])
    code_block(s, Inches(0.5), Inches(3.6), Inches(12.3), Inches(3.0), """
half metalMask = step(_MetalThreshold, ilm.r);
float hairSpec = pow(NdotH, exp) * ilm.b * (1.0 - metalMask);
half metal = MetalMap(viewNormal.xy * 0.5 + 0.5).r * metalMask;
specCol = (hairSpec + metal) * color * intensity * rampU * light;
""", 14)

    s = content_slide(prs, "04  FACE", "脸不用 NdotL：阴影形状画在 SDF 上", 13)
    bullets(s, Inches(0.5), Inches(1.25), Inches(12.3), Inches(5.4), [
        "脸颊、鼻子的法线会让顶光/侧光算出写实暗部，和卡通脸对不上。",
        "美术把「灯从左侧来时长什么样」画进一张灰度 SDF。",
        "Shader 只问水平面里：灯偏前还是偏后？偏左还是偏右？",
        "前/后 → 提高或降低比较阈值（正前全亮，正后全暗）。",
        "左/右 → 翻转 UV.x，一张图当两张用。翻面发生在正前/正后。",
        "绕世界 Y 转灯 = 改 L.xz，SDF 跟着走。",
    ], 17)

    s = content_slide(prs, "04  FACE", "只取灯光水平方向，并安全归一化", 14)
    bullets(s, Inches(0.5), Inches(1.25), Inches(6.2), Inches(5.3), [
        "丢掉 Y 后 L.xz 长度 < 1，要再归一化才能和脸朝前点积。",
        "顶光 xz≈0：不能除零，fallback 成正前 (1,0) → 不画假侧影。",
        "朝右不要用骨头 forward.xz（压平后不正交）。",
        "Rxz = (-Fxz.y, Fxz.x)，在 XZ 里把朝前旋 90°。",
    ], 15)
    code_block(s, Inches(6.9), Inches(1.3), Inches(5.9), Inches(5.3), """
float2 Lxz = L.xz;
float lxzLen = length(Lxz);
Lxz = lxzLen > 1e-4 ? Lxz / lxzLen
                    : float2(1.0, 0.0);

float2 Fxz = normalize(_FaceForwardWS.xz);
float2 Rxz = float2(-Fxz.y, Fxz.x);
""", 14)

    s = content_slide(prs, "04  FACE", "阈值比较 + 左右翻 UV", 15)
    bullets(s, Inches(0.5), Inches(1.22), Inches(6.2), Inches(2.15), [
        "front = 1 正前、0 正侧、-1 正后。",
        "threshold = 0.5 - front*0.5 → 正前 0（全亮），正后 1（全暗）。",
        "step(L·R) 决定是否镜像 UV。",
    ], 15)
    code_block(s, Inches(0.5), Inches(3.55), Inches(12.3), Inches(3.05), """
float front = dot(Lxz, Fxz);
float threshold = saturate(0.5 - front * 0.5);
float isRight = step(0.0, dot(Lxz, Rxz));
sdfUV.x = lerp(1.0 - uv.x, uv.x, isRight);
float faceShadow = smoothstep(threshold ± smooth, sdf);
""", 14)

    s = content_slide(prs, "04  FACE", "SDF 要跟 Bip001 Head 转，不能写死世界 +X", 16)
    bullets(s, Inches(0.5), Inches(1.22), Inches(12.3), Inches(2.15), [
        "SkinnedMesh 的物体矩阵是角色根。头转了，TransformObjectToWorldDir 也不会变。",
        "FaceHeadOrientation：LateUpdate 读头骨，PropertyBlock 写 _FaceForwardWS。",
        "Max 头骨局部 X 常沿脖子朝上。压平 Y 后再用 right，水平分量≈0，会永远 fallback 成世界 +X。",
        "AutoHorizontal：每帧选水平分量最大的轴（含正负）。",
    ], 15)
    code_block(s, Inches(0.5), Inches(3.55), Inches(12.3), Inches(3.05), """
Vector3 fwd = FlattenYaw(GetFaceForwardWorld());
block.SetVector("_FaceForwardWS", fwd);   // 所有脸 Renderer

float2 Rxz = float2(-Fxz.y, Fxz.x);       // 由朝前推朝右
""", 14)

    s = content_slide(prs, "04  FACE", "Head Lightmap：先 R 再 A", 17)
    bullets(s, Inches(0.5), Inches(1.22), Inches(6.2), Inches(2.0), [
        "必须先 R 再 A：眼睛若落在皮肤白区，最后用 A 打掉阴影。",
        "B 是 AO，工程里 Debug 能看，finalColor 尚未乘 headilm.b。",
        "G 几乎全白，不用。脸没有 Ramp / 夜 / 深度 Rim。",
    ], 15)
    code_block(s, Inches(0.5), Inches(3.5), Inches(12.3), Inches(3.1), """
faceShadow = lerp(1.0, faceShadow, headilm.r);  // 非皮肤不受 SDF
faceShadow = lerp(faceShadow, 1.0, headilm.a);  // 眼睛常亮
shadowLit  = lerp(_FaceShadowColor, 1, faceShadow);
finalColor = albedo * shadowLit * light;        // 未乘 headilm.b
""", 14)

    section_slide(prs, "05", "描边 Outline",
                  "倒插外壳 + 裁剪空间等像素外扩。URP 默认不画自定义 LightMode，要用 SRPDefaultUnlit，而且它画在 Forward 之前。", 18)

    s = content_slide(prs, "05  OUTLINE", "倒插外壳：Cull Front，LightMode 必须能被 URP 画到", 19)
    bullets(s, Inches(0.5), Inches(1.22), Inches(12.3), Inches(2.3), [
        "顶点沿法线外扩，Cull Front 只留下轮廓。内缝会被 Forward 盖住。",
        "Tags { LightMode = Outline } 默认 URP 不画。要用 SRPDefaultUnlit。",
        "URP 把 Unlit 画在 UniversalForward 之前，所以 Pass 写在文件后面也没关系。",
        "颜色：lerp(outlineColor, albedo * outlineColor, 0.5)。脸比身体细。",
    ], 15)
    code_block(s, Inches(0.5), Inches(3.7), Inches(12.3), Inches(2.85), """
Pass {
    Name "Outline"
    Tags { "LightMode" = "SRPDefaultUnlit" }
    Cull Front
}
""", 15)

    s = content_slide(prs, "05  OUTLINE", "外扩要过投影矩阵的 2×2，再除屏幕像素", 20)
    bullets(s, Inches(0.5), Inches(1.22), Inches(6.2), Inches(2.15), [
        "只拿视空间法线 XY 会受宽高比影响，胶囊左右粗细不均。",
        "clipDir = P 的 2×2 × nVS.xy，再 normalize，乘 width * w * 2 / screen。",
        "normalVS.z = -0.5 那种修正，只用 XY 之后不再需要。",
    ], 15)
    code_block(s, Inches(0.5), Inches(3.55), Inches(12.3), Inches(3.05), """
float2 clipDir = mul((float2x2)UNITY_MATRIX_P, normalVS.xy);
clipDir = length(clipDir) > 1e-5 ? normalize(clipDir) : float2(1, 0);
posCS.xy += clipDir * width * posCS.w * 2.0 / _ScreenParams.xy;
""", 14)

    s = content_slide(prs, "05  OUTLINE", "平滑法线必须进 TANGENT，UV2 不会被蒙皮", 21)
    bullets(s, Inches(0.5), Inches(1.22), Inches(12.3), Inches(2.3), [
        "硬法线给光照（布料折边），平滑法线给挤轮廓。导入时按位置平均法线。",
        "Unity 蒙皮只变 POSITION / NORMAL / TANGENT。UV 只插值，动作一弯描边就裂。",
        "UV 还经常存成 float2，法线 Z 会丢。TANGENT 是 float4 且跟着骨骼。",
        "SmoothOutlineNormalPostprocessor 写 TANGENT.xyz，保留 w。菜单：Reimport Models。",
        "没有 Normal Map 时可以占切线。头发当前 Blinn 也不依赖真正的 TBN。",
    ], 15)
    code_block(s, Inches(0.5), Inches(3.75), Inches(12.3), Inches(2.8), """
float3 n = (dot(smooth, smooth) > 0.01) ? smooth : normalOS;
// OutlineAttributes: float4 tangentOS : TANGENT;
GenshinOutlinePositionCS(pos, normalOS, tangentOS.xyz, width);
""", 14)

    s = content_slide(prs, "05  OUTLINE", "以后要上 Normal Map：切线留给 TBN，平滑法线改存切线空间", 22)
    bullets(s, Inches(0.5), Inches(1.25), Inches(12.3), Inches(5.4), [
        "物体空间平滑法线直接写进 TANGENT，和法线贴图抢通道。",
        "替代：绑定姿势把 smooth 点到 TBN 上，塞 UV2；动画后用蒙皮过的 TBN 重建。",
        "道理和法线贴图一样：数据停在切线空间，TBN 一转，方向跟着走。UV 本身不用蒙皮。",
        "静止、均匀缩放时，和直接写切线肉眼几乎一样；非均匀缩放 / TBN 重建不一致才会略歪。",
        "现在这套角色没有 Normal Map，继续直接用切线更干净。",
    ], 16)

    section_slide(prs, "06", "展示场：倒影不是 SSR",
                  "角色用镜像相机再画一遍；天空用 Cubemap 反射向量。粒子云以后走平面 RT，火星不要进反射相机。", 23)

    s = content_slide(prs, "06  SHOWCASE", "平面倒影：隐藏相机 + 斜裁剪 + 反转剔除", 24)
    bullets(s, Inches(0.5), Inches(1.22), Inches(12.3), Inches(2.4), [
        "PlanarReflection 挂在地板上。相机 HideAndDontSave，enabled=false，主相机 Begin 时手动 Render。",
        "worldToCamera = main.worldToCamera × 反射矩阵。斜近裁剪切掉镜子背面。",
        "GL.invertCulling：镜像后绕组反了，正向 Cull Back 和描边 Cull Front 都还正确。",
        "清成透明黑，只留下角色（含描边）。地板自己渲染时关掉，避免套娃。",
        "地板用屏幕 UV 采样 RT：点在平面上时，主相机和镜像相机投到同一像素。",
    ], 15)
    code_block(s, Inches(0.5), Inches(3.85), Inches(12.3), Inches(2.7), """
dest.worldToCameraMatrix = src.worldToCameraMatrix * reflection;
dest.projectionMatrix    = src.CalculateObliqueMatrix(clipPlane);
GL.invertCulling = true;  dest.Render();  GL.invertCulling = false;
""", 14)

    s = content_slide(prs, "06  SHOWCASE", "天空是 Cubemap，不是 SSR；地平线亮带能藏接缝", 25)
    bullets(s, Inches(0.5), Inches(1.22), Inches(12.3), Inches(2.5), [
        "水平镜采的是天上那一半：R = reflect(-V, +Y)。近处地板采天顶（暗+星），擦边采赤道（亮青）。",
        "屏幕下方暗，是倒过来的天顶，不是 cubemap 下半球涂黑。星点能和天上对称，就是这个。",
        "SSR 会把前景粒子也糊进地板，出屏就缺。展示界面要绕人转，用平面反射才稳。",
        "面片要铺过地平线，不要把网格边裁在红线上。Fresnel 擦边→1，亮青接上天空盒。",
        "飘动的云以后进同一张平面 RT（Alpha Blend）；加法粒子 alpha=0 会被当前 mask 乘没。",
    ], 15)
    code_block(s, Inches(0.5), Inches(3.95), Inches(12.3), Inches(2.6), """
float3 R = reflect(-V, N);
half3 sky = SampleSkyCubemap(R, _SkyMip);
col = lerp(_FloorColor, sky, fresnel);
col = lerp(col, planar.rgb * tint, planar.a * fade);
""", 14)

    s = content_slide(prs, "07  POST", "URP Volume：Bloom + Neutral，不要 ACES", 26)
    bullets(s, Inches(0.5), Inches(1.25), Inches(12.3), Inches(1.6), [
        "相机已开 Post Processing，全局 Volume 已在场景里。当前 Tonemapping Mode=None，等于没做色调映射。",
        "后处理吃 HDR>1：书本 emission、金属、粒子才会进 Bloom；Ramp 里的衣服是 0~1。",
    ], 15)
    add_table(s, Inches(0.5), Inches(3.15), Inches(12.3), Inches(3.4), [
        ["Override", "作用", "建议"],
        ["Bloom", "发光点晕开", "Threshold 0.9~1.2，Intensity 0.25~0.6"],
        ["Tonemapping", "HDR 收进显示器", "Neutral；ACES 会把二次元压脏"],
        ["Color Adjustments", "对比 / 饱和 / 曝光", "Contrast、Saturation 约 10~20"],
        ["Vignette", "可选压四角", "Intensity ≤ 0.15"],
        ["Depth of Field", "全身展示不要", "关"],
    ], col_w=[Inches(3.2), Inches(4.2), Inches(4.9)])

    s = content_slide(prs, "WORKFLOW", "先 Debug 通道，再接算法", 27)
    add_table(s, Inches(0.5), Inches(1.25), Inches(12.3), Inches(5.3), [
        ["DebugView", "应该看到", "不对时查"],
        ["ilmR/G/B/A", "身体/头发通道各管各的", "sRGB、接错贴图"],
        ["rampU / ramp", "亮暗分界、行颜色", "Point、Mip、行号 *2"],
        ["SDF", "侧光才有鼻子/脸颊形", "阈值被推到 0 或 1"],
        ["headilmR / A / B", "脸白、眼白、鼻底暗", "TGA 通道拆反"],
        ["specular", "头发灰区发丝；头饰白点金属", "Metal 阈值太低吃到灰发"],
        ["tangentWS", "平滑描边法线（烘进切线后）", "没 Reimport / 仍用 UV2"],
    ], col_w=[Inches(3.0), Inches(4.4), Inches(4.9)])

    s = content_slide(prs, "PITFALLS", "做过的坑", 28)
    items = [
        ("URP 后处理", "Renderer 没带 PostProcessData，相机 PP 全灭。"),
        ("Depth Priming Forced", "没 DepthOnly 时角色消失；和 MSAA 也冲突。"),
        ("Ramp NPOT", "高度 20 缩成 16。Clamp + Point + 关 Mip，LOD 0。"),
        ("脸写成 dot(F,R)", "灯没参与，转灯 SDF 不动。必须 dot(Lxz,Fxz)。"),
        ("头骨 right 压平 Y", "X 沿脖子朝上时水平分量为 0，阴影钉死。"),
        ("头发 A 当身体 *2", "打到空白行。头发只有 0/1 两档。"),
        ("ilm.r 当连续金属", "灰头发变铬。硬金属必须 step(0.9)。"),
        ("LightMode=Outline", "URP 不画。改 SRPDefaultUnlit。"),
        ("描边混了宽高比", "要用 P 的 2×2，再 / _ScreenParams。"),
        ("平滑法线进 UV2", "不蒙皮，Z 还可能丢。写 TANGENT。"),
        ("展示当场当 SSR", "粒子会进地板、出屏会缺。角色走平面相机。"),
        ("网格边裁在地平线", "镜头一动接缝就露。面片要铺过视平线。"),
    ]
    for i, (h, d) in enumerate(items):
        col, row = i % 3, i // 3
        x = Inches(0.45 + col * 4.25)
        y = Inches(1.22 + row * 1.4)
        add_round(s, x, y, Inches(4.1), Inches(1.28), CARD)
        add_text(s, x + Inches(0.14), y + Inches(0.1), Inches(3.82), Inches(0.32), h, 13, GOLD, True)
        add_text(s, x + Inches(0.14), y + Inches(0.44), Inches(3.82), Inches(0.74), d, 12, MUTED)

    s = content_slide(prs, "INDEX", "工程文件", 29)
    add_table(s, Inches(0.5), Inches(1.22), Inches(12.3), Inches(5.35), [
        ["文件", "职责"],
        ["Yuanshen/Shader/Genshin_Body.shader", "Ramp / ILM / Matcap+Blinn / 深度 Rim / Emission / Outline"],
        ["Yuanshen/Shader/Genshin_Hair.shader", "两行 Ramp / 发丝高光 / 金属 Matcap / Outline"],
        ["Yuanshen/Shader/Genshin_Face.shader", "SDF + 头朝向 + Head Lightmap / Outline"],
        ["Yuanshen/Shader/Genshin_Outline.hlsl", "裁剪空间外扩；优先用切线里的平滑法线"],
        ["Yuanshen/Shader/Genshin_Floor.shader", "Cubemap 天空 + 平面 RT 角色倒影"],
        ["Cha/Scripts/FaceHeadOrientation.cs", "Bip001 Head → _FaceForwardWS"],
        ["Cha/Scripts/PlanarReflection.cs", "镜像相机、斜裁剪、全局 _PlanarReflectionTex"],
        ["Cha/Scripts/Editor/SmoothOutlineNormalBaker.cs", "导入时平均法线写入 TANGENT"],
    ], col_w=[Inches(6.5), Inches(5.8)])

    s = content_slide(prs, "STATUS", "已经能跑 vs 还没做", 30)
    add_round(s, Inches(0.5), Inches(1.3), Inches(6.05), Inches(5.2), CARD)
    add_text(s, Inches(0.75), Inches(1.5), Inches(5.6), Inches(0.4), "已完成", 16, ACCENT, True)
    bullets(s, Inches(0.75), Inches(2.05), Inches(5.55), Inches(4.2), [
        "Body / Hair / Face 正向光照",
        "倒插描边 + 切线平滑法线",
        "平面角色倒影 + Cubemap 地板",
        "Debug 枚举、头朝向、灯光 XZ 辅助",
    ], 15)
    add_round(s, Inches(6.75), Inches(1.3), Inches(6.05), Inches(5.2), CARD)
    add_text(s, Inches(7.0), Inches(1.5), Inches(5.6), Inches(0.4), "待补", 16, GOLD, True)
    bullets(s, Inches(7.0), Inches(2.05), Inches(5.55), Inches(4.2), [
        "脸 finalColor × headilm.b（AO）",
        "Volume：Bloom + Neutral + 调色",
        "飘动云进平面 RT（层 + Alpha）",
        "脸深度 Rim / 脸 Ramp 夜（可选）",
        "有 Normal Map 时平滑法线改 UV2",
    ], 15)

    s = prs.slides.add_slide(prs.slide_layouts[6])
    fill_slide(s)
    add_rect(s, Inches(0), Inches(0), Inches(0.18), H, ACCENT)
    add_text(s, Inches(0.7), Inches(2.15), Inches(12), Inches(0.35), "REBUILD", 14, GOLD, True)
    add_text(s, Inches(0.7), Inches(2.6), Inches(12), Inches(0.9), "改完 Shader 后重新导出", 28, WHITE, True)
    bullets(s, Inches(0.7), Inches(3.7), Inches(11.5), Inches(2.4), [
        "python Docs/build_genshin_ppt.py",
        "输出：Docs/GenshinCharacterShading.pptx",
        "这份覆盖到展示场倒影为止；粒子云和自定义后处理 Shader 还没写进代码，只写在待补页。",
    ], 17)
    add_footer(s, 31)

    s = prs.slides.add_slide(prs.slide_layouts[6])
    fill_slide(s)
    add_rect(s, Inches(0), Inches(0), Inches(0.18), H, ACCENT)
    add_rect(s, Inches(0), Inches(6.85), W, Inches(0.65), CARD)
    add_text(s, Inches(0.7), Inches(2.5), Inches(12), Inches(0.4), "END", 14, GOLD, True)
    add_text(s, Inches(0.7), Inches(3.0), Inches(12), Inches(1.2), "角色 Shader 这一课可以收口了", 30, WHITE, True)
    add_text(s, Inches(0.7), Inches(4.4), Inches(12), Inches(0.8),
             "光照在三套材质里；轮廓靠切线里的平滑法线；\n展示场把天和人分成 Cubemap 与平面相机两条路。", 16, MUTED)
    add_text(s, Inches(0.7), Inches(7.0), Inches(12), Inches(0.3), "32 / 32", 13, MUTED)
    add_footer(s, 32)

    out = Path(__file__).resolve().parent / "GenshinCharacterShading.pptx"
    prs.save(str(out))
    print(out)


if __name__ == "__main__":
    build()
