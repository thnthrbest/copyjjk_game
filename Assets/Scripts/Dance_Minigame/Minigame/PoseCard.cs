using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// การ์ดท่าเต้นที่เลื่อนจากขวาไปซ้ายในแถบจังหวะ
/// ตอน Spawn จะขอถ่ายภาพ Silhouette ของท่านั้นจาก SilhouetteSnapshot
/// แต่ละใบมี Sprite ของตัวเอง ทำให้มีหลายใบพร้อมกันในแถบได้
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class PoseCard : MonoBehaviour
{
    [Header("UI")]
    public Image  silhouetteImage;   // Raw Image แสดงภาพ Silhouette
    public Image  cardBackground;

    [Header("สี")]
    public Color colorDefault = new Color(0.1f, 0.1f, 0.1f, 1f);
    public Color colorJudge   = new Color(0.2f, 0.6f, 1.0f, 1f);
    public Color colorSuccess = new Color(0.2f, 0.85f, 0.3f, 1f);
    public Color colorMiss    = new Color(0.85f, 0.2f, 0.2f, 1f);

    [HideInInspector] public DancePose dancePose;
    [HideInInspector] public float     scrollSpeed;

    RhythmLaneUI _laneUI;
    CanvasGroup  _canvasGroup;
    bool         _judgeStarted;

    public enum CardState { Incoming, Judging, Success, Miss }
    public CardState State { get; private set; } = CardState.Incoming;

    void Awake() => _canvasGroup = GetComponent<CanvasGroup>();

    void OnDestroy()
    {
        // ล้าง Texture2D/Sprite ที่ SilhouetteSnapshot สร้างไว้ให้การ์ดใบนี้โดยเฉพาะ
        // ป้องกัน memory leak เพราะ Texture2D/Sprite เป็น Unity Object ที่ GC ไม่เก็บให้เอง
        if (silhouetteImage != null && silhouetteImage.sprite != null)
        {
            Texture2D tex = silhouetteImage.sprite.texture;
            Destroy(silhouetteImage.sprite);
            if (tex != null) Destroy(tex);
        }
    }

    /// <summary>
    /// เรียกตอน Spawn
    /// limb + targetValue ใช้สั่ง SilhouetteSnapshot ถ่ายภาพท่านั้นโดยเฉพาะ
    /// </summary>
    public void Init(
        DancePose pose,
        float speed,
        RhythmLaneUI lane,
        SilhouetteSnapshot snapshot,
        RandomPoseGenerator.LimbPart limb,
        float targetValue)
    {
        dancePose   = pose;
        scrollSpeed = speed;
        _laneUI     = lane;

        SetColor(colorDefault);

        // ถ่ายภาพท่านี้แล้วใส่เป็น Sprite ของการ์ดใบนี้
        if (snapshot != null)
        {
            snapshot.CaptureForCard(limb, targetValue, sprite =>
            {
                if (silhouetteImage != null)
                    silhouetteImage.sprite = sprite;
            });
        }
        else
        {
            Debug.LogWarning("[PoseCard] ไม่มี SilhouetteSnapshot");
        }
    }

    void Update()
    {
        if (State == CardState.Success || State == CardState.Miss) return;

        transform.localPosition += Vector3.left * scrollSpeed * Time.deltaTime;
        float x = transform.localPosition.x;

        if (x <= _laneUI.MissLine)
        {
            State = CardState.Miss;
            _laneUI.OnCardMiss(this);
            return;
        }

        if (!_judgeStarted && x <= _laneUI.JudgeZoneRight)
        {
            _judgeStarted = true;
            State         = CardState.Judging;
            SetColor(colorJudge);
            _laneUI.OnCardEnterJudgeZone(this);
        }
    }

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
        float elapsed = 0f, start = _canvasGroup.alpha;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(start, 0f, elapsed / duration);
            yield return null;
        }
        Destroy(gameObject);
    }

    void SetColor(Color c)
    {
        if (cardBackground != null) cardBackground.color = c;
    }
}