using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

/// <summary>
/// สุ่มท่า "ครั้งเดียว" แล้วใช้ค่าเดียวกันนั้นทำ 2 อย่างพร้อมกัน:
///   1) ส่งให้ DancePoseEvaluator ใช้เป็นเป้าหมายเช็คท่าจริงของผู้เล่น
///   2) ส่งให้ PoseSilhouetteBuilder ใช้วาดเงาใบ้ท่าให้ผู้เล่นทำตาม
///
/// ไม่มีการสุ่มสองครั้งสองที่ — pose object เดียวกันถูกส่งเข้าทั้งสองปลายทาง
/// จึงรับประกันว่า "เงาที่เห็น" กับ "ท่าที่ใช้ตัดสิน" เป็นท่าเดียวกันเป๊ะเสมอ
///
/// ทำงานแบบ "ถือนิ่งทีละท่า ไม่มีเวลาจำกัด":
/// เมื่อผู้เล่นทำท่าปัจจุบันผ่าน (DancePoseEvaluator.OnPoseComplete ยิง)
/// จะสุ่มท่าใหม่และอัปเดตทั้งเงาและเป้าหมายให้อัตโนมัติ
/// </summary>
public class RandomPoseGenerator : MonoBehaviour
{
    [Header("จำนวนขั้นของแต่ละจุด (ต้องตรงกับจำนวน stepAngles จริงใน FingerToLimbMapper)")]
    [Tooltip("แขน: ยกขึ้น/ลง เช่น 3 ขั้น = -90, 0, 90")]
    public int armLiftSteps = 3;
    [Tooltip("แขน: หมุนหน้า/หลัง")]
    public int armSwingSteps = 3;
    [Tooltip("ขา: ยกด้านข้าง")]
    public int legLiftSteps = 2;
    [Tooltip("ขา: หน้า/หลัง")]
    public int legSwingSteps = 3;

    [Header("ค่าเริ่มต้นของ Pose ที่สุ่มสร้าง")]
    public float defaultTolerance = 0.15f;
    public float defaultHoldTime  = 1.0f;

    [Header("ปลายทางที่จะรับท่าเดียวกัน (ต้องใส่ทั้งคู่)")]
    [Tooltip("ใช้เช็คว่าท่าจริงของผู้เล่นตรงกับท่าที่สุ่มมาหรือไม่")]
    public DancePoseEvaluator evaluator;
    [Tooltip("ใช้วาดเงาใบ้ท่าให้ผู้เล่นเห็นว่าต้องทำท่าไหน")]
    public PoseSilhouetteBuilder silhouetteBuilder;

    [Header("เริ่มสุ่มท่าแรกอัตโนมัติตอน Start หรือไม่")]
    public bool autoStartOnPlay = true;

    /// <summary>ท่าปัจจุบันที่กำลังให้ผู้เล่นทำอยู่ (ตัวเดียวกับที่ทั้ง evaluator และ silhouette ใช้)</summary>
    public DancePose CurrentChallenge { get; private set; }

    readonly System.Random _rng = new System.Random();

    void Start()
    {
        if (evaluator != null)
            evaluator.OnPoseComplete += HandlePoseComplete;

        if (autoStartOnPlay)
            GenerateNewChallenge();
    }

    void OnDestroy()
    {
        if (evaluator != null)
            evaluator.OnPoseComplete -= HandlePoseComplete;
    }

    void HandlePoseComplete(int starsIgnored)
    {
        // ผู้เล่นทำท่าปัจจุบันผ่านแล้ว -> สุ่มท่าใหม่ทันที (โหมดถือนิ่งทีละท่า ไม่มีเวลาจำกัด)
        GenerateNewChallenge();
    }

    // ────────────────────────────────────────────────
    // ฟังก์ชันหลัก: สุ่ม 1 ท่า แล้วส่งไปทั้ง evaluator และ silhouette พร้อมกัน
    // ────────────────────────────────────────────────

    /// <summary>
    /// สุ่มท่าใหม่ 1 ท่า แล้วอัปเดตทั้งเงาใบ้และเป้าหมายตัดสินให้ตรงกันทันที
    /// เรียกฟังก์ชันนี้ตัวเดียวพอ ไม่ต้องสุ่มแยกที่อื่นอีก
    /// </summary>
    public DancePose GenerateNewChallenge()
    {
        DancePose pose = GenerateRandomPose();
        CurrentChallenge = pose;

        if (evaluator != null)
            evaluator.SetPose(pose);

        if (silhouetteBuilder != null)
            silhouetteBuilder.ApplyPose(pose);

        Debug.Log(
            $"[RandomPoseGenerator] ท่าใหม่: {pose.poseName}\n" +
            $"L: armLift={pose.leftArmLift:F2} armSwing={pose.leftArmSwing:F2} " +
            $"legLift={pose.leftLegLift:F2} legSwing={pose.leftLegSwing:F2}\n" +
            $"R: armLift={pose.rightArmLift:F2} armSwing={pose.rightArmSwing:F2} " +
            $"legLift={pose.rightLegLift:F2} legSwing={pose.rightLegSwing:F2}");

        return pose;
    }

    /// <summary>ปุ่มทดสอบใน Inspector — สุ่มท่าใหม่ทันทีโดยไม่ต้องรอผ่านท่าเดิม</summary>
    [ContextMenu("สุ่มท่าใหม่ทันที")]
    public void ForceNewChallenge() => GenerateNewChallenge();

    // ────────────────────────────────────────────────
    // ตัวสุ่มท่าเดียว (ใช้ภายใน GenerateNewChallenge)
    // ────────────────────────────────────────────────

    float RandomStepValue(int stepCount)
    {
        if (stepCount <= 1) return 0f;
        int idx = _rng.Next(0, stepCount); // 0 ถึง stepCount-1
        return idx / (float)(stepCount - 1);
    }

    /// <summary>
    /// สุ่ม DancePose 1 ท่า แบบ public ใช้ได้จากที่อื่น เช่น BeatmapPlayer
    /// (เรียกอันนี้เมื่ออยากได้ "ท่าสุ่ม" เฉยๆ โดยไม่ต้องส่งเข้า evaluator/silhouette อัตโนมัติ
    ///  ถ้าอยากสุ่ม + ใช้กับ evaluator/silhouette พร้อมกันในจุดเดียว ให้เรียก GenerateNewChallenge() แทน)
    /// </summary>
    public DancePose GenerateRandomPose(string poseName = null)
    {
        DancePose pose = ScriptableObject.CreateInstance<DancePose>();

        pose.poseName  = string.IsNullOrEmpty(poseName) ? $"Random_{_rng.Next(1000, 9999)}" : poseName;
        pose.tolerance = defaultTolerance;
        pose.holdTime  = defaultHoldTime;

        pose.leftArmLift   = RandomStepValue(armLiftSteps);
        pose.leftArmSwing  = RandomStepValue(armSwingSteps);
        pose.leftLegLift   = RandomStepValue(legLiftSteps);
        pose.leftLegSwing  = RandomStepValue(legSwingSteps);

        pose.rightArmLift  = RandomStepValue(armLiftSteps);
        pose.rightArmSwing = RandomStepValue(armSwingSteps);
        pose.rightLegLift  = RandomStepValue(legLiftSteps);
        pose.rightLegSwing = RandomStepValue(legSwingSteps);

        return pose;
    }

    /// <summary>จำนวน combination ทั้งหมดที่เป็นไปได้ตามจำนวน step ที่ตั้งไว้ (ไว้เช็คดูเฉยๆ)</summary>
    public long TotalPossibleCombinations()
    {
        long armCombos = (long)armLiftSteps * armSwingSteps;
        long legCombos = (long)legLiftSteps * legSwingSteps;
        return armCombos * armCombos * legCombos * legCombos;
    }

    // ────────────────────────────────────────────────
    // Editor Tool (ไม่บังคับ): สุ่มหลายท่าแล้วบันทึกเป็นไฟล์ .asset
    // ใช้ตอนอยากได้ท่าสำรองไว้กำหนดตายตัวบางท่าเอง (เช่น ท่าไหว้ตอนจบ)
    // ────────────────────────────────────────────────

#if UNITY_EDITOR
    [Header("Editor Tool (ไม่บังคับ): บันทึกท่าสุ่มเป็นไฟล์ .asset")]
    public int batchGenerateCount = 10;
    public string saveFolder = "Assets/DancePoses/Random";

    [ContextMenu("สุ่มหลายท่า + บันทึกเป็นไฟล์ (Editor Only)")]
    public void BatchGenerateAndSaveAssets()
    {
        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        for (int i = 0; i < batchGenerateCount; i++)
        {
            DancePose p = GenerateRandomPose();
            p.poseName = $"Random_{i + 1:000}";
            string path = AssetDatabase.GenerateUniqueAssetPath($"{saveFolder}/{p.poseName}.asset");
            AssetDatabase.CreateAsset(p, path);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[RandomPoseGenerator] บันทึกท่าสุ่ม {batchGenerateCount} ท่า ที่ {saveFolder}");
    }
#endif
}