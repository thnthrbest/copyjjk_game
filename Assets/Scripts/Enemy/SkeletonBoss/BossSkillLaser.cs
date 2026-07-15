using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSkillLaser : MonoBehaviour
{
    [Header("Cooldown Settings")]
    [SerializeField] private float skillCooldown = 5f;

    [Header("Animation Settings")]
    [SerializeField] private Animator bossAnimator;
    [SerializeField] private string startTrigger = "EyeLaserStart"; 
    [SerializeField] private string endTrigger = "EyeLaserEnd";
    [SerializeField] private float vfxSpawnDelay = 0.5f; // << ระยะเวลารอให้อนิเมชันเล่นถึงจุดที่เริ่มชาร์จ

    [Header("Prefab Settings")]
    [SerializeField] private GameObject laserGridPrefab;    
    
    [Header("VFX Stages (ชาร์จพลัง 2 ขั้น)")]
    [SerializeField] private GameObject chargeStage1Prefab; // ขั้นที่ 1: ออร่ารวบรวมพลังงาน
    [SerializeField] private GameObject chargeStage2Prefab; // ขั้นที่ 2: บอลพลังงานเข้มข้น (Power Ball)

    [Header("Dual Eye References (3D)")]
    [SerializeField] private Transform leftEyeSpawnPoint;   
    [SerializeField] private Transform rightEyeSpawnPoint;  
    
    private Transform playerTransform;                      
    private GameObject leftLaserInstance;
    private GameObject rightLaserInstance;

    [Header("Timing Settings")]
    [SerializeField] private float lockOnDuration = 3.0f;   // เวลาชาร์จทั้งหมด
    [Range(0.1f, 0.9f)]
    [SerializeField] private float stage2Threshold = 0.5f;  // จุดเปลี่ยนขั้นที่ 2 (เช่น 0.5 คือผ่านไปครึ่งทางของเวลาชาร์จ)
    [SerializeField] private float laserDuration = 3.0f;    
    [SerializeField] private float recoveryDuration = 1.5f; 

    private bool isSkillRunning = false; 
    private float cooldownTimer = 0f;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
    }

    public void TryActivateSkill()
    {
        if (!isSkillRunning && cooldownTimer <= 0) StartCoroutine(ExecuteSkill());
    }

    public IEnumerator ExecuteSkill()
    {
        if (laserGridPrefab == null || leftEyeSpawnPoint == null || rightEyeSpawnPoint == null || playerTransform == null) yield break;

        isSkillRunning = true; 

        // ----------------------------------------------------
        // 🎬 เฟสเริ่ม: เล่นแอนิเมชัน และ รอจังหวะ (Delay)
        // ----------------------------------------------------
        if (bossAnimator != null) bossAnimator.SetTrigger(startTrigger);
        yield return new WaitForSeconds(vfxSpawnDelay);

        // ----------------------------------------------------
        // 🎯 เฟสที่ 1: เริ่มชาร์จพลังขั้นที่ 1 (ออร่ารวบรวม) + หมุนล็อกตาม
        // ----------------------------------------------------
        GameObject leftVFX = Instantiate(chargeStage1Prefab, leftEyeSpawnPoint.position, Quaternion.identity);
        GameObject rightVFX = Instantiate(chargeStage1Prefab, rightEyeSpawnPoint.position, Quaternion.identity);

        leftLaserInstance = Instantiate(laserGridPrefab, leftEyeSpawnPoint.position, leftEyeSpawnPoint.rotation, leftEyeSpawnPoint);
        rightLaserInstance = Instantiate(laserGridPrefab, rightEyeSpawnPoint.position, rightEyeSpawnPoint.rotation, rightEyeSpawnPoint);
        LaserGridController leftL = leftLaserInstance.GetComponent<LaserGridController>();
        LaserGridController rightL = rightLaserInstance.GetComponent<LaserGridController>();

        bool isStage2Active = false;
        float timer = 0f;
        while (timer < lockOnDuration)
        {
            if (playerTransform != null)
            {
                Vector3 targetPos = playerTransform.position;
                RotateEyeToTarget3D(leftEyeSpawnPoint, targetPos);
                RotateEyeToTarget3D(rightEyeSpawnPoint, targetPos);
                
                // ให้ VFX วิ่งตามตำแหน่งตา (ใช้ World Space เพื่อกันปัญหา Scale)
                if (leftVFX != null) leftVFX.transform.position = leftEyeSpawnPoint.position;
                if (rightVFX != null) rightVFX.transform.position = rightEyeSpawnPoint.position;
            }

            // ----------------------------------------------------
            // 🔥 ตรวจสอบจุดเปลี่ยนสู่ขั้นที่ 2 (บอลพลังงาน)
            // ----------------------------------------------------
            if (!isStage2Active && timer >= lockOnDuration * stage2Threshold)
            {
                isStage2Active = true;
                // ลบออร่าอันเดิมออก
                if (leftVFX != null) Destroy(leftVFX);
                if (rightVFX != null) Destroy(rightVFX);
                // เสกบอลพลังงานอันใหม่มาแทน
                leftVFX = Instantiate(chargeStage2Prefab, leftEyeSpawnPoint.position, Quaternion.identity);
                rightVFX = Instantiate(chargeStage2Prefab, rightEyeSpawnPoint.position, Quaternion.identity);
            }

            timer += Time.deltaTime;
            yield return null; 
        }

        // ----------------------------------------------------
        // 🛑 เฟสที่ 2: หยุดหมุน + ลบเอฟเฟกต์ชาร์จ -> ยิงเลเซอร์
        // ----------------------------------------------------
        if (leftVFX != null) Destroy(leftVFX);
        if (rightVFX != null) Destroy(rightVFX);

        if (leftL != null) leftL.ActivateFullLaser(true);
        if (rightL != null) rightL.ActivateFullLaser(true);

        yield return new WaitForSeconds(laserDuration);

        // ----------------------------------------------------
        // 🧹 เฟสจบ: ปิดเลเซอร์ เคลียร์สนาม
        // ----------------------------------------------------
        if (leftL != null) leftL.ActivateFullLaser(false);
        if (rightL != null) rightL.ActivateFullLaser(false);
        if (leftLaserInstance != null) Destroy(leftLaserInstance);
        if (rightLaserInstance != null) Destroy(rightLaserInstance);
        
        if (bossAnimator != null) bossAnimator.SetTrigger(endTrigger);
        yield return new WaitForSeconds(recoveryDuration);

        cooldownTimer = skillCooldown;
        isSkillRunning = false; 
    }

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