using UnityEngine;

public class CrusherTracker : MonoBehaviour
{
    [Header("Tracking Settings")]
    [SerializeField] private float trackSpeed = 2.2f;
    [SerializeField] private float arenaLimit = 10.5f;

    private Hazard crusher;
    private Transform player;

    private Vector3 startingPosition;

    private void Start()
    {
        startingPosition = transform.position;
        crusher = GetComponentInChildren<Hazard>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
        else Debug.LogWarning("CrusherTracker: Player not found. Check Player tag.");
    }

    private void Update()
    {
        if (player == null || crusher == null) return;
        if (!crusher.IsResting || FreezeEffect.Active) return;
        Vector3 target = new Vector3(
            Mathf.Clamp(player.position.x, -arenaLimit, arenaLimit),
            startingPosition.y,
            Mathf.Clamp(player.position.z, -arenaLimit, arenaLimit)
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            trackSpeed * Time.deltaTime
        );
    }
}
