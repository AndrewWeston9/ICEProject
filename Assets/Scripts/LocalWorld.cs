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

/// A local level block represents a component of the environment
/// in which the player exists. The complete environment will consist
/// of several of these blocks, and any other environmentally global
/// components.
/// This class allow interrogation of the local world structure, as 
/// recently updated from the master copy on the server. It also matches
/// this to GameObjects in the scene holding geometry and other attributes.
class LocalLevelBlock
{
    public RegionBlock region;
    public GameObject  gobject;
}

public class LocalWorld : NetworkBehaviour {

    /// The game object that will be the base object for each
    /// local level block. Presumably an empty.
    public GameObject localLevelElement;
    
    /// The game object used to represent a brick.
    public GameObject localBrick;
    
    private List<LocalLevelBlock> levelStructure; 
    
    private bool foundPlayer;
    
    // Use this for initialization
    void Start () {
        levelStructure = new List<LocalLevelBlock> ();
        
        foundPlayer = false;
        
        Debug.Log ("Local world level instance started");
        
    }
    
    /// Identify the level block corresponding to a particular position.
    /// May return null if no such block is cached locally.
    LocalLevelBlock findLevelBlock (Vector3 position)
    {
        foreach (LocalLevelBlock i in levelStructure)
        {
            if (i.region.contains (position.x, position.z))
            {
                return i;
            }
        }
        return null;
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
        switch (netMsg.msgType)
        {
            case LevelMsgType.LevelResponse:
            {
                
                RegionBlock rb = netMsg.ReadMessage<RegionBlock>();
                Debug.Log ("Server Got message: " + rb.blockSize);
                
                MeshFilter mf = GetComponent <MeshFilter>();
                rb.convertToMesh (mf.mesh);        
                
                Vector2 rbpos = rb.getPosition ();
                Vector3 llbpos = new Vector3 (rbpos.x, 0.0f, rbpos.y);
                LocalLevelBlock llb = findLevelBlock (llbpos);
                if (llb == null)
                {
                    Debug.Log ("New local block " + llbpos);
                    llb = new LocalLevelBlock ();
                    llb.region = rb;
                    llb.gobject = Object.Instantiate (localLevelElement, llbpos, Quaternion.identity);
                    levelStructure.Add (llb);
                }
                
                // llb should now be valid.
                llb.region.placeBlocks (localBrick, llb.gobject.transform);
            }
            break;
            default:
            {
                Debug.Log ("Unexpected message type in LocalWorld");                
            }
            break;
        }
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
