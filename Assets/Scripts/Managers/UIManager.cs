using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public Canvas canvasPrefab;
    public CanvasGroup blackScreen;
    public TextMeshProUGUI blackScreenText;
    public TextMeshProUGUI interactionText;

    public Button quitButton;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (blackScreen != null)
        {
            blackScreen.alpha = 0f;
            blackScreen.gameObject.SetActive(false);
        }

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }

        if (quitButton != null)
        {
            quitButton.gameObject.SetActive(false);
            quitButton.onClick.AddListener(() =>
            {
                Application.Quit();
            });
        }
    }

    public void ShowCanvas()
    {
        if (canvasPrefab != null)
        {
            Instantiate(canvasPrefab);
        }
    }

    public void ShowInteractionText(string text)
    {
        if (interactionText != null)
        {
            interactionText.text = text;
            interactionText.gameObject.SetActive(true);
        }
    }

    public void HideInteractionText()
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    public Sequence ShowBlackScreen(
        float fadeInDuration = 1f,
        float stayDuration = 1f,
        float fadeOutDuration = 1f)
    {
        if (blackScreen == null)
        {
            return null;
        }

        return blackScreen.FadeInStayFadeOut(
            fadeInDuration,
            stayDuration,
            fadeOutDuration
        );
    }

    public Sequence ShowBlackScreenWithText(
        string text,
        float fadeInDuration = 1f,
        float stayDuration = 1f,
        float fadeOutDuration = 1f)
    {
        if (blackScreen == null || blackScreenText == null)
        {
            return null;
        }

        blackScreenText.text = text;

        return blackScreen.FadeInStayFadeOut(
            fadeInDuration,
            stayDuration,
            fadeOutDuration
        );
    
    }

    public void ShowEnd(string text)
    {
        if (blackScreenText != null)
        {
            blackScreenText.text = text;
            blackScreenText.gameObject.SetActive(true);
            blackScreen.gameObject.SetActive(true);
            blackScreen.FadeIn(0.5f, 0f, Ease.Linear);
        }
        if (quitButton != null)
        {
            quitButton.gameObject.SetActive(true);
        }
    }
}