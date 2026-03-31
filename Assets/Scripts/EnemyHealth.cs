using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{


    
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Hit Reaction")]
    public float knockbackForce = 2.8f;
    public float knockbackDuration = 0.12f;
    public float hitShakeDuration = 0.05f;
    public float hitShakeAmount = 0.04f;
    

    private Rigidbody2D rb;
    private Animator anim;
    private bool isHurt = false;
    private bool isDead = false;
    private Vector3 originalPosition;
    public float deathDestroyDelay = 1.0f;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        Debug.Log(gameObject.name + " HP initialized: " + currentHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isHurt || isDead) return;

        currentHealth -= damage;
        Debug.Log(gameObject.name + " took " + damage + " damage. HP left: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }
        if (anim != null)
{
            anim.SetTrigger("Hit");
}

        StartCoroutine(HitReaction());
    }

    IEnumerator HitReaction()
    {
        isHurt = true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        float knockDir = 1f;

        if (player != null)
        {
            if (player.transform.position.x < transform.position.x)
                knockDir = 1f;    // 玩家在左，敌人往右退
            else
                knockDir = -1f;   // 玩家在右，敌人往左退
        }

        originalPosition = transform.position;

        float shakeTimer = 0f;
        while (shakeTimer < hitShakeDuration)
        {
            float offsetX = Random.Range(-hitShakeAmount, hitShakeAmount);

            transform.position = new Vector3(
                originalPosition.x + offsetX,
                transform.position.y,
                transform.position.z
            );

            shakeTimer += Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(
            originalPosition.x,
            transform.position.y,
            transform.position.z
        );

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(knockDir * knockbackForce, rb.linearVelocity.y);
        }

        yield return new WaitForSeconds(knockbackDuration);

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        isHurt = false;
    }

    public bool IsHurt()
{
    return isHurt;
}

    public bool IsDead()
{
    return isDead;
}

   void Die()
{
    if (isDead) return;
    isDead = true;

    Debug.Log(gameObject.name + " died.");

    if (anim != null)
    {
        anim.SetTrigger("Die");
    }

    if (rb != null)
{
    rb.linearVelocity = Vector2.zero;
    rb.angularVelocity = 0f;
    rb.bodyType = RigidbodyType2D.Kinematic;
}

    gameObject.layer = LayerMask.NameToLayer("DeadEnemy");

    Destroy(gameObject, deathDestroyDelay);
}
}