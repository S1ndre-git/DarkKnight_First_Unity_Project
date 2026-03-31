using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Attack Settings")]
    public float attackCooldown = 2f;
    private float attackTimer;
    public Transform player;

    [Header("Chase Settings")]
    public float moveSpeed = 2f;
    public float stopDistance = 1.5f;
    public float detectionRange = 6f;

    [Header("Patrol Settings")]
    public float patrolSpeed = 1f;
    public float patrolDistance = 2f;
    public float patrolWaitTime = 1.5f;

    private Vector3 startPoint;
    private bool movingRight = true;


    void Start()
    {
        startPoint = transform.position;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        EnemyHealth enemyHealth = GetComponent<EnemyHealth>();

if (enemyHealth != null && enemyHealth.IsDead())
{
    if (rb != null)
        rb.linearVelocity = Vector2.zero;

    if (anim != null)
        anim.SetBool("isWalking", false);

    return;
}

if (enemyHealth != null && enemyHealth.IsHurt())
{
    if (anim != null)
    {
        anim.SetBool("isWalking", false);
    }
    return;
}
        if (player == null)
        {
            if (rb != null)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (anim != null)
            {
                anim.SetBool("isWalking", false);
            }

            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer(distanceToPlayer);
        }
        else
        {
            Patrol();
        }
    }

    void ChasePlayer(float distanceToPlayer)
    {
        if (distanceToPlayer > stopDistance)
        {
            float dir = player.position.x > transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);

            if (anim != null)
            {
                anim.SetBool("isWalking", true);
            }

            attackTimer = 0;
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (anim != null)
            {
                anim.SetBool("isWalking", false);
            }

            attackTimer += Time.deltaTime;

            if (attackTimer >= attackCooldown)
            {
                Attack();
                attackTimer = 0;
            }
        }

        FaceTarget(player.position);
    }

    void Patrol()
    {
        float leftLimit = startPoint.x - patrolDistance;
        float rightLimit = startPoint.x + patrolDistance;

        if (movingRight)
        {
            rb.linearVelocity = new Vector2(patrolSpeed, rb.linearVelocity.y);

            if (anim != null)
            {
                anim.SetBool("isWalking", true);
            }

            if (transform.position.x >= rightLimit)
            {
                transform.position = new Vector3(rightLimit, transform.position.y, transform.position.z);
                movingRight = false;
            }

            FaceTarget(transform.position + Vector3.right);
        }
        else
        {
            rb.linearVelocity = new Vector2(-patrolSpeed, rb.linearVelocity.y);

            if (anim != null)
            {
                anim.SetBool("isWalking", true);
            }

            if (transform.position.x <= leftLimit)
            {
                transform.position = new Vector3(leftLimit, transform.position.y, transform.position.z);
                movingRight = true;
            }

            FaceTarget(transform.position + Vector3.left);
        }
    }

    void FaceTarget(Vector3 target)
    {
        if (target.x > transform.position.x)
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
        else if (target.x < transform.position.x)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.green;
        Vector3 patrolCenter = Application.isPlaying ? startPoint : transform.position;
        Gizmos.DrawLine(
            new Vector3(patrolCenter.x - patrolDistance, patrolCenter.y, patrolCenter.z),
            new Vector3(patrolCenter.x + patrolDistance, patrolCenter.y, patrolCenter.z)
        );
    }

    void Attack()
    {
        Debug.Log("Enemy Attack Called");

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }
    }
}