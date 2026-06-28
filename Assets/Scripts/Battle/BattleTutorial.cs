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
                return "Gunakan Basic Attack. Basic Attack menghasilkan Energy dan memulihkan 1 Skill Point.";
            case BattleTutorialStep.EnemyWeakness:
                return "Setiap musuh memiliki Weakness. Hanya serangan dengan Flavor yang cocok yang mengurangi Break Gauge. Garden Overflow lemah terhadap Bitter milik Rei.";
            case BattleTutorialStep.Skill:
                return "Gunakan Skill. Skill mengonsumsi 1 Skill Point, tetapi menghasilkan damage atau efek yang lebih kuat.";
            case BattleTutorialStep.BreakGauge:
                return "Serang Weakness untuk menghabiskan Break Gauge. Musuh yang Broken kehilangan defense dan akan melewatkan satu turn.";
            case BattleTutorialStep.BreakEnemy:
                return "Habiskan Break Gauge Garden Overflow. Perhatikan urutan ally berikutnya pada Turn Queue.";
            case BattleTutorialStep.SupportChain:
                return "Break memicu Support Chain. Ally dengan giliran terdekat akan memakai Support Skill secara otomatis tanpa mengonsumsi turn.";
            case BattleTutorialStep.Ultimate:
                return "Energy party telah diisi untuk tutorial. Gunakan Ultimate kapan saja; Ultimate tidak mengonsumsi turn.";
            default:
                return string.Empty;
        }
    }
}
