using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] float knockback = 5f;
    [SerializeField] float lifetimeSeconds = 4f;
    Vector3 direction;
    float speed;
    float age;
    bool launched;

    public void Launch(Vector3 aimDirection, float bulletSpeed)
    {
        direction = aimDirection.normalized;
        speed = bulletSpeed;
        launched = true;
    }

    void Update()
    {
        if (!launched || FreezeEffect.Active) return;
        age += Time.deltaTime;
        transform.position += direction * (speed * Time.deltaTime);
        if (age >= lifetimeSeconds) Pop();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!launched || FreezeEffect.Active) return;
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out Health health) && health.TakeDamage(damage))
            {
                Rigidbody body = other.attachedRigidbody;
                if (body != null) body.AddForce(direction * knockback, ForceMode.Impulse);
            }
            Pop();
            return;
        }
        if (other.isTrigger || other.GetComponentInParent<Turret>() != null) return;
        Pop();
    }

    void Pop()
    {
        JuiceFx.Burst(transform.position, JuiceFx.DamageColor, 8, 3f, 0.1f, 0.3f);
        Destroy(gameObject);
    }
}