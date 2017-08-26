using UnityEngine;
using UnityEngine.Networking;

using System;

/// Player object management.
public class PlayerMove : NetworkBehaviour
{
	/// A link to the local level representation,which can be
	/// queried for any player control operations.
	protected LocalWorld localWorld;


	/// Manage when a block is attached to an edge of the world.
	protected bool attached;
	protected Vector3 attachPoint;

	bool pickup = false;

	// Current block type selected by the player to be placed.
	int currentBlockType = 1;

	/// Access method used when the local world registers itself
	/// with this player object.
	public void setLocalWorld (LocalWorld lw)
	{
		localWorld = lw;
		//       Debug.Log ("Add block " + localWorld); 
	}



	/// Actions on each update of the player:
	///   - handle key presses
	///   - check that the player stays in a valid region.
	void Update()
	{
		DontDestroyOnLoad (this.gameObject);
		;

		Ray ray = Camera.main.ScreenPointToRay(new Vector3((Screen.width/2),(Screen.height/2),0));
		RaycastHit hit;

		Debug.Log ("Player update - " + isLocalPlayer + " - " + localWorld);

		if (!isLocalPlayer)
		{
			return;

		}

		if (localWorld == null)
		{
			return;
		}

		/// Jump action.
		var y = 0.0f;
		if (Input.GetKeyDown(KeyCode.Space) && (isGrounded () || attached))
		{
			y = 1.0f;
			attached = false;
		}

		Vector3 playerpos = transform.position;

		//Vector3 forward = transform.forward;
		//Vector3 placement = transform.position + forward * 2;

		/// Place block action by right mouse click


		//if (Input.GetKeyDown(KeyCode.Mouse1) && (isGrounded () || attached))
		//{


			/*int px = (int) (hit.point.x);
			int py = Math.Max ((int) (playerpos.y), (int) WorldManager.minLevelHeight);
			int pz = (int) (playerpos.z);

			Debug.Log ("place at " + px + " " + pz + " " + py);
			localWorld.placeBlock (px, pz, py);

			//y = 0.2f;
			//transform.Translate(0, 1, 0);

			attached = false;


			int px = (int) (placement.x + 10.5);
			int py = Math.Max ((int) (placement.y), (int) WorldManager.minLevelHeight);
			int pz = (int) (placement.z + 0.5);
 

            Debug.Log ("place at " + px + " " + pz + " " + py);
            localWorld.placeBlock (px, pz, py);

            y = 0.2f;
            transform.Translate(0, 1, 0);
		
			attached = false;*/


		/*	if(Physics.Raycast(ray, out hit, 10))
			{


				int px = (int) (placement.x + 0.5);
				int py = Math.Max ((int) (placement.y), (int) WorldManager.minLevelHeight);
				int pz = (int) (placement.z + 0.5);


				Debug.Log ("place at " + px + " " + pz + " " + py);
				localWorld.placeBlock (px, pz, py, currentBlockType);

				y = 0.2f;
				transform.Translate(0, 1, 0);

				attached = false;



			}

		} */

		/// Place block action.
		if (Input.GetKeyDown(KeyCode.LeftControl) && (isGrounded () || attached))
		{
			int px = (int) (playerpos.x + 0.5);
			int py = Math.Max ((int) (playerpos.y), (int) WorldManager.minLevelHeight);
			int pz = (int) (playerpos.z + 0.5);

			Debug.Log ("place at " + px + " " + pz + " " + py);
			localWorld.placeBlock (px, pz, py, currentBlockType);

			y = 0.2f;
			transform.Translate(0, 1, 0);

			attached = false;
		}


		/// Change the currentBlockType

		if (Input.GetKey (KeyCode.Alpha1))
		{
			currentBlockType = 1;
		}
		if (Input.GetKey (KeyCode.Alpha2))
		{
			currentBlockType = 2;
		}
		if (Input.GetKey (KeyCode.Alpha3))
		{
			currentBlockType = 3;
		}

		//Pick up 

		if(Input.GetKey(KeyCode.E)&& (isGrounded () || attached))
		{
			if(Physics.Raycast(ray, out hit, 50))
			{
				pickup = true;
			}
		}
		else
		{
			pickup = false;
		}

		//open inventory window
		if(Input.GetKey(KeyCode.I))
		{


		}




		/// Turn and move forward.
		//var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
		//var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

		var x = (Input.GetAxis("Mouse X") * 5.0f);
		/// Update player state based on actions set.
		/// 

		var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;
		var strafe = Input.GetAxis("Horizontal") * Time.deltaTime * 3.0f;
		var testY = Input.GetAxis("Mouse Y") * Time.deltaTime * 100.0f;

		/// Update player state based on actions set.
	transform.Rotate(0, x, 0);
	transform.Translate(strafe, 0, z);

	Rigidbody rb = GetComponent<Rigidbody> ();
	rb.AddForce (80.0f * transform.up * y);

		/// Check that movement is allowed.
		playerpos = transform.position;
		if (playerpos.y < WorldManager.minLevelHeight - 0.5f/* && Input.GetKeyDown(KeyCode.RightControl)*/)
		{
			//           Debug.Log ("Fallen and can't get up: " + playerpos);

			// Find a block that the player can attach to.
			Vector3 freepos;
			bool found = localWorld.findNearestBlock (playerpos, out freepos);
			if (found)
			{
				Debug.Log  ("Finding neighbour for: " + playerpos + " at " + freepos);
				attachPoint  = freepos;
				attached = true;
			}
			else
			{
				// emergency - no blocks available.
			}
		}

		if (attached)
		{
			//transform.position = attachPoint;
			transform.position = Vector3.MoveTowards(transform.position, attachPoint, 1.0f); //attachPoint;
			rb.velocity = new Vector3 (0.0f, 0.0f, 0.0f);
		}
	}

	/// Some actions are only allowed when the player is on the ground. This method
	/// checks for that case.
	bool isGrounded()
	{
		var distToGround = 0.4f;
		//         Debug.Log ("distance " + Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f) + " " + transform.position + " - " + (-Vector3.up) + " - " + (distToGround + 0.1f));
		return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
	}

	/// When a client side instance of the player is started then:
	///   - update player view.
	///   - attach the main camera to give a external, tracked view of the player.
	public override void OnStartLocalPlayer()
	{
		GameObject playerShape = transform.Find("PlayerShape").gameObject;
		playerShape.GetComponent<MeshRenderer>().material.color = Color.blue; 
		
		if(isLocalPlayer)
		{ //if I am the owner of this prefab
			GameObject camera = GameObject.Find("Main Camera");
			Debug.Log ("Setup camera" + camera);
			camera.transform.parent = transform;
		}
	}

	//when ray collide with blocks
	void OnCollisionEnter(Collision other)
	{
		if(pickup == true)
		{
			if(other.gameObject.CompareTag("Block"))
			{
				other.gameObject.SetActive(false);
			}
		}
	}



}
