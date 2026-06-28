using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BattleCharacter", menuName = "Brewmasters/Battle/Character")]
public sealed class BattleCharacterData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string id = "character_id";
    [SerializeField] private string displayName = "Character";
    [SerializeField] private BattleTeam team;
    [SerializeField] private FlavorType flavor;
    [SerializeField] private FlavorType[] weaknesses = Array.Empty<FlavorType>();
    [SerializeField] private Sprite portrait;
    [SerializeField] private GameObject battlePrefab;

    [Header("Stats")]
    [SerializeField, Min(1)] private int maxHp = 100;
    [SerializeField, Min(0)] private int attack = 10;
    [SerializeField, Min(0)] private int defense = 10;
    [SerializeField, Min(1)] private int speed = 100;
    [SerializeField, Min(0)] private int maxEnergy = 100;
    [SerializeField, Min(0)] private int maxBreakGauge;

    [Header("Player Kit")]
    [SerializeField] private BattleActionData basicAttack = new BattleActionData();
    [SerializeField] private BattleActionData skill = new BattleActionData();
    [SerializeField] private BattleActionData ultimate = new BattleActionData();
    [SerializeField] private BattleActionData supportSkill = new BattleActionData();

    [Header("Enemy AI")]
    [Tooltip("Dipakai berurutan dan mengulang dari awal. Kosongkan untuk hero.")]
    [SerializeField] private BattleActionData[] enemyPattern = Array.Empty<BattleActionData>();

    public string Id => id;
    public string DisplayName => displayName;
    public BattleTeam Team => team;
    public FlavorType Flavor => flavor;
    public FlavorType[] Weaknesses => weaknesses;
    public Sprite Portrait => portrait;
    public GameObject BattlePrefab => battlePrefab;
    public int MaxHp => maxHp;
    public int Attack => attack;
    public int Defense => defense;
    public int Speed => speed;
    public int MaxEnergy => maxEnergy;
    public int MaxBreakGauge => maxBreakGauge;
    public BattleActionData BasicAttack => basicAttack;
    public BattleActionData Skill => skill;
    public BattleActionData Ultimate => ultimate;
    public BattleActionData SupportSkill => supportSkill;
    public BattleActionData[] EnemyPattern => enemyPattern;

    public void ConfigureIdentity(
        string characterId,
        string characterName,
        BattleTeam characterTeam,
        FlavorType characterFlavor,
        params FlavorType[] characterWeaknesses)
    {
        id = characterId;
        displayName = characterName;
        team = characterTeam;
        flavor = characterFlavor;
        weaknesses = characterWeaknesses ?? Array.Empty<FlavorType>();
    }

    public void ConfigureStats(
        int characterMaxHp,
        int characterAttack,
        int characterDefense,
        int characterSpeed,
        int characterMaxEnergy,
        int characterMaxBreakGauge)
    {
        maxHp = characterMaxHp;
        attack = characterAttack;
        defense = characterDefense;
        speed = characterSpeed;
        maxEnergy = characterMaxEnergy;
        maxBreakGauge = characterMaxBreakGauge;
        ClampValues();
    }

    public void ConfigurePlayerKit(
        BattleActionData basic,
        BattleActionData characterSkill,
        BattleActionData characterUltimate,
        BattleActionData characterSupportSkill)
    {
        basicAttack = basic ?? new BattleActionData();
        skill = characterSkill ?? new BattleActionData();
        ultimate = characterUltimate ?? new BattleActionData();
        supportSkill = characterSupportSkill ?? new BattleActionData();
    }

    public void ConfigureEnemyPattern(params BattleActionData[] pattern)
    {
        enemyPattern = pattern ?? Array.Empty<BattleActionData>();
    }

    private void OnValidate()
    {
        ClampValues();
    }

    private void ClampValues()
    {
        maxHp = Mathf.Max(1, maxHp);
        attack = Mathf.Max(0, attack);
        defense = Mathf.Max(0, defense);
        speed = Mathf.Max(1, speed);
        maxEnergy = Mathf.Max(0, maxEnergy);
        maxBreakGauge = Mathf.Max(0, maxBreakGauge);
    }
}
