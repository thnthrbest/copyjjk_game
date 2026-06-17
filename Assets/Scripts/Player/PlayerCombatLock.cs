using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatLock : MonoBehaviour
{
    [Header("Enemy Detect Box")]
    public GameObject detectorBox;
    public LayerMask enemyLayer;

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

    Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        ScanEnemies();
        ValidateCurrentTarget();
        HandleInput();
    }

    void ScanEnemies()
    {
        if (detectorBox == null) return;

        BoxCollider box = detectorBox.GetComponent<BoxCollider>();
        if (box == null) return;

        Collider[] hits = Physics.OverlapBox(
            box.bounds.center,
            box.bounds.extents,
            detectorBox.transform.rotation,
            enemyLayer
        );

        enemiesInRange.Clear();

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                enemiesInRange.Add(hit.transform);
            }
        }

        //Debug.Log("Enemies in range: " + enemiesInRange.Count);

        // 🔥 Auto lock ถ้ายังไม่มี target
        if (currentTarget == null && enemiesInRange.Count > 0)
        {
            SetTarget(enemiesInRange[0]);
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

            // 🔥 ล็อกใหม่ทันทีถ้ายังมี enemy
            if (enemiesInRange.Count > 0)
            {
                SetTarget(enemiesInRange[0]);
            }
        }
    }

    void HandleInput()
    {
        string input = HandInputReceiver.RightHand;

        if (input == lastInput) return;

        lastInput = input;

        //if (input == "SWITCH_TARGET")
        if (input == "AIM")
        {
            ChangeTargetRight();
        }

        if (input == "SHOOT")
        {
            animator.SetTrigger("attack");
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
        if (index < 0) index = enemiesInRange.Count - 1;

        SetTarget(enemiesInRange[index]);
    }

    void ChangeTargetRight()
    {
        if (enemiesInRange.Count <= 1) return;

        enemiesInRange.Sort((a, b) =>
            a.position.x.CompareTo(b.position.x));

        int index = enemiesInRange.IndexOf(currentTarget);

        index++;
        if (index >= enemiesInRange.Count) index = 0;

        SetTarget(enemiesInRange[index]);
    }

    public void Shoot()
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

        BoxCollider box = detectorBox.GetComponent<BoxCollider>();
        if (box == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
    }
}