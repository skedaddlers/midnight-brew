using UnityEngine;
using Fungus;

public class GardenBattleController : MonoBehaviour
{
    public static GardenBattleController Instance;

    public Flowchart flowchart;
    public string firstBlockName = "GardenBattle_Intro";
    public string afterFirstBattleBlockName = "GardenBattle_AfterFirstBattle";
    void Start()
    {
        switch (StoryManager.Instance.CurrentProgress)
        {
            case StoryProgress.Opening:
                flowchart.ExecuteBlock(firstBlockName);
                break;
            case StoryProgress.FirstBattleFinished:
                flowchart.ExecuteBlock(afterFirstBattleBlockName);
                break;
        }
    }
}