using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    public GameObject arrowPrefab;
    public Transform firePoint;
    public Transform player;

    [Header("Hit Effect")]
    public float hitShakeDuration = 0.12f;
    public float hitShakeAmount = 0.15f;
    public float hitFlashTime = 0.08f;

    public float detectionRange = 10f;
    public float shootRange = 6f;
    public float moveSpeed = 2f;
    public float shootInterval = 2f;

    [Header("Movement Delay")]
    public float moveResumeDelay = 0.5f;

    private float shootTimer;
    private float moveResumeTimer;
    private Rigidbody2D rb;
    private Animator animator;
    private EnemyHealth enemyHealth;

    private bool deathHandled = false;

    // ===== 新增：受击检测 =====
    private bool wasHurt = false;

    // ===== 新增：闪白支持（多SpriteRenderer）=====
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();

        // ===== 新增：抓所有SpriteRenderer =====
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }
    }

    void Update()
    {
        if (enemyHealth != null && enemyHealth.IsDead())
        {
            HandleDeath();
            return;
        }

        // ===== 新增：监听“刚受击” =====
        if (enemyHealth != null)
        {
            bool isHurtNow = enemyHealth.IsHurt();

            if (isHurtNow && !wasHurt)
            {
                PlayHitEffect();
            }

            wasHurt = isHurtNow;
        }

        if (enemyHealth != null && enemyHealth.IsHurt())
        {
            StopMoving();
            SetMoving(false);
            return;
        }

        if (player == null || arrowPrefab == null || firePoint == null)
        {
            StopMoving();
            SetMoving(false);
            return;
        }

        float distanceToPlayer = Mathf.Abs(player.position.x - transform.position.x);

        if (distanceToPlayer > detectionRange)
        {
            shootTimer = 0f;
            moveResumeTimer = 0f;
            StopMoving();
            SetMoving(false);
            return;
        }

        FacePlayer();

        if (distanceToPlayer > shootRange)
        {
            moveResumeTimer += Time.deltaTime;

            if (moveResumeTimer >= moveResumeDelay)
            {
                float dir = player.position.x > transform.position.x ? 1f : -1f;

                if (rb != null)
                    rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);

                SetMoving(true);
            }
            else
            {
                StopMoving();
                SetMoving(false);
            }

            shootTimer = 0f;
        }
        else
        {
            moveResumeTimer = 0f;

            StopMoving();
            SetMoving(false);

            shootTimer += Time.deltaTime;

            if (shootTimer >= shootInterval)
            {
                PlayAttackAnimation();
                shootTimer = 0f;
            }
        }
    }

    void PlayAttackAnimation()
    {
        if (deathHandled) return;

        if (animator != null)
            animator.SetTrigger("Attack");
    }

    public void FireArrow()
    {
        if (deathHandled) return;
        if (player == null || arrowPrefab == null || firePoint == null) return;

        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);

        float dir = (player.position.x - transform.position.x) > 0 ? 1f : -1f;

        ArrowProjectile arrowScript = arrow.GetComponent<ArrowProjectile>();
        if (arrowScript != null)
        {
            arrowScript.SetDirection(dir);
        }
    }

    void HandleDeath()
    {
        if (deathHandled) return;
        deathHandled = true;

        shootTimer = 0f;
        moveResumeTimer = 0f;
        StopMoving();
        SetMoving(false);

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
        }

        int deadLayer = LayerMask.NameToLayer("DeadEnemy");
        if (deadLayer != -1)
        {
            SetLayerRecursively(gameObject, deadLayer);
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        enabled = false;
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    void FacePlayer()
    {
        Vector3 scale = transform.localScale;

        if (player.position.x < transform.position.x)
            scale.x = -Mathf.Abs(scale.x);
        else
            scale.x = Mathf.Abs(scale.x);

        transform.localScale = scale;
    }

    void StopMoving()
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void SetMoving(bool moving)
    {
        if (animator != null)
            animator.SetBool("isMoving", moving);
    }

    // ===== 新增：受击效果 =====
    void PlayHitEffect()
    {
        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.Play("RangedEnemy_Idle", 0, 0f);
            animator.Update(0f);
        }

        StopAllCoroutines();
        StartCoroutine(HitEffectCoroutine());
    }

    System.Collections.IEnumerator HitEffectCoroutine()
    {
        Vector3 originalPos = transform.position;

        // 白闪（更明显一点）
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                spriteRenderers[i].color = new Color(1.5f, 1.5f, 1.5f, 1f); // 比纯白更亮
        }

        float timer = 0f;

        while (timer < hitShakeDuration)
        {
            float offsetX = Random.Range(-hitShakeAmount, hitShakeAmount);

            // 直接改 Transform（覆盖 Rigidbody）
            transform.position = new Vector3(
                originalPos.x + offsetX,
                originalPos.y,
                originalPos.z
            );

            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;

        yield return new WaitForSeconds(hitFlashTime);

        // 恢复颜色
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                spriteRenderers[i].color = originalColors[i];
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}