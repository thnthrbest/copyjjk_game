using UnityEngine;

/// <summary>
/// ท่าเป้าหมาย — เก็บค่า 0-1 ของแต่ละจุดควบคุม
/// ค่า -1 = ไม่ต้องเช็คจุดนี้ (ใช้ในโหมดสุ่มทีละชิ้นส่วน)
///
/// จุดควบคุมใหม่ (6 จุด):
///   leftArmLift, leftArmSwing, leftLegSwing
///   rightArmLift, rightArmSwing, rightLegSwing
/// </summary>
[CreateAssetMenu(fileName = "NewDancePose", menuName = "Dance/Pose")]
public class DancePose : ScriptableObject
{
    [Header("ชื่อท่า")]
    public string poseName;

    [Header("=== ค่าเป้าหมาย ฝั่งซ้าย (0-1 หรือ -1=ไม่เช็ค) ===")]
    [Range(-1, 1)] public float leftArmLift;   // นิ้วกลางซ้าย
    [Range(-1, 1)] public float leftArmSwing;  // นิ้วชี้ซ้าย
    [Range(-1, 1)] public float leftLegSwing;  // นิ้วโป้งซ้าย

    [Header("=== ค่าเป้าหมาย ฝั่งขวา (0-1 หรือ -1=ไม่เช็ค) ===")]
    [Range(-1, 1)] public float rightArmLift;
    [Range(-1, 1)] public float rightArmSwing;
    [Range(-1, 1)] public float rightLegSwing;

    [Header("เกณฑ์การให้คะแนน")]
    [Tooltip("ความคลาดเคลื่อนปกติ (0-1)")]
    [Range(0.01f, 0.5f)] public float tolerance = 0.15f;

    [Tooltip("ความคลาดเคลื่อนพิเศษของ step เป้าหมาย (กว้างกว่าปกติ ทำให้ผ่านง่ายขึ้น)")]
    [Range(0.01f, 0.8f)] public float targetStepTolerance = 0.35f;

    [Tooltip("ต้องค้างท่าให้ตรงกี่วินาทีจึงผ่าน")]
    public float holdTime = 1.0f;
}