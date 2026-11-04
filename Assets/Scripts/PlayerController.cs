using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveForce = 10f;

    [Header("Ice")]
    [SerializeField] float iceDamping = 0.1f;
    [SerializeField] float iceControl = 0.5f;
    [SerializeField] float iceMaxSpeed = 7f;
    int iceContacts;
    float normalLinearDamping;
    float normalAngularDamping;

    Rigidbody rb;
    PlayerJuice juice;
    Vector3 input;
    float speedMultiplier = 1f;
    Coroutine boost;
    float boostEndTime;
    bool controlsEnabled = true;

    public bool IsSpeedBoosted => boost != null;
    public float SpeedBoostSecondsRemaining => boost == null ? 0f : Mathf.Max(0f, boostEndTime - Time.time);

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.sleepThreshold = 0f;
        juice = GetComponent<PlayerJuice>();
        normalLinearDamping = rb.linearDamping;
        normalAngularDamping = rb.angularDamping;
    }

    void OnDisable()
    {
        speedMultiplier = 1f;
        boost = null;
    }

    void Update()
    {
        if (!controlsEnabled)
        {
            input = Vector3.zero;
            return;
        }
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        input = Vector3.ClampMagnitude(new Vector3(h, 0f, v), 1f);
    }

    void FixedUpdate()
    {
        bool onIce = iceContacts > 0;
        rb.AddForce(input * moveForce * speedMultiplier * (onIce ? iceControl : 1f));
        if (!onIce) return;

        Vector3 velocity = rb.linearVelocity;
        Vector3 flat = new Vector3(velocity.x, 0f, velocity.z);
        if (flat.magnitude > iceMaxSpeed)
        {
            flat = flat.normalized * iceMaxSpeed;
            rb.linearVelocity = new Vector3(flat.x, velocity.y, flat.z);
        }
    }

    public void SetControlsEnabled(bool controlsAllowed)
    {
        controlsEnabled = controlsAllowed;
        if (!controlsAllowed) input = Vector3.zero;
    }

    public void BoostSpeed(float multiplier, float seconds)
    {
        if (multiplier <= 0f || seconds <= 0f) return;
        if (boost != null) StopCoroutine(boost);
        boost = StartCoroutine(Boost(multiplier, seconds));
    }

    IEnumerator Boost(float multiplier, float seconds)
    {
        speedMultiplier = multiplier;
        boostEndTime = Time.time + seconds;
        if (juice != null) juice.SpeedBoostStarted();
        yield return new WaitForSeconds(seconds);
        speedMultiplier = 1f;
        boost = null;
        if (juice != null) juice.SpeedBoostEnded();
    }

    public void CancelBoost()
    {
        if (boost == null) return;
        StopCoroutine(boost);
        boost = null;
        speedMultiplier = 1f;
        if (juice != null) juice.SpeedBoostEnded();
    }

    public void EnterIce() { iceContacts++; ApplyIce(); }
    public void ExitIce() { iceContacts = Mathf.Max(0, iceContacts - 1); ApplyIce(); }

    void ApplyIce()
    {
        bool onIce = iceContacts > 0;
        rb.linearDamping = onIce ? iceDamping : normalLinearDamping;
        rb.angularDamping = onIce ? iceDamping : normalAngularDamping;
    }
}