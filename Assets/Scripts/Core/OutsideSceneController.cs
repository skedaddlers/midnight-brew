using UnityEngine;
using Fungus;

public class OutsideSceneController : MonoBehaviour
{
    public static OutsideSceneController Instance;

    public GameObject player;

    public Flowchart flowchart;
    public string openingSceneName = "OutsideScene_Intro";
    public string afterFirstBattleSceneName = "OutsideScene_AfterFirstBattle";

    public Transform spawnPointOpeningScene;
    public Transform spawnPointAfterFirstBattleScene;

    void Start()
    {
        switch (StoryManager.Instance.CurrentProgress)
        {
            case StoryProgress.Opening:
                flowchart.ExecuteBlock(openingSceneName);
                TeleportPlayerToSpawnPoint(spawnPointOpeningScene);
                break;
            case StoryProgress.FirstBattleFinished:
                flowchart.ExecuteBlock(afterFirstBattleSceneName);
                TeleportPlayerToSpawnPoint(spawnPointAfterFirstBattleScene);
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
}