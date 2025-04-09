using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    // Singleton pattern
    public static GameManager Instance { get; private set; }

    // Eventos estáticos
    public static event Action OnGameOver;
    public static event Action OnGameWin;
    public static event Action OnGamePaused;
    public static event Action OnGameResumed;
    public static event Action OnGameStarted;

    // Estados del juego
    private bool isPaused = false;
    [SerializeField] public bool gameStarted { get; private set; } = false;
    [SerializeField] public bool gameWon { get; private set; } = false;
    [SerializeField] public bool isGameOver { get; private set; } = false;

    // Música
    private string musicNameStartGame = "Mix Game";
    private string musicNameMenu = "Mix Pantalla de inicio";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            Debug.Log("GameManager instanciado");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reiniciar estados del juego cuando se carga una escena de juego
        if (!IsMenuScene(scene.name))
        {
            ResetGameStates();
        }

        // Configuración específica para el menú principal
        if (scene.name == "MainMenu")
        {
            SetupMainMenu();
        }
    }

    private bool IsMenuScene(string sceneName)
    {
        return sceneName == "OptionMenu" || sceneName == "CreditsMenu" || sceneName == "MainMenu";
    }

    private void SetupMainMenu()
    {
        ResetGameStates();
        gameStarted = false;
        AudioManager.Instance.PlayMusic(musicNameMenu);
    }

    private void ResetGameStates()
    {
        isGameOver = false;
        gameWon = false;
        isPaused = false;
        Time.timeScale = 1f;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        AudioManager.Instance.PlayMusic(musicNameMenu);
    }

    void Update()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (Input.GetKeyDown(KeyCode.Escape) && !IsMenuScene(SceneManager.GetActiveScene().name))
        {
            TogglePause();
        }
    }

    public void StartGame()
    {
        // Limpiar suscriptores de eventos para evitar múltiples llamadas
        ClearEventSubscribers();

        ResetGameStates();
        gameStarted = true;
        AudioManager.Instance.PlayMusic(musicNameStartGame);
        OnGameStarted?.Invoke();
        Debug.Log("El juego ha comenzado");
    }

    private void ClearEventSubscribers()
    {
        // Esto limpia todos los suscriptores de los eventos
        OnGameOver = null;
        OnGameWin = null;
        OnGamePaused = null;
        OnGameResumed = null;
        OnGameStarted = null;
    }

    public void GameOver()
    {
        if (isGameOver) return;

        Debug.Log("Juego perdido");
        isGameOver = true;
        Time.timeScale = 0f;
        OnGameOver?.Invoke();
    }

    public void WinGame()
    {
        if (gameWon) return;

        Debug.Log("¡Has ganado el juego!");
        gameWon = true;
        Time.timeScale = 0f;
        OnGameWin?.Invoke();
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        OnGamePaused?.Invoke();
        OpenOptionsMenu();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        OnGameResumed?.Invoke();
    }

    public void LoadSceneByName(string nameScene)
    {
        SceneManager.LoadScene(nameScene);
    }

    public void ReloadCurrentScene()
    {
        ResetGameStates();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("No hay más escenas. ¡Has completado el juego!");
            ReloadCurrentScene();
        }
    }

    public void OpenOptionsMenu()
    {
        SceneManager.LoadScene("OptionMenu", LoadSceneMode.Additive);
    }

    public void OpenCreditsMenu()
    {
        SceneManager.LoadScene("CreditsMenu", LoadSceneMode.Additive);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}