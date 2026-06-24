using UnityEngine;
using System.Collections;

public enum StoryProgress
{
    Opening,
    FirstBattleFinished,
    SecondDay,
    SecondBattleFinished,
    PrologueFinished
}

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance;

    public StoryProgress CurrentProgress = StoryProgress.Opening;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}