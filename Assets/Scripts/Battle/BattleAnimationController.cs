using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections;
using DG.Tweening;

public class BattleAnimationController : MonoBehaviour
{
    [SerializeField] RectTransform attackPanel;
    [SerializeField] RectTransform attackImage;
    [SerializeField] TMP_Text actionText;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float waitAfterAttack = 0.2f;
    [SerializeField] private Material flashMaterial;
    [SerializeField] private float flashDuration = 0.15f;

    private Coroutine flashCoroutine;

    public IEnumerator PlayAction(BattleUnit actor, BattleActionData action)
    {
        attackPanel.gameObject.SetActive(true);
        attackImage.gameObject.SetActive(true);
        attackImage.GetComponent<Image>().sprite = actor.Definition.AttackPortrait;
        string actionTextValue = actor.Definition.DisplayName + " uses " + action.displayName + "!";
        actionText.text = actionTextValue;

        attackImage.anchoredPosition =
            new Vector2(-500, 0);

        yield return attackImage
            .MoveAndFadeIn(Vector2.zero, transitionDuration).WaitForCompletion();

        yield return new WaitForSeconds(waitAfterAttack);

        attackPanel.gameObject.SetActive(false);
        attackImage.gameObject.SetActive(false);
    }

    public IEnumerator PlayHit(GameObject targetView)
    {
        if (targetView == null)
        {
            yield break;
        }

        SpriteRenderer spriteRenderer = targetView.GetComponent<SpriteRenderer>();
        Material originalMaterial = spriteRenderer.material;
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashRoutine(spriteRenderer, originalMaterial));

        yield return new WaitForSeconds(0.5f); // Wait for the hit animation to complete
    }
    private IEnumerator FlashRoutine(SpriteRenderer spriteRenderer, Material originalMaterial)
    {
        if (spriteRenderer == null || flashMaterial == null)
        {
            yield break;
        }

        spriteRenderer.material = flashMaterial; // Swap to flash color
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.material = originalMaterial; // Swap back to normal
        flashCoroutine = null;
    }
}


