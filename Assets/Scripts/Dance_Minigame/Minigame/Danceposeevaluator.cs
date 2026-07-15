using System;
using UnityEngine;

/// <summary>
/// เทียบท่าปัจจุบันของตัวละคร (จาก FingerToLimbMapper) กับ DancePose เป้าหมาย
/// รองรับโหมดชิ้นส่วนเดียว: ถ้า DancePose field ไหนมีค่า -1 จะข้ามไม่เช็ค
/// คำนวณ accuracy จากเฉพาะชิ้นส่วนที่มีค่า >= 0 เท่านั้น
///
/// จุดควบคุมใหม่ (6 จุด):
///   leftArmLift, leftArmSwing, leftLegSwing
///   rightArmLift, rightArmSwing, rightLegSwing
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

    [Header("ระบบช่วยเหลือ")]
    public float hintAfterSeconds = 10f;

    public event Action<float>  OnAccuracyChanged;
    public event Action<int>    OnPoseComplete;
    public event Action<string> OnNeedHint;

    float _holdTimer;
    float _stuckTimer;
    bool  _completed;
    bool  _hintShown;
    string _worstLimb = "";

    void Update()
    {
        if (currentPose == null || mapper == null) return;

        float total    = 0f;
        int   count    = 0;
        float worstAcc = 1f;

        void Check(float actual, float target, string limbName)
        {
            if (target < 0f) return;
            // ชิ้นส่วนที่สุ่มมา (target >= 0) ใช้ targetStepTolerance ที่กว้างกว่า
            float s = ScoreWithTolerance(actual, target, currentPose.targetStepTolerance);
            total += s;
            count++;
            if (s < worstAcc) { worstAcc = s; _worstLimb = limbName; }
        }

        Check(mapper.leftArmLift.NormalizedAngle(),   currentPose.leftArmLift,   "leftArmLift");
        Check(mapper.leftArmSwing.NormalizedAngle(),  currentPose.leftArmSwing,  "leftArmSwing");
        Check(mapper.leftLegSwing.NormalizedAngle(),  currentPose.leftLegSwing,  "leftLegSwing");
        Check(mapper.rightArmLift.NormalizedAngle(),  currentPose.rightArmLift,  "rightArmLift");
        Check(mapper.rightArmSwing.NormalizedAngle(), currentPose.rightArmSwing, "rightArmSwing");
        Check(mapper.rightLegSwing.NormalizedAngle(), currentPose.rightLegSwing, "rightLegSwing");

        accuracy = count > 0 ? total / count : 1f;
        OnAccuracyChanged?.Invoke(accuracy);

        if (_completed) return;

        bool inThreshold = accuracy >= (1f - currentPose.tolerance);
        _holdTimer  += inThreshold ? Time.deltaTime : -Time.deltaTime * 0.5f;
        _holdTimer   = Mathf.Clamp(_holdTimer, 0f, currentPose.holdTime);
        holdProgress = currentPose.holdTime > 0f ? _holdTimer / currentPose.holdTime : 1f;

        if (!inThreshold)
        {
            _stuckTimer += Time.deltaTime;
            if (_stuckTimer >= hintAfterSeconds && !_hintShown)
            {
                _hintShown = true;
                OnNeedHint?.Invoke(_worstLimb);
            }
        }
        else
        {
            _stuckTimer = 0f;
            _hintShown  = false;
        }

        if (_holdTimer >= currentPose.holdTime)
        {
            _completed     = true;
            isPoseComplete = true;
            OnPoseComplete?.Invoke(CalcStars(accuracy));
        }
    }

    public void SetPose(DancePose newPose)
    {
        currentPose    = newPose;
        _holdTimer     = 0f;
        _stuckTimer    = 0f;
        _completed     = false;
        isPoseComplete = false;
        _hintShown     = false;
        _worstLimb     = "";
        accuracy       = 0f;
        holdProgress   = 0f;
    }

    float ScoreWithTolerance(float actual, float target, float tolerance)
    {
        float diff = Mathf.Abs(actual - target);
        return Mathf.Clamp01(1f - diff / Mathf.Max(tolerance, 0.001f));
    }

    int CalcStars(float acc)
    {
        if (acc >= 0.95f) return 3;
        if (acc >= 0.85f) return 2;
        return 1;
    }
}