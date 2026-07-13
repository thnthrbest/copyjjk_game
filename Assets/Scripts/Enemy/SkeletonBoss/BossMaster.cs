using System.Collections;
using UnityEngine;

public class BossMaster : MonoBehaviour
{
    public bool isDead = false;

    [Header("Skill Scripts Reference")]
    public BossSkillSlam skillSlam;
    public BossSkillSummon skillSummon;
    public BossSkillLaser skillLaser;

    private void Start()
    {
        // เริ่มต้นลูปการทำงานของบอสทันทีที่เกมเริ่ม
        StartCoroutine(BossActionLoop());
    }

    IEnumerator BossActionLoop()
    {
        while (!isDead)
        {
            // 1. สุ่มเวลารอก่อนใช้สกิลถัดไป (4 - 6 วินาที)
            float waitBeforeSkill = Random.Range(4f, 6f);
            yield return new WaitForSeconds(waitBeforeSkill);

            // เช็คเผื่อบอสตายระหว่างรอ
            if (isDead) break;

            // 2. สุ่มสกิลที่จะใช้ (1 ถึง 3)
            int randomSkill = Random.Range(1, 4); // Random.Range ของ int ตัวหลังสุดจะไม่นับรวม ดังนั้นใส่ 4 จะได้ 1, 2, 3

            // สั่งรันสกิลที่สุ่มได้ และใช้ yield return เพื่อรอให้สกิลนั้นๆ ทำงานจนเสร็จสิ้นก่อน
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
                yield return StartCoroutine(skillLaser.ExecuteSkill());
            }

            // 3. เมื่อใช้สกิลเสร็จ สุ่มเวลาฟื้นตัว (1 - 2 วินาที) ให้ผู้เล่นทำดาเมจ
            float recoveryTime = Random.Range(1f, 2f);
            yield return new WaitForSeconds(recoveryTime);
        }
    }

    // ฟังก์ชันสำหรับเรียกเมื่อบอสตาย (เอาไว้ให้ระบบเลือดของบอสมาเรียกใช้)
    public void Die()
    {
        isDead = true;
        StopAllCoroutines();
        Debug.Log("Boss is Dead!");
    }
}