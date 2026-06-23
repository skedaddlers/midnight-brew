using UnityEngine;
using Fungus;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Flowchart flowchart;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void OnSceneChanged()
    {
        flowchart = FindFirstObjectByType<Flowchart>();
    }
}