using Fusion;
using System;

[Serializable]
public struct PlayerData : INetworkStruct
{
    [Networked] public int HP { get; set; }
    [Networked] public int MaxHP { get; set; }
    [Networked] public int Mana { get; set; }
    [Networked] public int MaxMana { get; set; }
    [Networked] public int XP { get; set; }
    [Networked] public int Level { get; set; }
    [Networked] public NetworkString<_16> CharacterName { get; set; }
    [Networked] public NetworkString<_16> CharacterClass { get; set; }
    [Networked] public NetworkString<_16> CharacterRace { get; set; }

    public void Init(string name, string @class, string race)
    {
        CharacterName = name;
        CharacterClass = @class;
        CharacterRace = race;
        HP = 100; // Valores iniciais
        MaxHP = 100;
        Mana = 50;
        MaxMana = 50;
        XP = 0;
        Level = 1;
    }
}

