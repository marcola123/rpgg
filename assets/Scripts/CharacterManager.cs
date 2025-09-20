using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SocketIOClient;

public class CharacterManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject characterSelectionPanel;
    public GameObject characterCreationPanel;
    public Transform characterListContainer;
    public GameObject characterButtonPrefab;
    
    public TMP_InputField characterNameInput;
    public TMP_Dropdown classDropdown;
    public TMP_Dropdown raceDropdown;
    public Button createCharacterButton;
    
    private List<CharacterData> characters = new List<CharacterData>();

    void Start()
    {
        SetupUI();
        SetupSocketEvents();
    }

    void SetupUI()
    {
        createCharacterButton.onClick.AddListener(OnCreateCharacterClicked);
        
        // Preencher dropdowns
        classDropdown.ClearOptions();
        classDropdown.AddOptions(new List<string> { "Guerreiro", "Mago", "Arqueiro", "Ladino", "Clérigo" });
        
        raceDropdown.ClearOptions();
        raceDropdown.AddOptions(new List<string> { "Humano", "Elfo", "Elfo Negro", "Orc", "Anão" });
    }

    void SetupSocketEvents()
    {
        SocketIOManager.Instance.On("get_servers_response", (response) =>
        {
            var data = SocketIOManager.GetData<ServersResponse>(response);
            if (data.success)
            {
                Debug.Log("Servidores carregados: " + data.servers.Length);
                // Aqui você pode mostrar a lista de servidores
            }
        });
        
        SocketIOManager.Instance.On("get_characters_response", (response) =>
        {
            var data = SocketIOManager.GetData<CharactersResponse>(response);
            if (data.success)
            {
                characters = data.characters;
                PopulateCharacterList();
            }
        });
        
        SocketIOManager.Instance.On("create_character_response", (response) =>
        {
            var data = SocketIOManager.GetData<CreateCharacterResponse>(response);
            if (data.success)
            {
                Debug.Log("Personagem criado com sucesso!");
                LoadCharacters();
                ShowCharacterSelection();
            }
            else
            {
                Debug.LogError("Erro ao criar personagem: " + data.error);
            }
        });
        
        SocketIOManager.Instance.On("select_character_response", (response) =>
        {
            var data = SocketIOManager.GetData<SelectCharacterResponse>(response);
            if (data.success)
            {
                Debug.Log("Personagem selecionado: " + data.character.name);
                SocketIOManager.Instance.characterId = data.character.id;
                EnterGameWorld();
            }
            else
            {
                Debug.LogError("Erro ao selecionar personagem: " + data.error);
            }
        });
    }

    public void LoadCharacters()
    {
        SocketIOManager.Instance.Emit("get_characters");
    }

    void PopulateCharacterList()
    {
        // Limpar lista atual
        foreach (Transform child in characterListContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Adicionar personagens
        foreach (var character in characters)
        {
            var buttonObj = Instantiate(characterButtonPrefab, characterListContainer);
            var button = buttonObj.GetComponent<CharacterButton>();
            button.Setup(character, OnCharacterSelected);
        }
        
        // Botão para criar novo personagem
        var createButtonObj = Instantiate(characterButtonPrefab, characterListContainer);
        var createButton = createButtonObj.GetComponent<CharacterButton>();
        createButton.SetupAsCreateButton(OnCreateNewCharacterClicked);
    }

    void OnCharacterSelected(CharacterData character)
    {
        var selectData = new SelectCharacterRequest
        {
            characterId = character.id
        };
        
        SocketIOManager.Instance.Emit("select_character", selectData);
    }

    void OnCreateNewCharacterClicked()
    {
        ShowCharacterCreation();
    }

    void OnCreateCharacterClicked()
    {
        if (string.IsNullOrEmpty(characterNameInput.text))
        {
            Debug.LogError("Digite um nome para o personagem");
            return;
        }
        
        // Mapear para os valores do servidor
        string classValue = classDropdown.value switch
        {
            0 => "warrior",
            1 => "mage",
            2 => "archer",
            3 => "rogue",
            4 => "cleric",
            _ => "warrior"
        };
        
        string raceValue = raceDropdown.value switch
        {
            0 => "human",
            1 => "elf",
            2 => "dark_elf",
            3 => "orc",
            4 => "dwarf",
            _ => "human"
        };
        
        var createData = new CreateCharacterRequest
        {
            name = characterNameInput.text,
            @class = classValue,
            race = raceValue
        };
        
        SocketIOManager.Instance.Emit("create_character", createData);
    }

    void ShowCharacterSelection()
    {
        characterSelectionPanel.SetActive(true);
        characterCreationPanel.SetActive(false);
        LoadCharacters();
    }

    void ShowCharacterCreation()
    {
        characterSelectionPanel.SetActive(false);
        characterCreationPanel.SetActive(true);
        characterNameInput.text = "";
    }

    void EnterGameWorld()
    {
        Debug.Log("Entrando no mundo do jogo...");
        // Aqui você carregaria a cena do jogo
    }

    [System.Serializable]
    public class ServersResponse
    {
        public bool success;
        public ServerData[] servers;
    }

    [System.Serializable]
    public class ServerData
    {
        public int id;
        public string name;
        public string status;
        public int player_count;
        public int max_players;
    }

    [System.Serializable]
    public class CharactersResponse
    {
        public bool success;
        public string error;
        public List<CharacterData> characters;
    }

    [System.Serializable]
    public class CreateCharacterRequest
    {
        public string name;
        public string @class;
        public string race;
    }

    [System.Serializable]
    public class CreateCharacterResponse
    {
        public bool success;
        public string error;
        public string characterId;
    }

    [System.Serializable]
    public class SelectCharacterRequest
    {
        public string characterId;
    }

    [System.Serializable]
    public class SelectCharacterResponse
    {
        public bool success;
        public string error;
        public CharacterData character;
    }

    [System.Serializable]
    public class CharacterData
    {
        public string id;
        public string name;
        public string @class;
        public string race;
        public int level;
        public int exp;
        public int str;
        public int dex;
        public int vit;
        public int @int;
        public int luk;
        public int hp;
        public int max_hp;
        public int mp;
        public int max_mp;
        public float pos_x;
        public float pos_y;
        public float pos_z;
        public string map;
    }
}