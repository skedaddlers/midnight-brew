using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleTurnQueueSlot : MonoBehaviour
{
    [SerializeField] private GameObject contentRoot;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text currentMarkerText;
    [SerializeField] private Image background;
    [SerializeField] private Color allyColor = new Color(0.12f, 0.35f, 0.55f);
    [SerializeField] private Color enemyColor = new Color(0.55f, 0.16f, 0.18f);
    [SerializeField] private Color currentColor = new Color(0.9f, 0.55f, 0.12f);

    public void Show(BattleUnit unit, bool isCurrent)
    {
        SetVisible(true);
        if (nameText != null)
        {
            nameText.text = unit.DisplayName;
        }

        if (currentMarkerText != null)
        {
            currentMarkerText.text = isCurrent ? "NOW" : string.Empty;
        }

        if (background != null)
        {
            background.color = isCurrent
                ? currentColor
                : unit.Team == BattleTeam.Ally ? allyColor : enemyColor;
        }
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        (contentRoot != null ? contentRoot : gameObject).SetActive(visible);
    }
}
