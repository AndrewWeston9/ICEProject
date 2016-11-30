// Not currently used.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BlobNetworkManager : NetworkManager {
    
    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect (conn);
        
        Debug.Log ("Client connected");
    }
}
