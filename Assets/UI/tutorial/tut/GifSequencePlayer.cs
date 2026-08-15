using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plays multiple .gif clips back-to-back, in order, then loops the whole
/// sequence forever (or a fixed number of times). Useful for tutorial steps
/// that show e.g. "move-left.gif" then "move-right.gif" repeating.
///
/// Setup: drag each .gif.bytes TextAsset into "Gif Clips" in order.
/// Attach to a GameObject that also has an Image or Raw Image.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class GifSequencePlayer : MonoBehaviour
{
    [Header("Source (in playback order)")]
    [Tooltip("Drag .gif.bytes TextAssets here in the order they should play.")]
    public List<TextAsset> gifClips = new List<TextAsset>();

    [Header("Playback")]
    public bool playOnAwake = true;
    [Tooltip("How many times to play the full sequence. 0 = loop forever.")]
    public int loopCount = 0;
    [Tooltip("Multiplier on each GIF's own per-frame delay. 1 = original speed.")]
    public float playbackSpeed = 1f;
    [Tooltip("Optional pause between clips, in seconds.")]
    public float gapBetweenClips = 0f;

    [Header("Target (auto-detected if left empty)")]
    public Image targetImage;
    public RawImage targetRawImage;

    private List<GifDecoder.GifData> _clips = new List<GifDecoder.GifData>();
    private Coroutine _playRoutine;
    private bool _isLoaded;

    public bool IsPlaying { get; private set; }
    public int CurrentClipIndex { get; private set; }

    private void Awake()
    {
        if (targetImage == null) targetImage = GetComponent<Image>();
        if (targetRawImage == null) targetRawImage = GetComponent<RawImage>();
    }

    private void Start()
    {
        if (playOnAwake)
        {
            Load();
            Play();
        }
    }

    /// <summary>Decodes all configured clips into memory. Safe to call multiple times.</summary>
    public void Load()
    {
        if (_isLoaded) return;

        if (gifClips == null || gifClips.Count == 0)
        {
            Debug.LogError("[GifSequencePlayer] No gif clips assigned.", this);
            return;
        }

        _clips.Clear();
        foreach (var asset in gifClips)
        {
            if (asset == null)
            {
                Debug.LogError("[GifSequencePlayer] A gif clip slot is empty.", this);
                continue;
            }

            var data = GifDecoder.Decode(asset.bytes);
            if (data == null || data.frames.Count == 0)
            {
                Debug.LogError($"[GifSequencePlayer] Failed to decode gif clip: {asset.name}", this);
                continue;
            }
            _clips.Add(data);
        }

        _isLoaded = _clips.Count > 0;
        if (!_isLoaded)
        {
            Debug.LogError("[GifSequencePlayer] No clips decoded successfully.", this);
        }
    }

    public void Play()
    {
        if (!_isLoaded) Load();
        if (!_isLoaded) return;

        if (_playRoutine != null) StopCoroutine(_playRoutine);
        _playRoutine = StartCoroutine(PlaySequenceRoutine());
    }

    public void Stop()
    {
        if (_playRoutine != null)
        {
            StopCoroutine(_playRoutine);
            _playRoutine = null;
        }
        IsPlaying = false;
    }

    /// <summary>Restarts the whole sequence from the first clip, first frame.</summary>
    public void Restart()
    {
        CurrentClipIndex = 0;
        Play();
    }

    private IEnumerator PlaySequenceRoutine()
    {
        IsPlaying = true;
        int loopsDone = 0;

        do
        {
            for (int clipIndex = 0; clipIndex < _clips.Count; clipIndex++)
            {
                CurrentClipIndex = clipIndex;
                var clip = _clips[clipIndex];

                for (int f = 0; f < clip.frames.Count; f++)
                {
                    var frame = clip.frames[f];
                    ApplyFrame(frame.texture);

                    float wait = frame.delaySeconds / Mathf.Max(playbackSpeed, 0.01f);
                    yield return new WaitForSecondsRealtime(wait);
                }

                if (gapBetweenClips > 0f)
                {
                    yield return new WaitForSecondsRealtime(gapBetweenClips);
                }
            }

            loopsDone++;
        } while (loopCount <= 0 || loopsDone < loopCount);

        IsPlaying = false;
    }

    private void ApplyFrame(Texture2D tex)
    {
        if (targetImage != null)
        {
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            targetImage.sprite = sprite;
        }
        else if (targetRawImage != null)
        {
            targetRawImage.texture = tex;
        }
        else
        {
            Debug.LogWarning("[GifSequencePlayer] No Image or RawImage target assigned/found.", this);
        }
    }

    private void OnDestroy()
    {
        foreach (var clip in _clips)
        {
            foreach (var f in clip.frames)
            {
                if (f.texture != null) Destroy(f.texture);
            }
        }
    }
}