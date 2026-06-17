using UnityEngine;

public class FingerToLimbMapper : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }

    [System.Serializable]
    public class FingerJointMap
    {
        [Header("ข้อมูล")]
        public string label;
        public Transform joint;

        [Header("แกนที่ต้องการให้นิ้วนี้ควบคุม")]
        public RotationAxis axis = RotationAxis.X;

        [Header("ทิศทาง")]
        [Tooltip("ติ๊ก = กลับทิศ (นิ้วเหยียด = stepAngles ท้าย, นิ้วงอ = stepAngles ต้น)")]
        public bool invert = false;

        [Header("องศาของแต่ละขั้น (ใส่กี่ค่าก็ได้)")]
        [Tooltip("ค่าจะถูก interpolate ตามค่านิ้ว 0.0-1.0 โดยตรง ไม่มีการสร้างค่ากลางเพิ่ม")]
        public float[] stepAngles = { -90f, -45f, 0f, 45f, 90f };

        [Header("ความนุ่มนวล")]
        public float smoothSpeed = 8f;

        [HideInInspector] public float currentAngle;

        float GetTargetAngle(float fingerValue)
        {
            if (stepAngles == null || stepAngles.Length == 0) return 0f;
            if (stepAngles.Length == 1) return stepAngles[0];

            float t = invert ? (1f - fingerValue) : fingerValue;
            t = Mathf.Clamp01(t);

            float scaled = t * (stepAngles.Length - 1);
            int lowerIndex = Mathf.FloorToInt(scaled);
            int upperIndex = Mathf.Min(lowerIndex + 1, stepAngles.Length - 1);
            float blend = scaled - lowerIndex;

            return Mathf.Lerp(stepAngles[lowerIndex], stepAngles[upperIndex], blend);
        }

        public void Apply(float fingerValue, float deltaTime)
        {
            if (joint == null) return;

            float targetAngle = GetTargetAngle(fingerValue);
            currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, deltaTime * smoothSpeed);

            Vector3 euler = joint.localEulerAngles;
            switch (axis)
            {
                case RotationAxis.X: euler.x = currentAngle; break;
                case RotationAxis.Y: euler.y = currentAngle; break;
                case RotationAxis.Z: euler.z = currentAngle; break;
            }
            joint.localEulerAngles = euler;
        }

        /// <summary>คืนค่า 0-1 เทียบกับช่วง stepAngles ทั้งหมด (ใช้ใน DancePoseEvaluator)</summary>
        public float NormalizedAngle()
        {
            if (stepAngles == null || stepAngles.Length < 2) return 0f;
            float min = stepAngles[0];
            float max = stepAngles[stepAngles.Length - 1];
            if (Mathf.Approximately(min, max)) return 0f;
            return Mathf.InverseLerp(min, max, currentAngle);
        }
    }

    [Header("=== มือซ้าย ===")]
    public FingerJointMap leftArmLift;      // นิ้วชี้ซ้าย   → ยกแขนซ้าย ขึ้น/ลง
    public FingerJointMap leftArmSwing;     // นิ้วกลางซ้าย  → หมุนแขนซ้าย หน้า/หลัง
    public FingerJointMap leftLegLift;      // นิ้วนางซ้าย   → ยกขาซ้าย ด้านข้าง
    public FingerJointMap leftLegSwing;     // นิ้วก้อยซ้าย  → ขาซ้าย หน้า/หลัง

    [Header("=== มือขวา ===")]
    public FingerJointMap rightArmLift;     // นิ้วชี้ขวา   → ยกแขนขวา ขึ้น/ลง
    public FingerJointMap rightArmSwing;    // นิ้วกลางขวา  → หมุนแขนขวา หน้า/หลัง
    public FingerJointMap rightLegLift;     // นิ้วนางขวา   → ยกขาขวา ด้านข้าง
    public FingerJointMap rightLegSwing;    // นิ้วก้อยขวา  → ขาขวา หน้า/หลัง

    void Update()
    {
        float dt = Time.deltaTime;

        // ใช้ FingerData ให้ตรงกับ HandDataReceiver
        FingerData left  = HandDataReceiver.LeftHand;
        FingerData right = HandDataReceiver.RightHand;

        if (HandDataReceiver.LeftHandDetected)
        {
            leftArmLift.Apply(left.index,  dt);
            leftArmSwing.Apply(left.middle, dt);
            leftLegLift.Apply(left.ring,   dt);
            leftLegSwing.Apply(left.pinky,  dt);
        }

        if (HandDataReceiver.RightHandDetected)
        {
            rightArmLift.Apply(right.index,  dt);
            rightArmSwing.Apply(right.middle, dt);
            rightLegLift.Apply(right.ring,   dt);
            rightLegSwing.Apply(right.pinky,  dt);
        }
    }
}