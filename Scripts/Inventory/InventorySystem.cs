using System;                      // Para ArraySegment<>
using System.Collections.Generic;  // Para Dictionary<>
using Fusion;
using Fusion.Sockets;               // Para ReliableKey, NetAddress, NetConnectFailedReason, NetDisconnectReason, HostMigrationToken
using UnityEngine;

public class InventorySystem : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static InventorySystem Instance { get; private set; }
    private Dictionary<PlayerRef, PlayerInventory> _playerInventories = new Dictionary<PlayerRef, PlayerInventory>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public override void Spawned()
    {
        if (Runner.IsServer)
        {
            foreach (var player in Runner.ActivePlayers)
                InitializeInventoryForPlayer(player);
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
            InitializeInventoryForPlayer(player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer && _playerInventories.ContainsKey(player))
            _playerInventories.Remove(player);
    }

    private void InitializeInventoryForPlayer(PlayerRef player)
    {
        if (!_playerInventories.ContainsKey(player))
        {
            _playerInventories.Add(player, new PlayerInventory());
            Debug.Log($"Inventário inicializado para o jogador {player.PlayerId}.");
        }
    }

    public void AddItem(PlayerRef player, string itemName)
    {
        if (Runner.IsServer && _playerInventories.ContainsKey(player))
        {
            _playerInventories[player].AddItem(itemName);
            Debug.Log($"{itemName} adicionado ao inventário do jogador {player.PlayerId}.");
        }
    }

    // Métodos obrigatórios da interface
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}

// Classe de inventário do jogador
public class PlayerInventory
{
    public Dictionary<string, int> Items = new Dictionary<string, int>();

    public void AddItem(string itemName)
    {
        if (Items.ContainsKey(itemName)) Items[itemName]++;
        else Items.Add(itemName, 1);
    }

    public void RemoveItem(string itemName)
    {
        if (Items.ContainsKey(itemName))
        {
            Items[itemName]--;
            if (Items[itemName] <= 0)
                Items.Remove(itemName);
        }
    }
}
