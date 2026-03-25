/*
using UnityEngine;
using System.Collections.Generic; // NYT: Giver os adgang til at lave Lister

public class MasseSteal : MonoBehaviour
{
[Header("Steal Settings")]
public float Stealfactor = 0.5f;

private MeteorController meteorController;

// NYT: En liste der husker PRÆCIS hvilke fjender vi rører lige nu
private List<GameObject> activeCollisions = new List<GameObject>();

void Start()
{
  meteorController = GetComponent<MeteorController>();
}

private void OnCollisionEnter2D(Collision2D collision)
{
  GameObject hitObj = collision.gameObject;
  string hitTag = hitObj.tag;

  if (hitTag == "Enemy" || hitTag == "Meteor" || hitTag == "RareMeteor" || hitTag == "MeteorRare")
  {
      // Tjekker at vi IKKE allerede har stjålet fra denne specifikke fjende under dette sammenstød
      if (!activeCollisions.Contains(hitObj))
      {
          // Lås fjenden, så vi ikke kan stjæle fra den igen før vi forlader den
          activeCollisions.Add(hitObj);

          if (meteorController != null)
          {
              // 1. Beregn hvor meget masse vi vil stjæle
              float massToSteal = (meteorController.CurrentDamage * Stealfactor) / meteorController.currentLiveMass * meteorController.CurrentActualSpeed;

              // 2. Find ud af, hvad spillerens MAX masse er (baseret på deres køb i UpgradeManager)
              float maxAllowedMass = 100f; // Fallback, hvis UpgradeManager mangler
              if (UpgradeManager.Instance != null)
              {
                  maxAllowedMass = UpgradeManager.Instance.GetCurrentMass();
              }

              // 3. Læg massen til, men sørg for det ALDRIG overstiger vores max grænse (Clamp)
              meteorController.currentLiveMass = Mathf.Clamp(meteorController.currentLiveMass + massToSteal, 0f, maxAllowedMass);

              // 4. Opdater spillerens størrelse visuelt
              meteorController.RefreshMeteorScale();

              Debug.Log("<color=green>Stjal " + massToSteal.ToString("F1") + " masse! Nuværende masse: " + meteorController.currentLiveMass.ToString("F1") + " / " + maxAllowedMass + "</color>");
          }
      }
  }
}

// NYT: Denne funktion kaldes automatisk af Unity, når vi FORLADER et objekt
private void OnCollisionExit2D(Collision2D collision)
{
  // Hvis vi forlader en fjende, sletter vi den fra listen. Nu kan vi stjæle fra den igen!
  if (activeCollisions.Contains(collision.gameObject))
  {
      activeCollisions.Remove(collision.gameObject);
  }
}
} */