using System.Collections;
using UnityEngine;

public class MeteorSkill : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject meteorPrefab;
    public GameObject indicatorPrefab;

    [Header("ยิงขึ้นฟ้า (2 จุดพร้อมกัน)")]
    public Transform firePoint;
    public Transform skyPointA;
    public Transform skyPointB;

    [Header("ตำแหน่งเสก Meteor")]
    public Transform leftSpawn;
    public Transform midSpawn;
    public Transform rightSpawn;

    [Header("ตำแหน่ง Indicator")]
    public Transform leftIndicator;
    public Transform midIndicator;
    public Transform rightIndicator;

    [Header("🎯 Target จริง")]
    public Transform leftTarget;
    public Transform midTarget;
    public Transform rightTarget;

    [Header("Timing")]
    public float delayBeforeIndicator = 1f;
    public float delayBeforeHit = 1f;
    public float delayBetweenRounds = 0.5f; // 🔥 เว้นระหว่างรอบ


    public Animator animator; // 🔥 เพิ่ม Animator

    // =========================
    public IEnumerator Execute()
    {
        
        animator.SetBool("skill1", true); // 🔥 เริ่ม Animatior
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(XXX());
    }
    public IEnumerator XXX()
    {
        int loopCount = 3; // 🔥 จำนวนรอบ
        for (int round = 0; round < loopCount; round++)
        {
            // 🔥 ยิงขึ้นฟ้า  ใช้ animetion เรียก
            ShootToSkyDual();

            yield return new WaitForSeconds(delayBeforeIndicator);

            // 🎲 เลือก 2 จาก 3
            int[] chosen = GetTwoRandomIndices();

            // 🔔 indicator
            GameObject[] indicators = new GameObject[2];

            for (int i = 0; i < 2; i++)
            {
                Transform indPos = GetIndicator(chosen[i]);

                indicators[i] = Instantiate(
                    indicatorPrefab,
                    indPos.position,
                    Quaternion.identity
                );
            }

            yield return new WaitForSeconds(delayBeforeHit);

            // ☄️ ยิง meteor จริง
            for (int i = 0; i < 2; i++)
            {
                Transform spawnPos = GetSpawn(chosen[i]);
                Transform targetPos = GetTarget(chosen[i]);

                GameObject meteor = Instantiate(
                    meteorPrefab,
                    spawnPos.position,
                    Quaternion.identity
                );

                Projectile proj = meteor.GetComponent<Projectile>();
                if (proj != null && targetPos != null)
                {
                    proj.SetTarget(targetPos.position);
                }
            }

            // 🧹 ลบ indicator
            foreach (var ind in indicators)
            {
                if (ind != null)
                    Destroy(ind);
            }

            // ⏱ เว้นก่อนรอบถัดไป (ยกเว้นรอบสุดท้าย)
            if (round < loopCount - 1)
                yield return new WaitForSeconds(delayBetweenRounds);
        }
        animator.SetBool("skill1", false); // 🔥 จบ Animation
    }

    // =========================
    void ShootToSkyDual()
    {
        SpawnMeteorToSky(skyPointA);
        SpawnMeteorToSky(skyPointB);
    }

    void SpawnMeteorToSky(Transform skyTarget)
    {
        if (skyTarget == null || firePoint == null) return;

        GameObject meteor = Instantiate(
            meteorPrefab,
            firePoint.position,
            Quaternion.identity
        );

        Projectile proj = meteor.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetTarget(skyTarget.position);
        }
    }

    // =========================
    Transform GetSpawn(int index)
    {
        switch (index)
        {
            case 0: return leftSpawn;
            case 1: return midSpawn;
            case 2: return rightSpawn;
        }
        return midSpawn;
    }

    Transform GetIndicator(int index)
    {
        switch (index)
        {
            case 0: return leftIndicator;
            case 1: return midIndicator;
            case 2: return rightIndicator;
        }
        return midIndicator;
    }

    Transform GetTarget(int index)
    {
        switch (index)
        {
            case 0: return leftTarget;
            case 1: return midTarget;
            case 2: return rightTarget;
        }
        return midTarget;
    }

    // =========================
    int[] GetTwoRandomIndices()
    {
        int first = Random.Range(0, 3);
        int second;

        do
        {
            second = Random.Range(0, 3);
        } while (second == first);

        return new int[] { first, second };
    }
}