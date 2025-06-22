using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class SetPlayerNickname : NetworkBehaviour
{
    public override void Spawned()
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
    }
}
