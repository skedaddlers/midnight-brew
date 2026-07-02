using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleUnitView : MonoBehaviour
{
    [SerializeField] private GameObject contentRoot;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text flavorOrWeaknessText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private GameObject energyRoot;
    [SerializeField] private Slider energySlider;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private GameObject breakRoot;
    [SerializeField] private Slider breakSlider;
    [SerializeField] private TMP_Text breakText;
    [SerializeField] private Button targetButton;
    [SerializeField] private GameObject selectedIndicator;

    private Action _targetCallback;

    public void SetTargetCallback(Action callback)
    {
        _targetCallback = callback;
        if (targetButton == null)
        {
            return;
        }

        targetButton.onClick.RemoveListener(InvokeTargetCallback);
        targetButton.onClick.AddListener(InvokeTargetCallback);
    }

    public void Show(BattleUnit unit, bool selected, bool isEnemy)
    {
        SetVisible(true);
        if (nameText != null)
        {
            nameText.text = unit.DisplayName;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = unit.Definition.Portrait;
            portraitImage.enabled = unit.Definition.Portrait != null;
        }

        if (flavorOrWeaknessText != null)
        {
            flavorOrWeaknessText.text = isEnemy
                ? $"Weakness: {BattleFormatting.FormatFlavors(unit.Definition.Weaknesses)}"
                : $"Flavor: {unit.Flavor}";
        }

        SetSlider(hpSlider, unit.HpNormalized);
        if (hpText != null)
        {
            hpText.text = $"{unit.CurrentHp} / {unit.MaxHp}";
        }

        if (energyRoot != null)
        {
            energyRoot.SetActive(!isEnemy);
        }
        if (!isEnemy)
        {
            SetSlider(energySlider, unit.EnergyNormalized);
            if (energyText != null)
            {
                energyText.text = $"{unit.Energy} / {unit.MaxEnergy}";
            }
        }

        if (breakRoot != null)
        {
            breakRoot.SetActive(isEnemy);
        }
        if (isEnemy)
        {
            SetSlider(breakSlider, unit.BreakNormalized);
            if (breakText != null)
            {
                breakText.text = $"{unit.BreakGauge} / {unit.MaxBreakGauge}";
            }
        }

        if (statusText != null)
        {
            statusText.text = BuildStatus(unit);
        }

        if (targetButton != null)
        {
            targetButton.interactable = isEnemy && unit.IsAlive;
        }

        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(selected && isEnemy);
        }
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void InvokeTargetCallback()
    {
        _targetCallback?.Invoke();
    }

    private void SetVisible(bool visible)
    {
        (contentRoot != null ? contentRoot : gameObject).SetActive(visible);
    }

    private static void SetSlider(Slider slider, float value)
    {
        if (slider != null)
        {
            slider.value = Mathf.Clamp01(value);
        }
    }

    private static string BuildStatus(BattleUnit unit)
    {
        string status = unit.IsBroken ? "BROKEN" : string.Empty;
        if (unit.Shield > 0)
        {
            status += (status.Length > 0 ? "  |  " : string.Empty) + $"Shield {unit.Shield}";
        }

        if (unit.AttackBuffTurns > 0)
        {
            status += (status.Length > 0 ? "  |  " : string.Empty) +
                      $"ATK +{unit.AttackBuffPercent * 100f:0}% ({unit.AttackBuffTurns})";
        }

        return status;
    }
}
