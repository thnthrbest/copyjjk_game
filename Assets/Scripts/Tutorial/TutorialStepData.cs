using System.Collections.Generic;
using UnityEngine;

// ตัวแปรที่ใช้เช็คเงื่อนไขได้ ตรงกับค่าที่ HandInputReceiver อัปเดตอยู่แล้ว
public enum TutorialConditionSource
{
    LeftHand,   // HandInputReceiver.LeftHand  เช่น "MoveLeft", "MoveRight", "Jump", "Crouch", "Idle"
    RightHand,  // HandInputReceiver.RightHand เช่น "SHOOT", "AIM", "SWITCH TARGET", "NONE"
    Gesture     // HandInputReceiver.Gesture   เช่น "rabbit", "dog", "cow", "deer", "dont"
}

// Any = ตรงข้อใดข้อหนึ่งก็ผ่าน (OR) / All = ต้องตรงทุกข้อพร้อมกัน (AND)
public enum TutorialConditionLogic
{
    Any,
    All
}

[System.Serializable]
public class TutorialCondition
{
    public TutorialConditionSource source;
    public string expectedValue; // ต้องตรงกับ string ที่ HandInputReceiver ใช้เป๊ะ (case-sensitive)
}

[CreateAssetMenu(fileName = "TutorialStep", menuName = "Tutorial/Step")]
public class TutorialStepData : ScriptableObject
{
    [Header("ผูกกับจุดใน StagePathController.stagePoints")]
    [Tooltip("Index ของ stagePoints ที่จะ trigger tutorial นี้")]
    public int stageIndex;

    [Header("เงื่อนไขผ่าน (สำหรับ step ทั่วไป / หรือจังหวะที่ 1 ของ step ใช้สกิล)")]
    public TutorialConditionLogic conditionLogic = TutorialConditionLogic.Any;
    [Tooltip("เช่น step เดิน จะมี 2 เงื่อนไข (Any): MoveLeft กับ MoveRight ทำอันใดอันหนึ่งก็ผ่าน\nstep ใช้สกิล จังหวะแรกจะตั้ง Logic เป็น All: LeftHand=Idle และ RightHand=NONE ต้องตรงพร้อมกัน")]
    public List<TutorialCondition> acceptedConditions = new List<TutorialCondition>();

    [Header("ระยะเวลาที่ต้องค้างท่าไว้ก่อนนับว่าผ่าน (วินาที, unscaled)")]
    public float holdDuration = 0.5f;

    [Header("กรณีพิเศษ — Step ใช้สกิล (มี 2 จังหวะ)")]
    [Tooltip("ติ๊กเฉพาะ step ใช้สกิล: จังหวะแรกใช้ acceptedConditions (รอมือเปล่าค้าง) พอผ่านจะปลด freeze ให้ทำจริง แล้วรอ Skill Final Conditions (เช่น Gesture=rabbit) ถึงจะจบ step จริง")]
    public bool isSkillStep = false;

    [Tooltip("เงื่อนไขจังหวะที่ 2 ของ step ใช้สกิล (ตรวจแบบ Any) — ใช้ตอน isSkillStep เท่านั้น")]
    public List<TutorialCondition> skillFinalConditions = new List<TutorialCondition>();

    // เช็คเงื่อนไขจังหวะที่ 1 (หรือ step ทั่วไป)
    public bool IsEntryConditionMet()
    {
        return Evaluate(acceptedConditions, conditionLogic);
    }

    // เช็คเงื่อนไขจังหวะที่ 2 (เฉพาะ step ใช้สกิล)
    public bool IsSkillFinalConditionMet()
    {
        return Evaluate(skillFinalConditions, TutorialConditionLogic.Any);
    }

    private bool Evaluate(List<TutorialCondition> conditions, TutorialConditionLogic logic)
    {
        if (conditions == null || conditions.Count == 0) return false;

        foreach (var cond in conditions)
        {
            string currentValue = cond.source switch
            {
                TutorialConditionSource.LeftHand => HandInputReceiver.LeftHand,
                TutorialConditionSource.RightHand => HandInputReceiver.RightHand,
                TutorialConditionSource.Gesture => HandInputReceiver.Gesture,
                _ => null
            };

            bool matched = currentValue == cond.expectedValue;

            if (logic == TutorialConditionLogic.Any && matched) return true;
            if (logic == TutorialConditionLogic.All && !matched) return false;
        }

        // Any: ไม่มีข้อไหนตรงเลย -> false / All: ตรงทุกข้อแล้ว -> true
        return logic == TutorialConditionLogic.All;
    }
}