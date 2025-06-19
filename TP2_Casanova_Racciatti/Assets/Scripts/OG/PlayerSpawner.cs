using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] NetworkPrefabRef _playerPrefab;
    private bool _hasSpawned;

    public void PlayerJoined(PlayerRef player)
    {
        //Debug.Log($"[Spawner] PlayerJoined: {player} | Local: {Runner.LocalPlayer}");
        if (_hasSpawned || player != Runner.LocalPlayer)
            return;

        Debug.Log($"[Spawner] Spawning local player for: {player}");
        
        Runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, inputAuthority: player);
        _hasSpawned = true;
    }
}