using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// จัดการแถบ Rhythm UI — ระบบ 3 โซน
///
/// Layout (Local X จากซ้ายไปขวา):
///   [Red Zone (พลาด)] [Green Zone (ตัดสิน)] [Black Zone (รอ)]
///   missLine           judgeLeft  judgeRight             spawnX
///
/// - PoseCard เลื่อนจาก spawnX ไปทางซ้าย
/// - เมื่อถึง judgeRight → เข้าโซนเขียว เริ่มเช็ค accuracy
/// - ผ่านเกณฑ์ก่อนเลย judgeLeft → Success
/// - เลย missLine ยังไม่ผ่าน → Miss
/// </summary>
public class RhythmLaneUI : MonoBehaviour
{
    [Header("Zone Boundaries (Local X)")]
    [Tooltip("เส้นซ้ายสุด: PoseCard ข้ามนี้ = Miss")]
    public float missLine   = -650f;
    [Tooltip("ขอบซ้ายของโซนเขียว")]
    public float judgeLeft  = -500f;
    [Tooltip("ขอบขวาของโซนเขียว: PoseCard เข้าเส้นนี้ = เริ่มเช็ค")]
    public float judgeRight = -350f;
    [Tooltip("จุด spawn PoseCard (ขวาสุดของแถบดำ)")]
    public float spawnX     =  800f;

    [Header("อ้างอิง")]
    public DancePoseEvaluator evaluator;
    public Transform cardContainer;
    public GameObject poseCardPrefab;
    [Tooltip("ตัวถ่ายภาพ Silhouette ทีละใบ")]
    public SilhouetteSnapshot silhouetteSnapshot;

    [Header("Progress UI")]
    [Tooltip("RawImage ที่จะเปลี่ยน RenderTexture ตามระดับเปอร์เซ็นต์ที่ทำได้")]
    public RawImage progressImage;
    [Tooltip("0-25%")] public RenderTexture progressSprite0;
    [Tooltip("25-50%")] public RenderTexture progressSprite25;
    [Tooltip("50-75%")] public RenderTexture progressSprite50;
    [Tooltip("75-100%")] public RenderTexture progressSprite75;
    public TextMeshProUGUI feedbackText;

    [Header("เกณฑ์")]
    [Tooltip("accuracy ขั้นต่ำที่นับว่าผ่านท่านี้ (0-1)")]
    [Range(0f, 1f)] public float passThreshold = 0.7f;

    [Tooltip("เปอร์เซ็นต์รวมขั้นต่ำที่ต้องทำได้ถึงจะผ่านด่าน (0-100)")]
    [Range(0f, 100f)] public float stagePassPercent = 80f;

    // เผื่อ BeatmapPlayer ต้องรู้ว่าทั้งหมดกี่ท่า (ตั้งตอนเริ่มเพลง)
    [HideInInspector] public int totalCues;

    public float JudgeZoneLeft  => judgeLeft;
    public float JudgeZoneRight => judgeRight;
    public float MissLine       => missLine;

    readonly List<PoseCard> _activeCards = new();
    PoseCard _currentJudgeCard;

    int _successCount;
    int _judgedCount;

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

    public float GetSpawnToJudgeDistance() => spawnX - judgeRight;

    /// <summary>BeatmapPlayer เรียกเพื่อ spawn PoseCard ใหม่</summary>
    public void SpawnPoseCard(
        BeatmapCue cue,
        DancePose resolvedPose,
        float scrollSpeed,
        RandomPoseGenerator.LimbPart limb,
        float targetValue)
    {
        if (poseCardPrefab == null || cardContainer == null)
        {
            Debug.LogError("[RhythmLaneUI] poseCardPrefab หรือ cardContainer เป็น null");
            return;
        }

        GameObject go = Instantiate(poseCardPrefab, cardContainer);
        go.transform.localPosition = new Vector3(spawnX, 0f, 0f);

        PoseCard card = go.GetComponent<PoseCard>();
        card.Init(resolvedPose, scrollSpeed, this, silhouetteSnapshot, limb, targetValue);
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
        if (progressImage == null) return;

        float p = CurrentPercent;

        RenderTexture target;
        if (p < 25f)       target = progressSprite0;
        else if (p < 50f)  target = progressSprite25;
        else if (p < 75f)  target = progressSprite50;
        else               target = progressSprite75;

        if (target != null)
            progressImage.texture = target;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector3 o = transform.position;
        float h = 50f;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(o + new Vector3(missLine,   -h, 0), o + new Vector3(missLine,    h, 0));

        Gizmos.color = Color.green;
        Gizmos.DrawLine(o + new Vector3(judgeLeft,  -h, 0), o + new Vector3(judgeLeft,   h, 0));
        Gizmos.DrawLine(o + new Vector3(judgeRight, -h, 0), o + new Vector3(judgeRight,  h, 0));

        Gizmos.color = Color.white;
        Gizmos.DrawLine(o + new Vector3(spawnX,     -h, 0), o + new Vector3(spawnX,      h, 0));
    }
#endif
}