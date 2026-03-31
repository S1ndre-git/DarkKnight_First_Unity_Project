using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealthUI : MonoBehaviour
{


    [Header("Health Bar Transform")]
public Transform healthBarCanvas;

private Vector3 healthBarOriginalScale;
private Vector3 healthBarOriginalLocalPos;
    [Header("UI Fade")]
    public CanvasGroup healthCanvasGroup;
    public float showDuration = 3f;
    public float fadeSpeed = 2f;

    private float lastHitTime;
    private bool isFading = false;

    // 死亡判定
    [SerializeField] private Animator visualAnimator;
    private Animator animator;
    public GameObject gameOverText;
    private bool isDead = false;

    [Header("Health Bar")]
    public Image healthFill;
    public int maxHealth = 5;
    public int currentHealth = 5;

    [Header("Hit Reaction")]
    public float knockbackForce = 3.5f;
    public float knockbackDuration = 0.15f;
    public float hitShakeDuration = 0.08f;
    public float hitShakeAmount = 0.05f;
    public Color hurtColor = Color.red;
    public float hurtColorDuration = 0.1f;

    [Header("Death Collision")]
    public LayerMask enemyCollisionLayers;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isHurt = false;
    private Collider2D[] playerColliders;

    private PlayerAudio playerAudio;
    void Start()
    {
        
        if (healthBarCanvas != null)
{
    healthBarOriginalScale = healthBarCanvas.localScale;
    healthBarOriginalLocalPos = healthBarCanvas.localPosition;
}
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerColliders = GetComponentsInChildren<Collider2D>();
        playerAudio = GetComponentInChildren<PlayerAudio>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        if (currentHealth < 0)
            currentHealth = 0;

        if (healthCanvasGroup != null)
        {
            healthCanvasGroup.alpha = 0f;
        }

        UpdateHealthBar();
    }

    void LateUpdate()
{
    if (healthBarCanvas != null)
    {
        Vector3 scale = healthBarOriginalScale;
        Vector3 pos = healthBarOriginalLocalPos;

        if (transform.localScale.x < 0)
        {
            scale.x = -Mathf.Abs(healthBarOriginalScale.x);
            pos.x = -healthBarOriginalLocalPos.x;
        }
        else
        {
            scale.x = Mathf.Abs(healthBarOriginalScale.x);
            pos.x = healthBarOriginalLocalPos.x;
        }

        healthBarCanvas.localScale = scale;
        healthBarCanvas.localPosition = pos;
    }
}

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, null);
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        if (isDead) return;

        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null && playerController.IsInvincible())
        {
            return;
        }

        ShowHealthBar();

        currentHealth -= damage;

        if (playerAudio != null)
{
    playerAudio.PlayHurtSfx();
}

        Debug.Log("currentHealth after = " + currentHealth);

        if (currentHealth < 0)
            currentHealth = 0;

        UpdateHealthBar();

        if (currentHealth > 0 && visualAnimator != null)
        {
            visualAnimator.SetTrigger("Hit");
        }

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (!isHurt)
        {
            StartCoroutine(HitReaction(attacker));
        }
    }

    void ShowHealthBar()
    {
        if (healthCanvasGroup == null) return;

        healthCanvasGroup.alpha = 1f;
        lastHitTime = Time.time;

        if (!isFading)
        {
            StartCoroutine(HideHealthBar());
        }
    }

    IEnumerator HideHealthBar()
    {
        isFading = true;

        while (Time.time - lastHitTime < showDuration)
        {
            yield return null;
        }

        while (healthCanvasGroup != null && healthCanvasGroup.alpha > 0f)
        {
            if (Time.time - lastHitTime < showDuration)
            {
                isFading = false;
                yield break;
            }

            healthCanvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        if (healthCanvasGroup != null)
        {
            healthCanvasGroup.alpha = 0f;
        }

        isFading = false;
    }

    void UpdateHealthBar()
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    IEnumerator HitReaction(Transform attacker)
    {
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetHurt(true);
        }

        isHurt = true;

        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null)
{
        StartCoroutine(attack.CameraShake());
}

        float knockDir = -1f;

        if (attacker != null)
        {
            if (attacker.position.x < transform.position.x)
                knockDir = 1f;
            else
                knockDir = -1f;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = hurtColor;
        }

        Vector3 startPosition = transform.position;
        float shakeTimer = 0f;

        while (shakeTimer < hitShakeDuration)
        {
            float offsetX = Random.Range(-hitShakeAmount, hitShakeAmount);

            transform.position = new Vector3(
                startPosition.x + offsetX,
                transform.position.y,
                transform.position.z
            );

            shakeTimer += Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(
            startPosition.x,
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

        if (spriteRenderer != null)
        {
            yield return new WaitForSeconds(hurtColorDuration);
            spriteRenderer.color = originalColor;
        }

        if (playerController != null)
        {
            playerController.SetHurt(false);
        }

        isHurt = false;
    }

    void DisableEnemyCollisionsOnDeath()
    {
        if (playerColliders == null || playerColliders.Length == 0) return;

        Collider2D[] allColliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);

        foreach (Collider2D playerCol in playerColliders)
        {
            if (playerCol == null) continue;

            foreach (Collider2D otherCol in allColliders)
            {
                if (otherCol == null) continue;
                if (otherCol.transform.IsChildOf(transform)) continue;

                if (((1 << otherCol.gameObject.layer) & enemyCollisionLayers) != 0)
                {
                    Physics2D.IgnoreCollision(playerCol, otherCol, true);
                }
            }
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        DisableEnemyCollisionsOnDeath();

        if (visualAnimator != null)
        {
            visualAnimator.SetTrigger("Die");
        }

        if (gameOverText != null)
            gameOverText.SetActive(true);

        GetComponent<PlayerController>().enabled = false;
        GetComponent<PlayerAttack>().enabled = false;

        StartCoroutine(RestartGame());
    }

    IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}