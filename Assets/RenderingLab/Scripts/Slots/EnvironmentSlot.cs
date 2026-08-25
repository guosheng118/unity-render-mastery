using UnityEngine;

namespace RenderingLab
{
    /// <summary>
    /// Parent for environment meshes you import. Mark them Static and Contribute GI.
    /// </summary>
    public class EnvironmentSlot : MonoBehaviour
    {
        [ContextMenu("Mark Children Static + Contribute GI")]
        public void PrepareForBake()
        {
            int layer = LayerMask.NameToLayer("Environment");
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.isStatic = true;
                if (layer >= 0) t.gameObject.layer = layer;
            }

            foreach (var r in GetComponentsInChildren<MeshRenderer>(true))
            {
                r.receiveGI = ReceiveGI.Lightmaps;
                r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
            }
        }
    }
}
