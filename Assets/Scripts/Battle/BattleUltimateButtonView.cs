using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleUltimateButtonView : MonoBehaviour
{
    [SerializeField] private GameObject contentRoot;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Slider energySlider;

    private Action _clickCallback;

    public void SetClickCallback(Action callback)
    {
        _clickCallback = callback;
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(InvokeClickCallback);
        button.onClick.AddListener(InvokeClickCallback);
    }

    public void Show(BattleUnit unit, bool interactable)
    {
        SetVisible(true);
        if (button != null)
        {
            button.interactable = interactable;
        }

        if (label != null)
        {
            string ultimateName = unit.Definition.Ultimate != null
                ? unit.Definition.Ultimate.displayName
                : "Ultimate";
            label.text = $"{unit.DisplayName}\n{ultimateName}\n{unit.Energy} / {unit.MaxEnergy}";
        }

        if (energySlider != null)
        {
            energySlider.value = unit.EnergyNormalized;
        }
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void InvokeClickCallback()
    {
        _clickCallback?.Invoke();
    }

    private void SetVisible(bool visible)
    {
        (contentRoot != null ? contentRoot : gameObject).SetActive(visible);
    }
}
