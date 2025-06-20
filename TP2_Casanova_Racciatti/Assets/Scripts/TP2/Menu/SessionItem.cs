using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _sessionName;
    [SerializeField] private TMP_Text _playerCount;
    [SerializeField] private Button _joinButton;


    public event Action<SessionInfo> OnJoinSession;

    public void Initialize(SessionInfo sessionInfo)
    {
        _sessionName.text = sessionInfo.Name;
        _playerCount.text = $"{sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers}";

        if (sessionInfo.PlayerCount >= sessionInfo.MaxPlayers)
        {
            _joinButton.interactable = false;
            return;
        }

        _joinButton.onClick.AddListener(() => { OnJoinSession?.Invoke(sessionInfo); });
    }
}