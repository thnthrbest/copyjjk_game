using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatLock : MonoBehaviour
{
    [Header("Enemy Detect Box")]
    public GameObject detectorBox;

    [Header("Lock Icon Prefab")]
    public GameObject lockPrefab;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public float shootCooldown = 0.4f;

    private float nextShootTime = 0f;

    private List<Transform> enemiesInRange = new List<Transform>();

    private Transform currentTarget;

    private GameObject currentLockIcon;

    private string lastInput = "";

    void Update()
    {
        ScanEnemies();
        ValidateCurrentTarget();
        HandleInput();
    }

    void ScanEnemies()
    {
        Collider[] hits = Physics.OverlapBox(
            detectorBox.transform.position,
            detectorBox.transform.localScale / 2,
            detectorBox.transform.rotation
        );

        enemiesInRange.Clear();

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                enemiesInRange.Add(hit.transform);
            }
        }

        if (currentTarget == null && enemiesInRange.Count > 0)
        {
            int randomIndex = Random.Range(0, enemiesInRange.Count);
            SetTarget(enemiesInRange[randomIndex]);
        }
    }

    void SetTarget(Transform enemy)
    {
        currentTarget = enemy;

        if (currentLockIcon != null)
        {
            Destroy(currentLockIcon);
        }

        currentLockIcon = Instantiate(lockPrefab);

        currentLockIcon.transform.SetParent(enemy);

        currentLockIcon.transform.localPosition = new Vector3(0, 2f, 0);
    }

    void ValidateCurrentTarget()
    {
        if (currentTarget == null) return;

        if (!enemiesInRange.Contains(currentTarget))
        {
            if (currentLockIcon != null)
            {
                Destroy(currentLockIcon);
            }

            currentTarget = null;
        }
    }

    void HandleInput()
    {
        string input = HandInputReceiver.RightHand;

        if (input == lastInput) return;

        lastInput = input;

        if (input == "AIM_LEFT")
        {
            ChangeTargetLeft();
        }

        if (input == "AIM_RIGHT")
        {
            ChangeTargetRight();
        }

        if (input == "AIM_UP" || input == "AIM_DOWN")
        {
            Shoot();
        }
    }

    void ChangeTargetLeft()
    {
        if (enemiesInRange.Count <= 1) return;

        enemiesInRange.Sort((a, b) =>
            a.position.x.CompareTo(b.position.x));

        int index = enemiesInRange.IndexOf(currentTarget);

        index--;

        if (index < 0)
            index = enemiesInRange.Count - 1;

        SetTarget(enemiesInRange[index]);
    }

    void ChangeTargetRight()
    {
        if (enemiesInRange.Count <= 1) return;

        enemiesInRange.Sort((a, b) =>
            a.position.x.CompareTo(b.position.x));

        int index = enemiesInRange.IndexOf(currentTarget);

        index++;

        if (index >= enemiesInRange.Count)
            index = 0;

        SetTarget(enemiesInRange[index]);
    }

    void Shoot()
    {
        if (Time.time < nextShootTime) return;

        if (currentTarget == null) return;

        nextShootTime = Time.time + shootCooldown;

        GameObject bullet = Instantiate(
            bulletPrefab,
            shootPoint.position,
            Quaternion.identity
        );

        HomingBullet bulletScript = bullet.GetComponent<HomingBullet>();

        if (bulletScript != null)
        {
            bulletScript.SetTarget(currentTarget);
        }
    }

    void OnDrawGizmos()
    {
        if (detectorBox == null) return;

        Gizmos.color = Color.red;

        Gizmos.matrix = detectorBox.transform.localToWorldMatrix;

        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}