using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string interactionPrompt = "Press Space to Interact";
    protected bool playerInside;


    protected virtual void Update()
    {
        if(playerInside && Input.GetKeyDown(KeyCode.Space))
        {
            // Handle interactable logic here
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            playerInside = true;
            UIManager.Instance.ShowInteractionText(interactionPrompt);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            playerInside = false;
            UIManager.Instance.HideInteractionText();
        }
    }

}