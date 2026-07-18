using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;

    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SoundManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SoundManager");
                    _instance = go.AddComponent<SoundManager>();
                }
            }
            return _instance;
        }
    }

    [Header("BGM Settings")]
    public AudioClip defaultBGM;
    private AudioSource bgmSource;

    private const string VolumePrefKey = "GameVolume";
    private const string MutePrefKey = "GameMute";

    private void Start()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f; // 2D BGM

        if (defaultBGM != null)
        {
            PlayBGM(defaultBGM);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null) return;

        if (clip == null)
        {
            bgmSource.Stop();
            return;
        }

        if (bgmSource.isPlaying && bgmSource.clip == clip)
        {
            return;
        }

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAndApplySettings();
    }

    public void LoadAndApplySettings()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        bool savedMute = PlayerPrefs.GetInt(MutePrefKey, 0) == 1;

        if (savedMute)
        {
            AudioListener.volume = 0f;
        }
        else
        {
            AudioListener.volume = savedVolume;
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        PlayClip(clip, Vector3.zero, false);
    }

    public void PlaySFXAtPoint(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;
        PlayClip(clip, position, true);
    }

    private void PlayClip(AudioClip clip, Vector3 position, bool is3D)
    {
        GameObject go = new GameObject("TempSFX_" + clip.name);
        if (is3D)
        {
            go.transform.position = position;
        }
        
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = is3D ? 1f : 0f;
        source.playOnAwake = false;
        
        // AudioListener.volume controls the global volume, so keeping this at 1.0f is standard.
        source.volume = 1f;
        source.Play();

        Destroy(go, clip.length);
    }
}
