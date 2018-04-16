using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherSpot : MonoBehaviour {

    [SerializeField]
    int ID;
    [SerializeField]
    int itemID;

    bool isAvailable = true;
    public Sprite normalSprite;
    public Sprite harvestedSprite;
    GameObject network;

	// Use this for initialization
	void Start ()
    {
        network = GameObject.Find("Network");
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            print("In interaction range of " + gameObject.name);

            if (Input.GetMouseButton(0) && isAvailable)
            {
                print("Got " + gameObject.name + "!");
                GetComponent<SpriteRenderer>().sprite = harvestedSprite;

                // Will be important later
                //network.GetComponent<GatheringCrafting>().ItemGathered(ID);

                isAvailable = false;
            }

            if (collision.gameObject.transform.position.y > transform.position.y)
            {
                GetComponent<SpriteRenderer>().sortingLayerName = "EnvironmentPlayerBehind";
            }

            else
            {
                GetComponent<SpriteRenderer>().sortingLayerName = "EnvironmentPlayerInFront";
            }
        }
    }

    public int GetID()
    {
        return ID;
    }

    public void disableSpot()
    {
        GetComponent<SpriteRenderer>().sprite = harvestedSprite;
        isAvailable = false;
    }
}
