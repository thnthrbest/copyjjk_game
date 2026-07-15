using System.Collections;
using UnityEngine;

public class BossSkillSummon : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator bossAnimator;
    [SerializeField] private string startTrigger = "SummonStart"; // ท่าเริ่มร่าย
    [SerializeField] private string endTrigger = "SummonEnd";     // ท่าจบการร่าย

    [Header("Prefab Settings")]
    [SerializeField] private GameObject minionPrefab;       // ตัวมอนสเตอร์
    [SerializeField] private GameObject spawnVFX;           // (Option) เอฟเฟกต์วงเวทตอนเสก

    [Header("Spawn Settings")]
    [SerializeField] private Collider spawnArea;            // พื้นที่สุ่มเกิด
    [SerializeField] private float delayBeforeFirstMinion = 1.0f; // เวลาก่อนตัวแรกจะออก (รอท่าเริ่ม)
    [SerializeField] private float delayBetweenMinions = 0.8f;    // เวลาระหว่างการเสกแต่ละตัว
    [SerializeField] private float delayAfterLastMinion = 0.5f;   // เวลาก่อนจะปิดท่า (รอตัวสุดท้ายออก)

    public IEnumerator ExecuteSkill()
    {
        Debug.Log("<color=magenta>-> [Skill 2] เริ่มท่าร่ายเวท (Phase 1: Start)</color>");

        // 1. สั่งเล่นแอนิเมชันท่าเริ่ม (เช่น บอสชูมือขึ้น)
        if (bossAnimator != null) bossAnimator.SetTrigger(startTrigger);

        // 2. รอให้แอนิเมชันเข้าสู่ท่าค้าง (Loop/Hold)
        yield return new WaitForSeconds(delayBeforeFirstMinion);

        // 3. เริ่มการทยอยเสกลูกสมุน (Phase 2: Spawning Loop)
        int minionCount = Random.Range(3, 6); // สุ่ม 3-5 ตัว
        for (int i = 0; i < minionCount; i++)
        {
            SpawnOneMinion();
            Debug.Log($"[Skill 2] เสกตัวที่ {i + 1} ออกมาแล้ว");
            
            // เวลาระหว่างการเสกแต่ละตัว (บอสจะค้างท่าร่ายไว้ช่วงนี้)
            yield return new WaitForSeconds(delayBetweenMinions);
        }

        // 4. รอสักพักหลังตัวสุดท้ายออก เพื่อความสวยงาม
        yield return new WaitForSeconds(delayAfterLastMinion);

        // 5. สั่งเล่นแอนิเมชันท่าจบ (Phase 3: End - เช่น บอสเอามือลง)
        if (bossAnimator != null) bossAnimator.SetTrigger(endTrigger);
        Debug.Log("<color=magenta>-> [Skill 2] จบด่านการร่ายเวท</color>");

        // 6. ช่วงเวลาฟื้นตัว (Recovery) - บอสจะยืนนิ่งเฉยๆ ให้ผู้เล่นเคลียร์มอน
        float recoveryTime = Random.Range(2f, 3f);
        Debug.Log($"[Skill 2] บอสพักเหนื่อยเป็นเวลา: {recoveryTime:F2} วินาที");
        yield return new WaitForSeconds(recoveryTime);
    }

    private void SpawnOneMinion()
    {
        if (spawnArea == null || minionPrefab == null) return;

        Vector3 spawnPos = GetRandomPointInBounds(spawnArea.bounds);
        
        // ถ้ามีเอฟเฟกต์วงเวท ให้เสกเอฟเฟกต์ออกมาก่อน
        if (spawnVFX != null)
        {
            Instantiate(spawnVFX, spawnPos, Quaternion.identity);
        }

        // เสกตัวมอนสเตอร์
        Instantiate(minionPrefab, spawnPos, Quaternion.identity);
    }

    private Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}