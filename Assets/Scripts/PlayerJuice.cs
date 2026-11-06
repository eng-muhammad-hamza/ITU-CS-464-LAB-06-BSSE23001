using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerController))]
public class PlayerJuice : MonoBehaviour
{
    [Header("Tuning")]
    [SerializeField] float hitStopSeconds = 0.06f;
    [SerializeField] float boostEndingWarningSeconds = 1.2f;
    [SerializeField] float invulnerabilityBlinkRate = 14f;

    Health health;
    PlayerController controller;
    Renderer bodyRenderer;
    Material bodyMaterial;
    Color baseBodyColor;
    ShieldBubble activeShieldBubble;
    TrailRenderer speedTrail;
    ParticleSystem speedSparks;
    Color flashColor;
    float flashSecondsLeft;
    float flashTotalSeconds;
    bool isDead;

    void Awake()
    {
        health = GetComponent<Health>();
        controller = GetComponent<PlayerController>();
        bodyRenderer = GetComponent<Renderer>();
        if (bodyRenderer == null) bodyRenderer = GetComponentInChildren<Renderer>();
        bodyMaterial = bodyRenderer.material;
        baseBodyColor = bodyMaterial.color;
        BuildSpeedEffects();
    }

    void BuildSpeedEffects()
    {
        speedTrail = gameObject.AddComponent<TrailRenderer>();
        speedTrail.time = 0.4f;
        speedTrail.startWidth = 0.8f;
        speedTrail.endWidth = 0f;
        speedTrail.minVertexDistance = 0.05f;
        speedTrail.sharedMaterial = JuiceFx.ParticleMaterial;
        Color trailColor = JuiceFx.SpeedColor;
        speedTrail.startColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0.85f);
        speedTrail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
        speedTrail.emitting = false;

        speedSparks = JuiceFx.CreateTrailSparks(transform, JuiceFx.SpeedColor);
    }

    void OnDisable()
    {
        if (Time.timeScale < 1f && !IsGameFinished()) Time.timeScale = 1f;
    }

    public void TookDamage(int heartsLost)
    {
        Vector3 position = transform.position;
        JuiceFx.Burst(position, JuiceFx.DamageColor, 22, 6f, 0.2f, 0.6f);
        JuiceFx.Ring(position, JuiceFx.DamageColor, 2.6f, 0.4f);
        JuiceFx.FloatingText(position + Vector3.up * 1.2f, $"-{heartsLost}", JuiceFx.DamageColor, 18f);
        ShakeCamera(0.28f, 0.35f);
        StartBodyFlash(JuiceFx.DamageColor, 0.25f);
        StartCoroutine(HitStopRoutine(hitStopSeconds));
    }

    public void ShieldBroke()
    {
        Vector3 position = transform.position;
        JuiceFx.Burst(position, JuiceFx.ShieldColor, 32, 7f, 0.2f, 0.6f);
        JuiceFx.Ring(position, JuiceFx.ShieldColor, 3f, 0.4f);
        ShakeCamera(0.18f, 0.2f);
        StartBodyFlash(JuiceFx.ShieldColor, 0.2f);
        StartCoroutine(HitStopRoutine(hitStopSeconds * 0.7f));
        if (activeShieldBubble != null)
        {
            activeShieldBubble.Close();
            activeShieldBubble = null;
        }
    }

    public void ShieldGained()
    {
        if (activeShieldBubble == null) activeShieldBubble = ShieldBubble.Create(transform);
    }

    public void Healed(int heartsGained)
    {
        JuiceFx.Burst(transform.position, JuiceFx.PositiveColor, 24, 5f, 0.18f, 0.6f);
        JuiceFx.FloatingText(transform.position + Vector3.up * 1.2f, $"+{heartsGained}", JuiceFx.PositiveColor, 18f);
    }

    public void Died()
    {
        isDead = true;
        speedTrail.emitting = false;
        Vector3 position = transform.position;
        JuiceFx.Burst(position, JuiceFx.DamageColor, 60, 8f, 0.25f, 0.9f);
        JuiceFx.Burst(position, Color.white, 20, 5f, 0.15f, 0.6f);
        JuiceFx.Ring(position, JuiceFx.DamageColor, 4.5f, 0.55f);
        ShakeCamera(0.5f, 0.6f);
        StartCoroutine(HitStopRoutine(hitStopSeconds * 2f));
    }

    public void Respawned()
    {
        isDead = false;
        speedTrail.Clear();
        Vector3 position = transform.position;
        JuiceFx.Burst(position, Color.white, 28, 6f, 0.18f, 0.6f);
        JuiceFx.Ring(position, Color.white, 3.5f, 0.5f);
    }

    public void SpeedBoostStarted()
    {
        speedTrail.emitting = true;
        speedSparks.Play();
        JuiceFx.Ring(transform.position, JuiceFx.SpeedColor, 3f, 0.4f);
    }

    public void SpeedBoostEnded()
    {
        speedTrail.emitting = false;
        speedSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        JuiceFx.Burst(transform.position, JuiceFx.SpeedColor, 14, 3f, 0.14f, 0.4f);
    }

    void Update()
    {
        bool blinkHidden = health.IsInvulnerable && Mathf.FloorToInt(Time.time * invulnerabilityBlinkRate) % 2 == 0;
        bodyRenderer.enabled = !isDead && !blinkHidden;

        Color finalColor = baseBodyColor;
        if (controller.IsSpeedBoosted)
        {
            float blend = 0.5f + 0.25f * Mathf.Sin(Time.time * 12f);
            if (controller.SpeedBoostSecondsRemaining < boostEndingWarningSeconds)
                blend *= Mathf.Repeat(Time.time * 8f, 1f) < 0.5f ? 0f : 1f;
            finalColor = Color.Lerp(baseBodyColor, JuiceFx.SpeedColor, blend);
        }
        if (health.IsGhost)
        {
            float ghostBlend = 0.75f;
            if (health.GhostSecondsRemaining < boostEndingWarningSeconds)
                ghostBlend *= Mathf.Repeat(Time.time * 8f, 1f) < 0.5f ? 0f : 1f;
            finalColor = Color.Lerp(finalColor, JuiceFx.GhostColor, ghostBlend);
        }
        if (flashSecondsLeft > 0f)
        {
            flashSecondsLeft -= Time.unscaledDeltaTime;
            finalColor = Color.Lerp(finalColor, flashColor, Mathf.Clamp01(flashSecondsLeft / flashTotalSeconds));
        }
        bodyMaterial.color = finalColor;
    }

    void StartBodyFlash(Color color, float seconds)
    {
        flashColor = color;
        flashTotalSeconds = Mathf.Max(0.05f, seconds);
        flashSecondsLeft = flashTotalSeconds;
    }

    static void ShakeCamera(float seconds, float magnitude)
    {
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(seconds, magnitude);
    }

    IEnumerator HitStopRoutine(float seconds)
    {
        if (seconds <= 0f || IsGameFinished()) yield break;
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(seconds);
        Time.timeScale = IsGameFinished() ? 0f : 1f;
    }

    static bool IsGameFinished() => GameManager.Instance != null && GameManager.Instance.IsFinished;
}