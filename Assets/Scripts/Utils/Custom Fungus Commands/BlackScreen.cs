using UnityEngine;
using Fungus;
using DG.Tweening;
using System.Collections;

[CommandInfo(
    "Brewmasters",
    "Show Black Screen",
    "Display a black screen"
)]
[AddComponentMenu("")]
public class ShowBlackScreenCommand : Command
{
    public string textToDisplay;
    public float fadeInDuration = 1f;
    public float stayDuration = 1f;
    public float fadeOutDuration = 1f;
    public override void OnEnter()
    {
        StartCoroutine(ShowBlackScreenAndWait());
    }

    IEnumerator ShowBlackScreenAndWait()
    {
        Sequence blackScreenSequence = UIManager.Instance.ShowBlackScreenWithText(
            textToDisplay,
            fadeInDuration,
            stayDuration,
            fadeOutDuration
        );

        if (blackScreenSequence != null)
        {
            yield return blackScreenSequence.WaitForCompletion();
        }

        Continue();
    }

    public override string GetSummary()
    {
        return "Show Black Screen: " + textToDisplay;
    }
}