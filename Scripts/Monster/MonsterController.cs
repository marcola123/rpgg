using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class MonsterController : NetworkBehaviour
{
    [Networked]
    public MonsterData Data { get; set; }

    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _chaseRange = 10f;
    [SerializeField] private float _attackRange = 2f;
    [SerializeField] private float _attackCooldown = 2f;
    [SerializeField] private int _attackDamage = 10; // Dano do monstro
    [SerializeField] private List<ItemData> _lootTable; // Itens que o monstro pode dropar

    private enum MonsterState { Patrolling, Chasing, Attacking, Dead }
    [Networked]
    private MonsterState _currentState { get; set; }

    private Vector3 _patrolTarget;
    private TickTimer _attackTimer;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Data = new MonsterData();
            Data.Init("Goblin", 50, 10); // Exemplo de inicialização
            _currentState = MonsterState.Patrolling;
            SetNewPatrolTarget();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return; // Apenas o servidor controla a lógica do monstro

        switch (_currentState)
        {
            case MonsterState.Patrolling:
                Patrol();
                break;
            case MonsterState.Chasing:
                Chase();
                break;
            case MonsterState.Attacking:
                Attack();
                break;
            case MonsterState.Dead:
                // Monstro morto, aguardando despawn ou respawn
                break;
        }

        // Lógica de morte
        if (Data.HP <= 0 && _currentState != MonsterState.Dead)
        {
            _currentState = MonsterState.Dead;
            Debug.Log($"{Data.MonsterName} morreu!");
            // Dropar loot
            if (LootSystem.Instance != null && _lootTable != null && _lootTable.Count > 0)
            {
                LootSystem.Instance.DropLoot(transform.position, _lootTable);
            }
            Runner.Despawn(Object);
        }
    }

    private void Patrol()
    {
        // Encontrar o jogador mais próximo
        PlayerController nearestPlayer = FindNearestPlayer();
        if (nearestPlayer != null && Vector3.Distance(transform.position, nearestPlayer.transform.position) <= _chaseRange)
        {
            _currentState = MonsterState.Chasing;
            return;
        }

        // Mover para o ponto de patrulha
        transform.position = Vector3.MoveTowards(transform.position, _patrolTarget, _moveSpeed * Runner.DeltaTime);
        if (Vector3.Distance(transform.position, _patrolTarget) < 0.5f)
        {
            SetNewPatrolTarget();
        }
    }

    private void Chase()
    {
        PlayerController nearestPlayer = FindNearestPlayer();
        if (nearestPlayer == null || Vector3.Distance(transform.position, nearestPlayer.transform.position) > _chaseRange)
        {
            _currentState = MonsterState.Patrolling;
            SetNewPatrolTarget(); // Volta a patrulhar se o jogador estiver muito longe
            return;
        }

        if (Vector3.Distance(transform.position, nearestPlayer.transform.position) <= _attackRange)
        {
            _currentState = MonsterState.Attacking;
            _attackTimer = TickTimer.CreateFromSeconds(Runner, _attackCooldown);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, nearestPlayer.transform.position, _moveSpeed * Runner.DeltaTime);
        transform.LookAt(nearestPlayer.transform.position); // Olhar para o jogador
    }

    private void Attack()
    {
        PlayerController nearestPlayer = FindNearestPlayer();
        if (nearestPlayer == null || Vector3.Distance(transform.position, nearestPlayer.transform.position) > _attackRange)
        {
            _currentState = MonsterState.Chasing;
            return;
        }

        transform.LookAt(nearestPlayer.transform.position); // Continuar olhando para o jogador

        if (_attackTimer.Expired(Runner))
        {
            Debug.Log($"{Data.MonsterName} atacou {nearestPlayer.Data.CharacterName}!");
            // Chamar CombatSystem para aplicar dano
            if (CombatSystem.Instance != null)
            {
                CombatSystem.Instance.RPC_PerformAttack(Object.InputAuthority, nearestPlayer.Object.Id, _attackDamage);
            }
            _attackTimer = TickTimer.CreateFromSeconds(Runner, _attackCooldown);
        }
    }

    private PlayerController FindNearestPlayer()
    {
        PlayerController nearestPlayer = null;
        float minDistance = float.MaxValue;

        foreach (var playerObject in Runner.GetAllBehaviours<PlayerController>())
        {
            float distance = Vector3.Distance(transform.position, playerObject.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPlayer = playerObject;
            }
        }
        return nearestPlayer;
    }

    private void SetNewPatrolTarget()
    {
        _patrolTarget = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
    }

    // Método para receber dano (chamado pelo CombatSystem)
    public void TakeDamage(int damage)
    {
        if (!Object.HasStateAuthority) return; // Apenas o servidor processa o dano

        var tempData = Data;
tempData.HP -= damage;
Data = tempData;
        Debug.Log($"{Data.MonsterName} recebeu {damage} de dano. HP restante: {Data.HP}");

        // Notificar clientes sobre a mudança de HP (opcional, pode ser feito via Networked properties)
        // CombatSystem.Instance.RPC_NotifyDamage(Object.Id, damage, Data.HP);
    }
}

