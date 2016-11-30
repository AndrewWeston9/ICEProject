using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

class LevelSyncMessage : MessageBase
{
    public string message;
    
    public Vector3 playerPosition;
}

class BlockAddMessage : MessageBase
{
    public float px;
    public float pz;
}

public class LocalWorld : NetworkBehaviour {
    
    public Texture2D mapPattern;
    
    private bool foundPlayer;
    
    // Use this for initialization
    void Start () {
        foundPlayer = false;
        
        Debug.Log ("Local world level instance started");
        
    }
    
    public override void OnStartClient()
    {
        Debug.Log ("On Client start " + NetworkClient.allClients);
        
        NetworkClient.allClients[0].RegisterHandler (LevelMsgType.LevelResponse, ServerCommandHandler);
    }
    
    // Update is called once per frame
    void Update () {
        // Check if the player has been created, and associate with that player if so.
        if (!foundPlayer)
        {
            if (ClientScene.localPlayers.Count > 0)
            {
                PlayerMove player = ClientScene.localPlayers[0].gameObject.GetComponent <PlayerMove>();
                player.setLocalWorld (this);
                foundPlayer = true;
            }
        }
        
        //         Debug.Log ("Update");
        NetworkManager nm = NetworkManager.singleton;
        if (ClientScene.localPlayers.Count > 0)
        {
            PlayerController player = ClientScene.localPlayers[0];
            
            if (nm.client != null)
            {
                LevelSyncMessage m = new LevelSyncMessage ();
                m.message = "Hello World!";
                m.playerPosition = player.gameObject.transform.position;
                
                NetworkManager.singleton.client.Send (LevelMsgType.LevelRequest, m);
            }
        }
    }
    
    void ServerCommandHandler (NetworkMessage netMsg)
    {
        RegionBlock rb = netMsg.ReadMessage<RegionBlock>();
        Debug.Log ("Server Got message: " + rb.blockSize);
        
        MeshFilter mf = GetComponent <MeshFilter>();
        rb.convertToMesh (mf.mesh);        
    }
    
    // Add a new block at the given position.
    public void placeBlock (float x, float z)
    {
        BlockAddMessage m = new BlockAddMessage ();
        m.px = x;
        m.pz = z;
        NetworkManager.singleton.client.Send (LevelMsgType.LevelUpdate, m);        
    }
}
