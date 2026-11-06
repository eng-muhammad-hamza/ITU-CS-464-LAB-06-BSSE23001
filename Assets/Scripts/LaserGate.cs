using System.Collections;
using UnityEngine;

public class LaserGate : MonoBehaviour
{
    [SerializeField] Collider beamCollider;
    [SerializeField] Renderer beamRenderer;
    [SerializeField] float offSeconds = 1.5f;
    [SerializeField] float warnSeconds = 0.6f;
    [SerializeField] float onSeconds = 1.2f;
    [SerializeField] float startDelay = 0f;

    Color normalColor;
    bool dangerousWanted;
    bool wasFrozen;

    void Awake() => normalColor = beamRenderer.material.color;

    IEnumerator Start()
    {
        SetBeam(false, false);
        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);
        while (true)
        {
            SetBeam(false, false);
            yield return new WaitForSeconds(offSeconds);

            float t = 0f;
            while (t < warnSeconds)
            {
                SetBeam(Mathf.FloorToInt(t * 12f) % 2 == 0, false);
                t += Time.deltaTime;
                yield return null;
            }

            SetBeam(true, true);
            yield return new WaitForSeconds(onSeconds);
        }
    }

    void Update()
    {
        bool frozen = FreezeEffect.Active;
        beamCollider.enabled = dangerousWanted && !frozen;
        if (frozen == wasFrozen) return;
        wasFrozen = frozen;
        beamRenderer.material.color = frozen ? JuiceFx.FreezeColor : normalColor;
    }

    void SetBeam(bool visible, bool dangerous)
    {
        beamRenderer.enabled = visible;
        dangerousWanted = dangerous;
    }
}