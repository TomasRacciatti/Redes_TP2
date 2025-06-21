using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using System.Linq;

public class PlayerController : NetworkBehaviour
{
    [Header("Dice Settings")] [SerializeField]
    private int _maxDice = 5;
    
    [Networked] public int RemainingDice { get; set; }

    public List<int> RolledDice { get; set; } = new List<int>();

    public bool IsAlive => RemainingDice > 0;

    [Networked, OnChangedRender(nameof(OnTurnIdChanged))]
    public int myTurnId { get; set; }


    public override void Spawned()
    {
        for (int i = 0; i < _maxDice; i++)
        {
            RolledDice.Add(1);
        }

        RemainingDice = _maxDice;

        if (HasInputAuthority)
        {
            UIManager.Instance.SetPlayerReference(this);
        }

        GameManager.Instance.RegisterPlayer(this);
    }

    private void OnTurnIdChanged()
    {
        if (HasInputAuthority)
        {
            UIManager.Instance?.UpdateTurnIndicator();
        }
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_ReceiveRolledDice(int[] diceValues)
    {
        RolledDice = new List<int>(diceValues);
        UIManager.Instance.UpdateRolledDice(RolledDice);
    }
    
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_NotifyDiceLost(int newRemaining)
    {
        RemainingDice = newRemaining;
        
        if (RolledDice.Count > RemainingDice)
            RolledDice.RemoveAt(RolledDice.Count - 1);

        UIManager.Instance.UpdateRolledDice(RolledDice);
    }
    
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestAdvanceTurnFromHost()
    {
        GameManager.Instance.AdvanceTurnHostAuthoritative();
    }
}