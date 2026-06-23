using UnityEngine;
using Fungus;

public class CafeSceneController : MonoBehaviour
{
    public static CafeSceneController Instance;

    public Flowchart flowchart;

    void Start()
    {
        switch (StoryManager.Instance.CurrentProgress)
        {
            case StoryProgress.Opening:
                flowchart.ExecuteBlock("CafeScene_Intro");
                break;
        }
    }
}