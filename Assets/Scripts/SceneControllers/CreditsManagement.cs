using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsManagement : MonoBehaviour
{
    public void exitCredits()
    {
        PlayerPrefs.Save();
        SceneManager.UnloadSceneAsync("CreditsMenu");
    }
    public void clicSound()
    {
        AudioManager.Instance.PlayFX("start");
    }
}
