using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Spinner : MonoBehaviour
{
    [SerializeField] float degreesPerSecond = 60f;
    Rigidbody body;
    Hazard hazard;
    float angle;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        hazard = GetComponent<Hazard>();
        angle = transform.eulerAngles.y;
    }

    void FixedUpdate()
    {
        bool frozen = FreezeEffect.Active;
        if (hazard != null) hazard.enabled = !frozen;
        if (frozen) return;
        angle += degreesPerSecond * Time.fixedDeltaTime;
        body.MoveRotation(Quaternion.Euler(0f, angle, 0f));
    }
}