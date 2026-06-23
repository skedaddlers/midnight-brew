using UnityEngine;

public class InteractableDoor : Interactable
{
    public string sceneToLoad = "Scene_Cafe";

    protected override void Update()
    {
        if(playerInside && Input.GetKeyDown(KeyCode.Space))
        {
            SceneLoader.LoadScene(sceneToLoad);

        }
    }

}