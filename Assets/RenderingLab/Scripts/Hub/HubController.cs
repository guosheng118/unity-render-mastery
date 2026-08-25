using UnityEngine;

namespace RenderingLab
{
    public class HubController : MonoBehaviour
    {
        [TextArea(6, 20)]
        public string welcome =
            "Unity 6.3 渲染实验室\n" +
            "左上角切档位 / 切场景。\n" +
            "00 Hub  01 灯光  02 GI/APV  03 反射\n" +
            "04 Feature  05 后处理  06 档位  07 霓虹主切片\n" +
            "把 FBX 放到 CharacterSlot，环境放到 EnvironmentSlot。";
    }
}
