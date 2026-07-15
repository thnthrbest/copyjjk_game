using UnityEngine;

/// <summary>
/// Mapping นิ้วมือ → แขนขา (เวอร์ชันใหม่):
///   นิ้วโป้ง  → ขาหน้า/หลัง
///   นิ้วชี้   → แขนหน้า/หลัง
///   นิ้วกลาง → แขนขึ้น/ลง
///   นิ้วนาง  → ไม่ใช้
///   นิ้วก้อย → ไม่ใช้
///
///   ซ้ายคุมซ้าย ขวาคุมขวา (สมมาตร)
///   ไม่มีการยกขาด้านข้างอีกต่อไป
/// </summary>
public class FingerToLimbMapper : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }

    [System.Serializable]
    public class FingerJointMap
    {
        [Header("ข้อมูล")]
        [Tooltip("ชื่อไว้ดูง่ายๆ เช่น 'แขนซ้าย-ยกขึ้น'")]
        public string label;

        [Tooltip("GameObject ข้อต่อที่จะหมุน")]
        public Transform joint;

        [Header("แกนที่ต้องการให้นิ้วนี้ควบคุม")]
        public RotationAxis axis = RotationAxis.X;

        [Header("ทิศทาง")]
        [Tooltip("ติ๊ก = กลับทิศการหมุน")]
        public bool invert = false;

        [Header("ขั้นองศา — ใส่กี่ค่าก็ได้ ค่านิ้ว 0-1 จะ interpolate ระหว่างขั้น")]
        [Tooltip("ตัวอย่างแขน 3 ขั้น: -90, 0, 90\nตัวอย่างขา 3 ขั้น: -45, 0, 45")]
        public float[] stepAngles = { -90f, 0f, 90f };

        [Header("ความนุ่มนวล")]
        public float smoothSpeed = 8f;

        [HideInInspector] public float currentAngle;

        public float GetTargetAngle(float fingerValue)
        {
            if (stepAngles == null || stepAngles.Length == 0) return 0f;
            if (stepAngles.Length == 1) return stepAngles[0];

            float t      = invert ? (1f - fingerValue) : fingerValue;
            t            = Mathf.Clamp01(t);
            float scaled = t * (stepAngles.Length - 1);
            int   lo     = Mathf.FloorToInt(scaled);
            int   hi     = Mathf.Min(lo + 1, stepAngles.Length - 1);
            float frac   = scaled - lo;

            return Mathf.Lerp(stepAngles[lo], stepAngles[hi], frac);
        }

        public void Apply(float fingerValue, float dt)
        {
            if (joint == null) return;

            float target = GetTargetAngle(fingerValue);
            currentAngle = Mathf.LerpAngle(currentAngle, target, dt * smoothSpeed);

            Vector3 e = joint.localEulerAngles;
            switch (axis)
            {
                case RotationAxis.X: e.x = currentAngle; break;
                case RotationAxis.Y: e.y = currentAngle; break;
                case RotationAxis.Z: e.z = currentAngle; break;
            }
            joint.localEulerAngles = e;
        }

        public float NormalizedAngle()
        {
            if (stepAngles == null || stepAngles.Length <= 1) return 0f;
            float min = stepAngles[0];
            float max = stepAngles[stepAngles.Length - 1];
            if (Mathf.Approximately(min, max)) return 0f;
            return Mathf.InverseLerp(min, max, currentAngle);
        }
    }

    [Header("=== ฝั่งซ้าย (ควบคุมด้วยมือซ้าย) ===")]
    [Tooltip("นิ้วกลางซ้าย — แขนซ้ายขึ้น/ลง")]
    public FingerJointMap leftArmLift;
    [Tooltip("นิ้วชี้ซ้าย — แขนซ้ายหน้า/หลัง")]
    public FingerJointMap leftArmSwing;
    [Tooltip("นิ้วโป้งซ้าย — ขาซ้ายหน้า/หลัง")]
    public FingerJointMap leftLegSwing;

    [Header("=== ฝั่งขวา (ควบคุมด้วยมือขวา) ===")]
    [Tooltip("นิ้วกลางขวา — แขนขวาขึ้น/ลง")]
    public FingerJointMap rightArmLift;
    [Tooltip("นิ้วชี้ขวา — แขนขวาหน้า/หลัง")]
    public FingerJointMap rightArmSwing;
    [Tooltip("นิ้วโป้งขวา — ขาขวาหน้า/หลัง")]
    public FingerJointMap rightLegSwing;

    void Update()
    {
        float dt = Time.deltaTime;

        FingerData left  = HandDataReceiver.LeftHand;
        FingerData right = HandDataReceiver.RightHand;

        if (HandDataReceiver.LeftHandDetected)
        {
            leftArmLift.Apply(left.middle, dt);   // นิ้วกลาง → แขนขึ้น/ลง
            leftArmSwing.Apply(left.index,  dt);   // นิ้วชี้   → แขนหน้า/หลัง
            leftLegSwing.Apply(left.thumb,  dt);   // นิ้วโป้ง  → ขาหน้า/หลัง
        }

        if (HandDataReceiver.RightHandDetected)
        {
            rightArmLift.Apply(right.middle, dt);
            rightArmSwing.Apply(right.index,  dt);
            rightLegSwing.Apply(right.thumb,  dt);
        }
    }
}