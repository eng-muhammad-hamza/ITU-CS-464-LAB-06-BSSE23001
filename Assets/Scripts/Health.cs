using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int max = 3;
    [SerializeField] float iFrames = 1f;
    int current;
    float invulnerableUntil;
    bool shielded;
    PlayerJuice juice;
    PlayerController controller;

    public int Current => current;
    public int Max => max;
    public bool Shielded => shielded;
    public bool IsInvulnerable => Time.time < invulnerableUntil;

    void Awake()
    {
        current = max;
        juice = GetComponent<PlayerJuice>();
        controller = GetComponent<PlayerController>();
    }

    public bool TakeDamage(int amount)
    {
        if (amount <= 0 || current <= 0 || IsInvulnerable) return false;
        invulnerableUntil = Time.time + iFrames;

        if (shielded)
        {
            shielded = false;
            if (juice != null) juice.ShieldBroke();
            return false;
        }

        int previous = current;
        current = Mathf.Max(0, current - amount);
        if (GameManager.Instance != null) GameManager.Instance.UpdateUI();

        if (current > 0)
        {
            if (juice != null) juice.TookDamage(previous - current);
            return true;
        }

        if (controller != null)
        {
            controller.SetControlsEnabled(false);
            controller.CancelBoost();
        }
        if (juice != null) juice.Died();
        if (GameManager.Instance != null) GameManager.Instance.PlayerDied();
        return true;
    }

    public bool Heal(int amount)
    {
        if (amount <= 0 || current <= 0 || current >= max) return false;
        int previous = current;
        current = Mathf.Min(max, current + amount);
        if (juice != null) juice.Healed(current - previous);
        if (GameManager.Instance != null) GameManager.Instance.UpdateUI();
        return true;
    }

    public void GrantShield()
    {
        shielded = true;
        if (juice != null) juice.ShieldGained();
    }

    public void Restore(float protection)
    {
        current = max;
        invulnerableUntil = Time.time + protection;
        if (controller != null) controller.SetControlsEnabled(true);
        if (juice != null) juice.Respawned();
        if (GameManager.Instance != null) GameManager.Instance.UpdateUI();
    }
}