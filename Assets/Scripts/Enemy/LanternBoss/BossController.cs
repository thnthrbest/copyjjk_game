using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Detection")]
    public float sightRange = 25f;
    private Transform player;
    private bool playerInRange = false;

    [Header("Basic Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 0.75f;

    [Header("Timing")]
    public float basicMinTime = 5f;
    public float basicMaxTime = 7f;

    public float recoveryMin = 1f;
    public float recoveryMax = 1.5f;

    [Header("Aim Settings")]
    public float aimHeightOffset = 1.5f;
    public bool useFixedHeight = false;

    [Header("Skills")]
    public FireLineSkill fireLineSkill;
    public MeteorSkill meteorSkill;
    public BarrageSkill barrageSkill;

    private List<System.Func<IEnumerator>> skillList;
    private int lastSkillIndex = -1;

    public Animator animator;

    // =========================
    void Start()
    {
        skillList = new List<System.Func<IEnumerator>>()
        {
            () => fireLineSkill.Execute(),
            () => meteorSkill.Execute(),
            () => barrageSkill.Execute()
        };

        StartCoroutine(MainLoop());
    }

    void Update()
    {
        DetectPlayer();
    }

    // =========================
    // 🔍 ตรวจจับ Player
    // =========================
    void DetectPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p == null)
        {
            player = null;
            playerInRange = false;
            return;
        }

        float dist = Vector3.Distance(transform.position, p.transform.position);

        if (dist <= sightRange)
        {
            player = p.transform;     // ✔️ cache ตอนเจอ
            playerInRange = true;
        }
        else
        {
            playerInRange = false;
        }
    }

    // =========================
    IEnumerator MainLoop()
    {
        while (true)
        {
            yield return new WaitUntil(() => playerInRange && player != null);

            float duration = Random.Range(basicMinTime, basicMaxTime);
            yield return StartCoroutine(BasicAttackPhase(duration));
            animator.SetBool("attack_nm", false);
            int skillIndex = GetRandomSkillIndex();
            yield return StartCoroutine(skillList[skillIndex]());
            //yield return StartCoroutine(skillList[0]());

            lastSkillIndex = skillIndex;

            float recoveryTime = Random.Range(recoveryMin, recoveryMax);
            yield return new WaitForSeconds(recoveryTime);
        }
    }

    // =========================
    IEnumerator BasicAttackPhase(float duration)
    {
        animator.SetBool("attack_nm", true);
        float timer = 0f;
        float shootTimer = 0f;

        while (timer < duration)
        {
            if (!playerInRange || player == null)
                yield break;

            timer += Time.deltaTime;
            shootTimer += Time.deltaTime;

            if (shootTimer >= fireRate)
            {
                Shoot();
                shootTimer = 0f;
            }

            yield return null;
        }
    }

    // =========================
    void Shoot()
    {
        if (player == null || firePoint == null || projectilePrefab == null)
            return;

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

        Debug.DrawLine(firePoint.position, targetPos, Color.red, 1f);
    }

    // =========================
    Vector3 GetTargetPosition()
    {
        Vector3 target = player.position + Vector3.up * aimHeightOffset;

        if (useFixedHeight && firePoint != null)
        {
            target.y = firePoint.position.y;
        }

        return target;
    }

    // =========================
    int GetRandomSkillIndex()
    {
        if (skillList.Count <= 1) return 0;

        int index;
        do
        {
            index = Random.Range(0, skillList.Count);
        }
        while (index == lastSkillIndex);

        return index;
    }

    // =========================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}