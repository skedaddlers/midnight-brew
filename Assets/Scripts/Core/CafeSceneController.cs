using UnityEngine;
using Fungus;

public class CafeSceneController : MonoBehaviour
{
    public static CafeSceneController Instance;

    public Flowchart flowchart;
    public string openingSceneName = "CafeScene_Intro";
    public string afterFirstBattleSceneName = "CafeScene_AfterFirstBattle";
    public string secondDaySceneName = "CafeScene_SecondDay";

    void Start()
    {
        switch (StoryManager.Instance.CurrentProgress)
        {
            case StoryProgress.Opening:
                flowchart.ExecuteBlock(openingSceneName);
                break;
            case StoryProgress.FirstBattleFinished:
                flowchart.ExecuteBlock(afterFirstBattleSceneName);
                break;
            case StoryProgress.SecondDay:
                flowchart.ExecuteBlock(secondDaySceneName);
                break;
        }
    }
}