using System;                      // Para ArraySegment<>
using System.Collections.Generic;  // Para List<> e Dictionary<>
using Fusion;
using Fusion.Sockets;               // Para HostMigrationToken, ReliableKey, NetAddress, NetConnectFailedReason, NetDisconnectReason
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.Linq; // Necessário para Count() em IEnumerable

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [SerializeField] private NetworkRunner _runnerPrefab;
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    [SerializeField] private NetworkPrefabRef _monsterPrefab;
    [SerializeField] private NetworkPrefabRef _combatSystemPrefab;
    [SerializeField] private NetworkPrefabRef _lootSystemPrefab;
    [SerializeField] private NetworkPrefabRef _inventorySystemPrefab;

    // Adicione referências de cena no inspector
    [SerializeField] private SceneRef characterSelectSceneRef;
    [SerializeField] private SceneRef worldSceneRef;

    private NetworkRunner _runner;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public async System.Threading.Tasks.Task StartGame(GameMode mode, string roomName)
    {
        _runner = Instantiate(_runnerPrefab);
        _runner.AddCallbacks(this);
        _runner.ProvideInput = true;

        var startGameArgs = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomName,
            SceneManager = _runner.GetComponent<NetworkSceneManagerDefault>()
        };

        await _runner.StartGame(startGameArgs);
    }

    public void ShutdownGame()
    {
        if (_runner != null)
        {
            _runner.Shutdown();
            _runner = null;
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player.PlayerId} joined.");
        if (runner.IsServer && runner.ActivePlayers.Count() == 1)
        {
		runner.GetComponent<NetworkSceneManagerDefault>().LoadScene(
		characterSelectSceneRef,
		new NetworkLoadSceneParameters()
		);

        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player.PlayerId} left.");
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new PlayerController.NetworkInputData();
        data.HorizontalInput = Input.GetAxis("Horizontal");
        data.VerticalInput = Input.GetAxis("Vertical");
        data.IsAttackPressed = Input.GetMouseButtonDown(0);
        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"Shutdown: {shutdownReason}");
        SceneManager.LoadScene("LoginScene");
    }

    public void OnConnectedToServer(NetworkRunner runner) { Debug.Log("Connected to server."); }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { Debug.LogError($"Connect failed: {reason}"); }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { request.Accept(); }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("Scene Load Done.");
        if (runner.IsServer && SceneManager.GetActiveScene().name == "WorldScene")
        {
            if (CombatSystem.Instance == null && _combatSystemPrefab.IsValid)
                runner.Spawn(_combatSystemPrefab, Vector3.zero, Quaternion.identity);
            if (LootSystem.Instance == null && _lootSystemPrefab.IsValid)
                runner.Spawn(_lootSystemPrefab, Vector3.zero, Quaternion.identity);
            if (InventorySystem.Instance == null && _inventorySystemPrefab.IsValid)
                runner.Spawn(_inventorySystemPrefab, Vector3.zero, Quaternion.identity);

            SpawnPlayer(runner, runner.LocalPlayer);
            SpawnMonster(runner, new Vector3(5, 0, 5));
            SpawnMonster(runner, new Vector3(-5, 0, -5));
        }
    }

    public void OnSceneLoadStart(NetworkRunner runner) { Debug.Log("Scene Load Start."); }

    public void LoadWorldScene()
    {
        if (_runner != null && _runner.IsServer)
        {
            _runner.GetComponent<NetworkSceneManagerDefault>().LoadScene(
    worldSceneRef,
    new NetworkLoadSceneParameters()
);

        }
    }

    private void SpawnPlayer(NetworkRunner runner, PlayerRef playerRef)
    {
        if (_playerPrefab.IsValid)
        {
            string characterDataJson = PlayerPrefs.GetString("LastCreatedCharacterData", string.Empty);
            CharacterData characterData = null;
            if (!string.IsNullOrEmpty(characterDataJson))
                characterData = JsonConvert.DeserializeObject<CharacterData>(characterDataJson);

            Vector3 spawnPosition = Vector3.zero;
            NetworkObject playerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, playerRef);
            Debug.Log($"Player {playerRef.PlayerId} spawned with NetworkObject {playerObject.Id}.");

            if (playerObject.TryGetBehaviour<PlayerController>(out var playerController))
            {
                if (characterData != null)
                    playerController.InitPlayerData(characterData);
                else
                    playerController.InitPlayerData(new CharacterData("DefaultPlayer", "Warrior", "Human"));
            }
        }
        else
        {
            Debug.LogError("Player Prefab não está configurado no NetworkManager.");
        }
    }

    private void SpawnMonster(NetworkRunner runner, Vector3 position)
    {
        if (_monsterPrefab.IsValid)
        {
            runner.Spawn(_monsterPrefab, position, Quaternion.identity);
            Debug.Log($"Monstro spawnado em {position}.");
        }
        else
            Debug.LogError("Monster Prefab não está configurado no NetworkManager.");
    }

    public NetworkRunner GetRunner() => _runner;
}
