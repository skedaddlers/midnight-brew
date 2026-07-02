using System;
using System.Collections.Generic;
using UnityEngine;

public enum FlavorType
{
    None,
    Bitter,
    Sweet,
    Sour
}

public enum BattleTeam
{
    Ally,
    Enemy
}

public enum BattleState
{
    WaitingForIntro,
    Resolving,
    AwaitingPlayerInput,
    Victory,
    Defeat
}

public enum BattleActionType
{
    BasicAttack,
    Skill,
    Ultimate,
    SupportSkill
}

public enum BattleActionEffect
{
    SingleTargetDamage,
    AllOpponentsDamage,
    ShieldLowestHpAlly,
    SelfAttackBuff,
    HealRandomAllyAndAttackBuff,
    AllOpponentsDamageAndHealLowestHpAlly
}

public enum DamageScaling
{
    Attack,
    MaxHp
}

[Serializable]
public sealed class BattleActionData
{
    public string displayName = "Action";
    public BattleActionEffect effect = BattleActionEffect.SingleTargetDamage;
    public DamageScaling damageScaling = DamageScaling.Attack;
    [Min(0f)] public float powerMultiplier = 1f;
    [Min(1)] public int hitCount = 1;
    [Min(0)] public int breakDamagePerHit;
    [Min(0)] public int energyGain;
    [Tooltip("Basic biasanya +1, Skill biasanya -1.")]
    public int skillPointDelta;
    [Min(0f)] public float shieldSourceMaxHpMultiplier;
    [Range(0f, 1f)] public float healFromDamageRatio;
    [Min(0f)] public float healTargetMaxHpMultiplier;
    [Min(0f)] public float attackBuffPercent;
    [Min(0)] public int attackBuffTurns;
    public AudioClip actionSound;
    public bool IsDamagingAction =>
        effect == BattleActionEffect.SingleTargetDamage ||
        effect == BattleActionEffect.AllOpponentsDamage ||
        effect == BattleActionEffect.AllOpponentsDamageAndHealLowestHpAlly;
}

public readonly struct BattleQueueEntry
{
    public BattleUnit Unit { get; }
    public float ActionTime { get; }

    public BattleQueueEntry(BattleUnit unit, float actionTime)
    {
        Unit = unit;
        ActionTime = actionTime;
    }
}

public static class BattleFormatting
{
    public static string FormatFlavors(IReadOnlyList<FlavorType> flavors)
    {
        if (flavors == null || flavors.Count == 0)
        {
            return "None";
        }

        string result = string.Empty;
        for (int i = 0; i < flavors.Count; i++)
        {
            if (i > 0)
            {
                result += ", ";
            }

            result += flavors[i].ToString();
        }

        return result;
    }
}
