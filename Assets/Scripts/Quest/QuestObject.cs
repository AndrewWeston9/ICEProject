using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestObject : MonoBehaviour {

    private bool inTrigger = false;

    public List<int> availableIDs = new List<int>();
    public List<int> receivableIDs = new List<int>();


    public GameObject qMarker;
    public Image qImage;

    public Sprite qAvailableSprite;
    public Sprite qReceivableSprite;

	// Use this for initialization
	void Start () {
        setQMarker();
	}
	
	//set marker on top of npc
	public void setQMarker()
    {
        if(QuestManager.qManager.CheckCompleteQ(this))
        {
            qMarker.SetActive(true);
            qImage.sprite = qReceivableSprite;
            qImage.color = Color.yellow;
        }
        else if(QuestManager.qManager.CheckAvailableQ(this))
        {
            qMarker.SetActive(true);
            qImage.sprite = qAvailableSprite;
            qImage.color = Color.yellow;
        }
        else if (QuestManager.qManager.CheckAcceptedQ(this))
        {
            qMarker.SetActive(true);
            qImage.sprite = qReceivableSprite;
            qImage.color = Color.gray;
        }
        else
        {
            qMarker.SetActive(false);
        }
    }


	// Update is called once per frame
	void Update () {
		if(inTrigger == true && Input.GetKeyDown(KeyCode.R))
        {
			if (!QuestUI.uiManager.activePanel) {
				
				//open UI
				QuestUI.uiManager.checkQuest (this);
			}
          

        }
	}

    //NPC have rigidbodave and 2 colliders. 1 collider(non trigger) and the other collider(trigger)
    //Player must have rigidbody, collider(non trigger) and a tag called "Player"
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            inTrigger = true;
            
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            inTrigger = false;
            
        }
    }

}
