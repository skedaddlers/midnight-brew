using UnityEngine;
using Fungus;
using System.Collections;

public class CharacterMover : MonoBehaviour
{
    public float moveSpeed = 3f;
    public bool IsMoving { get; private set; }
    private FourDirectionSprite directionSprite;

    private void Awake()
    {
        directionSprite = GetComponent<FourDirectionSprite>();
    }

    public IEnumerator MoveTo(Vector3 target)
    {
        IsMoving = true;

        Vector3 horizontalPoint =
            new Vector3(
                target.x,
                transform.position.y,
                transform.position.z);

        yield return MoveStraight(horizontalPoint);

        yield return MoveStraight(target);

        IsMoving = false;
    }

    IEnumerator MoveStraight(Vector3 target)
    {
        while(Vector3.Distance(transform.position,target) > 0.05f)
        {
            Vector3 movementDirection = target - transform.position;
            directionSprite?.SetDirection(movementDirection);

            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    target,
                    moveSpeed * Time.deltaTime);

            yield return null;
        }

        transform.position = target;
    }
}
