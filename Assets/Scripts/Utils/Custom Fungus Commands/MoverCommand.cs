using UnityEngine;
using Fungus;
using System.Collections;

[CommandInfo(
    "Brewmasters",
    "Move Character",
    "Move character to target"
)]
[AddComponentMenu("")]
public class MoveCharacterCommand : Command
{
    public CharacterMover characterMover;
    public Transform target;

    public override void OnEnter()
    {
        StartCoroutine(MoveAndWait());
    }

    IEnumerator MoveAndWait()
    {
        yield return StartCoroutine(
            characterMover.MoveTo(target.position)
        );

        Continue();
    }

    public override string GetSummary()
    {
        return "Move " + characterMover.name + " to: " + target.name;
    }
}