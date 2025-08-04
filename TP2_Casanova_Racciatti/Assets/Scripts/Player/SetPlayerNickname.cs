using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class SetPlayerNickname : NetworkBehaviour
{
    [Networked] public NetworkString<_16> CurrentNickname {get; set;}

    private ChangeDetector _changeDetector;

    public event Action OnNameUpdated;
    public event Action OnLeft;
    
    
    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        
        if (HasInputAuthority)
        {
            /*
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
            */
            
            string nickname = LocalPlayerData.Instance.Nickname;
            
            if (string.IsNullOrWhiteSpace(nickname))
            {
                nickname = $"Player {Runner.LocalPlayer.PlayerId}";
            }

            RPC_RequestSetNickname(nickname);
        }
    }
    
    /*
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SendNickname(NetworkString<_16> nickname)
    {
        CurrentNickname = nickname;
    }
    */
    
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestSetNickname(string nickname)
    {
        if (HasStateAuthority)
        {
            CurrentNickname = nickname;
            OnNameUpdated?.Invoke();
        }
    }
    
    
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        OnLeft?.Invoke();
    }

    
    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            if (change == nameof(CurrentNickname))
            {
                OnNameUpdated?.Invoke();
            }
        }
    }
}
