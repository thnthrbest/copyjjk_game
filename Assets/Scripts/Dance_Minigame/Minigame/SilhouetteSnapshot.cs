using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ถ่ายภาพ SilhouetteCharacter หลังตั้งท่าเสร็จ 1 frame
/// แล้วแปลงเป็น Sprite ใส่ใน RawImage ของ PoseCard ใบนั้นทันที
/// ทำให้การ์ดแต่ละใบมีภาพท่าของตัวเองโดยใช้ SilhouetteCharacter แค่ตัวเดียว
///
/// สำคัญ: ใช้ระบบคิว ถ่ายภาพทีละคำขอเท่านั้น
/// เพราะ SilhouetteCharacter มีตัวเดียว ถ้าหลายการ์ด spawn พร้อมกัน
/// แล้วยิง coroutine ถ่ายภาพซ้อนกัน จะตั้งท่าทับกันจนได้ภาพเดียวกันผิดๆ
/// </summary>
public class SilhouetteSnapshot : MonoBehaviour
{
    [Header("กล้องที่ถ่าย SilhouetteCharacter")]
    public Camera silhouetteCamera;

    [Header("RenderTexture ที่กล้องส่งภาพมา")]
    public RenderTexture renderTexture;

    [Header("PoseSilhouetteBuilder ของ SilhouetteCharacter")]
    public PoseSilhouetteBuilder silhouetteBuilder;

    struct CaptureRequest
    {
        public RandomPoseGenerator.LimbPart limb;
        public float targetValue;
        public System.Action<Sprite> onDone;
    }

    readonly Queue<CaptureRequest> _queue = new Queue<CaptureRequest>();
    bool _isProcessing = false;

    // ────────────────────────────────────────────────
    // เรียกจาก RhythmLaneUI ตอน Spawn การ์ดใหม่
    // ────────────────────────────────────────────────

    /// <summary>
    /// ขอถ่ายภาพท่าใหม่ — ถ้ามีคำขออื่นกำลังถ่ายอยู่ จะเข้าคิวรอ
    /// ไม่ทำพร้อมกันเด็ดขาด เพื่อกันท่าปนกัน
    /// </summary>
    public void CaptureForCard(
        RandomPoseGenerator.LimbPart limb,
        float targetValue,
        System.Action<Sprite> onDone)
    {
        _queue.Enqueue(new CaptureRequest
        {
            limb        = limb,
            targetValue = targetValue,
            onDone      = onDone
        });

        if (!_isProcessing)
            StartCoroutine(ProcessQueue());
    }

    IEnumerator ProcessQueue()
    {
        _isProcessing = true;

        while (_queue.Count > 0)
        {
            CaptureRequest req = _queue.Dequeue();
            yield return CaptureRoutine(req.limb, req.targetValue, req.onDone);
        }

        _isProcessing = false;
    }

    IEnumerator CaptureRoutine(
        RandomPoseGenerator.LimbPart limb,
        float targetValue,
        System.Action<Sprite> onDone)
    {
        // 1. ตั้งท่าให้ SilhouetteCharacter
        if (silhouetteBuilder != null)
        {
            silhouetteBuilder.ResetHighlight();
            silhouetteBuilder.ResetPose();
            silhouetteBuilder.ApplyPartialPose(limb, targetValue);
        }

        // 2. รอ 1 frame ให้ Unity render เสร็จก่อนถ่ายภาพ
        yield return new WaitForEndOfFrame();

        // 3. อ่านภาพจาก RenderTexture
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D tex = new Texture2D(
            renderTexture.width,
            renderTexture.height,
            TextureFormat.RGBA32,
            false);
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();

        RenderTexture.active = prev;

        // 4. แปลงเป็น Sprite (ภาพนี้เป็นของการ์ดใบนี้โดยเฉพาะ ไม่ถูกแก้ไขซ้ำอีก)
        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f));

        // 5. ส่ง Sprite กลับไปให้ผู้เรียก (PoseCard หรือ RhythmLaneUI)
        onDone?.Invoke(sprite);
    }
}