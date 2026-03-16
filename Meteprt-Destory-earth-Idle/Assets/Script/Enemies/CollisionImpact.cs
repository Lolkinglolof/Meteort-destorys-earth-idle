using UnityEngine;

public class CollisionImpact : MonoBehaviour
{
    [Header("Impact Settings")]
    [Tooltip("Hvor meget fart mister spilleren ved sammenstød?")]
    public float speedPenalty = 3f;

    [Tooltip("Skal spilleren miste grebet (isGrabbing = false) ved dette sammenstød?")]
    public float impactForce = 1f;
}