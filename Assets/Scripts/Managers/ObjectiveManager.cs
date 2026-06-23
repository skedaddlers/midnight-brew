using UnityEngine;
using TMPro;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    public TextMeshProUGUI objectiveText;

    void Awake()
    {
        Instance = this;
    }

    public void SetObjective(string text)
    {
        objectiveText.text = text;
    }

    public void DisableText()
    {
        objectiveText.text = "";
    }
}