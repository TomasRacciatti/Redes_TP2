using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class SetPlayerNickname : NetworkBehaviour
{
    [Networked] private NetworkString<_16> CurrentNickname {get; set;}

    private ChangeDetector _changeDetector;

    public event Action onLeft;
    
    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        
        if (HasInputAuthority)
        {
            NetworkString<_16> loadedNickname;
            
            if (PlayerPrefs.HasKey("PlayerNickname"))
            {
                loadedNickname = PlayerPrefs.GetString("PlayerNickname");
            }
            else
            {
                loadedNickname = $"Player {Runner.LocalPlayer.PlayerId}";
            }

            RPC_SendNickname(loadedNickname);
        }
    }
    
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        onLeft?.Invoke();
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(CurrentNickname):
                {
                    UpdateNickname();
                    break;
                }
            }
        }
    }

    private void UpdateNickname()
    {
        
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)] 
    private void RPC_SendNickname(NetworkString<_16> nickname)
    {
        CurrentNickname = nickname;
    }
}
