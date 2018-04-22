using DarkRift.Client.Unity;
using System.Collections;
using System.Collections.Generic;
using DarkRift.Client;
using DarkRift;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    public UnityClient client;

    [SerializeField]
    [Tooltip("The tree frefab.")]
    public GameObject tree;

    Text itemGatheredText;
    GameObject loadingScreen;

    [SerializeField]
    [Tooltip("The herb prefab.")]
    public GameObject herb;

    Dictionary<int, GatherSpot> gatherSpots = new Dictionary<int, GatherSpot>();

    void Awake()
    {
        client.MessageReceived += HandleMessage;
    }

    void HandleMessage(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        {
            if (message.Tag == Tags.GatherSpotsTag)
            {
                SpawnGatherSpots(sender, e);
            }

            else if (message.Tag == Tags.GatherItemTag)
            {
                UpdateGatherSpots(sender, e);
            }

            else if (message.Tag == Tags.InventoryUpdateTag)
            {
                InventoryUpdate(sender, e);
            }
        }
    }

    // Disable taken gather spots
    void UpdateGatherSpots(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            // Read ID of gathered gather spot and disable the gather spot corresponding to the ID
            using (DarkRiftReader reader = message.GetReader())
            {
                int spotID = reader.ReadUInt16();

                gatherSpots[spotID].DisableGatherSpot();

            }
        }
    }

    void SpawnGatherSpots(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                int count = 0;

                while (reader.Position < reader.Length)
                {
                    count++;
                    ushort spotID = reader.ReadUInt16();
                    ushort itemID = reader.ReadUInt16();
                    Vector3 pos = new Vector3(reader.ReadInt16(), reader.ReadInt16(), 1);

                    if (itemID == 0)
                    {
                        GameObject temp = Instantiate(herb, pos, Quaternion.identity);
                        GatherSpot gatherScript = temp.GetComponent<GatherSpot>();
                        gatherScript.SetID(spotID);
                        gatherScript.SetItemID(0);
                        gatherScript.SetItemManager(this);
                        gatherSpots.Add(spotID, gatherScript);
                    }

                    else if (itemID == 1)
                    {
                        GameObject temp = Instantiate(tree, pos, Quaternion.identity);
                        GatherSpot gatherScript = temp.GetComponent<GatherSpot>();
                        gatherScript.SetID(spotID);
                        gatherScript.SetItemID(1);
                        gatherScript.SetItemManager(this);
                        gatherSpots.Add(spotID, gatherScript);
                    }
                }
            }

            loadingScreen = GameObject.Find("LoadingScreen");
            itemGatheredText = GameObject.Find("ItemPickUpText").GetComponent<Text>();
            loadingScreen.SetActive(false);

        }
    }

    public void ItemGathered(int ID)
    {
        // Sending gathering attempt to server
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write((ushort)ID);

            using (Message message = Message.Create(Tags.GatherItemTag, writer))
                client.SendMessage(message, SendMode.Reliable);
        }

        string itemName = " ";
        int itemID = gatherSpots[ID].GetItemID();

        if (itemID == 0)
        {
            itemName = "Herb";
        }

        else if (itemID == 1)
        {
            itemName = "Wood";
        }

        if (itemGatheredText != null)
        {
            itemGatheredText.text = "Got " + itemName;
        }
    }

    void InventoryUpdate(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                //string itemName = " ";
                //ushort itemID = reader.ReadUInt16();

                //if (itemID == 0)
                //{
                //    itemName = "Herb";
                //}

                //else if (itemID == 1)
                //{
                //    itemName = "Wood";
                //}

                if (itemGatheredText != null)
                {
                    itemGatheredText.text = " ";
                }
            }
        }
    }
}
