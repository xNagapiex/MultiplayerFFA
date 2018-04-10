using DarkRift.Client.Unity;
using System.Collections;
using System.Collections.Generic;
using DarkRift.Client;
using System;
using DarkRift;
using UnityEngine;

public class networkPlayerManager : MonoBehaviour
{

    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    UnityClient client;

    Dictionary<ushort, playerObject> networkPlayers = new Dictionary<ushort, playerObject>();

    void Awake()
    {
        client.MessageReceived += NetworkMovement;
    }

    public void Add(ushort id, playerObject player)
    {
        networkPlayers.Add(id, player);
    }

    void NetworkMovement(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        using (DarkRiftReader reader = message.GetReader())
        {
            if (message.Tag == Tags.MovePlayerTag)
            {
                if (reader.Length % 17 != 0)
                {
                    Debug.LogWarning("Received malformed movement packet.");
                }

                print("Got movement update");

                while (reader.Position < reader.Length)
                {
                    ushort id = reader.ReadUInt16();
                    Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle());
                    Quaternion rotation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                    foreach (KeyValuePair<ushort, playerObject> entry in networkPlayers)
                    {
                        if (entry.Key == id)
                        {
                            entry.Value.transform.position = Vector3.MoveTowards(transform.position, position, 6 * Time.deltaTime);
                            entry.Value.transform.rotation = rotation;
                        }
                    }
                }

            }
        }
    }
}