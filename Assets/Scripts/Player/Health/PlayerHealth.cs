using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth    = 100f;
    public float currentHealth;

    [Header("UI")]
    public Slider healthSlider;
    public Image  healthFill;       // Image ของ Slider Fill

    [Header("Health Color")]
    public Color colorHigh   = Color.green;   // เลือดเต็ม
    public Color colorMid    = Color.yellow;  // เลือดกลาง
    public Color colorLow    = Color.red;     // เลือดน้อย

    [Header("Invincible")]
    public float invincibleTime = 0.5f;       // โดนดาเมจซ้ำไม่ได้กี่วิ
    private float invincibleTimer = 0f;
    private bool  isInvincible   = false;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    void Update()
    {
        // ─── นับเวลา Invincible ───
        if (isInvincible)
        {
            invincibleTimer += Time.deltaTime;
            if (invincibleTimer >= invincibleTime)
            {
                isInvincible    = false;
                invincibleTimer = 0f;
            }
        }
    }

    // ─────────────────────────────
    //  รับดาเมจ — เรียกจากภายนอก
    // ─────────────────────────────
    public void TakeDamage(float damage, string source = "Unknown")
    {
        if (isDead || isInvincible) return;

        currentHealth  = Mathf.Max(0, currentHealth - damage);
        isInvincible   = true;
        invincibleTimer = 0f;

        UpdateUI();

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"[HP] ฟื้น {amount} | เหลือ {currentHealth}/{maxHealth}");
        UpdateUI();
    }

    void UpdateUI()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth / maxHealth;

        // ─── เปลี่ยนสีตามเลือดที่เหลือ ───
        if (healthFill != null)
        {
            float ratio = currentHealth / maxHealth;

            if (ratio > 0.5f)
                healthFill.color = Color.Lerp(colorMid, colorHigh, (ratio - 0.5f) * 2f);
            else
                healthFill.color = Color.Lerp(colorLow, colorMid, ratio * 2f);
        }
    }

    void Die()
    {
       healthFill.gameObject.SetActive(false);
        if (isDead) return;
        isDead = true;
        Debug.Log("[HP] Player ตายแล้ว!");

        // ─── เพิ่ม Logic ตาย ───
        // GetComponent<Animator>()?.SetTrigger("Die");
        // GameManager.Instance?.GameOver();
    }

    // ─── เช็คสถานะ ───
    public bool IsDead()    => isDead;
    public float GetRatio() => currentHealth / maxHealth;
}