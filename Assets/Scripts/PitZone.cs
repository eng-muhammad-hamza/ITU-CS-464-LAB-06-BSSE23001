using UnityEngine;

public class PitZone : MonoBehaviour
{
    [SerializeField] Transform[] exitPoints;
    [SerializeField] int damage = 1;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Rigidbody body = other.attachedRigidbody;
        if (body == null || exitPoints == null || exitPoints.Length == 0) return;

        Transform best = exitPoints[0];
        float bestDistance = float.MaxValue;
        foreach (Transform exit in exitPoints)
        {
            Vector3 d = exit.position - body.position;
            d.y = 0f;
            if (d.sqrMagnitude < bestDistance) { bestDistance = d.sqrMagnitude; best = exit; }
        }

        body.position = best.position;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        JuiceFx.Burst(best.position, JuiceFx.DamageColor, 20, 4f, 0.16f, 0.5f);
        JuiceFx.FloatingText(best.position + Vector3.up * 1.2f, "FELL!", JuiceFx.DamageColor, 18f);

        if (other.TryGetComponent(out Health health)) health.TakeDamage(damage);
    }
}