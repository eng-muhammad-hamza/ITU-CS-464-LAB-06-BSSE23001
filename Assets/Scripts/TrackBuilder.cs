using System.Collections.Generic;
using UnityEngine;

public class TrackBuilder : MonoBehaviour
{
    public enum Kind { Barrier, Train, Oncoming, CoinLine, CoinArch, Heart, Magnet, Spring, Shield }

    readonly struct Row
    {
        public readonly Kind kind;
        public readonly string lanes;
        public readonly float z;
        public readonly float size;
        public readonly int count;
        public Row(Kind kind, string lanes, float z, float size = 0f, int count = 0)
        { this.kind = kind; this.lanes = lanes; this.z = z; this.size = size; this.count = count; }
    }

    static readonly Row[] Rows =
    {
        new Row(Kind.Barrier,  "M",   28),
        new Row(Kind.CoinArch, "M",   28),
        new Row(Kind.Barrier,  "LR",  44),
        new Row(Kind.CoinLine, "M",   39, 1.4f, 8),
        new Row(Kind.Barrier,  "LMR", 60),
        new Row(Kind.CoinArch, "M",   60),
        new Row(Kind.Train,    "L",   78, 6f),
        new Row(Kind.CoinLine, "M",   72, 1.6f, 8),
        new Row(Kind.Train,    "M",   92, 6f),
        new Row(Kind.Barrier,  "R",   92),
        new Row(Kind.CoinLine, "L",   86, 1.6f, 8),
        new Row(Kind.Train,    "LM",  108, 8f),
        new Row(Kind.CoinLine, "R",   100, 1.5f, 9),
        new Row(Kind.Train,    "MR",  125, 6f),
        new Row(Kind.CoinLine, "L",   118, 1.5f, 9),
        new Row(Kind.Magnet,   "M",   132),
        new Row(Kind.Train,    "M",   138, 4f),
        new Row(Kind.Barrier,  "LR",  138),
        new Row(Kind.CoinArch, "LR",  138),
        new Row(Kind.CoinLine, "M",   160, 1.5f, 6),
        new Row(Kind.Barrier,  "LMR", 170),
        new Row(Kind.CoinArch, "M",   170),
        new Row(Kind.Train,    "L",   182, 6f),
        new Row(Kind.Barrier,  "M",   182),
        new Row(Kind.CoinLine, "R",   176, 1.5f, 8),
        new Row(Kind.Train,    "M",   196, 10f),
        new Row(Kind.Heart,    "L",   196),
        new Row(Kind.CoinLine, "R",   190, 1.5f, 8),
        new Row(Kind.Train,    "LR",  210, 6f),
        new Row(Kind.CoinLine, "M",   203, 1.6f, 8),
        new Row(Kind.Shield,   "M",   217),
        new Row(Kind.Barrier,  "LM",  222),
        new Row(Kind.CoinLine, "R",   218, 1.5f, 6),
        new Row(Kind.Barrier,  "MR",  230),
        new Row(Kind.CoinLine, "L",   228, 1.2f, 5),
        new Row(Kind.Spring,   "LMR", 236),
        new Row(Kind.Train,    "LMR", 250, 3f),
        new Row(Kind.Heart,    "M",   260),
        new Row(Kind.Train,    "LR",  268, 8f),
        new Row(Kind.CoinLine, "M",   264, 1.3f, 9),
        new Row(Kind.Barrier,  "LMR", 284),
        new Row(Kind.CoinArch, "M",   284),
        new Row(Kind.CoinLine, "M",   308, 1.3f, 14),
        new Row(Kind.Train,    "LR",  322, 6f),
        new Row(Kind.Barrier,  "LMR", 336),
        new Row(Kind.CoinArch, "M",   336),
        new Row(Kind.CoinLine, "L",   352, 1.5f, 8),
        new Row(Kind.CoinLine, "R",   352, 1.5f, 8),
        new Row(Kind.Oncoming, "M",   376, 6f),
        new Row(Kind.Barrier,  "LMR", 388),
        new Row(Kind.CoinArch, "M",   388),
        new Row(Kind.CoinLine, "L",   400, 1.5f, 10),
        new Row(Kind.Oncoming, "R",   424, 6f),
        new Row(Kind.Oncoming, "M",   428, 6f),
        new Row(Kind.Barrier,  "LMR", 436),
        new Row(Kind.CoinArch, "LMR", 436),
    };

    [SerializeField] float laneWidth = 2.5f;
    [SerializeField] bool drawPreview = true;

    [Header("Prefabs")]
    [SerializeField] GameObject barrierPrefab;
    [SerializeField] GameObject trainPrefab;
    [SerializeField] GameObject oncomingPrefab;
    [SerializeField] GameObject coinPrefab;
    [SerializeField] GameObject heartPrefab;
    [SerializeField] GameObject magnetPrefab;
    [SerializeField] GameObject springPrefab;
    [SerializeField] GameObject shieldPrefab;

    const float CoinY = 0.5f;
    const float PickupY = 0.9f;
    const int ArchCount = 5;
    const float ArchSpacing = 1.4f;
    const float ArchHeight = 1.2f;

    void Start()
    {
        foreach (Row row in Rows)
            foreach (char lane in row.lanes)
                Spawn(row, LaneX(lane));
    }

    float LaneX(char lane) => lane == 'L' ? -laneWidth : lane == 'R' ? laneWidth : 0f;

    void Spawn(Row row, float x)
    {
        switch (row.kind)
        {
            case Kind.Barrier: PlaceGrounded(barrierPrefab, x, row.z, 0f); break;
            case Kind.Train: PlaceGrounded(trainPrefab, x, row.z, row.size); break;
            case Kind.Oncoming: PlaceGrounded(oncomingPrefab, x, row.z, row.size); break;
            case Kind.Heart: PlaceAt(heartPrefab, new Vector3(x, PickupY, row.z)); break;
            case Kind.Magnet: PlaceAt(magnetPrefab, new Vector3(x, PickupY, row.z)); break;
            case Kind.Spring: PlaceAt(springPrefab, new Vector3(x, PickupY, row.z)); break;
            case Kind.Shield: PlaceAt(shieldPrefab, new Vector3(x, PickupY, row.z)); break;
            default:
                foreach (Vector3 position in CoinPositions(row, x)) PlaceAt(coinPrefab, position);
                break;
        }
    }

    IEnumerable<Vector3> CoinPositions(Row row, float x)
    {
        bool arch = row.kind == Kind.CoinArch;
        int count = arch ? ArchCount : row.count;
        float spacing = arch ? ArchSpacing : row.size;
        float startZ = arch ? row.z - (count - 1) * spacing * 0.5f : row.z;
        for (int i = 0; i < count; i++)
        {
            float y = CoinY;
            if (arch) y += Mathf.Sin(i / (float)(count - 1) * Mathf.PI) * ArchHeight;
            yield return new Vector3(x, y, startZ + i * spacing);
        }
    }

    void PlaceGrounded(GameObject prefab, float x, float z, float length)
    {
        if (prefab == null) return;
        Vector3 position = new Vector3(x, prefab.transform.localScale.y * 0.5f, z);
        GameObject instance = Instantiate(prefab, position, Quaternion.identity, transform);
        if (length > 0f)
        {
            Vector3 scale = instance.transform.localScale;
            scale.z = length;
            instance.transform.localScale = scale;
        }
    }

    void PlaceAt(GameObject prefab, Vector3 position)
    {
        if (prefab != null) Instantiate(prefab, position, Quaternion.identity, transform);
    }

    void OnDrawGizmos()
    {
        if (!drawPreview) return;
        foreach (Row row in Rows)
            foreach (char lane in row.lanes)
            {
                float x = LaneX(lane);
                switch (row.kind)
                {
                    case Kind.Barrier:
                        Gizmos.color = new Color(1f, 0.55f, 0.1f);
                        Gizmos.DrawWireCube(new Vector3(x, 0.3f, row.z), new Vector3(2.1f, 0.6f, 0.4f));
                        break;
                    case Kind.Train:
                        Gizmos.color = new Color(0.3f, 0.6f, 1f);
                        Gizmos.DrawWireCube(new Vector3(x, 1.1f, row.z), new Vector3(2.1f, 2.2f, row.size));
                        break;
                    case Kind.Oncoming:
                        Gizmos.color = new Color(1f, 0.3f, 0.3f);
                        Gizmos.DrawWireCube(new Vector3(x, 1.1f, row.z), new Vector3(2.1f, 2.2f, row.size));
                        break;
                    case Kind.CoinLine:
                    case Kind.CoinArch:
                        Gizmos.color = new Color(1f, 0.85f, 0.2f);
                        foreach (Vector3 position in CoinPositions(row, x)) Gizmos.DrawWireSphere(position, 0.25f);
                        break;
                    default:
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawWireSphere(new Vector3(x, PickupY, row.z), 0.5f);
                        break;
                }
            }
    }
}