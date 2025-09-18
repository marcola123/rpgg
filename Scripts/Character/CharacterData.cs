using System;

[Serializable]
public class CharacterData
{
    public string Name;
    public string Class;
    public string Race;

    public CharacterData(string name, string @class, string race)
    {
        Name = name;
        Class = @class;
        Race = race;
    }
}

