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
        if (type == PowerUpType.Speed)
        {
            if (other.TryGetComponent(out PlayerController playerController))
                playerController.BoostSpeed(multiplier, duration);
        }
        else
        {
            if (other.TryGetComponent(out Health playerHealth))
                playerHealth.shielded = true;
        }
        Destroy(gameObject);
    }
}