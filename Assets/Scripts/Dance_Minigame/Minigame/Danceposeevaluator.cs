using System;
using UnityEngine;

/// <summary>
/// เทียบท่าปัจจุบันของตัวละคร (จาก FingerToLimbMapper) กับ DancePose เป้าหมาย
/// แบบ "ถือนิ่งทีละท่า ไม่มีเวลาจำกัด"
/// Hold timer แบบ "ใจดี": เพิ่มเร็วตอนแม่น, ลดช้าตอนหลุด
/// </summary>
public class DancePoseEvaluator : MonoBehaviour
{
    [Header("อ้างอิงตัวควบคุมแขนขา")]
    public FingerToLimbMapper mapper;

    [Header("ท่าเป้าหมายปัจจุบัน")]
    public DancePose currentPose;

    [Header("สถานะ (อ่านอย่างเดียว)")]
    [Range(0, 1)] public float accuracy;
    [Range(0, 1)] public float holdProgress;
    public bool isPoseComplete;

    [Header("ระบบช่วยเหลือ (Assist)")]
    [Tooltip("ถ้าค้างนานเกินนี้ (วินาที) โดยยังไม่ผ่าน จะ trigger OnNeedHint")]
    public float hintAfterSeconds = 10f;

    public event Action<float> OnAccuracyChanged;
    public event Action<int>   OnPoseComplete;
    public event Action<string> OnNeedHint;

    float _holdTimer;
    float _stuckTimer;
    bool  _completed;
    bool  _hintShown;

    void Update()
    {
        if (currentPose == null || mapper == null) return;

        // คำนวณความแม่นยำของแต่ละจุด — ชื่อ field ตรงกับ FingerToLimbMapper
        float accLeftArmLift   = Score(mapper.leftArmLift.NormalizedAngle(),   currentPose.leftArmLift,   currentPose.tolerance);
        float accLeftArmSwing  = Score(mapper.leftArmSwing.NormalizedAngle(),  currentPose.leftArmSwing,  currentPose.tolerance);
        float accLeftLegLift   = Score(mapper.leftLegLift.NormalizedAngle(),   currentPose.leftLegLift,   currentPose.tolerance);
        float accLeftLegSwing  = Score(mapper.leftLegSwing.NormalizedAngle(),  currentPose.leftLegSwing,  currentPose.tolerance);
        float accRightArmLift  = Score(mapper.rightArmLift.NormalizedAngle(),  currentPose.rightArmLift,  currentPose.tolerance);
        float accRightArmSwing = Score(mapper.rightArmSwing.NormalizedAngle(), currentPose.rightArmSwing, currentPose.tolerance);
        float accRightLegLift  = Score(mapper.rightLegLift.NormalizedAngle(),  currentPose.rightLegLift,  currentPose.tolerance);
        float accRightLegSwing = Score(mapper.rightLegSwing.NormalizedAngle(), currentPose.rightLegSwing, currentPose.tolerance);

        accuracy = (accLeftArmLift + accLeftArmSwing + accLeftLegLift + accLeftLegSwing +
                    accRightArmLift + accRightArmSwing + accRightLegLift + accRightLegSwing) / 8f;

        OnAccuracyChanged?.Invoke(accuracy);

        if (_completed) return;

        // Hold timer แบบใจดี
        bool inThreshold = accuracy >= (1f - currentPose.tolerance);
        if (inThreshold)
            _holdTimer += Time.deltaTime;
        else
            _holdTimer -= Time.deltaTime * 0.5f;

        _holdTimer   = Mathf.Clamp(_holdTimer, 0f, currentPose.holdTime);
        holdProgress = currentPose.holdTime > 0f ? _holdTimer / currentPose.holdTime : 1f;

        // Assist เมื่อติดนาน
        if (!inThreshold)
        {
            _stuckTimer += Time.deltaTime;
            if (_stuckTimer >= hintAfterSeconds && !_hintShown)
            {
                _hintShown = true;
                OnNeedHint?.Invoke(FindWorstLimb(
                    accLeftArmLift, accLeftArmSwing, accLeftLegLift, accLeftLegSwing,
                    accRightArmLift, accRightArmSwing, accRightLegLift, accRightLegSwing));
            }
        }
        else
        {
            _stuckTimer = 0f;
            _hintShown  = false;
        }

        // ผ่านท่า
        if (_holdTimer >= currentPose.holdTime)
        {
            _completed     = true;
            isPoseComplete = true;
            OnPoseComplete?.Invoke(CalcStars(accuracy));
        }
    }

    /// <summary>เริ่มประเมินท่าใหม่ (เรียกตอนเปลี่ยนเงาเป็นท่าถัดไป)</summary>
    public void SetPose(DancePose newPose)
    {
        currentPose    = newPose;
        _holdTimer     = 0f;
        _stuckTimer    = 0f;
        _completed     = false;
        isPoseComplete = false;
        _hintShown     = false;
        accuracy       = 0f;
        holdProgress   = 0f;
    }

    float Score(float actual, float target, float tolerance)
    {
        float diff = Mathf.Abs(actual - target);
        return Mathf.Clamp01(1f - diff / tolerance);
    }

    int CalcStars(float acc)
    {
        if (acc >= 0.95f) return 3;
        if (acc >= 0.85f) return 2;
        return 1;
    }

    string FindWorstLimb(float a, float b, float c, float d, float e, float f, float g, float h)
    {
        string[] names  = { "leftArmLift", "leftArmSwing", "leftLegLift", "leftLegSwing",
                             "rightArmLift", "rightArmSwing", "rightLegLift", "rightLegSwing" };
        float[]  values = { a, b, c, d, e, f, g, h };

        int worstIndex = 0;
        for (int i = 1; i < values.Length; i++)
            if (values[i] < values[worstIndex]) worstIndex = i;

        return names[worstIndex];
    }
}