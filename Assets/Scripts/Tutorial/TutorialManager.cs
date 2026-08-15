using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("อ้างอิง (ลากจาก Scene)")]
    public StagePathController pathController;
    public BulletTime bulletTime;
    public TutorialUIController uiController;

    [Header("รายการ Tutorial Step ทั้งหมด")]
    public List<TutorialStepData> steps = new List<TutorialStepData>();

    // ───── สถานะภายใน ─────
    private FieldInfo currentIndexField;
    private Dictionary<int, TutorialStepData> stepMap;
    private HashSet<int> triggeredIndices = new HashSet<int>();

    private bool isTutorialActive = false;
    private TutorialStepData activeStep = null;

    // สำหรับนับเวลาค้างท่า (unscaled เพราะ timeScale อาจเป็น 0 อยู่)
    private float holdTimer = 0f;

    // เฉพาะ step ใช้สกิล: true = ยังอยู่จังหวะที่ 1 (รอมือเปล่าค้าง ก่อนปลด freeze)
    //                     false = จังหวะที่ 2 (ปลด freeze แล้ว รอท่าสกิลจริง)
    private bool skillPhaseOne = false;

    private bool stageCompleteFired = false;

    void Awake()
    {
        Instance = this;

        currentIndexField = typeof(StagePathController)
            .GetField("currentIndex", BindingFlags.NonPublic | BindingFlags.Instance);

        stepMap = new Dictionary<int, TutorialStepData>();
        foreach (var step in steps)
        {
            stepMap[step.stageIndex] = step;
        }
    }

    void Start()
    {
        uiController.EnterTutorialMode();
        PreloadAllGifs();
    }

    void PreloadAllGifs()
    {
        var gifPlayers = uiController.tutorialCanvas.GetComponentsInChildren<GifPlayer>(true);
        foreach (var gp in gifPlayers)
        {
            gp.Load();
        }

        var gifSequences = uiController.tutorialCanvas.GetComponentsInChildren<GifSequencePlayer>(true);
        foreach (var gs in gifSequences)
        {
            gs.Load();
        }
    }

    void Update()
    {
        if (pathController == null) return;

        // ─── เช็คจบทั้งด่าน (ถึงจุดสุดท้ายที่ไม่มี tutorial ผูกไว้) ───
        if (!stageCompleteFired)
        {
            int idx = (int)currentIndexField.GetValue(pathController);
            if (idx >= pathController.stagePoints.Count)
            {
                stageCompleteFired = true;
                uiController.ShowFinishScreen();
                return;
            }
        }

        if (isTutorialActive)
        {
            UpdateActiveStep();
            return;
        }

        // ─── เช็คว่าถึงจุดที่มี tutorial ผูกไว้หรือยัง ───
        if (!pathController.moving && pathController.enabled)
        {
            int idx = (int)currentIndexField.GetValue(pathController);

            if (!triggeredIndices.Contains(idx) && stepMap.TryGetValue(idx, out TutorialStepData step))
            {
                triggeredIndices.Add(idx);
                BeginStep(step);
            }
        }
    }

    // =========================
    // เริ่ม Tutorial Step (freeze นิ่งสนิทเหมือนกันทุก step)
    // =========================
    void BeginStep(TutorialStepData step)
    {
        isTutorialActive = true;
        activeStep = step;
        holdTimer = 0f;
        skillPhaseOne = step.isSkillStep;

        pathController.enabled = false;
        if (bulletTime != null) bulletTime.enabled = false;

        uiController.ShowStepUI(step.stageIndex);   // ← เปิด UI ก่อน

        Time.timeScale = 0f;                        // ← ค่อย freeze ทีหลัง
    }

    // =========================
    // ระหว่าง Tutorial Step กำลังทำงาน
    // =========================
    void UpdateActiveStep()
    {
        // ─── Step ใช้สกิล จังหวะที่ 1: รอมือเปล่าค้างไว้ (freeze อยู่) ───
        if (activeStep.isSkillStep && skillPhaseOne)
        {
            if (activeStep.IsEntryConditionMet())
            {
                holdTimer += Time.unscaledDeltaTime;
                if (holdTimer >= activeStep.holdDuration)
                {
                    // จบจังหวะ 1: ปิดคำสอน ปลด freeze ให้เล่นจริง แต่ยังไม่จบ step
                    uiController.HideAll();
                    if (bulletTime != null) bulletTime.enabled = true;
                    Time.timeScale = 1f;
                    // pathController ยังปิดอยู่ต่อไป เพราะ step ยังไม่จบ

                    skillPhaseOne = false;
                    holdTimer = 0f;
                }
            }
            else
            {
                holdTimer = 0f;
            }
            return;
        }

        // ─── Step ใช้สกิล จังหวะที่ 2: เวลาเดินปกติแล้ว รอท่าสกิลจริง ───
        if (activeStep.isSkillStep && !skillPhaseOne)
        {
            if (activeStep.IsSkillFinalConditionMet())
            {
                holdTimer += Time.unscaledDeltaTime;
                if (holdTimer >= activeStep.holdDuration)
                {
                    EndStep();
                }
            }
            else
            {
                holdTimer = 0f;
            }
            return;
        }

        // ─── Step ทั่วไป (จังหวะเดียว) ───
        if (activeStep.IsEntryConditionMet())
        {
            holdTimer += Time.unscaledDeltaTime;
            if (holdTimer >= activeStep.holdDuration)
            {
                EndStep();
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }

    // =========================
    // จบ Tutorial Step
    // =========================
    void EndStep()
    {
        uiController.HideAll();

        // คืนค่าทุกอย่าง (สำหรับ step ใช้สกิล บางอย่างคืนไปแล้วตอนจบจังหวะ 1 ก็ไม่เป็นไร ตั้งซ้ำได้)
        if (bulletTime != null) bulletTime.enabled = true;
        Time.timeScale = 1f;
        pathController.enabled = true;

        isTutorialActive = false;
        activeStep = null;
    }
}