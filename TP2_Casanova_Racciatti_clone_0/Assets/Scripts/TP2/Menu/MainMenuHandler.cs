using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private NetworkRunnerHandler _runnerHandler;
    
    [Header("Panels")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _connectingPanel;
    [SerializeField] private GameObject _browserPanel;
    [SerializeField] private GameObject _hostPanel;
    
    [Header("Buttons")]
    [SerializeField] private Button _connectButton;
    [SerializeField] private Button _goToHostButton;
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _quitButton;
    
    [Header("Texts")]
    [SerializeField] private TMP_Text _connectingText;
    
    [Header("InputFields")] 
    [SerializeField] private TMP_InputField _sessionNameField;

    private void Awake()
    {
        _connectButton.onClick.AddListener(JoinLobby);
        _goToHostButton.onClick.AddListener(ShowHostPanel);
        _hostButton.onClick.AddListener(HostSession);
        _quitButton.onClick.AddListener(QuitGame);
        
        _runnerHandler.OnLobbyJoined += () => 
        { 
            _connectingPanel.SetActive(false);
            _browserPanel.SetActive(true);
        };
    }

    private void JoinLobby()
    {
        _runnerHandler.JoinLobby();
        
        _mainMenuPanel.SetActive(false);
        _connectingPanel.SetActive(true);
        
        _connectingText.text = "Connecting to lobby...";
    }
    
    private void ShowHostPanel()
    {
        _browserPanel.SetActive(false);
        _hostPanel.SetActive(true);
        
        _hostButton.interactable = true; // Si apague el boton en Host sesion aca me aseguro que si vuelve a este panel, el boton funciona de vuelta
    }
    
    private void HostSession()
    {
        _hostButton.interactable = false; // Si vuelve al menu tengo que asegurarme que esto se vuelva true de vuelta
        
        _runnerHandler.CreateGame(_sessionNameField.text, "Game");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
}
