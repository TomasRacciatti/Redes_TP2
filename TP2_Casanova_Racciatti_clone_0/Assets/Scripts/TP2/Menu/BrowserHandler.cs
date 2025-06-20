using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BrowserHandler : MonoBehaviour
{
    [SerializeField] private SessionItem _sessionItemPrefab;
    [SerializeField] private NetworkRunnerHandler _runnerHandler;
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private VerticalLayoutGroup _verticalLayout;

    private void OnEnable()
    {
        _runnerHandler.OnSessionListUpdate += RecieveSessionList;
    }

    private void OnDisable()
    {
        _runnerHandler.OnSessionListUpdate -= RecieveSessionList;
    }

    private void RecieveSessionList(List<SessionInfo> sessionList)
    {
        ClearBrowser();

        if (sessionList.Count == 0)
        {
            _statusText.gameObject.SetActive(true);
            return;
        }
        
        foreach (var sessionInfo in sessionList)
        {
            AddToSessionBrowser(sessionInfo);
        }
    }

    private void AddToSessionBrowser(SessionInfo sessionInfo)
    {
        var newSessionItem = Instantiate(_sessionItemPrefab, _verticalLayout.transform);
        newSessionItem.Initialize(sessionInfo);
        newSessionItem.OnJoinSession += JoinSelectedSession;
    }

    private void JoinSelectedSession(SessionInfo sessionInfo)
    {
        _runnerHandler.JoinGame(sessionInfo);
    }

    private void ClearBrowser()
    {
        foreach (Transform child in _verticalLayout.transform)
        {
            Destroy(child.gameObject);
        }
        
        _statusText.gameObject.SetActive(false);
    }
}
