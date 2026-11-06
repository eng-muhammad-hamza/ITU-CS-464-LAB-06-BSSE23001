using UnityEngine;

public class TrackStripes : MonoBehaviour
{
    [SerializeField] float stripeLength = 4f;
    [SerializeField] bool swapAxes = false;
    [SerializeField] Color colorA = new Color(0.13f, 0.14f, 0.19f);
    [SerializeField] Color colorB = new Color(0.18f, 0.19f, 0.26f);

    void Start()
    {
        Texture2D texture = swapAxes
            ? new Texture2D(2, 1, TextureFormat.RGBA32, false)
            : new Texture2D(1, 2, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.SetPixel(0, 0, colorA);
        if (swapAxes) texture.SetPixel(1, 0, colorB); else texture.SetPixel(0, 1, colorB);
        texture.Apply();

        float repeats = transform.localScale.z / (stripeLength * 2f);
        Material material = GetComponent<Renderer>().material;
        material.color = Color.white;
        material.mainTexture = texture;
        material.mainTextureScale = swapAxes ? new Vector2(repeats, 1f) : new Vector2(1f, repeats);
    }
}