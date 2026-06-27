using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// อ่าน BeatmapSO และ spawn PoseCard ตามเวลาที่กำหนด
/// sync กับ AudioSource โดยใช้ audioSource.time เป็น ground truth
/// ทำให้ตรงจังหวะแม้เกิด frame drop
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BeatmapPlayer : MonoBehaviour
{
    [Header("อ้างอิง")]
    public BeatmapSO beatmap;
    public RhythmLaneUI laneUI;       // แถบ UI ที่จะ spawn PoseCard เข้าไป

    [Header("ตัวสุ่มท่า (ใช้เมื่อ cue ไม่ได้กำหนด DancePose ไว้ล่วงหน้า)")]
    [Tooltip("ถ้า cue.dancePose เป็นค่าว่าง ระบบจะสุ่มท่าจากตัวนี้ให้อัตโนมัติตอน spawn")]
    public RandomPoseGenerator randomPoseGenerator;

    [Header("สถานะ (อ่านอย่างเดียว)")]
    public bool isPlaying;
    public float currentBeat;

    /// <summary>ยิงตอนเพลงเล่นจบ ส่งค่า true/false ว่าผู้เล่นผ่านด่านหรือไม่</summary>
    public event System.Action<bool> OnSongFinished;

    AudioSource _audio;
    int _nextCueIndex;
    float _spawnAheadTime;
    bool _started;
    bool _waitingForCard;   // รอให้การ์ดปัจจุบันหายก่อนค่อย spawn ใบถัดไป

    void Awake() => _audio = GetComponent<AudioSource>();

    void Start()
    {
        // เริ่มเล่นอัตโนมัติตอนเริ่มเกม (ถ้ามี beatmap ใส่ไว้)
        // ถอดออกได้ภายหลังถ้าอยากให้เรียกจาก MinigameTrigger แทน
        if (beatmap != null)
            Play(beatmap);
        else
            Debug.LogWarning("[BeatmapPlayer] ยังไม่ได้ใส่ beatmap ใน Inspector");
    }

    public void Play(BeatmapSO map)
    {
        if (map == null || map.audioClip == null)
        {
            Debug.LogError("[BeatmapPlayer] beatmap หรือ audioClip เป็น null");
            return;
        }

        beatmap           = map;
        _nextCueIndex     = 0;
        _started          = false;
        _waitingForCard   = false;
        isPlaying         = true;

        laneUI.ResetProgress(beatmap.cues.Length);

        // ดักฟัง event จาก RhythmLaneUI ว่าการ์ดปัจจุบันหายไปแล้ว
        laneUI.OnCardFinished -= OnCardFinished;
        laneUI.OnCardFinished += OnCardFinished;

        _spawnAheadTime = laneUI.GetSpawnToJudgeDistance() / beatmap.scrollSpeed;

        _audio.clip = beatmap.audioClip;
        _audio.time = 0f;
        Invoke(nameof(StartAudio), beatmap.leadInTime);

        Debug.Log($"[BeatmapPlayer] เริ่ม: {beatmap.songName} | BPM: {beatmap.bpm}");
    }

    void StartAudio()
    {
        _audio.Play();
        _started = true;
    }

    public void Stop()
    {
        isPlaying = false;
        _audio.Stop();
        CancelInvoke(nameof(StartAudio));
        if (laneUI != null) laneUI.OnCardFinished -= OnCardFinished;
    }

    void OnCardFinished()
    {
        // การ์ดใบปัจจุบันหายไปแล้ว พร้อม spawn ใบถัดไป
        _waitingForCard = false;
    }

    void Update()
    {
        if (!isPlaying || beatmap == null) return;

        float gameTime = _started
            ? _audio.time + beatmap.leadInTime
            : Mathf.Max(0f, Time.timeSinceLevelLoad);

        currentBeat = (gameTime / 60f) * beatmap.bpm;

        // Spawn การ์ดทีละใบ รอให้ใบก่อนหายก่อนค่อย spawn ใบถัดไป
        if (!_waitingForCard && _nextCueIndex < beatmap.cues.Length)
        {
            BeatmapCue cue     = beatmap.cues[_nextCueIndex];
            float      cueTime = beatmap.BeatToSeconds(cue.beat);
            float      spawnAt = cueTime - _spawnAheadTime;

            if (gameTime >= spawnAt + beatmap.leadInTime)
            {
                DancePose resolvedPose = cue.dancePose != null
                    ? cue.dancePose
                    : randomPoseGenerator != null
                        ? randomPoseGenerator.GenerateNewChallenge()
                        : null;

                if (resolvedPose == null)
                {
                    Debug.LogWarning($"[BeatmapPlayer] cue {_nextCueIndex} ไม่มี DancePose ข้าม");
                }
                else
                {
                    laneUI.SpawnPoseCard(cue, resolvedPose, beatmap.scrollSpeed);
                    _waitingForCard = true;   // รอการ์ดใบนี้หายก่อน
                }

                _nextCueIndex++;
            }
        }

        // จบเพลง
        if (_started && !_audio.isPlaying && _nextCueIndex >= beatmap.cues.Length && !_waitingForCard)
        {
            isPlaying = false;
            laneUI.OnCardFinished -= OnCardFinished;
            bool passed = laneUI != null && laneUI.IsStagePassed;
            Debug.Log($"[BeatmapPlayer] จบเพลง | ผ่านด่าน: {passed}");
            OnSongFinished?.Invoke(passed);
        }
    }
}