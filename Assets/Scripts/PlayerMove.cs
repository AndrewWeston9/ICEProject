using UnityEngine;
using UnityEngine.Networking;

public class PlayerMove : NetworkBehaviour
{
    public GameObject brick;    
    public GameObject levelObject;
       
    protected LocalWorld localWorld;
    
    public void setLocalWorld (LocalWorld lw)
    {
      localWorld = lw;
      Debug.Log ("Add block " + localWorld); 
    }
    
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;

        }
        
        var y = 0.0f;
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded ())
        {
            y = 1.0f;
        }
        
        if (Input.GetKeyDown(KeyCode.LeftControl) && isGrounded ())
        {
            Vector3 playerpos = transform.position;
            int px = (int) (playerpos.x + 0.5);
            int py = (int) playerpos.y;
            int pz = (int) (playerpos.z + 0.5);
            
            localWorld.placeBlock (px, pz);

//             Vector3 pos = new Vector3 (px, py + 0.5f, pz);
//             GameObject thisBrick = Object.Instantiate (brick, pos, Quaternion.identity);
                    
//             thisBrick.transform.parent = levelObject.transform;
            y = 0.2f;
            transform.Translate(0, 1, 0);
            
        }
        
        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);
        
        Rigidbody rb = GetComponent<Rigidbody> ();
        rb.AddForce (80.0f * transform.up * y);
    }
    
    bool isGrounded()
    {
        var distToGround = 0.4f;
//         Debug.Log ("distance " + Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f) + " " + transform.position + " - " + (-Vector3.up) + " - " + (distToGround + 0.1f));
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
    }
        
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
