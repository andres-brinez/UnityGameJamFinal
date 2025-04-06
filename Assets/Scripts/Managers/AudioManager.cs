using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource fxSource;
    private AudioSource musicSource;

    [SerializeField] public AudioMixer audioMixer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        // Inicializa las fuentes de audio
        fxSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        // Inicializamos el Mixer
        audioMixer = Resources.Load<AudioMixer>("AudioMaster");

        // Asignamos canales al audioMixer
        if (audioMixer != null)
        {
            // Asigna el canal "Fx" al fxSource
            fxSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Fx")[0];

            // Asigna el canal "Music" al musicSource
            musicSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Music")[0];
            
            // Asigna el canal "General" al musicSource
            //musicSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Master")[0];
        }
        else
        {
            Debug.LogWarning("El mixer de audio no se pudo cargar.");
        }
    }

    // Método para reproducir efectos de sonido
    public void PlayFX(string clipName)
    {
        string path = $"Audio/SFX/{clipName}"; // Ruta dentro de la carpeta Resources
        AudioClip clip = Resources.Load<AudioClip>(path); // Carga el archivo .wav
        if (clip != null)
        {
            fxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"No se encontró el archivo de audio: {path}");
        }
    }
    
    // Método para reproducir música
    public void PlayMusic(string clipName, bool loop = true)
    {
        string path = $"Audio/Music/{clipName}"; // Ruta dentro de la carpeta Resources
        AudioClip clip = Resources.Load<AudioClip>(path); // Carga el archivo .wav
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning($"No se encontró el archivo de audio: {path}");
        }
    }

    // Método para detener la música
    public void StopMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }


}
