using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class SnapBackgroundsTool
{
    [MenuItem("Tools/Snap Selected Backgrounds Together %#g")]
    private static void SnapSelectedBackgroundsTogether()
    {
        GameObject[] selected = Selection.gameObjects;

        if (selected == null || selected.Length < 2)
        {
            Debug.LogWarning("Vælg mindst 2 baggrunde.");
            return;
        }

        // Filtrer kun objekter der har en Renderer
        List<GameObject> backgrounds = selected
            .Where(go => go.GetComponent<Renderer>() != null)
            .ToList();

        if (backgrounds.Count < 2)
        {
            Debug.LogWarning("De valgte objekter skal have en Renderer eller SpriteRenderer.");
            return;
        }

        // Sortér fra venstre mod højre ud fra world position
        backgrounds = backgrounds.OrderBy(go => go.transform.position.x).ToList();

        Undo.RecordObjects(backgrounds.Select(x => x.transform).ToArray(), "Snap Backgrounds Together");

        // Første objekt bliver stående som reference
        GameObject first = backgrounds[0];
        Bounds firstBounds = GetWorldBounds(first);

        float currentRightEdge = firstBounds.max.x;
        for (int i = 1; i < backgrounds.Count; i++)
        {
            GameObject current = backgrounds[i];
            Bounds currentBounds = GetWorldBounds(current);

            float width = currentBounds.size.x;
            float halfWidth = width * 0.5f;

            Vector3 pos = current.transform.position;

            // Placer objektet så venstre kant rører forrige objekts højre kant
            pos.x = currentRightEdge + halfWidth;

            current.transform.position = pos;

            // Opdater bounds efter flytning
            currentBounds = GetWorldBounds(current);
            currentRightEdge = currentBounds.max.x;
        }

        Debug.Log("Baggrunde snapped sammen uden mellemrum.");
    }

    private static Bounds GetWorldBounds(GameObject go)
    {
        Renderer rend = go.GetComponent<Renderer>();

        if (rend != null)
            return rend.bounds;

        // Fallback hvis noget mærkeligt sker
        return new Bounds(go.transform.position, Vector3.zero);
    }
}
