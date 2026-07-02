using UnityEditor;
using UnityEngine;

public static class DefaultBattleAssetCreator
{
    private const string RootFolder = "Assets/GameData";
    private const string BattleFolder = RootFolder + "/Battle";

    [MenuItem("Tools/Brewmasters/Create or Refresh Default Battle Assets")]
    public static void CreateDefaultAssets()
    {
        EnsureFolder("Assets", "GameData");
        EnsureFolder(RootFolder, "Battle");

        BattleCharacterData rei = CreateOrLoadCharacter("Rei");
        ConfigureRei(rei);

        BattleCharacterData kayla = CreateOrLoadCharacter("Kayla");
        ConfigureKayla(kayla);

        BattleCharacterData gardenOverflow = CreateOrLoadCharacter("GardenOverflow");
        ConfigureGardenOverflow(gardenOverflow);

        BattleCharacterData insomniacPhantom = CreateOrLoadCharacter("InsomniacPhantom");
        ConfigureInsomniacPhantom(insomniacPhantom);

        BattleEncounterData firstEncounter = CreateOrLoadEncounter("FirstBattleEncounter");
        Undo.RecordObject(firstEncounter, "Configure First Battle Encounter");
        firstEncounter.Configure(
            "Garden Overflow Tutorial",
            true,
            new[] { rei, kayla },
            new[] { gardenOverflow });
        EditorUtility.SetDirty(firstEncounter);

        BattleEncounterData secondEncounter = CreateOrLoadEncounter("SecondBattleEncounter");
        Undo.RecordObject(secondEncounter, "Configure Second Battle Encounter");
        secondEncounter.Configure(
            "Insomniac Phantom",
            false,
            new[] { rei, kayla },
            new[] { insomniacPhantom });
        EditorUtility.SetDirty(secondEncounter);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = firstEncounter;
        EditorGUIUtility.PingObject(firstEncounter);
        Debug.Log($"Default battle assets created or refreshed in {BattleFolder}.");
    }

    private static void ConfigureRei(BattleCharacterData data)
    {
        Undo.RecordObject(data, "Configure Rei Battle Data");
        data.ConfigureIdentity("rei", "Rei", BattleTeam.Ally, FlavorType.Bitter);
        data.ConfigureStats(1000, 100, 60, 110, 100, 0);
        data.ConfigurePlayerKit(
            DamageAction("Coffee Stirrer Strike", 1f, 1, 20, 20, 1),
            DamageAction("Full-Bodied Thrust", 2.5f, 1, 40, 30, -1),
            DamageAction("Espresso Overdrive", 1f, 4, 10, 0, 0),
            new BattleActionData
            {
                displayName = "Extra Extraction",
                effect = BattleActionEffect.SelfAttackBuff,
                energyGain = 15,
                attackBuffPercent = 0.15f,
                attackBuffTurns = 2
            });
        data.ConfigureEnemyPattern();
        EditorUtility.SetDirty(data);
    }

    private static void ConfigureKayla(BattleCharacterData data)
    {
        Undo.RecordObject(data, "Configure Kayla Battle Data");
        data.ConfigureIdentity("kayla", "Kayla", BattleTeam.Ally, FlavorType.Sweet);
        data.ConfigureStats(900, 80, 70, 120, 100, 0);
        data.ConfigurePlayerKit(
            DamageAction("Latte Wand", 0.75f, 1, 10, 20, 1),
            new BattleActionData
            {
                displayName = "Latte Guard",
                effect = BattleActionEffect.ShieldLowestHpAlly,
                energyGain = 30,
                skillPointDelta = -1,
                shieldSourceMaxHpMultiplier = 1f
            },
            new BattleActionData
            {
                displayName = "Signature Blend",
                effect = BattleActionEffect.AllOpponentsDamageAndHealLowestHpAlly,
                damageScaling = DamageScaling.MaxHp,
                powerMultiplier = 0.6f,
                hitCount = 1,
                healFromDamageRatio = 0.5f
            },
            new BattleActionData
            {
                displayName = "Aroma Therapy",
                effect = BattleActionEffect.HealRandomAllyAndAttackBuff,
                healTargetMaxHpMultiplier = 0.2f,
                attackBuffPercent = 0.15f,
                attackBuffTurns = 2
            });
        data.ConfigureEnemyPattern();
        EditorUtility.SetDirty(data);
    }

    private static void ConfigureGardenOverflow(BattleCharacterData data)
    {
        Undo.RecordObject(data, "Configure Garden Overflow Battle Data");
        data.ConfigureIdentity(
            "garden_overflow",
            "Garden Overflow",
            BattleTeam.Enemy,
            FlavorType.None,
            FlavorType.Bitter);
        data.ConfigureStats(520, 65, 50, 95, 0, 60);
        data.ConfigureEnemyPattern(
            DamageAction("Sugar Rush Strike", 1f),
            new BattleActionData
            {
                displayName = "Hyperactive Burst",
                effect = BattleActionEffect.AllOpponentsDamage,
                damageScaling = DamageScaling.Attack,
                powerMultiplier = 0.75f,
                hitCount = 1
            });
        EditorUtility.SetDirty(data);
    }

    private static void ConfigureInsomniacPhantom(BattleCharacterData data)
    {
        Undo.RecordObject(data, "Configure Insomniac Phantom Battle Data");
        data.ConfigureIdentity(
            "insomniac_phantom",
            "Insomniac Phantom",
            BattleTeam.Enemy,
            FlavorType.None,
            FlavorType.Sour,
            FlavorType.Sweet);
        data.ConfigureStats(700, 75, 55, 105, 0, 80);
        data.ConfigureEnemyPattern(
            new BattleActionData
            {
                displayName = "Panic Brew",
                effect = BattleActionEffect.SelfAttackBuff,
                attackBuffPercent = 0.3f,
                attackBuffTurns = 1
            },
            DamageAction("Anxious Assault", 1.2f));
        EditorUtility.SetDirty(data);
    }

    private static BattleActionData DamageAction(
        string actionName,
        float power,
        int hits = 1,
        int breakDamage = 0,
        int energyGain = 0,
        int skillPointDelta = 0)
    {
        return new BattleActionData
        {
            displayName = actionName,
            effect = BattleActionEffect.SingleTargetDamage,
            damageScaling = DamageScaling.Attack,
            powerMultiplier = power,
            hitCount = hits,
            breakDamagePerHit = breakDamage,
            energyGain = energyGain,
            skillPointDelta = skillPointDelta
        };
    }

    private static BattleCharacterData CreateOrLoadCharacter(string fileName)
    {
        string path = $"{BattleFolder}/{fileName}.asset";
        BattleCharacterData asset = AssetDatabase.LoadAssetAtPath<BattleCharacterData>(path);
        if (asset != null)
        {
            return asset;
        }

        asset = ScriptableObject.CreateInstance<BattleCharacterData>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static BattleEncounterData CreateOrLoadEncounter(string fileName)
    {
        string path = $"{BattleFolder}/{fileName}.asset";
        BattleEncounterData asset = AssetDatabase.LoadAssetAtPath<BattleEncounterData>(path);
        if (asset != null)
        {
            return asset;
        }

        asset = ScriptableObject.CreateInstance<BattleEncounterData>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }
}
