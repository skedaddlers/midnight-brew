using UnityEngine;
using Fungus;

/// <summary>
/// Base class for all interactable objects in the game. Handles player interaction and triggering Fungus flowchart blocks.
/// </summary>
public class Interactable : MonoBehaviour
{
    public string interactionPrompt = "Press Space to Interact";
    public Flowchart flowchart;
    public string blockName = "YourBlockName";
    protected bool isInteractable = true;
    protected bool playerInside;


    protected virtual void Update()
    {
        if(isInteractable && playerInside && Input.GetKeyDown(KeyCode.Space))
        {
            if(flowchart != null)
            {
                flowchart.ExecuteBlock(blockName);
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if(isInteractable && other.CompareTag("Player"))
        {
            playerInside = true;
            UIManager.Instance.ShowInteractionText(interactionPrompt);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if(isInteractable && other.CompareTag("Player"))
        {
            playerInside = false;
            UIManager.Instance.HideInteractionText();
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        if(isInteractable && other.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    public void SetInteractable(bool value)
    {
        isInteractable = value;
        if(!isInteractable && playerInside)
        {
            UIManager.Instance.HideInteractionText();
        }
        if(isInteractable && playerInside)
        {
            UIManager.Instance.ShowInteractionText(interactionPrompt);
        }
    }
}