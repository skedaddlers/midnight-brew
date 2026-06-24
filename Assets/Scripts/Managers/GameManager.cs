using UnityEngine;
using Fungus;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Flowchart flowchart;

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

    public void OnSceneChanged()
    {
        flowchart = FindFirstObjectByType<Flowchart>();
    }
}