using UnityEngine;

public enum PowerUpType { Speed, Shield }

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

        bool isSpeed = type == PowerUpType.Speed;
        Color pickupColor = isSpeed ? JuiceFx.SpeedColor : JuiceFx.ShieldColor;
        JuiceFx.Burst(transform.position, pickupColor, 30, 5f, 0.18f, 0.7f);
        JuiceFx.Ring(transform.position, pickupColor, 2.4f, 0.45f);

        if (isSpeed)
        {
            if (other.TryGetComponent(out PlayerController playerController))
                playerController.BoostSpeed(multiplier, duration);
        }
        else
        {
            if (other.TryGetComponent(out Health playerHealth))
                playerHealth.GrantShield();
        }
        Destroy(gameObject);
    }
}