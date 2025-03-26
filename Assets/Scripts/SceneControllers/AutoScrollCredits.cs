using UnityEngine;
using UnityEngine.UI;

public class AutoScrollCredits : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float scrollSpeed = 30f;

    void Update()
    {
        if (scrollRect.verticalNormalizedPosition > 0)
        {
            scrollRect.verticalNormalizedPosition -= scrollSpeed * Time.deltaTime;
        }
    }

    // Método para reiniciar el scroll al inicio
    public void RestartCredits()
    {
        scrollRect.verticalNormalizedPosition = 1f; // Reinicia al principio (parte superior)
    }
}