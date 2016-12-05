using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// A message sent by client to server advising of player position.
class LevelSyncMessage : MessageBase
{
    /// Position in space of the player.
    public Vector3 playerPosition;
    
    /// The distance that the player should be able to see. The server
    /// will register the player's interest in neighbouring region blocks
    /// as defined by this.
    public float   visibleRadius;
}

/// The message sent when the player is updating the level. 
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
public class LocalLevelBlock
{
    public RegionBlock region;
    public GameObject  gobject;
}

/// A client equivalent of the Level Structure and World Managers. Cached
/// copies of the regionblocks that the client can use for navigating about
/// thw world.
public class LocalWorld : NetworkBehaviour {

    /// The game object that will be the base object for each
    /// local level block. Presumably an empty.
    public GameObject localLevelElement;
    
    /// The game object used to represent a brick.
    public GameObject localBrick;
    
    /// Define the distance about the player that we
    /// are interested in seeing things.
    public float viewRadius;
    
    /// Local level cache.
    private List<LocalLevelBlock> levelStructure; 
    
    /// The class will attempt to register with the player object. Once
    /// this has been achieved then this value will be set to true.
    private bool foundPlayer;
    
    // Use this for initialization
    void Start () {
        levelStructure = new List<LocalLevelBlock> ();
        
        foundPlayer = false;
        
        Debug.Log ("Local world level instance started");
        
    }
    
    /// Identify the level block corresponding to a particular position.
    /// May return null if no such block is cached locally.
    public LocalLevelBlock findLevelBlock (Vector3 position)
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
    
    /// Return the value of the cell at the given position. Returns
    /// false if no cell exists within the local cache.
    public bool findBlock (Vector3 position, out int value)
    {
        LocalLevelBlock llb = findLevelBlock (position);
        if (llb != null)
        {
          int rx = (int) (position.x - llb.region.blockCoordX);
          int ry = (int) (position.z - llb.region.blockCoordY);
        
          value = llb.region.getBlock (rx, ry);
          return true; 
        }
        value = 0;
        return false;
    }
    
    /// Create a level block corresponding to the given region block and
    /// add this to the cache of blocks. Assumes that such a block does
    /// not already exist, or a duplicate will be created.
    public LocalLevelBlock addLevelBlock (RegionBlock rb, Vector3 position)
    {
//         Debug.Log ("New local block " + position);
        LocalLevelBlock llb = new LocalLevelBlock ();
        llb.region = rb;
        llb.gobject = UnityEngine.Object.Instantiate (localLevelElement, position, Quaternion.identity);
        levelStructure.Add (llb);
        return llb;
    }                    
    
    /// Remove any cached region blocks that are outside the visible region.
    private void flushRegions ()
    {
        NetworkManager nm = NetworkManager.singleton;
        if (nm.client != null)
        {
            PlayerController player = ClientScene.localPlayers[0];
            
            Vector3 playerPosition = player.gameObject.transform.position;
            
            for (int i = levelStructure.Count - 1; i >= 0; i--)
            {
                if (!levelStructure[i].region.contains (playerPosition.x, playerPosition.z, viewRadius))
                {
                    UnityEngine.Object.Destroy (levelStructure[i].gobject);
                    levelStructure.RemoveAt (i);
                }
            }
        }
        
    }
    
    /// This object is deployed on the clients. When it starts:
    ///   - Register a network message handler for any level updates.
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
        
        // Send player position to the server.
        NetworkManager nm = NetworkManager.singleton;
        if ((nm.client != null) && (foundPlayer))
        {
            if (ClientScene.localPlayers.Count > 0)
            {
                PlayerController player = ClientScene.localPlayers[0];
                
                LevelSyncMessage m = new LevelSyncMessage ();
                m.playerPosition = player.gameObject.transform.position;
                m.visibleRadius = viewRadius;
                
                NetworkManager.singleton.client.Send (LevelMsgType.LevelRequest, m);
            }
        }
    }
    
    /// Handle incoming updates from the server.
    void ServerCommandHandler (NetworkMessage netMsg)
    {
        switch (netMsg.msgType)
        {
            case LevelMsgType.LevelResponse:
                // Received an updated region block from the server. Update
                // the cache, and ensure that the local visual representation
                // is consistent.
            {
                RegionBlock rb = netMsg.ReadMessage<RegionBlock>();
//                 Debug.Log ("Server Got message: " + rb.blockSize);
                
                MeshFilter mf = GetComponent <MeshFilter>();
                rb.convertToMesh (mf.mesh);        
                
                Vector2 rbpos = rb.getPosition ();
                Vector3 llbpos = new Vector3 (rbpos.x, 0.0f, rbpos.y);
                LocalLevelBlock llb = findLevelBlock (llbpos);
                if (llb == null)
                {
                    llb = addLevelBlock (rb, llbpos);
                    
                    // llb should now be valid.
                    llb.region.placeBlocks (localBrick, llb.gobject.transform);
                }
                else
                {
                    // if version is newer than the one we already have, then update it.
                    if (rb.timeLastChanged > llb.region.timeLastChanged)
                    {
                        llb.region = rb;
//                         Debug.Log ("Got update ..................................>");
                        llb.region.placeBlocks (localBrick, llb.gobject.transform);
                    }
                }
                
                flushRegions ();
            }
            break;
            
            default:
            {
                Debug.Log ("Unexpected message type in LocalWorld");                
            }
            break;
        }
    }
    
    // Add a new block at the given position. The intent is to allow players to immediately
    // reflect actions in the local game, which may eventually be replaced when the update
    // is returned from the server.
    public void placeBlock (float x, float z)
    {
        Vector3 llbpos = new Vector3 (x, 0.0f, z);
        LocalLevelBlock llb = findLevelBlock (llbpos);
        if (llb != null)
        {
            Vector3 regionpos = new Vector3 (x - llb.region.blockCoordX, z - llb.region.blockCoordY);
            // llb should now be valid.
            llb.region.placeSingleBlock (localBrick, regionpos, llb.gobject.transform);
        }
        else
            // else no region - potential problem.
        {
            Debug.Log ("No level block at " + llbpos);
        }
        
        
        BlockAddMessage m = new BlockAddMessage ();
        m.px = x;
        m.pz = z;
        NetworkManager.singleton.client.Send (LevelMsgType.LevelUpdate, m);        
    }
    
    /// Check the neighbouring blocks to see if there is an attachment candidate available.
    /// If so, return the height of that neighbour. 
    private bool validNearestBlockCandidate (int x, int z, out int y)
    {
        
        y = 0;
        int [] offx = { -1, -1 , -1, 0, 1, 1, 1, 0 };
        int [] offz = {  1, 0 , -1, -1, -1, 0, 1, 1 };
        for (int i = 0; i < 8; i++)
        {
          int value;
          bool hasValue = findBlock (new Vector3 (x + offx[i], 0.0f, z + offz[i]), out value);
          if (hasValue && (value == 1))
          {
              y = (int) WorldManager.minLevelHeight;
              return true;
          }
        }
        return false;
    }
    
    /// Find the open position next to a block nearest the given position. 
    public bool findNearestBlock (Vector3 position, out Vector3 availablePosition)
    {
        int r = 0;
        // search in increasing radius about the player.
        while (r < viewRadius)
        {
            // scan in a ring of the given radius r.
            for (int offset = 0; offset <= r; offset++)
            {
                int cx;
                int cz;
                int cy;
                
                // search coordinates for a square ring.
                int [] offxfixed    = { -1, -1, +1, +1,  0,  0,  0,  0  };
                int [] offxvariable = {  0,  0,  0,  0,  -1, +1, -1, +1  };
                int [] offzfixed    = {  0,  0,  0,  0,  -1, -1, +1, +1 };
                int [] offzvariable = { -1, +1, -1, +1,   0,  0,  0,  0 };
                for (int i = 0; i < 8; i++)
                {
                    cx = offxfixed[i] * r + offxvariable[i] * offset + (int) (position.x + 0.5f);
                    cz = offzfixed[i] * r + offzvariable[i] * offset + (int) (position.z + 0.5f);
                    
                    /// Check if the current block is an appropriate candidate. We assume this is empty
                    /// since it would have been previously checked as being a potential neighbour 
                    /// candidate. So just check to see if this has a valid neighbour, and get the height 
                    /// of the neighbouring block if it exists.
                    if (validNearestBlockCandidate (cx, cz, out cy))
                    {
                        availablePosition = new Vector3 (cx, cy, cz);
                        return true;
                    }
                }
            }
            r++;
        }
        // No block found.
        availablePosition = new Vector3 (0.0f, 0.0f, 0.0f);
        return false;
    }
}
