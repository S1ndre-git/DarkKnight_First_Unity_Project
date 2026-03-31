using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    [Header("Hit Stop")]
    public float hitStopDuration = 0.04f;

    [Header("Camera Shake")]
    public float shakeDuration = 0.08f;
    public float shakeMagnitude = 0.15f;

    [Header("Hit Recoil")]
    public float hitRecoilForce = 1.2f;
    public float hitRecoilDuration = 0.08f;

    private Rigidbody2D rb;
    // private bool isRecoiling = false;


    public Transform attackPoint;
    public Vector2 attackSize = new Vector2(1.2f, 0.6f);
    public LayerMask enemyLayers;
    public int attackDamage = 1;

    public float attackCooldown = 0.3f;
    private float lastAttackTime = -999f;


    void Update()
{
    if (Input.GetKeyDown(KeyCode.J) && Time.time >= lastAttackTime + attackCooldown)
    {
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null && playerController.IsDodging())
            return;

        animator.SetTrigger("Attack");
        lastAttackTime = Time.time;
    }
}

   public void Attack()
{
    PlayerController playerController = GetComponent<PlayerController>();
    if (playerController != null && playerController.IsDodging())
    {
        return;
    }

    Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(
        attackPoint.position,
        attackSize,
        0f,
        enemyLayers
    );

    Debug.Log("Attack! Hit count: " + hitEnemies.Length);

    bool hasHitEnemy = false;

    foreach (Collider2D enemy in hitEnemies)
    {
        Debug.Log("Hit " + enemy.name);

        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            hasHitEnemy = true;
            enemyHealth.TakeDamage(attackDamage);
            continue;
        }

        BossHurtBox hurtBox = enemy.GetComponent<BossHurtBox>();
        if (hurtBox != null)
        {
            hasHitEnemy = true;
            Debug.Log("Hit Boss HurtBox");
            hurtBox.TakeDamage(attackDamage);
            continue;
        }

        BossAI boss = enemy.GetComponent<BossAI>();
        if (boss != null)
        {
            hasHitEnemy = true;
            Debug.Log("Hit Boss Directly");
            boss.TakeDamage(attackDamage);
        }
    }

    PlayerAudio attackAudio = GetComponentInChildren<PlayerAudio>();
    if (attackAudio != null)
    {
        if (hasHitEnemy)
        {
            attackAudio.PlayHitSfx();

            StartCoroutine(CameraShake());//screen shake
        }
        else
        {
            attackAudio.PlaySwingSfx();
        }
    }
}



    IEnumerator HitRecoil()
{
    // isRecoiling = true;

    PlayerController playerController = GetComponent<PlayerController>();
    if (playerController != null)
    {
        playerController.SetRecoiling(true);
    }

    float recoilDir = transform.localScale.x > 0 ? -1f : 1f;

    if (rb != null)
    {
        rb.linearVelocity = new Vector2(recoilDir * hitRecoilForce, rb.linearVelocity.y);
    }

    yield return new WaitForSeconds(hitRecoilDuration);

    if (playerController != null)
    {
        playerController.SetRecoiling(false);
    }

    // isRecoiling = false;
}
    

    void OnDrawGizmos()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(attackPoint.position, attackSize);
    }
    void Start()
{
    rb = GetComponent<Rigidbody2D>();
    animator = transform.Find("Visual").GetComponent<Animator>();
}

    IEnumerator HitStop()
{
    float originalTimeScale = Time.timeScale;

    Time.timeScale = 0f;

    yield return new WaitForSecondsRealtime(hitStopDuration);

    Time.timeScale = originalTimeScale;
}

    public IEnumerator CameraShake()
{
    Transform cam = Camera.main.transform;

    Vector3 originalPos = cam.localPosition;

    float elapsed = 0f;

    while (elapsed < shakeDuration)
    {
        float x = Random.Range(-1f, 1f) * shakeMagnitude;
        float y = Random.Range(-1f, 1f) * shakeMagnitude;

        cam.localPosition = new Vector3(
            originalPos.x + x,
            originalPos.y + y,
            originalPos.z
        );

        elapsed += Time.deltaTime;
        yield return null;
    }

    cam.localPosition = originalPos;
}
}