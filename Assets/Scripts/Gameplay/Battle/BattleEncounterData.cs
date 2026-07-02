using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BattleEncounter", menuName = "Brewmasters/Battle/Encounter")]
public sealed class BattleEncounterData : ScriptableObject
{
    [SerializeField] private string encounterName = "Encounter";
    [SerializeField] private bool enableTutorial;
    [SerializeField] private BattleCharacterData[] allies = Array.Empty<BattleCharacterData>();
    [SerializeField] private BattleCharacterData[] enemies = Array.Empty<BattleCharacterData>();

    public string EncounterName => encounterName;
    public bool EnableTutorial => enableTutorial;
    public BattleCharacterData[] Allies => allies;
    public BattleCharacterData[] Enemies => enemies;

    public void Configure(
        string newEncounterName,
        bool tutorialEnabled,
        BattleCharacterData[] encounterAllies,
        BattleCharacterData[] encounterEnemies)
    {
        encounterName = newEncounterName;
        enableTutorial = tutorialEnabled;
        allies = encounterAllies ?? Array.Empty<BattleCharacterData>();
        enemies = encounterEnemies ?? Array.Empty<BattleCharacterData>();
    }
}
