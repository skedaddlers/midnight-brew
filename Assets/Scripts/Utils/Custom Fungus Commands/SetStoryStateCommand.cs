using UnityEngine;
using Fungus;
using System.Collections;

[CommandInfo(
    "Brewmasters",
    "Set Story State",
    "Set the current story state"
)]
[AddComponentMenu("")]
public class SetStoryStateCommand : Command
{
    public StoryProgress newStoryState;
    public override void OnEnter()
    {
        StoryManager.Instance.CurrentProgress = newStoryState;
        Continue();
    }

    public override string GetSummary()
    {
        return "Set Story State to: " + newStoryState.ToString();
    }
}