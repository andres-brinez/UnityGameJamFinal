using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class OptionMenu : MonoBehaviour
{
    public Slider fxSlider;
    public Slider musicSlider;
    public Slider generalSlider;

    [SerializeField] private Toggle muteToggle;         // Toggle para silenciar/desilenciar
    private float previousVolume;
    void Awake()
    {
        float sfxVolume = PlayerPrefs.GetFloat("FxVolume", 0f); // Valor predeterminado en 0 dB
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0f); // Valor predeterminado en 0 dB
        float generalVolume = PlayerPrefs.GetFloat("GeneralVolume", 0f); // Valor predeterminado en 0 dB   
        fxSlider.value = sfxVolume;
        musicSlider.value = musicVolume;
        generalSlider.value = generalVolume;

        SetFxVolume(sfxVolume);
        SetGeneralVolume(musicVolume);
        SetMusicVolume(musicVolume);
    }
    void Start()
    {
        // Asignar los métodos a los eventos de cambio de valor de los sliders
        fxSlider.onValueChanged.AddListener(SetFxVolume); 
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        generalSlider.onValueChanged.AddListener(SetGeneralVolume);

        muteToggle.onValueChanged.AddListener(OnMuteToggleChanged);
        InitializeToggleState();
    }

    void InitializeToggleState()
    {
        //muteToggle.isOn = false;
        float currentVolume;
        AudioManager.Instance.audioMixer.GetFloat("Master",out currentVolume);
        muteToggle.isOn = currentVolume == -80f;
    }
    void OnMuteToggleChanged(bool isMuted)
    {
        if (isMuted)
        {
            // Guarda el volumen actual antes de mutear
            AudioManager.Instance.audioMixer.GetFloat("Master", out previousVolume);

            // Silencia el volumen (establece el volumen a -80 dB)
            AudioManager.Instance.audioMixer.SetFloat("Master", -80f);
        }
        else
        {
            // Restaura el volumen anterior
            AudioManager.Instance.audioMixer.SetFloat("Master", previousVolume);
        }
    }
    public void SetFxVolume(float volume)
    {
        Debug.Log($"Setting FX Volume: {volume}");
        if (AudioManager.Instance.audioMixer != null)
        {
            // Asigna directamente el valor en dB al AudioMixer
            AudioManager.Instance.audioMixer.SetFloat("Fx", volume);
            PlayerPrefs.SetFloat("FxVolume", volume); // Guarda el valor
        }
    }

    // Método para ajustar el volumen de la música
    public void SetMusicVolume(float volume)
    {
        Debug.Log($"Setting Music Volume: {volume}");
        if (AudioManager.Instance.audioMixer != null)
        {
            // Asigna directamente el valor en dB al AudioMixer
            AudioManager.Instance.audioMixer.SetFloat("Music", volume);
            PlayerPrefs.SetFloat("MusicVolume", volume); // Guarda el valor
        }
    }
    // Método para ajustar el volumen del general
    public void SetGeneralVolume(float volume)
    {
        Debug.Log($"Setting General Volume: {volume}");
        if (AudioManager.Instance.audioMixer != null)
        {
            // Asigna directamente el valor en dB al AudioMixer
            AudioManager.Instance.audioMixer.SetFloat("Master", volume);
            PlayerPrefs.SetFloat("GeneralVolume", volume); // Guarda el valor
        }
    }
    public void clicSound()
    {
        AudioManager.Instance.PlayFX("start");
    }

    public void exitMenu()
    {
        PlayerPrefs.Save();
        SceneManager.UnloadSceneAsync("OptionMenu");
    }

}
