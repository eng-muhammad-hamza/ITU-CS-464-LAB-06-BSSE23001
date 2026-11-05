using UnityEngine;

public class TeleportPad : MonoBehaviour
{
    [SerializeField] TeleportPad destination;
    [SerializeField] float pulseInterval = 1.4f;
    static float blockedUntil;
    float nextPulse;

    void Update()
    {
        if (Time.time < nextPulse) return;
        nextPulse = Time.time + pulseInterval;
        JuiceFx.Ring(transform.position + Vector3.up * 0.1f, JuiceFx.PortalColor, 1.6f, 0.9f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (destination == null || Time.time < blockedUntil || !other.CompareTag("Player")) return;
        Rigidbody body = other.attachedRigidbody;
        if (body == null) return;

        blockedUntil = Time.time + 1f;
        JuiceFx.Burst(transform.position + Vector3.up * 0.4f, JuiceFx.PortalColor, 24, 4f, 0.16f, 0.5f);

        Vector3 target = destination.transform.position;
        target.y = body.position.y;
        body.position = target;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;

        JuiceFx.Burst(target, JuiceFx.PortalColor, 24, 4f, 0.16f, 0.5f);
        JuiceFx.Ring(target, JuiceFx.PortalColor, 3f, 0.4f);
    }
}