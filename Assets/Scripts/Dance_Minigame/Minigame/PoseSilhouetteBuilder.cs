using UnityEngine;

/// <summary>
/// ขยับ joint ของ SilhouetteCharacter ให้ตรงกับ DancePose
/// รองรับโหมด "ชิ้นส่วนเดียว" ไฮไลต์เฉพาะชิ้นส่วนที่สุ่มมา
///
/// จุดควบคุมใหม่ (6 จุด ไม่มี LegLift แล้ว):
///   leftArmLift, leftArmSwing, leftLegSwing
///   rightArmLift, rightArmSwing, rightLegSwing
/// </summary>
public class PoseSilhouetteBuilder : MonoBehaviour
{
    [Header("ดึงการตั้งค่าจาก FingerToLimbMapper ของตัวละครหลัก")]
    public FingerToLimbMapper sourceMapper;

    [Header("Joint ของ SilhouetteCharacter")]
    public Transform leftArmJoint;
    public Transform rightArmJoint;
    public Transform leftLegJoint;
    public Transform rightLegJoint;

    [Header("Renderer ของแต่ละชิ้นส่วน (ใช้เปลี่ยน Material ไฮไลต์)")]
    public Renderer leftArmRenderer;
    public Renderer rightArmRenderer;
    public Renderer leftLegRenderer;
    public Renderer rightLegRenderer;

    [Header("Material")]
    [Tooltip("Material สีดำปกติ")]
    public Material defaultMaterial;
    [Tooltip("Material สีไฮไลต์ (เช่น เหลือง)")]
    public Material highlightMaterial;

    // ────────────────────────────────────────────────
    // โหมดชิ้นส่วนเดียว
    // ────────────────────────────────────────────────

    public void ApplyPartialPose(RandomPoseGenerator.LimbPart part, float targetValue)
    {
        var map = GetMap(part);
        if (map == null) { Debug.LogWarning($"[Silhouette] GetMap null for {part}"); return; }

        float angle = map.GetTargetAngle(targetValue);
        SetJointAngle(GetJoint(part), angle, map.axis);
        SetMaterial(GetRenderer(part), highlightMaterial);
    }

    public void ResetHighlight()
    {
        SetMaterial(leftArmRenderer,  defaultMaterial);
        SetMaterial(rightArmRenderer, defaultMaterial);
        SetMaterial(leftLegRenderer,  defaultMaterial);
        SetMaterial(rightLegRenderer, defaultMaterial);
    }

    // ────────────────────────────────────────────────
    // โหมดเต็มท่า (ค่า -1 = ข้าม)
    // ────────────────────────────────────────────────

    public void ApplyPose(DancePose pose)
    {
        if (pose == null || sourceMapper == null) return;

        ApplyIfValid(leftArmJoint,  sourceMapper.leftArmLift,   pose.leftArmLift);
        ApplyIfValid(leftArmJoint,  sourceMapper.leftArmSwing,  pose.leftArmSwing);
        ApplyIfValid(leftLegJoint,  sourceMapper.leftLegSwing,  pose.leftLegSwing);
        ApplyIfValid(rightArmJoint, sourceMapper.rightArmLift,  pose.rightArmLift);
        ApplyIfValid(rightArmJoint, sourceMapper.rightArmSwing, pose.rightArmSwing);
        ApplyIfValid(rightLegJoint, sourceMapper.rightLegSwing, pose.rightLegSwing);
    }

    void ApplyIfValid(Transform joint, FingerToLimbMapper.FingerJointMap map, float value)
    {
        if (value < 0f || joint == null || map == null) return;
        SetJointAngle(joint, map.GetTargetAngle(value), map.axis);
    }

    // ────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────

    void SetJointAngle(Transform joint, float angle, FingerToLimbMapper.RotationAxis axis)
    {
        if (joint == null) return;
        Vector3 e = joint.localEulerAngles;
        switch (axis)
        {
            case FingerToLimbMapper.RotationAxis.X: e.x = angle; break;
            case FingerToLimbMapper.RotationAxis.Y: e.y = angle; break;
            case FingerToLimbMapper.RotationAxis.Z: e.z = angle; break;
        }
        joint.localEulerAngles = e;
    }

    void SetMaterial(Renderer r, Material mat)
    {
        if (r == null || mat == null) return;
        r.material = mat;
    }

    FingerToLimbMapper.FingerJointMap GetMap(RandomPoseGenerator.LimbPart part)
    {
        if (sourceMapper == null) return null;
        return part switch
        {
            RandomPoseGenerator.LimbPart.LeftArmLift   => sourceMapper.leftArmLift,
            RandomPoseGenerator.LimbPart.LeftArmSwing  => sourceMapper.leftArmSwing,
            RandomPoseGenerator.LimbPart.LeftLegSwing  => sourceMapper.leftLegSwing,
            RandomPoseGenerator.LimbPart.RightArmLift  => sourceMapper.rightArmLift,
            RandomPoseGenerator.LimbPart.RightArmSwing => sourceMapper.rightArmSwing,
            RandomPoseGenerator.LimbPart.RightLegSwing => sourceMapper.rightLegSwing,
            _                                          => null
        };
    }

    Transform GetJoint(RandomPoseGenerator.LimbPart part)
    {
        return part switch
        {
            RandomPoseGenerator.LimbPart.LeftArmLift   => leftArmJoint,
            RandomPoseGenerator.LimbPart.LeftArmSwing  => leftArmJoint,
            RandomPoseGenerator.LimbPart.LeftLegSwing  => leftLegJoint,
            RandomPoseGenerator.LimbPart.RightArmLift  => rightArmJoint,
            RandomPoseGenerator.LimbPart.RightArmSwing => rightArmJoint,
            RandomPoseGenerator.LimbPart.RightLegSwing => rightLegJoint,
            _                                          => null
        };
    }

    Renderer GetRenderer(RandomPoseGenerator.LimbPart part)
    {
        return part switch
        {
            RandomPoseGenerator.LimbPart.LeftArmLift   => leftArmRenderer,
            RandomPoseGenerator.LimbPart.LeftArmSwing  => leftArmRenderer,
            RandomPoseGenerator.LimbPart.LeftLegSwing  => leftLegRenderer,
            RandomPoseGenerator.LimbPart.RightArmLift  => rightArmRenderer,
            RandomPoseGenerator.LimbPart.RightArmSwing => rightArmRenderer,
            RandomPoseGenerator.LimbPart.RightLegSwing => rightLegRenderer,
            _                                          => null
        };
    }
}