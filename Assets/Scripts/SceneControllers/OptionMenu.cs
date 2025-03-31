using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class OptionMenu : MonoBehaviour
{
    [Header("Audio Settings")]
    public Slider fxSlider;
    public Slider musicSlider;
    public Slider generalSlider;
    [SerializeField] private Toggle muteToggle;

    [Header("Display Settings")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    private float previousVolume;
    private Resolution[] resolutions;
    private bool isInitializing = true;
    public Animator animatorUI;

    void Awake()
    {
        // Cargar configuraciones de audio con valores por defecto (75%)
        float sfxVolume = PlayerPrefs.GetFloat("FxVolume", 0.75f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float generalVolume = PlayerPrefs.GetFloat("GeneralVolume", 0.75f);

        fxSlider.value = sfxVolume;
        musicSlider.value = musicVolume;
        generalSlider.value = generalVolume;

        // Aplicar valores iniciales
        SetFxVolume(sfxVolume, true);
        SetMusicVolume(musicVolume, true);
        SetGeneralVolume(generalVolume, true);

        // Configurar pantalla
        SetupResolutionDropdown();
        LoadDisplaySettings();
    }

    void Start()
    {
        isInitializing = false;

        // Configurar listeners de audio
        fxSlider.onValueChanged.AddListener((volume) => SetFxVolume(volume));
        musicSlider.onValueChanged.AddListener((volume) => SetMusicVolume(volume));
        generalSlider.onValueChanged.AddListener((volume) => SetGeneralVolume(volume));
        muteToggle.onValueChanged.AddListener(OnMuteToggleChanged);

        InitializeToggleState();

        // Configurar listeners de pantalla
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    #region Audio Settings
    void InitializeToggleState()
    {
        float currentVolume;
        AudioManager.Instance.audioMixer.GetFloat("Master", out currentVolume);
        muteToggle.isOn = currentVolume <= -80f;
    }

    void OnMuteToggleChanged(bool isMuted)
    {
        if (isMuted)
        {
            AudioManager.Instance.audioMixer.GetFloat("Master", out previousVolume);
            AudioManager.Instance.audioMixer.SetFloat("Master", -80f);
        }
        else
        {
            AudioManager.Instance.audioMixer.SetFloat("Master", previousVolume);
        }
        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetFxVolume(float volume)
    {
        SetFxVolume(volume, false);
    }

    private void SetFxVolume(float volume, bool initialLoad)
    {
        if (AudioManager.Instance?.audioMixer != null)
        {
            float dB = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
            AudioManager.Instance.audioMixer.SetFloat("Fx", dB);

            if (!initialLoad && !isInitializing)
            {
                PlayerPrefs.SetFloat("FxVolume", volume);
                PlayerPrefs.Save();
            }
        }
    }

    public void SetMusicVolume(float volume)
    {
        SetMusicVolume(volume, false);
    }

    private void SetMusicVolume(float volume, bool initialLoad)
    {
        if (AudioManager.Instance?.audioMixer != null)
        {
            float dB = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
            AudioManager.Instance.audioMixer.SetFloat("Music", dB);

            if (!initialLoad && !isInitializing)
            {
                PlayerPrefs.SetFloat("MusicVolume", volume);
                PlayerPrefs.Save();
            }
        }
    }

    public void SetGeneralVolume(float volume)
    {
        SetGeneralVolume(volume, false);
    }

    private void SetGeneralVolume(float volume, bool initialLoad)
    {
        if (AudioManager.Instance?.audioMixer != null)
        {
            float dB = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
            AudioManager.Instance.audioMixer.SetFloat("Master", dB);

            if (!initialLoad && !isInitializing)
            {
                PlayerPrefs.SetFloat("GeneralVolume", volume);
                PlayerPrefs.Save();
            }
        }
    }
    #endregion

    #region Display Settings
    void SetupResolutionDropdown()
    {
        resolutions = Screen.resolutions
            .Where(res => res.refreshRateRatio.Equals(Screen.currentResolution.refreshRateRatio))
            .Distinct()
            .ToArray();

        resolutionDropdown.ClearOptions();

        int currentResolutionIndex = 0;
        List<string> options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        resolutionDropdown.RefreshShownValue();
    }

    void LoadDisplaySettings()
    {
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
    #endregion

    public void AnimAudioUI()
    {
        animatorUI.SetTrigger("AudioEntry");
    }
    public void AnimControllsUI()
    {
        animatorUI.SetTrigger("ControllsEntry");
    }
    public void AnimDisplayUI()
    {
        animatorUI.SetTrigger("DisplayEntry");
    }

    public void BackToMenu()
    {

        SceneManager.LoadScene("MainMenu");
    }
    public void clicSound()
    {
        AudioManager.Instance?.PlayFX("start");
    }

    public void exitMenu()
    {
        PlayerPrefs.Save();
        SceneManager.UnloadSceneAsync("OptionMenu");
    }
}