using UnityEngine;
using Fungus;

public class InteractableChar : Interactable
{
    protected override void Update()
    {
        if(isInteractable && playerInside && Input.GetKeyDown(KeyCode.Space))
        {
            if(flowchart != null)
            {
                flowchart.ExecuteBlock(blockName);
            }
        }
    }
    
}