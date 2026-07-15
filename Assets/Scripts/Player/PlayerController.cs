using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 12f;

    public float minX = -20f;
    public float maxX = 20f;

    public float jumpForce = 8f;
    public float gravity = -20f;

    public float crouchHeight = 1f;

    public float jumpCooldown = 0.3f;
    public StagePathController stagePathController;

    [Header("Obstacle Collision Settings")]
    public float obstacleDamage = 15f;
    public float slowDuration = 2.0f;
    [Range(0f, 1f)]
    public float slowSpeedMultiplier = 0.4f;
    public float blinkInterval = 0.1f;
    [Tooltip("ใส่ Collider ของผู้เล่นที่ต้องการให้สไลด์เดินทะลุผ่านสิ่งกีดขวาง (ถ้าปล่อยว่างไว้ ระบบจะใช้ CharacterController อัตโนมัติ)")]
    public List<Collider> playerColliders = new List<Collider>();

    private bool isSlowed = false;
    private bool isBlinking = false;
    private float originalMoveSpeed;
    private float originalPathSpeed;

    [Header("Lighting Settings")]
    [Tooltip("ให้ Directional Light หมุนตามทิศทางของผู้เล่นโดยอัตโนมัติ")]
    public bool autoRotateDirectionalLight = true;
    private Light directionalLight;
    private Quaternion initialLightRotationOffset;
    private bool lightOffsetInitialized = false;

    CharacterController controller;

    float verticalVelocity;
    float originalHeight;

    float lastJumpTime = -10f;
    string lastCommand = "";

    Animator animator;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        originalHeight = controller.height;
        animator = GetComponent<Animator>();

        if (autoRotateDirectionalLight)
        {
            InitializeDirectionalLight();
        }
    }

    void InitializeDirectionalLight()
    {
        if (directionalLight == null)
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light l in lights)
            {
                if (l.type == LightType.Directional && l.gameObject.activeInHierarchy)
                {
                    directionalLight = l;
                    break;
                }
            }
        }

        if (directionalLight != null)
        {
            initialLightRotationOffset = Quaternion.Inverse(transform.rotation) * directionalLight.transform.rotation;
            lightOffsetInitialized = true;
        }
        else
        {
            Debug.LogWarning("PlayerController: No active Directional Light found in the scene to rotate.");
        }
    }

    void LateUpdate()
    {
        if (autoRotateDirectionalLight && directionalLight != null && lightOffsetInitialized)
        {
            directionalLight.transform.rotation = transform.rotation * initialLightRotationOffset;
        }
    }

    void Update()
    {
        if (stagePathController != null)
        {
            animator.SetBool("move", stagePathController.moving);
        }
        else
        {
            animator.SetBool("move", true);
        }

        string cmd = HandInputReceiver.LeftHand;

        float moveX = 0f;

        if (string.Equals(cmd, "Idle", System.StringComparison.OrdinalIgnoreCase) || 
            string.Equals(cmd, "Jump", System.StringComparison.OrdinalIgnoreCase) || 
            string.Equals(cmd, "Crouch", System.StringComparison.OrdinalIgnoreCase) || 
            string.Equals(cmd, "MoveForward", System.StringComparison.OrdinalIgnoreCase))
        {
            animator.SetBool("left", false);
            animator.SetBool("right", false);
        }

        if (string.Equals(cmd, "MoveLeft", System.StringComparison.OrdinalIgnoreCase))
        {
            moveX = -moveSpeed;
            animator.SetBool("left", true);
        }

        if (string.Equals(cmd, "MoveRight", System.StringComparison.OrdinalIgnoreCase))
        {   
            animator.SetBool("right", true);
            moveX = moveSpeed;
        }

        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        if (string.Equals(cmd, "Jump", System.StringComparison.OrdinalIgnoreCase)
            && !string.Equals(lastCommand, "Jump", System.StringComparison.OrdinalIgnoreCase)
            && controller.isGrounded
            && Time.time - lastJumpTime >= jumpCooldown)
        {
            verticalVelocity = jumpForce;
            lastJumpTime = Time.time;

            animator.SetTrigger("jump");
        }

        verticalVelocity += gravity * Time.deltaTime;

        // ขยับเฉพาะ X,Y
        Vector3 move = new Vector3(moveX, verticalVelocity, 0f);

        // แปลง local → world
        Vector3 worldMove = transform.parent.TransformDirection(move);

        controller.Move(worldMove * Time.deltaTime);

        // จำกัดการหลบซ้ายขวา
        Vector3 pos = transform.localPosition;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = 0f; // ล็อคแกน Z
        transform.localPosition = pos;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle") || other.CompareTag("Bullet") || other.CompareTag("Enemy"))
        {
            // ค้นหาว่า Collider ตัวไหนของผู้เล่นที่ทับซ้อนกับสิ่งกีดขวาง/กระสุน/ศัตรู
            Collider playerCollider = null;
            Collider[] colls = GetComponentsInChildren<Collider>(true);
            foreach (var pc in colls)
            {
                if (pc != null && pc != other && pc.bounds.Intersects(other.bounds))
                {
                    playerCollider = pc;
                    break;
                }
            }

            if (playerCollider != null)
            {
                // ถ้ากำหนดลิสต์ playerColliders ไว้ใน Inspector
                if (playerColliders != null && playerColliders.Count > 0)
                {
                    // ข้ามถ้าไม่ใช่ตัวที่เราลากใส่ไว้
                    if (!playerColliders.Contains(playerCollider))
                    {
                        return;
                    }
                }
                else
                {
                    // ถ้าปล่อยลิสต์ว่างไว้ ให้ข้ามการชนจาก Trigger หรือ BoxCollider (เช่น AttackRange) บนตัวผู้เล่น
                    if (playerCollider.isTrigger || playerCollider is BoxCollider)
                    {
                        return;
                    }
                }
            }

            HandleCollisionImpact(other, other.tag);
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Obstacle") || hit.collider.CompareTag("Bullet") || hit.collider.CompareTag("Enemy"))
        {
            HandleCollisionImpact(hit.collider, hit.collider.tag);
        }
    }

    void HandleCollisionImpact(Collider hitCollider, string tag)
    {
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null && health.IsDead()) return;

        bool isObstacle = (tag == "Obstacle");

        if (isObstacle)
        {
            cowskill cow = GetComponent<cowskill>();
            if (cow != null && cow.IsActive)
            {
                if (hitCollider != null)
                {
                    Debug.Log($"[PlayerController] CowSkill active: Destroying obstacle {hitCollider.gameObject.name} upon contact.");
                    Destroy(hitCollider.gameObject);
                }
                return;
            }
        }

        // If already blinking/slowed, don't trigger damage or slow down again
        if (isBlinking || isSlowed)
        {
            if (hitCollider != null && !hitCollider.isTrigger)
                StartCoroutine(IgnoreCollisionTemp(hitCollider));
            return;
        }

        // Damage the player only if it's an Obstacle (bullets/enemies do damage via their own scripts)
        if (isObstacle)
        {
            if (health != null && !health.IsInvincible)
                health.TakeDamage(obstacleDamage, "Obstacle");
        }

        StartCoroutine(ObstacleImpactRoutine(hitCollider, isObstacle));
    }

    private void SetObstacleCollisionIgnore(Collider obstacleCollider, bool ignore)
    {
        if (obstacleCollider == null) return;

        // ถ้ากำหนดใน List ไว้ ให้ใช้จาก List
        if (playerColliders != null && playerColliders.Count > 0)
        {
            foreach (var pc in playerColliders)
            {
                if (pc != null)
                {
                    Physics.IgnoreCollision(pc, obstacleCollider, ignore);
                }
            }
        }
        else
        {
            // ถ้าไม่กำหนด ให้ใช้เฉพาะ Collider ที่อยู่บน GameObject หลักของผู้เล่นเท่านั้น (เช่น CharacterController)
            Collider[] colls = GetComponents<Collider>();
            foreach (var pc in colls)
            {
                if (pc != null)
                {
                    // ข้าม trigger หรือ BoxCollider ที่ใช้ตรวจจับบนตัวหลัก (ถ้ามี)
                    if (pc.isTrigger || pc is BoxCollider)
                    {
                        continue;
                    }

                    Physics.IgnoreCollision(pc, obstacleCollider, ignore);
                }
            }
        }
    }

    private IEnumerator IgnoreCollisionTemp(Collider obstacleCollider)
    {
        if (obstacleCollider == null) yield break;

        SetObstacleCollisionIgnore(obstacleCollider, true);
        yield return new WaitForSeconds(slowDuration);

        SetObstacleCollisionIgnore(obstacleCollider, false);
    }

    private IEnumerator ObstacleImpactRoutine(Collider obstacleCollider, bool isObstacle)
    {
        isBlinking = true;

        if (isObstacle)
        {
            isSlowed = true;

            // 1. Slow down speeds
            originalMoveSpeed = moveSpeed;
            moveSpeed = originalMoveSpeed * slowSpeedMultiplier;

            if (stagePathController != null)
            {
                originalPathSpeed = stagePathController.moveSpeed;
                stagePathController.moveSpeed = originalPathSpeed * slowSpeedMultiplier;
            }
        }

        // 2. Ignore collision with this obstacle (if solid)
        if (obstacleCollider != null && !obstacleCollider.isTrigger)
        {
            SetObstacleCollisionIgnore(obstacleCollider, true);
        }

        // 3. Get all renderers and apply semi-transparency and blink
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
        Dictionary<Renderer, Color> originalBaseColors = new Dictionary<Renderer, Color>();

        foreach (var r in renderers)
        {
            if (r == null || r.material == null) continue;

            if (r.material.HasProperty("_Color"))
            {
                originalColors[r] = r.material.color;
                Color c = r.material.color;
                c.a = 0.5f;
                r.material.color = c;
            }
            else if (r.material.HasProperty("_BaseColor"))
            {
                originalBaseColors[r] = r.material.GetColor("_BaseColor");
                Color c = r.material.GetColor("_BaseColor");
                c.a = 0.5f;
                r.material.SetColor("_BaseColor", c);
            }
        }

        // Blink loop
        float elapsed = 0f;
        bool isVisible = true;
        while (elapsed < slowDuration)
        {
            isVisible = !isVisible;
            foreach (var r in renderers)
            {
                if (r != null) r.enabled = isVisible;
            }
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        // 4. Restore speeds if obstacle
        if (isObstacle)
        {
            moveSpeed = originalMoveSpeed;
            if (stagePathController != null)
            {
                stagePathController.moveSpeed = originalPathSpeed;
            }
            isSlowed = false;
        }

        // 5. Restore collision bypass
        SetObstacleCollisionIgnore(obstacleCollider, false);

        // Restore renderer states and colors
        foreach (var r in renderers)
        {
            if (r == null) continue;
            r.enabled = true;

            if (originalColors.ContainsKey(r) && r.material != null)
            {
                r.material.color = originalColors[r];
            }
            else if (originalBaseColors.ContainsKey(r) && r.material != null)
            {
                r.material.SetColor("_BaseColor", originalBaseColors[r]);
            }
        }

        isBlinking = false;
    }

    public void TriggerFlashOnly()
    {
        if (isBlinking || isSlowed) return;
        StartCoroutine(ObstacleImpactRoutine(null, false));
    }
}
