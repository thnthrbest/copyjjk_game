using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SetupUIController : MonoBehaviour
{
    [Header("Manager Reference")]
    public PlayerStatsManager statsManager;

    [Header("Points UI")]
    public TextMeshProUGUI pointsText;

    [Header("HP Upgrade UI")]
    public TextMeshProUGUI hpLevelText;
    public TextMeshProUGUI hpMaxText;
    public TextMeshProUGUI hpCostText;
    public Button hpUpgradeButton;
    public Image[] hpLevelImages;
    public Color hpActiveColor = new Color32(82, 176, 75, 255); // #52B04B
    public Color hpInactiveColor = new Color32(80, 80, 80, 128); // สีเทาจางสำหรับ level ที่ยังไม่ได้อัปเกรด

    [Header("MP Upgrade UI")]
    public TextMeshProUGUI mpLevelText;
    public TextMeshProUGUI mpMaxText;
    public TextMeshProUGUI mpCostText;
    public Button mpUpgradeButton;
    public Image[] mpLevelImages;
    public Color mpActiveColor = new Color32(106, 198, 241, 255); // #6AC6F1
    public Color mpInactiveColor = new Color32(80, 80, 80, 128); // สีเทาจางสำหรับ level ที่ยังไม่ได้อัปเกรด

    [Header("Navigation UI")]
    public Button startGameButton;
    public string nextStageSceneName = "stage_1";

    public Button backToMenuButton;
    public string mainMenuSceneName = "MainMenu";

    [Header("Sound Effects")]
    public AudioClip upgradeSuccessSound;
    public AudioClip upgradeFailSound;

    private void Start()
    {
        // สร้าง StatsManager ชั่วคราวถ้ายังไม่มีใน Scene
        if (statsManager == null && PlayerStatsManager.Instance != null)
        {
            statsManager = PlayerStatsManager.Instance;
        }

        // ผูกปุ่มกด
        // if (hpUpgradeButton != null) hpUpgradeButton.onClick.AddListener(OnUpgradeHPClicked);
        // if (mpUpgradeButton != null) mpUpgradeButton.onClick.AddListener(OnUpgradeMPClicked);
        // if (startGameButton != null) startGameButton.onClick.AddListener(OnStartGameClicked);
        // if (backToMenuButton != null) backToMenuButton.onClick.AddListener(OnBackToMainMenuClicked);

        UpdateUI();
    }

    public void UpdateUI()
    {
        int currentPoints = PlayerStatsManager.GetPoints();

        // 1. แสดงแต้มคงเหลือ
        if (pointsText != null)
        {
            pointsText.text = $"{currentPoints}";
        }

        // 2. แสดงข้อมูล HP
        int hpLevel = PlayerStatsManager.GetHPLevel();
        float maxHP = GetStatsManager().GetMaxHP();
        int hpCost = GetStatsManager().GetHPUpgradeCost();

        if (hpLevelText != null) hpLevelText.text = $"HP Level: {hpLevel}";
        if (hpMaxText != null) hpMaxText.text = $"Max HP: {maxHP}";
        if (hpCostText != null) hpCostText.text = $"Cost: {hpCost}";
        //if (hpUpgradeButton != null) hpUpgradeButton.interactable = (currentPoints >= hpCost);

        // อัปเดตสี Image ของ HP ตาม Level
        if (hpLevelImages != null)
        {
            for (int i = 0; i < hpLevelImages.Length; i++)
            {
                if (hpLevelImages[i] != null)
                {
                    hpLevelImages[i].color = (i < hpLevel) ? hpActiveColor : hpInactiveColor;
                }
            }
        }

        // 3. แสดงข้อมูล MP
        int mpLevel = PlayerStatsManager.GetMPLevel();
        float maxMP = GetStatsManager().GetMaxMP();
        int mpCost = GetStatsManager().GetMPUpgradeCost();

        if (mpLevelText != null) mpLevelText.text = $"MP Level: {mpLevel}";
        if (mpMaxText != null) mpMaxText.text = $"Max MP: {maxMP}";
        if (mpCostText != null) mpCostText.text = $"Cost: {mpCost}";
        if (mpUpgradeButton != null) mpUpgradeButton.interactable = (currentPoints >= mpCost);

        // อัปเดตสี Image ของ MP ตาม Level
        if (mpLevelImages != null)
        {
            for (int i = 0; i < mpLevelImages.Length; i++)
            {
                if (mpLevelImages[i] != null)
                {
                    mpLevelImages[i].color = (i < mpLevel) ? mpActiveColor : mpInactiveColor;
                }
            }
        }
    }

    public void OnUpgradeHPClicked()
    {
        Debug.Log("kuy");
        if (GetStatsManager().TryUpgradeHP())
        {
            PlaySFX(upgradeSuccessSound);
            UpdateUI();
        }
        else
        {
            PlaySFX(upgradeFailSound);
        }
    }

    public void OnUpgradeMPClicked()
    {
        if (GetStatsManager().TryUpgradeMP())
        {
            PlaySFX(upgradeSuccessSound);
            UpdateUI();
        }
        else
        {
            PlaySFX(upgradeFailSound);
        }
    }

    public void OnStartGameClicked()
    {
        Debug.Log($"SetupUI: เข้าสู่ฉาก {nextStageSceneName}");
        SceneManager.LoadScene(nextStageSceneName);
    }

    public void OnBackToMainMenuClicked()
    {
        Debug.Log($"SetupUI: กลับสู่ฉาก {mainMenuSceneName}");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private PlayerStatsManager GetStatsManager()
    {
        if (statsManager == null)
        {
            if (PlayerStatsManager.Instance != null)
                statsManager = PlayerStatsManager.Instance;
            else
            {
                // สร้าง Component ชั่วคราวหากไม่ได้วางไว้ใน Scene
                GameObject go = new GameObject("PlayerStatsManager");
                statsManager = go.AddComponent<PlayerStatsManager>();
            }
        }
        return statsManager;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(clip);
        }
    }
}
