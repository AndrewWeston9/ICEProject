﻿

  using UnityEngine; 
  using System.Collections; 
  using System.Collections.Generic; 
  using UnityEngine.UI;




public class EmoteWheel : MonoBehaviour
{
    public List<EmoteButton> buttons = new List<EmoteButton>();
    private Vector2 Mouseposition;
    private Vector2 fromVector2M = new Vector2(0.5f, 1.0f);
    private Vector2 centercirlce = new Vector2(0.5f, 0.5f);
    private Vector2 toVector2M;
    public Sprite sprite1;
    public Sprite sprite2;
    public Sprite sprite3;
    public Sprite sprite4;
    public float EmoteLifetime = 2;
    private bool menuon;
    public int menuItems;
    public int CurMenuItem;
    private int OldMenuItem;
    public CanvasGroup WheelButtons;
    void Start()
    {
        
        menuItems = buttons.Count;
        foreach (EmoteButton button in buttons)
        {
            button.sceneimage.color = button.NormalColor;
        }
        CurMenuItem = 0;
        OldMenuItem = 0;
    }
    void Update()
    {
       
         GetCurrentMenuItem();
        if (Input.GetButtonDown("Fire3"))
        {
            menuon = true;

        }
        
        
         if (menuon == false)
        {
            WheelButtons.blocksRaycasts = true;
            WheelButtons.alpha = 0.0f;
        }
        if (menuon == true)
        {
            WheelButtons.alpha = 1.0f;
            if (Input.GetButtonDown("Fire1" ))
            {
                ButtonAction();
            }
        }



    }
    public void GetCurrentMenuItem()
    {
        Mouseposition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        toVector2M = new Vector2(Mouseposition.x / Screen.width, Mouseposition.y / Screen.height);

        float angle = (Mathf.Atan2 (fromVector2M.y - centercirlce.y, fromVector2M.x - centercirlce.x) - Mathf.Atan2(toVector2M.y - centercirlce.y, toVector2M.x - centercirlce.x)) * Mathf.Rad2Deg;
        if (angle < 0)
        {
            angle += 360;
        }
       
        
            
        CurMenuItem = (int)(angle / (360f / menuItems));
              
        if (CurMenuItem != OldMenuItem)
        {
            buttons[OldMenuItem].sceneimage.color = buttons[OldMenuItem].NormalColor;
            OldMenuItem = CurMenuItem;
            buttons[CurMenuItem].sceneimage.color = buttons[CurMenuItem].HighlightedColor;

            //print(angle);
            //print(menuItems);
        }
    }


    public void ButtonAction()
    {
        buttons[CurMenuItem].sceneimage.color = buttons[CurMenuItem].PressedColor;
        GameObject Player = GameObject.Find("Player(Clone)");
        Vector3 offset = new Vector3(0.0f, 2.0f, 0.0f);
        IconObject DisplayEmote;
        //RED
        if (CurMenuItem == 0)
        {

            GameObject emoteobject = new GameObject("Emote");
            SpriteRenderer renderer = emoteobject.AddComponent<SpriteRenderer>();
            IconObject qwe = emoteobject.AddComponent<IconObject>();
            renderer.sprite = sprite1;
            Destroy(emoteobject, EmoteLifetime);

            /*
            print("You have pressed the first button");
            GameObject emoteobject = new GameObject("Emote");
            SpriteRenderer renderer = emoteobject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite1;
            Destroy(emoteobject, EmoteLifetime);
           
            emoteobject.transform.position = (Player.transform.position) + offset;
            emoteobject.transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward);
            */

        }
        else if (CurMenuItem == 1)
        {
            GameObject emoteobject = new GameObject("Emote");
            SpriteRenderer renderer = emoteobject.AddComponent<SpriteRenderer>();
            IconObject qwe = emoteobject.AddComponent<IconObject>();
            renderer.sprite = sprite2;
            Destroy(emoteobject, EmoteLifetime);


        }
        else if (CurMenuItem == 2)
        {
       


            GameObject emoteobject = new GameObject("Emote");
            SpriteRenderer renderer = emoteobject.AddComponent<SpriteRenderer>();
            IconObject qwe = emoteobject.AddComponent<IconObject>();
            renderer.sprite = sprite3;
            Destroy(emoteobject, EmoteLifetime);



        }
        else if (CurMenuItem == 3)
        {
            print("You have pressed the fourth button");
            // GameObject NewEmote = Instantiate(EmoteObject);
            // var Iconclass = new IconObject();

           
            GameObject emoteobject = new GameObject("Emote");
            SpriteRenderer renderer = emoteobject.AddComponent<SpriteRenderer>();
            IconObject qwe = emoteobject.AddComponent<IconObject>();
            renderer.sprite = sprite4;
            Destroy(emoteobject, EmoteLifetime);

            //SpriteRenderer renderer =NewEmote.AddComponent<SpriteRenderer>();
            //renderer.sprite = sprite3;

            /*
              GameObject emoteobject = new GameObject("Emote");
              SpriteRenderer renderer = emoteobject.AddComponent<SpriteRenderer>();
              renderer.sprite = sprite4;
              Destroy(emoteobject, EmoteLifetime);

              emoteobject.transform.position = (Player.transform.position) + offset;
              emoteobject.transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward);
            */




        }

        menuon = false;
    }
}


 public class IconObject : MonoBehaviour
{

    public void GetSprite(Sprite z)
    {
        //Emotetype = z;
       
    }
   public GameObject EmotePicture;
    GameObject Player;
    Vector3 offset = new Vector3(0.0f, 2.0f, 0.0f);
   // public Sprite Emotetype;
   // SpriteRenderer SR;

  
    public float lifetime = 2.0f;
    
    void Start()
    {
         Player = GameObject.Find("Player(Clone)");
        Destroy(this, 2);
        print("New object created");
        // GameObject go = new GameObject("Test");
        //SpriteRenderer renderer = EmotePicture.AddComponent<SpriteRenderer>();
        
    }

    void Update()
    {
        //SR.sprite = Emotetype;
        transform.position = (Player.transform.position) + offset;
        transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward);
        print("Object updating");
    }
}


[System.Serializable]
public class EmoteButton
{
    public string name;
    public Image sceneimage;
    //public Texture2D sceneTexure;
    public Color NormalColor = Color.white;
    public Color HighlightedColor = Color.grey;
    public Color PressedColor = Color.red;
}




