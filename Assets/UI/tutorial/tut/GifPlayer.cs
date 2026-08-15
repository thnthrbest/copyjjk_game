using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to a GameObject with an Image or RawImage component.
/// Decodes a .gif from StreamingAssets (or any absolute path) once,
/// caches the frames, then loops playback. No sprite sheet slicing needed.
///
/// Usage in Tutorial Mode: put one of these per gesture step
/// (movement.gif, jump.gif, shoot.gif, skill.gif), point gifFileName
/// at the file, and it autoplays when the step becomes active.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class GifPlayer : MonoBehaviour
{
    [Header("Source")]
    [Tooltip("Drag the .gif file directly here (imported as a TextAsset). Takes priority over the path fields below.")]
    public TextAsset gifAsset;

    [Tooltip("File name inside StreamingAssets, e.g. \"Tutorial/move.gif\". Used only if Gif Asset is empty.")]
    public string gifFileName;

    [Tooltip("Optional absolute path override. Used only if Gif Asset and Gif File Name are both empty.")]
    public string gifFullPath;

    [Header("Playback")]
    public bool playOnAwake = true;
    public bool loop = true;
    [Tooltip("Multiplier on the GIF's own per-frame delay. 1 = original speed.")]
    public float playbackSpeed = 1f;

    [Header("Target (auto-detected if left empty)")]
    public Image targetImage;
    public RawImage targetRawImage;

    private GifDecoder.GifData _gifData;
    private Coroutine _playRoutine;
    private int _currentFrame;
    private bool _isLoaded;

    public bool IsPlaying { get; private set; }

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

    /// <summary>Decodes the GIF file into memory. Safe to call multiple times (no-op after first).</summary>
    public void Load()
    {
        if (_isLoaded) return;

        // Priority 1: dragged-in TextAsset (bytes already in memory, no file path needed)
        if (gifAsset != null)
        {
            _gifData = GifDecoder.Decode(gifAsset.bytes);
            _isLoaded = _gifData != null && _gifData.frames.Count > 0;

            if (!_isLoaded)
            {
                Debug.LogError($"[GifPlayer] Failed to decode GIF asset: {gifAsset.name}", this);
            }
            return;
        }

        // Priority 2: file path (StreamingAssets or absolute)
        string path = ResolvePath();
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("[GifPlayer] No gif source configured (assign Gif Asset or a path).", this);
            return;
        }

        _gifData = GifDecoder.Decode(path);
        _isLoaded = _gifData != null && _gifData.frames.Count > 0;

        if (!_isLoaded)
        {
            Debug.LogError($"[GifPlayer] Failed to decode GIF at: {path}", this);
        }
    }

    private string ResolvePath()
    {
        if (!string.IsNullOrEmpty(gifFullPath)) return gifFullPath;
        if (!string.IsNullOrEmpty(gifFileName))
        {
            return Path.Combine(Application.streamingAssetsPath, gifFileName);
        }
        return null;
    }

    public void Play()
    {
        if (!_isLoaded) Load();
        if (!_isLoaded) return;

        if (_playRoutine != null) StopCoroutine(_playRoutine);
        _playRoutine = StartCoroutine(PlayRoutine());
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

    /// <summary>Restarts playback from frame 0. Useful when re-entering a tutorial step.</summary>
    public void Restart()
    {
        _currentFrame = 0;
        Play();
    }

    private IEnumerator PlayRoutine()
    {
        IsPlaying = true;
        _currentFrame = 0;

        do
        {
            for (int i = 0; i < _gifData.frames.Count; i++)
            {
                _currentFrame = i;
                var frame = _gifData.frames[i];
                ApplyFrame(frame.texture);

                float wait = frame.delaySeconds / Mathf.Max(playbackSpeed, 0.01f);
                yield return new WaitForSecondsRealtime(wait);
            }
        } while (loop);

        IsPlaying = false;
    }

    private void ApplyFrame(Texture2D tex)
    {
        if (targetImage != null)
        {
            // Image needs a Sprite; reuse a single Sprite object and swap its texture reference
            // via Sprite.Create per-frame (cheap since textures are already decoded/cached).
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            targetImage.sprite = sprite;
        }
        else if (targetRawImage != null)
        {
            targetRawImage.texture = tex;
        }
        else
        {
            Debug.LogWarning("[GifPlayer] No Image or RawImage target assigned/found.", this);
        }
    }

    /// <summary>Total playback duration of one loop, in seconds. Useful for syncing tutorial timing.</summary>
    public float GetLoopDuration()
    {
        if (!_isLoaded) return 0f;
        float total = 0f;
        foreach (var f in _gifData.frames) total += f.delaySeconds;
        return total / Mathf.Max(playbackSpeed, 0.01f);
    }

    private void OnDestroy()
    {
        // Clean up decoded textures to avoid leaking memory across scene reloads.
        if (_gifData != null)
        {
            foreach (var f in _gifData.frames)
            {
                if (f.texture != null) Destroy(f.texture);
            }
        }
    }
}