using UnityEngine;
using Fungus;

public class GardenBattleController : MonoBehaviour
{
    public static GardenBattleController Instance;

    public Flowchart flowchart;
    public string firstBlockName = "GardenBattle_Intro";
    public string secondBlockName = "Garden_Fight2";
    void Start()
    {
        AudioManager.Instance?.PlayBattleMusic();
        switch (StoryManager.Instance.CurrentProgress)
        {
            case StoryProgress.Opening:
                flowchart.ExecuteBlock(firstBlockName);
                break;
            case StoryProgress.SecondDay:
                flowchart.ExecuteBlock(secondBlockName);
                break;
        }
    }
}