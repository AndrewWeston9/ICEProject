using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using System;

/// Represents a square block of the level, intended for
/// sharing over the network. 
/// Update: region blocks now contain voxel desriptions. 
/// The x and y axes will be assumed to be the horizontal
/// directions, and the z axis represents the vertical component.
/// The horizontal directions are determined dynamically to be
/// equal and set by the block size. The vertical dimension is
/// currently a constant value.
/// Blocks only stack horizontally, each RegionBlock represents the
/// full height of the level.
public class RegionBlock : MessageBase
{
    public const int MaxBlockHeight = 10;

    public int blockCoordX;
    public int blockCoordY;
    public int blockSize;
    public int [] blockStructure;
    
    /// A timestamp indicating the time at which this object was
    /// last written.
    public float timeLastChanged;

    /// Update the time stamp to current time.
    private void updateTimeStamp ()
    {
        timeLastChanged = Time.time;
    }
    
    /// Utility function - get the index of the block element
    /// at coordinates (x, y, z) in the RegionBlock (requires 
    /// valid value for blockSize).
    public int getIndex (int x, int y, int z)
    {
       // Debug.Log ("index for " + x + " " + y + " " + z);
        return (y * blockSize + x) * blockSize + z;
    }
    
    /// Return the coordinates of the block in the world.
    public Vector2 getPosition ()
    {
      return new Vector2 ((float) blockCoordX, (float) blockCoordY);
    }
    
    /// Given coordinates (x,y), return true if
    /// that position is in the RegionBlock. Assumes
    /// each element in the block is 1 unit large.
    public bool contains (float x, float y)
    {
      return ((x >= blockCoordX) && (x < blockCoordX + blockSize) &&
              (y >= blockCoordY) && (y < blockCoordY + blockSize));
    }
    
    /// Given coordinates (x,y) and a radius, return true if
    /// the disc (square) centered at those coordinates overlaps the region block. 
    /// Assumes each element in the block is 1 unit large.
    public bool contains (float x, float y, float radius)
    {
      return ((x + radius >= blockCoordX) && (x - radius < blockCoordX + blockSize) &&
              (y + radius >= blockCoordY) && (y - radius < blockCoordY + blockSize));
    }
    
    /// Default constructor.
    public RegionBlock ()
    {
        blockCoordX = 0;
        blockCoordY = 0;
        
        blockSize = 0;
        blockStructure = null;        
        updateTimeStamp ();
    }
    
    /// Initialize a region block from the texture source, taking a blocksize square
    /// from position (x, y)
    public RegionBlock (Texture2D levelPattern, int x, int y, int mblockSize)
    {
        blockCoordX = x;
        blockCoordY = y;
        
        blockSize = mblockSize;
        blockStructure = new int [blockSize * blockSize * MaxBlockHeight];
        for (int i = 0; i < blockSize; i += 1)
        {
            for (int j = 0; j < blockSize; j += 1)
            {
                int xc = blockCoordX + i;
                int yc = blockCoordY + j;
                xc = Math.Max (0, Math.Min (levelPattern.width, xc));
                yc = Math.Max (0, Math.Min (levelPattern.height, yc));
                int zc = (int) (levelPattern.GetPixel (xc, yc).r * (float) MaxBlockHeight);
//                 Debug.Log ("Height: " + levelPattern.GetPixel (xc, yc).r + " " + zc);

                for (int k = 0; k < MaxBlockHeight; k += 1)
                {
                    if (k < zc)
                    {
                        blockStructure[getIndex (i, j, k)] = 1;
                    }
                    else
                    {
                        blockStructure[getIndex (i, j, k)] = 0;
                    }
                }
            }
        }
        updateTimeStamp ();
    }
    
    /// Produce a simplified mesh representation. Scales the region block to fit completely into
    /// a unit square.
    /// Minor update to voxel structure, but deprecated.
    public void convertToMesh (Mesh mesh)
    {
        mesh.Clear ();
        
        int xdim = blockSize;
        int ydim = blockSize;
        int zdim = MaxBlockHeight;
        
        Vector3 [] vertices = new Vector3[(xdim + 1) * (ydim + 1) * (zdim + 1)];
        Vector3 [] normals = new Vector3[(xdim + 1) * (ydim + 1) * (zdim + 1)];
        Vector2 [] uv = new Vector2[(xdim + 1) * (ydim + 1) * (zdim + 1)];
        
        for (int i = 0; i <= xdim; i += 1)
        {
            for (int j = 0; j <= ydim; j += 1)
            {
                for (int k = 0; k <= zdim; k += 1)
                {
                    vertices[(j * (ydim + 1) + i) * (xdim + 1) + k] = new Vector3 ((float) i / xdim, (float) j / ydim, (float) k / zdim);
                    normals[(j * (ydim + 1) + i) * (xdim + 1) + k] = -Vector3.forward;
                    uv[(j * (ydim + 1) + i) * (xdim + 1) + k] = new Vector2((float) i / xdim, (float) j / ydim);
                }
            }
        }
        
        mesh.vertices = vertices; 
        mesh.normals = normals;
        mesh.uv = uv;
        
        // Count the number of elements that actually need to be 
        // created as triangles.
        int blockSurfaces = 0;
        for (int i = 0; i < xdim; i += 1)
        {
            for (int j = 0; j < ydim; j += 1)
            {
                for (int k = 0; k < zdim; k += 1)
                {
                    if (blockStructure[getIndex (i, j, k)] == 1)
                    {
                        blockSurfaces++;
                    }
                }
            }
        }
        
        int [] tri = new int[blockSurfaces * 6];
        int tricount = 0;
        for (int i = 0; i < xdim; i += 1)
        {
            for (int j = 0; j < ydim; j += 1)
            {
                for (int k = 0; k < zdim; k += 1)
                {
                    if (blockStructure[getIndex (i, j, k)] == 1)
                    {
                        //  Lower left triangle.
                        tri[tricount++] = ((j + 0) * (ydim + 1) + (i + 0)) * (xdim + 1) + (k + 0);
                        tri[tricount++] = ((j + 1) * (ydim + 1) + (i + 0)) * (xdim + 1) + (k + 0);
                        tri[tricount++] = ((j + 1) * (ydim + 1) + (i + 1)) * (xdim + 1) + (k + 0);
                        
                        //  Upper right triangle.   
                        tri[tricount++] = ((j + 0) * (ydim + 1) + (i + 0)) * (xdim + 1) + (k + 0);
                        tri[tricount++] = ((j + 1) * (ydim + 1) + (i + 1)) * (xdim + 1) + (k + 0);
                        tri[tricount++] = ((j + 0) * (ydim + 1) + (i + 1)) * (xdim + 1) + (k + 0);
                    }
                }
            }
        }
        mesh.triangles = tri;
    }
    
    /// Modify the level by making a block appear at (x,y). These
    /// coordinates are relative to this RegionBlock.
    public void setBlock (int x, int y, int z)
    {
        if ((x >= 0) && (x < blockSize) && 
            (y >= 0) && (y < blockSize) &&
            (z >= 0) && (z < MaxBlockHeight))
        {
            blockStructure[getIndex (x, y, z)] = 1;
//              Debug.Log ("Setting " + x + " " + y + " " + z);
            updateTimeStamp ();
        }
    }
    
    /// Get the value for the block at the given position.
    /// Returns 0 for values out of range.
    public int getBlock (int x, int y, int z)
    {
        if ((x >= 0) && (x < blockSize) && 
            (y >= 0) && (y < blockSize) &&
            (z >= 0) && (z < MaxBlockHeight))
        {
            return blockStructure[getIndex (x, y, z)];
        }
        return 0;
    }
    
    /// Modify the scene to draw a brick at the given coordinates. Really nothing to do
    /// with a region block and could be transferred to an appropriate view class.
    /// Coordinates given are the coordinates relative to the parent transformation for this region. 
    /// Currently the y axis is up, x and z are the two horizontal directions.
    public void placeSingleBlock (GameObject block, Vector3 position, Transform parentObjectTransform)
    {
      Vector3 blockpos = new Vector3 (position.x, position.z, position.y);
      GameObject thisBrick = UnityEngine.Object.Instantiate (block, blockpos, Quaternion.identity);
      thisBrick.transform.SetParent (parentObjectTransform, false);
    }
    
    /// Build up a local copy of the scene from information in the region block. Also needs to end
    /// up in a view class, using appropriate methods to query the RegionBlock information.
    
    /// Removes all current views of that block.
    public void placeBlocks (GameObject block, Transform parentObjectTransform)
    {
        for (int i = parentObjectTransform.childCount - 1; i >= 0; i--)
        {
            Transform child = parentObjectTransform.GetChild (i);
            UnityEngine.Object.Destroy(child.gameObject);
        }
        
        for (int i = 0; i < blockSize; i += 1)
        {
            for (int j = 0; j < blockSize; j += 1)
            {
                //                   Debug.Log ("Block at " + i + " , " + j + " ==" + blockStructure[getIndex (i, j)] + " - " + timeLastChanged);
                for (int k = 0; k < MaxBlockHeight; k += 1)
                {
                    if (blockStructure[getIndex (i, j, k)] == 1)
                    {
                        Vector3 pos = new Vector3 (i, j, k);
                        placeSingleBlock (block, pos, parentObjectTransform);
                        //                   Debug.Log ("Block at " + i + " , " + j);
                    }
                }
            }
        }
    }
}

/// A class representing the entire level. This will only ever be instantiated
/// on the server. It is designed to be easily be decomposed into the individual
/// RegionBlocks which can be shared with the client, and this subset of RegionBlocks
/// maintained on the client as a cached version of a subset of the level.
public class LevelStructure
{
    /// The blocks representing the level.
    protected RegionBlock [,] levelStructures;
    
    /// The objects used on the server to visualize the level.
    protected GameObject [,]  levelViewObjects;

    /// The number of bricks in each dimension of the region blocks used.
    protected int blockSize;
    
    /// Construct a level using a texture as the source of data.
    public LevelStructure (Texture2D levelPattern, int mblockSize)
    {
        blockSize = mblockSize;
        
        int blocksx = (int) ((float) (levelPattern.width + blockSize - 1) / blockSize);
        int blocksy = (int) ((float) (levelPattern.height + blockSize - 1) / blockSize);
        
        levelStructures = new RegionBlock [blocksx, blocksy];
        
        levelViewObjects = new GameObject [blocksx, blocksy];
        
        for (int i = 0; i < blocksx; i += 1)
        {
            for (int j = 0; j < blocksy; j += 1)
            {
                levelStructures[i, j] = new RegionBlock (levelPattern, i * blockSize, j * blockSize, blockSize);
                levelViewObjects[i, j] = null;
            }
        }
    }
    
    /// Find the region block corresponding to the provided coordinates.
    /// Assumes each Region Block is blockSize in size.
    public RegionBlock getRegion (float x, float y)
    {
        int ix = (int) (x / blockSize);
        int iy = (int) (y / blockSize);
        
        if ((ix >= 0) && (ix < levelStructures.GetLength (0)) &&
            (iy >= 0) && (iy < levelStructures.GetLength (1)))
        {
            return levelStructures[ix, iy];
        }
        else
        {
            return null;
        }
    }

    /// Update the geometry representation of the level. Used on the server
    /// for visualization.
    public void refreshMesh ()
    {
        for (int i = 0; i < levelStructures.GetLength (0); i += 1)
        {
            for (int j = 0; j < levelStructures.GetLength (1); j += 1)
            {
                RegionBlock rb = getRegion (i * blockSize, j * blockSize);
                
                if (rb != null)
                {
                    GameObject t = levelViewObjects[i, j];
                    MeshFilter tmf = t.GetComponent <MeshFilter>();
                    rb.convertToMesh (tmf.mesh);
                }
            }
        }
    }
    
    /// Create an instance of the geometry representation of the level, which can
    /// then be updated using refreshMesh.
    public void convertToMesh (GameObject regionBlank, Transform transform)
    {
        for (int i = 0; i < levelStructures.GetLength (0); i += 1)
        {
            for (int j = 0; j < levelStructures.GetLength (1); j += 1)
            {
                RegionBlock rb = getRegion (i * blockSize, j * blockSize);
                
                if (rb != null)
                {
                    Vector3 pos = new Vector3 (i * 1.0f, j * 1.0f, 0.0f);
                    GameObject t = (GameObject) UnityEngine.Object.Instantiate (regionBlank, pos, Quaternion.identity);
                    t.transform.SetParent (transform, false);
                    
                    MeshFilter tmf = t.GetComponent <MeshFilter>();
                    rb.convertToMesh (tmf.mesh);
                    
                    levelViewObjects[i, j] = t;
                }
            }
        }
    }
    
    /// Place a block at the given coordinates, relative to the entire level itself.
    public void setBlock (float x, float y, float z)
    {
        int rx = ((int) x) % blockSize;
        int ry = ((int) y) % blockSize;
        int rz = ((int) z);
        
        RegionBlock rb = getRegion (x, y);
        
        if (rb != null)
        {
            rb.setBlock (rx, ry, rz);
        }
        
        refreshMesh ();
    }
}

/// Messages used for client/server synchronization to maintain the level
/// coordination.
public class LevelMsgType {
    /// Notify of a player position update, allowing the details of the
    /// regions being supplied to the client to be modified.
    public const short LevelRequest = MsgType.Highest + 1;
    
    /// Send a region block update to the client.
    public const short LevelResponse = MsgType.Highest + 2;        
    
    /// Player indicates some action resulting in a modification to the
    /// master copy of the level.
    public const short LevelUpdate = MsgType.Highest + 3;
};

/// For each client, keep track of which regions are within a zone of interest.
/// If any of these regions change, then send an update to that client.
public class AccessedRegion
{
    public RegionBlock region;

    /// A timestamp representing the time at which the last update
    /// was sent to the client. 
    public float timeLastUpdate;
    
    /// Used to ensure each region is checked.
    public bool tagged;
}

/// An element of a list of clients, keeping track of where that player is
/// and which regions that player is interested in. Regions of interest are 
/// actually square, despite being described by a radius parameter.
public class ClientDetails
{
    /// Last updated position of the player.
    public Vector3 position;
    
    /// Game object providing the server view proxy 
    /// representation on the server dashboard.
    public GameObject representation;
    
    /// The distance around the player's position that
    /// the client wants to receive updates for.
    public float radius;
    
    /// List of region blocks that this client is receiving updates for.
    public List<AccessedRegion> activeRegions;

    /// The position of the player during which the player's 
    /// list of current regions was last updated. Don't update
    /// this list until the player has moved a signifcant distance.
    public Vector3 lastUpdatePosition;
    
    /// The threshold distance required for updating list of regions.
    public const float updateDistance = 2.0f;
    
    /// Constructor for a new player entry.
    public ClientDetails ()
    {
        activeRegions = new List<AccessedRegion> ();
        
        // far far away - ensure update as soon as player starts.
        lastUpdatePosition = new Vector3 (1e32f, 1e32f, 1e32f);
    }
    
    /// Return the record for the given regionBlock, null
    /// if it doesn't exist.
    public AccessedRegion findRegion (RegionBlock rb)
    {
        foreach (AccessedRegion ar in activeRegions)
        {
            if (ar.region == rb)
            {
                return ar;
            }
        }
        return null;
    }
}

/// State management for the world as represented on the server.
public class WorldManager : NetworkBehaviour {
    
    /// A texture representing the current level layout.
    public Texture2D mapPattern;
    
    /// The game object that will be instantiated on each client
    /// to manage the client side representation of the portion of
    /// level managed by the client.
    public GameObject localLevelPrefab;
    
    /// The game object used to show each of the regions to provide
    /// a server dashboard view of the entire game level.
    public GameObject regionBlank;
    
    /// The game object containing the representation of each of the
    /// players that will be overlaid on the server dashboard view.
    public GameObject playerProxy;
    
    /// The data structure representing the complete world state, which is
    /// synchronized with the LocalWorld copies on each client.
    private LevelStructure levelStructure;
    
    /// The details of each client, indexed by the connection identifier.
    private Dictionary<int, ClientDetails> playerMonitoring;
    
    /// The number of blocks (blocksize ^ 2) in each of the region chunks
    /// representing the smallest chunk of a level which is shared.
    private int blockSize;
    
    public const float minLevelHeight = 2.0f;
    
    // Use this for initialization
    void Start () {
        blockSize = 10;
      
        playerMonitoring = new Dictionary<int, ClientDetails> ();
        
        levelStructure = new LevelStructure (mapPattern, blockSize);
        
        levelStructure.convertToMesh (regionBlank, transform);        
    }
    
    /// When the server starts:
    ///   - Register a handler for any messages that might originate from a client.
    ///   - Create a server dashboard object to visualize the level.
    public override void OnStartServer ()
    {
        NetworkServer.RegisterHandler (LevelMsgType.LevelRequest, ClientCommandHandler);
        NetworkServer.RegisterHandler (LevelMsgType.LevelUpdate, ClientCommandHandler);
        
        Debug.Log ("Spawn local on each client");
        
        Vector3 spawnPosition = new Vector3 (10, 0, 0);
        Quaternion spawnRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        
        var localLevel = (GameObject) Instantiate (localLevelPrefab, spawnPosition, spawnRotation);
        NetworkServer.Spawn(localLevel);
        Debug.Log ("Spawn local with id " + localLevel.GetComponent<NetworkIdentity>().netId);
    }
    
    /// Retrieve the record corresponding to the given player's connection.
    /// Creates a new blank record if none exists yet.
    ClientDetails getPlayerDetails (int connectionId)
    {
        ClientDetails cd;
        if (!playerMonitoring.ContainsKey (connectionId))
        {
            cd = new ClientDetails ();

            Vector3 spawnPosition = new Vector3 (0, 0, 0);
            Quaternion spawnRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            cd.representation = (GameObject) Instantiate (playerProxy, spawnPosition, spawnRotation);
            cd.representation.transform.parent = transform;
            
            playerMonitoring.Add (connectionId, cd);
        }
        else
        {
            cd = playerMonitoring[connectionId];
        }
        return cd;
    }
    
    /// Visually track player on level dashboard.
    void updatePlayerProxy (int connId)
    {
        ClientDetails thiscd = getPlayerDetails (connId);
        thiscd.representation.transform.localPosition = new Vector3 (thiscd.position.x / blockSize, thiscd.position.z / blockSize, 0.0f);
    }
    
    /// Send an update to the player identified by the given connection ID. Work out
    /// which regions have changed, and communicate those details.
    void sendPlayerUpdate (int connectionId, Vector3 position, float visibleRadius)
    {
        ClientDetails cd = getPlayerDetails (connectionId);
        cd.position = position;
        cd.radius = visibleRadius;
        
        // Check that the list of accessed regions is still up to date.
        if (Vector3.Distance (cd.position, cd.lastUpdatePosition) > ClientDetails.updateDistance)
        {
            // Clear tags.
            foreach (AccessedRegion ar in cd.activeRegions)
            {
                ar.tagged = false;
            }

            // Make sure all regions in the radius are tracked and tagged.
            int blockr = (int) (cd.radius / blockSize);
            for (int offx = -blockr; offx <= blockr; offx ++)
            {
                for (int offy = -blockr; offy <= blockr; offy ++)
                {
                    RegionBlock rb = levelStructure.getRegion (cd.position.x + offx * blockSize, cd.position.z + offy * blockSize);
                    if (rb != null)
                    {
//                         Debug.Log ("Updating player at rb " + rb + " xx " + rb.blockCoordX + " - " + rb.blockCoordY);
                        
                        AccessedRegion ar = cd.findRegion (rb);
                        if (ar == null)
                        {
                            ar = new AccessedRegion ();
                            ar.region = rb;
                            ar.timeLastUpdate = 0; // force update.
                            cd.activeRegions.Add (ar);
                        }
                        ar.tagged = true;
                    }
                }
            }
            
            // Remove any regions not tagged (no longer in radius)
            for (int i = cd.activeRegions.Count - 1; i >= 0; i--)
            {
                if (!cd.activeRegions[i].tagged)
                {
                    cd.activeRegions.RemoveAt (i);
                }
            }
            
            // Mark the position of last region list update.
            cd.lastUpdatePosition = cd.position;
        }

//          Debug.Log ("Updating player at " + cd.activeRegions + " --- " + cd.activeRegions.Count);
        
        // Send updates for those regions on the list who have changed since the last update timestamp.
        foreach (AccessedRegion ar in cd.activeRegions)
        {
            if (ar.timeLastUpdate < ar.region.timeLastChanged)
            {
//                Debug.Log ("Updating player at " + ar.timeLastUpdate + " - " + ar.region.timeLastChanged);            
                NetworkServer.SendToClient (connectionId, LevelMsgType.LevelResponse, ar.region); 
                // update timestamp.
                ar.timeLastUpdate = Time.time;
            }
        }
    }
    
    /// Handle incoming commands from the client. 
    void ClientCommandHandler (NetworkMessage netMsg)
    {
        switch (netMsg.msgType)
        {
            case LevelMsgType.LevelRequest:
                /// Player has notified of current position. Check to see if regions of interest
                /// need to be updated.
            {
                LevelSyncMessage m = netMsg.ReadMessage<LevelSyncMessage>();
//                 Debug.Log ("Got message: " + netMsg.conn.connectionId + " : ");                
                
                sendPlayerUpdate (netMsg.conn.connectionId, m.playerPosition, m.visibleRadius);
                
                updatePlayerProxy (netMsg.conn.connectionId);
            }
            break;
            
            case LevelMsgType.LevelUpdate:
                /// Player has changed the level in a way that affects other players.
            {
                Debug.Log ("Changed level");
                BlockAddMessage m = netMsg.ReadMessage<BlockAddMessage>();
                levelStructure.setBlock (m.px + 0.5f, m.pz + 0.5f, m.height);
            }
            break;
            
            default:
            {
                Debug.Log ("Unexpected message type");
                
            }
            break;
        }
    }
    
    // Update is called once per frame
    void Update () {
        
    }
}
