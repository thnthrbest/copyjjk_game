using System.Collections;
using UnityEngine;

public class BarrageSkill : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject projectilePrefab;

    [Header("References")]
    public Transform firePoint;
    public Transform player;
    public BoxCollider attackZone; // ใช้ BoxCollider

    [Header("Timing")]
    public float durationMin = 4f;
    public float durationMax = 6f;
    public float fireInterval = 0.25f;

    [Header("Chance (%)")]
    [Range(0, 100)] public int randomShotChance = 70;
    [Range(0, 100)] public int directPlayerChance = 30;

    private Vector3 lastRandomPoint;

    public Animator animator;
    // =========================
    public IEnumerator Execute()
    {
        float duration = Random.Range(durationMin, durationMax);
        float timer = 0f;

        while (timer < duration)
        {
            animator.SetBool("attack_nm", true);
            Shoot();
            yield return new WaitForSeconds(fireInterval);
            timer += fireInterval;
        }
        animator.SetBool("attack_nm", false);
    }

    // =========================
    void Shoot()
    {
        if (firePoint == null || projectilePrefab == null) return;

        Vector3 targetPos = GetTargetPosition();

        GameObject bullet = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        Projectile proj = bullet.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetTarget(targetPos);
        }

        Debug.DrawLine(firePoint.position, targetPos, Color.blue, 1f);
    }

    // =========================
    Vector3 GetTargetPosition()
    {
        
        int roll = Random.Range(0, 100);

        int randomMax = randomShotChance;
        int directMax = randomShotChance + directPlayerChance;

        // 🔴 ยิงมั่วในโซน
        if (roll < randomMax)
        {

            return GetRandomPointInBox();
        }
        // 🔵 ยิงตรงผู้เล่น
        else if (roll < directMax && player != null)
        {
            return player.position;
        }

        // fallback กันค่าพัง
        return GetRandomPointInBox();
    }

    // =========================
    Vector3 GetRandomPointInBox()
    {
        if (attackZone == null) return transform.position;

        Bounds b = attackZone.bounds;

        Vector3 point;
        int attempts = 0;

        do
        {
            point = new Vector3(
                Random.Range(b.min.x, b.max.x),
                Random.Range(b.min.y, b.max.y), // 🔥 แก้ตรงนี้
                Random.Range(b.min.z, b.max.z)
            );

            attempts++;

        } while (Vector3.Distance(point, lastRandomPoint) < 2f && attempts < 10);

        lastRandomPoint = point;
        return point;
    }
}