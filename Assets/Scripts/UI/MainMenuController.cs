using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("The settings popup panel GameObject")]
    public GameObject settingsPopup;

    [Header("Volume Settings UI")]
    [Tooltip("Slider for volume control")]
    public Slider volumeSlider;
    
    [Tooltip("Toggle for mute control")]
    public Toggle muteToggle;

    [Tooltip("TextMeshPro text for volume percentage representation")]
    public TextMeshProUGUI volumeText;

    [Header("Separate Volume Settings UI")]
    [Tooltip("Slider for BGM volume control")]
    public Slider bgmSlider;

    [Tooltip("TextMeshPro text for BGM volume representation")]
    public TextMeshProUGUI bgmText;

    [Tooltip("Slider for SFX volume control")]
    public Slider sfxSlider;

    [Tooltip("TextMeshPro text for SFX volume representation")]
    public TextMeshProUGUI sfxText;

    private const string VolumePrefKey = "GameVolume";
    private const string BGMVolumePrefKey = "BGMVolume";
    private const string SFXVolumePrefKey = "SFXVolume";
    private const string MutePrefKey = "GameMute";

    private void Start()
    {
        // Deactivate settings popup at start
        if (settingsPopup != null)
        {
            settingsPopup.SetActive(false);
        }

        // Load and apply sound settings
        LoadSoundSettings();
        
        // Setup listeners
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        if (muteToggle != null)
        {
            muteToggle.onValueChanged.AddListener(SetMute);
        }
        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    /// <summary>
    /// Loads scene stage_1
    /// </summary>
    public void PlayGame()
    {
        Debug.Log("MainMenu: Loading stage_1 scene...");
        SceneManager.LoadScene("stage_1");
    }

    /// <summary>
    /// Loads scene tutorial
    /// </summary>
    public void OpenTutorial()
    {
        Debug.Log("MainMenu: Loading tutorial scene...");
        SceneManager.LoadScene("tutorial");
    }

    /// <summary>
    /// Opens the settings popup window
    /// </summary>
    public void OpenSettings()
    {
        if (settingsPopup != null)
        {
            settingsPopup.SetActive(true);
            LoadSoundSettings(); // Reload settings just in case
        }
        else
        {
            Debug.LogWarning("MainMenu: Settings Popup GameObject reference is missing!");
        }
    }

    /// <summary>
    /// Closes the settings popup window
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPopup != null)
        {
            settingsPopup.SetActive(false);
        }
    }

    /// <summary>
    /// Adjusts game volume
    /// </summary>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    public void SetVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        
        // If mute toggle is active, we don't apply immediately to AudioListener
        // but we still save the desired slider value.
        if (muteToggle != null && muteToggle.isOn)
        {
            AudioListener.volume = 0f;
        }
        else
        {
            AudioListener.volume = volume;
        }

        PlayerPrefs.SetFloat(VolumePrefKey, volume);
        PlayerPrefs.Save();

        UpdateVolumeText(volume);
    }

    /// <summary>
    /// Adjusts BGM volume separately
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.BGMVolume = volume;
        }
        UpdateBGMVolumeText(volume);
    }

    /// <summary>
    /// Adjusts SFX volume separately
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SFXVolume = volume;
        }
        UpdateSFXVolumeText(volume);
    }

    private void UpdateBGMVolumeText(float volume)
    {
        if (bgmText != null)
        {
            bgmText.text = Mathf.RoundToInt(volume * 100f) + "%";
        }
    }

    private void UpdateSFXVolumeText(float volume)
    {
        if (sfxText != null)
        {
            sfxText.text = Mathf.RoundToInt(volume * 100f) + "%";
        }
    }

    /// <summary>
    /// Toggles mute status of the game
    /// </summary>
    /// <param name="isMuted"></param>
    public void SetMute(bool isMuted)
    {
        if (isMuted)
        {
            AudioListener.volume = 0f;
        }
        else
        {
            float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
            AudioListener.volume = savedVolume;
            if (volumeSlider != null)
            {
                volumeSlider.value = savedVolume;
            }
        }

        PlayerPrefs.SetInt(MutePrefKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads saved volume & mute settings
    /// </summary>
    private void LoadSoundSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        bool savedMute = PlayerPrefs.GetInt(MutePrefKey, 0) == 1;

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
        }

        if (muteToggle != null)
        {
            muteToggle.isOn = savedMute;
        }

        // Apply settings
        if (savedMute)
        {
            AudioListener.volume = 0f;
        }
        else
        {
            AudioListener.volume = savedVolume;
        }

        UpdateVolumeText(savedVolume);

        // Load separate BGM & SFX volumes
        float savedBGMVolume = PlayerPrefs.GetFloat(BGMVolumePrefKey, 1f);
        float savedSFXVolume = PlayerPrefs.GetFloat(SFXVolumePrefKey, 1f);

        if (bgmSlider != null)
        {
            bgmSlider.value = savedBGMVolume;
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = savedSFXVolume;
        }

        UpdateBGMVolumeText(savedBGMVolume);
        UpdateSFXVolumeText(savedSFXVolume);
    }

    /// <summary>
    /// Updates the TMP volume text indicator
    /// </summary>
    private void UpdateVolumeText(float volume)
    {
        if (volumeText != null)
        {
            volumeText.text = Mathf.RoundToInt(volume * 100f) + "%";
        }
    }
}
