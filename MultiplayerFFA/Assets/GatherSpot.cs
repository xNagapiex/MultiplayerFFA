﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherSpot : MonoBehaviour {

    public int ID;
    bool isAvailable = true;
    public Sprite normalSprite;
    public Sprite harvestedSprite;

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
            print("In interaction range of " + gameObject.name);

            if (Input.GetKeyDown(KeyCode.E) && isAvailable)
            {
                print("Got " + gameObject.name + "!");
                GetComponent<SpriteRenderer>().sprite = harvestedSprite;
                isAvailable = false;
            }
        }
    }
}
