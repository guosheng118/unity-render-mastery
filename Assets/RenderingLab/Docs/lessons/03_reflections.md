# 3. 反射：Probe、Planar、以及 6.3 没有官方 SSR

场景：`03_Reflections`

## 你在编辑器里点哪里

- `ReflectionProbe_Box`：Realtime + Box Projection。URP Asset 上开 Reflection Probe Blending / Box Projection（PC High/Mid）。
- 地面 `Floor` 用 `RenderingLab/WetGround`，`PlanarReflectionPlane` 把镜像相机画进 `_PlanarReflectionTexture`。
- 档位 PC High：Planar 全分辨率；PC Mid：半分辨率；移动档：`_LabPlanarEnabled=0`，只靠 Probe + 高光。

URP 原生 SSR 在 6.3 **还没进正式版**（官方预览对标更后面的版本）。本仓库 `SimpleSsrRendererFeature` 是教学 stub，默认关闭。量产湿地面用 Planar。

## GPU 上实际发生了什么

Probe：把周围烘/拍成立方体贴图，像素用反射向量采样，Box Projection 把盒子映射到室内，避免「反射在无限远」。

Planar：把相机按地面平面镜像，斜裁剪平面（oblique clip），画到 RT，地面 Shader 用屏幕 UV（X 翻转）去采。只对「一张大平面」成立：街道、水面、地板。不能同时服务两张不同朝向的镜子。

## 和绝区零观感的对应

雨后柏油、地砖、地铁站地板 = Planar + 高 Smoothness + Fresnel。角色眼睛和枪金属 = Probe。不要用 SSR 当唯一反射，动作游戏镜头一甩 SSR 全是破洞。

## 换自己模型时的检查表

- 地面能看到角色倒影（PC High）。
- Mobile Low 倒影消失但高光还在，不要变成塑料黑块。
- Probe 盒子包住房间，Box Projection 打开后墙面反射不再「漂」。
- 倒影相机不要把自己再画一遍（`PlanarReflectionPlane` 用 `_inside` 防递归）。
