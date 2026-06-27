using UnityEngine;
using Fungus;

public class CafeSceneController : MonoBehaviour
{
    public static CafeSceneController Instance;

    public Flowchart flowchart;
    public string openingSceneName = "CafeScene_Intro";
    public string afterFirstBattleSceneName = "CafeScene_AfterFirstBattle";
    public string secondDaySceneName = "CafeScene_SecondDay";
    public string secondBattleFinishedSceneName = "CafeScene_SecondBattleFinished";

    public GameObject openingSceneGO;
    public GameObject afterFirstBattleSceneGO;
    public GameObject secondDaySceneGO;
    public GameObject secondBattleFinishedGO;

    void Start()
    {
        switch (StoryManager.Instance.CurrentProgress)
        {
            case StoryProgress.Opening:
                SetActiveScene(openingSceneGO);
                flowchart.ExecuteBlock(openingSceneName);
                break;
            case StoryProgress.FirstBattleFinished:
                SetActiveScene(afterFirstBattleSceneGO);
                flowchart.ExecuteBlock(afterFirstBattleSceneName);
                break;
            case StoryProgress.SecondDay:
                SetActiveScene(secondDaySceneGO);
                flowchart.ExecuteBlock(secondDaySceneName);
                break;
            case StoryProgress.SecondBattleFinished:
                SetActiveScene(secondBattleFinishedGO);
                flowchart.ExecuteBlock(secondBattleFinishedSceneName);
                break;
        }
    }

    void SetActiveScene(GameObject sceneGO)
    {
        openingSceneGO.SetActive(false);
        afterFirstBattleSceneGO.SetActive(false);
        secondDaySceneGO.SetActive(false);
        secondBattleFinishedGO.SetActive(false);

        sceneGO.SetActive(true);
    }
}