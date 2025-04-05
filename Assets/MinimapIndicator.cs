using UnityEngine;

public class NonRotatingMinimapIndicator : MonoBehaviour
{
    [Header("Referencias")]
    public Transform target;          // Objeto a rastrear (poción, enemigo, etc.)
    public Transform player;         // Jugador (centro de referencia)
    public SpriteRenderer indicatorRenderer; // SpriteRenderer del indicador
    public Camera minimapCamera;     // Cámara del minimapa
    public RectTransform minimapCircle; // RectTransform del minimapa circular (UI)

    [Header("Ajustes")]
    public float maxRadius = 100f;   // Radio máximo del minimapa en unidades de mundo
    public float edgePadding = 0.9f; // Margen desde el borde (0.8 a 0.95)
    public float edgeScale = 1.5f;   // Escala cuando está en el borde

    [Header("Colores")]
    public Color normalColor = Color.white;
    public Color edgeColor = Color.red;

    private Vector3 originalScale;
    private Vector3 originalRotation; // Guarda la rotación inicial

    void Start()
    {
        originalScale = indicatorRenderer.transform.localScale;
        originalRotation = indicatorRenderer.transform.eulerAngles; // Rotación inicial
    }

    void LateUpdate()
    {
        if (target == null || player == null || minimapCamera == null) return;

        // 1. Calcula la dirección y distancia entre el jugador y el objeto
        Vector3 playerToTarget = target.position - player.position;
        float distance = playerToTarget.magnitude;
        Vector3 direction = playerToTarget.normalized;

        // 2. Posición relativa al jugador (ignorando altura)
        Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);
        Vector3 worldEdgePos = player.position + flatDirection * maxRadius;

        // 3. Proyecta la posición en el minimapa
        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(worldEdgePos);
        Vector2 viewportCenter = new Vector2(0.5f, 0.5f);
        Vector2 viewportDir = new Vector2(viewportPos.x, viewportPos.y) - viewportCenter;

        // 4. Fuerza la posición al borde circular del minimapa
        float angle = Mathf.Atan2(viewportDir.y, viewportDir.x);
        Vector2 edgeViewportPos = viewportCenter + new Vector2(
            Mathf.Cos(angle) * 0.5f * edgePadding,
            Mathf.Sin(angle) * 0.5f * edgePadding
        );

        // 5. Convierte a posición de mundo (conservando la altura original)
        Vector3 finalWorldPos = minimapCamera.ViewportToWorldPoint(
            new Vector3(edgeViewportPos.x, edgeViewportPos.y, viewportPos.z)
        );
        finalWorldPos.y = target.position.y;

        // 6. Decide si el objeto está dentro o fuera del minimapa
        bool isInsideMinimap = (distance <= maxRadius);

        if (!isInsideMinimap)
        {
            // --- OBJETO FUERA DEL RADIO ---
            indicatorRenderer.transform.position = finalWorldPos;
            indicatorRenderer.color = edgeColor;
            indicatorRenderer.transform.localScale = originalScale * edgeScale;

            // 🔄 Mantiene la rotación original (no rota)
            indicatorRenderer.transform.eulerAngles = originalRotation;
        }
        else
        {
            // --- OBJETO DENTRO DEL MINIMAPA ---
            indicatorRenderer.transform.position = target.position;
            indicatorRenderer.color = normalColor;
            indicatorRenderer.transform.localScale = originalScale;
            indicatorRenderer.transform.eulerAngles = originalRotation; // Opcional: mantener rotación
        }
    }
}