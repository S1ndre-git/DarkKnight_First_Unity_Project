using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    
    public float speed = 8f;
    public int damage = 1;
    public float lifeTime = 3f;

    private float direction = 1f;

    public void SetDirection(float dir)
    {
        direction = dir;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        transform.localScale = scale;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        
        transform.position += Vector3.right * direction * speed * Time.deltaTime;
    }

   private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.CompareTag("Player"))
    {
        PlayerHealthUI playerHealth = collision.GetComponent<PlayerHealthUI>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage, transform);
        }

        Destroy(gameObject);
    }
    else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
    {
        Destroy(gameObject);
    }
}
}