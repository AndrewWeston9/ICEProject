using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class PlayerState : NetworkBehaviour {
    
    private const float barSize = 0.2f;

//     [SyncVar(hook = "OnChangeResourceLevels")]
    public SyncListFloat resourceLevels = new SyncListFloat ();
    [SyncVar(hook = "OnChangeResources")]
    private bool resourceChanged;
    
    /// The prefab for components used to display resource levels.
    public GameObject resourceDisplayElement;
    
    /// The actual objects used as part of the bar displaying level of resources.
    private GameObject [] resourceDisplayObjects = null;
    
	bool inTrigger;

	string ResourceName;

	Vector3 ResourcePosition;

	GameObject playerObject;

    // Use this for initialization
    void Start () {
        resourceLevels.Add (0.34f);
        resourceLevels.Add (0.64f);
        resourceLevels.Add (0.07f);
        
        OnChangeResourceLevels (resourceLevels);

		inTrigger = false;
		ResourceName = "";
		ResourcePosition = new Vector3 ();
    }

	void Update()
	{
		if (inTrigger == true && Input.GetKeyDown (KeyCode.G))
		{
			takeResource ();
		}
	}

	// Update is called once per frame
	/*void Update () {
		if (inTrigger == true && Input.GetKeyDown (KeyCode.G))
		{
			//Debug.LogError ("In resource trigger range!!!!!!!!!!!!!!!!!!!!!!");
			if (ResourceName == "WoodResourceBrick(Clone)")
			{
				changeResource (0, 0.05f);
				ResourceTakeMessage m = new ResourceTakeMessage ();
				m.position = ResourcePosition;
				NetworkManager.singleton.client.Send (LevelMsgType.ResourceUpdate, m);  
			}
		}
	}*/
    
    public void changeResource (int resourceType, float deltaResource)
    {
        if (!isServer)
        {
            return;
        }
        
        resourceLevels[resourceType] += deltaResource;
        
        if (resourceLevels[resourceType] > 1.0f)
        {
            resourceLevels[resourceType] = 1.0f;
        }
        
        resourceChanged = !resourceChanged;
        Debug.Log ("Changed " + this);
    }

    void OnChangeResources (bool changed)
    {
                Debug.Log ("Refreshfix");
        OnChangeResourceLevels (resourceLevels);
    }

//       void OnGUI() {
//           Debug.Log ("Updating resource");
//           GUI.DrawTexture(new Rect(10, 10, 60, 60), aTexture, ScaleMode.ScaleToFit, true, 10.0F);
//       }
    void OnChangeResourceLevels (SyncListFloat resourceLevels )
    {
                Debug.Log ("Refresh");
        if (!isLocalPlayer)
        {
            return;
        }
                
        // Initialize objects for the resource bar.
        if (resourceDisplayObjects == null)
        {
            /// The object under which resource display elements are shown.
            GameObject resourceAreaDisplay = GameObject.Find("ResourceAreaDisplay");
            GameObject gameGlobals = GameObject.Find("GameGlobals");

            resourceDisplayObjects = new GameObject [GloopResources.NumberOfResources];
            for (int i = 0; i < GloopResources.NumberOfResources; i++)
            {
              GameObject go = UnityEngine.Object.Instantiate (resourceDisplayElement, new Vector3 (0, 0, 0), Quaternion.identity);
              resourceDisplayObjects[i] = go;
              resourceDisplayObjects[i].transform.SetParent (resourceAreaDisplay.transform, false);
              resourceDisplayObjects[i].GetComponent<MeshRenderer>().material = gameGlobals.GetComponent<GloopResources>().resourceMaterials[i];
            }
        }
        
        // Translate resource levels into size and position of the resource bar.
        float position = -0.75f;
        for (int i = 0; i < GloopResources.NumberOfResources; i++)
        {
            resourceDisplayObjects[i].transform.localPosition = new Vector3 (position, 0.75f, 0.0f);
            float amt = barSize * resourceLevels[i];
            resourceDisplayObjects[i].transform.localScale = new Vector3 (amt, barSize, barSize);
            
            position += barSize;
        }
    }

	public void takeResource()
	{
		if (ResourceName == "WoodResourceBrick(Clone)")
		{
			//Debug.LogError ("In resource trigger range!!!!!!!!!!!!!!!!!!!!!!");
			changeResource (0, 0.05f);
			inTrigger = false;
			ResourceTakeMessage m = new ResourceTakeMessage ();
			m.position = ResourcePosition;
			NetworkManager.singleton.client.Send (LevelMsgType.ResourceUpdate, m);  
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.tag == "Resource")
		{
			inTrigger = true;
			ResourceName = other.name;
			ResourcePosition = other.gameObject.transform.position;
			playerObject = other.transform.parent.gameObject;
		}
	}

	void OnTriggerStay(Collider other)
	{
		if(other.gameObject.tag == "Resource")
		{
			inTrigger = true;
			ResourceName = other.name;
			ResourcePosition = other.gameObject.transform.position;
			playerObject = other.transform.parent.gameObject;
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other.tag == "Resource")
		{
			inTrigger = false;
		}
	}
}
