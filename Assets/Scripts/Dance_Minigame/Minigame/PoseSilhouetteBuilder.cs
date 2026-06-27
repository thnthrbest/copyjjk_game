using UnityEngine;

/// <summary>
/// ขยับ joint ของ SilhouetteCharacter ให้ตรงกับ DancePose ที่สุ่มมา
/// ดึงการตั้งค่า (axis, stepAngles, invert) มาจาก FingerToLimbMapper ของตัวละครหลักโดยตรง
/// ไม่ต้องตั้งค่าซ้ำ และรองรับการแบ่งซ้าย/ขวาอัตโนมัติ
/// </summary>
public class PoseSilhouetteBuilder : MonoBehaviour
{
    [Header("ดึงการตั้งค่าจาก FingerToLimbMapper ของตัวละครหลัก")]
    [Tooltip("ลาก FingerToLimbMapper ที่ติดอยู่บน HandSystem มาใส่")]
    public FingerToLimbMapper sourceMapper;

    [Header("Joint ของ SilhouetteCharacter (ตัวละคร 3D สีดำ)")]
    [Tooltip("ลาก joint แขนซ้ายของ SilhouetteCharacter (ไม่ใช่ตัวละครหลัก)")]
    public Transform leftArmJoint;
    public Transform rightArmJoint;
    public Transform leftLegJoint;
    public Transform rightLegJoint;

    /// <summary>
    /// รับ DancePose แล้วขยับ joint ของ SilhouetteCharacter ให้ตรงท่านั้น
    /// โดยใช้การตั้งค่า (axis, stepAngles, invert) จาก sourceMapper ของตัวละครหลักทั้งหมด
    /// </summary>
    public void ApplyPose(DancePose pose)
    {
        if (pose == null || sourceMapper == null) return;

        ApplyFromMap(leftArmJoint,  sourceMapper.leftArmLift,  pose.leftArmLift);
        ApplyFromMap(leftArmJoint,  sourceMapper.leftArmSwing, pose.leftArmSwing);
        ApplyFromMap(rightArmJoint, sourceMapper.rightArmLift, pose.rightArmLift);
        ApplyFromMap(rightArmJoint, sourceMapper.rightArmSwing, pose.rightArmSwing);
        ApplyFromMap(leftLegJoint,  sourceMapper.leftLegLift,  pose.leftLegLift);
        ApplyFromMap(leftLegJoint,  sourceMapper.leftLegSwing, pose.leftLegSwing);
        ApplyFromMap(rightLegJoint, sourceMapper.rightLegLift, pose.rightLegLift);
        ApplyFromMap(rightLegJoint, sourceMapper.rightLegSwing, pose.rightLegSwing);
    }

    /// <summary>
    /// ใช้การตั้งค่าจาก FingerJointMap ของตัวละครหลักมาคำนวณมุม
    /// แล้วใส่ให้ joint ของ SilhouetteCharacter ตรงๆ
    /// </summary>
    void ApplyFromMap(Transform joint, FingerToLimbMapper.FingerJointMap map, float poseValue)
    {
        if (joint == null || map == null) return;

        // ใช้ GetTargetAngle() ของ map ซึ่งคำนวณจาก minAngle/maxAngle และ invert
        // เหมือนกับที่ตัวละครหลักทำทุกประการ
        float targetAngle = map.GetTargetAngle(poseValue);

        Vector3 e = joint.localEulerAngles;
        switch (map.axis)
        {
            case FingerToLimbMapper.RotationAxis.X: e.x = targetAngle; break;
            case FingerToLimbMapper.RotationAxis.Y: e.y = targetAngle; break;
            case FingerToLimbMapper.RotationAxis.Z: e.z = targetAngle; break;
        }
        joint.localEulerAngles = e;
    }
}