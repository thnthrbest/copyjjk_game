using System.Collections;
using UnityEngine;

public class FireLineSkill : MonoBehaviour
{
    [Header("Projectile Prefab (ก้อนใหญ่)")]
    public GameObject fireLinePrefab;

    [Header("Spawn Point")]
    public Transform spawnPoint;

    [Header("Target (ผู้เล่น)")]
    public Transform player;

    [Header("Wave Settings")]
    public int waveCount = 5;
    public float delayBetweenWaves = 1.5f;

    public Animator animator;

    public IEnumerator Execute()
    {
        animator.SetBool("skill_line", true);
        yield return new WaitForSeconds(3f);
        for (int i = 0; i < waveCount; i++)
        {
            SpawnFireLine();
            yield return new WaitForSeconds(delayBetweenWaves);
        }
        animator.SetBool("skill_line", false);
    }
    

    void SpawnFireLine()
    {
        if (fireLinePrefab == null || spawnPoint == null || player == null)
            return;

        // 📍 เอาตำแหน่งเป้า ณ ตอนยิง (snapshot)
        Vector3 targetPos = GetTargetPosition();

        GameObject obj = Instantiate(
            fireLinePrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        // 🔥 ส่ง target ให้ Projectile กลาง
        Projectile proj = obj.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetTarget(targetPos);
        }
        else
        {
            Debug.LogWarning("FireLinePrefab ไม่มี Projectile script!");
        }
    }

    Vector3 GetTargetPosition()
    {
        Collider col = player.GetComponent<Collider>();

        if (col != null)
            return col.bounds.center;

        return player.position + Vector3.up * 1.5f;
    }
}