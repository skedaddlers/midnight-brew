using UnityEngine;
using Fungus;

public class OutsideSceneController : MonoBehaviour
{
    public static OutsideSceneController Instance;

    public Flowchart flowchart;
    public string openingSceneName = "OutsideScene_Intro";
    public string afterFirstBattleSceneName = "OutsideScene_AfterFirstBattle";

    void Start()
    {
        switch (StoryManager.Instance.CurrentProgress)
        {
            case StoryProgress.Opening:
                flowchart.ExecuteBlock(openingSceneName);
                Debug.Log("Executing opening scene block: " + openingSceneName);
                break;
            case StoryProgress.FirstBattleFinished:
                flowchart.ExecuteBlock(afterFirstBattleSceneName);
                Debug.Log("Executing after first battle block: " + afterFirstBattleSceneName);
                break;
        }
    }
}