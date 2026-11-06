using UnityEngine;

public class ShieldBubble : MonoBehaviour
{
    const float BubbleDiameter = 1.5f;
    const int RingSegments = 48;

    Material bubbleMaterial;
    LineRenderer ringLine;
    Light glowLight;
    float openElapsedSeconds;
    float closeElapsedSeconds;
    bool isClosing;

    public static ShieldBubble Create(Transform playerTransform)
    {
        GameObject root = new GameObject("ShieldBubble");
        root.transform.SetParent(playerTransform, false);
        ShieldBubble bubble = root.AddComponent<ShieldBubble>();
        bubble.Build();
        return bubble;
    }

    void Build()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(sphere.GetComponent<Collider>());
        sphere.transform.SetParent(transform, false);
        sphere.transform.localScale = Vector3.one * BubbleDiameter;
        bubbleMaterial = new Material(JuiceFx.ParticleMaterial);
        sphere.GetComponent<MeshRenderer>().sharedMaterial = bubbleMaterial;

        GameObject ringObject = new GameObject("ShieldRing");
        ringObject.transform.SetParent(transform, false);
        ringLine = ringObject.AddComponent<LineRenderer>();
        ringLine.useWorldSpace = false;
        ringLine.loop = true;
        ringLine.positionCount = RingSegments;
        ringLine.sharedMaterial = JuiceFx.ParticleMaterial;
        float ringRadius = BubbleDiameter * 0.5f + 0.05f;
        for (int i = 0; i < RingSegments; i++)
        {
            float angle = i * Mathf.PI * 2f / RingSegments;
            ringLine.SetPosition(i, new Vector3(Mathf.Cos(angle) * ringRadius, Mathf.Sin(angle) * ringRadius, 0f));
        }

        GameObject lightObject = new GameObject("ShieldLight");
        lightObject.transform.SetParent(transform, false);
        glowLight = lightObject.AddComponent<Light>();
        glowLight.type = LightType.Point;
        glowLight.color = JuiceFx.ShieldColor;
        glowLight.range = 3.5f;
        glowLight.intensity = 0f;
    }

    public void Close() => isClosing = true;

    void Update()
    {
        float time = Time.unscaledTime;
        float scaleMultiplier;
        float openness;

        if (!isClosing)
        {
            openElapsedSeconds += Time.deltaTime;
            float progress = Mathf.Clamp01(openElapsedSeconds / 0.3f);
            float shifted = progress - 1f;
            float easedOpen = 1f + 2.70158f * shifted * shifted * shifted + 1.70158f * shifted * shifted;
            scaleMultiplier = easedOpen * (1f + 0.04f * Mathf.Sin(time * 6f));
            openness = 1f;
        }
        else
        {
            closeElapsedSeconds += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(closeElapsedSeconds / 0.25f);
            scaleMultiplier = 1f + 0.6f * progress;
            openness = 1f - progress;
            if (progress >= 1f)
            {
                Destroy(gameObject);
                return;
            }
        }

        transform.localScale = Vector3.one * scaleMultiplier;
        Color shieldColor = JuiceFx.ShieldColor;
        bubbleMaterial.color = new Color(shieldColor.r, shieldColor.g, shieldColor.b, (0.22f + 0.08f * Mathf.Sin(time * 5f)) * openness);

        Color ringColor = new Color(shieldColor.r, shieldColor.g, shieldColor.b, 0.95f * openness);
        ringLine.startColor = ringColor;
        ringLine.endColor = ringColor;
        float ringWidth = 0.05f + 0.02f * Mathf.Sin(time * 8f);
        ringLine.startWidth = ringWidth;
        ringLine.endWidth = ringWidth;
        Camera mainCamera = Camera.main;
        if (mainCamera != null) ringLine.transform.rotation = mainCamera.transform.rotation;   // ring always faces the camera

        glowLight.intensity = 1.6f * openness;
    }
}