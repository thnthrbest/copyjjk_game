using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BeatmapPlayer : MonoBehaviour
{
    [Header("อ้างอิง")]
    public BeatmapSO        beatmap;
    public RhythmLaneUI     laneUI;
    public RandomPoseGenerator randomPoseGenerator;

    [Header("สถานะ (อ่านอย่างเดียว)")]
    public bool  isPlaying;
    public float currentBeat;

    public event System.Action<bool> OnSongFinished;

    AudioSource _audio;
    int   _nextCueIndex;
    float _spawnAheadTime;
    bool  _started;
    float _gameTimer;   // นับตั้งแต่ Play() ถูกเรียก

    void Awake() => _audio = GetComponent<AudioSource>();

    void Start()
    {
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

        beatmap       = map;
        _nextCueIndex = 0;
        _started      = false;
        isPlaying     = true;
        _gameTimer    = 0f;

        laneUI.ResetProgress(beatmap.cues.Length);

        // คำนวณว่าการ์ดต้องใช้เวลากี่วินาทีเลื่อนจาก spawnX มาถึง judgeRight
        // นี่คือ leadInTime จริงๆ ที่ทำให้การ์ดถึงเส้นพอดีกับเพลงเริ่ม
        _spawnAheadTime = laneUI.GetSpawnToJudgeDistance() / beatmap.scrollSpeed;

        _audio.clip = beatmap.audioClip;
        _audio.time = 0f;

        // ใช้ leadInTime จาก BeatmapSO ที่ปรับให้ตรงจังหวะแล้ว
        Invoke(nameof(StartAudio), beatmap.leadInTime);

        Debug.Log($"[BeatmapPlayer] เริ่ม: {beatmap.songName} | BPM: {beatmap.bpm} | spawnAhead: {_spawnAheadTime:F2}s");
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
    }

    void Update()
    {
        if (!isPlaying || beatmap == null) return;

        _gameTimer += Time.deltaTime;

        // beat ปัจจุบัน (นับจากเพลงเริ่มจริง)
        if (_started)
            currentBeat = (_audio.time / 60f) * beatmap.bpm;

        while (_nextCueIndex < beatmap.cues.Length)
        {
            BeatmapCue cue     = beatmap.cues[_nextCueIndex];
            float      cueTime = beatmap.BeatToSeconds(cue.beat);

            // เวลาที่ต้อง spawn นับจาก Play()
            // cueTime คือเวลาที่การ์ดควรถึงเส้น judgeRight (ซึ่งตรงกับเวลาใน audio)
            // แต่เพลงเริ่มหลัง _spawnAheadTime วินาที ดังนั้น:
            // spawnAt = _spawnAheadTime + cueTime - _spawnAheadTime = cueTime
            // สรุปคือ spawn การ์ดตรงกับ cueTime ที่นับจาก Play() พอดี
            float spawnAt = cueTime;

            if (_gameTimer >= spawnAt)
            {
                DancePose resolvedPose;
                RandomPoseGenerator.LimbPart limb = default;
                float targetValue = 0f;

                if (cue.dancePose != null)
                {
                    resolvedPose = cue.dancePose;
                }
                else if (randomPoseGenerator != null)
                {
                    resolvedPose = randomPoseGenerator.GenerateNewChallenge();
                    limb         = randomPoseGenerator.CurrentLimb;
                    targetValue  = GetLimbValue(resolvedPose, limb);
                }
                else
                {
                    Debug.LogWarning($"[BeatmapPlayer] cue {_nextCueIndex} ไม่มี DancePose ข้าม");
                    _nextCueIndex++;
                    continue;
                }

                laneUI.SpawnPoseCard(cue, resolvedPose, beatmap.scrollSpeed, limb, targetValue);
                _nextCueIndex++;
            }
            else break;
        }

        // จบเพลง
        if (_started && !_audio.isPlaying && _nextCueIndex >= beatmap.cues.Length)
        {
            isPlaying = false;
            bool passed = laneUI != null && laneUI.IsStagePassed;
            Debug.Log($"[BeatmapPlayer] จบเพลง | ผ่านด่าน: {passed}");
            OnSongFinished?.Invoke(passed);
        }
    }

    float GetLimbValue(DancePose pose, RandomPoseGenerator.LimbPart limb)
    {
        return limb switch
        {
            RandomPoseGenerator.LimbPart.LeftArmLift   => pose.leftArmLift,
            RandomPoseGenerator.LimbPart.LeftArmSwing  => pose.leftArmSwing,
            RandomPoseGenerator.LimbPart.RightArmLift  => pose.rightArmLift,
            RandomPoseGenerator.LimbPart.RightArmSwing => pose.rightArmSwing,
            RandomPoseGenerator.LimbPart.LeftLegLift   => pose.leftLegLift,
            RandomPoseGenerator.LimbPart.LeftLegSwing  => pose.leftLegSwing,
            RandomPoseGenerator.LimbPart.RightLegLift  => pose.rightLegLift,
            RandomPoseGenerator.LimbPart.RightLegSwing => pose.rightLegSwing,
            _                                          => 0f
        };
    }
}