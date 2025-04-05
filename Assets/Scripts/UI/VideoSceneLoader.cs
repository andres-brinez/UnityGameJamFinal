using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoSceneLoader : MonoBehaviour
{
    public VideoPlayer videoPlayer; // Referencia al VideoPlayer
    public string sceneToLoad = "MainMenu"; // Nombre de la escena que quieres cargar

    void Start()
    {
        // Asegúrate de que el VideoPlayer esté configurado correctamente
        videoPlayer.loopPointReached += OnVideoEnd; // Evento cuando el video llega al final
    }

    // Método que se ejecuta cuando el video ha terminado
    void OnVideoEnd(VideoPlayer vp)
    {
        // Cargar la escena
        SceneManager.LoadScene(sceneToLoad);
    }
}
