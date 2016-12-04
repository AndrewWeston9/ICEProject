using UnityEngine;
using UnityEngine.Networking;

/// Player object management.
public class PlayerMove : NetworkBehaviour
{
    /// A link to the local level representation,which can be
    /// queried for any player control operations.
    protected LocalWorld localWorld;
    
    /// Access method used when the local world registers itself
    /// with this player object.
    public void setLocalWorld (LocalWorld lw)
    {
      localWorld = lw;
//       Debug.Log ("Add block " + localWorld); 
    }
    
    /// Actions on each update of the player:
    ///   - handle key presses
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;

        }
        
        /// Jump action.
        var y = 0.0f;
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded ())
        {
            y = 1.0f;
        }
        
        /// Place block action.
        if (Input.GetKeyDown(KeyCode.LeftControl) && isGrounded ())
        {
            Vector3 playerpos = transform.position;
            int px = (int) (playerpos.x + 0.5);
            int py = (int) playerpos.y;
            int pz = (int) (playerpos.z + 0.5);
            
            localWorld.placeBlock (px, pz);

            y = 0.2f;
            transform.Translate(0, 1, 0);
            
        }
        
        /// Turn and move forward.
        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        /// Update player state based on actions set.
        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);
        
        Rigidbody rb = GetComponent<Rigidbody> ();
        rb.AddForce (80.0f * transform.up * y);
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
}
