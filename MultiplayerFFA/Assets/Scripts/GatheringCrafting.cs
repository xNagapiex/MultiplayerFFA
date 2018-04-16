using DarkRift.Client.Unity;
using System.Collections;
using System.Collections.Generic;
using DarkRift.Client;
using System;
using DarkRift;
using UnityEngine;

public class GatheringCrafting : MonoBehaviour
{

    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    UnityClient Client;

    Dictionary<int, GatherSpot> gatherSpots = new Dictionary<int, GatherSpot>();

    // Use this for initialization
    void Start ()
    {
        GameObject[] tempArray = GameObject.FindGameObjectsWithTag("GatherSpot");

        foreach(GameObject gatherspot in tempArray)
        {
            gatherSpots.Add(gatherspot.GetComponent<GatherSpot>().GetID(), gatherspot.GetComponent<GatherSpot>());

            print("Gather spot " + gatherspot.GetComponent<GatherSpot>().GetID() + " added to the dictionary");
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    //public void ItemGathered(int ID)
    //{
    //    // Sending gathering attempt to server
    //    using (DarkRiftWriter writer = DarkRiftWriter.Create())
    //    {
    //        writer.Write(ID);

    //        using (Message message = Message.Create(Tags.GatherItemTag, writer))
    //            Client.SendMessage(message, SendMode.Reliable);
    //    }
    //}
}
