using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

/// <summary>
/// สุ่มชิ้นส่วนเดียวต่อท่า แล้วให้ผู้เล่นขยับเฉพาะส่วนนั้น
/// ชิ้นส่วนที่สุ่มได้จะเปลี่ยนสีบน SilhouetteCharacter เพื่อบอกผู้เล่น
/// ชิ้นส่วนอื่นคงท่าเดิมไว้ ไม่ต้องขยับและไม่นำมาเช็ค
/// </summary>
public class RandomPoseGenerator : MonoBehaviour
{
    // ชิ้นส่วนที่สุ่มได้ — ตรงกับ field ใน FingerToLimbMapper และ DancePose
    public enum LimbPart
    {
        LeftArmLift, LeftArmSwing,
        RightArmLift, RightArmSwing,
        LeftLegLift, LeftLegSwing,
        RightLegLift, RightLegSwing
    }

    /// <summary>กลุ่ม limb ที่อนุญาตให้สุ่ม (ใช้ติ๊กใน Inspector)</summary>
    public enum LimbGroup
    {
        LeftArm,    // นิ้วชี้ + นิ้วกลาง ฝั่งซ้าย
        RightArm,   // นิ้วชี้ + นิ้วกลาง ฝั่งขวา
        LeftLeg,    // นิ้วนาง + นิ้วก้อย ฝั่งซ้าย
        RightLeg,   // นิ้วนาง + นิ้วก้อย ฝั่งขวา
    }

    [Header("ดึงการตั้งค่าจาก FingerToLimbMapper")]
    public FingerToLimbMapper sourceMapper;

    [Header("ค่าเริ่มต้น")]
    public float defaultTolerance = 0.2f;
    public float defaultHoldTime  = 1.0f;

    [Header("กำหนดว่าเพลงนี้สุ่มได้แค่ส่วนไหนบ้าง (ติ๊กอย่างน้อย 1 อัน)")]
    public bool allowLeftArm  = true;
    public bool allowRightArm = true;
    public bool allowLeftLeg  = true;
    public bool allowRightLeg = true;

    [Header("ปลายทาง")]
    public DancePoseEvaluator    evaluator;
    public PoseSilhouetteBuilder silhouetteBuilder;

    [Header("สีไฮไลต์ชิ้นส่วนที่ต้องขยับ")]
    public Color highlightColor = Color.yellow;

    [Header("เริ่มอัตโนมัติตอน Start")]
    public bool autoStartOnPlay = true;

    /// <summary>ชิ้นส่วนที่สุ่มได้ในรอบปัจจุบัน</summary>
    public LimbPart CurrentLimb { get; private set; }

    /// <summary>ท่าปัจจุบัน (มีค่าแค่ชิ้นส่วนที่สุ่ม ที่เหลือ = 0)</summary>
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
    // สุ่มชิ้นส่วนเดียว → อัปเดต evaluator + silhouette
    // ────────────────────────────────────────────────

    [ContextMenu("สุ่มท่าใหม่ทันที")]
    public DancePose GenerateNewChallenge()
    {
        // 1) รวบรวม LimbPart ที่อนุญาตให้สุ่มตามที่ติ๊กไว้
        var allowed = new System.Collections.Generic.List<LimbPart>();
        if (allowLeftArm)  { allowed.Add(LimbPart.LeftArmLift);  allowed.Add(LimbPart.LeftArmSwing);  }
        if (allowRightArm) { allowed.Add(LimbPart.RightArmLift); allowed.Add(LimbPart.RightArmSwing); }
        if (allowLeftLeg)  { allowed.Add(LimbPart.LeftLegLift);  allowed.Add(LimbPart.LeftLegSwing);  }
        if (allowRightLeg) { allowed.Add(LimbPart.RightLegLift); allowed.Add(LimbPart.RightLegSwing); }

        // ถ้าไม่ได้ติ๊กอะไรเลย ให้สุ่มจากทั้งหมด (fallback)
        if (allowed.Count == 0)
        {
            Debug.LogWarning("[RandomPoseGenerator] ไม่ได้ติ๊ก allow ไว้เลย สุ่มจากทุกส่วนแทน");
            foreach (LimbPart p in System.Enum.GetValues(typeof(LimbPart)))
                allowed.Add(p);
        }

        // 2) สุ่มจาก list ที่อนุญาต
        CurrentLimb = allowed[_rng.Next(allowed.Count)];

        // 2) สุ่มค่าองศา (step) ของชิ้นส่วนนั้น
        float targetValue = RandomStepValue(GetMap(CurrentLimb));

        // 3) สร้าง DancePose ที่มีค่าแค่ชิ้นส่วนที่สุ่ม ที่เหลือ = -1 (ไม่เช็ค)
        DancePose pose   = ScriptableObject.CreateInstance<DancePose>();
        pose.poseName    = $"{CurrentLimb}_{_rng.Next(100, 999)}";
        pose.tolerance   = defaultTolerance;
        pose.holdTime    = defaultHoldTime;

        // ตั้งทุกอันเป็น -1 ก่อน (หมายถึง "ไม่ต้องเช็ค")
        pose.leftArmLift   = -1f; pose.leftArmSwing  = -1f;
        pose.leftLegLift   = -1f; pose.leftLegSwing  = -1f;
        pose.rightArmLift  = -1f; pose.rightArmSwing = -1f;
        pose.rightLegLift  = -1f; pose.rightLegSwing = -1f;

        // ใส่ค่าเฉพาะชิ้นส่วนที่สุ่มมา
        SetPoseValue(pose, CurrentLimb, targetValue);

        CurrentChallenge = pose;

        // 4) ส่งให้ evaluator เช็คเฉพาะชิ้นส่วนนั้น
        if (evaluator != null)
            evaluator.SetPose(pose);

        // 5) บอก silhouette ให้ไฮไลต์ชิ้นส่วนที่ต้องขยับ
        if (silhouetteBuilder != null)
        {
            silhouetteBuilder.ResetHighlight();
            silhouetteBuilder.ApplyPartialPose(CurrentLimb, targetValue);
        }

        Debug.Log($"[RandomPoseGenerator] Limb: {CurrentLimb} | Value: {targetValue:F2}");
        return pose;
    }

    // ────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────

    /// <summary>
    /// ให้ MinigameTrigger เรียกก่อนเริ่มเกม เพื่อกำหนดว่าเพลงนี้สุ่มได้แค่ส่วนไหน
    /// </summary>
    public void SetAllowedLimbs(bool leftArm, bool rightArm, bool leftLeg, bool rightLeg)
    {
        allowLeftArm  = leftArm;
        allowRightArm = rightArm;
        allowLeftLeg  = leftLeg;
        allowRightLeg = rightLeg;
    }

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
        Debug.Log($"[RandomPoseGenerator] Saved {batchGenerateCount} partial poses");
    }
#endif
}