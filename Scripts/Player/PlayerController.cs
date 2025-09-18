using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Networked]
    public PlayerData Data { get; set; }

    [SerializeField] private float _moveSpeed = 5f;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Debug.Log($"Player {Object.InputAuthority.PlayerId} (local) spawned. Name: {Data.CharacterName}");
            // Inicializar câmera ou outros componentes específicos do jogador local
        }
        else
        {
            Debug.Log($"Player {Object.InputAuthority.PlayerId} (remote) spawned. Name: {Data.CharacterName}");
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            Vector3 moveDirection = new Vector3(data.HorizontalInput, 0, data.VerticalInput).normalized;
            transform.position += moveDirection * _moveSpeed * Runner.DeltaTime;

            if (moveDirection != Vector3.zero)
            {
                transform.forward = moveDirection;
            }

            // TODO: Implementar lógica de ataque básico
            if (data.IsAttackPressed)
            {
                Debug.Log($"Player {Object.InputAuthority.PlayerId} atacou!");
                // RPC para notificar o servidor sobre o ataque
            }
        }
    }

    // Estrutura para inputs de rede
    public struct NetworkInputData : INetworkInput
    {
        public float HorizontalInput;
        public float VerticalInput;
        public bool IsAttackPressed;
    }

    // Método para inicializar os dados do jogador (chamado pelo NetworkManager)
    public void InitPlayerData(CharacterData characterData)
    {
        Data = new PlayerData();
        Data.Init(characterData.Name, characterData.Class, characterData.Race);
    }
}


