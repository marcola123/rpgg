using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class LootSystem : NetworkBehaviour
{
    public static LootSystem Instance { get; private set; }

    [SerializeField] private NetworkPrefabRef _lootItemPrefab; // Prefab do item de loot (NetworkObject)

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Chamado pelo servidor quando um monstro morre
    public void DropLoot(Vector3 position, List<ItemData> itemsToDrop)
    {
        if (!Runner.IsServer) return; // Apenas o servidor pode dropar loot

        foreach (var itemData in itemsToDrop)
        {
            // Spawnar o prefab do item de loot na rede
            NetworkObject lootObject = Runner.Spawn(_lootItemPrefab, position + Random.insideUnitSphere * 0.5f, Quaternion.identity);
            if (lootObject.TryGetBehaviour<LootItem>(out var lootItem))
            {
                lootItem.Initialize(itemData.ItemName); // Passar o nome do item para o LootItem
            }
        }
    }

    // RPC para o cliente solicitar a coleta de um item
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestPickup(PlayerRef player, NetworkId lootItemId)
    {
        if (!Runner.IsServer) return; // Apenas o servidor processa a coleta

        if (Runner.TryFindObject(lootItemId, out NetworkObject lootObject))
        {
            if (lootObject.TryGetBehaviour<LootItem>(out var lootItem))
            {
                // TODO: Adicionar item ao inventário do jogador
                // InventorySystem.Instance.AddItem(player, lootItem.ItemName);
                Debug.Log($"Player {player.PlayerId} coletou {lootItem.ItemName}.");

                Runner.Despawn(lootObject); // Despawnar o item de loot da rede
            }
        }
    }
}

