using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class CharacterVariation
{
    public string variationName;       // Nombre de la variación 
    public Material sharedMaterial;   // Material que se aplicará
    public Sprite variationIcon;      // Icono opcional para el UI
}

[System.Serializable]
public class CharacterData
{
    public GameObject characterObject; // Personaje en la escena
    public string characterName;      // Nombre para mostrar en el UI
    public List<CharacterVariation> variations; // Lista de estilos/materiales
    public string enterAnimationTrigger = "Enter"; // Trigger de animación inicial
    public string idleAnimationState = "Idle";    // Nombre del estado de animación Idle
    public Image avatarImage;         // Imagen del avatar para mostrar en UI al confirmar
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
    private bool hasConfirmedSelection = false; // Evita que siga animando después de confirmar

    void Start()
    {
        InitializeCharacters(); // Desactiva todos los personajes
        InitializeUI();        // Configura los dropdowns y botones
        StartCoroutine(PlayEnterAnimation()); // Animación al iniciar
    }

    // Animación de entrada + Idle continuo
    IEnumerator PlayEnterAnimation()
    {
        ShowSelectedCharacter(); // Activa el personaje seleccionado

        if (currentAnimator != null)
        {
            // 1. Reproduce animación de entrada
            if (!string.IsNullOrEmpty(characters[currentCharIndex].enterAnimationTrigger))
            {
                currentAnimator.SetTrigger(characters[currentCharIndex].enterAnimationTrigger);
                yield return new WaitForSeconds(enterAnimationDuration);
            }

            // 2. Entra en bucle Idle hasta que se confirme
            if (!string.IsNullOrEmpty(characters[currentCharIndex].idleAnimationState))
            {
                currentAnimator.Play(characters[currentCharIndex].idleAnimationState);
            }
        }
    }

    void InitializeCharacters()
    {
        foreach (var character in characters)
        {
            if (character.characterObject != null)
            {
                character.characterObject.SetActive(false);

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

    void InitializeUI()
    {
        characterDropdown.ClearOptions();
        List<string> charOptions = new List<string>();
        foreach (var character in characters)
        {
            charOptions.Add(character.characterName);
        }
        characterDropdown.AddOptions(charOptions);
        characterDropdown.onValueChanged.AddListener(OnCharacterSelected);

        UpdateVariationsDropdown();
        variationDropdown.onValueChanged.AddListener(OnVariationSelected);

        confirmButton.onClick.AddListener(ConfirmSelection);
    }

    void OnCharacterSelected(int index)
    {
        if (characters[currentCharIndex].characterObject != null)
        {
            characters[currentCharIndex].characterObject.SetActive(false);
        }

        currentCharIndex = index;
        currentVarIndex = 0;
        ShowSelectedCharacter();
        UpdateVariationsDropdown();

        // Reinicia la animación Idle para el nuevo personaje
        if (!hasConfirmedSelection)
        {
            StartCoroutine(PlayEnterAnimation());
        }
    }

    void ShowSelectedCharacter()
    {
        if (characters[currentCharIndex].characterObject != null)
        {
            GameObject selectedChar = characters[currentCharIndex].characterObject;
            selectedChar.SetActive(true);
            selectedChar.transform.position = previewPosition.position;
            selectedChar.transform.rotation = previewPosition.rotation;

            characterRenderers = new List<SkinnedMeshRenderer>(selectedChar.GetComponentsInChildren<SkinnedMeshRenderer>());
            currentAnimator = selectedChar.GetComponent<Animator>();

            ApplyCurrentVariation();
        }
    }

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

    void OnVariationSelected(int index)
    {
        currentVarIndex = index;
        ApplyCurrentVariation();
    }

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

        UpdateUIInfo();
    }

    void UpdateUIInfo()
    {
        infoText.text = $"<b>{characters[currentCharIndex].characterName}</b>\n" +
                      $"<color=#FFD700>{characters[currentCharIndex].variations[currentVarIndex].variationName}</color>";
    }

    void ConfirmSelection()
    {
        if (characters[currentCharIndex].characterObject != null)
        {
            characters[currentCharIndex].characterObject.transform.position = spawnPosition.position;
            characters[currentCharIndex].characterObject.transform.rotation = spawnPosition.rotation;
            hasConfirmedSelection = true; // Detiene la rotación y animación Idle

            // Activa la imagen del avatar y desactiva las demás
            ActivateAvatarImage();
        }
    }

    void ActivateAvatarImage()
    {
        // Desactiva todas las imágenes de avatar primero
        foreach (var character in characters)
        {
            if (character.avatarImage != null)
            {
                character.avatarImage.gameObject.SetActive(false);
            }
        }

        // Activa solo la imagen del avatar seleccionado
        if (characters[currentCharIndex].avatarImage != null)
        {
            characters[currentCharIndex].avatarImage.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        // Rota el personaje solo si no se ha confirmado la selección
        if (!hasConfirmedSelection &&
            characters.Count > currentCharIndex &&
            characters[currentCharIndex].characterObject != null &&
            characters[currentCharIndex].characterObject.activeSelf)
        {
            characters[currentCharIndex].characterObject.transform.Rotate(0, 20 * Time.deltaTime, 0);
        }
    }
}