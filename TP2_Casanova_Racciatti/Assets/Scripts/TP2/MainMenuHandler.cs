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
    [SerializeField] private Button _quitButton;
    [SerializeField] private Button _goToHostButton;
    [SerializeField] private Button _hostButton;
    
    [Header("Texts")]
    [SerializeField] private TMP_Text _connectingText;
    
    [Header("InputFields")] 
    [SerializeField] private TMP_InputField _sessionNameField;

    private void Awake()
    {
        throw new NotImplementedException();
    }
    
}
