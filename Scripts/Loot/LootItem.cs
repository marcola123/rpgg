using Fusion;
using UnityEngine;

public class LootItem : NetworkBehaviour
{
    [Networked]
    public NetworkString<_16> ItemName { get; set; }

    public void Initialize(string itemName)
    {
        ItemName = itemName;
    }

    // Detectar colisão com o jogador para permitir a coleta
    private void OnTriggerEnter(Collider other)
    {
        if (Object.HasInputAuthority) // Apenas o cliente com InputAuthority pode solicitar a coleta
        {
            if (other.CompareTag("Player")) // Assumindo que o jogador tem a tag "Player"
            {
                // Solicitar ao servidor para coletar o item
                if (LootSystem.Instance != null)
                {
                    LootSystem.Instance.RPC_RequestPickup(Object.InputAuthority, Object.Id);
                }
            }
        }
    }
}

