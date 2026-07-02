using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    private Rigidbody2D rb;
    private FourDirectionSprite directionSprite;
    private Vector2 inputDirection;
    private bool canMove = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        directionSprite = GetComponent<FourDirectionSprite>();
    }

    void Update()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        // 1. Get raw input instantly every frame
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        
        inputDirection = new Vector2(moveX, moveY).normalized;
        directionSprite?.SetDirection(inputDirection);
    }

    void FixedUpdate()
    {
        // 2. Apply snappy velocity changes on the physics loop
        if (canMove)
        {
            rb.linearVelocity = new Vector2(inputDirection.x * moveSpeed, inputDirection.y * moveSpeed);
        }
    }

    public void SetMovement(bool value)
    {
        canMove = value;
    }

    public void EnableMovement()
    {
        canMove = true;
    }

    public void DisableMovement()
    {
        canMove = false;
        inputDirection = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

}
