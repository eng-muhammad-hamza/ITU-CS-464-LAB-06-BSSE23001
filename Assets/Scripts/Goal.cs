using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] float pulseInterval = 1.2f;
    float nextPulse;
    bool reached;

    void Update()
    {
        if (Time.time < nextPulse) return;
        nextPulse = Time.time + pulseInterval;
        JuiceFx.Ring(transform.position + Vector3.up * 0.1f, JuiceFx.PositiveColor, 2.2f, 0.9f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (reached || !other.CompareTag("Player")) return;
        reached = true;
        if (GameManager.Instance != null) GameManager.Instance.ReachGoal();
    }
}