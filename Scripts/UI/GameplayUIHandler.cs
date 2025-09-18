using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class GameplayUIHandler : MonoBehaviour
{
    [Header("Player Status UI")]
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private Slider _manaSlider;
    [SerializeField] private TextMeshProUGUI _manaText;
    [SerializeField] private Slider _xpSlider;
    [SerializeField] private TextMeshProUGUI _xpText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _characterNameText;

    [Header("Inventory UI")]
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private Transform _inventorySlotsParent;
    [SerializeField] private GameObject _itemSlotPrefab; // Prefab de um slot de inventário

    private PlayerController _localPlayerController;

    private void Update()
    {
        if (_localPlayerController == null)
        {
            // Tenta encontrar o PlayerController local
            _localPlayerController = FindLocalPlayerController();
            if (_localPlayerController != null)
            {
                InitializeUI();
            }
        }
        else
        {
            UpdatePlayerStatusUI();
            // Atualizar a UI do inventário se necessário
        }
    }

    private PlayerController FindLocalPlayerController()
    {
        foreach (var player in FindObjectsOfType<PlayerController>())
        {
            if (player.Object.HasInputAuthority)
            {
                return player;
            }
        }
        return null;
    }

    private void InitializeUI()
    {
        if (_localPlayerController == null) return;

        _characterNameText.text = _localPlayerController.Data.CharacterName.ToString();
        UpdatePlayerStatusUI();
    }

    private void UpdatePlayerStatusUI()
    {
        if (_localPlayerController == null) return;

        PlayerData data = _localPlayerController.Data;

        _hpSlider.maxValue = data.MaxHP;
        _hpSlider.value = data.HP;
        _hpText.text = $"{data.HP} / {data.MaxHP}";

        _manaSlider.maxValue = data.MaxMana;
        _manaSlider.value = data.Mana;
        _manaText.text = $"{data.Mana} / {data.MaxMana}";

        // TODO: Adicionar lógica de XP e Level no PlayerData
        // _xpSlider.maxValue = ...;
        // _xpSlider.value = data.XP;
        // _xpText.text = ...;

        _levelText.text = $"Level: {data.Level}";
    }

    // TODO: Implementar lógica para atualizar a UI do inventário
    public void UpdateInventoryUI(PlayerInventory inventory)
    {
        // Limpar slots existentes
        foreach (Transform child in _inventorySlotsParent)
        {
            Destroy(child.gameObject);
        }

        // Criar novos slots para cada item no inventário
        foreach (var item in inventory.Items)
        {
            GameObject slot = Instantiate(_itemSlotPrefab, _inventorySlotsParent);
            // Configurar o slot com o ícone e a quantidade do item
            // Ex: slot.GetComponent<Image>().sprite = ...;
            // Ex: slot.GetComponentInChildren<TextMeshProUGUI>().text = item.Value.ToString();
        }
    }
}

