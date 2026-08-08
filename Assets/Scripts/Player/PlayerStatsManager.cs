using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance { get; private set; }

    [Header("Base Stat Settings")]
    public float baseMaxHP = 100f;
    public float hpPerLevel = 20f;
    public int baseHPCost = 10;
    public int hpCostIncreasePerLevel = 5;

    public float baseMaxMP = 100f;
    public float mpPerLevel = 20f;
    public int baseMPCost = 10;
    public int mpCostIncreasePerLevel = 5;

    // PlayerPrefs Keys
    private const string KeyPoints = "PlayerPoints";
    private const string KeyHPLevel = "HP_Level";
    private const string KeyMPLevel = "MP_Level";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ─────────────────────────────
    //  Points
    // ─────────────────────────────
    public static int GetPoints()
    {
        return PlayerPrefs.GetInt(KeyPoints, 0);
    }

    public static void AddPoints(int amount)
    {
        int points = GetPoints() + amount;
        PlayerPrefs.SetInt(KeyPoints, points);
        PlayerPrefs.Save();
        Debug.Log($"[PlayerStats] +{amount} Points! รวมเป็น: {points}");
    }

    public static bool ConsumePoints(int amount)
    {
        int points = GetPoints();
        if (points >= amount)
        {
            PlayerPrefs.SetInt(KeyPoints, points - amount);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }

    // ─────────────────────────────
    //  HP Upgrades
    // ─────────────────────────────
    public static int GetHPLevel()
    {
        return PlayerPrefs.GetInt(KeyHPLevel, 0);
    }

    public float GetMaxHP()
    {
        return baseMaxHP + (GetHPLevel() * hpPerLevel);
    }

    public int GetHPUpgradeCost()
    {
        return baseHPCost + (GetHPLevel() * hpCostIncreasePerLevel);
    }

    public bool TryUpgradeHP()
    {
        int cost = GetHPUpgradeCost();
        if (ConsumePoints(cost))
        {
            int nextLevel = GetHPLevel() + 1;
            PlayerPrefs.SetInt(KeyHPLevel, nextLevel);
            PlayerPrefs.Save();
            Debug.Log($"[PlayerStats] อัปเกรด HP สำเร็จ! ระดับปัจจุบัน: {nextLevel} | Max HP: {GetMaxHP()}");
            return true;
        }
        Debug.LogWarning("[PlayerStats] แต้มไม่พอสำหรับอัปเกรด HP!");
        return false;
    }

    // ─────────────────────────────
    //  MP / Energy Upgrades
    // ─────────────────────────────
    public static int GetMPLevel()
    {
        return PlayerPrefs.GetInt(KeyMPLevel, 0);
    }

    public float GetMaxMP()
    {
        return baseMaxMP + (GetMPLevel() * mpPerLevel);
    }

    public int GetMPUpgradeCost()
    {
        return baseMPCost + (GetMPLevel() * mpCostIncreasePerLevel);
    }

    public bool TryUpgradeMP()
    {
        int cost = GetMPUpgradeCost();
        if (ConsumePoints(cost))
        {
            int nextLevel = GetMPLevel() + 1;
            PlayerPrefs.SetInt(KeyMPLevel, nextLevel);
            PlayerPrefs.Save();
            Debug.Log($"[PlayerStats] อัปเกรด MP สำเร็จ! ระดับปัจจุบัน: {nextLevel} | Max MP: {GetMaxMP()}");
            return true;
        }
        Debug.LogWarning("[PlayerStats] แต้มไม่พอสำหรับอัปเกรด MP!");
        return false;
    }

    // ─────────────────────────────
    //  Reset Stats (สำหรับทดสอบ)
    // ─────────────────────────────
    public static void ResetAllStats()
    {
        PlayerPrefs.DeleteKey(KeyPoints);
        PlayerPrefs.DeleteKey(KeyHPLevel);
        PlayerPrefs.DeleteKey(KeyMPLevel);
        PlayerPrefs.Save();
        Debug.Log("[PlayerStats] รีเซ็ตสเตตัสและแต้มเรียบร้อยแล้ว!");
    }
}
