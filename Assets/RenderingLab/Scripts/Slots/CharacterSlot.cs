using UnityEngine;

namespace RenderingLab
{
    /// <summary>
    /// Drop your character FBX here. Expected children: Head (face SDF), Hair (aniso), Body.
    /// Rendering Layer should include Character. Receive GI = Light Probes.
    /// </summary>
    public class CharacterSlot : MonoBehaviour
    {
        public MeshRenderer faceRenderer;
        public MeshRenderer hairRenderer;
        public MeshRenderer[] bodyRenderers;

        [ContextMenu("Apply Stylized Materials To Children")]
        public void ApplyDefaultMaterials()
        {
            foreach (var r in GetComponentsInChildren<MeshRenderer>())
            {
                bool face = r == faceRenderer || r.name.ToLowerInvariant().Contains("face") || r.name.ToLowerInvariant().Contains("head");
                bool hair = r == hairRenderer || r.name.ToLowerInvariant().Contains("hair");
                r.sharedMaterial = LabMaterials.Stylized(r.sharedMaterial != null ? r.sharedMaterial.color : Color.white, face, hair);
                r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
                r.receiveGI = ReceiveGI.LightProbes;
                int layer = LayerMask.NameToLayer("Character");
                if (layer >= 0) r.gameObject.layer = layer;
            }
        }
    }
}
