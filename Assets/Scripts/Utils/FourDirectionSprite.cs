using UnityEngine;

public class FourDirectionSprite : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Directional Sprites")]
    [SerializeField] private Sprite spriteUp;
    [SerializeField] private Sprite spriteDown;
    [SerializeField] private Sprite spriteLeft;
    [SerializeField] private Sprite spriteRight;

    private Vector2 lastDirection = Vector2.down;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        ApplyDirection(lastDirection);
    }

    public void SetDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        lastDirection = direction;
        ApplyDirection(direction);
    }

    private void ApplyDirection(Vector2 direction)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        // The dominant axis determines the displayed direction on diagonal movement.
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            spriteRenderer.sprite = direction.x > 0f ? spriteRight : spriteLeft;
        }
        else
        {
            spriteRenderer.sprite = direction.y > 0f ? spriteUp : spriteDown;
        }
    }
}
