using UnityEngine;

public sealed class BattleUnit
{
    public BattleCharacterData Definition { get; }
    public int CurrentHp { get; private set; }
    public int Energy { get; private set; }
    public int Shield { get; private set; }
    public int BreakGauge { get; private set; }
    public bool IsBroken { get; private set; }
    public int EnemyPatternIndex { get; set; }

    public float AttackBuffPercent { get; private set; }
    public int AttackBuffTurns { get; private set; }
    private int _attackBuffAppliedOnAction;

    public string Id => Definition.Id;
    public string DisplayName => Definition.DisplayName;
    public BattleTeam Team => Definition.Team;
    public FlavorType Flavor => Definition.Flavor;
    public int MaxHp => Definition.MaxHp;
    public int Attack => Definition.Attack;
    public int Defense => Definition.Defense;
    public int Speed => Mathf.Max(1, Definition.Speed);
    public int MaxEnergy => Definition.MaxEnergy;
    public int MaxBreakGauge => Definition.MaxBreakGauge;
    public bool IsAlive => CurrentHp > 0;
    public bool HasUltimate => MaxEnergy > 0 && Energy >= MaxEnergy;
    public float HpNormalized => MaxHp <= 0 ? 0f : (float)CurrentHp / MaxHp;
    public float EnergyNormalized => MaxEnergy <= 0 ? 0f : (float)Energy / MaxEnergy;
    public float BreakNormalized => MaxBreakGauge <= 0 ? 0f : (float)BreakGauge / MaxBreakGauge;
    public float EffectiveAttack => Attack * (1f + AttackBuffPercent);

    public BattleUnit(BattleCharacterData definition)
    {
        Definition = definition;
        CurrentHp = definition.MaxHp;
        Energy = 0;
        BreakGauge = definition.MaxBreakGauge;
    }

    public int ReceiveDamage(int amount)
    {
        amount = Mathf.Max(0, amount);
        int absorbed = Mathf.Min(Shield, amount);
        Shield -= absorbed;

        int hpDamage = Mathf.Min(CurrentHp, amount - absorbed);
        CurrentHp -= hpDamage;
        return hpDamage;
    }

    public int Heal(int amount)
    {
        if (!IsAlive)
        {
            return 0;
        }

        int healed = Mathf.Min(Mathf.Max(0, amount), MaxHp - CurrentHp);
        CurrentHp += healed;
        return healed;
    }

    public void AddShield(int amount)
    {
        Shield += Mathf.Max(0, amount);
    }

    public void GainEnergy(int amount)
    {
        if (MaxEnergy <= 0)
        {
            return;
        }

        Energy = Mathf.Clamp(Energy + amount, 0, MaxEnergy);
    }

    public void FillEnergy()
    {
        Energy = MaxEnergy;
    }

    public void SpendUltimateEnergy()
    {
        Energy = 0;
    }

    public bool HasWeakness(FlavorType flavor)
    {
        if (flavor == FlavorType.None || Definition.Weaknesses == null)
        {
            return false;
        }

        for (int i = 0; i < Definition.Weaknesses.Length; i++)
        {
            if (Definition.Weaknesses[i] == flavor)
            {
                return true;
            }
        }

        return false;
    }

    public bool ReduceBreakGauge(int amount)
    {
        if (Team != BattleTeam.Enemy || IsBroken || BreakGauge <= 0)
        {
            return false;
        }

        BreakGauge = Mathf.Max(0, BreakGauge - Mathf.Max(0, amount));
        if (BreakGauge > 0)
        {
            return false;
        }

        IsBroken = true;
        return true;
    }

    public void RecoverFromBreak()
    {
        IsBroken = false;
        BreakGauge = MaxBreakGauge;
    }

    public void AddAttackBuff(float percent, int turns, int actionSequence)
    {
        AttackBuffPercent = Mathf.Max(AttackBuffPercent, percent);
        AttackBuffTurns = Mathf.Max(AttackBuffTurns, turns);
        _attackBuffAppliedOnAction = actionSequence;
    }

    public void TickEffectsAfterOwnTurn(int actionSequence)
    {
        if (AttackBuffTurns <= 0 || actionSequence <= _attackBuffAppliedOnAction)
        {
            return;
        }

        AttackBuffTurns--;
        if (AttackBuffTurns <= 0)
        {
            AttackBuffPercent = 0f;
        }
    }
}
