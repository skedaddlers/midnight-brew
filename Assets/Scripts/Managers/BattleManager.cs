using UnityEngine;
using Fungus;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    public Flowchart flowchart;
    public string afterBattleSceneName = "GardenScene";

    public void EndBattle()
    {
        switch (StoryManager.Instance.CurrentProgress)
        {
            case StoryProgress.Opening:
                StoryManager.Instance.CurrentProgress = StoryProgress.FirstBattleFinished;
                SceneLoader.LoadScene(afterBattleSceneName);
                break;
            case StoryProgress.SecondDay:
                StoryManager.Instance.CurrentProgress = StoryProgress.SecondBattleFinished;
                SceneLoader.LoadScene(afterBattleSceneName);
                break;
        }
    }
}