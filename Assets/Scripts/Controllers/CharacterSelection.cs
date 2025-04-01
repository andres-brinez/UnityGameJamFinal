using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class CharacterVariation
{
    public string variationName;       // Nombre de la variación 
    public Material sharedMaterial;    // Material que se aplicará a todos los SkinnedMeshRenderers
    public Sprite variationIcon;       // Icono opcional para el UI
}

[System.Serializable]
public class CharacterData
{
    public GameObject characterObject; // Personaje en la escena (hombre/mujer)
    public string characterName;      // Nombre para mostrar en el UI
    public List<CharacterVariation> variations; // Lista de estilos/materiales
    public string enterAnimationTrigger = "Enter"; // Trigger de animación inicial
}

public class CharacterSelection : MonoBehaviour
{
    [Header("CHARACTER SETTINGS")]
    public List<CharacterData> characters;      // Lista de personajes disponibles
    public Transform previewPosition;          // Posición durante la selección
    public Transform spawnPosition;            // Posición al confirmar
    public float enterAnimationDuration = 1f;  // Duración de la animación inicial

    [Header("UI SETTINGS")]
    public TMP_Dropdown characterDropdown;     // Dropdown para elegir personaje
    public TMP_Dropdown variationDropdown;     // Dropdown para elegir variación
    public TextMeshProUGUI infoText;           // Muestra el personaje/variación seleccionada
    public Button confirmButton;               // Botón para confirmar selección

    private int currentCharIndex = 0;          // Índice del personaje actual
    private int currentVarIndex = 0;           // Índice de la variación actual
    private List<SkinnedMeshRenderer> characterRenderers = new List<SkinnedMeshRenderer>();
    private Animator currentAnimator;          // Animator del personaje actual

    void Start()
    {
        InitializeCharacters(); // Desactiva todos los personajes
        InitializeUI();        // Configura los dropdowns y botones
        StartCoroutine(PlayEnterAnimation()); // Animación al iniciar
    }

    // entrada de animacion Idle
    IEnumerator PlayEnterAnimation()
    {
        ShowSelectedCharacter(); // Activa el personaje seleccionado

        if (currentAnimator != null && !string.IsNullOrEmpty(characters[currentCharIndex].enterAnimationTrigger))
        {
            currentAnimator.SetTrigger(characters[currentCharIndex].enterAnimationTrigger); // Dispara animación
            yield return new WaitForSeconds(enterAnimationDuration); // Espera a que termine
        }
    }

    // configura e inicializa los personajes
    void InitializeCharacters()
    {
        foreach (var character in characters)
        {
            if (character.characterObject != null)
            {
                character.characterObject.SetActive(false); // Desactiva todos al inicio

                // Asegura que cada personaje tenga Animator
                var animator = character.characterObject.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = character.characterObject.AddComponent<Animator>();
                    Debug.LogWarning($"[!] Se añadió Animator a {character.characterName}");
                }
            }
        }
    }

    // confugra la UI
    void InitializeUI()
    {
        // Dropdown de Personajes
        characterDropdown.ClearOptions();
        List<string> charOptions = new List<string>();
        foreach (var character in characters)
        {
            charOptions.Add(character.characterName);
        }
        characterDropdown.AddOptions(charOptions);
        characterDropdown.onValueChanged.AddListener(OnCharacterSelected);

        // Dropdown de Variaciones
        UpdateVariationsDropdown();
        variationDropdown.onValueChanged.AddListener(OnVariationSelected);

        // Botón de Confirmación
        confirmButton.onClick.AddListener(ConfirmSelection);
    }

    // cambiar personaje
    void OnCharacterSelected(int index)
    {
        // Oculta el personaje actual
        if (characters[currentCharIndex].characterObject != null)
        {
            characters[currentCharIndex].characterObject.SetActive(false);
        }

        // Actualiza el índice y muestra el nuevo personaje
        currentCharIndex = index;
        currentVarIndex = 0;
        ShowSelectedCharacter();
        UpdateVariationsDropdown();
    }

    // muestra el personaje seleccionado
    void ShowSelectedCharacter()
    {
        if (characters[currentCharIndex].characterObject != null)
        {
            GameObject selectedChar = characters[currentCharIndex].characterObject;
            selectedChar.SetActive(true);
            selectedChar.transform.position = previewPosition.position;
            selectedChar.transform.rotation = previewPosition.rotation;

            // Obtiene los SkinnedMeshRenderers para cambiar materiales
            characterRenderers = new List<SkinnedMeshRenderer>(selectedChar.GetComponentsInChildren<SkinnedMeshRenderer>());

            // Guarda el Animator para las animaciones
            currentAnimator = selectedChar.GetComponent<Animator>();

            ApplyCurrentVariation(); // Aplica el material seleccionado
        }
    }

    // actualiza Dropdown de variaciones
    void UpdateVariationsDropdown()
    {
        variationDropdown.ClearOptions();

        if (characters.Count == 0 || currentCharIndex >= characters.Count) return;

        List<string> varOptions = new List<string>();
        foreach (var variation in characters[currentCharIndex].variations)
        {
            varOptions.Add(variation.variationName);
        }

        variationDropdown.AddOptions(varOptions);
        variationDropdown.value = currentVarIndex;
    }

    // cambio de variacion
    void OnVariationSelected(int index)
    {
        currentVarIndex = index;
        ApplyCurrentVariation();
    }

    // aplica material (variacion) seleccionada
    void ApplyCurrentVariation()
    {
        if (characterRenderers == null || characterRenderers.Count == 0) return;
        if (characters[currentCharIndex].variations.Count == 0) return;

        Material materialToApply = characters[currentCharIndex].variations[currentVarIndex].sharedMaterial;

        foreach (var renderer in characterRenderers)
        {
            if (renderer != null)
            {
                renderer.material = materialToApply;
            }
        }

        UpdateUIInfo(); // Actualiza el texto en pantalla
    }

    // actualiza texto de seleccion
    void UpdateUIInfo()
    {
        infoText.text = $"<b>{characters[currentCharIndex].characterName}</b>\n" +
                      $"<color=#FFD700>{characters[currentCharIndex].variations[currentVarIndex].variationName}</color>";
    }

    // confirmar personaje para el spawn del mapa
    void ConfirmSelection()
    {
        if (characters[currentCharIndex].characterObject != null)
        {
            characters[currentCharIndex].characterObject.transform.position = spawnPosition.position;
            characters[currentCharIndex].characterObject.transform.rotation = spawnPosition.rotation;
        }
    }

    // Rota el personaje en la preview
    void Update()
    {
        if (characters.Count > currentCharIndex &&
            characters[currentCharIndex].characterObject != null &&
            characters[currentCharIndex].characterObject.activeSelf)
        {
            characters[currentCharIndex].characterObject.transform.Rotate(0, 20 * Time.deltaTime, 0);
        }
    }
}