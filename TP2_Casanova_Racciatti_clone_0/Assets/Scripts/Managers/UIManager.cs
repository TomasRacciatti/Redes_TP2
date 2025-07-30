using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Player Dice UI")] [SerializeField]
    private List<DieDisplay> _rolledDiceDisplays;

    [Header("Claim")] 
    [SerializeField] private TextMeshProUGUI _claimantText;
    [SerializeField] private TextMeshProUGUI _claimAmountText;
    [SerializeField] private DieDisplay _currentClaimDie;

    [Header("Turn")] [SerializeField] private GameObject _turnOverlay;
    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private GameObject _actionButtons;

    [Header("Players")] 
    [SerializeField] private TextMeshProUGUI _playerListText;
    [SerializeField] private TextMeshProUGUI _playerLobbyListText;

    [Header("Round Information")] [SerializeField]
    private GameObject _roundInfoPanel;

    [SerializeField] private TextMeshProUGUI _diceDistributionText;
    [SerializeField] private TextMeshProUGUI _claimText;
    [SerializeField] private TextMeshProUGUI _loserText;

    [Header("Game Over")] [SerializeField] private GameObject _winnerOverlay;
    [SerializeField] private GameObject _loserOverlay;
    [SerializeField] private GameObject _GameOverButtons;


    private PlayerController _localPlayer;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(gameObject);
    }

    public void SetPlayerReference(PlayerController player)
    {
        _localPlayer = player;
    }

    public void UpdateClaim(int quantity, int face) // Que incluya el nickname
    {
        var lastPlayer  = GameManager.Instance.Players
            .FirstOrDefault(p => p.MyTurnId == GameManager.Instance.LastTurnId);
        
        string name = lastPlayer != null ? lastPlayer.Nickname : $"Player {GameManager.Instance.LastTurnId}";

        _claimantText.text = $"{name}'s claim: ";
        _claimAmountText.text = quantity.ToString();
        _currentClaimDie.ShowValue(face);
    }

    public void UpdateTurnIndicator()
    {
        if (_localPlayer == null) return;

        bool isMyTurn = _localPlayer.MyTurnId > 0 && _localPlayer.MyTurnId == GameManager.Instance.currentTurnId;

        _turnOverlay.SetActive(!isMyTurn);
        _actionButtons.SetActive(isMyTurn);

        var turnPlayer = GameManager.Instance.Players
            .FirstOrDefault(p => p.MyTurnId == GameManager.Instance.currentTurnId);

        string name = turnPlayer != null ? turnPlayer.Nickname : $"Player {GameManager.Instance.currentTurnId}";
        _turnText.text = $"{name}'s turn";
    }

    public void UpdateDiceCounts(List<PlayerController> players)
    {
        var diceTuples = GameManager.Instance.GetPlayerDiceTuples();
        
        var lines = diceTuples
            .Select(t => $"{t.Item1}: {t.Item2}")
            .ToList();

        _playerListText.text = string.Join("\n", lines);
    }

    public void UpdateSessionLobby(List<PlayerController> players)
    {
        _playerLobbyListText.text = "";
        
        var sb = new System.Text.StringBuilder();

        foreach (var playerController in players)
        {
            sb.AppendLine($"{playerController.Nickname}\n");
        }
        
        _playerLobbyListText.text = sb.ToString();
    }

    public void UpdateRolledDice(List<int> rolledValues)
    {
        for (int i = 0; i < _rolledDiceDisplays.Count; i++)
        {
            bool slotActive = i < rolledValues.Count;

            _rolledDiceDisplays[i].gameObject.SetActive(slotActive);

            if (slotActive)
                _rolledDiceDisplays[i].ShowValue(rolledValues[i]);
        }
    }

    public void ShowRoundSummary(Dictionary<int, int> distribution, int claimQuantity, int claimFace, int loserTurnId)
    {
        distribution.TryGetValue(claimFace, out var claimCount);
        var honest = claimCount >= claimQuantity;
        
        var loserName = GameManager.Instance.GetNicknameFromTurnId(loserTurnId);
        
        var summaryInfo = new
        {
            Rows = Enumerable.Range(1, 6)
                .Select(face => new
                {
                    Face  = face,
                    Count = distribution.TryGetValue(face, out var qty) ? qty : 0
                }),
            
            ClaimText = $"Claim: {claimFace} → {claimQuantity}",
            
            LoserText = honest
                ? $"Claim was honest.\n{loserName} loses a die"
                : $"Claim was a lie.\n{loserName} loses a die"
        };
        
        var sb = new System.Text.StringBuilder();
        
        foreach (var row in summaryInfo.Rows)
            sb.AppendLine($"{row.Face} → {row.Count}");
        
        _diceDistributionText.text = sb.ToString();

        _claimText.text = summaryInfo.ClaimText;
        _loserText.text = summaryInfo.LoserText;
        
        _roundInfoPanel.SetActive(true);
    }

    public IEnumerator ShowSummaryControlled(Dictionary<int, int> dist, float delayBetween = 0.01f,
        Action callback = null)
    {
        //_roundInfoPanel.SetActive(true);
        _diceDistributionText.text = "";
        
        for (int face = 1; face <= 6; face++) {
            dist.TryGetValue(face, out var cnt);
            _diceDistributionText.text += $"{face} → {cnt}\n";
            
            yield return new WaitForSeconds(delayBetween);
        }
        
        callback?.Invoke();
    }

    public void HideRoundSummary()
    {
        _roundInfoPanel.SetActive(false);
    }

    public void ShowDefeatOverlay()
    {
        _loserOverlay.SetActive(true);
        _GameOverButtons.SetActive(true);
    }

    public void ShowVictoryOverlay()
    {
        _winnerOverlay.SetActive(true);
        _GameOverButtons.SetActive(true);
    }

    public void OnReturnToMenuButtonClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.Runner.IsServer)
        {

            if (GameManager.Instance.Players.Count > 1)
            {
                return;
            }
        }

        StartCoroutine(ReturnToMenuRoutine());
    }
    
    private IEnumerator ReturnToMenuRoutine()
    {
        yield return GameManager.Instance.Runner.Shutdown();
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
}