using UnityEngine;

public class MenuManagement : MonoBehaviour
{
    public GameObject panelStartDesactivate;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.Instance.PlayMusic("Chunky_Monkey");  
    }

    private void Update()
    {
        // Verifica si hay alguna tecla presionada y que no sea un click del mouse
        if (Input.anyKeyDown && !IsMouseInput())
        {
            panelStartDesactivate.SetActive(false);
            enabled = false;
        }
    }
    private bool IsMouseInput()
    {
        return Input.GetMouseButtonDown(0) ||
               Input.GetMouseButtonDown(1) ||
               Input.GetMouseButtonDown(2);
    }
    public void clicSound()
    {
        AudioManager.Instance.PlayFX("start");
    }

    public void OpenOptions() 
    {
        GameManager.Instance.OpenOptionsMenu();
    }

    public void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance.audioMixer != null)
        {
            // Convierte el valor lineal del slider a logarítmico para el AudioMixer
            AudioManager.Instance.audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
            PlayerPrefs.SetFloat("MusicVolume", volume); // Guarda el valor
        }
    }

    public void SetFxVolume(float volume)
    {
        if (AudioManager.Instance.audioMixer != null)
        {
            // Convierte el valor lineal del slider a logarítmico para el AudioMixer
            AudioManager.Instance.audioMixer.SetFloat("Fx", Mathf.Log10(volume) * 20);
            PlayerPrefs.SetFloat("FxVolume", volume); // Guarda el valor
        }
    }

    public void SetGeneralVolume(float volume)
    {
        if (AudioManager.Instance.audioMixer != null)
        {
            // Convierte el valor lineal del slider a logarítmico para el AudioMixer
            AudioManager.Instance.audioMixer.SetFloat("Master", Mathf.Log10(volume) * 20);
            PlayerPrefs.SetFloat("GeneralVolume", volume); // Guarda el valor
        }
    }

    public void OpenCredits()
    {
        GameManager.Instance.OpenCreditsMenu();
    }

    public  void NextLevel()
    {
        GameManager.Instance.LoadNextScene();
    }

}
