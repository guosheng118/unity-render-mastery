using UnityEngine;

namespace RenderingLab
{
    public class LabCharacterBuilder : MonoBehaviour
    {
        public void Build(LabModule module, bool keepExisting)
        {
            var existing = transform.Find("CharacterSlot");
            if (existing != null)
            {
                if (keepExisting && existing.GetComponentInChildren<MeshRenderer>())
                    return;
                DestroyImmediate(existing.gameObject);
            }

            var slot = new GameObject("CharacterSlot");
            slot.transform.SetParent(transform, false);
            slot.transform.position = new Vector3(0f, 0f, 0.4f);
            var slotComp = slot.AddComponent<CharacterSlot>();

            int charLayer = LayerMask.NameToLayer("Character");
            if (charLayer < 0) charLayer = 0;

            // Placeholder "agent": pelvis + torso + head + hair cap + two arms.
            // Replace this hierarchy with your FBX; keep the CharacterSlot root.
            var pelvis = Make(slot.transform, "Pelvis", PrimitiveType.Capsule, new Vector3(0, 0.55f, 0), new Vector3(0.38f, 0.28f, 0.28f), charLayer, LabMaterials.Stylized(new Color(0.12f, 0.13f, 0.18f), false, false));
            var torso = Make(slot.transform, "Torso", PrimitiveType.Capsule, new Vector3(0, 1.15f, 0), new Vector3(0.55f, 0.42f, 0.32f), charLayer, LabMaterials.Stylized(new Color(0.85f, 0.22f, 0.28f), false, false));
            var head = Make(slot.transform, "Head", PrimitiveType.Sphere, new Vector3(0, 1.72f, 0.02f), Vector3.one * 0.38f, charLayer, LabMaterials.Stylized(new Color(0.96f, 0.82f, 0.74f), true, false));
            var hair = Make(slot.transform, "Hair", PrimitiveType.Sphere, new Vector3(0, 1.82f, -0.02f), new Vector3(0.42f, 0.28f, 0.4f), charLayer, LabMaterials.Stylized(new Color(0.12f, 0.09f, 0.16f), false, true));
            Make(slot.transform, "Arm_L", PrimitiveType.Capsule, new Vector3(-0.42f, 1.1f, 0), new Vector3(0.16f, 0.38f, 0.16f), charLayer, LabMaterials.Stylized(new Color(0.96f, 0.82f, 0.74f), false, false));
            Make(slot.transform, "Arm_R", PrimitiveType.Capsule, new Vector3(0.42f, 1.1f, 0), new Vector3(0.16f, 0.38f, 0.16f), charLayer, LabMaterials.Stylized(new Color(0.96f, 0.82f, 0.74f), false, false));

            slotComp.faceRenderer = head.GetComponent<MeshRenderer>();
            slotComp.hairRenderer = hair.GetComponent<MeshRenderer>();
            slotComp.bodyRenderers = new[]
            {
                pelvis.GetComponent<MeshRenderer>(),
                torso.GetComponent<MeshRenderer>()
            };

            if (module == LabModule.Hub)
            {
                slot.transform.position = new Vector3(0.8f, 0f, 0.2f);
            }
        }

        static GameObject Make(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 scale, int layer, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.layer = layer;
            Object.Destroy(go.GetComponent<Collider>());
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
            r.receiveGI = ReceiveGI.LightProbes;
            return go;
        }
    }
}
