using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Collections.Generic;
using Newtonsoft.Json;

public class CharacterCreationUIHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private TMP_Dropdown _classDropdown;
    [SerializeField] private TMP_Dropdown _raceDropdown;
    [SerializeField] private Button _createButton;
    [SerializeField] private TextMeshProUGUI _statusText;

    private CharacterManager _characterManager;
    private NetworkManager _networkManager;

    void Start()
    {
        _characterManager = FindObjectOfType<CharacterManager>();
        if (_characterManager == null)
        {
            Debug.LogError("CharacterManager não encontrado na cena.");
            // Adiciona um CharacterManager se não houver um na cena
            _characterManager = gameObject.AddComponent<CharacterManager>();
        }

        _networkManager = NetworkManager.Instance;
        if (_networkManager == null)
        {
            Debug.LogError("NetworkManager não encontrado na cena.");
        }

        _createButton.onClick.AddListener(OnCreateButtonClicked);
        _statusText.text = "";

        PopulateDropdowns();
    }

    private void PopulateDropdowns()
    {
        _classDropdown.ClearOptions();
        _classDropdown.AddOptions(new List<string> { "Guerreiro", "Mago", "Arqueiro" });

        _raceDropdown.ClearOptions();
        _raceDropdown.AddOptions(new List<string> { "Humano", "Elfo", "Anão" });
    }

    private async void OnCreateButtonClicked()
    {
        string characterName = _nameInput.text;
        string characterClass = _classDropdown.options[_classDropdown.value].text;
        string characterRace = _raceDropdown.options[_raceDropdown.value].text;

        if (string.IsNullOrWhiteSpace(characterName))
        {
            _statusText.text = "O nome do personagem não pode ser vazio.";
            return;
        }

        if (!_characterManager.IsCharacterNameUnique(characterName))
        {
            _statusText.text = $"O nome \'{characterName}\' já está em uso.";
            return;
        }

        _statusText.text = "Criando personagem...";
        _createButton.interactable = false;

        CharacterData newCharacter = new CharacterData(characterName, characterClass, characterRace);
        _characterManager.SaveCharacter(newCharacter);

        // Salva os dados do personagem recém-criado em PlayerPrefs para que o NetworkManager possa acessá-los
        PlayerPrefs.SetString("LastCreatedCharacterData", JsonConvert.SerializeObject(newCharacter));
        PlayerPrefs.Save();

        _statusText.text = $"Personagem \'{characterName}\' criado com sucesso! Entrando no mundo...";

        if (_networkManager != null)
        {
            _networkManager.LoadWorldScene();
        }
        else
        {
            Debug.LogError("NetworkManager não disponível para carregar a cena do mundo.");
            _createButton.interactable = true;
        }
    }
}

