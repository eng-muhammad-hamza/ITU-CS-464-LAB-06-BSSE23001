using UnityEngine;

public enum PowerUpType { Speed, Shield, Ghost, Emp, Magnet, Spring, Dash, Freeze, SlowTime, CoinX2 }

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

        switch (type)
        {
            case PowerUpType.Speed:
                if (other.TryGetComponent(out PlayerController pc)) pc.BoostSpeed(multiplier, duration);
                break;
            case PowerUpType.Shield:
                if (other.TryGetComponent(out Health shieldTarget)) shieldTarget.GrantShield();
                break;
            case PowerUpType.Ghost:
                if (other.TryGetComponent(out Health ghostTarget)) ghostTarget.GrantGhost(duration);
                break;
            case PowerUpType.Emp:
                if (other.TryGetComponent(out EmpAbility emp)) emp.AddCharge();
                break;
            case PowerUpType.Magnet:
                if (other.TryGetComponent(out MagnetAbility magnet)) magnet.Activate(duration);
                break;
            case PowerUpType.Spring:
                if (other.TryGetComponent(out SubwayController runner)) runner.AddSpring();
                break;
            case PowerUpType.Dash:
                if (other.TryGetComponent(out DashAbility dash)) dash.Activate(duration);
                break;
            case PowerUpType.Freeze:
                FreezeEffect.Begin(duration);
                JuiceFx.Ring(other.transform.position, JuiceFx.FreezeColor, 9f, 0.5f);
                break;
            case PowerUpType.SlowTime:
                if (GameManager.Instance != null) GameManager.Instance.SlowTimer(duration, multiplier);
                break;
            case PowerUpType.CoinX2:
                if (GameManager.Instance != null) GameManager.Instance.DoubleCoins(duration);
                break;
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
            case PowerUpType.Magnet: return JuiceFx.MagnetColor;
            case PowerUpType.Spring: return JuiceFx.SpringColor;
            case PowerUpType.Dash: return JuiceFx.DashColor;
            case PowerUpType.Freeze: return JuiceFx.FreezeColor;
            case PowerUpType.SlowTime: return JuiceFx.SlowColor;
            case PowerUpType.CoinX2: return JuiceFx.GoldColor;
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
            case PowerUpType.Magnet: return "MAGNET";
            case PowerUpType.Spring: return "SPRING";
            case PowerUpType.Dash: return "DASH [SPACE]";
            case PowerUpType.Freeze: return "FREEZE";
            case PowerUpType.SlowTime: return "SLOW-MO";
            case PowerUpType.CoinX2: return "COINS x2";
            default: return "EMP [E]";
        }
    }
}