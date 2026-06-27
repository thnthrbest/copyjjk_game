using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// จัดการแถบ Rhythm UI — ระบบ 2 โซนแบบง่าย (ผ่าน/ไม่ผ่าน ไม่มีระดับดาว)
///
/// Layout (Local X จากซ้ายไปขวา):
///   [Red Zone (พลาด)] [Green Zone (โซนตัดสิน/ผ่าน)]
///   missLine           judgeLine ......... spawnX
///
/// - PoseCard เลื่อนจาก spawnX ไปทางซ้าย
/// - เมื่อถึง judgeLine -> เข้าโซนเขียว เริ่มเช็ค accuracy ทุก frame
/// - ผ่านเกณฑ์ (>= passThreshold) เมื่อไหร่ -> Success ทันที (ไม่ต้องรอถึงไหน)
/// - ถ้าเลย missLine (เข้าโซนแดง) ก่อนผ่าน -> Miss
///
/// คะแนนรวมแสดงเป็น % จำนวนท่าที่สำเร็จจากทั้งหมด ไม่ใช้ระบบ Score สะสม
/// เกณฑ์ผ่านด่าน (ปรับได้): stagePassPercent (ค่าเริ่มต้น 80%)
/// </summary>
public class RhythmLaneUI : MonoBehaviour
{
    [Header("Zone Boundaries (Local X)")]
    [Tooltip("เส้นซ้ายสุด: ถ้า PoseCard ข้ามนี้ไปโดยยังไม่ผ่าน = Miss")]
    public float missLine  = -650f;
    [Tooltip("เส้นตัดสิน: PoseCard เข้าเส้นนี้ = เริ่มเช็ค accuracy")]
    public float judgeLine = -450f;
    [Tooltip("จุด spawn PoseCard (ขวาสุดของแถบดำ)")]
    public float spawnX    =  800f;

    [Header("อ้างอิง")]
    public DancePoseEvaluator evaluator;
    public Transform cardContainer;
    public GameObject poseCardPrefab;

    [Header("Progress UI")]
    [Tooltip("แสดงผลแบบ '8/10 (80%)'")]
    public TextMeshProUGUI percentText;
    public TextMeshProUGUI feedbackText;   // ขึ้นข้อความสั้นๆ ตอนตัดสินแต่ละท่า (PASS / MISS)

    [Header("เกณฑ์")]
    [Tooltip("accuracy ขั้นต่ำที่นับว่าผ่านท่านี้ (0-1)")]
    [Range(0f, 1f)] public float passThreshold = 0.7f;

    [Tooltip("เปอร์เซ็นต์รวมขั้นต่ำที่ต้องทำได้ถึงจะผ่านด่าน (0-100)")]
    [Range(0f, 100f)] public float stagePassPercent = 80f;

    // เผื่อ BeatmapPlayer ต้องรู้ว่าทั้งหมดกี่ท่า (ตั้งตอนเริ่มเพลง)
    [HideInInspector] public int totalCues;

    public float JudgeLine => judgeLine;
    public float MissLine  => missLine;

    readonly List<PoseCard> _activeCards = new();
    PoseCard _currentJudgeCard;

    int _successCount;
    int _judgedCount;     // ท่าที่ตัดสินแล้ว (ไม่ว่าผ่านหรือพลาด)

    public float CurrentPercent => totalCues > 0 ? (_successCount / (float)totalCues) * 100f : 0f;
    public bool  IsStagePassed  => CurrentPercent >= stagePassPercent && _judgedCount >= totalCues;

    // เรียกตอนเริ่มเพลงใหม่ เพื่อ reset และบอกจำนวนท่าทั้งหมด
    public void ResetProgress(int totalCueCount)
    {
        totalCues      = totalCueCount;
        _successCount  = 0;
        _judgedCount   = 0;
        _activeCards.Clear();
        _currentJudgeCard = null;
        UpdatePercentUI();
    }

    public float GetSpawnToJudgeDistance() => spawnX - judgeLine;

    /// <summary>BeatmapPlayer เรียกเพื่อ spawn PoseCard ใหม่ พร้อมท่าที่ resolve แล้ว (กำหนดไว้ล่วงหน้าหรือสุ่มมา)</summary>
    public void SpawnPoseCard(BeatmapCue cue, DancePose resolvedPose, float scrollSpeed)
    {
        if (poseCardPrefab == null || cardContainer == null)
        {
            Debug.LogError("[RhythmLaneUI] poseCardPrefab หรือ cardContainer เป็น null");
            return;
        }

        GameObject go = Instantiate(poseCardPrefab, cardContainer);
        go.transform.localPosition = new Vector3(spawnX, 0f, 0f);

        PoseCard card = go.GetComponent<PoseCard>();
        card.Init(resolvedPose, scrollSpeed, this);
        _activeCards.Add(card);
    }

    /// <summary>ยิงเมื่อการ์ดปัจจุบันหายไป (ผ่านหรือพลาด) พร้อมให้ spawn ใบถัดไปได้แล้ว</summary>
    public event System.Action OnCardFinished;

    public void OnCardEnterJudgeZone(PoseCard card)
    {
        _currentJudgeCard = card;
        evaluator?.SetPose(card.dancePose);
    }

    public void OnCardMiss(PoseCard card)
    {
        card.MarkMiss();
        _activeCards.Remove(card);
        _judgedCount++;

        ShowFeedback("MISS", Color.red);
        UpdatePercentUI();

        if (_currentJudgeCard == card)
            _currentJudgeCard = null;

        OnCardFinished?.Invoke();
    }

    void Update()
    {
        if (_currentJudgeCard == null || evaluator == null) return;
        if (_currentJudgeCard.State != PoseCard.CardState.Judging) return;

        if (evaluator.accuracy >= passThreshold)
        {
            _successCount++;
            _judgedCount++;

            _currentJudgeCard.MarkSuccess();
            _activeCards.Remove(_currentJudgeCard);
            _currentJudgeCard = null;

            ShowFeedback("PASS", Color.green);
            UpdatePercentUI();
            OnCardFinished?.Invoke();
        }
    }

    void ShowFeedback(string msg, Color color)
    {
        if (feedbackText == null) return;
        StopAllCoroutines();
        StartCoroutine(FeedbackFade(msg, color));
    }

    IEnumerator FeedbackFade(string msg, Color color)
    {
        feedbackText.text = msg;
        Color c = color;
        c.a = 1f;
        feedbackText.color = c;

        yield return new WaitForSeconds(0.3f);

        float elapsed = 0f, duration = 0.5f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / duration);
            feedbackText.color = c;
            yield return null;
        }
    }

    void UpdatePercentUI()
    {
        if (percentText == null) return;
        percentText.text = $"{_successCount}/{totalCues} ({CurrentPercent:F0}%)";
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector3 o = transform.position;
        float h = 50f;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(o + new Vector3(missLine,  -h, 0), o + new Vector3(missLine,  h, 0));

        Gizmos.color = Color.green;
        Gizmos.DrawLine(o + new Vector3(judgeLine, -h, 0), o + new Vector3(judgeLine, h, 0));

        Gizmos.color = Color.white;
        Gizmos.DrawLine(o + new Vector3(spawnX,    -h, 0), o + new Vector3(spawnX,    h, 0));
    }
#endif
}