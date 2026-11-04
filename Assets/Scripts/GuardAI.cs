using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardAI : MonoBehaviour
{
    public enum State { Patrol, Spot, Chase, Search, Return, Stunned }

    [Header("Route")]
    [SerializeField] Transform[] waypoints;
    [SerializeField] float waitAtPoint = 0.8f;

    [Header("Speeds")]
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] float chaseSpeed = 4.5f;

    [Header("Senses")]
    [SerializeField] float sightRange = 8f;
    [SerializeField, Range(10f, 90f)] float sightHalfAngle = 50f;
    [SerializeField] float closeSenseRange = 1.6f;
    [SerializeField] float spotSeconds = 0.5f;
    [SerializeField] float searchSeconds = 3f;
    [SerializeField] float searchTurnSpeed = 140f;

    [Header("Visuals")]
    [SerializeField] Renderer bodyRenderer;
    [SerializeField] Light sightLight;

    [Header("Read Only State")]
    [SerializeField] State state;

    static readonly Color SpotColor = new Color(1f, 0.92f, 0.2f);
    static readonly Color SearchColor = new Color(1f, 0.6f, 0.15f);
    static readonly Color StunColor = new Color(0.4f, 0.5f, 0.65f);

    NavMeshAgent agent;
    Hazard contactHazard;
    Material bodyMaterial;
    Color baseColor = Color.white;
    Color lightBaseColor = Color.white;
    Transform player;
    Health playerHealth;
    int waypointIndex;
    bool waiting;
    bool wasSeeing;
    float waitUntil;
    float stateEndTime;
    float stunnedUntil;
    Vector3 lastKnownPosition;

    bool HasRoute => waypoints != null && waypoints.Length > 0;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        contactHazard = GetComponent<Hazard>();
        if (bodyRenderer != null)
        {
            bodyMaterial = bodyRenderer.material;
            baseColor = bodyMaterial.color;
        }
        if (sightLight != null) lightBaseColor = sightLight.color;
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerHealth = playerObject.GetComponent<Health>();
        }
        if (!HasRoute) Debug.LogWarning($"{name}: no waypoints assigned.", this);
        EnterState(State.Patrol);
    }

    void Update()
    {
        if (state == State.Stunned)
        {
            if (Time.time < stunnedUntil) return;
            if (contactHazard != null) contactHazard.enabled = true;
            EnterState(State.Return);
            return;
        }

        bool sees = CanSeePlayer();
        if (sees) lastKnownPosition = player.position;

        switch (state)
        {
            case State.Patrol: UpdatePatrol(sees); break;
            case State.Spot: UpdateSpot(sees); break;
            case State.Chase: UpdateChase(sees); break;
            case State.Search: UpdateSearch(sees); break;
            case State.Return: UpdateReturn(sees); break;
        }
    }

    void EnterState(State next)
    {
        state = next;
        switch (next)
        {
            case State.Patrol:
                agent.isStopped = false;
                agent.speed = patrolSpeed;
                waiting = false;
                if (HasRoute) agent.SetDestination(waypoints[waypointIndex].position);
                break;
            case State.Spot:
                agent.isStopped = true;
                stateEndTime = Time.time + spotSeconds;
                JuiceFx.FloatingText(transform.position + Vector3.up * 1.4f, "!", JuiceFx.DamageColor, 22f);
                break;
            case State.Chase:
                agent.isStopped = false;
                agent.speed = chaseSpeed;
                wasSeeing = true;
                break;
            case State.Search:
                agent.isStopped = true;
                stateEndTime = Time.time + searchSeconds;
                JuiceFx.FloatingText(transform.position + Vector3.up * 1.4f, "?", SearchColor, 22f);
                break;
            case State.Return:
                agent.isStopped = false;
                agent.speed = patrolSpeed;
                waypointIndex = NearestWaypoint();
                if (HasRoute) agent.SetDestination(waypoints[waypointIndex].position);
                break;
            case State.Stunned:
                agent.isStopped = true;
                break;
        }
        ApplyColors(next);
    }

    void UpdatePatrol(bool sees)
    {
        if (sees) { EnterState(State.Spot); return; }
        if (!HasRoute || !Arrived()) return;

        if (!waiting)
        {
            waiting = true;
            waitUntil = Time.time + waitAtPoint;
            return;
        }
        if (Time.time < waitUntil) return;

        waiting = false;
        waypointIndex = (waypointIndex + 1) % waypoints.Length;
        agent.SetDestination(waypoints[waypointIndex].position);
    }

    void UpdateSpot(bool sees)
    {
        if (sees) FaceTowards(player.position);
        if (Time.time >= stateEndTime) EnterState(State.Chase);
    }

    void UpdateChase(bool sees)
    {
        if (sees)
        {
            wasSeeing = true;
            agent.SetDestination(player.position);
            return;
        }
        if (wasSeeing)
        {
            wasSeeing = false;
            agent.SetDestination(lastKnownPosition);
            return;
        }
        if (Arrived()) EnterState(State.Search);
    }

    void UpdateSearch(bool sees)
    {
        if (sees) { EnterState(State.Spot); return; }
        transform.Rotate(0f, searchTurnSpeed * Time.deltaTime, 0f);
        if (Time.time >= stateEndTime) EnterState(State.Return);
    }

    void UpdateReturn(bool sees)
    {
        if (sees) { EnterState(State.Spot); return; }
        if (!HasRoute || Arrived()) EnterState(State.Patrol);
    }

    public void Stun(float seconds)
    {
        bool alreadyStunned = state == State.Stunned;
        stunnedUntil = Mathf.Max(stunnedUntil, Time.time + seconds);
        if (alreadyStunned) return;

        if (contactHazard != null) contactHazard.enabled = false;
        EnterState(State.Stunned);
        JuiceFx.Burst(transform.position, JuiceFx.EmpColor, 18, 4f, 0.15f, 0.5f);
        JuiceFx.FloatingText(transform.position + Vector3.up * 1.4f, "STUN", JuiceFx.EmpColor, 16f);
    }

    bool Arrived() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.15f;

    int NearestWaypoint()
    {
        int best = 0;
        float bestDistance = float.MaxValue;
        if (!HasRoute) return 0;
        for (int i = 0; i < waypoints.Length; i++)
        {
            float d = (waypoints[i].position - transform.position).sqrMagnitude;
            if (d < bestDistance) { bestDistance = d; best = i; }
        }
        return best;
    }

    void FaceTowards(Vector3 point)
    {
        Vector3 flat = point - transform.position;
        flat.y = 0f;
        if (flat.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(flat), 540f * Time.deltaTime);
    }

    bool CanSeePlayer()
    {
        if (player == null || playerHealth == null) return false;
        if (playerHealth.IsGhost || playerHealth.Current <= 0) return false;

        Vector3 eye = transform.position;
        eye.y = 0.5f;
        Vector3 goal = player.position;
        goal.y = 0.5f;
        Vector3 toPlayer = goal - eye;
        float distance = toPlayer.magnitude;

        if (distance > sightRange) return false;
        if (distance < 0.01f) return true;
        if (distance > closeSenseRange && Vector3.Angle(transform.forward, toPlayer) > sightHalfAngle) return false;

        if (Physics.Raycast(eye, toPlayer / distance, out RaycastHit hit, distance + 0.1f, ~0, QueryTriggerInteraction.Ignore))
            return hit.collider.CompareTag("Player");
        return false;
    }

    void ApplyColors(State s)
    {
        Color body = baseColor;
        Color beam = lightBaseColor;
        switch (s)
        {
            case State.Spot: body = beam = SpotColor; break;
            case State.Chase: body = beam = JuiceFx.DamageColor; break;
            case State.Search: body = beam = SearchColor; break;
            case State.Stunned: body = beam = StunColor; break;
        }
        if (bodyMaterial != null) bodyMaterial.color = body;
        if (sightLight != null)
        {
            sightLight.color = beam;
            sightLight.enabled = s != State.Stunned;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position;
        Gizmos.DrawLine(origin, origin + Quaternion.Euler(0f, -sightHalfAngle, 0f) * transform.forward * sightRange);
        Gizmos.DrawLine(origin, origin + Quaternion.Euler(0f, sightHalfAngle, 0f) * transform.forward * sightRange);
        Gizmos.DrawWireSphere(origin, closeSenseRange);
    }
}