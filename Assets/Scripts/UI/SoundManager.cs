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
    private const string BGMVolumePrefKey = "BGMVolume";
    private const string SFXVolumePrefKey = "SFXVolume";
    private const string MutePrefKey = "GameMute";

    private float bgmVolume = 1f;
    private float sfxVolume = 1f;

    public float BGMVolume
    {
        get => bgmVolume;
        set
        {
            bgmVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(BGMVolumePrefKey, bgmVolume);
            PlayerPrefs.Save();
            UpdateBGMVolume();
        }
    }

    public float SFXVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SFXVolumePrefKey, sfxVolume);
            PlayerPrefs.Save();
        }
    }

    private void Start()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f; // 2D BGM

        UpdateBGMVolume();

        if (defaultBGM == null)
        {
            defaultBGM = Resources.Load<AudioClip>("Japanese Fantasy Music - Kanpai!");
        }

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
            if (_instance.defaultBGM == null && this.defaultBGM != null)
            {
                Debug.Log("SoundManager: Replacing empty/dummy instance with configured instance from scene.");
                Destroy(_instance.gameObject);
                _instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                LoadAndApplySettings();
                return;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        _instance = this;
        transform.SetParent(null);
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

        bgmVolume = PlayerPrefs.GetFloat(BGMVolumePrefKey, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFXVolumePrefKey, 1f);
        UpdateBGMVolume();
    }

    private void UpdateBGMVolume()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        PlayClip(clip, Vector3.zero, false, volumeScale);
    }

    public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip == null) return;
        PlayClip(clip, position, false, volumeScale);
    }

    private void PlayClip(AudioClip clip, Vector3 position, bool is3D, float volumeScale = 1f)
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
        
        source.volume = sfxVolume * volumeScale;
        source.Play();

        Destroy(go, clip.length);
    }
}
