using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{
    [System.Serializable]
    public class RendererSettings
    {
        public Renderer targetRenderer;
        public Material[] availableMaterials;
        public Vector3 previewPosition;  // Posición durante edición
        public Vector3 spawnPosition;    // Posición final al guardar
        public Vector3 previewScale = Vector3.one * 0.8f;
        public Vector3 finalScale = Vector3.one;
        [HideInInspector] public int selectedMaterialIndex = 0;
    }

    [Header("Configuración de Renderers")]
    public RendererSettings renderer1;
    public RendererSettings renderer2;

    [Header("UI References")]
    public TMP_Dropdown rendererDropdown;
    public TMP_Dropdown materialDropdown;
    public GameObject selectionCanvas;
    public Button saveButton;
    public TMP_Text feedbackText;

    private RendererSettings currentSettings;

    private void Start()
    {
        InitializeSystem();
    }

    private void InitializeSystem()
    {
        // Configurar dropdown de renderers
        rendererDropdown.ClearOptions();
        rendererDropdown.AddOptions(new System.Collections.Generic.List<string> { "Renderer 1", "Renderer 2" });
        rendererDropdown.onValueChanged.AddListener(SwitchActiveRenderer);

        // Posicionar inicialmente
        renderer1.targetRenderer.transform.position = renderer1.previewPosition;
        renderer2.targetRenderer.transform.position = renderer2.previewPosition;

        LoadPreferences();
        SwitchActiveRenderer(0);
    }

    private void ConfigureMaterialDropdown(RendererSettings settings)
    {
        materialDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();

        foreach (var material in settings.availableMaterials)
        {
            options.Add(new TMP_Dropdown.OptionData(material.name));
        }

        materialDropdown.AddOptions(options);
        materialDropdown.SetValueWithoutNotify(settings.selectedMaterialIndex);
        materialDropdown.onValueChanged.AddListener(ChangeMaterial);
    }

    public void SwitchActiveRenderer(int rendererIndex)
    {
        // Desactivar ambos
        renderer1.targetRenderer.gameObject.SetActive(false);
        renderer2.targetRenderer.gameObject.SetActive(false);

        // Activar el seleccionado
        currentSettings = rendererIndex == 0 ? renderer1 : renderer2;
        GameObject activeRenderer = currentSettings.targetRenderer.gameObject;

        // Configurar preview
        activeRenderer.SetActive(true);
        activeRenderer.transform.position = currentSettings.previewPosition;
        activeRenderer.transform.localScale = currentSettings.previewScale;

        // Actualizar materiales
        ConfigureMaterialDropdown(currentSettings);
        ChangeMaterial(currentSettings.selectedMaterialIndex);
    }

    public void ChangeMaterial(int materialIndex)
    {
        if (currentSettings != null && materialIndex < currentSettings.availableMaterials.Length)
        {
            currentSettings.selectedMaterialIndex = materialIndex;
            currentSettings.targetRenderer.material = currentSettings.availableMaterials[materialIndex];
        }
    }

    public void SaveAndMoveToSpawn()
    {
        if (currentSettings == null) return;

        // Mover a posición final
        currentSettings.targetRenderer.transform.position = currentSettings.spawnPosition;
        currentSettings.targetRenderer.transform.localScale = currentSettings.finalScale;

        // Guardar selección
        PlayerPrefs.SetInt("LastSelectedRenderer", rendererDropdown.value);
        PlayerPrefs.SetInt($"Renderer{rendererDropdown.value + 1}_Material", currentSettings.selectedMaterialIndex);
        PlayerPrefs.Save();

        // Feedback
        feedbackText.text = $"{rendererDropdown.options[rendererDropdown.value].text} guardado!";
        feedbackText.gameObject.SetActive(true);
        Invoke(nameof(HideFeedback), 2f);

        // Ocultar UI
        selectionCanvas.SetActive(false);
    }

    public void LoadPreferences()
    {
        // Cargar renderer seleccionado
        if (PlayerPrefs.HasKey("LastSelectedRenderer"))
        {
            int lastRenderer = PlayerPrefs.GetInt("LastSelectedRenderer");
            rendererDropdown.value = lastRenderer;
        }

        // Cargar materiales para cada renderer
        if (PlayerPrefs.HasKey("Renderer1_Material"))
            renderer1.selectedMaterialIndex = PlayerPrefs.GetInt("Renderer1_Material");
        if (PlayerPrefs.HasKey("Renderer2_Material"))
            renderer2.selectedMaterialIndex = PlayerPrefs.GetInt("Renderer2_Material");
    }

    private void HideFeedback()
    {
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
    }

    public void ShowSelector()
    {
        selectionCanvas.SetActive(true);
        // Volver a posición de preview
        SwitchActiveRenderer(rendererDropdown.value);
    }
}