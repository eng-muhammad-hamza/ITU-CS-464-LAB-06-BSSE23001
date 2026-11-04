using UnityEngine;

public enum EaseType { Linear, SmoothStep, Bounce }

[RequireComponent(typeof(Rigidbody))]
public class EasedMover : MonoBehaviour
{
    [SerializeField] Vector3 offset = new Vector3(6f, 0f, 0f);
    [SerializeField] float travelSeconds = 2f;
    [SerializeField] float pauseSeconds = 0.8f;
    [SerializeField] float startDelay = 0f;
    [SerializeField] EaseType ease = EaseType.SmoothStep;

    Rigidbody body;
    Vector3 startPosition;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        startPosition = transform.position;
    }

    void FixedUpdate()
    {
        float travel = Mathf.Max(0.05f, travelSeconds);
        float cycle = (travel + pauseSeconds) * 2f;
        float p = Mathf.Repeat(Time.time + startDelay, cycle);

        float u;
        if (p < travel) u = p / travel;
        else if (p < travel + pauseSeconds) u = 1f;
        else if (p < travel * 2f + pauseSeconds) u = 1f - (p - travel - pauseSeconds) / travel;
        else u = 0f;

        body.MovePosition(startPosition + offset * Apply(u));
    }

    float Apply(float x)
    {
        switch (ease)
        {
            case EaseType.SmoothStep: return x * x * (3f - 2f * x);
            case EaseType.Bounce: return EaseOutBounce(x);
            default: return x;
        }
    }

    static float EaseOutBounce(float x)
    {
        const float n1 = 7.5625f, d1 = 2.75f;
        if (x < 1f / d1) return n1 * x * x;
        if (x < 2f / d1) { x -= 1.5f / d1; return n1 * x * x + 0.75f; }
        if (x < 2.5f / d1) { x -= 2.25f / d1; return n1 * x * x + 0.9375f; }
        x -= 2.625f / d1;
        return n1 * x * x + 0.984375f;
    }
}