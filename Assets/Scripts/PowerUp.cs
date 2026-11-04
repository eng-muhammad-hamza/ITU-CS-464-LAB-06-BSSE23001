using UnityEngine;

public enum PowerUpType { Speed, Shield, Ghost, Emp }

public class PowerUp : MonoBehaviour
{
    [SerializeField] PowerUpType type = PowerUpType.Speed;
    [SerializeField] float multiplier = 1.6f;
    [SerializeField] float duration = 5f;
    bool alreadyCollected;

    void OnTriggerEnter(Collider other)
    {
        if (alreadyCollected || !other.CompareTag("Player")) return;
        alreadyCollected = true;

        Color pickupColor = ColorFor(type);
        JuiceFx.Burst(transform.position, pickupColor, 30, 5f, 0.18f, 0.7f);
        JuiceFx.Ring(transform.position, pickupColor, 2.4f, 0.45f);
        JuiceFx.FloatingText(transform.position + Vector3.up * 0.9f, LabelFor(type), pickupColor, 14f);

        if (type == PowerUpType.Speed)
        {
            if (other.TryGetComponent(out PlayerController playerController))
                playerController.BoostSpeed(multiplier, duration);
        }
        else if (type == PowerUpType.Shield)
        {
            if (other.TryGetComponent(out Health shieldTarget)) shieldTarget.GrantShield();
        }
        else if (type == PowerUpType.Ghost)
        {
            if (other.TryGetComponent(out Health ghostTarget)) ghostTarget.GrantGhost(duration);
        }
        else if (other.TryGetComponent(out EmpAbility empAbility))
        {
            empAbility.AddCharge();
        }
        Destroy(gameObject);
    }

    static Color ColorFor(PowerUpType t)
    {
        switch (t)
        {
            case PowerUpType.Speed: return JuiceFx.SpeedColor;
            case PowerUpType.Shield: return JuiceFx.ShieldColor;
            case PowerUpType.Ghost: return JuiceFx.GhostColor;
            default: return JuiceFx.EmpColor;
        }
    }

    static string LabelFor(PowerUpType t)
    {
        switch (t)
        {
            case PowerUpType.Speed: return "SPEED";
            case PowerUpType.Shield: return "SHIELD";
            case PowerUpType.Ghost: return "GHOST";
            default: return "EMP [E]";
        }
    }
}