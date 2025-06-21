using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
using Unity.VisualScripting;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private PlayerSpawner _playerSpawner;
    [SerializeField] private GameObject _gameStartOverlay;

    [Networked] public int currentClaimQuantity { get; set; }
    [Networked] public int currentClaimFace { get; set; }

    public PlayerRef turnAuthority { get; set; }

    public int currentTurnId { get; set; }

    private int _lastTurnID;
    private PlayerController LastPlayer => _players.First(p => p.myTurnId == _lastTurnID);

    private List<PlayerController> _players = new List<PlayerController>();
    public IReadOnlyList<PlayerController> Players => _players;

    private bool _isFirstTurn;
    private bool _gameStarted;
    

    
    private void OnEnable()
    {
        _playerSpawner.OnPlayerDisconnected += HandlePlayerDisconnected;
    }

    private void OnDisable()
    {
        _playerSpawner.OnPlayerDisconnected -= HandlePlayerDisconnected;
    }
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        else
            Destroy(gameObject);
        
        _playerSpawner = FindObjectOfType<PlayerSpawner>();

        if (_playerSpawner == null)
        {
            Debug.LogError("[GameManager] PlayerSpawner not found in scene!");
        }
    }

    public void RegisterPlayer(PlayerController player)
    {
        if (!_players.Contains(player))
        {
            _players.Add(player);
            UIManager.Instance.UpdateSessionLobby(_players);
        }
    }
    
    public void OnStartButtonClicked() // Lo llamamos por boton
    {
        if (!HasStateAuthority) return;

        RPC_RequestStartGame();
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_RequestStartGame()
    {
        TryStartGame();
    }

    private void TryStartGame()
    {
        if (_gameStarted || _players.Count < 2) return;
        _gameStarted = true;
        _gameStartOverlay.SetActive(false);
        
        StartCoroutine(DelayedStart());
    }

    private void AssignTurnIDs()
    {
        var ordered = ActivePlayers();

        int id = 1;
        foreach (var p in ordered)
            p.myTurnId = id++;
    }
    
    public void HandlePlayerDisconnected(PlayerRef player)
    {
        if (!HasStateAuthority) return;

        var playerController = GetPlayerController(player);
        if (playerController != null)
        {
            RemoveFromList(playerController);
        }
        
        AssignTurnIDs();
        
        if (_players.Count > 1 && playerController.myTurnId == currentTurnId)
        {
            var next = _players.First();
            currentTurnId = next.myTurnId;
            turnAuthority = next.Object.InputAuthority;
            
            RPC_AdvanceTurn(); 
        }
    }

    private IEnumerable<PlayerController> ActivePlayersGenerator()
    {
        var active = _players
            .Where(p => p.IsAlive)
            .OrderBy(p => p.Object.InputAuthority.RawEncoded);
        //.ToList();

        foreach (var player in active)
        {
            yield return player;
        }
    }

    private List<PlayerController> ActivePlayers()
    {
        return ActivePlayersGenerator().ToList();
    }

    private IEnumerator DelayedStart()
    {
        yield return null;

        AssignTurnIDs();

        var championRaw = _players
            .Min(p => p.Object.InputAuthority.RawEncoded);

        if (Runner.LocalPlayer.RawEncoded == championRaw && HasStateAuthority)
        {
            var alive = ActivePlayers();

            int idx = UnityEngine.Random.Range(0,
                alive.Count); // Si tengo mas de 2 jugadores, esto lo voy a tener que settear tambien cuando muere un jugador
            var first = alive[idx];

            RPC_StartGame(first.Object.InputAuthority, first.myTurnId);
        }
    }

    private void StartRound(PlayerRef firstAuthority, int firstTurnId)
    {
        UIManager.Instance.HideRoundSummary();

        _isFirstTurn = true;
        currentClaimQuantity = 0;
        currentClaimFace = 1;

        turnAuthority = firstAuthority;
        currentTurnId = firstTurnId;

        if (HasStateAuthority)
        {
            RollAllPlayersDice();
        }

        UIManager.Instance.UpdateDiceCounts(_players);
        UpdateUI();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_StartGame(PlayerRef firstAuthority, int firstTurnId)
    {
        StartRound(firstAuthority, firstTurnId);
    }
    
    private void RollAllPlayersDice()
    {
        foreach (var player in _players)
        {
            var rolled = new int[player.RemainingDice];
            for (int i = 0; i < player.RemainingDice; i++)
            {
                rolled[i] = UnityEngine.Random.Range(1, 7);
            }
            player.RolledDice = new List<int>(rolled);
            
            player.RPC_ReceiveRolledDice(rolled);
        }
    }

    public void RequestNextTurn()
    {
        if (Runner.LocalPlayer != turnAuthority)
            return;

        RPC_AdvanceTurn();
    }

    [Rpc(RpcSources.All, RpcTargets.All)] 
    private void RPC_AdvanceTurn()
    {
        var alive = ActivePlayers();

        int index = alive.FindIndex(p => p.myTurnId == currentTurnId);
        int nextIdx = (index + 1) % alive.Count;
        var next = alive[nextIdx];

        turnAuthority = next.Object.InputAuthority;
        currentTurnId = next.myTurnId;

        UpdateUI();
    }

    private void UpdateUI()
    {
        UIManager.Instance.UpdateTurnIndicator();
        UIManager.Instance.UpdateClaim(currentClaimQuantity, currentClaimFace);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetClaim(int quantity, int face) // Lo llamamos en ClaimController que lo llama por boton
    {
        currentClaimQuantity = quantity;
        currentClaimFace = face;
        _isFirstTurn = false;
        _lastTurnID = currentTurnId;
        
        RequestNextTurn();
    }

    public void CallBluff() // Lo llamamos en el boton
    {
        if (_isFirstTurn)
            return;

        RPC_ResolveBluff();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_ResolveBluff()
    {
        if (!HasStateAuthority) return;

        var claimResult = CheckClaim(currentClaimFace, currentClaimQuantity);

        int face = claimResult.Item1;
        int actualQty = claimResult.Item2;
        bool honest = claimResult.Item3;

        var caller = _players.First(p => p.myTurnId == currentTurnId);
        var claimant = LastPlayer;
        var loser = honest ? caller : claimant;

        StartCoroutine(RoundSummaryRoutineAndAdvance(loser.myTurnId));
    }

    private Tuple<int, int, bool> CheckClaim(int claimFace, int claimQuantity)
    {
        var dist = GetDiceDistribution();
        dist.TryGetValue(claimFace, out int actualQty);
        bool honest = actualQty >= claimQuantity;
        return Tuple.Create(claimFace, actualQty, honest);
    }

    public List<Tuple<int, int>> GetPlayerDiceTuples()
    {
        return _players
            .Where(p => p.IsAlive)
            .OrderBy(p => p.myTurnId)
            .Select(p => Tuple.Create(p.myTurnId, p.RemainingDice))
            .ToList();
    }

    private IEnumerator RoundSummaryRoutineAndAdvance(int loserID)
    {
        int claimQty = currentClaimQuantity;
        int claimFace = currentClaimFace;
        var dist = GetDiceDistribution();
        int[] distArray = Enumerable.Range(1, 6)
            .Select(face => dist.TryGetValue(face, out var qty) ? qty : 0)
            .ToArray();

        UIManager.Instance.StartCoroutine(
            UIManager.Instance.ShowSummaryControlled(dist, delayBetween: 0.05f,
                callback: () => { RPC_ShowRoundSummary(claimQty, claimFace, loserID, distArray); }
            )
        );


        yield return new WaitForSeconds(3.5f);

        var loser = _players.First(p => p.myTurnId == loserID);
        
        loser.RemainingDice--;
        
        if (loser.RemainingDice <= 0)
        {
            RPC_GameOver(loser.Object.InputAuthority);
        }
        
        loser.RPC_NotifyDiceLost(loser.RemainingDice);

        RPC_StartGame(loser.Object.InputAuthority, loserID);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowRoundSummary(int claimQty, int claimFace, int loserId, int[] distribution)
    {
        var dist = new Dictionary<int, int>();
        for (int i = 1; i <= 6; i++)
        {
            dist[i] = distribution[i - 1];
        }

        UIManager.Instance.ShowRoundSummary(dist, claimQty, claimFace, loserId);
    }


    private Dictionary<int, int> GetDiceDistribution()
    {
        if (!HasStateAuthority) //Duda
            return new Dictionary<int, int>();

        return _players
            .Where(p => p.IsAlive)
            .SelectMany(p => p.RolledDice)
            .Aggregate(new Dictionary<int, int>(), (dict, face) =>
            {
                dict.TryAdd(face, 0);
                dict[face]++;
                return dict;
            });
    }

    /*
    // Original sin Aggregate
    private Dictionary<int, int> GetDiceDistribution()
    {
        Dictionary<int, int> distribution = new();

        foreach (var player in _players)
        {
            if (!player.IsAlive) continue;

            foreach (int face in player.RolledDice)
            {
                if (!distribution.ContainsKey(face))
                    distribution[face] = 0;

                distribution[face]++;
            }
        }

        // DEBUG para revisar proxies
        var sb = new System.Text.StringBuilder("[GetDiceDistribution] ");
        foreach (var kv in distribution)
            sb.Append($"{kv.Key}→{kv.Value}  ");
        Debug.Log(sb.ToString());


        return distribution;
    }
    */

    private void RemoveFromList(PlayerController player)
    {
        _players.Remove(player);
    }

    [Rpc]
    public void RPC_GameOver(PlayerRef client)
    {
        if (client == Runner.LocalPlayer)
        {
            UIManager.Instance.ShowDefeatOverlay();
        }

        var player = GetPlayerController(client);
        RemoveFromList(player);

        if (_players.Count == 1 && HasStateAuthority)
        {
            RPC_Win(_players[0].Object.InputAuthority);
        }

        // Agregar un boton para desconectarse. No hace falta que sea aca, pero la logica es:
        /*
        if (!Object.HasInputAuthority)
        {
            Runner.Disconnect(Object.InputAuthority);
        }
        */
    }

    [Rpc]
    private void RPC_Win([RpcTarget] PlayerRef client)
    {
        UIManager.Instance.ShowVictoryOverlay();
    }

    // Helper para obtener el PLayerController desde playerRef
    private PlayerController GetPlayerController(PlayerRef client)
    {
        return _players.FirstOrDefault(p => p.Object.InputAuthority == client);
    }
}