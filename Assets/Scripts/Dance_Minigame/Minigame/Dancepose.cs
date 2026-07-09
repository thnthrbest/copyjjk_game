using UnityEngine;

/// <summary>
/// ท่าเป้าหมาย (เงาสีดำ) — เก็บค่า normalized angle (0-1) ของ 8 จุด
/// ให้ตรงกับ field ใน FingerToLimbMapper:
///   leftArmLift, leftArmSwing, leftLegLift, leftLegSwing,
///   rightArmLift, rightArmSwing, rightLegLift, rightLegSwing
///
/// สร้างได้จากเมนู: Assets > Create > Dance > Pose
/// </summary>
[CreateAssetMenu(fileName = "NewDancePose", menuName = "Dance/Pose")]
public class DancePose : ScriptableObject
{
    [Header("ชื่อท่า")]
    public string poseName;

    [Header("=== ค่าเป้าหมาย ฝั่งซ้าย (0-1) ===")]
    [Range(0, 1)] public float leftArmLift;
    [Range(0, 1)] public float leftArmSwing;
    [Range(0, 1)] public float leftLegLift;
    [Range(0, 1)] public float leftLegSwing;

    [Header("=== ค่าเป้าหมาย ฝั่งขวา (0-1) ===")]
    [Range(0, 1)] public float rightArmLift;
    [Range(0, 1)] public float rightArmSwing;
    [Range(0, 1)] public float rightLegLift;
    [Range(0, 1)] public float rightLegSwing;

    [Header("เกณฑ์การให้คะแนน")]
    [Tooltip("ความคลาดเคลื่อนปกติต่อจุด (0-1)")]
    [Range(0.01f, 0.5f)] public float tolerance = 0.15f;

    [Tooltip("ความคลาดเคลื่อนพิเศษของ step เป้าหมาย (กว้างกว่า tolerance ปกติ เพื่อให้ผู้เล่นทำได้ง่ายขึ้น)")]
    [Range(0.01f, 0.8f)] public float targetStepTolerance = 0.35f;

    [Tooltip("ต้องค้างท่าให้ตรง (accuracy เกิน threshold) กี่วินาทีจึงผ่าน")]
    public float holdTime = 1.0f;
}