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
/// ดึงจำนวนขั้นจาก FingerToLimbMapper โดยตรง
/// ไม่ต้องตั้งค่าจำนวนขั้นซ้ำที่นี่ — แก้ที่ stepAngles ใน FingerToLimbMapper ที่เดียวพอ
/// </summary>
public class RandomPoseGenerator : MonoBehaviour
{
    [Header("ดึงจำนวนขั้นจาก FingerToLimbMapper โดยตรง (ไม่ต้องตั้งซ้ำ)")]
    [Tooltip("ลาก FingerToLimbMapper ที่ติดอยู่บน HandSystem มาใส่")]
    public FingerToLimbMapper sourceMapper;

    [Header("ค่าเริ่มต้นของ Pose ที่สุ่มสร้าง")]
    public float defaultTolerance = 0.15f;
    public float defaultHoldTime  = 1.0f;

    [Header("ปลายทางที่จะรับท่าเดียวกัน (ต้องใส่ทั้งคู่)")]
    public DancePoseEvaluator      evaluator;
    public PoseSilhouetteBuilder   silhouetteBuilder;

    [Header("เริ่มสุ่มท่าแรกอัตโนมัติตอน Start หรือไม่")]
    public bool autoStartOnPlay = true;

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

    void HandlePoseComplete(int _) => GenerateNewChallenge();

    // ────────────────────────────────────────────────
    // สุ่มท่าใหม่และส่งไปทั้ง evaluator และ silhouette พร้อมกัน
    // ────────────────────────────────────────────────

    [ContextMenu("สุ่มท่าใหม่ทันที")]
    public DancePose GenerateNewChallenge()
    {
        DancePose pose = GenerateRandomPose();
        CurrentChallenge = pose;

        if (evaluator        != null) evaluator.SetPose(pose);
        if (silhouetteBuilder != null) silhouetteBuilder.ApplyPose(pose);

        Debug.Log(
            $"[RandomPoseGenerator] New pose: {pose.poseName}\n" +
            $"L: armLift={pose.leftArmLift:F2} armSwing={pose.leftArmSwing:F2} " +
            $"legLift={pose.leftLegLift:F2} legSwing={pose.leftLegSwing:F2}\n" +
            $"R: armLift={pose.rightArmLift:F2} armSwing={pose.rightArmSwing:F2} " +
            $"legLift={pose.rightLegLift:F2} legSwing={pose.rightLegSwing:F2}");

        return pose;
    }

    // ────────────────────────────────────────────────
    // ดึงจำนวนขั้นจาก stepAngles ของ map แล้วสุ่ม step
    // ────────────────────────────────────────────────

    /// <summary>
    /// ดึงจำนวน step จาก stepAngles ของ map
    /// สุ่ม index แล้วแปลงเป็น 0-1 ให้ตรงกับ step นั้นพอดี
    /// เช่น stepAngles มี 3 ค่า → สุ่มได้ 0.0, 0.5, 1.0
    /// </summary>
    float RandomStepValue(FingerToLimbMapper.FingerJointMap map)
    {
        if (map == null || map.stepAngles == null || map.stepAngles.Length <= 1)
            return 0f;

        int steps = map.stepAngles.Length;
        int idx   = _rng.Next(0, steps);
        return idx / (float)(steps - 1);
    }

    public DancePose GenerateRandomPose(string poseName = null)
    {
        DancePose pose = ScriptableObject.CreateInstance<DancePose>();
        pose.poseName  = string.IsNullOrEmpty(poseName)
            ? $"Random_{_rng.Next(1000, 9999)}"
            : poseName;
        pose.tolerance = defaultTolerance;
        pose.holdTime  = defaultHoldTime;

        if (sourceMapper != null)
        {
            // ดึงจำนวนขั้นจาก FingerToLimbMapper โดยตรง
            // ค่าที่ได้จะตรงกับ stepAngles ของ mapper เสมอ ไม่มีการตั้งซ้ำ
            pose.leftArmLift   = RandomStepValue(sourceMapper.leftArmLift);
            pose.leftArmSwing  = RandomStepValue(sourceMapper.leftArmSwing);
            pose.leftLegLift   = RandomStepValue(sourceMapper.leftLegLift);
            pose.leftLegSwing  = RandomStepValue(sourceMapper.leftLegSwing);
            pose.rightArmLift  = RandomStepValue(sourceMapper.rightArmLift);
            pose.rightArmSwing = RandomStepValue(sourceMapper.rightArmSwing);
            pose.rightLegLift  = RandomStepValue(sourceMapper.rightLegLift);
            pose.rightLegSwing = RandomStepValue(sourceMapper.rightLegSwing);
        }
        else
        {
            Debug.LogWarning("[RandomPoseGenerator] sourceMapper เป็น null — ใช้การสุ่มแบบ continuous แทน");
            pose.leftArmLift   = (float)_rng.NextDouble();
            pose.leftArmSwing  = (float)_rng.NextDouble();
            pose.leftLegLift   = (float)_rng.NextDouble();
            pose.leftLegSwing  = (float)_rng.NextDouble();
            pose.rightArmLift  = (float)_rng.NextDouble();
            pose.rightArmSwing = (float)_rng.NextDouble();
            pose.rightLegLift  = (float)_rng.NextDouble();
            pose.rightLegSwing = (float)_rng.NextDouble();
        }

        return pose;
    }

    public long TotalPossibleCombinations()
    {
        if (sourceMapper == null) return 0;
        long s(FingerToLimbMapper.FingerJointMap m) => m?.stepAngles?.Length ?? 1;
        return s(sourceMapper.leftArmLift)  * s(sourceMapper.leftArmSwing)
             * s(sourceMapper.leftLegLift)  * s(sourceMapper.leftLegSwing)
             * s(sourceMapper.rightArmLift) * s(sourceMapper.rightArmSwing)
             * s(sourceMapper.rightLegLift) * s(sourceMapper.rightLegSwing);
    }

#if UNITY_EDITOR
    [Header("Editor Tool: บันทึกท่าสุ่มเป็นไฟล์ .asset (ไม่บังคับ)")]
    public int    batchGenerateCount = 10;
    public string saveFolder         = "Assets/DancePoses/Random";

    [ContextMenu("สุ่มหลายท่า + บันทึกเป็นไฟล์ (Editor Only)")]
    public void BatchGenerateAndSaveAssets()
    {
        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        for (int i = 0; i < batchGenerateCount; i++)
        {
            DancePose p = GenerateRandomPose($"Random_{i + 1:000}");
            string path = AssetDatabase.GenerateUniqueAssetPath($"{saveFolder}/{p.poseName}.asset");
            AssetDatabase.CreateAsset(p, path);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[RandomPoseGenerator] Saved {batchGenerateCount} poses to {saveFolder}");
    }
#endif
}