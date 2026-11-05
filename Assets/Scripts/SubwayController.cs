using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Health))]
public class SubwayController : MonoBehaviour
{
    [Header("Track")]
    [SerializeField] float laneWidth = 2.5f;
    [SerializeField] float startZ = 5f;
    [SerializeField] float finishZ = 446f;

    [Header("Speed")]
    [SerializeField] float startSpeed = 8f;
    [SerializeField] float endSpeed = 11f;
    [SerializeField] float launchDelay = 1.5f;
    [SerializeField] float respawnLaunchDelay = 0.6f;

    [Header("Lane change")]
    [SerializeField] float laneSnap = 14f;
    [SerializeField] float maxSideSpeed = 16f;

    [Header("Jump")]
    [SerializeField] float jumpSpeed = 7f;
    [SerializeField] float springJumpSpeed = 13f;
    [SerializeField] float gravityScale = 2f;
    [SerializeField] float jumpBufferSeconds = 0.15f;
    [SerializeField] float groundCheckDistance = 0.62f;

    [Header("HUD")]
    [SerializeField] TMP_Text runText;
    [SerializeField] GameObject hintObject;
    [SerializeField] float hintSeconds = 6f;

    Rigidbody rb;
    Health health;
    MagnetAbility magnet;
    int lane = 1;
    int springCharges;
    float jumpBufferedUntil;
    float launchAt;
    bool wasDead;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Health>();
        magnet = GetComponent<MagnetAbility>();
        rb.linearDamping = 0f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Start() => launchAt = Time.time + launchDelay;

    public void AddSpring() => springCharges = 1;

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsFinished) return;

        if (health.Current > 0)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) lane = Mathf.Max(0, lane - 1);
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) lane = Mathf.Min(2, lane + 1);
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                jumpBufferedUntil = Time.unscaledTime + jumpBufferSeconds;
        }

        if (hintObject != null && hintObject.activeSelf && Time.time > launchAt + hintSeconds)
            hintObject.SetActive(false);
        UpdateHud();
    }

    void FixedUpdate()
    {
        Vector3 velocity = rb.linearVelocity;

        if (health.Current <= 0)
        {
            wasDead = true;
            rb.linearVelocity = new Vector3(0f, velocity.y, 0f);
            return;
        }
        if (wasDead)
        {
            wasDead = false;
            lane = 1;
            launchAt = Time.time + respawnLaunchDelay;
        }
        if (GameManager.Instance != null && GameManager.Instance.IsFinished) return;

        float progress = Mathf.InverseLerp(startZ, finishZ, rb.position.z);
        float forward = Time.time < launchAt ? 0f : Mathf.Lerp(startSpeed, endSpeed, progress);

        float targetX = (lane - 1) * laneWidth;
        float side = Mathf.Clamp((targetX - rb.position.x) * laneSnap, -maxSideSpeed, maxSideSpeed);

        bool grounded = Physics.Raycast(rb.position, Vector3.down, groundCheckDistance, ~0, QueryTriggerInteraction.Ignore);
        float vertical = velocity.y + Physics.gravity.y * (gravityScale - 1f) * Time.fixedDeltaTime;

        if (grounded && Time.unscaledTime <= jumpBufferedUntil)
        {
            jumpBufferedUntil = 0f;
            bool mega = springCharges > 0;
            Vector3 feet = rb.position + Vector3.down * 0.4f;
            if (mega)
            {
                springCharges--;
                vertical = springJumpSpeed;
                JuiceFx.Burst(feet, JuiceFx.SpringColor, 18, 4f, 0.16f, 0.5f);
                JuiceFx.Ring(feet, JuiceFx.SpringColor, 2.2f, 0.35f);
                JuiceFx.FloatingText(rb.position + Vector3.up * 1.2f, "MEGA JUMP!", JuiceFx.SpringColor, 16f);
            }
            else
            {
                vertical = jumpSpeed;
                JuiceFx.Burst(feet, Color.white, 6, 2f, 0.1f, 0.3f);
            }
        }

        rb.linearVelocity = new Vector3(side, vertical, forward);
    }

    void UpdateHud()
    {
        if (runText == null) return;
        int percent = Mathf.RoundToInt(Mathf.Clamp01(Mathf.InverseLerp(startZ, finishZ, rb.position.z)) * 100f);
        int coins = GameManager.Instance != null ? GameManager.Instance.Score : 0;
        string line = $"Track {percent}%   Coins {coins}";
        if (magnet != null && magnet.IsActive) line += $"\nMAGNET {magnet.SecondsRemaining:0.0}s";
        if (springCharges > 0) line += "\nMEGA JUMP READY - jump just before the wall";
        runText.text = line;
    }
}