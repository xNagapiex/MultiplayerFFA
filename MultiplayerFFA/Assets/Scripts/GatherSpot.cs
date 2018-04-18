using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherSpot : MonoBehaviour {

    int ID;

    [SerializeField]
    int ItemID;

    public Sprite normalSprite;
    public Sprite harvestedSprite;
    ItemManager itemManager;

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {

            if (Input.GetMouseButton(0) && GetComponent<SpriteRenderer>().sprite != harvestedSprite)
            {
                GetComponent<SpriteRenderer>().sprite = harvestedSprite;

                itemManager.ItemGathered(ID);
            }

            // Making the sprite appear behind or in front of player depending on where the player is compared to the item on the Y-axel
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

    public void SetID(int newID)
    {
        ID = newID;
    }

    public int GetItemID()
    {
        return ItemID;
    }

    public void SetItemManager(ItemManager it)
    {
        itemManager = it;
    }

    public void DisableGatherSpot()
    {
        GetComponent<SpriteRenderer>().sprite = harvestedSprite;
    }
}
