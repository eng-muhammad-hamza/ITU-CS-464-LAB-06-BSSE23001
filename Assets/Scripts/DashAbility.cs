using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class DashAbility : MonoBehaviour
{
    [SerializeField] float dashSpeed = 16f;
    [SerializeField] float dashSeconds = 0.2f;
    [SerializeField] float cooldownSeconds = 1.5f;

    PlayerController controller;
    Health health;
    Rigidbody body;
    float activeUntil;
    float readyAt;

    public bool IsActive => Time.time < activeUntil;
    public bool IsReady => Time.time >= readyAt;
    public float SecondsRemaining => Mathf.Max(0f, activeUntil - Time.time);

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        health = GetComponent<Health>();
        body = GetComponent<Rigidbody>();
    }

    public void Activate(float seconds)
    {
        activeUntil = Time.time + seconds;
        readyAt = 0f;
    }

    void Update()
    {
        if (!IsActive || !IsReady || !Input.GetKeyDown(KeyCode.Space)) return;
        if (health.Current <= 0) return;
        if (GameManager.Instance != null && GameManager.Instance.IsFinished) return;

        Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        if (direction.sqrMagnitude < 0.01f)
        {
            direction = new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);
            if (direction.sqrMagnitude < 0.01f) direction = Vector3.forward;
        }
        direction.Normalize();

        controller.Impulse(direction, dashSpeed, dashSeconds);
        readyAt = Time.time + cooldownSeconds;
        JuiceFx.Burst(transform.position, JuiceFx.DashColor, 14, 4f, 0.14f, 0.35f);
        JuiceFx.Ring(transform.position, JuiceFx.DashColor, 1.8f, 0.25f);
    }
}