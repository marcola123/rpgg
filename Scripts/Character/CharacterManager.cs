using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class CharacterManager : MonoBehaviour
{
    private const string CHARACTER_SAVE_KEY = "SavedCharacters";

    public List<CharacterData> LoadCharacters()
    {
        if (PlayerPrefs.HasKey(CHARACTER_SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(CHARACTER_SAVE_KEY);
            return JsonConvert.DeserializeObject<List<CharacterData>>(json);
        }
        return new List<CharacterData>();
    }

    public void SaveCharacter(CharacterData newCharacter)
    {
        List<CharacterData> characters = LoadCharacters();
        // Check for unique name (mock validation)
        if (characters.Exists(c => c.Name == newCharacter.Name))
        {
            Debug.LogWarning($"Personagem com o nome '{newCharacter.Name}' já existe.");
            return;
        }
        characters.Add(newCharacter);
        string json = JsonConvert.SerializeObject(characters);
        PlayerPrefs.SetString(CHARACTER_SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log($"Personagem '{newCharacter.Name}' salvo com sucesso.");
    }

    public bool IsCharacterNameUnique(string name)
    {
        List<CharacterData> characters = LoadCharacters();
        return !characters.Exists(c => c.Name == name);
    }

    // Para fins de teste, pode ser útil limpar os personagens salvos
    public void ClearSavedCharacters()
    {
        PlayerPrefs.DeleteKey(CHARACTER_SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("Todos os personagens salvos foram limpos.");
    }
}

