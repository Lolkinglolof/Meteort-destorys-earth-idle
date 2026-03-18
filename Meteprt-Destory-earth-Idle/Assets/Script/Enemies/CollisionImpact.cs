using UnityEngine;

public class CollisionImpact : MonoBehaviour
{
    [Header("Impact Settings")]
    [Tooltip("Hvor meget fart mister spilleren ved sammenstød?")]
    public float speedPenalty = 3f;

    [Tooltip("Hvor hårdt objektet skubber spilleren tilbage.")]
    public float impactForce = 8f;

    [Tooltip("Skal spilleren miste grebet ved sammenstød?")]
    public bool breakGrabOnHit = true;

    [Tooltip("Objektets masse, hvis det ikke selv har en MeteorController.")]
    public float objectMass = 2f;
}