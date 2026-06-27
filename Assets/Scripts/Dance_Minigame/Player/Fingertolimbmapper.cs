using UnityEngine;

/// <summary>
/// แปลงค่านิ้ว (0-1) จาก HandDataReceiver เป็นการหมุน (localEulerAngles)
/// ของข้อต่อแขน/ขาแต่ละจุด ตาม mapping:
///
///   มือซ้าย
///     index  -> leftArmLift   (ยกแขนซ้าย ขึ้น/ลง)
///     middle -> leftArmSwing  (หมุนแขนซ้าย หน้า/หลัง)
///     ring   -> leftLegLift   (ยกขาซ้าย ด้านข้าง)
///     pinky  -> leftLegSwing  (ขาซ้าย หน้า/หลัง)
///
///   มือขวา (สมมาตรกัน)
///     index  -> rightArmLift
///     middle -> rightArmSwing
///     ring   -> rightLegLift
///     pinky  -> rightLegSwing
///
///   (นิ้วโป้งทั้งสองข้างไม่ได้ใช้ในตอนนี้)
/// </summary>
public class FingerToLimbMapper : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }

    [System.Serializable]
    public class FingerJointMap
    {
        [Header("ข้อมูล")]
        [Tooltip("ใส่ชื่อไว้ดูง่ายๆ เช่น 'แขนซ้าย-ยกขึ้น'")]
        public string label;

        [Tooltip("GameObject ข้อต่อที่จะหมุน")]
        public Transform joint;

        [Header("แกนที่ต้องการให้นิ้วนี้ควบคุม")]
        public RotationAxis axis = RotationAxis.X;

        [Header("ทิศทาง")]
        [Tooltip("ติ๊ก = กลับทิศการหมุน")]
        public bool invert = false;

        [Header("ขั้นองศา — ใส่กี่ค่าก็ได้ ค่านิ้ว 0-1 จะ interpolate ระหว่างขั้น")]
        [Tooltip("ตัวอย่างแขน 5 ขั้น: -90, -45, 0, 45, 90\nตัวอย่างขา 3 ขั้น: -30, 0, 45")]
        public float[] stepAngles = { -90f, 0f, 90f };

        [Header("ความนุ่มนวล")]
        public float smoothSpeed = 8f;

        /// <summary>มุมปัจจุบันที่ smooth แล้ว (องศา)</summary>
        [HideInInspector] public float currentAngle;

        /// <summary>
        /// แปลงค่านิ้ว (0-1) เป็นองศาโดย interpolate ระหว่าง stepAngles
        /// ค่านิ้ว 0.0 = stepAngles[0], ค่านิ้ว 1.0 = stepAngles[สุดท้าย]
        /// ค่าระหว่างขั้นจะ interpolate ไหลผ่านได้เลย ไม่มีการ snap
        /// </summary>
        public float GetTargetAngle(float fingerValue)
        {
            if (stepAngles == null || stepAngles.Length == 0) return 0f;
            if (stepAngles.Length == 1) return stepAngles[0];

            float t = invert ? (1f - fingerValue) : fingerValue;
            t = Mathf.Clamp01(t);

            // แปลง t (0-1) ให้เป็นตำแหน่งใน stepAngles array
            float scaled = t * (stepAngles.Length - 1);
            int   lo     = Mathf.FloorToInt(scaled);
            int   hi     = Mathf.Min(lo + 1, stepAngles.Length - 1);
            float frac   = scaled - lo;

            return Mathf.Lerp(stepAngles[lo], stepAngles[hi], frac);
        }

        /// <summary>คำนวณและหมุน joint ตามค่านิ้ว (0-1)</summary>
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

        /// <summary>
        /// แปลง currentAngle กลับเป็นค่า 0-1 ตามตำแหน่งใน stepAngles
        /// ใช้สำหรับเทียบกับท่าเป้าหมาย (DancePose)
        /// </summary>
        public float NormalizedAngle()
        {
            if (stepAngles == null || stepAngles.Length <= 1) return 0f;
            float min = stepAngles[0];
            float max = stepAngles[stepAngles.Length - 1];
            if (Mathf.Approximately(min, max)) return 0f;
            return Mathf.InverseLerp(min, max, currentAngle);
        }
    }

    [Header("=== แขน/ขา ซ้าย (ควบคุมด้วยมือซ้าย) ===")]
    [Tooltip("นิ้วชี้ซ้าย")]
    public FingerJointMap leftArmLift;
    [Tooltip("นิ้วกลางซ้าย")]
    public FingerJointMap leftArmSwing;
    [Tooltip("นิ้วนางซ้าย")]
    public FingerJointMap leftLegLift;
    [Tooltip("นิ้วก้อยซ้าย")]
    public FingerJointMap leftLegSwing;

    [Header("=== แขน/ขา ขวา (ควบคุมด้วยมือขวา) ===")]
    [Tooltip("นิ้วชี้ขวา")]
    public FingerJointMap rightArmLift;
    [Tooltip("นิ้วกลางขวา")]
    public FingerJointMap rightArmSwing;
    [Tooltip("นิ้วนางขวา")]
    public FingerJointMap rightLegLift;
    [Tooltip("นิ้วก้อยขวา")]
    public FingerJointMap rightLegSwing;

    void Update()
    {
        float dt = Time.deltaTime;

        FingerData left  = HandDataReceiver.LeftHand;
        FingerData right = HandDataReceiver.RightHand;

        if (left != null)
        {
            leftArmLift.Apply(left.index, dt);
            leftArmSwing.Apply(left.middle, dt);
            leftLegLift.Apply(left.ring, dt);
            leftLegSwing.Apply(left.pinky, dt);
        }

        if (right != null)
        {
            rightArmLift.Apply(right.index, dt);
            rightArmSwing.Apply(right.middle, dt);
            rightLegLift.Apply(right.ring, dt);
            rightLegSwing.Apply(right.pinky, dt);
        }
    }
}