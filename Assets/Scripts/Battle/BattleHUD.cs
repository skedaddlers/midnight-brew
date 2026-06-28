using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class BattleHUD : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private CanvasGroup rootCanvasGroup;

    [Header("Turn Queue")]
    [SerializeField] private BattleTurnQueueSlot[] turnQueueSlots;

    [Header("Unit Views")]
    [SerializeField] private BattleUnitView[] allyViews;
    [SerializeField] private BattleUnitView[] enemyViews;

    [Header("Battle Information")]
    [SerializeField] private TMP_Text currentTurnText;
    [SerializeField] private TMP_Text skillPointsText;
    [SerializeField] private TMP_Text battleLogText;

    [Header("Commands")]
    [SerializeField] private Button basicButton;
    [SerializeField] private TMP_Text basicButtonLabel;
    [SerializeField] private Button skillButton;
    [SerializeField] private TMP_Text skillButtonLabel;
    [SerializeField] private BattleUltimateButtonView[] ultimateButtons;

    [Header("Tutorial Modal")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TMP_Text tutorialTitleText;
    [SerializeField] private TMP_Text tutorialMessageText;
    [SerializeField] private Button tutorialContinueButton;

    [Header("Tutorial Banner")]
    [SerializeField] private GameObject tutorialBanner;
    [SerializeField] private TMP_Text tutorialBannerTitleText;
    [SerializeField] private TMP_Text tutorialBannerMessageText;

    [Header("Battle Result")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text resultMessageText;
    [SerializeField] private Button resultButton;
    [SerializeField] private TMP_Text resultButtonLabel;

    private BattleManager _battle;
    private int _selectedEnemy;
    private bool _listenersWired;

    public void Bind(BattleManager battle)
    {
        if (_battle != null)
        {
            _battle.BattleChanged -= Refresh;
        }

        _battle = battle;
        WireListenersOnce();

        if (_battle != null)
        {
            _battle.BattleChanged += Refresh;
        }

        Refresh();
    }

    private void OnDestroy()
    {
        if (_battle != null)
        {
            _battle.BattleChanged -= Refresh;
        }
    }

    private void WireListenersOnce()
    {
        if (_listenersWired)
        {
            return;
        }

        _listenersWired = true;
        basicButton?.onClick.AddListener(OnBasicClicked);
        skillButton?.onClick.AddListener(OnSkillClicked);
        tutorialContinueButton?.onClick.AddListener(OnTutorialContinueClicked);
        resultButton?.onClick.AddListener(OnResultClicked);

        if (enemyViews != null)
        {
            for (int i = 0; i < enemyViews.Length; i++)
            {
                int index = i;
                enemyViews[i]?.SetTargetCallback(() => SelectEnemy(index));
            }
        }

        if (ultimateButtons != null)
        {
            for (int i = 0; i < ultimateButtons.Length; i++)
            {
                int index = i;
                ultimateButtons[i]?.SetClickCallback(() => OnUltimateClicked(index));
            }
        }
    }

    private void Refresh()
    {
        bool battleVisible = _battle != null && _battle.State != BattleState.WaitingForIntro;
        SetCanvasVisible(battleVisible);
        if (!battleVisible)
        {
            return;
        }

        EnsureValidTarget();
        RefreshQueue();
        RefreshUnits();
        RefreshInformation();
        RefreshCommands();
        RefreshTutorial();
        RefreshResult();
    }

    private void RefreshQueue()
    {
        if (turnQueueSlots == null)
        {
            return;
        }

        var queue = _battle.GetTurnQueue(turnQueueSlots.Length);
        for (int i = 0; i < turnQueueSlots.Length; i++)
        {
            if (turnQueueSlots[i] == null)
            {
                continue;
            }

            if (i < queue.Count)
            {
                turnQueueSlots[i].Show(queue[i].Unit, i == 0);
            }
            else
            {
                turnQueueSlots[i].Hide();
            }
        }
    }

    private void RefreshUnits()
    {
        RefreshUnitList(allyViews, _battle.Allies, false);
        RefreshUnitList(enemyViews, _battle.Enemies, true);
    }

    private void RefreshUnitList(
        BattleUnitView[] views,
        System.Collections.Generic.IReadOnlyList<BattleUnit> units,
        bool enemies)
    {
        if (views == null)
        {
            return;
        }

        for (int i = 0; i < views.Length; i++)
        {
            if (views[i] == null)
            {
                continue;
            }

            if (i < units.Count)
            {
                views[i].Show(units[i], enemies && i == _selectedEnemy, enemies);
            }
            else
            {
                views[i].Hide();
            }
        }
    }

    private void RefreshInformation()
    {
        if (currentTurnText != null)
        {
            currentTurnText.text = _battle.CurrentUnit != null
                ? $"Turn: {_battle.CurrentUnit.DisplayName}"
                : "Resolving...";
        }

        if (skillPointsText != null)
        {
            skillPointsText.text = $"Skill Points  {_battle.SkillPoints} / {_battle.MaxSkillPoints}";
        }

        if (battleLogText != null)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < _battle.BattleLog.Count; i++)
            {
                if (i > 0)
                {
                    builder.AppendLine();
                }

                builder.Append("• ").Append(_battle.BattleLog[i]);
            }

            battleLogText.text = builder.ToString();
        }
    }

    private void RefreshCommands()
    {
        BattleUnit current = _battle.CurrentUnit;
        BattleActionData basic = current?.Definition.BasicAttack;
        BattleActionData skill = current?.Definition.Skill;

        if (basicButton != null)
        {
            basicButton.interactable = _battle.CanUseBasicAttack();
        }

        if (skillButton != null)
        {
            skillButton.interactable = _battle.CanUseSkill();
        }

        if (basicButtonLabel != null)
        {
            basicButtonLabel.text = FormatActionLabel(basic);
        }

        if (skillButtonLabel != null)
        {
            skillButtonLabel.text = FormatActionLabel(skill);
        }

        if (ultimateButtons == null)
        {
            return;
        }

        for (int i = 0; i < ultimateButtons.Length; i++)
        {
            if (ultimateButtons[i] == null)
            {
                continue;
            }

            if (i < _battle.Allies.Count)
            {
                BattleUnit ally = _battle.Allies[i];
                ultimateButtons[i].Show(ally, _battle.CanUseUltimate(ally));
            }
            else
            {
                ultimateButtons[i].Hide();
            }
        }
    }

    private void RefreshTutorial()
    {
        BattleTutorial tutorial = _battle.Tutorial;
        bool tutorialActive = tutorial.IsEnabled && tutorial.Step != BattleTutorialStep.Complete;
        bool showModal = tutorialActive && tutorial.ShowsContinueButton;
        bool showBanner = tutorialActive && !tutorial.ShowsContinueButton;

        tutorialPanel?.SetActive(showModal);
        tutorialBanner?.SetActive(showBanner);

        if (tutorialTitleText != null)
        {
            tutorialTitleText.text = tutorial.GetTitle();
        }

        if (tutorialMessageText != null)
        {
            tutorialMessageText.text = tutorial.GetMessage();
        }

        if (tutorialBannerTitleText != null)
        {
            tutorialBannerTitleText.text = tutorial.GetTitle();
        }

        if (tutorialBannerMessageText != null)
        {
            tutorialBannerMessageText.text = tutorial.GetMessage();
        }
    }

    private void RefreshResult()
    {
        bool victory = _battle.State == BattleState.Victory;
        bool defeat = _battle.State == BattleState.Defeat;
        resultPanel?.SetActive(victory || defeat);

        if (!victory && !defeat)
        {
            return;
        }

        if (resultTitleText != null)
        {
            resultTitleText.text = victory ? "VICTORY" : "DEFEAT";
        }

        if (resultMessageText != null)
        {
            resultMessageText.text = victory
                ? "Battle selesai. Lanjutkan story."
                : "Party tidak dapat melanjutkan battle.";
        }

        if (resultButtonLabel != null)
        {
            resultButtonLabel.text = victory ? "CONTINUE STORY" : "RETRY";
        }
    }

    private void EnsureValidTarget()
    {
        if (_selectedEnemy >= 0 && _selectedEnemy < _battle.Enemies.Count &&
            _battle.Enemies[_selectedEnemy].IsAlive)
        {
            return;
        }

        _selectedEnemy = 0;
        for (int i = 0; i < _battle.Enemies.Count; i++)
        {
            if (_battle.Enemies[i].IsAlive)
            {
                _selectedEnemy = i;
                return;
            }
        }
    }

    private void SelectEnemy(int index)
    {
        if (_battle == null || index < 0 || index >= _battle.Enemies.Count ||
            !_battle.Enemies[index].IsAlive)
        {
            return;
        }

        _selectedEnemy = index;
        Refresh();
    }

    private void OnBasicClicked()
    {
        _battle?.UseBasicAttack(_selectedEnemy);
    }

    private void OnSkillClicked()
    {
        _battle?.UseSkill(_selectedEnemy);
    }

    private void OnUltimateClicked(int allyIndex)
    {
        _battle?.UseUltimate(allyIndex, _selectedEnemy);
    }

    private void OnTutorialContinueClicked()
    {
        _battle?.ContinueTutorial();
    }

    private void OnResultClicked()
    {
        if (_battle == null)
        {
            return;
        }

        if (_battle.State == BattleState.Victory)
        {
            _battle.EndBattle();
        }
        else if (_battle.State == BattleState.Defeat)
        {
            _battle.RestartBattle();
        }
    }

    private void SetCanvasVisible(bool visible)
    {
        if (rootCanvasGroup == null)
        {
            return;
        }

        rootCanvasGroup.alpha = visible ? 1f : 0f;
        rootCanvasGroup.interactable = visible;
        rootCanvasGroup.blocksRaycasts = visible;
    }

    private static string FormatActionLabel(BattleActionData action)
    {
        if (action == null)
        {
            return "—";
        }

        string resourceText = string.Empty;
        if (action.skillPointDelta != 0)
        {
            resourceText += action.skillPointDelta > 0
                ? $"SP +{action.skillPointDelta}"
                : $"SP {action.skillPointDelta}";
        }

        if (action.energyGain > 0)
        {
            if (resourceText.Length > 0)
            {
                resourceText += "  |  ";
            }

            resourceText += $"Energy +{action.energyGain}";
        }

        return resourceText.Length == 0
            ? action.displayName
            : $"{action.displayName}\n{resourceText}";
    }
}
