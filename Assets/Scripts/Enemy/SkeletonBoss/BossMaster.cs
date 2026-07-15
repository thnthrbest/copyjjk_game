using System.Collections;
using UnityEngine;

public class BossMaster : MonoBehaviour
{
    public bool isDead = false;

    [Header("Detection Settings (ระบบตรวจจับด้วย Tag ส่วนกลาง)")]
    [SerializeField] private float detectionRadius = 15f;    // รัศมีวงกลมตรวจจับผู้เล่น

    [Header("Skill Scripts Reference")]
    public BossSkillSlam skillSlam;
    public BossSkillSummon skillSummon;
    public BossSkillLaser skillLaser;

    private bool isPlayerInRange = false;

    private void Start()
    {
        // เริ่มต้นลูปการทำงานของบอสทันทีที่เกมเริ่ม
        StartCoroutine(BossActionLoop());
    }

    private void Update()
    {
        if (isDead) return;

        // สแกนตรวจสอบผู้เล่นในระยะทุกเฟรม
        isPlayerInRange = CheckPlayerTagInRange();
    }

    private bool CheckPlayerTagInRange()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator BossActionLoop()
    {
        while (!isDead)
        {
            // 🛑 ถ้าผู้เล่นยังไม่อยู่ในระยะ บอสจะยืนรอเฉยๆ ไม่นับคูลดาวน์สกิล
            while (!isPlayerInRange)
            {
                if (isDead) break;
                yield return null; 
            }

            if (isDead) break;

            // 1. สุ่มเวลารอก่อนใช้สกิลถัดไป (4 - 6 วินาที)
            float waitBeforeSkill = Random.Range(4f, 6f);
            
            float timer = 0f;
            while (timer < waitBeforeSkill)
            {
                if (isDead) break;
                
                // ถ้าระหว่างชาร์จแล้วผู้เล่นวิ่งหนีออกจากระยะไป บอสจะหยุดเวลารอไว้ก่อน
                if (isPlayerInRange)
                {
                    timer += Time.deltaTime;
                }
                yield return null;
            }

            if (isDead) break;

            // 2. สุ่มสกิลที่จะใช้ (1 ถึง 3)
            int randomSkill = Random.Range(1, 4); // 1, 2, 3

            if (randomSkill == 1 && skillSlam != null)
            {
                yield return StartCoroutine(skillSlam.ExecuteSkill());
            }
            else if (randomSkill == 2 && skillSummon != null)
            {
                yield return StartCoroutine(skillSummon.ExecuteSkill());
            }
            else if (randomSkill == 3 && skillLaser != null)
            {
                // สามารถใช้ yield return StartCoroutine เรียกตรงนี้ได้แล้วเพราะฟังก์ชันเป็น public แล้วครับ
                yield return StartCoroutine(skillLaser.ExecuteSkill());
            }

            // 3. เมื่อใช้สกิลเสร็จ สุ่มเวลาฟื้นตัว (1 - 2 วินาที) ให้ผู้เล่นมีช่องว่างทำดาเมจ
            float recoveryTime = Random.Range(1f, 2f);
            yield return new WaitForSeconds(recoveryTime);
        }
    }

    public void Die()
    {
        isDead = true;
        StopAllCoroutines();
        Debug.Log("Boss is Dead!");
    }

    private void OnDrawGizmosSelected()
    {
        if (isPlayerInRange)
        {
            Gizmos.color = Color.red; 
        }
        else
        {
            Gizmos.color = Color.green; 
        }

        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}