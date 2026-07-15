using System.Collections;
using UnityEngine;

public class BossSkillSlam : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator bossAnimator;          // ตัว Animator ของบอส
    [SerializeField] private string rightSlamTrigger = "SlamRight"; // ชื่อ Trigger แขนขวา
    [SerializeField] private string leftSlamTrigger = "SlamLeft";   // ชื่อ Trigger แขนซ้าย

    [Header("Prefab Settings")]
    [SerializeField] private GameObject slamHitboxPrefab; // Prefab ของดาเมจ/เอฟเฟกต์ตอนทุบ

    [Header("Attach Targets (กระดูกโมเดลบอส)")]
    [SerializeField] private Transform rightHandBone;     // ลากกระดูกมือขวาของบอสมาใส่ที่นี่
    [SerializeField] private Transform leftHandBone;      // ลากกระดูกมือซ้ายของบอสมาใส่ที่นี่

    [Header("Timing Settings")]
    [SerializeField] private float attackDuration = 2.0f;     // 🔥 เวลาที่ใช้ในการทุบ+ลากจนจบ (เปิด Hitbox ค้างไว้เท่านี้)
    [SerializeField] private float delayBetweenSlams = 1.0f;  // เวลารอคั่นกลางระหว่างแขนขวากับแขนซ้าย

    public IEnumerator ExecuteSkill()
    {
        Debug.Log("<color=orange>-> [Skill 1] เริ่มทำงาน: ทุบพื้นและลากตามอนิเมชันโมเดล!</color>");

        // ===================================================
        // --- จังหวะที่ 1: แขนขวาทำงาน ---
        // ===================================================
        if (bossAnimator != null) bossAnimator.SetTrigger(rightSlamTrigger);

        // เสก Hitbox เข้าไปเป็นลูกของกระดูกมือขวาทันที
        GameObject rightHitbox = null;
        if (rightHandBone != null && slamHitboxPrefab != null)
        {
            // การใส่ rightHandBone ไว้ข้างหลังตอน Instantiate จะทำให้วัตถุเกิดมาเป็นลูก (Child) ของมือขวาทันที
            rightHitbox = Instantiate(slamHitboxPrefab, rightHandBone.position, rightHandBone.rotation, rightHandBone);
            // รีเซ็ตตำแหน่ง Local ให้ตรงกับตำแหน่งมือพอดี (ปรับเปลี่ยนเพิ่มเติมใน Inspector ของ Prefab ได้)
            rightHitbox.transform.localPosition = Vector3.zero; 
        }

        // เปิด Hitbox ค้างไว้ปล่อยให้อนิเมชันลากมือไปตามเรื่องของมัน
        yield return new WaitForSeconds(attackDuration);

        // เมื่อจบช่วงเวลาโจมตี ให้ทำลายล้าง Hitbox ทิ้ง (ยกแขนขึ้น)
        if (rightHitbox != null) Destroy(rightHitbox);

        // เวลารอคั่นกลางระหว่างแขนขวากับแขนซ้าย
        yield return new WaitForSeconds(delayBetweenSlams);


        // ===================================================
        // --- จังหวะที่ 2: แขนซ้ายทำงาน ---
        // ===================================================
        if (bossAnimator != null) bossAnimator.SetTrigger(leftSlamTrigger);

        // เสก Hitbox เข้าไปเป็นลูกของกระดูกมือซ้าย
        GameObject leftHitbox = null;
        if (leftHandBone != null && slamHitboxPrefab != null)
        {
            leftHitbox = Instantiate(slamHitboxPrefab, leftHandBone.position, leftHandBone.rotation, leftHandBone);
            leftHitbox.transform.localPosition = Vector3.zero;
        }

        // เปิด Hitbox ค้างไว้ปล่อยให้อนิเมชันลากกลับ
        yield return new WaitForSeconds(attackDuration);

        // จบการโจมตี ลบ Hitbox แขนซ้ายออก
        if (leftHitbox != null) Destroy(leftHitbox);

        Debug.Log("<color=orange>-> [Skill 1] ทำงานเสร็จสิ้นแบบล็อกติดโมเดล!</color>");
    }
}