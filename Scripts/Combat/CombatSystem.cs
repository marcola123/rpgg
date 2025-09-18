using Fusion;
using UnityEngine;

public class CombatSystem : NetworkBehaviour
{
    public static CombatSystem Instance { get; private set; }

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

    // RPC para o servidor processar um ataque
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_PerformAttack(PlayerRef attacker, NetworkId targetId, int damage)
    {
        if (!Runner.IsServer) return; // Apenas o servidor processa ataques

        if (Runner.TryFindObject(targetId, out NetworkObject targetObject))
        {
            if (targetObject.TryGetBehaviour<MonsterController>(out var monster))
            {
                monster.TakeDamage(damage);
            }
            else if (targetObject.TryGetBehaviour<PlayerController>(out var player))
            {
                // Lógica para dano entre jogadores (PvP)
                // player.TakeDamage(damage);
            }
        }
    }

    // RPC para notificar clientes sobre dano (opcional, pode ser feito via Networked properties)
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NotifyDamage(NetworkId targetId, int damage, int remainingHP)
    {
        // Clientes podem usar isso para atualizar a UI de HP, por exemplo
        Debug.Log($"Objeto {targetId} recebeu {damage} de dano. HP restante: {remainingHP}");
    }
}

