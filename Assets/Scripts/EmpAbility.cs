using TMPro;
using UnityEngine;

public class EmpAbility : MonoBehaviour
{
    [SerializeField] TMP_Text hudText;
    [SerializeField] float radius = 9f;
    [SerializeField] float stunSeconds = 4f;
    Health health;
    int charges;

    void Awake() => health = GetComponent<Health>();
    void Start() => Refresh();

    public void AddCharge()
    {
        charges++;
        Refresh();
    }

    void Update()
    {
        if (charges <= 0 || !Input.GetKeyDown(KeyCode.E)) return;
        if (health != null && health.Current <= 0) return;
        if (GameManager.Instance != null && GameManager.Instance.IsFinished) return;
        charges--;
        Fire();
        Refresh();
    }

    void Fire()
    {
        Vector3 center = transform.position;
        JuiceFx.Ring(center, JuiceFx.EmpColor, radius, 0.5f);
        JuiceFx.Burst(center, JuiceFx.EmpColor, 36, 7f, 0.18f, 0.6f);
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.2f, 0.2f);

        foreach (Collider hit in Physics.OverlapSphere(center, radius, ~0, QueryTriggerInteraction.Collide))
        {
            GuardAI guard = hit.GetComponentInParent<GuardAI>();
            if (guard != null) guard.Stun(stunSeconds);
        }
    }

    void Refresh()
    {
        if (hudText != null) hudText.text = charges > 0 ? $"EMP x{charges}  -  press E" : "";
    }
}