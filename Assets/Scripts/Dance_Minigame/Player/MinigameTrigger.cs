using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ผู้เล่นเดินเข้า Collider → ปิดกล้องหลัก + เปิดกล้องมินิเกม + เริ่มเล่น
/// ตอนจบ → ปิดกล้องมินิเกม + เปิดกล้องหลักคืน + แจ้งผลลัพธ์
/// </summary>
[RequireComponent(typeof(Collider))]
public class MinigameTrigger : MonoBehaviour
{
    [Header("กล้อง")]
    [Tooltip("กล้องหลักของเกม จะถูกปิดระหว่างเล่นมินิเกม")]
    public Camera mainCamera;
    [Tooltip("กล้องของมินิเกม จะถูกเปิดตอนเข้ามินิเกม")]
    public Camera minigameCamera;

    [Header("เพลงของจุดนี้")]
    public BeatmapSO songForThisTrigger;

    [Header("UI Calibration (เปิดก่อนมินิเกม)")]
    [Tooltip("Root ของ UI แสดงสถานะ Calibrate แยกจาก UI มินิเกม")]
    public GameObject calibrateUIRoot;

    [Header("กำหนดว่าเพลงนี้สุ่มส่วนไหนได้บ้าง")]
    public bool allowLeftArm  = true;
    public bool allowRightArm = true;
    public bool allowLeftLeg  = true;
    public bool allowRightLeg = true;

    [Header("อ้างอิงระบบมินิเกม")]
    public BeatmapPlayer minigamePlayer;
    [Tooltip("Canvas/Root ของ UI มินิเกม")]
    public GameObject minigameUIRoot;

    [Header("UI เกมหลักที่จะปิดระหว่างเล่นมินิเกม")]
    public GameObject[] mainGameUIToHide;

    [Header("ตัวกรองผู้เล่น")]
    public string playerTag = "Player";

    [Header("GameObject ตัวละครผู้เล่นที่จะซ่อนระหว่างเล่นมินิเกม")]
    [Tooltip("จะถูก SetActive(false) ตอนเข้ามินิเกม และ SetActive(true) ตอนออก")]
    public GameObject playerGameObject;

    [Header("เล่นซ้ำได้ไหม")]
    [Tooltip("ถ้าติ๊ก พอผ่านครั้งแรกแล้ว Trigger นี้จะไม่ทำงานอีก")]
    public bool oneTimeOnly = true;

    [Header("Countdown ก่อนเริ่มเพลง")]
    [Tooltip("Text UI สำหรับแสดง 3 2 1 (ลาก TextMeshProUGUI ใส่)")]
    public TMPro.TextMeshProUGUI countdownText;
    [Tooltip("เวลาแต่ละตัวเลข (วินาที)")]
    public float countdownInterval = 1f;

    [Header("Events")]
    public UnityEvent onRewardUnlocked;
    public UnityEvent onFailed;

    bool _alreadyCompleted;
    bool _isInsideMinigame;
    float _nextAllowedTriggerTime = 0f;


    [Header("client")]
    public HandInputReceiver handInputReceiver;

    void Start()
    {
        // ตั้งค่าเริ่มต้น: กล้องมินิเกมปิด, UI มินิเกมปิด
        if (minigameCamera   != null) minigameCamera.gameObject.SetActive(false);
        if (minigameUIRoot   != null) minigameUIRoot.SetActive(false);
        if (calibrateUIRoot  != null) calibrateUIRoot.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (Time.time < _nextAllowedTriggerTime)    return;
        if (_isInsideMinigame)                     return;
        if (oneTimeOnly && _alreadyCompleted)      return;
        if (!other.CompareTag(playerTag))          return;

        StartMinigame();
    }

    void StartMinigame()
    {
        handInputReceiver.StartMinigame();
        if (songForThisTrigger == null)
        {
            //Debug.LogError($"[MinigameTrigger] {name} ไม่ได้ใส่ songForThisTrigger");
            return;
        }
        if (minigamePlayer == null)
        {
           // Debug.LogError($"[MinigameTrigger] {name} ไม่ได้ลาก minigamePlayer");
            return;
        }

        _isInsideMinigame = true;

        // ปิดกล้องหลัก → เปิดกล้องมินิเกม
        if (mainCamera   != null) mainCamera.gameObject.SetActive(false);
        if (minigameCamera != null) minigameCamera.gameObject.SetActive(true);

        // ปิด UI เกมหลัก → เปิด UI มินิเกม
        foreach (var ui in mainGameUIToHide)
            if (ui != null) ui.SetActive(false);
        if (minigameUIRoot != null) minigameUIRoot.SetActive(true);

        // ปิด GameObject ตัวละครผู้เล่น
        if (playerGameObject != null)
            playerGameObject.SetActive(false);

        // เปิด UI Calibrate ก่อน รอให้ Python calibrate เสร็จ
        if (calibrateUIRoot != null) calibrateUIRoot.SetActive(true);

        // ฟังผลจบเพลง
        minigamePlayer.OnSongFinished += HandleSongFinished;

        // รอ calibrate เสร็จก่อนค่อยนับถอยหลัง
        StartCoroutine(WaitForCalibrationThenCountdown());

        //Debug.Log($"[MinigameTrigger] เริ่มมินิเกม: {songForThisTrigger.songName}");
    }

    IEnumerator WaitForCalibrationThenCountdown()
    {
        // รอจนกว่าจะได้รับ packet จาก Python ก่อน (กันกรณี Python ยังไม่รัน)
        while (!HandDataReceiver.HasReceivedData)
        {
            //Debug.Log("[MinigameTrigger] รอการเชื่อมต่อจาก Python...");
            yield return new WaitForSeconds(0.5f);
        }

        // รอจน Python ส่ง calib_phase = "done"
        while (!HandDataReceiver.IsCalibrated)
            yield return null;

        // Calibrate เสร็จ → ปิด calibrateUI เปิด minigameUI
        if (calibrateUIRoot != null) calibrateUIRoot.SetActive(false);
        if (minigameUIRoot  != null) minigameUIRoot.SetActive(true);

        // นับถอยหลังแล้วเริ่มเพลง
        yield return StartCoroutine(CountdownThenPlay());
    }

    IEnumerator CountdownThenPlay()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);

            string[] steps = { "3", "2", "1", "GO!" };
            foreach (string step in steps)
            {
                countdownText.text = step;
                yield return new WaitForSeconds(countdownInterval);
            }

            countdownText.text = "";
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(countdownInterval * 3);
        }

        // ตั้งค่า limb ที่สุ่มได้สำหรับเพลงนี้
        if (minigamePlayer.randomPoseGenerator != null)
            minigamePlayer.randomPoseGenerator.SetAllowedLimbs(
                allowLeftArm, allowRightArm, allowLeftLeg, allowRightLeg);

        // เรียก Play() ตรงๆ
        minigamePlayer.Play(songForThisTrigger);
    }

    void HandleSongFinished(bool passed)
    {
        minigamePlayer.OnSongFinished -= HandleSongFinished;
        _isInsideMinigame = false;

        // ปิด Trigger Collider ไม่ให้ trigger ซ้ำอีก
        GetComponent<Collider>().enabled = false;

        // ปิดกล้องมินิเกม → เปิดกล้องหลักคืน
        if (minigameCamera != null) minigameCamera.gameObject.SetActive(false);
        if (mainCamera     != null) mainCamera.gameObject.SetActive(true);

        // ปิด UI ทั้งหมด
        if (calibrateUIRoot != null) calibrateUIRoot.SetActive(false);
        if (minigameUIRoot  != null) minigameUIRoot.SetActive(false);
        foreach (var ui in mainGameUIToHide)
            if (ui != null) ui.SetActive(true);

        // เปิด GameObject ตัวละครผู้เล่นคืน
        if (playerGameObject != null)
            playerGameObject.SetActive(true);

        if (passed)
        {
            _alreadyCompleted = true;
           Debug.Log($"[MinigameTrigger] ผ่าน! ปลดล็อกรางวัล");
            onRewardUnlocked?.Invoke();
        }
        else
        {
            // ไม่ผ่าน → เปิด Collider คืนให้ลองใหม่ได้
            GetComponent<Collider>().enabled = true;
            Debug.Log($"[MinigameTrigger] ไม่ผ่าน ลองใหม่ได้");
            onFailed?.Invoke();
        }
        _nextAllowedTriggerTime = Time.time + 3f; // ตั้ง cooldown 3 วินาที เพื่อกันการเหยียบซ้ำทันที
        handInputReceiver.StopMinigame();
    }
}