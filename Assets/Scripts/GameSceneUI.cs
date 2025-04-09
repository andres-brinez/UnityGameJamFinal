using UnityEngine;

public class GameSceneUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private GameObject winCanvas;
    [SerializeField] private GameObject pauseCanvas; // Opcional

    private void OnEnable()
    {
        // Suscribirse a todos los eventos del GameManager
        GameManager.OnGameOver += ShowGameOver;
        GameManager.OnGameWin += ShowWinScreen;
        GameManager.OnGamePaused += ShowPauseMenu;
        GameManager.OnGameResumed += HidePauseMenu;
        GameManager.OnGameStarted += OnGameStart;
    }

    private void OnDisable()
    {
        // Desuscribirse para evitar memory leaks
        GameManager.OnGameOver -= ShowGameOver;
        GameManager.OnGameWin -= ShowWinScreen;
        GameManager.OnGamePaused -= ShowPauseMenu;
        GameManager.OnGameResumed -= HidePauseMenu;
        GameManager.OnGameStarted -= OnGameStart;
    }

    private void OnGameStart()
    {
        // Inicializar UI al comenzar el juego
        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);
        if (winCanvas != null) winCanvas.SetActive(false);
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
    }

    private void ShowGameOver()
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);
        }
    }

    private void ShowWinScreen()
    {
        if (winCanvas != null)
        {
            winCanvas.SetActive(true);
        }
    }

    private void ShowPauseMenu()
    {
        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(true);
        }
    }

    private void HidePauseMenu()
    {
        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
        }
    }
}