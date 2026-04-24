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

        List<GameObject> backgrounds = selected
            .Where(go => go.GetComponentInChildren<Renderer>() != null)
            .ToList();

        if (backgrounds.Count < 2)
        {
            Debug.LogWarning("De valgte objekter skal have en Renderer (kan være på et child-objekt).");
            return;
        }

        Undo.RecordObjects(backgrounds.Select(x => x.transform).ToArray(), "Snap Backgrounds Together");

        GameObject anchor = backgrounds[0];
        List<GameObject> placed = new List<GameObject>() { anchor };
        List<GameObject> unplaced = new List<GameObject>(backgrounds);
        unplaced.Remove(anchor);

        // NYT: En mikroskopisk overlapning for at forhindre "Seams" (flickering linjer)
        float overlapAmount = 0.02f;

        while (unplaced.Count > 0)
        {
            GameObject bestUnplaced = null;
            GameObject bestPlaced = null;
            float minDistance = float.MaxValue;

            foreach (var u in unplaced)
            {
                Bounds bU = GetWorldBounds(u);
                foreach (var p in placed)
                {
                    Bounds bP = GetWorldBounds(p);
                    float dist = Vector3.Distance(bU.center, bP.center);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        bestUnplaced = u;
                        bestPlaced = p;
                    }
                }
            }

            Bounds pBounds = GetWorldBounds(bestPlaced);
            Bounds uBounds = GetWorldBounds(bestUnplaced);
            Vector3 diff = uBounds.center - pBounds.center;

            Vector3 pos = bestUnplaced.transform.position;
            float shiftX = 0;
            float shiftY = 0;

            if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
            {
                // -- HORISONTALT SNAP (Højre / Venstre) --
                if (diff.x > 0)
                    shiftX = pBounds.max.x - uBounds.min.x - overlapAmount; // Træk lidt til venstre
                else
                    shiftX = pBounds.min.x - uBounds.max.x + overlapAmount; // Træk lidt til højre

                shiftY = pBounds.min.y - uBounds.min.y;
            }
            else
            {
                // -- VERTIKALT SNAP (Oppe / Nede) --
                if (diff.y > 0)
                    shiftY = pBounds.max.y - uBounds.min.y - overlapAmount; // Træk lidt ned
                else
                    shiftY = pBounds.min.y - uBounds.max.y + overlapAmount; // Træk lidt op

                shiftX = pBounds.min.x - uBounds.min.x;
            }

            pos.x += shiftX;
            pos.y += shiftY;
            bestUnplaced.transform.position = pos;

            placed.Add(bestUnplaced);
            unplaced.Remove(bestUnplaced);
        }

        Debug.Log("Baggrunde snapped sammen med et lille overlap for at undgå grafiske fejl.");
    }

    private static Bounds GetWorldBounds(GameObject go)
    {
        Renderer rend = go.GetComponentInChildren<Renderer>();
        if (rend != null) return rend.bounds;
        return new Bounds(go.transform.position, Vector3.zero);
    }
}