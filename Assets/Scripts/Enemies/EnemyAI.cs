using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection & Attack Settings")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Dash Attack Settings")]
    [SerializeField] private float windUpTime = 1f;
    [SerializeField] private float dashSpeed = 8f;
    [SerializeField] private float dashDuration = 0.5f;

    [Header("References")]
    [SerializeField] private GameObject warningUIPrefab;
    [SerializeField] private GameObject damageCollider;
    [SerializeField] private Transform spawnAttack;
    [SerializeField] private GameObject trailSpawn;

    private Transform player;
    private bool isAttacking = false;
    private Rigidbody2D rb;
    private Vector2 lastRoamDirection;
    private Coroutine roamingCoroutine;

    private Vector2 savedDirection; // Save attack direction
    private GameObject warningUIInstance;

    private enum State
    {
        Roaming,
        Chasing,
        Attacking
    }

    private State state;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        warningUIInstance = Instantiate(warningUIPrefab, spawnAttack.position, Quaternion.identity);
        warningUIInstance.SetActive(false);
        damageCollider.SetActive(false); // Ensure it's disabled at start
    }

    private void Start()
    {
        state = State.Roaming;
        roamingCoroutine = StartCoroutine(RoamingRoutine());
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius)
        {
            if (state != State.Chasing)
            {
                state = State.Chasing;
                StopRoaming();
            }

            if (!isAttacking)
            {
                HandleAttackAI(distanceToPlayer);
            }
        }
        else if (state == State.Chasing)
        {
            state = State.Roaming;
            StartRoaming();
        }

        // Update `SpawnAttack` to face the saved direction
        if (savedDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(savedDirection.y, savedDirection.x) * Mathf.Rad2Deg;
            spawnAttack.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void HandleAttackAI(float distance)
    {
        if (distance > attackRange)
        {
            MoveTowards(player.position);
        }
        else if (!isAttacking)
        {
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        Debug.Log("Enemy wind-up!");

        // Step 1: Stop movement and save attack direction
        rb.linearVelocity = Vector2.zero;
        savedDirection = (player.position - transform.position).normalized;

        // Step 2: Position `SpawnAttack` at the offset in saved direction
        float offsetDistance = 1.5f;
        spawnAttack.position = transform.position + (Vector3)savedDirection * offsetDistance;

        // Step 3: Ensure `spawnAttack` stays with the enemy during knockback
        spawnAttack.SetParent(transform, true);

        // **FIX: Ensure we are NOT creating a new warning UI, just using the existing one**
        if (warningUIInstance != null)
        {
            warningUIInstance.transform.SetParent(spawnAttack, false);
            warningUIInstance.transform.position = spawnAttack.position;

            // Set Rotation to Face Attack Direction
            float angle = Mathf.Atan2(savedDirection.y, savedDirection.x) * Mathf.Rad2Deg;
            warningUIInstance.transform.rotation = Quaternion.Euler(0, 0, angle);

            warningUIInstance.SetActive(true);
        }
        else
        {
            Debug.LogError("Warning UI is missing! Make sure it is assigned in the inspector.");
        }

        // Step 4: Wind-up Delay
        yield return new WaitForSeconds(windUpTime);

        // Step 5: Hide Warning UI and Start Attack
        if (warningUIInstance != null)
        {
            warningUIInstance.SetActive(false);
        }

        damageCollider.SetActive(true);
        trailSpawn.SetActive(true);

        float dashTime = 0f;
        Vector2 stopPosition = (Vector2)transform.position + savedDirection * 1.5f; // Hardcoded stop distance

        while (dashTime < dashDuration)
        {
            rb.linearVelocity = savedDirection * dashSpeed;
            dashTime += Time.deltaTime;

            if (Vector2.Distance(transform.position, stopPosition) <= 0.1f) // Stop when close enough
                break;

            yield return null;
        }

        // Ensure final position is exactly at the stop position
        rb.linearVelocity = Vector2.zero;
        rb.position = stopPosition;

        trailSpawn.SetActive(false);
        damageCollider.SetActive(false);

        yield return new WaitForSeconds(attackCooldown); // Cooldown before another attack
        isAttacking = false;
    }



    public Transform GetAttackCollider()
    {
        return damageCollider.transform;
    }

    // Movement Functions
    private void MoveTowards(Vector2 target)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    // Roaming Behavior
    private IEnumerator RoamingRoutine()
    {
        while (state == State.Roaming)
        {
            Vector2 roamPosition = GetRoamingPosition();
            MoveTowards(roamPosition);
            yield return new WaitForSeconds(2f);
        }
    }

    private void StartRoaming()
    {
        if (roamingCoroutine != null)
            StopCoroutine(roamingCoroutine);

        roamingCoroutine = StartCoroutine(RoamingRoutine());
    }

    private void StopRoaming()
    {
        if (roamingCoroutine != null)
        {
            StopCoroutine(roamingCoroutine);
            roamingCoroutine = null;
        }

        rb.linearVelocity = Vector2.zero;
    }

    private Vector2 GetRoamingPosition()
    {
        lastRoamDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        return (Vector2)transform.position + lastRoamDirection * 2f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
