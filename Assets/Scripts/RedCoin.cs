using UnityEngine;

public class RedCoin : MonoBehaviour
{
    [SerializeField] GameObject blueCoinPrefab;
    [SerializeField] int count = 50;
    [SerializeField] float radius = 5f;
    [SerializeField] float spinSpeed = 90f;
    [SerializeField] float limit = 7.25f;

    void Update() => transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager.Instance.SetBonusTotal(count);

        Vector3 center = other.transform.position;
        for (int i = 0; i < count; i++)
        {
            Vector2 r = Random.insideUnitCircle * radius;
            float x = Mathf.Clamp(center.x + r.x, -limit, limit);
            float z = Mathf.Clamp(center.z + r.y, -limit, limit);
            Instantiate(blueCoinPrefab, new Vector3(x, 0.5f, z), Quaternion.identity);
        }
        Destroy(gameObject);
    }
}