using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ถ่ายภาพ SilhouetteCharacter หลังตั้งท่าเสร็จ 1 frame
/// แล้วแปลงเป็น Sprite ใส่ใน RawImage ของ PoseCard ใบนั้นทันที
/// ทำให้การ์ดแต่ละใบมีภาพท่าของตัวเองโดยใช้ SilhouetteCharacter แค่ตัวเดียว
/// </summary>
public class SilhouetteSnapshot : MonoBehaviour
{
    [Header("กล้องที่ถ่าย SilhouetteCharacter")]
    public Camera silhouetteCamera;

    [Header("RenderTexture ที่กล้องส่งภาพมา")]
    public RenderTexture renderTexture;

    [Header("PoseSilhouetteBuilder ของ SilhouetteCharacter")]
    public PoseSilhouetteBuilder silhouetteBuilder;

    // ────────────────────────────────────────────────
    // เรียกจาก RhythmLaneUI ตอน Spawn การ์ดใหม่
    // ────────────────────────────────────────────────

    /// <summary>
    /// ตั้งท่าให้ SilhouetteCharacter แล้วถ่ายภาพ 1 frame
    /// เมื่อได้ภาพแล้วจะเรียก onDone(sprite) เพื่อส่ง Sprite ไปให้ PoseCard
    /// </summary>
    public void CaptureForCard(
        RandomPoseGenerator.LimbPart limb,
        float targetValue,
        System.Action<Sprite> onDone)
    {
        StartCoroutine(CaptureRoutine(limb, targetValue, onDone));
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

        // 4. แปลงเป็น Sprite
        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f));

        // 5. ส่ง Sprite กลับไปให้ผู้เรียก (PoseCard หรือ RhythmLaneUI)
        onDone?.Invoke(sprite);
    }
}