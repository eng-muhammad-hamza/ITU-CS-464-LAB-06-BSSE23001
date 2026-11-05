using UnityEngine;

public class MagnetAbility : MonoBehaviour
{
    [SerializeField] float radius = 7f;
    [SerializeField] float pullSpeed = 22f;
    readonly Collider[] hits = new Collider[64];
    float activeUntil;

    public bool IsActive => Time.time < activeUntil;
    public float SecondsRemaining => Mathf.Max(0f, activeUntil - Time.time);

    public void Activate(float seconds) => activeUntil = Time.time + seconds;

    void Update()
    {
        if (!IsActive) return;
        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, hits, ~0, QueryTriggerInteraction.Collide);
        for (int i = 0; i < count; i++)
        {
            Coin coin = hits[i].GetComponentInParent<Coin>();
            if (coin == null) continue;
            coin.transform.position = Vector3.MoveTowards(coin.transform.position, transform.position, pullSpeed * Time.deltaTime);
        }
    }
}