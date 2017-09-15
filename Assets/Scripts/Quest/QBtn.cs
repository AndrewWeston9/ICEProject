using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QBtn : MonoBehaviour {

    public int questID;
    public Text questTitle;

	/*private GameObject acceptButton;
	private GameObject declineButton;
	private GameObject completeButton;*/

	/*private QBtn acceptScript;
	private QBtn declineScript;
	private QBtn completeScript;


	void Start()
	{
		acceptButton = GameObject.Find ("QuestCanvas").transform.Find ("QuestPanel").transform.Find ("Description").transform.Find ("GameObject").transform.Find ("Accept").gameObject;
		acceptScript = acceptButton.GetComponent<QBtn> ();

		declineButton = GameObject.Find ("QuestCanvas").transform.Find ("QuestPanel").transform.Find ("Description").transform.Find ("GameObject").transform.Find ("Decline").gameObject;
		declineScript = declineButton.GetComponent<QBtn> ();

		completeButton = GameObject.Find ("QuestCanvas").transform.Find ("QuestPanel").transform.Find ("Description").transform.Find ("GameObject").transform.Find ("Complete").gameObject;
		completeScript = completeButton.GetComponent<QBtn> ();


		acceptButton.SetActive (false);
		declineButton.SetActive (false);
		completeButton.SetActive (false);


	}*/




	//Display quest info when player press the quest button in the panel
	public void DisplayInfos()
	{
		QuestUI.uiManager.displayQuestInfo (questID);

		//accept button
		if (QuestManager.qManager.requestAvailableQ(questID)) {
			QuestUI.uiManager.acceptBtn.SetActive (true);
			QuestUI.uiManager.acceptScript.questID = questID;

		} 
		else 
		{
			QuestUI.uiManager.acceptBtn.SetActive (false);
		}

		//decline button
		if (QuestManager.qManager.requestOngoingQ(questID)) {
			QuestUI.uiManager.giveupBtn.SetActive (true);
			QuestUI.uiManager.declineScript.questID = questID;
		} 
		else 
		{
			QuestUI.uiManager.giveupBtn.SetActive (false);
		}

		//complete button
		if (QuestManager.qManager.requestCompleteQ(questID)) {
			QuestUI.uiManager.completeBtn.SetActive (true);
			QuestUI.uiManager.completeScript.questID = questID;
		} 
		else 
		{
			QuestUI.uiManager.completeBtn.SetActive (false);
		}

	}



	public void acceptQuest()
	{
		QuestManager.qManager.AcceptQ (questID);
		QuestUI.uiManager.panelHide ();

		QuestObject[] currentQNPC = FindObjectsOfType (typeof(QuestObject)) as QuestObject[];

		foreach (QuestObject obj in currentQNPC) {
			obj.setQMarker ();
		}

	}
		
	public void declineQuest()
	{
		QuestManager.qManager.DeclineQ (questID);
		QuestUI.uiManager.panelHide ();

		QuestObject[] currentQNPC = FindObjectsOfType (typeof(QuestObject)) as QuestObject[];

		foreach (QuestObject obj in currentQNPC) {
			obj.setQMarker ();
		}

	}

	public void completeQuest()
	{
		QuestManager.qManager.CompleteQ (questID);
		QuestUI.uiManager.panelHide ();

		QuestObject[] currentQNPC = FindObjectsOfType (typeof(QuestObject)) as QuestObject[];

		foreach (QuestObject obj in currentQNPC) {
			obj.setQMarker ();
		}

	}

	public void closePanel()
	{
		QuestUI.uiManager.panelHide ();
		QuestUI.uiManager.acceptBtn.SetActive (false);
		QuestUI.uiManager.giveupBtn.SetActive (false);
		QuestUI.uiManager.completeBtn.SetActive (false);
	}













}
