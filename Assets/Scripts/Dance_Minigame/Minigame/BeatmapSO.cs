using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// เก็บข้อมูล Beatmap ของแต่ละเพลง
/// สร้างได้จากเมนู: Assets > Create > Dance > Beatmap
/// </summary>
[CreateAssetMenu(fileName = "NewBeatmap", menuName = "Dance/Beatmap")]
public class BeatmapSO : ScriptableObject
{
    [Header("ข้อมูลเพลง")]
    public string songName;
    public AudioClip audioClip;

    [Tooltip("จังหวะต่อนาที")]
    public float bpm = 120f;

    [Tooltip("ความเร็วการเลื่อนของ PoseCard (หน่วย UI unit/วินาที)")]
    public float scrollSpeed = 200f;

    [Tooltip("เพลงเริ่มเล่นก่อนกี่วินาทีก่อน beat แรก (เพื่อให้มีเวลาเตรียมตัว)")]
    public float leadInTime = 2f;

    [Header("ท่าทั้งหมดในเพลงนี้")]
    public BeatmapCue[] cues;

    /// <summary>แปลง beat number → เวลา (วินาที) นับจากเริ่มเพลง</summary>
    public float BeatToSeconds(float beat)
    {
        return (beat / bpm) * 60f;
    }

    // ────────────────────────────────────────────────
    // Auto-generate: สุ่มจังหวะทั้งเพลงให้อัตโนมัติ
    // หน้าที่ของคนออกแบบเหลือแค่ "ทุกกี่ beat ให้มีท่าใหม่"
    // ส่วนตัวท่าเอง (dancePose) ปล่อยว่างไว้ ให้ RandomPoseGenerator
    // สุ่มให้ตอน spawn จริงอยู่แล้ว (ดู BeatmapPlayer.cs)
    // ────────────────────────────────────────────────

    [Header("Auto-generate Cues (Editor Tool)")]
    [Tooltip("เริ่มมีท่าแรกที่ beat ไหน (เผื่อ intro เพลงไม่อยากให้มีท่า)")]
    public float startBeat = 4f;

    [Tooltip("ทุกกี่ beat ให้มีท่าใหม่ 1 ท่า เช่น 4 = ทุก 1 ห้องเพลง (4/4)")]
    public float beatInterval = 4f;

    [Tooltip("ทิ้งช่วงท้ายเพลงไว้กี่ beat ไม่ต้องมีท่า (กันท่าสุดท้ายโผล่ตอนเพลงใกล้จบเกินไป)")]
    public float endBeatPadding = 4f;

#if UNITY_EDITOR
    [ContextMenu("สุ่มสร้างจังหวะทั้งเพลงอัตโนมัติ")]
    public void AutoGenerateCues()
    {
        if (audioClip == null)
        {
            Debug.LogWarning("[BeatmapSO] ใส่ audioClip ก่อนถึงจะคำนวณความยาวเพลงได้");
            return;
        }
        if (bpm <= 0f || beatInterval <= 0f)
        {
            Debug.LogWarning("[BeatmapSO] bpm และ beatInterval ต้องมากกว่า 0");
            return;
        }

        float totalBeats = (audioClip.length / 60f) * bpm;
        float lastUsableBeat = totalBeats - endBeatPadding;

        if (lastUsableBeat <= startBeat)
        {
            Debug.LogWarning("[BeatmapSO] เพลงสั้นเกินไป หรือ startBeat/endBeatPadding ตั้งไว้มากเกินไป");
            return;
        }

        int count = Mathf.FloorToInt((lastUsableBeat - startBeat) / beatInterval) + 1;
        cues = new BeatmapCue[count];

        for (int i = 0; i < count; i++)
        {
            cues[i] = new BeatmapCue
            {
                beat = startBeat + i * beatInterval,
                dancePose = null   // เว้นว่างไว้ -> สุ่มอัตโนมัติตอน spawn
            };
        }

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        Debug.Log($"[BeatmapSO] สร้าง {count} cue ทั่วทั้งเพลง (ความยาวเพลง ≈ {totalBeats:F1} beat, ห่างกัน {beatInterval} beat)");
    }

    [ContextMenu("ล้าง cues ทั้งหมด")]
    public void ClearCues()
    {
        cues = new BeatmapCue[0];
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        Debug.Log("[BeatmapSO] ล้าง cues แล้ว");
    }
#endif
}

/// <summary>
/// หนึ่ง cue = ท่าหนึ่งท่าที่จะให้ผู้เล่นทำ ที่ beat ที่กำหนด
/// PoseCard จะถึงเส้น judgeLine พอดีตอน beat ที่กำหนด
/// </summary>
[System.Serializable]
public class BeatmapCue
{
    [Tooltip("PoseCard จะถึงเส้นตัดสินที่ beat นี้")]
    public float beat;

    [Tooltip(
        "ท่าที่ต้องทำ (ไม่บังคับ — เว้นว่างไว้ได้)\n" +
        "ถ้าเว้นว่าง ระบบจะสุ่มท่าให้อัตโนมัติตอน spawn ผ่าน RandomPoseGenerator " +
        "ที่ลากใส่ไว้ใน BeatmapPlayer\n" +
        "ลาก DancePose มาใส่เฉพาะ cue ที่อยากกำหนดท่าตายตัวเอง (เช่น ท่าไหว้ตอนจบเพลง)")]
    public DancePose dancePose;
}