using Fusion;
using System;

[Serializable]
public struct MonsterData : INetworkStruct
{
    [Networked] public int HP { get; set; }
    [Networked] public int MaxHP { get; set; }
    [Networked] public int XP { get; set; }
    [Networked] public NetworkString<_16> MonsterName { get; set; }

    public void Init(string name, int maxHp, int xp)
    {
        MonsterName = name;
        MaxHP = maxHp;
        HP = maxHp;
        XP = xp;
    }
}

