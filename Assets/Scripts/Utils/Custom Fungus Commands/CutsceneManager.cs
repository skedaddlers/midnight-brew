using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CutsceneManager : MonoBehaviour
{
    public List<SpriteRenderer> cutsceneSprites;

    public void SetActiveCutscene(int index)
    {
        for (int i = 0; i < cutsceneSprites.Count; i++)
        {
            cutsceneSprites[i].enabled = (i == index);
        }
    }

    public void SetActiveCutsceneWithNext(int index)
    {
        for (int i = 0; i < cutsceneSprites.Count; i++)
        {
            cutsceneSprites[i].enabled = (i == index) || (i == index + 1);
        }
    }

    public void DisableAllCutscenes()
    {
        foreach (var sprite in cutsceneSprites)
        {
            sprite.enabled = false;
        }
    }

}