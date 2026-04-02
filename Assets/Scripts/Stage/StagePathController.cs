using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StagePoint
{
    public Transform stopPoint;
    public GameObject enemyZone;
}

public class StagePathController : MonoBehaviour
{
    public List<StagePoint> stagePoints = new List<StagePoint>();

    public float moveSpeed = 6f;
    public float stopDistance = 0.5f;

    int currentIndex = 0;
    bool moving = true;

    void Update()
    {
        if (stagePoints.Count == 0) return;

        StagePoint point = stagePoints[currentIndex];

        if (moving)
        {
            MoveForward(point.stopPoint);
        }
        else
        {
            CheckEnemies(point.enemyZone);
        }
    }

    void MoveForward(Transform target)
    {
        float distance = target.position.z - transform.position.z;

        if (distance <= stopDistance)
        {
            moving = false;
            Debug.Log("Reached Point " + currentIndex);
            return;
        }

        transform.position += Vector3.forward * moveSpeed * Time.deltaTime;
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

        //Debug.Log("Enemies Remaining: " + enemyCount);

        if (enemyCount == 0)
            GoNextPoint();
    }

    void GoNextPoint()
    {
        currentIndex++;

        if (currentIndex >= stagePoints.Count)
        {
            Debug.Log("Stage Complete");
            return;
        }

        moving = true;

        Debug.Log("Moving to Point " + currentIndex);
    }
}