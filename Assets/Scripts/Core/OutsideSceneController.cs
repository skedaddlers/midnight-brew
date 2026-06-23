using UnityEngine;
using Fungus;

public class OutsideSceneController : MonoBehaviour
{
    public static OutsideSceneController Instance;

    public Flowchart flowchart;

    void Start()
    {
        switch (StoryManager.Instance.CurrentProgress)
        {
            case StoryProgress.Opening:
                flowchart.ExecuteBlock("OutsideScene_Intro");
                break;
        }
    }
}