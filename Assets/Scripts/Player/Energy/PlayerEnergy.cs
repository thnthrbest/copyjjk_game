using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerEnergy : MonoBehaviour
{
    [Header("Energy Settings")]
    public float maxEnergyPerStack = 100f;   // พลังงานต่อ 1 สแต็ก
    public int   maxStacks         = 3;      // สแต็กสูงสุด

    [Header("Energy Gain")]
    public float energyPerKill     = 40f;    // ได้จากฆ่า Enemy
    public float energyPerPickup   = 10f;    // ได้จากเก็บของ

    // ─── State ───
    private float currentEnergy = 0f;
    private int   currentStacks = 0;

    [Header("UI")]
    public Slider      energySlider;         // แถบพลังงานปัจจุบัน
    public Image       energyFill;           // สีแถบ
    public GameObject[] stackIcons;          // Icon แสดงสแต็ก 3 อัน

    [Header("Stack Color")]
    public Color colorEmpty    = Color.gray;
    public Color colorFilled   = Color.cyan;
    public Color colorMax      = Color.yellow;  // ครบ 3 สแต็ก

    // ─── Singleton ───
    public static PlayerEnergy Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    // ─────────────────────────────
    //  เพิ่มพลังงาน
    // ─────────────────────────────
    public void AddEnergy(float amount, string source = "")
    {
        if (currentStacks >= maxStacks) return;  // เต็มแล้ว

        currentEnergy += amount;
        Debug.Log($"[Energy] +{amount} จาก {source} | " +
                  $"Energy: {currentEnergy}/{maxEnergyPerStack} | " +
                  $"Stack: {currentStacks}/{maxStacks}");

        // ─── เช็คว่าครบ 100 ไหม ───
        while (currentEnergy >= maxEnergyPerStack && currentStacks < maxStacks)
        {
            currentEnergy -= maxEnergyPerStack;
            currentStacks++;
            Debug.Log($"[Energy] ได้ Stack! รวม {currentStacks}/{maxStacks}");
            OnStackGained(currentStacks);
        }

        // ─── ถ้าสแต็กเต็มแล้ว พลังงานส่วนเกินทิ้ง ───
        if (currentStacks >= maxStacks)
            currentEnergy = 0f;

        UpdateUI();
    }

    // ─────────────────────────────
    //  ใช้สแต็ก
    // ─────────────────────────────
    public bool UseStack(int amount = 1)
    {
        if (currentStacks < amount)
        {
            Debug.Log($"[Energy] Stack ไม่พอ! มี {currentStacks} ต้องการ {amount}");
            return false;
        }

        currentStacks -= amount;
        Debug.Log($"[Energy] ใช้ {amount} Stack | เหลือ {currentStacks}/{maxStacks}");
        UpdateUI();
        return true;
    }

    // ─────────────────────────────
    //  เรียกจากภายนอก
    // ─────────────────────────────
    public void OnKillEnemy()
    {
        AddEnergy(energyPerKill, "Kill");
    }

    public void OnPickup()
    {
        AddEnergy(energyPerPickup, "Pickup");
    }

    // ─── เช็คสถานะ ───
    public int   GetStacks()     => currentStacks;
    public float GetEnergy()     => currentEnergy;
    public bool  HasStack()      => currentStacks > 0;
    public bool  IsMaxStack()    => currentStacks >= maxStacks;

    // ─────────────────────────────
    //  Event
    // ─────────────────────────────
    void OnStackGained(int stackCount)
    {
        Debug.Log($"[Energy] Stack {stackCount} เต็ม!");
        // เพิ่ม Effect เช่น particle, sound
    }

    // ─────────────────────────────
    //  UI
    // ─────────────────────────────
    void UpdateUI()
    {
        // ─── Energy Slider ───
        if (energySlider != null)
        {
            energySlider.value = currentStacks >= maxStacks
                ? 0f
                : currentEnergy / maxEnergyPerStack;
        }

        // ─── สีแถบ ───
        if (energyFill != null)
        {
            if (currentStacks >= maxStacks)
                energyFill.color = colorMax;
            else if (currentStacks > 0)
                energyFill.color = colorFilled;
            else
                energyFill.color = Color.cyan;
        }

        // ─── Stack Icons ───
        if (stackIcons != null)
        {
            for (int i = 0; i < stackIcons.Length; i++)
            {
                if (stackIcons[i] == null) continue;

                Image icon = stackIcons[i].GetComponent<Image>();
                if (icon == null) continue;

                if (i < currentStacks)
                {
                    icon.color = i == maxStacks - 1 && currentStacks == maxStacks
                        ? colorMax      // สแต็กสุดท้าย = สีพิเศษ
                        : colorFilled;  // มีสแต็ก
                }
                else
                {
                    icon.color = colorEmpty;  // ว่าง
                }
            }
        }
    }

    void OnGUI()
    {
        GUIStyle style  = new GUIStyle();
        style.fontSize  = 18;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.cyan;

        GUI.Label(new Rect(10, 100, 400, 30),
            $"Energy: {currentEnergy:F0}/{maxEnergyPerStack} | " +
            $"Stack: {currentStacks}/{maxStacks}", style);
    }
}