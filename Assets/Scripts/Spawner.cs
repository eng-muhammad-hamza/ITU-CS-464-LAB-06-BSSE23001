using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Play area and placement rules")]
    [SerializeField] Transform player;
    [SerializeField] float area = 9f;
    [SerializeField] float minDistanceFromPlayer = 2f;
    [SerializeField] float hazardClearance = 0.8f;
    [SerializeField] float minDistanceBetweenPickups = 1.5f;

    [Header("Coins")]
    [SerializeField] GameObject coinPrefab;
    [SerializeField] float interval = 1.5f;
    [SerializeField] int maxCoins = 5;

    [Header("Power-ups (the type is chosen at random)")]
    [SerializeField] GameObject[] powerUpPrefabs;
    [SerializeField] float powerUpInterval = 8f;
    [SerializeField] int maxPowerUps = 2;
    [SerializeField] float powerUpLifetimeSeconds = 15f;
    [SerializeField] float powerUpHeight = 0.8f;

    [Header("Bonus (optional)")]
    [SerializeField] GameObject redCoinPrefab;

    const int MaxPlacementAttempts = 30;
    const float CoinHeight = 0.5f;

    Transform coinContainer;
    Transform powerUpContainer;
    SpawnBlocker[] spawnBlockers;

    void Start()
    {
        if (player == null || coinPrefab == null)
        {
            Debug.LogError("Spawner needs Player and Coin Prefab assigned.", this);
            enabled = false;
            return;
        }

        coinContainer = CreateContainer("SpawnedCoins");
        powerUpContainer = CreateContainer("SpawnedPowerUps");
        spawnBlockers = FindObjectsByType<SpawnBlocker>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        InvokeRepeating(nameof(SpawnCoin), 1f, Mathf.Max(0.1f, interval));
        if (HasAnyPowerUpPrefab())
            InvokeRepeating(nameof(SpawnPowerUp), powerUpInterval, Mathf.Max(0.1f, powerUpInterval));
    }

    Transform CreateContainer(string containerName)
    {
        Transform container = new GameObject(containerName).transform;
        container.SetParent(transform, false);
        return container;
    }

    bool HasAnyPowerUpPrefab()
    {
        if (powerUpPrefabs == null) return false;
        foreach (GameObject prefab in powerUpPrefabs)
            if (prefab != null) return true;
        return false;
    }

    void SpawnCoin()
    {
        if (coinContainer.childCount >= maxCoins) return;
        if (!TryFindFreePosition(CoinHeight, out Vector3 spawnPosition)) return;
        Instantiate(coinPrefab, spawnPosition, Quaternion.identity, coinContainer);
    }

    void SpawnPowerUp()
    {
        if (powerUpContainer.childCount >= maxPowerUps) return;
        GameObject chosenPrefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
        if (chosenPrefab == null) return;
        if (!TryFindFreePosition(powerUpHeight, out Vector3 spawnPosition)) return;
        GameObject spawnedPowerUp = Instantiate(chosenPrefab, spawnPosition, Quaternion.identity, powerUpContainer);
        if (powerUpLifetimeSeconds > 0f) Destroy(spawnedPowerUp, powerUpLifetimeSeconds);
    }

    public void SpawnRed()
    {
        if (redCoinPrefab == null || player == null) return;
        if (!TryFindFreePosition(CoinHeight, out Vector3 spawnPosition)) return;
        Instantiate(redCoinPrefab, spawnPosition, Quaternion.identity);
    }

    bool TryFindFreePosition(float height, out Vector3 freePosition)
    {
        for (int attempt = 0; attempt < MaxPlacementAttempts; attempt++)
        {
            Vector3 candidate = new Vector3(Random.Range(-area, area), height, Random.Range(-area, area));
            if (!IsFreeSpot(candidate)) continue;
            freePosition = candidate;
            return true;
        }
        freePosition = default;
        return false;
    }

    bool IsFreeSpot(Vector3 candidate)
    {
        if (FlatDistance(candidate, player.position) < minDistanceFromPlayer) return false;

        foreach (SpawnBlocker blocker in spawnBlockers)
            if (blocker != null && blocker.Covers(candidate, hazardClearance)) return false;

        return !IsTooCloseToAnyChild(coinContainer, candidate)
            && !IsTooCloseToAnyChild(powerUpContainer, candidate);
    }

    bool IsTooCloseToAnyChild(Transform container, Vector3 candidate)
    {
        foreach (Transform child in container)
            if (FlatDistance(candidate, child.position) < minDistanceBetweenPickups) return true;
        return false;
    }

    static float FlatDistance(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }
}