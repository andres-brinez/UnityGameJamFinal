using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private bool isPaused = false;
    [SerializeField] public bool gameStarted { get; private set; } = false;
    [SerializeField] public bool gameWon { get; private set; } = false;
    [SerializeField] public bool isGameOver { get; private set; } = false;
    private string musicNameStartGame = "Mix Game";
    private string musicNameMenu = "Mix Pantalla de inicio";
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private GameObject winCanvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            Debug.Log("Game manager se intancio");
        }
        else
        {
            Destroy(this.gameObject);

        }
    }
    void Start()
    {
        AudioManager.Instance.PlayMusic(musicNameMenu);
        
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !SceneManager.GetSceneByName("OptionMenu").isLoaded)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PauseGame();
            OpenOptionsMenu();
        }

        if(gameWon)
        {
            Time.timeScale = 0f; // Pausa el juego al ganar
            winCanvas.SetActive(true); // Muestra el canvas de victoria
        }
    }
    public void StartGame()
    {
        gameStarted = true;
        AudioManager.Instance.PlayMusic(musicNameStartGame); // Cambia la música al iniciar el juego
        Debug.Log("El juego ha comenzado");

    }

    public void GameOver()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (isGameOver) return; // Evitar múltiples llamadas

        Debug.Log("Juego perdido");
        isGameOver = true;
        Time.timeScale = 0f;
        ShowGameOverMenu();
    }

    public void WinGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (gameWon) return; // Evita que se ejecute más de una vez

        gameWon = true;
        Debug.Log("¡Has ganado el juego!");

        Time.timeScale = 0;
        SceneManager.LoadScene("WinScreen", LoadSceneMode.Additive);
    }

    // nameScene: Nombre de la escena
    public void LoadSceneByName(string nameScene)
    {
        SceneManager.LoadScene(nameScene);
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Carga la siguiente escena por index
    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Verifica si hay una siguiente escena
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("No hay m�s escenas. �Has completado el juego!");
            ReloadCurrentScene(); // Recargar la primera escena
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
    }
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }
    public void OpenOptionsMenu()
    {
        SceneManager.LoadScene("OptionMenu", LoadSceneMode.Additive);
    }

    public void OpenCreditsMenu()
    {
        SceneManager.LoadScene("CreditsMenu", LoadSceneMode.Additive);
    }

    public void ShowGameOverMenu()
    {
        gameOverCanvas.SetActive(true);
        Time.timeScale = 0f;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu");
    }
}
/*Forma de utilizar funciones en otros scripts, llamar escenas por nombres
GameManager.instance.LoadSceneByName("Menu") */