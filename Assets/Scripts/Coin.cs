using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] int scoreValue = 1;
    [SerializeField] float spinDegreesPerSecond = 120f;

    void Update()
    {
        transform.Rotate(0f, spinDegreesPerSecond * Time.deltaTime, 0f, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        GameManager.Instance.AddScore(scoreValue);
        Destroy(gameObject);
    }
}