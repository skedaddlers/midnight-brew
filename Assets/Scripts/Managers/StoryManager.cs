using UnityEngine;
using System.Collections;

public enum StoryProgress
{
    Opening,
    GardenFinished,
    NightStarted,
    PrologueFinished
}

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance;

    public StoryProgress CurrentProgress = StoryProgress.Opening;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}