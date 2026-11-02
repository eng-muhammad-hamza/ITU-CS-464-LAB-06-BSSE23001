using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject coinPrefab;
    [SerializeField] Transform player;
    [SerializeField] float interval = 1.5f;
    [SerializeField] int maxCoins = 5;
    [SerializeField] float area = 9f;
    [SerializeField] GameObject redCoinPrefab;

    void Start()
    {
        if (coinPrefab == null || player == null)
        {
            Debug.LogError("Spawner needs Coin Prefab and Player assigned.", this);
            enabled = false;
            return;
        }
        InvokeRepeating(nameof(SpawnCoin), 1f, interval);
    }

    void SpawnCoin()
    {
        if (transform.childCount >= maxCoins) return;
        float x = Random.Range(-area, area);
        float z = Random.Range(-area, area);
        Vector3 pos = new Vector3(x, 0.5f, z);
        if (Vector3.Distance(pos, player.position) < 2f) return;
        Instantiate(coinPrefab, pos, Quaternion.identity, transform);
    }

    public void SpawnRed()
    {
        if (redCoinPrefab == null || player == null) return;
        for (int i = 0; i < 20; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-area, area), 0.5f, Random.Range(-area, area));
            if (Vector3.Distance(pos, player.position) < 2f) continue;
            Instantiate(redCoinPrefab, pos, Quaternion.identity);
            return;
        }
    }
}