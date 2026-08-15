using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUIController : MonoBehaviour
{
    [Header("Canvas Tutorial")]
    [Tooltip("Canvas สำหรับ Tutorial โดยเฉพาะ จะถูกเปิดตอนเข้า Tutorial (Canvas หลักของเกมไม่ต้องปิด เปิดคู่กันได้เลย)")]
    public GameObject tutorialCanvas;

    [Serializable]
    public class StepUIMapping
    {
        [Tooltip("ต้องตรงกับ stageIndex ของ TutorialStepData ตัวที่ต้องการผูก")]
        public int stageIndex;
        [Tooltip("ลาก GameObject parent ของ UI step นี้ (ลูกของ Canvas Tutorial) มาใส่")]
        public GameObject uiObject;
    }

    [Header("จับคู่ stageIndex กับ UI (ผูกที่นี่แทน เพราะเป็น Scene object)")]
    public List<StepUIMapping> stepUIMappings = new List<StepUIMapping>();

    private Dictionary<int, GameObject> uiMap;

    [Header("UI หน้าจบด่าน")]
    [Tooltip("GameObject parent ของหน้าจอ 'จบด่านแล้ว' (เตรียมไว้ล่วงหน้าใน Canvas Tutorial เหมือนกับ step อื่นๆ)")]
    public GameObject finishUIObject;
    public float finishDelaySeconds = 3f;
    [Tooltip("ชื่อ Scene เมนูหลัก ต้องตรงกับที่อยู่ใน Build Settings")]
    public string menuSceneName = "Menu";

    // เก็บว่าตอนนี้ UI ของ step ไหนกำลังโชว์อยู่ เพื่อจะได้ปิดตอนเปลี่ยน step
    private GameObject currentlyShown;

    void Awake()
    {
        uiMap = new Dictionary<int, GameObject>();
        foreach (var mapping in stepUIMappings)
        {
            uiMap[mapping.stageIndex] = mapping.uiObject;
        }
    }

    // =========================
    // เข้าสู่โหมด Tutorial (เรียกครั้งเดียวตอนเริ่ม scene)
    // =========================
    public void EnterTutorialMode()
    {
        if (tutorialCanvas != null) tutorialCanvas.SetActive(true);

        // กันเหนียว: ปิด UI ของทุก step ไว้ก่อน เผื่อลืมปิดไว้ตั้งแต่ใน Editor
        if (finishUIObject != null) finishUIObject.SetActive(false);
        currentlyShown = null;
    }

    // =========================
    // โชว์ UI ของ step ที่กำหนด (ปิดของ step ก่อนหน้าอัตโนมัติ)
    // =========================
    public void ShowStepUI(int stageIndex)
    {
        if (currentlyShown != null)
            currentlyShown.SetActive(false);

        if (uiMap.TryGetValue(stageIndex, out GameObject stepUIObject) && stepUIObject != null)
        {
            stepUIObject.SetActive(true);
            currentlyShown = stepUIObject;
        }
        else
        {
            Debug.LogWarning($"[TutorialUIController] ไม่พบ UI ที่ผูกกับ stageIndex {stageIndex} — เช็ค Step UI Mappings ใน Inspector");
            currentlyShown = null;
        }
    }

    // =========================
    // ปิด UI ที่กำลังโชว์อยู่ทั้งหมด
    // =========================
    public void HideAll()
    {
        if (currentlyShown != null)
            currentlyShown.SetActive(false);

        currentlyShown = null;
    }

    // =========================
    // โชว์หน้าจบด่าน แล้วกลับ Menu อัตโนมัติหลัง 3 วิ
    // =========================
    public void ShowFinishScreen()
    {
        HideAll();

        if (finishUIObject != null) finishUIObject.SetActive(true);

        StartCoroutine(ReturnToMenuAfterDelay());
    }

    private System.Collections.IEnumerator ReturnToMenuAfterDelay()
    {
        // ใช้ unscaled เผื่อ timeScale ไม่ใช่ 1 ตอนนั้น
        yield return new WaitForSecondsRealtime(finishDelaySeconds);
        UnityEngine.SceneManagement.SceneManager.LoadScene(menuSceneName);
    }
}