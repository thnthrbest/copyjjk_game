using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

/// <summary>
/// สุ่มชิ้นส่วนเดียวต่อท่า แล้วใช้ค่าเดียวกันนั้นทำ 2 อย่างพร้อมกัน:
///   1) ส่งให้ DancePoseEvaluator ใช้เป็นเป้าหมายเช็คท่าจริงของผู้เล่น
///   2) ส่งให้ PoseSilhouetteBuilder ใช้วาดเงาใบ้ท่าให้ผู้เล่นทำตาม
///
/// ดึงจำนวนขั้นจาก FingerToLimbMapper โดยตรง ไม่ต้องตั้งซ้ำ
/// รองรับการจำกัดว่าเพลงนี้สุ่มได้แค่ส่วนไหน (แขนซ้าย/ขวา, ขาซ้าย/ขวา)
/// </summary>
public class RandomPoseGenerator : MonoBehaviour
{
    public enum LimbPart
    {
        LeftArmLift, LeftArmSwing,
        RightArmLift, RightArmSwing,
        LeftLegLift, LeftLegSwing,
        RightLegLift, RightLegSwing
    }

    [Header("ดึงการตั้งค่าจาก FingerToLimbMapper")]
    [Tooltip("ลาก FingerToLimbMapper ที่ติดอยู่บน HandSystem มาใส่")]
    public FingerToLimbMapper sourceMapper;

    [Header("ค่าเริ่มต้น")]
    public float defaultTolerance           = 0.2f;
    public float defaultTargetStepTolerance = 0.35f;
    public float defaultHoldTime            = 1.0f;

    [Header("กำหนดว่าเพลงนี้สุ่มได้แค่ส่วนไหนบ้าง (ติ๊กอย่างน้อย 1 อัน)")]
    public bool allowLeftArm  = true;
    public bool allowRightArm = true;
    public bool allowLeftLeg  = true;
    public bool allowRightLeg = true;

    [Header("ปลายทางที่จะรับท่าเดียวกัน (ต้องใส่ทั้งคู่)")]
    public DancePoseEvaluator    evaluator;
    public PoseSilhouetteBuilder silhouetteBuilder;

    [Header("เริ่มสุ่มท่าแรกอัตโนมัติตอน Start หรือไม่")]
    public bool autoStartOnPlay = true;

    public DancePose CurrentChallenge { get; private set; }
    public LimbPart  CurrentLimb      { get; private set; }

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
    // สุ่มชิ้นส่วนเดียว → อัปเดต evaluator + silhouette
    // ────────────────────────────────────────────────

    [ContextMenu("สุ่มท่าใหม่ทันที")]
    public DancePose GenerateNewChallenge()
    {
        // 1) รวบรวม LimbPart ที่อนุญาตให้สุ่ม
        var allowed = new System.Collections.Generic.List<LimbPart>();
        if (allowLeftArm)  { allowed.Add(LimbPart.LeftArmLift);  allowed.Add(LimbPart.LeftArmSwing);  }
        if (allowRightArm) { allowed.Add(LimbPart.RightArmLift); allowed.Add(LimbPart.RightArmSwing); }
        if (allowLeftLeg)  { allowed.Add(LimbPart.LeftLegLift);  allowed.Add(LimbPart.LeftLegSwing);  }
        if (allowRightLeg) { allowed.Add(LimbPart.RightLegLift); allowed.Add(LimbPart.RightLegSwing); }

        if (allowed.Count == 0)
        {
            Debug.LogWarning("[RandomPoseGenerator] ไม่ได้ติ๊ก allow ไว้เลย สุ่มจากทุกส่วนแทน");
            foreach (LimbPart p in System.Enum.GetValues(typeof(LimbPart)))
                allowed.Add(p);
        }

        // 2) สุ่มชิ้นส่วน
        CurrentLimb = allowed[_rng.Next(allowed.Count)];

        // 3) สุ่มค่า step
        float targetValue = RandomStepValue(GetMap(CurrentLimb));

        // 4) สร้าง DancePose ที่มีค่าแค่ชิ้นส่วนที่สุ่ม ที่เหลือ = -1 (ไม่เช็ค)
        DancePose pose = ScriptableObject.CreateInstance<DancePose>();
        pose.poseName                = $"{CurrentLimb}_{_rng.Next(100, 999)}";
        pose.tolerance               = defaultTolerance;
        pose.targetStepTolerance     = defaultTargetStepTolerance;
        pose.holdTime                = defaultHoldTime;

        pose.leftArmLift   = -1f; pose.leftArmSwing  = -1f;
        pose.leftLegLift   = -1f; pose.leftLegSwing  = -1f;
        pose.rightArmLift  = -1f; pose.rightArmSwing = -1f;
        pose.rightLegLift  = -1f; pose.rightLegSwing = -1f;

        SetPoseValue(pose, CurrentLimb, targetValue);

        CurrentChallenge = pose;

        // 5) ส่งให้ evaluator และ silhouette
        if (evaluator        != null) evaluator.SetPose(pose);
        if (silhouetteBuilder != null)
        {
            silhouetteBuilder.ResetHighlight();
            silhouetteBuilder.ApplyPartialPose(CurrentLimb, targetValue);
        }

        Debug.Log($"[RandomPoseGenerator] Limb: {CurrentLimb} | Value: {targetValue:F2}");
        return pose;
    }

    /// <summary>ให้ MinigameTrigger เรียกก่อนเริ่มเกม เพื่อกำหนดว่าสุ่มได้แค่ส่วนไหน</summary>
    public void SetAllowedLimbs(bool leftArm, bool rightArm, bool leftLeg, bool rightLeg)
    {
        allowLeftArm  = leftArm;
        allowRightArm = rightArm;
        allowLeftLeg  = leftLeg;
        allowRightLeg = rightLeg;
    }

    // ────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────

    float RandomStepValue(FingerToLimbMapper.FingerJointMap map)
    {
        if (map == null || map.stepAngles == null || map.stepAngles.Length <= 1) return 0f;
        int steps = map.stepAngles.Length;
        int idx   = _rng.Next(0, steps);
        return idx / (float)(steps - 1);
    }

    FingerToLimbMapper.FingerJointMap GetMap(LimbPart part)
    {
        if (sourceMapper == null) return null;
        return part switch
        {
            LimbPart.LeftArmLift   => sourceMapper.leftArmLift,
            LimbPart.LeftArmSwing  => sourceMapper.leftArmSwing,
            LimbPart.RightArmLift  => sourceMapper.rightArmLift,
            LimbPart.RightArmSwing => sourceMapper.rightArmSwing,
            LimbPart.LeftLegLift   => sourceMapper.leftLegLift,
            LimbPart.LeftLegSwing  => sourceMapper.leftLegSwing,
            LimbPart.RightLegLift  => sourceMapper.rightLegLift,
            LimbPart.RightLegSwing => sourceMapper.rightLegSwing,
            _                      => null
        };
    }

    void SetPoseValue(DancePose pose, LimbPart part, float value)
    {
        switch (part)
        {
            case LimbPart.LeftArmLift:   pose.leftArmLift   = value; break;
            case LimbPart.LeftArmSwing:  pose.leftArmSwing  = value; break;
            case LimbPart.RightArmLift:  pose.rightArmLift  = value; break;
            case LimbPart.RightArmSwing: pose.rightArmSwing = value; break;
            case LimbPart.LeftLegLift:   pose.leftLegLift   = value; break;
            case LimbPart.LeftLegSwing:  pose.leftLegSwing  = value; break;
            case LimbPart.RightLegLift:  pose.rightLegLift  = value; break;
            case LimbPart.RightLegSwing: pose.rightLegSwing = value; break;
        }
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
    [Header("Editor Tool")]
    public int    batchGenerateCount = 10;
    public string saveFolder         = "Assets/DancePoses/Random";

    [ContextMenu("สุ่มหลายท่า + บันทึก (Editor Only)")]
    public void BatchSave()
    {
        if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);
        for (int i = 0; i < batchGenerateCount; i++)
        {
            DancePose p = GenerateNewChallenge();
            p.poseName  = $"Partial_{i + 1:000}_{CurrentLimb}";
            string path = AssetDatabase.GenerateUniqueAssetPath($"{saveFolder}/{p.poseName}.asset");
            AssetDatabase.CreateAsset(p, path);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[RandomPoseGenerator] Saved {batchGenerateCount} poses");
    }
#endif
}