using UnityEngine;

public class CameraFollowLookAhead : MonoBehaviour
{
    private int facingDirection = 1; // 1 = right, -1 = left
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Base Offset")]
    [SerializeField] private Vector3 baseOffset = new Vector3(0f, 1.5f, -10f);

    [Header("Look Ahead")]
    [SerializeField] private float lookAheadDistance = 3f;
    [SerializeField] private float lookAheadSmoothTime = 0.25f;

    [Header("Follow Smooth")]
    [SerializeField] private float followSmoothTime = 0.15f;

    private float currentLookAheadX;
    private float lookAheadVelocity;
    private Vector3 followVelocity;

    private bool isFacingRight = true;
    private SpriteRenderer targetSpriteRenderer;


    public void SetFacingDirection(int direction)
{
    if (direction != 0)
        facingDirection = direction > 0 ? 1 : -1;
}

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollowLookAhead: Target is not assigned.");
            return;
        }

        targetSpriteRenderer = target.GetComponent<SpriteRenderer>();

        Vector3 startPos = target.position + baseOffset;
        transform.position = startPos;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float targetLookAheadX = facingDirection * lookAheadDistance;
        // UpdateFacingDirection();
        // float targetLookAheadX = isFacingRight ? lookAheadDistance : -lookAheadDistance;

        currentLookAheadX = Mathf.SmoothDamp(
            currentLookAheadX,
            targetLookAheadX,
            ref lookAheadVelocity,
            lookAheadSmoothTime
        );

        Vector3 desiredPosition = target.position + baseOffset + new Vector3(currentLookAheadX, 0f, 0f);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref followVelocity,
            followSmoothTime
        );
    }

    private void UpdateFacingDirection()
    {
        if (targetSpriteRenderer != null)
        {
            isFacingRight = !targetSpriteRenderer.flipX;
            return;
        }

        Vector3 targetScale = target.localScale;
        if (targetScale.x > 0)
            isFacingRight = true;
        else if (targetScale.x < 0)
            isFacingRight = false;
    }
}