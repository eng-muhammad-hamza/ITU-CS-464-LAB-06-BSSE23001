using UnityEngine;

public class RedCoin : MonoBehaviour
{
    [SerializeField] GameObject blueCoinPrefab;
    [SerializeField] int count = 12;
    [SerializeField] Vector2 xRange = new Vector2(-7.25f, 7.25f);
    [SerializeField] Vector2 zRange = new Vector2(-7.25f, 7.25f);
    [SerializeField] float spinSpeed = 90f;

    void Update() => transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || GameManager.Instance == null) return;

        GameManager.Instance.SetBonusTotal(count);
        for (int i = 0; i < count; i++)
        {
            Vector3 position = new Vector3(Random.Range(xRange.x, xRange.y), 0.5f, Random.Range(zRange.x, zRange.y));
            Instantiate(blueCoinPrefab, position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}