using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionScene : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip TransitionCanvasEnd;
    [SerializeField] private int sceneToLoad = 2;

    private void Start()
    {
        if (animator == null)
            Debug.LogError("No se encontró Animator en el GameObject.");
        if (TransitionCanvasEnd == null)
            Debug.LogWarning("No se asignó el clip de transición.");
    }

    public void StartSceneTransition()
    {
        Debug.Log("TRANSICIÓN INICIADA"); // ← este debería aparecer al presionar el botón
        StartCoroutine(ChangeScene());
    }

    IEnumerator ChangeScene()
    {
        animator.SetTrigger("Start");
        yield return new WaitForSeconds(TransitionCanvasEnd.length);
        SceneManager.LoadScene(sceneToLoad);
    }
}




