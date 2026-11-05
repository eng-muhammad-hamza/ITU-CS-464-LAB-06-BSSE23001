using UnityEngine;

[RequireComponent(typeof(CameraShake))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0f, 14f, -8f);
    [SerializeField] float smoothing = 6f;
    [SerializeField] float minZ = -22f;
    [SerializeField] float maxZ = 6f;
    [SerializeField] bool lockHeight = false;
    CameraShake shake;

    void Awake() => shake = GetComponent<CameraShake>();

    void Start()
    {
        if (target != null) shake.RestingLocalPosition = Desired();
    }

    void LateUpdate()
    {
        if (target == null) return;
        float blend = 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime);
        shake.RestingLocalPosition = Vector3.Lerp(shake.RestingLocalPosition, Desired(), blend);
    }

    Vector3 Desired()
    {
        Vector3 p = target.position + offset;
        p.x = offset.x;
        if (lockHeight) p.y = offset.y;
        p.z = Mathf.Clamp(p.z, minZ, maxZ);
        return p;
    }
}