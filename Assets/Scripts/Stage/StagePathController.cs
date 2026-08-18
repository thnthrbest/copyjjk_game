using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

[System.Serializable]
public class StagePoint
{
    public Transform stopPoint;
    public GameObject enemyZone;
    [Tooltip("กลุ่มวัตถุ/ฉากของโซนนี้ที่จะใช้ซ่อน/แสดงตามตำแหน่งการเดินทาง")]
    public GameObject stageContent;
}

public class StagePathController : MonoBehaviour
{
    public List<StagePoint> stagePoints = new List<StagePoint>();

    public float moveSpeed = 6f;
    public float stopDistance = 0.5f;

    [Tooltip("ชื่อ Scene ที่จะเปลี่ยนไปเมื่อจบ Stage (เช่น EndGame)")]
    public string endSceneName = "EndGame";
    [Tooltip("เวลารอก่อนเปลี่ยน Scene (วินาที)")]
    public float endSceneDelay = 2.0f;

    public GameObject ps,ps2;
    ParticleSystem particle,particle2;
    ParticleSystem.EmissionModule emission,emission2;

    int currentIndex = 0;
    public bool moving = true;
    private bool stageCompleted = false;

    void Start()
    {
        particle = ps.GetComponent<ParticleSystem>();
        particle2 = ps2.GetComponent<ParticleSystem>();
        emission = particle.emission;
        emission2 = particle2.emission;
    }

    void Update()
    {
        if (stagePoints.Count == 0 || stageCompleted) return;

        StagePoint point = stagePoints[currentIndex];

        if (moving)
        {
            MoveForward(point.stopPoint);
            emission.enabled = true;
            emission2.enabled = true;
        }
        else
        {
            CheckEnemies(point.enemyZone);
            emission.enabled = false;
            emission2.enabled = false;
        }
    }

    void MoveForward(Transform target)
    {
        Vector3 direction = (target.position - transform.position);
        float distance = direction.magnitude;

        // เช็คถึงจุด
        if (distance <= stopDistance)
        {
            moving = false;
            Debug.Log("Reached Point " + currentIndex);
            return;
        }

        // Normalize direction
        direction = direction.normalized;

        // Move ไปตามทิศจริง (ไม่ใช่แค่ Z)
        transform.position += direction * moveSpeed * Time.deltaTime;

        // 🔥 เพิ่ม: หมุนให้หันไปทางที่กำลังเดิน
        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                8f * Time.deltaTime
            );
        }
    }

    void CheckEnemies(GameObject zone)
    {
        if (zone == null)
        {
            GoNextPoint();
            return;
        }

        Collider zoneCollider = zone.GetComponent<Collider>();

        if (zoneCollider == null)
        {
            GoNextPoint();
            return;
        }

        Collider[] hits = Physics.OverlapBox(
            zoneCollider.bounds.center,
            zoneCollider.bounds.extents
        );

        int enemyCount = 0;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
                enemyCount++;
        }

        if (enemyCount == 0)
            GoNextPoint();
    }

    void GoNextPoint()
    {
        // ปิดห้องที่ผ่านมา
        if (currentIndex > 0 && currentIndex < stagePoints.Count)
        {
            var prevPoint = stagePoints[currentIndex-1];
            if (prevPoint != null && prevPoint.stageContent != null)
            {
                prevPoint.stageContent.SetActive(false);
            }
        }

        currentIndex++;

        if (currentIndex >= stagePoints.Count)
        {
            stageCompleted = true;
            Debug.Log($"Stage Complete! Loading end scene '{endSceneName}' in {endSceneDelay} seconds...");
            if (!string.IsNullOrEmpty(endSceneName))
            {
                Invoke(nameof(LoadEndScene), endSceneDelay);
            }
            return;
        }

        moving = true;
    }

    private void LoadEndScene()
    {
        SceneManager.LoadScene(endSceneName);
    }
}