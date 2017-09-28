using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class StormGenerator : NetworkBehaviour
{

    public GameObject stormCloud;

    Vector3 CloudPosition = new Vector3(18, 5, 7);
    Quaternion CloudRotation= Quaternion.identity;
    void GenerateCloud(Vector3 Position, Vector3 Direction, int CloudLevel)
    {
        Instantiate(stormCloud);
        //StormCloud.AddComponent<StormCloud>();
    }
    public void GenerateCloud()
    {

        //Instantiate(StormCloud, CloudPosition, CloudRotation);
        SpawnObject();
    }

    public override void OnStartClient()
    {
        ClientScene.RegisterPrefab(stormCloud);
    }


    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update()
    {


        if (Input.GetButtonDown("Fire2"))
        {

            SpawnObject();

        }
    }

    [Server]
    public void SpawnObject()
    {
        GameObject obj = (GameObject)Instantiate(stormCloud, CloudPosition, CloudRotation);
        NetworkServer.Spawn(obj);
    }
}