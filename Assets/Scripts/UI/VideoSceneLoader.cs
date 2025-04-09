using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoSceneLoader : MonoBehaviour
{
    public VideoPlayer videoPlayer; // Referencia al VideoPlayer
    public string sceneToLoad = "MainMenu"; // Nombre de la escena que quieres cargar

    void Start()
    {
        // Aseg�rate de que el VideoPlayer est� configurado correctamente
        videoPlayer.loopPointReached += OnVideoEnd; // Evento cuando el video llega al final
    }

    // M�todo que se ejecuta cuando el video ha terminado
    void OnVideoEnd(VideoPlayer vp)
    {
        // Cargar la escena
        SceneManager.LoadScene(sceneToLoad);
    }
}
