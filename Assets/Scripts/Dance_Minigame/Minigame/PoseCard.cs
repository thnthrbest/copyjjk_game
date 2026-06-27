using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PoseCard = รูป Silhouette ท่าเดียวที่เลื่อนจากขวาไปซ้ายในแถบ
///
/// Layout แถบ (ซ้าย -> ขวา):
///   [Red Zone (พลาด)] [Green Zone (ผ่าน)]
///   missLine           judgeLine ... spawnX
///
/// เข้าโซนเขียว (x <= judgeLine) -> เริ่มเช็ค accuracy ทันที
///   - ผ่านเกณฑ์เมื่อไหร่ -> Success ทันที
///   - ถ้าเลย missLine (เข้าโซนแดง) ยังไม่ผ่าน -> Miss
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class PoseCard : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("ไม่ต้องลาก — ระบบจะหา PoseSilhouetteBuilder จาก Scene อัตโนมัติตอน Spawn")]
    public Image cardBackground;

    [Header("สี")]
    public Color colorDefault = new Color(0.1f, 0.1f, 0.1f, 1f);
    public Color colorJudge   = new Color(0.2f, 0.6f, 1.0f, 1f);
    public Color colorSuccess = new Color(0.2f, 0.85f, 0.3f, 1f);
    public Color colorMiss    = new Color(0.85f, 0.2f, 0.2f, 1f);

    [HideInInspector] public DancePose dancePose;
    [HideInInspector] public float scrollSpeed;

    RhythmLaneUI         _laneUI;
    CanvasGroup          _canvasGroup;
    PoseSilhouetteBuilder _silhouetteBuilder;  // หาจาก Scene ตอน Init ไม่ใช่ลากจาก Inspector

    public enum CardState { Incoming, Judging, Success, Miss }
    public CardState State { get; private set; } = CardState.Incoming;

    bool _judgeStarted;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Init(DancePose pose, float speed, RhythmLaneUI lane)
    {
        dancePose   = pose;
        scrollSpeed = speed;
        _laneUI     = lane;

        // หา PoseSilhouetteBuilder จาก Scene อัตโนมัติ
        // (ติดอยู่บน SilhouetteCharacter ซึ่งเป็น GameObject ใน Scene ไม่ใช่ใน Prefab)
        if (_silhouetteBuilder == null)
            _silhouetteBuilder = FindObjectOfType<PoseSilhouetteBuilder>();

        if (_silhouetteBuilder != null && pose != null)
            _silhouetteBuilder.ApplyPose(pose);
        else if (_silhouetteBuilder == null)
            Debug.LogWarning("[PoseCard] หา PoseSilhouetteBuilder ใน Scene ไม่เจอ ตรวจสอบว่า SilhouetteCharacter มี component นี้ติดอยู่");

        SetColor(colorDefault);
    }

    void Update()
    {
        if (State == CardState.Success || State == CardState.Miss) return;

        transform.localPosition += Vector3.left * scrollSpeed * Time.deltaTime;
        float x = transform.localPosition.x;

        // เข้าโซนแดง (เลย judgeLine ไปแล้วและยังไม่ผ่าน) = Miss
        if (x <= _laneUI.MissLine)
        {
            State = CardState.Miss;
            _laneUI.OnCardMiss(this);
            return;
        }

        // เข้าโซนเขียว (judgeLine) = เริ่มเช็ค accuracy
        if (!_judgeStarted && x <= _laneUI.JudgeLine)
        {
            _judgeStarted = true;
            State = CardState.Judging;
            SetColor(colorJudge);
            _laneUI.OnCardEnterJudgeZone(this);
        }
    }

    /// <summary>เรียกจาก RhythmLaneUI เมื่อ accuracy ผ่านเกณฑ์ (ไม่มีระดับ แค่ผ่าน/ไม่ผ่าน)</summary>
    public void MarkSuccess()
    {
        State = CardState.Success;
        SetColor(colorSuccess);
        StartCoroutine(FadeAndDestroy(0.4f));
    }

    public void MarkMiss()
    {
        State = CardState.Miss;
        SetColor(colorMiss);
        StartCoroutine(FadeAndDestroy(0.4f));
    }

    IEnumerator FadeAndDestroy(float duration)
    {
        float elapsed = 0f;
        float startAlpha = _canvasGroup.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            yield return null;
        }

        Destroy(gameObject);
    }

    void SetColor(Color c)
    {
        if (cardBackground != null) cardBackground.color = c;
    }
}