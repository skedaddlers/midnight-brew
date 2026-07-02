public enum BattleTutorialStep
{
    None,
    BasicAttack,
    EnemyWeakness,
    Skill,
    BreakGauge,
    BreakEnemy,
    SupportChain,
    Ultimate,
    Complete
}

public sealed class BattleTutorial
{
    public BattleTutorialStep Step { get; private set; } = BattleTutorialStep.None;
    public bool IsEnabled { get; private set; }

    public bool ShowsContinueButton =>
        Step == BattleTutorialStep.EnemyWeakness ||
        Step == BattleTutorialStep.BreakGauge ||
        Step == BattleTutorialStep.SupportChain;

    public bool BlocksTurnAdvance =>
        ShowsContinueButton || Step == BattleTutorialStep.Ultimate;

    public void Begin(bool enabled)
    {
        IsEnabled = enabled;
        Step = enabled ? BattleTutorialStep.BasicAttack : BattleTutorialStep.Complete;
    }

    public bool AllowsAction(BattleActionType actionType)
    {
        if (!IsEnabled || Step == BattleTutorialStep.Complete)
        {
            return true;
        }

        switch (Step)
        {
            case BattleTutorialStep.BasicAttack:
                return actionType == BattleActionType.BasicAttack;
            case BattleTutorialStep.Skill:
                return actionType == BattleActionType.Skill;
            case BattleTutorialStep.BreakEnemy:
                return actionType == BattleActionType.BasicAttack ||
                       actionType == BattleActionType.Skill;
            case BattleTutorialStep.Ultimate:
                return actionType == BattleActionType.Ultimate;
            default:
                return false;
        }
    }

    public void OnPlayerAction(BattleActionType actionType)
    {
        if (!IsEnabled)
        {
            return;
        }

        if (Step == BattleTutorialStep.BasicAttack && actionType == BattleActionType.BasicAttack)
        {
            Step = BattleTutorialStep.EnemyWeakness;
        }
        else if (Step == BattleTutorialStep.Skill && actionType == BattleActionType.Skill)
        {
            Step = BattleTutorialStep.BreakGauge;
        }
        else if (Step == BattleTutorialStep.Ultimate && actionType == BattleActionType.Ultimate)
        {
            Step = BattleTutorialStep.Complete;
        }
    }

    public bool OnEnemyBroken()
    {
        if (!IsEnabled || Step != BattleTutorialStep.BreakEnemy)
        {
            return false;
        }

        Step = BattleTutorialStep.SupportChain;
        return true;
    }

    public BattleTutorialStep Continue()
    {
        switch (Step)
        {
            case BattleTutorialStep.EnemyWeakness:
                Step = BattleTutorialStep.Skill;
                break;
            case BattleTutorialStep.BreakGauge:
                Step = BattleTutorialStep.BreakEnemy;
                break;
            case BattleTutorialStep.SupportChain:
                Step = BattleTutorialStep.Ultimate;
                break;
        }

        return Step;
    }

    public string GetTitle()
    {
        switch (Step)
        {
            case BattleTutorialStep.BasicAttack: return "1 / 6  BASIC ATTACK";
            case BattleTutorialStep.EnemyWeakness: return "2 / 6  ENEMY WEAKNESS";
            case BattleTutorialStep.Skill: return "3 / 6  SKILL";
            case BattleTutorialStep.BreakGauge:
            case BattleTutorialStep.BreakEnemy: return "4 / 6  BREAK GAUGE";
            case BattleTutorialStep.SupportChain: return "5 / 6  SUPPORT CHAIN";
            case BattleTutorialStep.Ultimate: return "6 / 6  ULTIMATE";
            default: return string.Empty;
        }
    }

    public string GetMessage()
    {
        switch (Step)
        {
            case BattleTutorialStep.BasicAttack:
                return "Use Basic Attack. Basic Attack generates Energy and restores 1 Skill Point.";
            case BattleTutorialStep.EnemyWeakness:
                return "Each enemy has a Weakness. Only attacks with the matching Flavor reduce the Break Gauge. Garden Overflow is weak to Rei's Bitter.";
            case BattleTutorialStep.Skill:
                return "Use Skill. Skills consume 1 Skill Point but deal more damage or have stronger effects than Basic Attack. Skills also generate Energy.";
            case BattleTutorialStep.BreakGauge:
                return "Attack Weakness to deplete the Break Gauge. Broken enemies lose defense and will skip one turn.";
            case BattleTutorialStep.BreakEnemy:
                return "Deplete Garden Overflow's Break Gauge. Watch the next ally in the Turn Queue.";
            case BattleTutorialStep.SupportChain:
                return "Breaking triggers a Support Chain. The ally with the next turn will automatically use their Support Skill without consuming a turn.";
            case BattleTutorialStep.Ultimate:
                return "The party's Energy has been filled for the tutorial. Use Ultimate at any time; Ultimate does not consume a turn.";
            default:
                return string.Empty;
        }
    }
}
