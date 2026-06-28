using UnityEngine;
using Fungus;

public class CafeSceneController : MonoBehaviour
{
    public static CafeSceneController Instance;
    public GameObject player;

    public Flowchart flowchart;
    public string openingSceneName = "CafeScene_Intro";
    public string afterFirstBattleSceneName = "CafeScene_AfterFirstBattle";
    public string secondDaySceneName = "CafeScene_SecondDay";
    public string secondBattleFinishedSceneName = "CafeScene_SecondBattleFinished";

    public GameObject openingSceneGO;
    public GameObject afterFirstBattleSceneGO;
    public GameObject secondDaySceneGO;
    public GameObject secondBattleFinishedGO;

    public Transform spawnPointOpeningScene;
    public Transform spawnPointAfterFirstBattleScene;
    public Transform spawnPointSecondDayScene;
    public Transform spawnPointSecondBattleFinishedScene;

    void Start()
    {
        switch (StoryManager.Instance.CurrentProgress)
        {
            case StoryProgress.Opening:
                SetActiveScene(openingSceneGO);
                flowchart.ExecuteBlock(openingSceneName);
                TeleportPlayerToSpawnPoint(spawnPointOpeningScene);
                break;
            case StoryProgress.FirstBattleFinished:
                SetActiveScene(afterFirstBattleSceneGO);
                flowchart.ExecuteBlock(afterFirstBattleSceneName);
                TeleportPlayerToSpawnPoint(spawnPointAfterFirstBattleScene);
                break;
            case StoryProgress.SecondDay:
                SetActiveScene(secondDaySceneGO);
                flowchart.ExecuteBlock(secondDaySceneName);
                TeleportPlayerToSpawnPoint(spawnPointSecondDayScene);
                break;
            case StoryProgress.SecondBattleFinished:
                SetActiveScene(secondBattleFinishedGO);
                flowchart.ExecuteBlock(secondBattleFinishedSceneName);
                TeleportPlayerToSpawnPoint(spawnPointSecondBattleFinishedScene);
                break;
        }
    }

    void TeleportPlayerToSpawnPoint(Transform spawnPoint)
    {
        if (player != null && spawnPoint != null)
        {
            player.transform.position = spawnPoint.position;
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