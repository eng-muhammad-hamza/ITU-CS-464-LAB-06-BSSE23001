using UnityEngine;

public class OncomingTrain : MonoBehaviour
{
    [SerializeField] float speed = 6f;
    [SerializeField] float activateDistance = 25f;
    Vector3 startPosition;
    Transform player;
    bool moving;

    void Awake() => startPosition = transform.position;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
    }

    void Update()
    {
        if (player == null) return;

        if (player.position.z < startPosition.z - activateDistance - 3f)
        {
            moving = false;
            transform.position = startPosition;
            return;
        }
        if (!moving && transform.position.z - player.position.z < activateDistance) moving = true;
        if (moving) transform.position += Vector3.back * (speed * Time.deltaTime);
    }
}