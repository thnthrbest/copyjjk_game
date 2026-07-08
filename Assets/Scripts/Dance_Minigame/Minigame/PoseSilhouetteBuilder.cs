using UnityEngine;

/// <summary>
/// ขยับ joint ของ SilhouetteCharacter ให้ตรงกับ DancePose
/// รองรับโหมด "ชิ้นส่วนเดียว" — ไฮไลต์เฉพาะชิ้นส่วนที่สุ่มมาด้วยสีพิเศษ
/// ชิ้นส่วนอื่นคงท่าเดิมและกลับสีปกติ
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

    [Header("Renderer ของแต่ละชิ้นส่วน (ใช้เปลี่ยนสีไฮไลต์)")]
    public Renderer leftArmRenderer;
    public Renderer rightArmRenderer;
    public Renderer leftLegRenderer;
    public Renderer rightLegRenderer;

    [Header("Material สำหรับเปลี่ยนสี (ลากใส่ 2 อัน)")]
    [Tooltip("Material สีดำปกติ (Unlit/Color สีดำ)")]
    public Material defaultMaterial;
    [Tooltip("Material สีไฮไลต์ (Unlit/Color สีที่อยากให้เปลี่ยน เช่น เหลือง)")]
    public Material highlightMaterial;

    // ────────────────────────────────────────────────
    // โหมดชิ้นส่วนเดียว: สุ่ม 1 ส่วน ไฮไลต์ ที่เหลือคงเดิม
    // ────────────────────────────────────────────────

    /// <summary>
    /// ไฮไลต์ชิ้นส่วนที่สุ่มมาด้วยสีพิเศษ และขยับ joint ให้ตรงค่าเป้าหมาย
    /// ชิ้นส่วนอื่นคง joint เดิมไว้ ไม่ขยับ ไม่เปลี่ยนสี
    /// </summary>
    public void ApplyPartialPose(RandomPoseGenerator.LimbPart part, float targetValue)
    {
        var map = GetMap(part);
        if (map == null) { Debug.LogWarning($"[Silhouette] GetMap null for {part}"); return; }

        float angle = map.GetTargetAngle(targetValue);
        SetJointAngle(GetJoint(part), angle, map.axis);

        // เปลี่ยน material ของชิ้นส่วนนั้นเป็น highlightMaterial
        SetMaterial(GetRenderer(part), highlightMaterial);
    }

    public void ResetHighlight()
    {
        // คืน material ทุกชิ้นส่วนกลับเป็น defaultMaterial
        SetMaterial(leftArmRenderer,  defaultMaterial);
        SetMaterial(rightArmRenderer, defaultMaterial);
        SetMaterial(leftLegRenderer,  defaultMaterial);
        SetMaterial(rightLegRenderer, defaultMaterial);
    }

    /// <summary>
    /// หมุนทุก joint กลับไปยังท่ากลาง (ค่ากึ่งกลางของ stepAngles แต่ละชิ้น)
    /// เรียกก่อนสุ่มท่าใหม่ทุกครั้ง ป้องกันท่าเก่าค้างผสมกับท่าใหม่
    /// </summary>
    public void ResetPose()
    {
        ResetJoint(leftArmJoint,  sourceMapper?.leftArmLift);
        ResetJoint(leftArmJoint,  sourceMapper?.leftArmSwing);
        ResetJoint(rightArmJoint, sourceMapper?.rightArmLift);
        ResetJoint(rightArmJoint, sourceMapper?.rightArmSwing);
        ResetJoint(leftLegJoint,  sourceMapper?.leftLegLift);
        ResetJoint(leftLegJoint,  sourceMapper?.leftLegSwing);
        ResetJoint(rightLegJoint, sourceMapper?.rightLegLift);
        ResetJoint(rightLegJoint, sourceMapper?.rightLegSwing);
    }

    void ResetJoint(Transform joint, FingerToLimbMapper.FingerJointMap map)
    {
        if (joint == null || map == null) return;
        float angle = map.GetTargetAngle(0.5f); // ค่ากึ่งกลาง = ท่ากลาง
        SetJointAngle(joint, angle, map.axis);
    }

    // ────────────────────────────────────────────────
    // โหมดเต็มท่า (ใช้กรณีต้องการแสดงท่าครบทุกชิ้นส่วน)
    // ────────────────────────────────────────────────

    /// <summary>ขยับทุกชิ้นส่วนตาม DancePose (ค่า -1 = ข้าม ไม่ขยับ)</summary>
    public void ApplyPose(DancePose pose)
    {
        if (pose == null || sourceMapper == null) return;

        ApplyIfValid(leftArmJoint,  sourceMapper.leftArmLift,   pose.leftArmLift);
        ApplyIfValid(leftArmJoint,  sourceMapper.leftArmSwing,  pose.leftArmSwing);
        ApplyIfValid(rightArmJoint, sourceMapper.rightArmLift,  pose.rightArmLift);
        ApplyIfValid(rightArmJoint, sourceMapper.rightArmSwing, pose.rightArmSwing);
        ApplyIfValid(leftLegJoint,  sourceMapper.leftLegLift,   pose.leftLegLift);
        ApplyIfValid(leftLegJoint,  sourceMapper.leftLegSwing,  pose.leftLegSwing);
        ApplyIfValid(rightLegJoint, sourceMapper.rightLegLift,  pose.rightLegLift);
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
            RandomPoseGenerator.LimbPart.RightArmLift  => sourceMapper.rightArmLift,
            RandomPoseGenerator.LimbPart.RightArmSwing => sourceMapper.rightArmSwing,
            RandomPoseGenerator.LimbPart.LeftLegLift   => sourceMapper.leftLegLift,
            RandomPoseGenerator.LimbPart.LeftLegSwing  => sourceMapper.leftLegSwing,
            RandomPoseGenerator.LimbPart.RightLegLift  => sourceMapper.rightLegLift,
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
            RandomPoseGenerator.LimbPart.RightArmLift  => rightArmJoint,
            RandomPoseGenerator.LimbPart.RightArmSwing => rightArmJoint,
            RandomPoseGenerator.LimbPart.LeftLegLift   => leftLegJoint,
            RandomPoseGenerator.LimbPart.LeftLegSwing  => leftLegJoint,
            RandomPoseGenerator.LimbPart.RightLegLift  => rightLegJoint,
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
            RandomPoseGenerator.LimbPart.RightArmLift  => rightArmRenderer,
            RandomPoseGenerator.LimbPart.RightArmSwing => rightArmRenderer,
            RandomPoseGenerator.LimbPart.LeftLegLift   => leftLegRenderer,
            RandomPoseGenerator.LimbPart.LeftLegSwing  => leftLegRenderer,
            RandomPoseGenerator.LimbPart.RightLegLift  => rightLegRenderer,
            RandomPoseGenerator.LimbPart.RightLegSwing => rightLegRenderer,
            _                                          => null
        };
    }
}