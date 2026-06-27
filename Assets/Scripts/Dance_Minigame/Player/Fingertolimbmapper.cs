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
        [Tooltip("ติ๊ก = นิ้วเหยียด(0) -> มุมมากสุด, นิ้วงอ(1) -> มุมน้อยสุด (กลับทิศจากปกติ)")]
        public bool invert = false;

        [Header("ช่วงมุมที่หมุนได้จริง (องศา)")]
        [Tooltip("มุมเมื่อค่านิ้ว = 0 (หรือ 1 ถ้า invert)")]
        public float minAngle = -45f;
        [Tooltip("มุมเมื่อค่านิ้ว = 1 (หรือ 0 ถ้า invert)")]
        public float maxAngle = 90f;

        [Header("ความนุ่มนวล")]
        public float smoothSpeed = 8f;

        /// <summary>มุมปัจจุบันที่ smooth แล้ว (องศา)</summary>
        [HideInInspector] public float currentAngle;

        /// <summary>
        /// คำนวณมุมเป้าหมายจากค่า poseValue (0-1) โดยใช้การตั้งค่าของ map นี้
        /// ใช้โดย PoseSilhouetteBuilder เพื่อขยับ SilhouetteCharacter ให้ตรงท่าเดียวกัน
        /// </summary>
        public float GetTargetAngle(float poseValue)
        {
            float t = invert ? (1f - poseValue) : poseValue;
            return Mathf.Lerp(minAngle, maxAngle, Mathf.Clamp01(t));
        }

        /// <summary>คำนวณและหมุน joint ตามค่านิ้ว (0-1)</summary>
        public void Apply(float fingerValue, float dt)
        {
            if (joint == null) return;

            float t = invert ? (1f - fingerValue) : fingerValue;
            float target = Mathf.Lerp(minAngle, maxAngle, Mathf.Clamp01(t));

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
        /// แปลง currentAngle กลับเป็นค่า 0-1 (0 = ตรง minAngle, 1 = ตรง maxAngle)
        /// ใช้สำหรับเทียบกับท่าเป้าหมาย (DancePose)
        /// </summary>
        public float NormalizedAngle()
        {
            if (Mathf.Approximately(maxAngle, minAngle)) return 0f;
            return Mathf.InverseLerp(minAngle, maxAngle, currentAngle);
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