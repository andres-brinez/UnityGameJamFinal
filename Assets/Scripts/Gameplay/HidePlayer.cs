using UnityEngine;

public class HidePlayer : MonoBehaviour
{
    [SerializeField] private float hideRadius = 2f;
    [SerializeField] private LayerMask playerLayer;

    public bool IsPlayerHiding { get; private set; }
    private PlayerController currentPlayer;

    void Update()
    {
        Collider[] players = Physics.OverlapSphere(transform.position, hideRadius, playerLayer);

        if (players.Length > 0)
        {
            PlayerController player = players[0].GetComponent<PlayerController>();
            if (player != null)
            {
                currentPlayer = player;
                IsPlayerHiding = player.isCrouching;
            }
        }
        else
        {
            // Solo resetear si el jugador que salió era el que estaba escondido
            if (currentPlayer != null && players.Length == 0)
            {
                IsPlayerHiding = false;
                currentPlayer = null;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = IsPlayerHiding ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, hideRadius);
    }
}