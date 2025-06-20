using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class SessionItem : MonoBehaviour
{
    public event Action<SessionInfo> OnJoinSession;
    
    public void Initialize(SessionInfo sessionInfo)
    {
        
    }
}
