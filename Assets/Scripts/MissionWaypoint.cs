using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionWaypoint : MonoBehaviour
{
    public List<Image> imgs;
    public List<Transform> targets;
    public List<Text> meters;
    public List<Vector3> offsets;
    public Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    private void Update()
    {
        if (player == null)
        {
            player = PlayerController.PlayerTransform;
            if (player == null) return;
        }

        for (int i = targets.Count - 1; i >= 0; i--)
        {
            // Verificar que el índice sea válido y que los elementos necesarios existan
            if (i >= imgs.Count || i >= meters.Count || imgs[i] == null || meters[i] == null)
            {
                continue;
            }

            // Si el target es null, está desactivado, o su GameObject está inactivo
            if (targets[i] == null || !targets[i].gameObject.activeInHierarchy)
            {
                RemoveWaypoint(i);
            }
            else if (imgs[i].gameObject.activeSelf) // Solo actualizar si está activo
            {
                UpdateWaypoint(i);
            }
        }
    }

    private void UpdateWaypoint(int index)
    {
        // Verificar que todos los elementos necesarios no son null
        if (Camera.main == null ||
            index < 0 ||
            index >= imgs.Count ||
            index >= targets.Count ||
            index >= meters.Count ||
            index >= offsets.Count ||
            targets[index] == null ||
            player == null ||
            imgs[index] == null ||
            meters[index] == null)
        {
            return;
        }

        float minX = imgs[index].GetPixelAdjustedRect().width / 2;
        float maxX = Screen.width - minX;
        float minY = imgs[index].GetPixelAdjustedRect().height / 2;
        float maxY = Screen.height - minY;

        Vector2 pos = Camera.main.WorldToScreenPoint(targets[index].position + offsets[index]);

        if (Vector3.Dot((targets[index].position - player.position), player.forward) < 0)
        {
            pos.x = (pos.x < Screen.width / 2) ? maxX : minX;
        }

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        imgs[index].transform.position = pos;

        float distance = Vector3.Distance(targets[index].position, player.position);
        meters[index].text = (distance <= 0.1f) ? "0m" : ((int)distance).ToString() + "m";
    }

    public void RemoveWaypoint(int index)
    {
        imgs[index].gameObject.SetActive(false);
        meters[index].gameObject.SetActive(false);
    }

    public void ActivateWaypoint(int index)
    {
        if (index >= 0 && index < imgs.Count)
        {
            imgs[index].gameObject.SetActive(true);
            meters[index].gameObject.SetActive(true);
        }
    }
}
