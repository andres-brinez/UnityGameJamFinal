using UnityEngine;
using TMPro;

public class ShowMessage : MonoBehaviour
{
    [Header("Configuraci�n del Mensaje")]
    [TextArea(2, 5)]
    public string message = "�Has encontrado algo interesante!";

    public GameObject messageUI;
    public TextMeshProUGUI messageText;

    private void Start()
    {
        if (messageUI != null)
            messageUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger activado con: " + other.name); // Verifica si el Player entra

        if (other.CompareTag("Player"))
        {
            Debug.Log("El jugador ha entrado al �rea."); // Debug para confirmar que se detect� al Player

            if (messageUI != null && messageText != null)
            {
                messageText.text = message;
                messageUI.SetActive(true);
                Debug.Log("Mensaje mostrado: " + message);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger salido por: " + other.name);

        if (other.CompareTag("Player"))
        {
            if (messageUI != null)
            {
                messageUI.SetActive(false);
                Debug.Log("Mensaje ocultado.");
            }
        }
    }
}


