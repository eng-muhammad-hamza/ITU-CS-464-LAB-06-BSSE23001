using UnityEngine;

public class TileGrid : MonoBehaviour
{
    [SerializeField] GameObject tilePrefab;
    [SerializeField] int columns = 5;
    [SerializeField] int rows = 5;
    [SerializeField] float tileSize = 2f;

    void Start()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("TileGrid needs a Tile Prefab.", this);
            return;
        }
        float startX = -(columns - 1) * tileSize * 0.5f;
        float startZ = -(rows - 1) * tileSize * 0.5f;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
            {
                Vector3 position = transform.position + new Vector3(startX + c * tileSize, 0f, startZ + r * tileSize);
                GameObject instance = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                if (instance.TryGetComponent(out CollapsingTile tile)) tile.SetShade((r + c) % 2 == 0 ? 1f : 0.82f);
            }
    }
}