using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 冲刺墙体检测
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckPadding = 0.02f;

    // 空中冲刺
    private bool hasAirDodged = false;   // 是否已经在空中冲刺过

    private bool isRecoiling = false;
    private bool isHurt = false;

    [Header("Collision Layers")]
    private int playerLayerIndex;
    private int enemyLayerIndex;
    private CameraFollowLookAhead cameraFollow;

    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public int maxJumpCount = 1;

    private int jumpCount;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private float moveInput;
    [SerializeField] private Animator animator;

    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;

    private bool isGrounded;
    private bool wasGrounded;

    private bool isFacingRight = true;

    [Header("Dodge")]
    public float dodgeDistance = 2f;
    public float dodgeDuration = 0.18f;
    public float dodgeCooldown = 0.6f;

   
    [SerializeField] private GameObject visualObject;
    [SerializeField] private Vector2 dashEffectOffset = new Vector2(0f, -0.5f);

    private bool isDodging = false;
    private bool canDodge = true;
    private bool isInvincible = false;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        jumpCount = maxJumpCount;
        cameraFollow = Camera.main.GetComponent<CameraFollowLookAhead>();

        playerLayerIndex = LayerMask.NameToLayer("Player");
        enemyLayerIndex = LayerMask.NameToLayer("Enemy");
    }

    void Update()
    {
        if (isRecoiling)
        {
            wasGrounded = isGrounded;
        }

        if (isHurt)
        {
            wasGrounded = isGrounded;
            return;
        }

        if (isDodging)
{
            if (animator != null)
    {
                animator.SetBool("isRunning", false);
                animator.SetBool("isJumping", false);
                animator.SetBool("isDashing", true);
    }

            wasGrounded = isGrounded;
            return;
}

        moveInput = Input.GetAxisRaw("Horizontal");
      
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            checkRadius,
            groundLayer
        );
        PlayerAudio audio = GetComponentInChildren<PlayerAudio>();
if (audio != null)
{
    bool isMovingOnGround = moveInput != 0 && isGrounded && !isDodging && !isHurt && !isRecoiling;
    audio.TryPlayFootstep(isMovingOnGround);
}


        if (animator != null)
        {
            animator.SetBool("isRunning", moveInput != 0 && isGrounded);
            animator.SetBool("isJumping", !isGrounded);
        }

        if (isGrounded && !wasGrounded)
{
    jumpCount = maxJumpCount;
    hasAirDodged = false;

    
    if (audio != null)
    {
        audio.PlayLandSfx();
    }
}

        if (!isDodging && Input.GetKeyDown(KeyCode.K) && jumpCount > 0)
{
    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

  
    if (audio != null)
    {
        if (jumpCount == maxJumpCount)
        {
            audio.PlayJump1Sfx(); // 第一段跳
        }
        else
        {
            audio.PlayJump2Sfx(); // 第二段跳
        }
    }

    jumpCount--;
}

        // 闪避键 L
        if (Input.GetKeyDown(KeyCode.L) && canDodge && !isDodging)
{
   
    if (audio != null)
    {
        audio.PlayDashSfx();
    }

    if (isGrounded)
    {
        StartCoroutine(Dodge());
    }
    else if (!hasAirDodged)
    {
        hasAirDodged = true;
        StartCoroutine(Dodge());
    }
}

        // 朝向
        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }

        wasGrounded = isGrounded;
    }

    void FixedUpdate()
    {
        if (isRecoiling) return;
        if (isHurt) return;
        if (isDodging) return;

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    IEnumerator Dodge()
    {
        if (animator != null)
{
    animator.SetBool("isDashing", true);
}

        canDodge = false;
        isDodging = true;
        isInvincible = true;

        float dodgeDirection;

        // 有输入时按输入方向闪避；没输入时按当前朝向
        if (moveInput != 0)
        {
            dodgeDirection = Mathf.Sign(moveInput);
        }
        else
        {
            dodgeDirection = isFacingRight ? 1f : -1f;
        }

        Vector2 dashDir = new Vector2(dodgeDirection, 0f);

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        // 闪避期间忽略玩家和敌人的物理碰撞
        if (playerLayerIndex != -1 && enemyLayerIndex != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayerIndex, enemyLayerIndex, true);
        }

       
        // 高速短位移，不再用瞬移
        float elapsed = 0f;
        float movedDistance = 0f;
        float dashSpeed = dodgeDistance / dodgeDuration;

        RaycastHit2D[] castResults = new RaycastHit2D[4];
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.useLayerMask = true;
        contactFilter.layerMask = wallLayer;
        contactFilter.useTriggers = false;

        while (elapsed < dodgeDuration)
        {
            float stepDistance = dashSpeed * Time.fixedDeltaTime;
            float remainingDistance = dodgeDistance - movedDistance;
            if (remainingDistance <= 0f)
            {
                break;
            }

            stepDistance = Mathf.Min(stepDistance, remainingDistance);

            float allowedDistance = stepDistance;
            bool hitWall = false;

            if (playerCollider != null)
            {
                int hitCount = playerCollider.Cast(
                    dashDir,
                    contactFilter,
                    castResults,
                    stepDistance + wallCheckPadding
                );

                if (hitCount > 0)
                {
                    float nearestDistance = castResults[0].distance;

                    for (int i = 1; i < hitCount; i++)
                    {
                        if (castResults[i].distance < nearestDistance)
                        {
                            nearestDistance = castResults[i].distance;
                        }
                    }

                    allowedDistance = Mathf.Max(0f, nearestDistance - wallCheckPadding);
                    hitWall = true;
                }
            }

            if (allowedDistance > 0f)
            {
                rb.MovePosition(rb.position + dashDir * allowedDistance);
                movedDistance += allowedDistance;
            }

            if (hitWall)
            {
                break;
            }

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;

        isDodging = false;
        isInvincible = false;

        if (animator != null)
{
    animator.SetBool("isDashing", false);
}

        // 恢复玩家和敌人的物理碰撞
        if (playerLayerIndex != -1 && enemyLayerIndex != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayerIndex, enemyLayerIndex, false);
        }

        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;

        if (cameraFollow != null)
            cameraFollow.SetFacingDirection(isFacingRight ? 1 : -1);

        Debug.Log("Flip: " + (isFacingRight ? "Right" : "Left"));
    }

    public bool IsDodging()
    {
        return isDodging;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }

    public void SetHurt(bool hurt)
    {
        isHurt = hurt;
    }

    public bool IsHurt()
    {
        return isHurt;
    }

    public void SetRecoiling(bool recoiling)
    {
        isRecoiling = recoiling;
    }

    public bool IsRecoiling()
    {
        return isRecoiling;
    }
}