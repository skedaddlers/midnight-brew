using UnityEngine;
using Fungus;

public class InteractableChar : Interactable
{
    public string blockName;
    public Flowchart flowchart;

    protected override void Update()
    {
        if(playerInside && Input.GetKeyDown(KeyCode.Space))
        {
            if(flowchart != null)
            {
                flowchart.ExecuteBlock(blockName);
            }
        }
    }
    
}