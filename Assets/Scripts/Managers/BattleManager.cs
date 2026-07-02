using System;
using System.Collections;
using System.Collections.Generic;
using Fungus;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    private sealed class TimelineSlot
    {
        public BattleUnit Unit;
        public float ReadyAt;
        public long Order;
    }

    public static BattleManager Instance;

    [Header("Story Integration")]
    public Flowchart flowchart;
    public string afterBattleSceneName = "Scene_Cafe";

    [Header("Battle Data")]
    [SerializeField] private BattleEncounterData firstBattleEncounter;
    [SerializeField] private BattleEncounterData secondBattleEncounter;

    [Header("Scene UI")]
    [SerializeField] private BattleHUD battleHud;

    [Header("Optional Battle Prefab Spawn Points")]
    [SerializeField] private Transform[] allySpawnPoints;
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("Battle Rules")]
    [SerializeField, Min(1)] private int maxSkillPoints = 5;
    [SerializeField, Min(0)] private int startingSkillPoints = 3;
    [SerializeField, Range(0f, 1f)] private float brokenDefenseReduction = 0.3f;

    [Header("Optional Animation Controller")]
    [SerializeField] private BattleAnimationController animationController;

    private readonly List<BattleUnit> _allies = new List<BattleUnit>();
    private readonly List<BattleUnit> _enemies = new List<BattleUnit>();
    private readonly List<TimelineSlot> _timeline = new List<TimelineSlot>();
    private readonly List<string> _battleLog = new List<string>();
    private readonly List<GameObject> _spawnedBattleObjects = new List<GameObject>();
    private readonly Dictionary<BattleUnit, GameObject> _unitToViewMap = new Dictionary<BattleUnit, GameObject>();
    private readonly BattleTutorial _tutorial = new BattleTutorial();

    private float _timelineClock;
    private long _timelineOrder;
    private int _actionSequence;
    private BattleUnit _pendingSupportUnit;
    private BattleUnit _pendingBrokenEnemy;
    private BattleEncounterData _encounter;

    public event Action BattleChanged;

    public BattleState State { get; private set; } = BattleState.WaitingForIntro;
    public BattleUnit CurrentUnit { get; private set; }
    public int SkillPoints { get; private set; }
    public int MaxSkillPoints => maxSkillPoints;
    public IReadOnlyList<BattleUnit> Allies => _allies;
    public IReadOnlyList<BattleUnit> Enemies => _enemies;
    public IReadOnlyList<string> BattleLog => _battleLog;
    public BattleTutorial Tutorial => _tutorial;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (battleHud == null)
        {
            battleHud = FindFirstObjectByType<BattleHUD>();
        }

        if (battleHud != null)
        {
            battleHud.Bind(this);
        }
        else
        {
            Debug.LogWarning("BattleManager tidak menemukan BattleHUD di scene.", this);
        }
    }

    private IEnumerator Start()
    {
        PrepareBattle();

        yield return null;

        while(flowchart != null && flowchart.HasExecutingBlocks())
            yield return null;

        StartBattle();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void StartBattle()
    {
        battleHud?.gameObject.SetActive(true);
        SkillPoints = Mathf.Clamp(startingSkillPoints, 0, maxSkillPoints);
        _tutorial.Begin(_encounter.EnableTutorial);

        for (int i = 0; i < _allies.Count; i++)
        {
            ScheduleInitialTurn(_allies[i]);
        }

        for (int i = 0; i < _enemies.Count; i++)
        {
            ScheduleInitialTurn(_enemies[i]);
        }

        State = BattleState.Resolving;
        NotifyChanged();
        AdvanceTurn();
    }

    public bool CanUseBasicAttack()
    {
        return State == BattleState.AwaitingPlayerInput &&
               CurrentUnit != null &&
               CurrentUnit.Team == BattleTeam.Ally &&
               CurrentUnit.Definition.BasicAttack != null &&
               _tutorial.AllowsAction(BattleActionType.BasicAttack);
    }

    public bool CanUseSkill()
    {
        return State == BattleState.AwaitingPlayerInput &&
               CurrentUnit != null &&
               CurrentUnit.Team == BattleTeam.Ally &&
               CurrentUnit.Definition.Skill != null &&
               SkillPoints + CurrentUnit.Definition.Skill.skillPointDelta >= 0 &&
               _tutorial.AllowsAction(BattleActionType.Skill);
    }

    public bool CanUseUltimate(BattleUnit unit)
    {
        if (unit == null || !unit.IsAlive || unit.Team != BattleTeam.Ally ||
            !unit.HasUltimate || unit.Definition.Ultimate == null)
        {
            return false;
        }

        if (State == BattleState.WaitingForIntro || State == BattleState.Victory || State == BattleState.Defeat)
        {
            return false;
        }

        return _tutorial.AllowsAction(BattleActionType.Ultimate);
    }

    public bool UseBasicAttack(int enemyIndex = 0)
    {
        BattleUnit target = GetLivingEnemy(enemyIndex);
        if (!CanUseBasicAttack() || target == null)
        {
            return false;
        }

        State = BattleState.Resolving;
        BattleUnit actor = CurrentUnit;
        _actionSequence++;

        StartCoroutine(PlayAttackRoutine(
            actor,
            target,
            actor.Definition.BasicAttack
        ));
        _tutorial.OnPlayerAction(BattleActionType.BasicAttack);

        return true;
    }

    public bool UseSkill(int enemyIndex = 0)
    {
        if (!CanUseSkill())
        {
            return false;
        }

        BattleUnit actor = CurrentUnit;
        BattleUnit target = GetLivingEnemy(enemyIndex);
        if (RequiresOpponentTarget(actor.Definition.Skill) && target == null)
        {
            return false;
        }

        State = BattleState.Resolving;
        _actionSequence++;
        StartCoroutine(PlayAttackRoutine(
            actor,
            target,
            actor.Definition.Skill
        ));
        _tutorial.OnPlayerAction(BattleActionType.Skill);
        return true;
    }

    public bool UseUltimate(int allyIndex, int enemyIndex = 0)
    {
        if (allyIndex < 0 || allyIndex >= _allies.Count)
        {
            return false;
        }

        BattleUnit actor = _allies[allyIndex];
        BattleUnit target = GetLivingEnemy(enemyIndex);
        BattleActionData ultimate = actor.Definition.Ultimate;
        if (!CanUseUltimate(actor) || (RequiresOpponentTarget(ultimate) && target == null))
        {
            return false;
        }

        BattleState previousState = State;
        State = BattleState.Resolving;
        actor.SpendUltimateEnergy();
        StartCoroutine(PlayUltimateRoutine(
            actor,
            target
        ));

        _tutorial.OnPlayerAction(BattleActionType.Ultimate);

        return true;
    }

    public void ContinueTutorial()
    {
        if (!_tutorial.ShowsContinueButton)
        {
            return;
        }

        BattleTutorialStep previousStep = _tutorial.Step;
        BattleTutorialStep nextStep = _tutorial.Continue();

        if (previousStep == BattleTutorialStep.SupportChain)
        {
            ResolvePendingSupportChain();
            for (int i = 0; i < _allies.Count; i++)
            {
                if (_allies[i].IsAlive)
                {
                    _allies[i].FillEnergy();
                }
            }

            AddLog("Tutorial: party Energy filled. Ultimate is ready.");
        }

        NotifyChanged();
        if (nextStep != BattleTutorialStep.Ultimate)
        {
            AdvanceTurn();
        }
    }

    private IEnumerator PlayAttackRoutine(BattleUnit actor, BattleUnit target, BattleActionData action)
    {
        if (actor == null || target == null || action == null)
        {
            yield break;
        }

        yield return animationController.PlayAction(actor, action);

        ExecuteAction(actor, action, target);

        if (action.IsDamagingAction)
        {
            GameObject targetView = null;
            targetView = _unitToViewMap.TryGetValue(target, out GameObject view) ? view : null;
            yield return animationController.PlayHit(targetView);
        }

        if(action != actor.Definition.Ultimate)
        {
            ApplyActionResources(actor, action);
            CompleteNormalTurn(actor);
        }
    }

    private IEnumerator PlayUltimateRoutine(
        BattleUnit actor,
        BattleUnit target)
    {
        yield return animationController.PlayAction(
            actor,
            actor.Definition.Ultimate);

        ExecuteAction(actor, actor.Definition.Ultimate, target);

        NotifyChanged();

        if (actor.Definition.Ultimate.IsDamagingAction)
        {
            GameObject targetView = null;
            targetView = _unitToViewMap.TryGetValue(target, out GameObject view) ? view : null;
            yield return animationController.PlayHit(targetView);
        }

        if (CheckBattleEnd())
            yield break;

        if (CurrentUnit != null &&
            CurrentUnit.Team == BattleTeam.Ally)
        {
            State = BattleState.AwaitingPlayerInput;
        }
        else
        {
            State = BattleState.Resolving;
            AdvanceTurn();
        }

        NotifyChanged();
    }

    private IEnumerator PlayEnemyRoutine(BattleUnit enemy)
    {
        _actionSequence++;
        BattleActionData[] pattern = enemy.Definition.EnemyPattern;
        if (pattern == null || pattern.Length == 0)
        {
            AddLog($"{enemy.DisplayName} has no configured enemy action and skips its turn.");
            yield return new WaitForSeconds(0.5f);
        }

        BattleActionData action = pattern[enemy.EnemyPatternIndex % pattern.Length];
        if (action == null)
        {
            AddLog($"{enemy.DisplayName}'s enemy pattern contains an empty action.");
            enemy.EnemyPatternIndex++;
            yield break;
        }
        yield return animationController.PlayAction(
            enemy,
            action
        );
        BattleUnit target = PickRandomLivingOpponent(enemy);
        ExecuteAction(enemy, action, target);
        enemy.GainEnergy(action.energyGain);
        enemy.EnemyPatternIndex++;

        if (action.IsDamagingAction)
        {
            GameObject targetView = null;
            targetView = _unitToViewMap.TryGetValue(target, out GameObject view) ? view : null;
            yield return animationController.PlayHit(targetView);
        }
        CompleteNormalTurn(enemy);
    }

    public List<BattleQueueEntry> GetTurnQueue(int maxEntries = 8)
    {
        List<BattleQueueEntry> result = new List<BattleQueueEntry>();
        if (CurrentUnit != null && CurrentUnit.IsAlive)
        {
            result.Add(new BattleQueueEntry(CurrentUnit, _timelineClock));
        }

        List<TimelineSlot> sorted = new List<TimelineSlot>(_timeline);
        sorted.Sort(CompareTimelineSlots);
        for (int i = 0; i < sorted.Count && result.Count < maxEntries; i++)
        {
            if (sorted[i].Unit.IsAlive)
            {
                result.Add(new BattleQueueEntry(sorted[i].Unit, sorted[i].ReadyAt));
            }
        }

        return result;
    }

    public void EndBattle()
    {
        if (State != BattleState.Victory || StoryManager.Instance == null)
        {
            return;
        }

        switch (StoryManager.Instance.CurrentProgress)
        {
            case StoryProgress.Opening:
                StoryManager.Instance.CurrentProgress = StoryProgress.FirstBattleFinished;
                break;
            case StoryProgress.SecondDay:
                StoryManager.Instance.CurrentProgress = StoryProgress.SecondBattleFinished;
                break;
        }

        SceneLoader.LoadScene(afterBattleSceneName);
    }

    public void RestartBattle()
    {
        SceneLoader.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ResetRuntimeState()
    {
        _allies.Clear();
        _enemies.Clear();
        _timeline.Clear();
        _battleLog.Clear();
        _timelineClock = 0f;
        _timelineOrder = 0;
        _actionSequence = 0;
        _pendingSupportUnit = null;
        _pendingBrokenEnemy = null;
        _encounter = null;
        CurrentUnit = null;

        for (int i = 0; i < _spawnedBattleObjects.Count; i++)
        {
            if (_spawnedBattleObjects[i] != null)
            {
                Destroy(_spawnedBattleObjects[i]);
            }
        }

        _spawnedBattleObjects.Clear();
    }

    private bool TryLoadEncounter(BattleEncounterData encounter)
    {
        ResetRuntimeState();
        if (encounter == null)
        {
            Debug.LogError("Battle encounter belum dipasang pada BattleManager.", this);
            return false;
        }

        AddUnits(encounter.Allies, BattleTeam.Ally, _allies, encounter);
        AddUnits(encounter.Enemies, BattleTeam.Enemy, _enemies, encounter);

        if (_allies.Count == 0 || _enemies.Count == 0)
        {
            Debug.LogError(
                $"Encounter '{encounter.name}' harus memiliki minimal satu Ally dan satu Enemy.",
                encounter);
            ResetRuntimeState();
            return false;
        }

        SpawnBattlePrefabs(_allies, allySpawnPoints);
        SpawnBattlePrefabs(_enemies, enemySpawnPoints);

        _encounter = encounter;

        return true;
    }

    private void PrepareBattle()
    {
        StoryProgress progress = StoryManager.Instance != null
            ? StoryManager.Instance.CurrentProgress
            : StoryProgress.Opening;

        BattleEncounterData encounter = progress == StoryProgress.SecondDay
            ? secondBattleEncounter
            : firstBattleEncounter;

        TryLoadEncounter(encounter);

        if (battleHud != null)
            battleHud.gameObject.SetActive(false);
    }

    private void SpawnBattlePrefabs(IReadOnlyList<BattleUnit> units, IReadOnlyList<Transform> spawnPoints)
    {
        if (spawnPoints == null)
        {
            return;
        }

        int count = Mathf.Min(units.Count, spawnPoints.Count);
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = units[i].Definition.BattlePrefab;
            Transform spawnPoint = spawnPoints[i];
            if (prefab == null || spawnPoint == null)
            {
                continue;
            }

            GameObject instance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
            _spawnedBattleObjects.Add(instance);
            _unitToViewMap[units[i]] = instance;
        }
    }

    private static void AddUnits(
        IReadOnlyList<BattleCharacterData> source,
        BattleTeam expectedTeam,
        ICollection<BattleUnit> destination,
        BattleEncounterData encounter)
    {
        if (source == null)
        {
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            BattleCharacterData data = source[i];
            if (data == null)
            {
                Debug.LogWarning($"Ada slot character kosong pada encounter '{encounter.name}'.", encounter);
                continue;
            }

            if (data.Team != expectedTeam)
            {
                Debug.LogError(
                    $"Character '{data.name}' bertipe {data.Team}, tetapi dipasang pada list {expectedTeam}.",
                    data);
                continue;
            }

            destination.Add(new BattleUnit(data));
        }
    }

    private void ScheduleInitialTurn(BattleUnit unit)
    {
        _timeline.Add(new TimelineSlot
        {
            Unit = unit,
            ReadyAt = 10000f / unit.Speed,
            Order = _timelineOrder++
        });
    }

    private void ScheduleNextTurn(BattleUnit unit)
    {
        _timeline.Add(new TimelineSlot
        {
            Unit = unit,
            ReadyAt = _timelineClock + (10000f / unit.Speed),
            Order = _timelineOrder++
        });
    }

    private void AdvanceTurn()
    {
        if (State == BattleState.Victory || State == BattleState.Defeat || CurrentUnit != null)
        {
            return;
        }

        if (CheckBattleEnd() || _tutorial.BlocksTurnAdvance)
        {
            State = BattleState.Resolving;
            NotifyChanged();
            return;
        }

        TimelineSlot next = PopNextLivingSlot();
        if (next == null)
        {
            CheckBattleEnd();
            return;
        }

        CurrentUnit = next.Unit;
        _timelineClock = next.ReadyAt;

        if (CurrentUnit.Team == BattleTeam.Enemy && CurrentUnit.IsBroken)
        {
            BattleUnit skippedEnemy = CurrentUnit;
            _actionSequence++;
            AddLog($"{skippedEnemy.DisplayName} is Broken and skips its turn.");
            skippedEnemy.RecoverFromBreak();
            CompleteNormalTurn(skippedEnemy);
            return;
        }

        if (CurrentUnit.Team == BattleTeam.Ally)
        {
            State = BattleState.AwaitingPlayerInput;
            AddLog($"{CurrentUnit.DisplayName}'s turn.");
            NotifyChanged();
            return;
        }

        State = BattleState.Resolving;
        NotifyChanged();
        StartCoroutine(PlayEnemyRoutine(CurrentUnit));
    }

    private TimelineSlot PopNextLivingSlot()
    {
        while (_timeline.Count > 0)
        {
            _timeline.Sort(CompareTimelineSlots);
            TimelineSlot result = _timeline[0];
            _timeline.RemoveAt(0);
            if (result.Unit.IsAlive)
            {
                return result;
            }
        }

        return null;
    }

    private static int CompareTimelineSlots(TimelineSlot a, TimelineSlot b)
    {
        int timeComparison = a.ReadyAt.CompareTo(b.ReadyAt);
        return timeComparison != 0 ? timeComparison : a.Order.CompareTo(b.Order);
    }

    private void CompleteNormalTurn(BattleUnit actor)
    {
        actor.TickEffectsAfterOwnTurn(_actionSequence);
        if (actor.IsAlive)
        {
            ScheduleNextTurn(actor);
        }

        CurrentUnit = null;
        if (!CheckBattleEnd())
        {
            State = BattleState.Resolving;
            NotifyChanged();
            AdvanceTurn();
        }
    }

    private int ExecuteAction(BattleUnit source, BattleActionData action, BattleUnit preferredTarget)
    {
        if (source == null || action == null || !source.IsAlive)
        {
            return 0;
        }

        AddLog($"{source.DisplayName} uses {action.displayName}.");
        int totalHpDamage = 0;

        switch (action.effect)
        {
            case BattleActionEffect.SingleTargetDamage:
                totalHpDamage += ExecuteHits(source, preferredTarget, action);
                break;

            case BattleActionEffect.AllOpponentsDamage:
            case BattleActionEffect.AllOpponentsDamageAndHealLowestHpAlly:
                IReadOnlyList<BattleUnit> opponents = GetOpponents(source);
                for (int i = 0; i < opponents.Count; i++)
                {
                    if (opponents[i].IsAlive)
                    {
                        totalHpDamage += ExecuteHits(source, opponents[i], action);
                    }
                }
                break;

            case BattleActionEffect.ShieldLowestHpAlly:
                BattleUnit shieldTarget = FindLowestHpTeammate(source);
                if (shieldTarget != null)
                {
                    int shield = Mathf.RoundToInt(source.MaxHp * action.shieldSourceMaxHpMultiplier);
                    shieldTarget.AddShield(shield);
                    AddLog($"{shieldTarget.DisplayName} gains {shield} Shield.");
                }
                break;

            case BattleActionEffect.SelfAttackBuff:
                source.AddAttackBuff(action.attackBuffPercent, action.attackBuffTurns, _actionSequence);
                AddLog($"{source.DisplayName} gains ATK +{action.attackBuffPercent * 100f:0}% for {action.attackBuffTurns} turn(s).");
                break;

            case BattleActionEffect.HealRandomAllyAndAttackBuff:
                BattleUnit randomTarget = PickRandomLivingTeammate(source);
                if (randomTarget != null)
                {
                    int healAmount = Mathf.RoundToInt(randomTarget.MaxHp * action.healTargetMaxHpMultiplier);
                    int healed = randomTarget.Heal(healAmount);
                    randomTarget.AddAttackBuff(action.attackBuffPercent, action.attackBuffTurns, _actionSequence);
                    AddLog($"{randomTarget.DisplayName} heals {healed} HP and gains ATK +{action.attackBuffPercent * 100f:0}%.");
                }
                break;
        }

        if (action.effect == BattleActionEffect.AllOpponentsDamageAndHealLowestHpAlly)
        {
            BattleUnit healTarget = FindLowestHpTeammate(source);
            if (healTarget != null)
            {
                int healed = healTarget.Heal(Mathf.RoundToInt(totalHpDamage * action.healFromDamageRatio));
                AddLog($"{healTarget.DisplayName} heals {healed} HP.");
            }
        }

        return totalHpDamage;
    }

    private int ExecuteHits(BattleUnit source, BattleUnit target, BattleActionData action)
    {
        if (target == null)
        {
            return 0;
        }

        int totalHpDamage = 0;
        int hits = Mathf.Max(1, action.hitCount);
        for (int hit = 0; hit < hits && target.IsAlive; hit++)
        {
            totalHpDamage += DealActionHit(source, target, action);
        }

        return totalHpDamage;
    }

    private int DealActionHit(BattleUnit source, BattleUnit target, BattleActionData action)
    {
        if (source == null || target == null || !source.IsAlive || !target.IsAlive)
        {
            return 0;
        }

        float basePower = action.damageScaling == DamageScaling.MaxHp
            ? source.MaxHp
            : source.EffectiveAttack;
        int rawDamage = Mathf.RoundToInt(basePower * action.powerMultiplier);
        int hpDamage = DealRawDamage(source, target, rawDamage);

        if (target.IsAlive && target.Team == BattleTeam.Enemy && action.breakDamagePerHit > 0)
        {
            if (target.HasWeakness(source.Flavor))
            {
                bool justBroken = target.ReduceBreakGauge(action.breakDamagePerHit);
                AddLog($"Weakness hit: -{action.breakDamagePerHit} Break Gauge.");
                if (justBroken)
                {
                    HandleEnemyBroken(target);
                }
            }
            else
            {
                AddLog($"{source.Flavor} does not match the enemy Weakness.");
            }
        }

        return hpDamage;
    }

    private void ApplyActionResources(BattleUnit actor, BattleActionData action)
    {
        actor.GainEnergy(action.energyGain);
        SkillPoints = Mathf.Clamp(SkillPoints + action.skillPointDelta, 0, maxSkillPoints);
    }

    private int DealRawDamage(BattleUnit source, BattleUnit target, int rawDamage)
    {
        float defense = target.Defense;
        if (target.IsBroken)
        {
            defense *= 1f - brokenDefenseReduction;
        }

        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(rawDamage * (100f / (100f + defense))));
        int hpDamage = target.ReceiveDamage(finalDamage);
        AddLog($"{target.DisplayName} takes {finalDamage} damage ({hpDamage} HP) from {source.DisplayName}.");
        return hpDamage;
    }

    private void HandleEnemyBroken(BattleUnit enemy)
    {
        AddLog($"BREAK! {enemy.DisplayName} is stunned and its defense is reduced.");
        BattleUnit supportUnit = FindNextSupportUnit();

        if (_tutorial.OnEnemyBroken())
        {
            _pendingBrokenEnemy = enemy;
            _pendingSupportUnit = supportUnit;
            NotifyChanged();
            return;
        }

        ExecuteSupportSkill(supportUnit, enemy);
    }

    private BattleUnit FindNextSupportUnit()
    {
        TimelineSlot best = null;
        for (int i = 0; i < _timeline.Count; i++)
        {
            TimelineSlot slot = _timeline[i];
            if (!slot.Unit.IsAlive || slot.Unit.Team != BattleTeam.Ally)
            {
                continue;
            }

            if (best == null || CompareTimelineSlots(slot, best) < 0)
            {
                best = slot;
            }
        }

        if (best != null)
        {
            return best.Unit;
        }

        for (int i = 0; i < _allies.Count; i++)
        {
            if (_allies[i].IsAlive)
            {
                return _allies[i];
            }
        }

        return null;
    }

    private void ResolvePendingSupportChain()
    {
        if (_pendingBrokenEnemy == null)
        {
            return;
        }

        ExecuteSupportSkill(_pendingSupportUnit, _pendingBrokenEnemy);
        _pendingSupportUnit = null;
        _pendingBrokenEnemy = null;
    }

    private void ExecuteSupportSkill(BattleUnit supporter, BattleUnit brokenEnemy)
    {
        if (supporter == null || !supporter.IsAlive || brokenEnemy == null)
        {
            return;
        }

        BattleActionData action = supporter.Definition.SupportSkill;
        if (action == null)
        {
            AddLog($"{supporter.DisplayName} has no configured Support Skill.");
            return;
        }

        AddLog($"Support Chain: {supporter.DisplayName} acts without consuming a turn.");
        ExecuteAction(supporter, action, brokenEnemy);
        supporter.GainEnergy(action.energyGain);

        NotifyChanged();
    }

    private BattleUnit GetLivingEnemy(int preferredIndex)
    {
        if (preferredIndex >= 0 && preferredIndex < _enemies.Count && _enemies[preferredIndex].IsAlive)
        {
            return _enemies[preferredIndex];
        }

        for (int i = 0; i < _enemies.Count; i++)
        {
            if (_enemies[i].IsAlive)
            {
                return _enemies[i];
            }
        }

        return null;
    }

    private static bool RequiresOpponentTarget(BattleActionData action)
    {
        return action != null && action.effect == BattleActionEffect.SingleTargetDamage;
    }

    private IReadOnlyList<BattleUnit> GetOpponents(BattleUnit source)
    {
        return source.Team == BattleTeam.Ally ? _enemies : _allies;
    }

    private IReadOnlyList<BattleUnit> GetTeammates(BattleUnit source)
    {
        return source.Team == BattleTeam.Ally ? _allies : _enemies;
    }

    private BattleUnit FindLowestHpTeammate(BattleUnit source)
    {
        BattleUnit result = null;
        IReadOnlyList<BattleUnit> teammates = GetTeammates(source);
        for (int i = 0; i < teammates.Count; i++)
        {
            BattleUnit teammate = teammates[i];
            if (!teammate.IsAlive)
            {
                continue;
            }

            if (result == null || teammate.HpNormalized < result.HpNormalized)
            {
                result = teammate;
            }
        }

        return result;
    }

    private BattleUnit PickRandomLivingOpponent(BattleUnit source)
    {
        return PickRandomLivingUnit(GetOpponents(source));
    }

    private BattleUnit PickRandomLivingTeammate(BattleUnit source)
    {
        return PickRandomLivingUnit(GetTeammates(source));
    }

    private static BattleUnit PickRandomLivingUnit(IReadOnlyList<BattleUnit> units)
    {
        List<BattleUnit> livingUnits = new List<BattleUnit>();
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].IsAlive)
            {
                livingUnits.Add(units[i]);
            }
        }

        return livingUnits.Count == 0
            ? null
            : livingUnits[UnityEngine.Random.Range(0, livingUnits.Count)];
    }

    private bool CheckBattleEnd()
    {
        bool anyEnemyAlive = false;
        for (int i = 0; i < _enemies.Count; i++)
        {
            anyEnemyAlive |= _enemies[i].IsAlive;
        }

        if (!anyEnemyAlive && _enemies.Count > 0)
        {
            CurrentUnit = null;
            State = BattleState.Victory;
            AddLog("Victory!");
            NotifyChanged();
            return true;
        }

        bool anyAllyAlive = false;
        for (int i = 0; i < _allies.Count; i++)
        {
            anyAllyAlive |= _allies[i].IsAlive;
        }

        if (!anyAllyAlive && _allies.Count > 0)
        {
            CurrentUnit = null;
            State = BattleState.Defeat;
            AddLog("Party defeated.");
            NotifyChanged();
            return true;
        }

        return false;
    }

    private void AddLog(string message)
    {
        _battleLog.Add(message.TrimEnd());
        if (_battleLog.Count > 12)
        {
            _battleLog.RemoveAt(0);
        }
    }

    private void NotifyChanged()
    {
        BattleChanged?.Invoke();
    }
}
