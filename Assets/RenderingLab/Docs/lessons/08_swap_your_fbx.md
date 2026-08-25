# 8. 作业：换成你的 FBX

## 步骤

1. 把角色 FBX 拖进 `Assets/RenderingLab/Art/YourCharacter/`（自己建文件夹）。
2. Import：Rig 按你的项目；Normals 先 Import。如果描边裂，再准备一套平滑法线。
3. 拖进场景里名为 `CharacterSlot` 的物体下，或替换它的子物体。
4. 选 `CharacterSlot` → 右键组件 `Apply Stylized Materials To Children`，再手工把 Head / Hair 赋对开关。
5. 环境 FBX 放 `EnvironmentSlot` 下 → `Prepare For Bake` → Lighting 窗口 Generate。
6. 在 **PC High / Mobile High / Mobile Low** 同一机位截图，放进你的 TA 笔记。

## 检查表（交作业用）

- [ ] 脸：明暗交界可读，随头转
- [ ] 发：有各向异性，不是圆点高光
- [ ] 描边：不断线，低档可关
- [ ] 地面：PC High 有倒影，Mobile Low 没有但不黑死
- [ ] GI：关掉实时霓虹灯，角色仍有环境色
- [ ] 后处理：Bloom 只晕灯管，不晕脸
- [ ] 六档都能运行，HUD 描述和画面一致

## CharacterSlot 约定

| 子物体名包含 | 材质意图 |
| --- | --- |
| Head / Face | `_UseFaceShadow` |
| Hair | `_UseHairAniso` |
| 其它 | Ramp 身体 |

Rendering Layer 与 Layer 都建议标 Character，方便灯和 Outline 过滤。
