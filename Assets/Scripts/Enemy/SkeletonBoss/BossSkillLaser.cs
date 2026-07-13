using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSkillLaser : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator bossAnimator;
    [SerializeField] private string startTrigger = "EyeLaserStart"; // ท่าเริ่มชาร์จพลัง
    [SerializeField] private string endTrigger = "EyeLaserEnd";     // ท่าจบสกิล

    [Header("Prefab Settings")]
    [SerializeField] private GameObject laserGridPrefab;    // พรีแฟบแผงเลเซอร์ (Laser_Grid)

    [Header("Dual Eye References (3D)")]
    [SerializeField] private Transform leftEyeSpawnPoint;   // จุดตาซ้าย (แกน Z สีน้ำเงินต้องชี้ไปข้างหน้าบอส)
    [SerializeField] private Transform rightEyeSpawnPoint;  // จุดตาขวา (แกน Z สีน้ำเงินต้องชี้ไปข้างหน้าบอส)
    
    private Transform playerTransform;                      
    private GameObject leftLaserInstance;
    private GameObject rightLaserInstance;

    [Header("Timing Settings")]
    [SerializeField] private float lockOnDuration = 2.0f;   // เวลาชาร์จพลัง + หมุนตามผู้เล่น (หน่วยเป็นวินาที)
    [SerializeField] private float laserDuration = 3.0f;    // เวลายิงเลเซอร์แช่ค้างไว้กับที่ (หน่วยเป็นวินาที)
    [SerializeField] private float recoveryDuration = 1.5f; // เวลาบอสพักเหนื่อยหลังยิงเสร็จ

    void Start()
    {
        // ค้นหาผู้เล่นอัตโนมัติในฉากด้วย Tag
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    public IEnumerator ExecuteSkill()
    {
        if (laserGridPrefab == null || leftEyeSpawnPoint == null || rightEyeSpawnPoint == null || playerTransform == null) yield break;

        // ===================================================
        // 🎬 เฟสที่ 1: เริ่มชาร์จพลัง + ตาหมุนล็อกเป้าตามผู้เล่น
        // ===================================================
        if (bossAnimator != null) bossAnimator.SetTrigger(startTrigger);

        // เสกแผงเลเซอร์มารอไว้ที่ตาซ้ายและขวา (ตัวลำแสงข้างในยังปิดซ่อนอยู่)
        leftLaserInstance = Instantiate(laserGridPrefab, leftEyeSpawnPoint.position, leftEyeSpawnPoint.rotation, leftEyeSpawnPoint);
        rightLaserInstance = Instantiate(laserGridPrefab, rightEyeSpawnPoint.position, rightEyeSpawnPoint.rotation, rightEyeSpawnPoint);

        LaserGridController leftController = leftLaserInstance.GetComponent<LaserGridController>();
        LaserGridController rightController = rightLaserInstance.GetComponent<LaserGridController>();

        // ลูปทำงานตลอดช่วงเวลาชาร์จพลัง (ตามค่า lockOnDuration)
        float timer = 0f;
        while (timer < lockOnDuration)
        {
            if (playerTransform != null)
            {
                Vector3 targetPos = playerTransform.position;

                // อัปเดตมุมหันของตาทั้งสองข้างให้จ้องตามผู้เล่นแบบ 3D ทุกเฟรม
                RotateEyeToTarget3D(leftEyeSpawnPoint, targetPos);
                RotateEyeToTarget3D(rightEyeSpawnPoint, targetPos);
            }

            timer += Time.deltaTime;
            yield return null; // รอเฟรมถัดไป
        }

        // ===================================================
        // 🛑 เฟสที่ 2: ชาร์จเสร็จแล้ว! หยุดหมุนตาม + เปิดฉากยิงจุดสุดท้าย
        // ===================================================
        // เมื่อหลุดลูปด้านบน ตาบอสจะ "หยุดล็อกตาม" และค้างอยู่ที่ตำแหน่งผู้เล่น ณ วินาทีนั้นทันที
        Debug.Log("🔒 ล็อกพิกัดสุดท้ายสำเร็จ! ระเบิดลำแสงคู่แช่กับที่!");
        
        // สั่งระเบิดลำแสงเลเซอร์ทรงกระบอกคู่พุ่งตรงออกจากเบ้าตาที่ค้างอยู่
        if (leftController != null) leftController.ActivateFullLaser(true);
        if (rightController != null) rightController.ActivateFullLaser(true);

        // ยิงแช่ค้างไว้ตรงจุดนั้น (ผู้เล่นสามารถแดช/วิ่งหลบออกจากวิถีเลเซอร์ได้แล้ว)
        yield return new WaitForSeconds(laserDuration);

        // ===================================================
        // 🧹 เฟสที่ 3: ยิงเสร็จเรียบร้อย ปิดเลเซอร์และเคลียร์ของ
        // ===================================================
        if (leftController != null) leftController.ActivateFullLaser(false);
        if (rightController != null) rightController.ActivateFullLaser(false);
        
        if (leftLaserInstance != null) Destroy(leftLaserInstance);
        if (rightLaserInstance != null) Destroy(rightLaserInstance);
        
        // เล่นอนิเมชันจบ/พักเหนื่อย
        if (bossAnimator != null) bossAnimator.SetTrigger(endTrigger);
        yield return new WaitForSeconds(recoveryDuration);
    }

    // ฟังก์ชันคำนวณการหันวัตถุในโลก 3 มิติ (หันแกน Z ชี้หาเป้าหมาย)
    private void RotateEyeToTarget3D(Transform eyeTransform, Vector3 targetPosition)
    {
        Vector3 targetDirection = targetPosition - eyeTransform.position;
        
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            eyeTransform.rotation = targetRotation;
        }
    }
}