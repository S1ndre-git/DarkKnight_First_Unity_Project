using UnityEngine;
using System.Collections;

public class BossAI : MonoBehaviour
{

    [Header("Victory")]
    public VictoryManager victoryManager;

    public Transform attackRoot;
    
    [Header("Attack")]
    public Transform attackPoint;
    public Vector2 attackSize = new Vector2(2f, 1.5f);
    public Transform attackPoint1;
    public Vector2 attackSize1 = new Vector2(2f, 1.5f);
    public Transform attackPoint2;
    public Vector2 attackSize2 = new Vector2(2f, 1.5f);
    public Transform attackPoint3;
    public Vector2 attackSize3 = new Vector2(2f, 1.5f);
    public Transform attackPoint4;
    public Vector2 attackSize4 = new Vector2(2f, 1.5f);
    public LayerMask playerLayer;
    public int attackDamage = 1;

    [Header("Move")]
    public float moveSpeed = 2f;

    [Header("Detection")]
    public Transform player;
    public float detectionRange = 8f;
    public float loseRange = 10f;
    public float stopDistance = 3f;

    [Header("AI")]
    public float attackCooldown = 1.2f;
    public float attack1LockTime = 1.0f;
    public float attack3LockTime = 1.0f;
    public float attack4LockTime = 1.2f;
    public float turnLockTime = 0.35f;

    [Header("Attack4 Roll")]
    public float attack4RollSpeed = 6f;

    [Header("Health")]
    public int maxHP = 20;
    private int currentHP;
    private bool isDead = false;

    [Header("Death")]
    public float destroyAfterDeath = 2f;

    [Header("Hit Flash")]
    public SpriteRenderer spriteRenderer;
    public float hitFlashDuration = 0.1f;
    private Color originalColor;
    private Coroutine hitFlashCoroutine;

    [Header("Hit Shake")]
    public float hitShakeDuration = 0.1f;
    public float hitShakeStrength = 0.08f;
    private Coroutine hitShakeCoroutine;
    private Vector3 visualOriginalLocalPosition;

    [Header("Refs")]
    public Rigidbody2D rb;
    public Animator animator;
    public Transform visual;
    public Collider2D hurtBoxCol;
    public GameObject hurtBoxObject;

    private float moveDir = 0f;
    private bool isChasing = false;
    private Collider2D col;

    private bool isActing = false;
    private float nextAttackTime = 0f;

    private bool isRollingAttack4 = false;

    private BossAudio bossAudio;

    

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (transform.Find("Visual") != null)
            visual = transform.Find("Visual");

        if (visual != null)
        {
            animator = visual.GetComponent<Animator>();
            spriteRenderer = visual.GetComponent<SpriteRenderer>();
        }

        if (hurtBoxObject == null && transform.Find("HurtBox") != null)
            hurtBoxObject = transform.Find("HurtBox").gameObject;

        if (hurtBoxCol == null && hurtBoxObject != null)
            hurtBoxCol = hurtBoxObject.GetComponent<Collider2D>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (attackPoint1 != null)
            Gizmos.DrawWireCube(attackPoint1.position, attackSize1);

        Gizmos.color = Color.yellow;
        if (attackPoint != null)
            Gizmos.DrawWireCube(attackPoint.position, attackSize);

        Gizmos.color = Color.cyan;
        if (attackPoint2 != null)
            Gizmos.DrawWireCube(attackPoint2.position, attackSize2);

        Gizmos.color = Color.green;
        if (attackPoint3 != null)
            Gizmos.DrawWireCube(attackPoint3.position, attackSize3);

        Gizmos.color = Color.magenta;
        if (attackPoint4 != null)
            Gizmos.DrawWireCube(attackPoint4.position, attackSize4);
    }

    private void Awake()
    {
        bossAudio = GetComponentInChildren<BossAudio>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (col == null)
            col = GetComponent<Collider2D>();

        if (visual == null && transform.Find("Visual") != null)
            visual = transform.Find("Visual");

        if (animator == null && visual != null)
            animator = visual.GetComponent<Animator>();

        if (spriteRenderer == null && visual != null)
            spriteRenderer = visual.GetComponent<SpriteRenderer>();

        if (hurtBoxObject == null && transform.Find("HurtBox") != null)
            hurtBoxObject = transform.Find("HurtBox").gameObject;

        if (hurtBoxCol == null && hurtBoxObject != null)
            hurtBoxCol = hurtBoxObject.GetComponent<Collider2D>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (visual != null)
            visualOriginalLocalPosition = visual.localPosition;

        currentHP = maxHP;
    }

    private void Update()
    {
        if (isDead) return;
        if (player == null) return;

        float distance = Vector2.Distance(player.position, transform.position);

        if (!isChasing && distance <= detectionRange)
        {
            isChasing = true;
        }
        else if (isChasing && distance >= loseRange)
        {
            isChasing = false;
        }

        if (isActing)
        {
            moveDir = 0f;
            UpdateAnimatorWalk();
            return;
        }

        if (isChasing)
        {
            if (distance > stopDistance)
            {
                moveDir = player.position.x > transform.position.x ? 1f : -1f;
            }
            else
            {
                moveDir = 0f;

                if (Time.time >= nextAttackTime)
                {
                    DecideAttackOrTurn();
                }
            }
        }
        else
        {
            moveDir = 0f;
        }

        UpdateAnimatorWalk();

        if (moveDir != 0)
        {
            SetFacing(moveDir);
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
            return;
        }

        if (rb != null)
        {
            if (isRollingAttack4)
            {
                float rollDir = visual != null && visual.localScale.x > 0f ? 1f : -1f;
                rb.linearVelocity = new Vector2(rollDir * attack4RollSpeed, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);
            }
        }
    }

    private void UpdateAnimatorWalk()
    {
        if (animator != null)
            animator.SetBool("isWalking", moveDir != 0);
    }

    private void SetFacing(float faceX)
    {
        faceX = faceX >= 0 ? 1f : -1f;

        if (visual != null)
        {
            Vector3 visualScale = visual.localScale;
            visualScale.x = Mathf.Abs(visualScale.x) * faceX;
            visual.localScale = visualScale;
        }

        if (attackRoot != null)
        {
            Vector3 attackScale = attackRoot.localScale;
            attackScale.x = Mathf.Abs(attackScale.x) * faceX;
            attackRoot.localScale = attackScale;
        }
    }

    private void DecideAttackOrTurn()
    {
        bool playerInFront = IsPlayerInFront();

        if (playerInFront)
        {
            float roll = Random.value;
            if (roll < 0.5f)
            {
                StartAttack("attack1", attack1LockTime);
            }
            else
            {
                StartAttack("attack4", attack4LockTime);
            }
        }
        else
        {
            float roll = Random.value;
            if (roll < 0.6f)
            {
                StartAttack("attack3", attack3LockTime);
            }
            else
            {
                StartCoroutine(TurnRoutine());
            }
        }
    }

    private bool IsPlayerInFront()
    {
        if (visual == null || player == null)
            return true;

        bool facingRight = visual.localScale.x > 0f;
        bool playerOnRight = player.position.x > transform.position.x;

        return facingRight == playerOnRight;
    }

    private void StartAttack(string triggerName, float lockTime)
    {
        if (animator == null) return;

        isActing = true;
        moveDir = 0f;
        UpdateAnimatorWalk();

        animator.ResetTrigger("attack1");
        animator.ResetTrigger("attack3");
        animator.ResetTrigger("attack4");
        animator.SetTrigger(triggerName);

        nextAttackTime = Time.time + attackCooldown;
        StartCoroutine(ActionLockRoutine(lockTime));
    }

    private IEnumerator TurnRoutine()
    {
        isActing = true;
        moveDir = 0f;
        UpdateAnimatorWalk();

        if (player != null)
        {
            float faceX = player.position.x > transform.position.x ? 1f : -1f;
            SetFacing(faceX);
        }

        nextAttackTime = Time.time + attackCooldown;
        yield return new WaitForSeconds(turnLockTime);

        isActing = false;
    }

    private IEnumerator ActionLockRoutine(float lockTime)
    {
        yield return new WaitForSeconds(lockTime);
        isActing = false;
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("Boss TakeDamage: " + damage);
        if (isDead) return;

        currentHP -= damage;

        FlashWhite();
        PlayHitShake();

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    public void DealDamage()
    {

        if (bossAudio != null)
    {
        bossAudio.PlayAttack1Sfx();
    }
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            attackPoint.position,
            attackSize,
            0f,
            playerLayer
        );

        foreach (Collider2D hit in hits)
        {
            PlayerHealthUI playerHealth = hit.GetComponentInParent<PlayerHealthUI>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    public void DealDamage1()
    {
        if (bossAudio != null)
    {
        bossAudio.PlayAttack2Sfx();
    }
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            attackPoint1.position,
            attackSize1,
            0f,
            playerLayer
        );

        foreach (Collider2D hit in hits)
        {
            PlayerHealthUI playerHealth = hit.GetComponentInParent<PlayerHealthUI>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    public void DealDamage2()
    {
        if (bossAudio != null)
    {
        bossAudio.PlayAttack3Sfx();
    }
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            attackPoint2.position,
            attackSize2,
            0f,
            playerLayer
        );

        foreach (Collider2D hit in hits)
        {
            PlayerHealthUI playerHealth = hit.GetComponentInParent<PlayerHealthUI>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    public void DealDamage3()
    {
        if (bossAudio != null)
    {
        bossAudio.PlayAttack4Sfx();
    }
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            attackPoint3.position,
            attackSize3,
            0f,
            playerLayer
        );

        foreach (Collider2D hit in hits)
        {
            PlayerHealthUI playerHealth = hit.GetComponentInParent<PlayerHealthUI>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    public void DealDamage4()
    {
        if (bossAudio != null)
    {
        bossAudio.PlayAttack5Sfx();
    }
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            attackPoint4.position,
            attackSize4,
            0f,
            playerLayer
        );

        foreach (Collider2D hit in hits)
        {
            PlayerHealthUI playerHealth = hit.GetComponentInParent<PlayerHealthUI>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    public void StartAttack4Roll()
    {
        isRollingAttack4 = true;
    }

    public void StopAttack4Roll()
    {
        isRollingAttack4 = false;

        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private void FlashWhite()
    {
        Debug.Log("FlashWhite called");
        if (spriteRenderer == null) return;

        if (hitFlashCoroutine != null)
            StopCoroutine(hitFlashCoroutine);

        hitFlashCoroutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(hitFlashDuration);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        hitFlashCoroutine = null;
    }

    private void PlayHitShake()
    {
        if (visual == null) return;

        if (hitShakeCoroutine != null)
        {
            StopCoroutine(hitShakeCoroutine);
            visual.localPosition = visualOriginalLocalPosition;
        }

        hitShakeCoroutine = StartCoroutine(HitShakeRoutine());
    }

    private IEnumerator HitShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < hitShakeDuration)
        {
            float offsetX = Random.Range(-hitShakeStrength, hitShakeStrength);
            float offsetY = Random.Range(-hitShakeStrength, hitShakeStrength);

            if (visual != null)
                visual.localPosition = visualOriginalLocalPosition + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (visual != null)
            visual.localPosition = visualOriginalLocalPosition;

        hitShakeCoroutine = null;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        moveDir = 0f;
        isChasing = false;
        isActing = false;
        isRollingAttack4 = false;

        if (hitShakeCoroutine != null)
        {
            StopCoroutine(hitShakeCoroutine);
            hitShakeCoroutine = null;
        }

        if (visual != null)
            visual.localPosition = visualOriginalLocalPosition;

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetTrigger("dead");
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (col != null)
            col.enabled = false;

        if (hurtBoxCol != null)
            hurtBoxCol.enabled = false;

        if (hurtBoxObject != null)
            hurtBoxObject.SetActive(false);

        if (victoryManager != null)
{
    victoryManager.StartVictorySequence();
}
else
{
    Debug.LogWarning("VictoryManager is not assigned on BossAI!");
}

        if (BGMManager.Instance != null)
{
    BGMManager.Instance.BossDefeated();
}

        Destroy(gameObject, destroyAfterDeath);
    }
}