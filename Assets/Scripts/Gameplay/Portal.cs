using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private string musicClipName; 
    [SerializeField] private bool loopMusic = true; 
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LevelManager.instance.EnterPortal();

            // Reproducir la m�sica usando el AudioManager
            if (!string.IsNullOrEmpty(musicClipName))
            {
                AudioManager.Instance.PlayMusic(musicClipName, loopMusic);
            }
            else
            {
                Debug.LogWarning("No se ha asignado un nombre de clip de m�sica en el portal");
            }
        }
    }
}