﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using System;

/// Represents a square block of the level, intended for
/// sharing over the network.
public class RegionBlock : MessageBase
{
    public int blockCoordX;
    public int blockCoordY;
    public int blockSize;
    public int [] blockStructure;
    
    // Utility function - get the index of the block element
    // at coordinates (x, y) in the RegionBlock (requires 
    // valid value for blockSize).
    public int getIndex (int x, int y)
    {
        return y * blockSize + x;
    }
    
    public RegionBlock ()
    {
        blockCoordX = 0;
        blockCoordY = 0;
        
        blockSize = 0;
        blockStructure = null;        
    }
    
    // Initialize a region block from the texture source, taking a blocksize square
    // from position (x * blocksize,y * blocksize)
    public RegionBlock (Texture2D levelPattern, int x, int y, int mblockSize)
    {
        blockCoordX = x;
        blockCoordY = y;
        
        blockSize = mblockSize;
        blockStructure = new int [blockSize * blockSize];
        for (int i = 0; i < blockSize; i += 1)
        {
            for (int j = 0; j < blockSize; j += 1)
            {
                int xc = blockCoordX * blockSize + i;
                int yc = blockCoordY * blockSize + j;
                xc = Math.Max (0, Math.Min (levelPattern.width, xc));
                yc = Math.Max (0, Math.Min (levelPattern.height, yc));
                if (levelPattern.GetPixel (xc, yc).r != 0)
                {
                    blockStructure[getIndex (i, j)] = 1;
                }
                else
                {
                    blockStructure[getIndex (i, j)] = 0;
                }
            }
        }
    }
    
    
    public void convertToMesh (Mesh mesh)
    {
        mesh.Clear ();
        
        int xdim = blockSize;
        int ydim = blockSize;
        
        Vector3 [] vertices = new Vector3[(xdim + 1) * (ydim + 1)];
        Vector3 [] normals = new Vector3[(xdim + 1) * (ydim + 1)];
        Vector2 [] uv = new Vector2[(xdim + 1) * (ydim + 1)];
        
        for (int i = 0; i <= xdim; i += 1)
        {
            for (int j = 0; j <= ydim; j += 1)
            {
                vertices[j * (ydim + 1) + i] = new Vector3 ((float) i / xdim, (float) j / ydim, 0.0f);
                normals[j * (ydim + 1) + i] = -Vector3.forward;
                uv[j * (ydim + 1) + i] = new Vector2((float) i / xdim, (float) j / ydim);
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
                if (blockStructure[getIndex (i, j)] == 1)
                {
                    blockSurfaces++;
                }
            }
        }
        
        int [] tri = new int[blockSurfaces * 6];
        int tricount = 0;
        for (int i = 0; i < xdim; i += 1)
        {
            for (int j = 0; j < ydim; j += 1)
            {
                if (blockStructure[getIndex (i, j)] == 1)
                {
                    //  Lower left triangle.
                    tri[tricount++] = (j + 0) * (ydim + 1) + (i + 0);
                    tri[tricount++] = (j + 1) * (ydim + 1) + (i + 0);
                    tri[tricount++] = (j + 1) * (ydim + 1) + (i + 1);
                    
                    //  Upper right triangle.   
                    tri[tricount++] = (j + 0) * (ydim + 1) + (i + 0);
                    tri[tricount++] = (j + 1) * (ydim + 1) + (i + 1);
                    tri[tricount++] = (j + 0) * (ydim + 1) + (i + 1);
                }
            }
        }
        mesh.triangles = tri;
    }
    
    public void setBlock (int x, int y)
    {
        if ((x >= 0) && (x < blockSize) && 
            (y >= 0) && (y < blockSize))
        {
            blockStructure[getIndex (x, y)] = 1;
            Debug.Log ("Setting " + x + " " + y);
        }
    }
}

public class LevelStructure
{
    // The blocks representing the level.
    protected RegionBlock [,] levelStructures;
    
    // The objects used on the server to visualize the level.
    protected GameObject [,]  levelViewObjects;

    protected int blockSize;
    
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
                levelStructures[i, j] = new RegionBlock (levelPattern, i, j, blockSize);
                levelViewObjects[i, j] = null;
            }
        }
    }
    
    public RegionBlock getRegion (int x, int y)
    {
        if ((x >= 0) && (x < levelStructures.GetLength (0)) &&
            (y >= 0) && (y < levelStructures.GetLength (1)))
        {
            return levelStructures[x, y];
        }
        else
        {
            return null;
        }
    }

    // Update the geometry representation of the level. Used on the server
    // for visualization.
    public void refreshMesh ()
    {
        for (int i = 0; i < levelStructures.GetLength (0); i += 1)
        {
            for (int j = 0; j < levelStructures.GetLength (1); j += 1)
            {
                RegionBlock rb = getRegion (i, j);
                
                if (rb != null)
                {
                    GameObject t = levelViewObjects[i, j];
                    MeshFilter tmf = t.GetComponent <MeshFilter>();
                    rb.convertToMesh (tmf.mesh);
                }
            }
        }
    }
    
    public void convertToMesh (GameObject regionBlank, Transform transform)
    {
        for (int i = 0; i < levelStructures.GetLength (0); i += 1)
        {
            for (int j = 0; j < levelStructures.GetLength (1); j += 1)
            {
                RegionBlock rb = getRegion (i, j);
                
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
    
    public void setBlock (float x, float y)
    {
        int bx = (int) (x);
        int by = (int) (y);
        
        int rx = (int) ((x - (float) bx) * blockSize);
        int ry = (int) ((y - (float) by) * blockSize);
        
        RegionBlock rb = getRegion (bx, by);
        
        if (rb != null)
        {
            rb.setBlock (rx, ry);
        }
        
        refreshMesh ();
    }
}

public class LevelMsgType {
    public const short LevelRequest = MsgType.Highest + 1;
    public const short LevelResponse = MsgType.Highest + 2;        
    public const short LevelUpdate = MsgType.Highest + 3;
};

public class ClientDetails
{
    public Vector3 position;
    
    public GameObject representation;
}

public class WorldManager : NetworkBehaviour {
    
    public Texture2D mapPattern;
    
    public GameObject localLevelPrefab;
    
    public GameObject regionBlank;
    
    public GameObject playerProxy;
    
    private LevelStructure levelStructure;
    
    private Dictionary<int, ClientDetails> playerMonitoring;
    
    // Use this for initialization
    void Start () {
        playerMonitoring = new Dictionary<int, ClientDetails> ();
        
        levelStructure = new LevelStructure (mapPattern, 10);
        
        levelStructure.convertToMesh (regionBlank, transform);        
    }
    
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
    
    // Visually track player on level.
    void updatePlayer (int connId, Vector3 position)
    {
        if (!playerMonitoring.ContainsKey (connId))
        {
            ClientDetails cd = new ClientDetails ();
            Vector3 spawnPosition = new Vector3 (0, 0, 0);
            Quaternion spawnRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            cd.representation = (GameObject) Instantiate (playerProxy, spawnPosition, spawnRotation);
            cd.representation.transform.parent = transform;
            
            playerMonitoring.Add (connId, cd);
        }
        
        ClientDetails thiscd = playerMonitoring[connId];
        thiscd.position = position;
        thiscd.representation.transform.localPosition = new Vector3 (thiscd.position.x, thiscd.position.z, 0.0f);
    }
    
    /// Handle incoming commands from the client. 
    void ClientCommandHandler (NetworkMessage netMsg)
    {
        switch (netMsg.msgType)
        {
            case LevelMsgType.LevelRequest:
            {
                LevelSyncMessage m = netMsg.ReadMessage<LevelSyncMessage>();
//                 Debug.Log ("Got message: " + m.message + " : " + netMsg.conn.connectionId + " : " + m.playerPosition);
                
                updatePlayer (netMsg.conn.connectionId, m.playerPosition);
                
                MessageBase nm = levelStructure.getRegion ((int) m.playerPosition.x, (int) m.playerPosition.z);
                if (nm != null)
                {
                    NetworkServer.SendToClient (netMsg.conn.connectionId, LevelMsgType.LevelResponse, nm); 
                }
            }
            break;
            case LevelMsgType.LevelUpdate:
            {
                Debug.Log ("Changed level");
                BlockAddMessage m = netMsg.ReadMessage<BlockAddMessage>();
                levelStructure.setBlock (m.px, m.pz);
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