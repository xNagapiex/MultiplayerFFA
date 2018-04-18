using DarkRift.Client.Unity;
using System.Collections;
using System.Collections.Generic;
using DarkRift.Client;
using DarkRift;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    public UnityClient client;

    [SerializeField]
    [Tooltip("The tree frefab.")]
    public GameObject tree;

    [SerializeField]
    [Tooltip("The herb prefab.")]
    public GameObject herb;


    void Awake()
    {
        client.MessageReceived += SpawnGatherSpots;
    }

    // Update is called once per frame
    void Update ()
    {
    }

    public void SpawnGatherSpots(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        {
            if(message.Tag == Tags.GatherSpotsTag)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    int count = 0;

                    while (reader.Position < reader.Length)
                    {
                        count++;
                        ushort spotID = reader.ReadUInt16();
                        ushort itemID = reader.ReadUInt16();
                        Vector3 pos = new Vector3 (reader.ReadInt16(), reader.ReadInt16(), 1);

                        print("SpotID: " + spotID);
                        print("ItemID: " + itemID);
                        print("X: " + pos.x);
                        print("Y: " + pos.y);

                        if (itemID == 0)
                        {
                            GameObject temp = GameObject.Instantiate(herb, pos, Quaternion.identity);
                            temp.GetComponent<GatherSpot>().SetID(spotID);
                            temp.GetComponent<GatherSpot>().SetItemManager(this);
                        }

                        else if (itemID == 1)
                        {
                            GameObject temp = GameObject.Instantiate(tree, pos, Quaternion.identity);
                            temp.GetComponent<GatherSpot>().SetID(spotID);
                            temp.GetComponent<GatherSpot>().SetItemManager(this);
                        }
                    }
                }
            }
        }
    }

    public void ItemGathered(int ID)
    {
        // Sending gathering attempt to server
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(ID);

            using (Message message = Message.Create(Tags.GatherItemTag, writer))
                client.SendMessage(message, SendMode.Reliable);
        }
    }
}
