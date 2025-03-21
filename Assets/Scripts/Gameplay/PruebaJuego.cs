using TMPro.Examples;
using UnityEngine;

public class PruebaJuego : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.ReloadCurrentScene();
        }
    }
}
