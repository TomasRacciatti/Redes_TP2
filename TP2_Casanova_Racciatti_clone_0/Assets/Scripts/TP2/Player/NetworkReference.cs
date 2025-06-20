using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkReference : NetworkBehaviour
{
    public static NetworkReference Local { get; private set; }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
        }
    }
}
