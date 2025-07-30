using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerData : MonoBehaviour
{
    public static LocalPlayerData Instance { get; private set; }

    public string Nickname { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetNickname(string nickname)
    {
        Nickname = nickname;
    }
}
