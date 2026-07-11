using UnityEngine;
using TMPro;

public class PythonConnectionLoader : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Panel UI ของหน้าจอ Loading ที่จะบดบังหน้าจอทั้งหมด")]
    public GameObject loadingPanel;
    
    [Tooltip("ข้อความที่แสดงสถานะการเชื่อมต่อ")]
    public TextMeshProUGUI statusText;

    [Header("Gameplay Controls to Lock")]
    [Tooltip("ใส่ Player GameObject เพื่อปิดการควบคุมระหว่างดาวน์โหลด (หากปล่อยว่างไว้จะหาในฉากให้อัตโนมัติ)")]
    public PlayerController playerController;

    [Tooltip("สคริปต์ควบคุมเส้นทางการเดินของฉาก (หากปล่อยว่างไว้จะหาในฉากให้อัตโนมัติ)")]
    public StagePathController stagePathController;

    [Tooltip("สคริปต์เชื่อมต่อ TCP (หากปล่อยว่างไว้จะหาในฉากให้อัตโนมัติ)")]
    public HandInputReceiver handInputReceiver;

    private Animator playerAnimator;
    private BulletTime bulletTime;
    private bool isGameStarted = false;

    void Start()
    {
        // ค้นหา HandInputReceiver อัตโนมัติถ้ายังไม่ได้ลากใส่ช่อง
        if (handInputReceiver == null)
        {
            handInputReceiver = FindObjectOfType<HandInputReceiver>();
        }

        // ค้นหา PlayerController อัตโนมัติถ้ายังไม่ได้ลากใส่ช่อง
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        // ค้นหา StagePathController อัตโนมัติถ้ายังไม่ได้ลากใส่ช่อง
        if (stagePathController == null)
        {
            stagePathController = FindObjectOfType<StagePathController>();
        }

        // ค้นหา BulletTime อัตโนมัติในฉาก
        if (bulletTime == null)
        {
            bulletTime = FindObjectOfType<BulletTime>();
        }

        // ค้นหา Animator จาก Player เพื่อหยุดท่าทางวิ่ง
        if (playerController != null)
        {
            playerAnimator = playerController.GetComponent<Animator>();
        }

        // 1. เปิดหน้าจอโหลดค้างไว้ตอนเริ่ม
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        if (statusText != null)
        {
            statusText.text = "Waiting for connection to Python...";
        }

        // 2. ปิดสคริปต์การควบคุมของผู้เล่นและการเคลื่อนที่ของฉาก
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (stagePathController != null)
        {
            stagePathController.enabled = false;
        }

        // ปิดการทำงานของ BulletTime ชั่วคราว เพื่อไม่ให้ยื้อเวลา Time.timeScale กลับเป็น 1.0
        if (bulletTime != null)
        {
            bulletTime.enabled = false;
        }

        // 3. หยุดแอนิเมชันของตัวละครไว้ (ตัวละครจะไม่ทำท่าวิ่งค้าง)
        if (playerAnimator != null)
        {
            playerAnimator.speed = 0f;
        }

        // 4. หยุดเวลาทางฟิสิกส์ของ Unity ทั้งหมด
        Time.timeScale = 0f; 
    }

    void Update()
    {
        if (isGameStarted) return;

        // เช็คว่า HandInputReceiver เชื่อมต่อ TCP สำเร็จแล้วหรือยัง
        if (handInputReceiver != null && handInputReceiver.IsConnected)
        {
            StartGame();
        }
        else
        {
            if (statusText != null)
            {
                statusText.text = "Connecting to Python Server... Please run python code.";
            }
        }
    }

    void StartGame()
    {
        isGameStarted = true;

        // 1. ปิดหน้าจอโหลด
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }

        // 2. เปิดการทำงานของสคริปต์ทั้งหมดให้ผู้เล่นขยับและด่านเคลื่อนที่ได้
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (stagePathController != null)
        {
            stagePathController.enabled = true;
        }

        if (bulletTime != null)
        {
            bulletTime.enabled = true;
        }

        // 3. ปล่อยให้แอนิเมชันทำงานต่อตามปกติ
        if (playerAnimator != null)
        {
            playerAnimator.speed = 1f;
        }

        // 4. ปล่อยเวลาในเกมให้ดำเนินไปตามปกติ
        Time.timeScale = 1f;

        Debug.Log("Connection established! Game started successfully.");
    }
}
