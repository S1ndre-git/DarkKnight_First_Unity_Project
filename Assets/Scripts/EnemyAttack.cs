using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public Transform attackPoint;
    public Vector2 attackSize = new Vector2(1.2f, 0.8f);
    public LayerMask playerLayer;
    public int damage = 1;

    public void Attack()
    {
        Debug.Log("Enemy Attack Called");

        Collider2D hitPlayer = Physics2D.OverlapBox(
            attackPoint.position,
            attackSize,
            0f,
            playerLayer
        );

        if (hitPlayer != null)
        {
            Debug.Log("Player Hit");

            PlayerHealthUI playerHealth = FindFirstObjectByType<PlayerHealthUI>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            else
            {
                Debug.Log("PlayerHealthUI not found");
            }
        }
        else
        {
            Debug.Log("No Player In Attack Box");
        }
    }

    private void OnDrawGizmos()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPoint.position, attackSize);
    }

    public void DoAttack()
{
    Collider2D hit = Physics2D.OverlapBox(
        attackPoint.position,
        attackSize,
        0f,
        playerLayer
    );

    if (hit != null)
    {
        Debug.Log("Player Hit");

        PlayerHealthUI playerHealth = hit.GetComponent<PlayerHealthUI>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage, transform);
        }
    }
}
}