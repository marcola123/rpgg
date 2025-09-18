using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public string ItemName = "New Item";
    public Sprite Icon = null;
    public bool IsStackable = true;
    public int MaxStackSize = 1;
    public ItemType Type = ItemType.Generic;
    // Adicione mais propriedades conforme necessário (ex: dano, defesa, efeitos, etc.)
}

public enum ItemType
{
    Generic,
    Weapon,
    Armor,
    Consumable,
    QuestItem
}

