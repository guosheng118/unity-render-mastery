using UnityEngine;

[ExecuteAlways]
public class FaceHeadOrientation : MonoBehaviour
{
    public enum BoneAxis
    {
        AutoHorizontal = 0,
        LocalX = 1,
        LocalY = 2,
        LocalZ = 3
    }

    [Tooltip("拖 Bip001 Head。Max 两足头骨的 X 通常沿脖子朝上，不是出鼻子。")]
    public Transform headBone;

    [Tooltip("脸部分层后的所有 SkinnedMeshRenderer，都会写入同一套朝前方向。")]
    public Renderer[] faceRenderers;

    [HideInInspector]
    [SerializeField]
    Renderer faceRenderer;

    [Tooltip("脸朝前对应头骨哪根轴。Auto 每帧取水平分量最大的那根（含正负）。")]
    public BoneAxis faceForwardAxis = BoneAxis.AutoHorizontal;

    public bool flipForward;

    [Tooltip("Scene 里从头骨画出当前用的水平朝前")]
    public bool drawGizmo = true;

    static readonly int FwdId = Shader.PropertyToID("_FaceForwardWS");
    MaterialPropertyBlock _block;

    void OnValidate()
    {
        MigrateLegacyRenderer();
    }

    void OnEnable()
    {
        MigrateLegacyRenderer();
    }

    void LateUpdate()
    {
        if (headBone == null)
            return;

        Vector3 fwd = FlattenYaw(GetFaceForwardWorld());

        if (_block == null)
            _block = new MaterialPropertyBlock();

        ApplyForward(fwd, faceRenderer);
        if (faceRenderers == null)
            return;

        for (int i = 0; i < faceRenderers.Length; i++)
            ApplyForward(fwd, faceRenderers[i]);
    }

    void ApplyForward(Vector3 fwd, Renderer renderer)
    {
        if (renderer == null)
            return;

        renderer.GetPropertyBlock(_block);
        _block.SetVector(FwdId, fwd);
        renderer.SetPropertyBlock(_block);
    }

    void MigrateLegacyRenderer()
    {
        if (faceRenderer == null)
            return;
        if (faceRenderers != null && faceRenderers.Length > 0)
            return;

        faceRenderers = new[] { faceRenderer };
    }

    Vector3 GetFaceForwardWorld()
    {
        Vector3 axis;
        switch (faceForwardAxis)
        {
            case BoneAxis.LocalX:
                axis = headBone.right;
                break;
            case BoneAxis.LocalY:
                axis = headBone.up;
                break;
            case BoneAxis.LocalZ:
                axis = headBone.forward;
                break;
            default:
                axis = PickMostHorizontalAxis();
                break;
        }

        if (flipForward)
            axis = -axis;
        return axis;
    }

    Vector3 PickMostHorizontalAxis()
    {
        Vector3[] candidates =
        {
            headBone.right, -headBone.right,
            headBone.up, -headBone.up,
            headBone.forward, -headBone.forward
        };

        Vector3 best = candidates[0];
        float bestSq = -1f;
        for (int i = 0; i < candidates.Length; i++)
        {
            Vector3 horizontal = candidates[i];
            horizontal.y = 0f;
            float sq = horizontal.sqrMagnitude;
            if (sq > bestSq)
            {
                bestSq = sq;
                best = candidates[i];
            }
        }

        return best;
    }

    static Vector3 FlattenYaw(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 1e-6f)
            return Vector3.right;
        return dir.normalized;
    }

    void OnDrawGizmos()
    {
        if (!drawGizmo || headBone == null)
            return;

        Vector3 origin = headBone.position;
        Vector3 fwd = FlattenYaw(GetFaceForwardWorld());
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + fwd * 0.25f);
        Gizmos.DrawSphere(origin + fwd * 0.25f, 0.015f);
    }
}
